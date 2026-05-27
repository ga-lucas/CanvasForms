using CanvasApp = Canvas.Windows.Forms.CanvasApplication;

namespace System.Windows.Forms;

public static class Application
{
    public static void EnableVisualStyles()
    {
        // No-op for canvas host.
    }

    public static void SetCompatibleTextRenderingDefault(bool defaultValue)
    {
        // No-op for canvas host.
    }

    public static bool SetHighDpiMode(HighDpiMode highDpiMode)
    {
        // WinForms returns bool; keep signature for common templates.
        return true;
    }

    public static bool IsRunning => CanvasApp.IsRunning;

    public static void Run(Form mainForm) => CanvasApp.Run(mainForm);

    public static void Run() => CanvasApp.Run();

    /// <summary>
    /// Runs the application with the specified <see cref="ApplicationContext"/>.
    /// Extracts the <see cref="ApplicationContext.MainForm"/> and runs it via <see cref="CanvasApp.Run(Form)"/>.
    /// </summary>
    public static void Run(ApplicationContext context)
    {
        if (context?.MainForm is Form f)
            CanvasApp.Run(f);
        else
            CanvasApp.Run();
    }

    public static void Exit() => CanvasApp.Exit();

    public static void Exit(int exitCode) => CanvasApp.Exit(exitCode);

    public static void DoEvents()
    {
        // No-op — the browser owns the event loop.
    }

    public static IReadOnlyList<Form> OpenForms => CanvasApp.OpenForms;

    public static event EventHandler? ApplicationExit
    {
        add => CanvasApp.ApplicationExit += value;
        remove => CanvasApp.ApplicationExit -= value;
    }

    /// <summary>
    /// Fired when an unhandled exception occurs on the UI thread.
    /// In the canvas host, unhandled exceptions from app logic surface as server-side
    /// exceptions; this event is provided for API compatibility only.
    /// </summary>
    public static event ThreadExceptionEventHandler? ThreadException;

    /// <summary>
    /// Specifies how unhandled thread exceptions are handled.
    /// Accepted for API compatibility — the canvas host always logs unhandled exceptions.
    /// </summary>
    public static void SetUnhandledExceptionMode(UnhandledExceptionMode mode) { }

    /// <summary>
    /// Specifies how unhandled thread exceptions are handled for a specific thread.
    /// Accepted for API compatibility.
    /// </summary>
    public static void SetUnhandledExceptionMode(UnhandledExceptionMode mode, bool threadScope) { }

    public static string CommonAppDataPath => CanvasApp.CommonAppDataPath;

    public static string UserAppDataPath => CanvasApp.UserAppDataPath;

    public static string CompanyName => CanvasApp.CompanyName;

    public static string ProductName => CanvasApp.ProductName;

    public static string ProductVersion => CanvasApp.ProductVersion;

    public static string ExecutablePath => System.Environment.ProcessPath ?? string.Empty;
    public static string StartupPath => System.AppContext.BaseDirectory;
    /// <summary>
    /// Raised when the application finishes processing messages and is about to enter
    /// an idle state.  In CanvasForms the message loop does not exist, so this event is
    /// provided for API compatibility only.
    /// </summary>
    public static event EventHandler? Idle;

    /// <summary>Fires the <see cref="Idle"/> event.  Called by the host between render cycles when appropriate.</summary>
    internal static void RaiseIdle() => Idle?.Invoke(null, EventArgs.Empty);

    /// <summary>
    /// Adds a message filter to monitor messages routed to the application.
    /// In CanvasForms there is no Win32 message loop, so filters are registered but
    /// never invoked.  Accepted for API compatibility.
    /// </summary>
    public static void AddMessageFilter(IMessageFilter value) { }

    /// <summary>Removes a previously added message filter.</summary>
    public static void RemoveMessageFilter(IMessageFilter value) { }

    /// <summary>
    /// Restarts the application.  In CanvasForms this is equivalent to calling
    /// <see cref="Exit()"/> because the browser tab (not the host) controls navigation.
    /// </summary>
    public static void Restart() => CanvasApp.Exit();

    /// <summary>Raises <see cref="ThreadException"/> (called by the canvas host on unhandled exceptions).</summary>
    internal static void RaiseThreadException(Exception ex)
        => ThreadException?.Invoke(null, new ThreadExceptionEventArgs(ex));
}

// ── Supporting types ──────────────────────────────────────────────────────────

/// <summary>Specifies how unhandled thread exceptions are propagated.</summary>
public enum UnhandledExceptionMode
{
    Automatic       = 0,
    ThrowException  = 1,
    CatchException  = 2,
}

/// <summary>Delegate for the <see cref="Application.ThreadException"/> event.</summary>
public delegate void ThreadExceptionEventHandler(object sender, ThreadExceptionEventArgs e);

/// <summary>Event args for <see cref="Application.ThreadException"/>.</summary>
public sealed class ThreadExceptionEventArgs : EventArgs
{
    public Exception Exception { get; }
    public ThreadExceptionEventArgs(Exception exception) => Exception = exception;
}

/// <summary>
/// Specifies the contextual information about an application thread.
/// Designer-generated <c>Program.cs</c> files typically call
/// <see cref="Application.Run(ApplicationContext)"/> passing an instance of this class.
/// </summary>
public class ApplicationContext : IDisposable
{
    private Form? _mainForm;

    public ApplicationContext() { }

    public ApplicationContext(Form mainForm)
    {
        MainForm = mainForm;
    }

    /// <summary>Gets or sets the main form for this context.</summary>
    public Form? MainForm
    {
        get => _mainForm;
        set
        {
            if (_mainForm != null)
                _mainForm.FormClosed -= OnMainFormClosed;

            _mainForm = value;

            if (_mainForm != null)
                _mainForm.FormClosed += OnMainFormClosed;
        }
    }

    /// <summary>Fired when the context's main form is closed.</summary>
    public event EventHandler? ThreadExit;

    protected virtual void OnMainFormClosed(object? sender, FormClosedEventArgs e)
    {
        ThreadExit?.Invoke(this, EventArgs.Empty);
    }

    public void ExitThread() => ThreadExit?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) { }
}

// ── Minimal stubs for common WinForms templates ───────────────────────────────

public enum HighDpiMode
{
    SystemAware = 0,
    PerMonitor = 1,
    PerMonitorV2 = 2,
    DpiUnaware = 3,
    DpiUnawareGdiScaled = 4,
}

/// <summary>
/// Defines a message filter interface.  Implementations can intercept messages before
/// they are dispatched to a control.  In CanvasForms this interface is provided for
/// API compatibility — no Win32 message loop exists in the browser.
/// </summary>
public interface IMessageFilter
{
    /// <summary>
    /// Filters a message before it is dispatched.
    /// Return <c>true</c> to suppress the message, <c>false</c> to allow dispatch.
    /// In CanvasForms this method is never called; it exists for API compatibility.
    /// </summary>
    bool PreFilterMessage(ref Message m);
}
