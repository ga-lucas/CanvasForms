using System.Windows.Forms;

namespace WebForms.Canvas;

/// <summary>
/// Provides static methods and properties to manage an application, such as methods to start and stop an application,
/// to process Windows messages, and properties to get information about an application.
/// This is compatible with System.Windows.Forms.Application
/// </summary>
public static class Application
{
    private static FormManager? _formManager;
    private static bool _isRunning = false;

    /// <summary>
    /// Gets or sets the FormManager for the application
    /// </summary>
    internal static FormManager? FormManager
    {
        get => _formManager;
        set => _formManager = value;
    }

    /// <summary>
    /// Gets whether the application is currently running
    /// </summary>
    public static bool IsRunning => _isRunning;

    /// <summary>
    /// Begins running a standard application message loop on the current thread, and makes the specified form visible.
    /// </summary>
    /// <param name="mainForm">A Form that represents the form to make visible.</param>
    public static void Run(Form mainForm)
    {
        if (_formManager == null)
        {
            throw new InvalidOperationException("Application must be initialized with a FormManager before calling Run()");
        }

        _isRunning = true;
        _formManager.ShowForm(mainForm);
    }

    /// <summary>
    /// Begins running a standard application message loop on the current thread, without a form.
    /// </summary>
    public static void Run()
    {
        _isRunning = true;
        // Message loop handled by Blazor
    }

    /// <summary>
    /// Informs all message pumps that they must terminate, and then closes all application windows after the messages have been processed.
    /// </summary>
    public static void Exit()
    {
        _isRunning = false;
        _formManager?.CloseAll();
    }

    /// <summary>
    /// Exits the application and closes all forms
    /// </summary>
    /// <param name="exitCode">The exit code to return to the operating system</param>
    public static void Exit(int exitCode)
    {
        Exit();
    }

    /// <summary>
    /// Gets the collection of open forms owned by the application
    /// </summary>
    public static IReadOnlyList<Form> OpenForms
    {
        get => _formManager?.OpenForms ?? Array.Empty<Form>();
    }

    /// <summary>
    /// Occurs when the application is about to shut down
    /// </summary>
    public static event EventHandler? ApplicationExit;

    internal static void OnApplicationExit()
    {
        ApplicationExit?.Invoke(null, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the path for the application data that is shared among all users
    /// </summary>
    public static string CommonAppDataPath => "/appdata/common";

    /// <summary>
    /// Gets the path for the application data of a user
    /// </summary>
    public static string UserAppDataPath => "/appdata/user";

    /// <summary>
    /// Gets the company name associated with the application
    /// </summary>
    public static string CompanyName => "WebForms Canvas";

    /// <summary>
    /// Gets the product name associated with this application
    /// </summary>
    public static string ProductName => "WebForms Canvas Application";

    /// <summary>
    /// Gets the product version associated with this application
    /// </summary>
    public static string ProductVersion => "1.0.0";
}
