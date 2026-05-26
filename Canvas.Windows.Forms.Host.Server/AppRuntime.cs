using System.Reflection;
using System.Runtime.Loader;
using Canvas.Windows.Forms.RemoteProtocol;
using System.Windows.Forms;
using Canvas.Windows.Forms;
using Canvas.Windows.Forms.Drawing;
using Canvas.Windows.Forms.Host.Server.Data;

namespace Canvas.Windows.Forms.Host.Server;

/// <summary>
/// Manages the lifecycle of running apps in the "OS".
/// Supports multiple concurrently open forms per app session.
/// </summary>
public sealed class AppRuntime : IDisposable
{
    private readonly ILogger<AppRuntime> _logger;
    private readonly ServerCanvasDataService? _dataService;
    private readonly CanvasProviderResolver? _providerResolver;
    private readonly object _lock = new();

    private AssemblyLoadContext? _appLoadContext;
    // Primary entry-point form (first form shown by the app).
    private Form? _mainForm;
    // Stable per-form IDs so the browser can reference individual windows.
    private readonly Dictionary<Form, string> _formIds = new(ReferenceEqualityComparer.Instance);
    private int _nextFormIndex = 0;
    private string? _currentAppId;
    private bool _isNativeApp;

    public event Action? DesktopChanged;

    /// <summary>
    /// Fired (debounced) when any tracked form calls <see cref="Control.Invalidate()"/>.
    /// Carries the stable form ID of the form that triggered the render.
    /// Subscribers (Program.cs) broadcast a fresh <see cref="RenderFrame"/> to SignalR clients.
    /// </summary>
    public event Action<string>? RenderRequested;

    public AppRuntime(
        ILogger<AppRuntime> logger,
        ServerCanvasDataService? dataService = null,
        CanvasProviderResolver? providerResolver = null)
    {
        _logger = logger;
        _dataService = dataService;
        _providerResolver = providerResolver;
    }

    public bool IsRunning => _mainForm != null || _formIds.Count > 0;
    public string? CurrentAppId => _currentAppId;
    public bool IsNativeApp => _isNativeApp;

    /// <summary>Returns the stable browser-facing ID for a given form, or null if not tracked.</summary>
    public string? GetFormId(Form form)
    {
        _formIds.TryGetValue(form, out var id);
        return id;
    }

    /// <summary>Returns the form for a given stable ID, or null.</summary>
    private Form? FindFormById(string formId)
        => _formIds.FirstOrDefault(kv => kv.Value == formId).Key;

    /// <summary>Registers a form and assigns it a stable ID; idempotent.</summary>
    private string RegisterForm(Form form)
    {
        if (_formIds.TryGetValue(form, out var existing))
            return existing;

        var id = $"{_currentAppId}-w{++_nextFormIndex}";
        _formIds[form] = id;

        form.FormClosed += (_, _) =>
        {
            lock (_lock)
            {
                _formIds.Remove(form);
                if (ReferenceEquals(form, _mainForm))
                    _mainForm = _formIds.Keys.FirstOrDefault();
                DesktopChanged?.Invoke();
            }
        };

        form.OnContainerChanged = () => DesktopChanged?.Invoke();

        // Wire Invalidate → RenderRequested (debounced: coalesce rapid calls).
        var renderPending = false;
        form.PropagateRequestRender(() =>
        {
            if (renderPending) return Task.CompletedTask;
            renderPending = true;
            _ = Task.Run(async () =>
            {
                await Task.Delay(16); // ~1 frame @ 60 fps
                renderPending = false;
                RenderRequested?.Invoke(id);
            });
            return Task.CompletedTask;
        });

        return id;
    }

    /// <summary>Hook invoked by FormManager whenever any new form is added during an app session.</summary>
    private void OnFormAdded(object? sender, Form form)
    {
        lock (_lock)
        {
            RegisterForm(form);
            DesktopChanged?.Invoke();
        }
    }

    /// <summary>
    /// Runs a native app (compiled directly with Canvas.Windows.Forms).
    /// </summary>
    public void RunNative(string appId, Func<Form> formFactory)
    {
        lock (_lock)
        {
            Stop();

            _logger.LogInformation("Starting native app: {AppId}", appId);
            _currentAppId = appId;
            _isNativeApp = true;

            if (CanvasApplication.FormManager != null)
                CanvasApplication.FormManager.FormAdded += OnFormAdded;

            _mainForm = formFactory();
            RegisterForm(_mainForm);
            _mainForm.Show();

            DesktopChanged?.Invoke();
        }
    }

    /// <summary>
    /// Runs a translated (uploaded) app from an assembly path.
    /// </summary>
    public void RunTranslated(string appId, string assemblyPath)
    {
        lock (_lock)
        {
            Stop();

            _logger.LogInformation("Starting translated app: {AppId} from {Path}", appId, assemblyPath);
            _currentAppId = appId;
            _isNativeApp = false;

            // Create isolated load context for the app
            _appLoadContext = new AssemblyLoadContext($"App_{appId}", isCollectible: true);

            try
            {
                // Add resolver for dependencies in the same folder
                var assemblyDir = Path.GetDirectoryName(assemblyPath)!;
                _appLoadContext.Resolving += (context, name) =>
                {
                    var dllPath = Path.Combine(assemblyDir, $"{name.Name}.dll");
                    if (File.Exists(dllPath))
                    {
                        return context.LoadFromAssemblyPath(dllPath);
                    }
                    return null;
                };

                // Auto-load canvas-connections.json from the app directory
                TryLoadConnectionsConfig(assemblyDir);

                // Subscribe before invoking entry point so we capture all forms.
                if (CanvasApplication.FormManager != null)
                    CanvasApplication.FormManager.FormAdded += OnFormAdded;

                var assembly = _appLoadContext.LoadFromAssemblyPath(assemblyPath);

                // Find entry point or Form subclass
                var entryPoint = assembly.EntryPoint;
                if (entryPoint != null)
                {
                    // Has Main method - invoke it
                    _logger.LogInformation("Invoking entry point: {Method}", entryPoint);

                    try
                    {
                        var parameters = entryPoint.GetParameters();
                        var args = parameters.Length == 0 ? null : new object?[] { Array.Empty<string>() };
                        entryPoint.Invoke(null, args);
                    }
                    catch (TargetInvocationException ex) when (ex.InnerException != null)
                    {
                        _logger.LogError(ex.InnerException, "Error in app entry point");
                        throw ex.InnerException;
                    }

                    // Use the first registered form as the main form.
                    _mainForm = _formIds.Keys.FirstOrDefault()
                                ?? CanvasApplication.OpenForms.FirstOrDefault();
                }
                else
                {
                    // No entry point - find first Form type and instantiate it
                    var formType = assembly.GetTypes()
                        .FirstOrDefault(t => typeof(Form).IsAssignableFrom(t) && !t.IsAbstract);

                    if (formType == null)
                    {
                        throw new InvalidOperationException("No Form type found in assembly");
                    }

                    _logger.LogInformation("Instantiating form type: {Type}", formType.FullName);
                    _mainForm = (Form)Activator.CreateInstance(formType)!;
                    _mainForm.Show();
                }

                if (_mainForm != null)
                {
                    RegisterForm(_mainForm);
                    DesktopChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load translated app");
                _appLoadContext.Unload();
                _appLoadContext = null;
                _currentAppId = null;
                throw;
            }
        }
    }

    /// <summary>
    /// Stops the currently running app.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (_mainForm != null || _formIds.Count > 0)
            {
                _logger.LogInformation("Stopping app: {AppId}", _currentAppId);

                // Unsubscribe global form-added listener.
                if (CanvasApplication.FormManager != null)
                    CanvasApplication.FormManager.FormAdded -= OnFormAdded;

                foreach (var form in _formIds.Keys.ToList())
                {
                    try { form.Close(); } catch (Exception ex) { _logger.LogWarning(ex, "Error closing form"); }
                }
                _formIds.Clear();
                _mainForm = null;
            }

            if (_appLoadContext != null)
            {
                _appLoadContext.Unload();
                _appLoadContext = null;
            }

            _currentAppId = null;
            _isNativeApp = false;
            _nextFormIndex = 0;

            // Clear app-registered data providers so next app starts clean
            _dataService?.ClearAppProviders();

            // Clear any forms from CanvasApplication
            CanvasApplication.Exit();

            DesktopChanged?.Invoke();
        }
    }

    // ── Connection config auto-loader ─────────────────────────────────────────

    private void TryLoadConnectionsConfig(string appDirectory)
    {
        if (_dataService == null || _providerResolver == null) return;

        var config = CanvasConnectionsConfig.TryLoad(appDirectory, _logger);
        if (config == null)
        {
            _logger.LogDebug("canvas-connections: no config file found in {Dir}", appDirectory);
            return;
        }

        config.Apply(_dataService, _providerResolver, _logger);
    }

    /// <summary>
    /// Gets the primary (entry-point) form, if any.
    /// </summary>
    public Form? GetCurrentForm()
    {
        lock (_lock)
        {
            return _mainForm;
        }
    }

    /// <summary>
    /// Gets a snapshot of all open forms for the desktop renderer.
    /// </summary>
    public DesktopSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            var snapshots = new List<FormSnapshot>();

            foreach (var (form, formId) in _formIds)
            {
                if (!form.Visible) continue;

                snapshots.Add(new FormSnapshot(
                    Id: formId,
                    Text: form.Text,
                    Left: form.Left,
                    Top: form.Top,
                    Width: form.Width,
                    Height: form.Height,
                    ZIndex: form.ZIndex,
                    Visible: form.Visible,
                    IsMinimized: form.WindowState == FormWindowState.Minimized,
                    IsMaximized: form.WindowState == FormWindowState.Maximized,
                    BackColorHex: $"#{form.BackColor.R:X2}{form.BackColor.G:X2}{form.BackColor.B:X2}"));
            }

            // Active form ID: prefer the FormManager's active form, fall back to main.
            var activeForm = CanvasApplication.FormManager?.ActiveForm ?? _mainForm;
            var activeId   = activeForm is not null && _formIds.TryGetValue(activeForm, out var aid) ? aid : snapshots.LastOrDefault()?.Id;

            return new DesktopSnapshot(snapshots.ToArray(), activeId);
        }
    }

    /// <summary>
    /// Moves a form to a new position.
    /// </summary>
    public void MoveForm(string formId, int left, int top)
    {
        lock (_lock)
        {
            var form = FindFormById(formId);
            if (form is null) return;
            form.Left = left;
            form.Top  = top;
            DesktopChanged?.Invoke();
        }
    }

    /// <summary>
    /// Resizes a form.
    /// </summary>
    public void ResizeForm(string formId, int left, int top, int width, int height)
    {
        lock (_lock)
        {
            var form = FindFormById(formId);
            if (form is null) return;
            form.Left   = left;
            form.Top    = top;
            form.Width  = Math.Max(100, width);
            form.Height = Math.Max(50, height);
            DesktopChanged?.Invoke();
        }
    }

    /// <summary>
    /// Minimizes a form.
    /// </summary>
    public void MinimizeForm(string formId)
    {
        lock (_lock)
        {
            var form = FindFormById(formId);
            if (form is null) return;
            form.WindowState = FormWindowState.Minimized;
            DesktopChanged?.Invoke();
        }
    }

    /// <summary>
    /// Maximizes or restores a form.
    /// </summary>
    public void MaximizeForm(string formId, int desktopWidth, int desktopHeight)
    {
        lock (_lock)
        {
            var form = FindFormById(formId);
            if (form is null) return;

            if (form.WindowState == FormWindowState.Maximized)
            {
                form.WindowState = FormWindowState.Normal;
            }
            else
            {
                form.WindowState = FormWindowState.Maximized;
                form.Left   = 0;
                form.Top    = 0;
                form.Width  = desktopWidth;
                form.Height = desktopHeight;
            }
            DesktopChanged?.Invoke();
        }
    }

    /// <summary>
    /// Activates (focuses) a form, also restoring it if minimized.
    /// </summary>
    public void ActivateForm(string formId)
    {
        lock (_lock)
        {
            var form = FindFormById(formId);
            if (form is null) return;

            if (form.WindowState == FormWindowState.Minimized)
                form.WindowState = FormWindowState.Normal;

            CanvasApplication.FormManager?.ActivateForm(form);
            DesktopChanged?.Invoke();
        }
    }

    /// <summary>
    /// Closes a specific form by its stable ID.
    /// </summary>
    public void CloseForm(string formId)
    {
        lock (_lock)
        {
            var form = FindFormById(formId);
            if (form is null) return;

            // If closing the main/last form, stop the whole session.
            if (ReferenceEquals(form, _mainForm) && _formIds.Count <= 1)
            {
                Stop();
                return;
            }

            try { form.Close(); } catch { /* ignored */ }
        }
    }

    /// <summary>
    /// Renders a specific form (or the main form if <paramref name="formId"/> is null) to draw commands.
    /// </summary>
    public RenderFrame Render(string? formId = null)
    {
        lock (_lock)
        {
            var form = (formId is not null ? FindFormById(formId) : null) ?? _mainForm;

            if (form == null)
            {
                return new RenderFrame(
                    FormId: "",
                    BorderWidth: 0,
                    TitleBarHeightWithBorder: 0,
                    ClientWidth: 0,
                    ClientHeight: 0,
                    Commands: Array.Empty<object[]>());
            }

            var resolvedId = _formIds.TryGetValue(form, out var fid) ? fid : formId ?? "unknown";

            var graphics  = new Graphics(form.Width, form.Height);
            var paintArgs = new PaintEventArgs(graphics, new Rectangle(0, 0, form.Width, form.Height));

            form.Invalidate();

            var onPaintMethod = typeof(Control).GetMethod("OnPaint",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            onPaintMethod?.Invoke(form, new object[] { paintArgs });

            var commands = graphics.GetCommands().Select(cmd => cmd.ToCommand()).ToArray();

            return new RenderFrame(
                FormId: resolvedId,
                BorderWidth: 4,
                TitleBarHeightWithBorder: 36,
                ClientWidth: form.Width,
                ClientHeight: form.Height,
                Commands: commands);
        }
    }

    /// <summary>
    /// Sends a mouse event to the specified form (or main form if null).
    /// </summary>
    public void SendMouseEvent(string eventType, int x, int y, int button, string? formId = null)
    {
        lock (_lock)
        {
            var form = (formId is not null ? FindFormById(formId) : null) ?? _mainForm;
            if (form is null) return;

            var mouseButton = button switch
            {
                0 => MouseButtons.Left,
                1 => MouseButtons.Middle,
                2 => MouseButtons.Right,
                _ => MouseButtons.None
            };

            form.DispatchMouseEvent(eventType, x, y, mouseButton);
        }
    }

    /// <summary>
    /// Sends a keyboard event to the specified form (or main form if null).
    /// </summary>
    public void SendKeyEvent(string eventType, int keyCode, bool alt, bool ctrl, bool shift, char keyChar, string? formId = null)
    {
        lock (_lock)
        {
            var form = (formId is not null ? FindFormById(formId) : null) ?? _mainForm;
            if (form is null) return;

            if (eventType == "keypress" && keyChar != '\0')
                form.DispatchKeyPress(keyChar);
            else
                form.DispatchKeyEvent(eventType, (Keys)keyCode, alt, ctrl, shift);
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
