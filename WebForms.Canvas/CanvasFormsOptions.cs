namespace Canvas.Windows.Forms;

public static class CanvasFormsOptions
{
    public static bool EnableFileDialogUpload { get; set; }

    /// <summary>
    /// Path to a JSON theme file that overrides default CanvasForms colors.
    /// When set, the theme is loaded automatically by the host at startup.
    /// Leave <c>null</c> to use built-in defaults.
    /// </summary>
    public static string? ThemeFilePath { get; set; }
}
