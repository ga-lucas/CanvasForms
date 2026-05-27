// Stubs for System.Drawing types that live in System.Drawing.Common (Windows-only GDI+).
// These allow translated WinForms apps to bind at runtime without
// referencing the Windows-only package.

namespace System.Drawing;

/// <summary>
/// <c>System.Drawing.Graphics</c> shim — inherits the canvas-layer
/// <see cref="Canvas.Windows.Forms.Drawing.Graphics"/> so that translated assemblies
/// referencing <c>System.Drawing.Graphics</c> bind without System.Drawing.Common.
/// </summary>
public class Graphics : Canvas.Windows.Forms.Drawing.Graphics { }

/// <summary>
/// Abstract base class for all brush types (stub).
/// Canvas rendering uses <see cref="Canvas.Windows.Forms.Drawing.Brush"/> internally;
/// this stub exists so translated assemblies that reference <c>System.Drawing.Brush</c>
/// resolve the type without requiring System.Drawing.Common.
/// </summary>
public abstract class Brush : IDisposable
{
    public virtual void Dispose() { }
}

/// <summary>Solid-color brush stub for <c>System.Drawing.SolidBrush</c>.</summary>
public sealed class SolidBrush : Brush
{
    public Color Color { get; set; }
    public SolidBrush(Color color) => Color = color;
}

/// <summary>
/// Stub for <c>System.Drawing.FontFamily</c>.
/// Stores the family name; used by <c>Drawing2D.GraphicsPath.AddString</c>.
/// </summary>
public sealed class FontFamily : IDisposable
{
    public string Name { get; }

    public FontFamily(string name) => Name = name;

    public static FontFamily GenericSerif    => new FontFamily("serif");
    public static FontFamily GenericSansSerif => new FontFamily("sans-serif");
    public static FontFamily GenericMonospace => new FontFamily("monospace");

    public bool IsStyleAvailable(int style) => true;
    public void Dispose() { }
}

/// <summary>
/// <c>System.Drawing.Font</c> shim — inherits the canvas-layer <see cref="Canvas.Windows.Forms.Drawing.Font"/>
/// so translated assemblies referencing System.Drawing.Font bind at runtime.
/// </summary>
public class Font : Canvas.Windows.Forms.Drawing.Font
{
    public Font(string family, float size)
        : base(family, size) { }

    public Font(string family, float size, Canvas.Windows.Forms.Drawing.FontStyle style)
        : base(family, size, style) { }

    public Font(string family, float size, Canvas.Windows.Forms.Drawing.FontStyle style, GraphicsUnit unit)
        : base(family, size, style, unit) { }

    public Font(string family, float size, GraphicsUnit unit)
        : base(family, size, Canvas.Windows.Forms.Drawing.FontStyle.Regular, unit) { }

    public Font(FontFamily family, float size)
        : base(family?.Name ?? "Arial", size) { }

    public Font(FontFamily family, float size, Canvas.Windows.Forms.Drawing.FontStyle style)
        : base(family?.Name ?? "Arial", size, style) { }

    public Font(FontFamily family, float size, Canvas.Windows.Forms.Drawing.FontStyle style, GraphicsUnit unit)
        : base(family?.Name ?? "Arial", size, style, unit) { }

    public Font(Canvas.Windows.Forms.Drawing.Font prototype, Canvas.Windows.Forms.Drawing.FontStyle newStyle)
        : base(prototype?.Family ?? "Arial", prototype?.Size ?? 9f, newStyle) { }
}

/// <summary>
/// Stub for <c>System.Drawing.StringFormat</c>.
/// Accepted by Graphics text-drawing APIs; alignment values are forwarded to
/// canvas text-align settings when the canvas renderer implements them.
/// </summary>
public sealed class StringFormat : IDisposable
{
    public StringAlignment Alignment     { get; set; } = StringAlignment.Near;
    public StringAlignment LineAlignment { get; set; } = StringAlignment.Near;
    public StringTrimming  Trimming      { get; set; } = StringTrimming.None;
    public StringFormatFlags FormatFlags { get; set; } = StringFormatFlags.NoWrap;

    public static StringFormat GenericDefault  => new StringFormat();
    public static StringFormat GenericTypographic => new StringFormat { FormatFlags = 0 };

    public void SetTabStops(float firstTabOffset, float[] tabStops) { }
    public void SetMeasurableCharacterRanges(CharacterRange[] ranges) { _ranges = ranges; }
    public CharacterRange[] GetMeasurableCharacterRanges() => _ranges ?? Array.Empty<CharacterRange>();
    private CharacterRange[]? _ranges;
    public void Dispose() { }
}

public enum StringAlignment { Near = 0, Center = 1, Far = 2 }

[Flags]
public enum StringFormatFlags
{
    DirectionRightToLeft  = 0x0001,
    DirectionVertical     = 0x0002,
    FitBlackBox           = 0x0004,
    DisplayFormatControl  = 0x0020,
    NoFontFallback        = 0x0400,
    MeasureTrailingSpaces = 0x0800,
    NoWrap                = 0x1000,
    LineLimit             = 0x2000,
    NoClip                = 0x4000,
}

public enum StringTrimming
{
    None              = 0,
    Character         = 1,
    Word              = 2,
    EllipsisCharacter = 3,
    EllipsisWord      = 4,
    EllipsisPath      = 5,
}

/// <summary>Represents a range of characters within a string.</summary>
public struct CharacterRange
{
    public int First  { get; set; }
    public int Length { get; set; }
    public CharacterRange(int first, int length) { First = first; Length = length; }
}

/// <summary>Stub for CopyPixelOperation (no GDI+ in browser — all overloads are no-ops).</summary>
public enum CopyPixelOperation
{
    Blackness            = 0x00000042,
    CaptureBlt           = 0x40000000,
    DestinationInvert    = 0x00550009,
    MergeCopy            = 0x00C000CA,
    MergePaint           = 0x00BB0226,
    NoMirrorBitmap       = unchecked((int)0x80000000),
    NotSourceCopy        = 0x00330008,
    NotSourceErase       = 0x001100A6,
    PatCopy              = 0x00F00021,
    PatInvert            = 0x005A0049,
    PatPaint             = 0x00FB0A09,
    SourceAnd            = 0x008800C6,
    SourceCopy           = 0x00CC0020,
    SourceErase          = 0x00440328,
    SourceInvert         = 0x00660046,
    SourcePaint          = 0x00EE0086,
    Whiteness            = 0x00FF0062,
}

/// <summary>
/// Translates colors to/from HTML and Win32 color representations.
/// </summary>
public static class ColorTranslator
{
    /// <summary>Converts an HTML color string (#RRGGBB, #RGB, or named CSS color) to a <see cref="Color"/>.</summary>
    public static Color FromHtml(string htmlColor)
    {
        if (string.IsNullOrWhiteSpace(htmlColor)) return Color.Empty;
        if (htmlColor.StartsWith('#'))
        {
            var hex = htmlColor.TrimStart('#');
            if (hex.Length == 6 && int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int rgb))
                return Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
            if (hex.Length == 3)
                return Color.FromArgb(255,
                    Convert.ToByte(new string(hex[0], 2), 16),
                    Convert.ToByte(new string(hex[1], 2), 16),
                    Convert.ToByte(new string(hex[2], 2), 16));
        }
        return Color.FromName(htmlColor);
    }

    /// <summary>Converts a <see cref="Color"/> to an HTML color string (#RRGGBB).</summary>
    public static string ToHtml(Color c)
    {
        if (c.IsEmpty) return string.Empty;
        if (c.A == 0) return "transparent";
        return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }

    /// <summary>Converts an OLE color integer (0x00BBGGRR) to a <see cref="Color"/>.</summary>
    public static Color FromOle(int oleColor)
    {
        return Color.FromArgb(255, oleColor & 0xFF, (oleColor >> 8) & 0xFF, (oleColor >> 16) & 0xFF);
    }

    /// <summary>Converts a <see cref="Color"/> to an OLE color integer.</summary>
    public static int ToOle(Color c) => c.R | (c.G << 8) | (c.B << 16);

    /// <summary>Converts a Win32 COLORREF to a <see cref="Color"/>.</summary>
    public static Color FromWin32(int win32Color) => FromOle(win32Color);

    /// <summary>Converts a <see cref="Color"/> to a Win32 COLORREF integer.</summary>
    public static int ToWin32(Color c) => ToOle(c);
}

/// <summary>
/// <c>System.Drawing.Pen</c> shim — inherits the canvas-layer pen so that
/// translated assemblies referencing <c>System.Drawing.Pen</c> bind correctly.
/// </summary>
public class Pen : Canvas.Windows.Forms.Drawing.Pen
{
    public Pen(Color color) : base(color) { }
    public Pen(Color color, float width) : base(color, width) { }
    public Pen(Brush brush) : base(Canvas.Windows.Forms.Drawing.Color.Black) { }
    public Pen(Brush brush, float width) : base(Canvas.Windows.Forms.Drawing.Color.Black, width) { }

    /// <summary>
    /// Shadows the base <c>DashStyle</c> property so that translated code referencing
    /// <c>System.Drawing.Drawing2D.DashStyle</c> compiles and round-trips correctly.
    /// </summary>
    public new System.Drawing.Drawing2D.DashStyle DashStyle
    {
        get => (System.Drawing.Drawing2D.DashStyle)base.DashStyle;
        set => base.DashStyle = (Canvas.Windows.Forms.Drawing.DashStyle)value;
    }

    /// <summary>Gets or sets the dash cap style (stub — no effect on canvas rendering).</summary>
    public System.Drawing.Drawing2D.DashCap DashCap { get; set; } = System.Drawing.Drawing2D.DashCap.Flat;
}

/// <summary>
/// Stub for <c>System.Drawing.TextureBrush</c>.
/// Texture-fill via image is not supported on canvas; stub satisfies compilation.
/// </summary>
public sealed class TextureBrush : Brush
{
    public TextureBrush(Image image) { }
    public TextureBrush(Image image, Drawing2D.WrapMode wrapMode) { }
}

/// <summary>
/// Stub for <c>System.Drawing.LinearGradientBrush</c> (System.Drawing.Drawing2D).
/// Gradient rendering is not yet implemented; stub satisfies compilation.
/// </summary>
public sealed class HatchBrush : Brush
{
    public HatchBrush(Drawing2D.HatchStyle hatchStyle, Color foreColor) { }
    public HatchBrush(Drawing2D.HatchStyle hatchStyle, Color foreColor, Color backColor) { }
}
