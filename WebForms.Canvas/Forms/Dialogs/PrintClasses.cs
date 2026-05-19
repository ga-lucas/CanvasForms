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
    /// Starts a print job.
    /// Raises <see cref="BeginPrint"/>, drives <see cref="PrintPage"/> events collecting
    /// each page via <see cref="System.Drawing.Printing.PrintGraphics"/>, raises
    /// <see cref="EndPrint"/>, then submits the <see cref="Canvas.Windows.Forms.PrintJob"/>
    /// to <see cref="Canvas.Windows.Forms.HostPrintService.Current"/> (if registered).
    /// Falls back to a no-op alert stub when no host service is wired up.
    /// </summary>
    public void Print()
    {
        var beginArgs = new PrintEventArgs();
        BeginPrint?.Invoke(this, beginArgs);
        if (beginArgs.Cancel) return;

        var pages = new List<Canvas.Windows.Forms.PrintPageData>();
        int pageIndex = 0;

        var settings = DefaultPageSettings;
        var pageArgs = new PrintPageEventArgs
        {
            PageBounds   = settings.Bounds,
            MarginBounds = new System.Drawing.Rectangle(
                settings.Margins.Left,
                settings.Margins.Top,
                settings.Bounds.Width  - settings.Margins.Left - settings.Margins.Right,
                settings.Bounds.Height - settings.Margins.Top  - settings.Margins.Bottom),
            PageSettings = settings
        };

        do
        {
            var g = new System.Drawing.Printing.PrintGraphics();
            pageArgs.HasMorePages = false;
            pageArgs.Cancel       = false;
            pageArgs.Graphics     = g;

            PrintPage?.Invoke(this, pageArgs);

            pages.Add(new Canvas.Windows.Forms.PrintPageData(
                pageIndex++,
                settings.PaperSize.Width,
                settings.PaperSize.Height,
                g.Commands));

        } while (pageArgs.HasMorePages && !pageArgs.Cancel);

        var endArgs = new PrintEventArgs();
        EndPrint?.Invoke(this, endArgs);

        // Fire-and-forget submission to the host
        _ = SubmitJobAsync(pages);
    }

    private async System.Threading.Tasks.Task SubmitJobAsync(
        List<Canvas.Windows.Forms.PrintPageData> pages)
    {
        var svc = Canvas.Windows.Forms.HostPrintService.Current;
        if (svc is null) return;

        var s = DefaultPageSettings;
        var job = new Canvas.Windows.Forms.PrintJob(
            DocumentName,
            PrinterSettings.Copies,
            PrinterSettings.Collate,
            s.Landscape,
            s.PaperSize.PaperName,
            s.PaperSize.Width,
            s.PaperSize.Height,
            s.Margins.Left,
            s.Margins.Top,
            s.Margins.Right,
            s.Margins.Bottom,
            pages);

        await svc.PrintAsync(job);
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
        => DialogResult.Cancel;

    /// <summary>
    /// Shows a canvas-rendered print dialog asynchronously, then calls
    /// <see cref="PrintDocument.Print()"/> on acceptance.
    /// </summary>
    public async Task<DialogResult> ShowPrintDialogAsync()
    {
        if (Document is null) return DialogResult.Cancel;

        // Gather available printer names from the host (optional)
        var svc = Canvas.Windows.Forms.HostPrintService.Current;
        string[] printers = svc is not null
            ? await svc.GetPrinterNamesAsync()
            : Array.Empty<string>();
        string defaultPrinter = (svc is not null ? await svc.GetDefaultPrinterAsync() : null)
            ?? Document.PrinterSettings.PrinterName;

        // Build the dialog form
        var dlg = new Form
        {
            Text          = "Print",
            Width         = 400,
            Height        = 280,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox   = false, MinimizeBox = false
        };

        int y = 16, lw = 110, fw = 240, lx = 16, fx = 130;

        // Printer label + combo
        var lblPrinter = new Label  { Text = "Printer:", Left = lx, Top = y, Width = lw };
        var cbPrinter  = new ComboBox { Left = fx, Top = y, Width = fw, DropDownStyle = ComboBoxStyle.DropDownList };
        cbPrinter.Items.Add(defaultPrinter);
        foreach (var p in printers) if (p != defaultPrinter) cbPrinter.Items.Add(p);
        cbPrinter.SelectedIndex = 0;
        y += 36;

        // Copies
        var lblCopies = new Label   { Text = "Copies:", Left = lx, Top = y, Width = lw };
        var nudCopies = new NumericUpDown { Left = fx, Top = y, Width = 80, Minimum = 1, Maximum = 999, Value = Document.PrinterSettings.Copies };
        y += 36;

        // Collate
        var chkCollate = new CheckBox { Text = "Collate", Left = fx, Top = y, Width = fw, Checked = Document.PrinterSettings.Collate };
        y += 36;

        // Page range
        var lblRange  = new Label   { Text = "Page range:", Left = lx, Top = y, Width = lw };
        var rbAll     = new RadioButton { Text = "All",   Left = fx,       Top = y, Width = 60, Checked = true };
        var rbPages   = new RadioButton { Text = "Pages:", Left = fx + 64, Top = y, Width = 70 };
        var txtFrom   = new TextBox { Left = fx + 138, Top = y, Width = 40, Text = "1" };
        var lblTo     = new Label   { Text = "to", Left = fx + 182, Top = y, Width = 20 };
        var txtTo     = new TextBox { Left = fx + 204, Top = y, Width = 40, Text = "1" };
        y += 44;

        // Buttons
        var btnOK     = new Button { Text = "Print",  DialogResult = DialogResult.OK,     Left = dlg.Width - 200, Top = y, Width = 80 };
        var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = dlg.Width - 110, Top = y, Width = 80 };

        dlg.Controls.AddRange(new Control[]
        {
            lblPrinter, cbPrinter, lblCopies, nudCopies, chkCollate,
            lblRange, rbAll, rbPages, txtFrom, lblTo, txtTo,
            btnOK, btnCancel
        });
        dlg.AcceptButton = btnOK;
        dlg.CancelButton = btnCancel;

        var result = await dlg.ShowDialogAsync();
        if (result != DialogResult.OK) return DialogResult.Cancel;

        // Apply settings back
        Document.PrinterSettings.PrinterName = cbPrinter.SelectedItem?.ToString() ?? defaultPrinter;
        Document.PrinterSettings.Copies      = (int)nudCopies.Value;
        Document.PrinterSettings.Collate     = chkCollate.Checked;
        if (rbPages.Checked &&
            int.TryParse(txtFrom.Text, out var from) &&
            int.TryParse(txtTo.Text,   out var to))
        {
            Document.PrinterSettings.PrintRange = PrintRange.SomePages;
            Document.PrinterSettings.FromPage   = from;
            Document.PrinterSettings.ToPage     = to;
        }
        else
        {
            Document.PrinterSettings.PrintRange = PrintRange.AllPages;
        }

        Document.Print();
        return DialogResult.OK;
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
        => DialogResult.Cancel;

    /// <summary>
    /// Shows a canvas-rendered page-setup dialog asynchronously, writing results
    /// back to <see cref="PageSettings"/> / <see cref="Document.DefaultPageSettings"/>.
    /// </summary>
    public async Task<DialogResult> ShowPageSetupDialogAsync()
    {
        var ps = PageSettings ?? Document?.DefaultPageSettings ?? new PageSettings();

        var dlg = new Form
        {
            Text            = "Page Setup",
            Width           = 380,
            Height          = 300,
            StartPosition   = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox     = false, MinimizeBox = false
        };

        int y = 16, lw = 110, lx = 16, fx = 140, fw = 200;

        // Paper size
        var lblPaper   = new Label   { Text = "Paper size:", Left = lx, Top = y, Width = lw };
        var cbPaper    = new ComboBox { Left = fx, Top = y, Width = fw, DropDownStyle = ComboBoxStyle.DropDownList };
        string[] sizes = { "A4", "Letter", "Legal", "A3" };
        cbPaper.Items.AddRange(sizes);
        cbPaper.SelectedItem = ps.PaperSize.PaperName;
        if (cbPaper.SelectedIndex < 0) cbPaper.SelectedIndex = 0;
        y += 36;

        // Orientation
        var lblOrient  = new Label      { Text = "Orientation:", Left = lx, Top = y, Width = lw };
        var rbPortrait = new RadioButton { Text = "Portrait",  Left = fx,       Top = y, Width = 90, Checked = !ps.Landscape };
        var rbLandscap = new RadioButton { Text = "Landscape", Left = fx + 94,  Top = y, Width = 90, Checked = ps.Landscape };
        y += 36;

        // Margins (hundredths of an inch)
        var lblMargins = new Label { Text = "Margins (1/100\"):", Left = lx, Top = y, Width = lw + 20 };
        y += 24;
        var lblL  = new Label   { Text = "Left:",   Left = lx,       Top = y, Width = 40 };
        var nudL  = new NumericUpDown { Left = lx + 44,   Top = y, Width = 60, Minimum = 0, Maximum = 2000, Value = ps.Margins.Left };
        var lblT  = new Label   { Text = "Top:",    Left = lx + 120,  Top = y, Width = 40 };
        var nudT  = new NumericUpDown { Left = lx + 164,  Top = y, Width = 60, Minimum = 0, Maximum = 2000, Value = ps.Margins.Top };
        y += 32;
        var lblR  = new Label   { Text = "Right:",  Left = lx,       Top = y, Width = 40 };
        var nudR  = new NumericUpDown { Left = lx + 44,   Top = y, Width = 60, Minimum = 0, Maximum = 2000, Value = ps.Margins.Right };
        var lblBt = new Label   { Text = "Bottom:", Left = lx + 120,  Top = y, Width = 40 };
        var nudB  = new NumericUpDown { Left = lx + 164,  Top = y, Width = 60, Minimum = 0, Maximum = 2000, Value = ps.Margins.Bottom };
        y += 40;

        var btnOK     = new Button { Text = "OK",     DialogResult = DialogResult.OK,     Left = dlg.Width - 200, Top = y, Width = 80 };
        var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = dlg.Width - 110, Top = y, Width = 80 };

        dlg.Controls.AddRange(new Control[]
        {
            lblPaper, cbPaper, lblOrient, rbPortrait, rbLandscap,
            lblMargins, lblL, nudL, lblT, nudT, lblR, nudR, lblBt, nudB,
            btnOK, btnCancel
        });
        dlg.AcceptButton = btnOK;
        dlg.CancelButton = btnCancel;

        var result = await dlg.ShowDialogAsync();
        if (result != DialogResult.OK) return DialogResult.Cancel;

        // Map paper name → dimensions (hundredths of an inch)
        var (pw, ph) = cbPaper.SelectedItem?.ToString() switch
        {
            "Letter" => (850,  1100),
            "Legal"  => (850,  1400),
            "A3"     => (1169, 1654),
            _        => (827,  1169)  // A4 default
        };

        ps.PaperSize  = new PaperSize(cbPaper.SelectedItem!.ToString()!, pw, ph);
        ps.Landscape  = rbLandscap.Checked;
        ps.Margins    = new Margins((int)nudL.Value, (int)nudR.Value, (int)nudT.Value, (int)nudB.Value);

        if (Document is not null) Document.DefaultPageSettings = ps;
        PageSettings = ps;
        return DialogResult.OK;
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
