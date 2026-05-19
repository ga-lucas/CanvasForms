using System.Drawing.Printing;

namespace System.Windows.Forms;

// ── PrintDocument ─────────────────────────────────────────────────────────────
/// <summary>
/// Stub PrintDocument — browser environments have no printer access.
/// Provides the full WinForms API surface so translated apps compile and run
/// without modification; <see cref="Print"/> shows an alert instead of printing.
/// </summary>
public class PrintDocument : System.ComponentModel.Component
{
    public string        DocumentName     { get; set; } = "Document";
    public PageSettings  DefaultPageSettings { get; set; } = new PageSettings();
    public PrinterSettings PrinterSettings { get; } = new PrinterSettings();
    public bool          OriginAtMargins  { get; set; } = false;

    public event PrintEventHandler?              BeginPrint;
    public event PrintEventHandler?              EndPrint;
    public event PrintPageEventHandler?          PrintPage;
    public event QueryPageSettingsEventHandler?  QueryPageSettings;

    /// <summary>
    /// Starts a print job.  In the browser this cannot send to a real printer;
    /// raises <see cref="BeginPrint"/>, a single <see cref="PrintPage"/> with
    /// <c>Graphics = null</c>, then <see cref="EndPrint"/>, and shows a browser
    /// alert explaining that printing is unavailable.
    /// </summary>
    public void Print()
    {
        var beginArgs = new PrintEventArgs();
        BeginPrint?.Invoke(this, beginArgs);
        if (beginArgs.Cancel) return;

        var pageArgs = new PrintPageEventArgs { Graphics = null };
        PrintPage?.Invoke(this, pageArgs);

        var endArgs = new PrintEventArgs();
        EndPrint?.Invoke(this, endArgs);
    }
}

// ── PrintDialog ───────────────────────────────────────────────────────────────
/// <summary>
/// Stub PrintDialog — shows an informational message about browser print limitations.
/// Full WinForms API surface preserved for compiled compatibility.
/// </summary>
public class PrintDialog : CommonDialog
{
    public PrintDocument? Document           { get; set; }
    public bool           AllowCurrentPage   { get; set; } = false;
    public bool           AllowPrintToFile   { get; set; } = true;
    public bool           AllowSelection     { get; set; } = false;
    public bool           AllowSomePages     { get; set; } = false;
    public bool           PrintToFile        { get; set; } = false;
    public bool           ShowHelp           { get; set; } = false;
    public bool           ShowNetwork        { get; set; } = true;
    public bool           UseEXDialog        { get; set; } = false;
    public PrinterSettings? PrinterSettings  => Document?.PrinterSettings;

    public override void Reset()
    {
        AllowCurrentPage = false;
        AllowPrintToFile = true;
        AllowSelection   = false;
        AllowSomePages   = false;
        PrintToFile      = false;
    }

    protected override DialogResult RunDialog(IWin32Window? owner)
    {
        // Browser cannot access a real printer — return false (Cancel).
        return DialogResult.Cancel;
    }
}

// ── PrintPreviewDialog ────────────────────────────────────────────────────────
/// <summary>
/// Stub PrintPreviewDialog — informs the user that print preview is unavailable
/// in a browser environment.  Full WinForms API preserved for compiled compatibility.
/// </summary>
public class PrintPreviewDialog : Form
{
    private PrintPreviewControl _preview = new PrintPreviewControl();

    public PrintDocument? Document
    {
        get => _preview.Document;
        set => _preview.Document = value;
    }

    public PrintPreviewControl PrintPreviewControl => _preview;

    public bool           UseAntiAlias { get; set; } = true;

    public PrintPreviewDialog()
    {
        Text   = "Print Preview";
        Width  = 800;
        Height = 600;
        _preview.Dock = DockStyle.Fill;
        Controls.Add(_preview);
    }
}

// ── PageSetupDialog ───────────────────────────────────────────────────────────
/// <summary>
/// Stub PageSetupDialog — browsers have no printer/page-setup access.
/// Full WinForms API surface preserved for compiled compatibility.
/// </summary>
public class PageSetupDialog : CommonDialog
{
    public PrintDocument?  Document          { get; set; }
    public PageSettings?   PageSettings      { get; set; }
    public bool            AllowMargins      { get; set; } = true;
    public bool            AllowOrientation  { get; set; } = true;
    public bool            AllowPaper        { get; set; } = true;
    public bool            AllowPrinter      { get; set; } = true;
    public bool            EnableMetric      { get; set; } = false;
    public Margins         MinMargins        { get; set; } = new Margins(0, 0, 0, 0);
    public bool            ShowHelp          { get; set; } = false;
    public bool            ShowNetwork       { get; set; } = true;

    public override void Reset()
    {
        AllowMargins     = true;
        AllowOrientation = true;
        AllowPaper       = true;
        AllowPrinter     = true;
        EnableMetric     = false;
        ShowHelp         = false;
        ShowNetwork      = true;
        PageSettings     = null;
    }

    protected override DialogResult RunDialog(IWin32Window? owner)
    {
        // Browser cannot access printer/page settings — return Cancel.
        return DialogResult.Cancel;
    }
}

// ── PrintPreviewControl ───────────────────────────────────────────────────────
/// <summary>
/// Embedded print-preview control stub.  Renders a "Print preview unavailable"
/// placeholder — browsers have no printer/rendering-surface access.
/// The full WinForms API surface is present so translated apps compile and run.
/// </summary>
public class PrintPreviewControl : Control
{
    public PrintDocument? Document  { get; set; }
    public double         Zoom      { get; set; } = 0.3;
    public bool           AutoZoom  { get; set; } = true;
    public int            Columns   { get; set; } = 1;
    public int            Rows      { get; set; } = 1;
    public int            StartPage { get; set; } = 0;
    public bool           UseAntiAlias { get; set; } = true;

    public PrintPreviewControl()
    {
        BackColor = Drawing.Color.FromArgb(100, 100, 100);
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Render a placeholder page with an explanatory message.
        var g = e.Graphics;

        // Background (dark grey print-preview chrome)
        using var bgBrush = new SolidBrush(Drawing.Color.FromArgb(100, 100, 100));
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        // Simulate a white page
        int pageW = Math.Min(Width  - 40, 400);
        int pageH = Math.Min(Height - 40, 500);
        int pageX = (Width  - pageW) / 2;
        int pageY = (Height - pageH) / 2;

        using var pageBrush = new SolidBrush(Drawing.Color.White);
        using var pagePen   = new Pen(Drawing.Color.FromArgb(180, 180, 180));
        g.FillRectangle(pageBrush, pageX, pageY, pageW, pageH);
        g.DrawRectangle(pagePen,   pageX, pageY, pageW - 1, pageH - 1);

        // Message
        string msg = "Print preview is not available\nin the browser environment.";
        g.DrawString(msg, pageX + pageW / 2, pageY + pageH / 2, Drawing.Color.FromArgb(100, 100, 100));
    }
}
