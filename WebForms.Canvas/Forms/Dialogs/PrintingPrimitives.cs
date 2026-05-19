using System.Collections.Specialized;
using System.ComponentModel;
using CFont       = Canvas.Windows.Forms.Drawing.Font;
using CPen        = Canvas.Windows.Forms.Drawing.Pen;
using CBrush      = Canvas.Windows.Forms.Drawing.Brush;
using CSolidBrush = Canvas.Windows.Forms.Drawing.SolidBrush;
using CColor      = Canvas.Windows.Forms.Drawing.Color;
using CImage      = Canvas.Windows.Forms.Drawing.Image;

namespace System.Drawing.Printing;

// ── Enums ─────────────────────────────────────────────────────────────────────
public enum PrintRange         { AllPages, Selection, SomePages, CurrentPage }
public enum Duplex             { Default, Simplex, Horizontal, Vertical }
public enum PaperKind          { Custom, Letter, A4 }
public enum PaperSourceKind    { AutomaticFeed, ManualFeed, Upper, Lower, Middle }
public enum PrinterResolutionKind { High, Medium, Low, Draft, Custom }
public enum PrintAction        { PrintToFile, PrintToPreview, PrintToPrinter }

// ── Margins ───────────────────────────────────────────────────────────────────
public class Margins
{
    public int Left   { get; set; } = 100;
    public int Right  { get; set; } = 100;
    public int Top    { get; set; } = 100;
    public int Bottom { get; set; } = 100;
    public Margins() { }
    public Margins(int left, int right, int top, int bottom)
    { Left = left; Right = right; Top = top; Bottom = bottom; }
}

// ── PaperSize ─────────────────────────────────────────────────────────────────
public class PaperSize
{
    public string    PaperName { get; set; }
    public int       Width     { get; set; }
    public int       Height    { get; set; }
    public PaperKind Kind      => PaperKind.Custom;
    public PaperSize()                                { PaperName = "A4"; Width = 827; Height = 1169; }
    public PaperSize(string name, int w, int h)       { PaperName = name; Width = w; Height = h; }
}

// ── PaperSource ───────────────────────────────────────────────────────────────
public class PaperSource
{
    public string         SourceName { get; set; } = "Auto";
    public PaperSourceKind Kind => PaperSourceKind.AutomaticFeed;
}

// ── PrinterResolution ─────────────────────────────────────────────────────────
public class PrinterResolution
{
    public PrinterResolutionKind Kind { get; set; } = PrinterResolutionKind.High;
    public int X { get; set; } = 600;
    public int Y { get; set; } = 600;
}

// ── PageSettings ──────────────────────────────────────────────────────────────
public class PageSettings
{
    public Drawing.Rectangle Bounds         => new Drawing.Rectangle(0, 0, 827, 1169);
    public Margins    Margins               { get; set; } = new Margins();
    public bool       Color                 { get; set; } = true;
    public bool       Landscape             { get; set; } = false;
    public PaperSize  PaperSize             { get; set; } = new PaperSize("A4", 827, 1169);
    public PaperSource PaperSource          { get; set; } = new PaperSource();
    public PrinterResolution PrinterResolution { get; set; } = new PrinterResolution();
    public PrinterSettings PrinterSettings  { get; } = new PrinterSettings();
}

// ── PrinterSettings ───────────────────────────────────────────────────────────
public class PrinterSettings
{
    public string    PrinterName           { get; set; } = "Canvas (unavailable)";
    public bool      IsValid               => false;
    public bool      IsDefaultPrinter      => true;
    public int       Copies                { get; set; } = 1;
    public bool      Collate               { get; set; } = false;
    public bool      PrintToFile           { get; set; } = false;
    public string    PrintFileName         { get; set; } = string.Empty;
    public PrintRange PrintRange           { get; set; } = PrintRange.AllPages;
    public int       FromPage              { get; set; } = 0;
    public int       ToPage                { get; set; } = 0;
    public int       MaximumPage           { get; set; } = 9999;
    public int       MinimumPage           { get; set; } = 0;
    public bool      SupportsColor         => false;
    public bool      CanDuplex             => false;
    public Duplex    Duplex                { get; set; } = Duplex.Default;
    public PageSettings DefaultPageSettings { get; } = new PageSettings();
    public static StringCollection InstalledPrinters => new StringCollection();
}

// ── PrintPageEventArgs ────────────────────────────────────────────────────────
public class PrintPageEventArgs : EventArgs
{
    public bool Cancel       { get; set; }
    public bool HasMorePages { get; set; }
    public Drawing.Rectangle PageBounds   { get; internal set; } = new Drawing.Rectangle(0, 0, 827, 1169);
    public Drawing.Rectangle MarginBounds { get; internal set; } = new Drawing.Rectangle(100, 100, 627, 969);
    public PageSettings PageSettings { get; internal set; } = new PageSettings();
    /// <summary>
    /// A <see cref="PrintGraphics"/> capture surface populated by <c>PrintDocument.Print()</c>.
    /// Draw to this object inside a <c>PrintPage</c> handler; commands are collected
    /// and forwarded to <see cref="Canvas.Windows.Forms.IHostPrintService"/>.
    /// </summary>
    public PrintGraphics? Graphics { get; internal set; }
}

// ── QueryPageSettingsEventArgs ────────────────────────────────────────────────
public class QueryPageSettingsEventArgs : EventArgs
{
    public bool        Cancel       { get; set; }
    public PageSettings PageSettings { get; } = new PageSettings();
}

// ── PrintEventArgs ────────────────────────────────────────────────────────────
public class PrintEventArgs : CancelEventArgs
{
    public PrintAction PrintAction { get; } = PrintAction.PrintToPrinter;
}

// ── Delegates ─────────────────────────────────────────────────────────────────
public delegate void PrintPageEventHandler(object sender, PrintPageEventArgs e);
public delegate void QueryPageSettingsEventHandler(object sender, QueryPageSettingsEventArgs e);
public delegate void PrintEventHandler(object sender, PrintEventArgs e);

// ── PrintGraphics ─────────────────────────────────────────────────────────────
/// <summary>
/// A <c>Graphics</c>-compatible capture surface used during <c>PrintDocument.Print()</c>.
/// Draw calls are recorded as <see cref="Canvas.Windows.Forms.PrintDrawCommand"/> entries
/// that the host can then render to a real printer, PDF, or other output.
/// </summary>
public class PrintGraphics
{
    private readonly List<Canvas.Windows.Forms.PrintDrawCommand> _commands = new();

    /// <summary>Returns the collected draw commands for this page.</summary>
    public IReadOnlyList<Canvas.Windows.Forms.PrintDrawCommand> Commands => _commands;

    /// <summary>Clears all recorded commands (called between pages).</summary>
    public void Clear() => _commands.Clear();

    // ── helpers ───────────────────────────────────────────────────────────────
    private static string ToCss(CColor c)
        => $"rgba({c.R},{c.G},{c.B},{c.A / 255f:F3})";

    private static string PenColor(CPen pen) => ToCss(pen.Color);

    private static (string color, string fontName, float fontSize, bool bold, bool italic)
        BrushFont(CBrush brush, CFont? font)
    {
        var color = brush is CSolidBrush sb ? ToCss(sb.Color) : "#000000";
        var isBold   = font?.Style == Canvas.Windows.Forms.Drawing.FontStyle.Bold
                    || font?.Style == Canvas.Windows.Forms.Drawing.FontStyle.BoldItalic;
        var isItalic = font?.Style == Canvas.Windows.Forms.Drawing.FontStyle.Italic
                    || font?.Style == Canvas.Windows.Forms.Drawing.FontStyle.BoldItalic;
        return (color, font?.Family ?? "Arial", font?.Size ?? 12f, isBold, isItalic);
    }

    // ── Draw primitives ───────────────────────────────────────────────────────
    public void DrawLine(CPen pen, float x1, float y1, float x2, float y2)
        => _commands.Add(new("line", x1, y1, x2 - x1, y2 - y1, null, PenColor(pen), null, 0, false, false, null));

    public void DrawLine(CPen pen, int x1, int y1, int x2, int y2)
        => DrawLine(pen, (float)x1, (float)y1, (float)x2, (float)y2);

    public void DrawLine(CPen pen, System.Drawing.Point pt1, System.Drawing.Point pt2)
        => DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);

    public void DrawRectangle(CPen pen, float x, float y, float w, float h)
        => _commands.Add(new("rect", x, y, w, h, null, PenColor(pen), null, 0, false, false, null));

    public void DrawRectangle(CPen pen, int x, int y, int w, int h)
        => DrawRectangle(pen, (float)x, (float)y, (float)w, (float)h);

    public void DrawRectangle(CPen pen, System.Drawing.Rectangle rect)
        => DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);

    public void FillRectangle(CBrush brush, float x, float y, float w, float h)
    {
        var color = brush is CSolidBrush sb ? ToCss(sb.Color) : "#000000";
        _commands.Add(new("fillRect", x, y, w, h, null, color, null, 0, false, false, null));
    }

    public void FillRectangle(CBrush brush, int x, int y, int w, int h)
        => FillRectangle(brush, (float)x, (float)y, (float)w, (float)h);

    public void FillRectangle(CBrush brush, System.Drawing.Rectangle rect)
        => FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);

    public void DrawEllipse(CPen pen, float x, float y, float w, float h)
        => _commands.Add(new("ellipse", x, y, w, h, null, PenColor(pen), null, 0, false, false, null));

    public void DrawEllipse(CPen pen, int x, int y, int w, int h)
        => DrawEllipse(pen, (float)x, (float)y, (float)w, (float)h);

    public void DrawEllipse(CPen pen, System.Drawing.Rectangle rect)
        => DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);

    public void FillEllipse(CBrush brush, float x, float y, float w, float h)
    {
        var color = brush is CSolidBrush sb ? ToCss(sb.Color) : "#000000";
        _commands.Add(new("fillEllipse", x, y, w, h, null, color, null, 0, false, false, null));
    }

    public void FillEllipse(CBrush brush, int x, int y, int w, int h)
        => FillEllipse(brush, (float)x, (float)y, (float)w, (float)h);

    public void FillEllipse(CBrush brush, System.Drawing.Rectangle rect)
        => FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);

    public void DrawString(string text, CFont font, CBrush brush, float x, float y)
    {
        var (color, fontName, fontSize, bold, italic) = BrushFont(brush, font);
        _commands.Add(new("text", x, y, 0, 0, text, color, fontName, fontSize, bold, italic, null));
    }

    public void DrawString(string text, CFont font, CBrush brush, int x, int y)
        => DrawString(text, font, brush, (float)x, (float)y);

    public void DrawString(string text, CFont font, CBrush brush, System.Drawing.RectangleF rect)
        => DrawString(text, font, brush, rect.X, rect.Y);

    public void DrawString(string text, CFont font, CBrush brush, System.Drawing.Rectangle rect)
        => DrawString(text, font, brush, (float)rect.X, (float)rect.Y);

    /// <summary>Measures the bounding box of a string (estimation).</summary>
    public System.Drawing.SizeF MeasureString(string text, CFont font)
        => new System.Drawing.SizeF(text.Length * font.Size * 0.6f, font.Size * 1.2f);

    public System.Drawing.SizeF MeasureString(string text, CFont font, int maxWidth)
        => MeasureString(text, font);

    public void DrawImage(CImage image, float x, float y, float w, float h)
        => _commands.Add(new("image", x, y, w, h, null, null, null, 0, false, false, null));

    public void DrawImage(CImage image, int x, int y, int w, int h)
        => DrawImage(image, (float)x, (float)y, (float)w, (float)h);

    public void DrawImage(CImage image, System.Drawing.Rectangle rect)
        => DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);

    // Clip / transform stubs (no-op — PoC)
    public System.Drawing.RectangleF ClipBounds => new System.Drawing.RectangleF(0, 0, float.MaxValue, float.MaxValue);
    public void SetClip(System.Drawing.Rectangle rect) { }
    public void SetClip(System.Drawing.RectangleF rect) { }
    public void ResetClip() { }
    public void TranslateTransform(float dx, float dy) { }
    public void ScaleTransform(float sx, float sy) { }
    public void RotateTransform(float angle) { }
    public void ResetTransform() { }
    public void Flush() { }
    public void Dispose() { }
}
