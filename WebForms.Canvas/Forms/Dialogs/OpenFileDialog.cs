namespace System.Windows.Forms;

/// <summary>
/// Prompts the user to open a file.
/// </summary>
public class OpenFileDialog : FileDialog
{
    public OpenFileDialog()
    {
        CheckFileExists = true;
        Multiselect = false;
    }

    protected override string DefaultTitle => "Open";
}
