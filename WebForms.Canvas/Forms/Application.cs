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
