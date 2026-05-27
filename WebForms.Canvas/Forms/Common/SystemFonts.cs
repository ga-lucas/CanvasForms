namespace System.Windows.Forms;

/// <summary>
/// Provides system-defined <see cref="Font"/> objects used by UI elements.
/// In CanvasForms these return fixed Font instances that approximate Windows defaults.
/// Font here is <c>Canvas.Windows.Forms.Drawing.Font</c> (available via global usings).
/// </summary>
public static class SystemFonts
{
    private static Font Make(string name, float size, FontStyle style = FontStyle.Regular)
        => new Font(name, size, style);

    public static Font DefaultFont      => Make("Segoe UI", 9f);
    public static Font CaptionFont      => Make("Segoe UI", 9f);
    public static Font IconTitleFont    => Make("Segoe UI", 9f);
    public static Font MenuFont         => Make("Segoe UI", 9f);
    public static Font MessageBoxFont   => Make("Segoe UI", 9f);
    public static Font SmallCaptionFont => Make("Segoe UI", 8f);
    public static Font StatusFont       => Make("Segoe UI", 9f);
    public static Font ToolTipFont      => Make("Segoe UI", 9f);

    public static Font? GetFontByName(string systemFontName) => systemFontName switch
    {
        "CaptionFont"      => CaptionFont,
        "DefaultFont"      => DefaultFont,
        "IconTitleFont"    => IconTitleFont,
        "MenuFont"         => MenuFont,
        "MessageBoxFont"   => MessageBoxFont,
        "SmallCaptionFont" => SmallCaptionFont,
        "StatusFont"       => StatusFont,
        _                  => null
    };
}
