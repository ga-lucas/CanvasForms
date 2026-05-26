using System.Windows.Forms;

namespace Canvas.Windows.Forms;

/// <summary>
/// Internal application lifecycle implementation.
/// Exposed to translated WinForms apps via System.Windows.Forms.Application shim.
/// </summary>
public static class CanvasApplication
{
    private static FormManager? _formManager;
    private static bool _isRunning;

    public static FormManager? FormManager
    {
        get => _formManager;
        internal set => _formManager = value;
    }

    public static bool IsRunning => _isRunning;

    public static void Run(Form mainForm)
    {
        if (_formManager == null)
        {
            throw new InvalidOperationException("Application must be initialized with a FormManager before calling Run()");
        }

        _isRunning = true;
        _formManager.ShowForm(mainForm);
    }

    public static void Run()
    {
        _isRunning = true;
        // Message loop handled by Blazor/host.
    }

    public static void Exit()
    {
        _isRunning = false;
        _formManager?.CloseAll();
    }

    public static void Exit(int exitCode) => Exit();

    public static IReadOnlyList<Form> OpenForms => _formManager?.OpenForms ?? Array.Empty<Form>();

    public static event EventHandler? ApplicationExit;

    internal static void OnApplicationExit() => ApplicationExit?.Invoke(null, EventArgs.Empty);

    public static string CommonAppDataPath => "/appdata/common";
    public static string UserAppDataPath => "/appdata/user";
    public static string CompanyName => "Canvas.Windows.Forms";
    public static string ProductName => "Canvas.Windows.Forms Application";
    public static string ProductVersion => "1.0.0";

    // ── MessageBox integration ────────────────────────────────────────────────

    /// <summary>
    /// Optional synchronous handler for <see cref="System.Windows.Forms.MessageBox.Show"/>.
    /// The host (e.g. Blazor Server) can assign this to forward messages to the browser.
    /// Signature: (owner, text, caption, buttons, icon, defaultButton, options) → DialogResult
    /// </summary>
    public static Func<
        System.Windows.Forms.IWin32Window?,
        string, string,
        System.Windows.Forms.MessageBoxButtons,
        System.Windows.Forms.MessageBoxIcon,
        System.Windows.Forms.MessageBoxDefaultButton,
        System.Windows.Forms.MessageBoxOptions,
        System.Windows.Forms.DialogResult>? MessageBoxHandler { get; set; }

    /// <summary>
    /// Optional async handler for <see cref="System.Windows.Forms.MessageBox.ShowAsync"/>.
    /// Signature: (owner, text, caption, buttons, icon) → Task&lt;DialogResult&gt;
    /// </summary>
    public static Func<
        System.Windows.Forms.IWin32Window?,
        string, string,
        System.Windows.Forms.MessageBoxButtons,
        System.Windows.Forms.MessageBoxIcon,
        Task<System.Windows.Forms.DialogResult>>? AsyncMessageBoxHandler { get; set; }
}
