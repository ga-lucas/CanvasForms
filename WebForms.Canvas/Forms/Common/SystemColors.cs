namespace System.Windows.Forms;

/// <summary>
/// Provides system-defined colors for UI elements.
/// In CanvasForms these return fixed RGB values that approximate the default
/// Windows visual theme.  They are provided as <c>System.Drawing.Color</c> for
/// full WinForms API compatibility.
/// </summary>
public static class SystemColors
{
    private static System.Drawing.Color C(byte r, byte g, byte b)
        => System.Drawing.Color.FromArgb(r, g, b);

    public static System.Drawing.Color ActiveBorder           => C(0xB4, 0xB4, 0xB4);
    public static System.Drawing.Color ActiveCaption          => C(0x99, 0xB4, 0xD1);
    public static System.Drawing.Color ActiveCaptionText      => C(0x00, 0x00, 0x00);
    public static System.Drawing.Color AppWorkspace           => C(0xAB, 0xAB, 0xAB);
    public static System.Drawing.Color ButtonFace             => C(0xF0, 0xF0, 0xF0);
    public static System.Drawing.Color ButtonHighlight        => C(0xFF, 0xFF, 0xFF);
    public static System.Drawing.Color ButtonShadow           => C(0xA0, 0xA0, 0xA0);
    public static System.Drawing.Color Control                => C(0xF0, 0xF0, 0xF0);
    public static System.Drawing.Color ControlDark            => C(0xA0, 0xA0, 0xA0);
    public static System.Drawing.Color ControlDarkDark        => C(0x69, 0x69, 0x69);
    public static System.Drawing.Color ControlLight           => C(0xE3, 0xE3, 0xE3);
    public static System.Drawing.Color ControlLightLight      => C(0xFF, 0xFF, 0xFF);
    public static System.Drawing.Color ControlText            => C(0x00, 0x00, 0x00);
    public static System.Drawing.Color Desktop                => C(0x00, 0x00, 0x00);
    public static System.Drawing.Color GradientActiveCaption  => C(0xB9, 0xD1, 0xEA);
    public static System.Drawing.Color GradientInactiveCaption => C(0xD7, 0xE4, 0xF2);
    public static System.Drawing.Color GrayText               => C(0x6D, 0x6D, 0x6D);
    public static System.Drawing.Color Highlight              => C(0x00, 0x78, 0xD7);
    public static System.Drawing.Color HighlightText          => C(0xFF, 0xFF, 0xFF);
    public static System.Drawing.Color HotTrack               => C(0x00, 0x66, 0xCC);
    public static System.Drawing.Color InactiveBorder         => C(0xF4, 0xF7, 0xFC);
    public static System.Drawing.Color InactiveCaption        => C(0xBF, 0xCD, 0xDB);
    public static System.Drawing.Color InactiveCaptionText    => C(0x43, 0x4E, 0x54);
    public static System.Drawing.Color Info                   => C(0xFF, 0xFF, 0xE1);
    public static System.Drawing.Color InfoText               => C(0x00, 0x00, 0x00);
    public static System.Drawing.Color Menu                   => C(0xF0, 0xF0, 0xF0);
    public static System.Drawing.Color MenuBar                => C(0xF0, 0xF0, 0xF0);
    public static System.Drawing.Color MenuHighlight          => C(0x00, 0x78, 0xD7);
    public static System.Drawing.Color MenuText               => C(0x00, 0x00, 0x00);
    public static System.Drawing.Color ScrollBar              => C(0xC8, 0xC8, 0xC8);
    public static System.Drawing.Color Window                 => C(0xFF, 0xFF, 0xFF);
    public static System.Drawing.Color WindowFrame            => C(0x64, 0x64, 0x64);
    public static System.Drawing.Color WindowText             => C(0x00, 0x00, 0x00);
}
