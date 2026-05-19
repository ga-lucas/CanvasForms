using System.Collections.Specialized;
using System.ComponentModel;

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
    public Drawing.Rectangle PageBounds   { get; } = new Drawing.Rectangle(0, 0, 827, 1169);
    public Drawing.Rectangle MarginBounds { get; } = new Drawing.Rectangle(100, 100, 627, 969);
    public PageSettings PageSettings { get; } = new PageSettings();
    // Graphics is null in the browser — no actual rendering surfaces
    public object? Graphics { get; internal set; }
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
