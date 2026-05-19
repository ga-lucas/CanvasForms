namespace Canvas.Windows.Forms;

public static class CanvasFormsOptions
{
    public static bool EnableFileDialogUpload { get; set; }

    /// <summary>
    /// Enables host-side print support. When <c>true</c>, <see cref="PrintDocument.Print()"/>
    /// collects page draw commands and submits a <see cref="PrintJob"/> to
    /// <see cref="HostPrintService.Current"/>. Has no effect when <see cref="HostPrintService.Current"/>
    /// is <c>null</c>. Defaults to <c>true</c>.
    /// </summary>
    public static bool EnablePrint { get; set; } = true;

    /// <summary>
    /// Path to a JSON theme file that overrides default CanvasForms colors.
    /// When set, the theme is loaded automatically by the host at startup.
    /// Leave <c>null</c> to use built-in defaults.
    /// </summary>
    public static string? ThemeFilePath { get; set; }
}
