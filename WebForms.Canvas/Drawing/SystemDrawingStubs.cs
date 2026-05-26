// Stubs for System.Drawing types that live in System.Drawing.Common (Windows-only GDI+).
// These allow translated WinForms apps to bind at runtime without
// referencing the Windows-only package.

namespace System.Drawing;

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
    public void SetMeasurableCharacterRanges(CharacterRange[] ranges) { }
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
