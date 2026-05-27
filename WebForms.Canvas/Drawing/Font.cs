namespace Canvas.Windows.Forms.Drawing;

public class Font : IDisposable
{
    public string Family { get; set; }
    public float Size { get; set; }
    public FontStyle Style { get; set; }

    // Convenience flag properties (WinForms API)
    public bool Bold      => (Style & FontStyle.Bold)      != 0;
    public bool Italic    => (Style & FontStyle.Italic)    != 0;
    public bool Underline => (Style & FontStyle.Underline) != 0;
    public bool Strikeout => (Style & FontStyle.Strikeout) != 0;

    // Aliases
    public string Name => Family;
    public float  SizeInPoints => Size;

    // Unit stub — always Points in the canvas renderer
    public System.Drawing.GraphicsUnit Unit => System.Drawing.GraphicsUnit.Point;

    // GDI compatibility stubs
    public byte GdiCharSet    { get; set; } = 1; // ANSI
    public bool GdiVerticalFont { get; set; } = false;

    // FontFamily wrapper
    public System.Drawing.FontFamily FontFamily => new System.Drawing.FontFamily(Family);

    // Line height used for multiline layout. Canvas renders with textBaseline='top'
    // and fontSize=Size px, so glyphs fit within Size pixels. We add 2px inter-line
    // spacing so consecutive lines don't touch, matching typical browser line-height.
    public int Height => (int)Size + 2;

    public Font(string family, float size, FontStyle style = FontStyle.Regular)
    {
        Family = family;
        Size = size;
        Style = style;
    }

    public Font(string family, float size, FontStyle style, System.Drawing.GraphicsUnit unit)
    {
        Family = family;
        Size = size;
        Style = style;
    }

    public Font(System.Drawing.FontFamily family, float size, FontStyle style = FontStyle.Regular)
    {
        Family = family.Name;
        Size = size;
        Style = style;
    }

    public Font(Font prototype, FontStyle newStyle)
    {
        Family = prototype.Family;
        Size = prototype.Size;
        Style = newStyle;
    }

    public Font Clone() => new Font(Family, Size, Style);

    public string ToCssString()
    {
        var parts = new System.Text.StringBuilder();
        if (Bold)      parts.Append("bold ");
        if (Italic)    parts.Append("italic ");
        return $"{parts}{Size}px {Family}";
    }

    public void Dispose() { }
    public override string ToString() => $"[Font: {Family}, {Size}pt, style={Style}]";
}

[Flags]
public enum FontStyle
{
    Regular = 0,
    Bold = 1,
    Italic = 2,
    BoldItalic = Bold | Italic,
    Underline = 4,
    Strikeout = 8
}
