namespace WebForms.Canvas.Drawing;

public class Font
{
    public string Family { get; set; }
    public float Size { get; set; }
    public FontStyle Style { get; set; }

    public Font(string family, float size, FontStyle style = FontStyle.Regular)
    {
        Family = family;
        Size = size;
        Style = style;
    }

    public string ToCssString()
    {
        var styleStr = Style switch
        {
            FontStyle.Bold => "bold ",
            FontStyle.Italic => "italic ",
            FontStyle.BoldItalic => "bold italic ",
            _ => ""
        };
        return $"{styleStr}{Size}px {Family}";
    }
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
