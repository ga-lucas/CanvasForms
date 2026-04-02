using WebForms.Canvas;
using WebForms.Canvas.Forms;
using WebForms.Canvas.Samples;

namespace WebForms.Canvas.Host;

/// <summary>
/// Example Program class showing how to structure a Windows Forms application
/// This follows the standard Windows Forms pattern:
/// 
/// static void Main()
/// {
///     Application.Run(new MainForm());
/// }
/// 
/// In the Blazor/WASM environment, the FormManager is initialized by the Desktop component,
/// and forms are shown using Application class methods or FormManager methods.
/// </summary>
public static class Program
{
    /// <summary>
    /// Entry point example - shows how a Windows Forms app would be structured
    /// Note: In Blazor WASM, this isn't called directly - see HomePage.razor instead
    /// </summary>
    public static void Main()
    {
        // This is what a traditional Windows Forms app looks like:
        // Application.Run(new WelcomeForm());

        // In our Blazor environment, the Desktop component initializes the FormManager
        // and you can show forms using:
        //   Application.FormManager?.ShowForm(new WelcomeForm());
        // or use the convenience methods:
        //   Application.FormManager?.ShowOrCreateForm<WelcomeForm>();
    }

    /// <summary>
    /// Example startup method that can be called from Blazor components
    /// </summary>
    public static void InitializeApplication(FormManager formManager)
    {
        // FormManager is automatically set by Desktop component
        // Just show the main form
        var mainForm = new WelcomeForm();
        formManager.MainForm = mainForm;
        Application.Run(mainForm);
    }
}
