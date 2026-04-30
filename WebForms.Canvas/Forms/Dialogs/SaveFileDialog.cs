namespace System.Windows.Forms;

/// <summary>
/// Prompts the user to specify a filename to save to.
/// Matches the WinForms <c>SaveFileDialog</c> API surface.
/// </summary>
public class SaveFileDialog : FileDialog
{
    // ── WinForms-compatible properties ────────────────────────────────────────

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box prompts the user
    /// for permission to create a file if the user specifies a file that does not exist.
    /// Matches WinForms <c>SaveFileDialog.CreatePrompt</c>.
    /// </summary>
    public bool CreatePrompt { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog box displays a warning
    /// if the user specifies a file name that already exists.
    /// Matches WinForms <c>SaveFileDialog.OverwritePrompt</c>.
    /// </summary>
    public bool OverwritePrompt { get; set; } = true;

    // ── Constructor ────────────────────────────────────────────────────────────

    public SaveFileDialog()
    {
        CheckFileExists = false;  // Save dialogs don't require the file to already exist
        CheckPathExists = true;
    }

    // ── FileDialog overrides ──────────────────────────────────────────────────

    protected override string DefaultTitle => "Save As";

    protected override bool IsSaveDialog => true;

    public override void Reset()
    {
        base.Reset();
        CreatePrompt    = false;
        OverwritePrompt = true;
    }

    // ── Stream helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Opens the file selected in the dialog with read/write access.
    /// Creates the file if it does not already exist.
    /// Matches WinForms <c>SaveFileDialog.OpenFile()</c>.
    /// </summary>
    public System.IO.Stream OpenFile()
    {
        if (string.IsNullOrWhiteSpace(FileName))
            throw new InvalidOperationException("No file selected.");

        return new System.IO.FileStream(
            FileName,
            System.IO.FileMode.Create,
            System.IO.FileAccess.ReadWrite);
    }
}
