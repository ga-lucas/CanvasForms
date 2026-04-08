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

    public static void Exit() => CanvasApp.Exit();

    public static void Exit(int exitCode) => CanvasApp.Exit(exitCode);

    public static IReadOnlyList<Form> OpenForms => CanvasApp.OpenForms;

    public static event EventHandler? ApplicationExit
    {
        add => CanvasApp.ApplicationExit += value;
        remove => CanvasApp.ApplicationExit -= value;
    }

    public static string CommonAppDataPath => CanvasApp.CommonAppDataPath;

    public static string UserAppDataPath => CanvasApp.UserAppDataPath;

    public static string CompanyName => CanvasApp.CompanyName;

    public static string ProductName => CanvasApp.ProductName;

    public static string ProductVersion => CanvasApp.ProductVersion;
}

// Minimal stubs for common WinForms templates.
public enum HighDpiMode
{
    SystemAware = 0,
    PerMonitor = 1,
    PerMonitorV2 = 2,
    DpiUnaware = 3,
    DpiUnawareGdiScaled = 4,
}
