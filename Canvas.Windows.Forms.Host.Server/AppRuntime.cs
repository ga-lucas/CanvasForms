using System.Reflection;
using System.Runtime.Loader;
using Canvas.Windows.Forms.RemoteProtocol;
using System.Windows.Forms;
using Canvas.Windows.Forms;
using Canvas.Windows.Forms.Drawing;

namespace Canvas.Windows.Forms.Host.Server;

/// <summary>
/// Manages the lifecycle of running apps in the "OS".
/// Only one app runs at a time.
/// </summary>
public sealed class AppRuntime : IDisposable
{
    private readonly ILogger<AppRuntime> _logger;
    private readonly object _lock = new();

    private AssemblyLoadContext? _appLoadContext;
    private Form? _mainForm;
    private string? _currentAppId;
    private bool _isNativeApp;

    public event Action? DesktopChanged;

    public AppRuntime(ILogger<AppRuntime> logger)
    {
        _logger = logger;
    }

    public bool IsRunning => _mainForm != null;
    public string? CurrentAppId => _currentAppId;
    public bool IsNativeApp => _isNativeApp;

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

            _mainForm = formFactory();
            _mainForm.OnContainerChanged = () => DesktopChanged?.Invoke();
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

                var assembly = _appLoadContext.LoadFromAssemblyPath(assemblyPath);

                // Find entry point or Form subclass
                var entryPoint = assembly.EntryPoint;
                if (entryPoint != null)
                {
                    // Has Main method - invoke it
                    _logger.LogInformation("Invoking entry point: {Method}", entryPoint);

                    // Capture the first form shown via Application.Run(form)
                    Form? createdForm = null;
                    void OnFormCreated(object? sender, Form f)
                    {
                        if (createdForm == null)
                        {
                            createdForm = f;
                            _logger.LogInformation("Captured form from entry point: {Form}", f.GetType().Name);
                        }
                    }

                    CanvasApplication.FormManager!.FormAdded += OnFormCreated;
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
                    finally
                    {
                        CanvasApplication.FormManager!.FormAdded -= OnFormCreated;
                    }

                    // Get the form - either captured from event or fall back to OpenForms
                    _mainForm = createdForm ?? CanvasApplication.OpenForms.FirstOrDefault();
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
                    _mainForm.OnContainerChanged = () => DesktopChanged?.Invoke();
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
            if (_mainForm != null)
            {
                _logger.LogInformation("Stopping app: {AppId}", _currentAppId);

                try
                {
                    _mainForm.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing form");
                }

                _mainForm = null;
            }

            if (_appLoadContext != null)
            {
                _appLoadContext.Unload();
                _appLoadContext = null;
            }

            _currentAppId = null;
            _isNativeApp = false;

            // Clear any forms from CanvasApplication
            CanvasApplication.Exit();

            DesktopChanged?.Invoke();
        }
    }

    /// <summary>
    /// Gets the current form (if any).
    /// </summary>
    public Form? GetCurrentForm()
    {
        lock (_lock)
        {
            return _mainForm;
        }
    }

    /// <summary>
    /// Gets a snapshot of the current desktop state.
    /// </summary>
    public DesktopSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            if (_mainForm == null || !_mainForm.Visible)
            {
                return new DesktopSnapshot(Array.Empty<FormSnapshot>(), null);
            }

            var snapshot = new FormSnapshot(
                Id: _currentAppId ?? "unknown",
                Text: _mainForm.Text,
                Left: _mainForm.Left,
                Top: _mainForm.Top,
                Width: _mainForm.Width,
                Height: _mainForm.Height,
                ZIndex: _mainForm.ZIndex,
                Visible: _mainForm.Visible,
                IsMinimized: _mainForm.WindowState == FormWindowState.Minimized,
                IsMaximized: _mainForm.WindowState == FormWindowState.Maximized,
                BackColorHex: $"#{_mainForm.BackColor.R:X2}{_mainForm.BackColor.G:X2}{_mainForm.BackColor.B:X2}");

            return new DesktopSnapshot(new[] { snapshot }, _currentAppId);
        }
    }

    /// <summary>
    /// Moves a form to a new position.
    /// </summary>
    public void MoveForm(string formId, int left, int top)
    {
        lock (_lock)
        {
            if (_mainForm == null || _currentAppId != formId) return;

            _mainForm.Left = left;
            _mainForm.Top = top;
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
            if (_mainForm == null || _currentAppId != formId) return;

            _mainForm.Left = left;
            _mainForm.Top = top;
            _mainForm.Width = Math.Max(100, width);
            _mainForm.Height = Math.Max(50, height);
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
            if (_mainForm == null || _currentAppId != formId) return;
            _mainForm.WindowState = FormWindowState.Minimized;
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
            if (_mainForm == null || _currentAppId != formId) return;

            if (_mainForm.WindowState == FormWindowState.Maximized)
            {
                _mainForm.WindowState = FormWindowState.Normal;
            }
            else
            {
                _mainForm.WindowState = FormWindowState.Maximized;
                _mainForm.Left = 0;
                _mainForm.Top = 0;
                _mainForm.Width = desktopWidth;
                _mainForm.Height = desktopHeight;
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
            if (_mainForm == null || _currentAppId != formId) return;

            if (_mainForm.WindowState == FormWindowState.Minimized)
            {
                _mainForm.WindowState = FormWindowState.Normal;
            }
            DesktopChanged?.Invoke();
        }
    }

    /// <summary>
    /// Closes a specific form.
    /// </summary>
    public void CloseForm(string formId)
    {
        lock (_lock)
        {
            if (_mainForm == null || _currentAppId != formId) return;
            Stop();
        }
    }

    /// <summary>
    /// Renders the current form to draw commands.
    /// </summary>
    public RenderFrame Render()
    {
        lock (_lock)
        {
            if (_mainForm == null)
            {
                return new RenderFrame(
                    FormId: "",
                    BorderWidth: 0,
                    TitleBarHeightWithBorder: 0,
                    ClientWidth: 0,
                    ClientHeight: 0,
                    Commands: Array.Empty<object[]>());
            }

            var graphics = new Graphics(_mainForm.Width, _mainForm.Height);
            var paintArgs = new PaintEventArgs(graphics, new Rectangle(0, 0, _mainForm.Width, _mainForm.Height));

            // Trigger paint
            _mainForm.Invalidate();

            // Get the form to paint itself via reflection (OnPaint is protected internal)
            var onPaintMethod = typeof(Control).GetMethod("OnPaint",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            onPaintMethod?.Invoke(_mainForm, new object[] { paintArgs });

            // Convert DrawingCommand[] to object[][] for serialization
            var commands = graphics.GetCommands().Select(cmd => cmd.ToCommand()).ToArray();

            return new RenderFrame(
                FormId: _currentAppId ?? "unknown",
                BorderWidth: 4,
                TitleBarHeightWithBorder: 36,
                ClientWidth: _mainForm.Width,
                ClientHeight: _mainForm.Height,
                Commands: commands);
        }
    }

    /// <summary>
    /// Sends a mouse event to the current form.
    /// </summary>
    public void SendMouseEvent(string eventType, int x, int y, int button)
    {
        lock (_lock)
        {
            if (_mainForm == null) return;

            var mouseButton = button switch
            {
                0 => MouseButtons.Left,
                1 => MouseButtons.Middle,
                2 => MouseButtons.Right,
                _ => MouseButtons.None
            };

            var args = new MouseEventArgs(mouseButton, 1, x, y);

            var methodName = eventType switch
            {
                "mousedown" => "OnMouseDown",
                "mouseup" => "OnMouseUp",
                "mousemove" => "OnMouseMove",
                "click" => "OnMouseClick",
                "dblclick" => "OnMouseDoubleClick",
                _ => null
            };

            if (methodName != null)
            {
                var method = typeof(Control).GetMethod(methodName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                method?.Invoke(_mainForm, new object[] { args });
            }
        }
    }

    /// <summary>
    /// Sends a keyboard event to the current form.
    /// </summary>
    public void SendKeyEvent(string eventType, int keyCode, bool alt, bool ctrl, bool shift, char keyChar)
    {
        lock (_lock)
        {
            if (_mainForm == null) return;

            if (eventType == "keypress" && keyChar != '\0')
            {
                var pressArgs = new KeyPressEventArgs(keyChar);
                var method = typeof(Control).GetMethod("OnKeyPress",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                method?.Invoke(_mainForm, new object[] { pressArgs });
            }
            else
            {
                var keys = (Keys)keyCode;
                var args = new KeyEventArgs(keys, alt, ctrl, shift);

                var methodName = eventType switch
                {
                    "keydown" => "OnKeyDown",
                    "keyup" => "OnKeyUp",
                    _ => null
                };

                if (methodName != null)
                {
                    var method = typeof(Control).GetMethod(methodName,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    method?.Invoke(_mainForm, new object[] { args });
                }
            }
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
