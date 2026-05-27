namespace System.Windows.Forms;

/// <summary>
/// Provides methods for measuring and rendering text using device context rendering.
/// In CanvasForms actual text drawing goes through the canvas pipeline;
/// this class provides the API surface so translated apps compile without modification.
/// Types use <c>Canvas.Windows.Forms.Drawing</c> via global usings.
/// </summary>
public static class TextRenderer
{
    // ── Drawing ───────────────────────────────────────────────────────────────

    /// <summary>Draws <paramref name="text"/> at <paramref name="pt"/> with the given font and color.</summary>
    public static void DrawText(Graphics dc, string? text, Font? font, Point pt,
        System.Drawing.Color foreColor)
        => dc?.DrawString(text ?? string.Empty, font ?? SystemFonts.DefaultFont,
            new SolidBrush((CanvasColor)foreColor), pt.X, pt.Y);

    /// <summary>Draws <paramref name="text"/> clipped to <paramref name="bounds"/>.</summary>
    public static void DrawText(Graphics dc, string? text, Font? font, Rectangle bounds,
        System.Drawing.Color foreColor)
        => dc?.DrawString(text ?? string.Empty, font ?? SystemFonts.DefaultFont,
            new SolidBrush((CanvasColor)foreColor), bounds.X, bounds.Y);

    /// <summary>Draws with formatting flags.</summary>
    public static void DrawText(Graphics dc, string? text, Font? font, Rectangle bounds,
        System.Drawing.Color foreColor, TextFormatFlags flags)
        => DrawText(dc, text, font, bounds, foreColor);

    /// <summary>Draws with foreground and background colors.</summary>
    public static void DrawText(Graphics dc, string? text, Font? font, Rectangle bounds,
        System.Drawing.Color foreColor, System.Drawing.Color backColor)
        => DrawText(dc, text, font, bounds, foreColor);

    /// <summary>Draws with foreground color, background color, and formatting flags.</summary>
    public static void DrawText(Graphics dc, string? text, Font? font, Rectangle bounds,
        System.Drawing.Color foreColor, System.Drawing.Color backColor, TextFormatFlags flags)
        => DrawText(dc, text, font, bounds, foreColor);

    // ── Measuring ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the size of the specified text when drawn with the specified font.
    /// Uses a heuristic since there is no GDI+ measurement available in the browser.
    /// </summary>
    public static Size MeasureText(string? text, Font? font)
    {
        if (string.IsNullOrEmpty(text)) return Size.Empty;
        float size = font?.Size ?? 9f;
        int w = (int)(text.Length * size * 0.6f) + 4;
        int h = (int)(size * 1.3f) + 4;
        return new Size(w, h);
    }

    public static Size MeasureText(string? text, Font? font, Size proposedSize)
        => MeasureText(text, font);

    public static Size MeasureText(string? text, Font? font, Size proposedSize, TextFormatFlags flags)
        => MeasureText(text, font);

    public static Size MeasureText(Graphics dc, string? text, Font? font)
        => MeasureText(text, font);

    public static Size MeasureText(Graphics dc, string? text, Font? font, Size proposedSize)
        => MeasureText(text, font);

    public static Size MeasureText(Graphics dc, string? text, Font? font, Size proposedSize, TextFormatFlags flags)
        => MeasureText(text, font);
}

/// <summary>
/// Specifies the display and layout information for text strings.
/// </summary>
[Flags]
public enum TextFormatFlags
{
    Default                     = 0x00000000,
    Bottom                      = 0x00000008,
    EndEllipsis                 = 0x00008000,
    ExpandTabs                  = 0x00000040,
    ExternalLeading             = 0x00000200,
    GlyphOverhangPadding        = 0x00000000,
    HidePrefix                  = 0x00100000,
    HorizontalCenter            = 0x00000001,
    Internal                    = 0x00001000,
    Left                        = 0x00000000,
    ModifyString                = 0x00010000,
    NoClipping                  = 0x00000100,
    NoFullWidthCharacterBreak   = 0x00080000,
    NoPrefix                    = 0x00000800,
    PathEllipsis                = 0x00004000,
    PrefixOnly                  = 0x00200000,
    PreserveGraphicsClipping    = 0x01000000,
    PreserveGraphicsTranslateTransform = 0x02000000,
    Right                       = 0x00000002,
    RightToLeft                 = 0x00020000,
    SingleLine                  = 0x00000020,
    TextBoxControl              = 0x00002000,
    Top                         = 0x00000000,
    VerticalCenter              = 0x00000004,
    WordBreak                   = 0x00000010,
    WordEllipsis                = 0x00040000,
}
