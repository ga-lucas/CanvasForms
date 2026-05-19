namespace Canvas.Windows.Forms;

// ── PrintPageData ─────────────────────────────────────────────────────────────
/// <summary>
/// Represents a single printed page as a structured collection of draw commands.
/// The host can use this to render the page to PDF, XPS, a real printer, etc.
/// </summary>
public sealed record PrintDrawCommand(
    string Kind,          // "text", "rect", "fillRect", "line", "image"
    float X,
    float Y,
    float W,
    float H,
    string? Text,
    string? Color,        // CSS color string, e.g. "#000000"
    string? FontName,
    float   FontSize,
    bool    Bold,
    bool    Italic,
    string? ImageData     // base-64 PNG for image commands
);

public sealed record PrintPageData(
    int PageIndex,
    float PageWidthHundredths,   // page width in hundredths of an inch (WinForms units)
    float PageHeightHundredths,
    IReadOnlyList<PrintDrawCommand> Commands
);

// ── PrintJob ──────────────────────────────────────────────────────────────────
/// <summary>
/// Describes a complete print job ready to be handled by the host.
/// </summary>
public sealed record PrintJob(
    string DocumentName,
    int    Copies,
    bool   Collate,
    bool   Landscape,
    string PaperSizeName,
    float  PaperWidthHundredths,
    float  PaperHeightHundredths,
    float  MarginLeftHundredths,
    float  MarginTopHundredths,
    float  MarginRightHundredths,
    float  MarginBottomHundredths,
    IReadOnlyList<PrintPageData> Pages
);

// ── IHostPrintService ─────────────────────────────────────────────────────────
/// <summary>
/// Host-side print service. Implement this interface in your host/server project
/// and assign it to <see cref="HostPrintService.Current"/> at startup.
/// <para>
/// The canvas layer collects all <c>PrintPage</c> draw commands from the
/// <see cref="System.Drawing.Printing.PrintDocument"/> and forwards a
/// <see cref="PrintJob"/> here. The host can then spool it to the OS printer,
/// render a PDF, save to disk, etc.
/// </para>
/// </summary>
public interface IHostPrintService
{
    /// <summary>
    /// Submits a print job to the host. Called after all <c>PrintPage</c> events
    /// have fired and pages have been collected.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the job was accepted; <c>false</c> to treat it as cancelled.
    /// </returns>
    Task<bool> PrintAsync(PrintJob job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optional: queries the host for the list of available printer names.
    /// Returns an empty array if the host does not support printer enumeration.
    /// </summary>
    Task<string[]> GetPrinterNamesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Array.Empty<string>());

    /// <summary>
    /// Optional: returns the host's default printer name, or <c>null</c> if unknown.
    /// </summary>
    Task<string?> GetDefaultPrinterAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);
}

// ── HostPrintService ──────────────────────────────────────────────────────────
/// <summary>
/// Static holder for the host-provided print service.
/// Set <see cref="Current"/> from your host project during startup:
/// <code>
/// HostPrintService.Current = new MyPrintService();
/// </code>
/// When <see cref="Current"/> is <c>null</c> the canvas layer falls back to a
/// browser-alert stub so translated apps still compile and run without a host.
/// </summary>
public static class HostPrintService
{
    /// <summary>
    /// Gets or sets the host-provided print service implementation.
    /// Assign this from the host/server project before the first print job is submitted.
    /// </summary>
    public static IHostPrintService? Current { get; set; }
}
