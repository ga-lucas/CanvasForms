// Stubs for System.Drawing.SystemPens and System.Drawing.SystemBrushes.
// These return canvas-layer Pen / SolidBrush instances built from the same
// system-color palette used by SystemColors.

namespace System.Drawing;

using CanvasColor = Canvas.Windows.Forms.Drawing.Color;
using CanvasBrush = Canvas.Windows.Forms.Drawing.SolidBrush;
using CanvasPen   = Canvas.Windows.Forms.Drawing.Pen;

/// <summary>
/// Provides <see cref="Pen"/> objects for the standard system colours.
/// Each property returns a new pen; callers should dispose it when done.
/// </summary>
public static class SystemPens
{
    private static CanvasPen P(byte r, byte g, byte b) => new CanvasPen(CanvasColor.FromArgb(r, g, b));

    public static CanvasPen ActiveBorder        => P(180, 180, 180);
    public static CanvasPen ActiveCaption       => P(153, 180, 209);
    public static CanvasPen ActiveCaptionText   => P(  0,   0,   0);
    public static CanvasPen AppWorkspace        => P(171, 171, 171);
    public static CanvasPen ButtonFace          => P(240, 240, 240);
    public static CanvasPen ButtonHighlight     => P(255, 255, 255);
    public static CanvasPen ButtonShadow        => P(160, 160, 160);
    public static CanvasPen Control             => P(240, 240, 240);
    public static CanvasPen ControlDark         => P(160, 160, 160);
    public static CanvasPen ControlDarkDark     => P(105, 105, 105);
    public static CanvasPen ControlLight        => P(227, 227, 227);
    public static CanvasPen ControlLightLight   => P(255, 255, 255);
    public static CanvasPen ControlText         => P(  0,   0,   0);
    public static CanvasPen Desktop             => P(  0,   0,   0);
    public static CanvasPen GradientActiveCaption  => P(185, 209, 234);
    public static CanvasPen GradientInactiveCaption => P(215, 228, 242);
    public static CanvasPen GrayText            => P(109, 109, 109);
    public static CanvasPen Highlight           => P(  0, 120, 215);
    public static CanvasPen HighlightText       => P(255, 255, 255);
    public static CanvasPen HotTrack            => P(  0, 102, 204);
    public static CanvasPen InactiveBorder      => P(244, 247, 252);
    public static CanvasPen InactiveCaption     => P(191, 205, 219);
    public static CanvasPen InactiveCaptionText => P(  0,   0,   0);
    public static CanvasPen Info                => P(255, 255, 225);
    public static CanvasPen InfoText            => P(  0,   0,   0);
    public static CanvasPen Menu                => P(240, 240, 240);
    public static CanvasPen MenuBar             => P(240, 240, 240);
    public static CanvasPen MenuHighlight       => P(  0, 120, 215);
    public static CanvasPen MenuText            => P(  0,   0,   0);
    public static CanvasPen ScrollBar           => P(200, 200, 200);
    public static CanvasPen Window              => P(255, 255, 255);
    public static CanvasPen WindowFrame         => P(100, 100, 100);
    public static CanvasPen WindowText          => P(  0,   0,   0);
}

/// <summary>
/// Provides <see cref="SolidBrush"/> objects for the standard system colours.
/// Each property returns a new brush; callers should dispose it when done.
/// </summary>
public static class SystemBrushes
{
    private static CanvasBrush B(byte r, byte g, byte b) => new CanvasBrush(CanvasColor.FromArgb(r, g, b));

    public static CanvasBrush ActiveBorder        => B(180, 180, 180);
    public static CanvasBrush ActiveCaption       => B(153, 180, 209);
    public static CanvasBrush ActiveCaptionText   => B(  0,   0,   0);
    public static CanvasBrush AppWorkspace        => B(171, 171, 171);
    public static CanvasBrush ButtonFace          => B(240, 240, 240);
    public static CanvasBrush ButtonHighlight     => B(255, 255, 255);
    public static CanvasBrush ButtonShadow        => B(160, 160, 160);
    public static CanvasBrush Control             => B(240, 240, 240);
    public static CanvasBrush ControlDark         => B(160, 160, 160);
    public static CanvasBrush ControlDarkDark     => B(105, 105, 105);
    public static CanvasBrush ControlLight        => B(227, 227, 227);
    public static CanvasBrush ControlLightLight   => B(255, 255, 255);
    public static CanvasBrush ControlText         => B(  0,   0,   0);
    public static CanvasBrush Desktop             => B(  0,   0,   0);
    public static CanvasBrush GradientActiveCaption  => B(185, 209, 234);
    public static CanvasBrush GradientInactiveCaption => B(215, 228, 242);
    public static CanvasBrush GrayText            => B(109, 109, 109);
    public static CanvasBrush Highlight           => B(  0, 120, 215);
    public static CanvasBrush HighlightText       => B(255, 255, 255);
    public static CanvasBrush HotTrack            => B(  0, 102, 204);
    public static CanvasBrush InactiveBorder      => B(244, 247, 252);
    public static CanvasBrush InactiveCaption     => B(191, 205, 219);
    public static CanvasBrush InactiveCaptionText => B(  0,   0,   0);
    public static CanvasBrush Info                => B(255, 255, 225);
    public static CanvasBrush InfoText            => B(  0,   0,   0);
    public static CanvasBrush Menu                => B(240, 240, 240);
    public static CanvasBrush MenuBar             => B(240, 240, 240);
    public static CanvasBrush MenuHighlight       => B(  0, 120, 215);
    public static CanvasBrush MenuText            => B(  0,   0,   0);
    public static CanvasBrush ScrollBar           => B(200, 200, 200);
    public static CanvasBrush Window              => B(255, 255, 255);
    public static CanvasBrush WindowFrame         => B(100, 100, 100);
    public static CanvasBrush WindowText          => B(  0,   0,   0);
}

/// <summary>
/// Provides <see cref="CanvasPen"/> objects for all named GDI+ colors,
/// matching the <c>System.Drawing.Pens</c> static class in real WinForms.
/// </summary>
public static class Pens
{
    private static CanvasPen P(byte r, byte g, byte b) => new CanvasPen(CanvasColor.FromArgb(r, g, b));

    public static CanvasPen AliceBlue            => P(240, 248, 255);
    public static CanvasPen AntiqueWhite         => P(250, 235, 215);
    public static CanvasPen Aqua                 => P(  0, 255, 255);
    public static CanvasPen Aquamarine           => P(127, 255, 212);
    public static CanvasPen Azure                => P(240, 255, 255);
    public static CanvasPen Beige                => P(245, 245, 220);
    public static CanvasPen Bisque               => P(255, 228, 196);
    public static CanvasPen Black                => P(  0,   0,   0);
    public static CanvasPen BlanchedAlmond       => P(255, 235, 205);
    public static CanvasPen Blue                 => P(  0,   0, 255);
    public static CanvasPen BlueViolet           => P(138,  43, 226);
    public static CanvasPen Brown                => P(165,  42,  42);
    public static CanvasPen BurlyWood            => P(222, 184, 135);
    public static CanvasPen CadetBlue            => P( 95, 158, 160);
    public static CanvasPen Chartreuse           => P(127, 255,   0);
    public static CanvasPen Chocolate            => P(210, 105,  30);
    public static CanvasPen Coral                => P(255, 127,  80);
    public static CanvasPen CornflowerBlue       => P(100, 149, 237);
    public static CanvasPen Cornsilk             => P(255, 248, 220);
    public static CanvasPen Crimson              => P(220,  20,  60);
    public static CanvasPen Cyan                 => P(  0, 255, 255);
    public static CanvasPen DarkBlue             => P(  0,   0, 139);
    public static CanvasPen DarkCyan             => P(  0, 139, 139);
    public static CanvasPen DarkGoldenrod        => P(184, 134,  11);
    public static CanvasPen DarkGray             => P(169, 169, 169);
    public static CanvasPen DarkGreen            => P(  0, 100,   0);
    public static CanvasPen DarkKhaki            => P(189, 183, 107);
    public static CanvasPen DarkMagenta          => P(139,   0, 139);
    public static CanvasPen DarkOliveGreen       => P( 85, 107,  47);
    public static CanvasPen DarkOrange           => P(255, 140,   0);
    public static CanvasPen DarkOrchid           => P(153,  50, 204);
    public static CanvasPen DarkRed              => P(139,   0,   0);
    public static CanvasPen DarkSalmon           => P(233, 150, 122);
    public static CanvasPen DarkSeaGreen         => P(143, 188, 143);
    public static CanvasPen DarkSlateBlue        => P( 72,  61, 139);
    public static CanvasPen DarkSlateGray        => P( 47,  79,  79);
    public static CanvasPen DarkTurquoise        => P(  0, 206, 209);
    public static CanvasPen DarkViolet           => P(148,   0, 211);
    public static CanvasPen DeepPink             => P(255,  20, 147);
    public static CanvasPen DeepSkyBlue          => P(  0, 191, 255);
    public static CanvasPen DimGray              => P(105, 105, 105);
    public static CanvasPen DodgerBlue           => P( 30, 144, 255);
    public static CanvasPen Firebrick            => P(178,  34,  34);
    public static CanvasPen FloralWhite          => P(255, 250, 240);
    public static CanvasPen ForestGreen          => P( 34, 139,  34);
    public static CanvasPen Fuchsia              => P(255,   0, 255);
    public static CanvasPen Gainsboro            => P(220, 220, 220);
    public static CanvasPen GhostWhite           => P(248, 248, 255);
    public static CanvasPen Gold                 => P(255, 215,   0);
    public static CanvasPen Goldenrod            => P(218, 165,  32);
    public static CanvasPen Gray                 => P(128, 128, 128);
    public static CanvasPen Green                => P(  0, 128,   0);
    public static CanvasPen GreenYellow          => P(173, 255,  47);
    public static CanvasPen Honeydew             => P(240, 255, 240);
    public static CanvasPen HotPink              => P(255, 105, 180);
    public static CanvasPen IndianRed            => P(205,  92,  92);
    public static CanvasPen Indigo               => P( 75,   0, 130);
    public static CanvasPen Ivory                => P(255, 255, 240);
    public static CanvasPen Khaki                => P(240, 230, 140);
    public static CanvasPen Lavender             => P(230, 230, 250);
    public static CanvasPen LavenderBlush        => P(255, 240, 245);
    public static CanvasPen LawnGreen            => P(124, 252,   0);
    public static CanvasPen LemonChiffon         => P(255, 250, 205);
    public static CanvasPen LightBlue            => P(173, 216, 230);
    public static CanvasPen LightCoral           => P(240, 128, 128);
    public static CanvasPen LightCyan            => P(224, 255, 255);
    public static CanvasPen LightGoldenrodYellow => P(250, 250, 210);
    public static CanvasPen LightGray            => P(211, 211, 211);
    public static CanvasPen LightGreen           => P(144, 238, 144);
    public static CanvasPen LightPink            => P(255, 182, 193);
    public static CanvasPen LightSalmon          => P(255, 160, 122);
    public static CanvasPen LightSeaGreen        => P( 32, 178, 170);
    public static CanvasPen LightSkyBlue         => P(135, 206, 250);
    public static CanvasPen LightSlateGray       => P(119, 136, 153);
    public static CanvasPen LightSteelBlue       => P(176, 196, 222);
    public static CanvasPen LightYellow          => P(255, 255, 224);
    public static CanvasPen Lime                 => P(  0, 255,   0);
    public static CanvasPen LimeGreen            => P( 50, 205,  50);
    public static CanvasPen Linen                => P(250, 240, 230);
    public static CanvasPen Magenta              => P(255,   0, 255);
    public static CanvasPen Maroon               => P(128,   0,   0);
    public static CanvasPen MediumAquamarine     => P(102, 205, 170);
    public static CanvasPen MediumBlue           => P(  0,   0, 205);
    public static CanvasPen MediumOrchid         => P(186,  85, 211);
    public static CanvasPen MediumPurple         => P(147, 112, 219);
    public static CanvasPen MediumSeaGreen       => P( 60, 179, 113);
    public static CanvasPen MediumSlateBlue      => P(123, 104, 238);
    public static CanvasPen MediumSpringGreen    => P(  0, 250, 154);
    public static CanvasPen MediumTurquoise      => P( 72, 209, 204);
    public static CanvasPen MediumVioletRed      => P(199,  21, 133);
    public static CanvasPen MidnightBlue         => P( 25,  25, 112);
    public static CanvasPen MintCream            => P(245, 255, 250);
    public static CanvasPen MistyRose            => P(255, 228, 225);
    public static CanvasPen Moccasin             => P(255, 228, 181);
    public static CanvasPen NavajoWhite          => P(255, 222, 173);
    public static CanvasPen Navy                 => P(  0,   0, 128);
    public static CanvasPen OldLace              => P(253, 245, 230);
    public static CanvasPen Olive                => P(128, 128,   0);
    public static CanvasPen OliveDrab            => P(107, 142,  35);
    public static CanvasPen Orange               => P(255, 165,   0);
    public static CanvasPen OrangeRed            => P(255,  69,   0);
    public static CanvasPen Orchid               => P(218, 112, 214);
    public static CanvasPen PaleGoldenrod        => P(238, 232, 170);
    public static CanvasPen PaleGreen            => P(152, 251, 152);
    public static CanvasPen PaleTurquoise        => P(175, 238, 238);
    public static CanvasPen PaleVioletRed        => P(219, 112, 147);
    public static CanvasPen PapayaWhip           => P(255, 239, 213);
    public static CanvasPen PeachPuff            => P(255, 218, 185);
    public static CanvasPen Peru                 => P(205, 133,  63);
    public static CanvasPen Pink                 => P(255, 192, 203);
    public static CanvasPen Plum                 => P(221, 160, 221);
    public static CanvasPen PowderBlue           => P(176, 224, 230);
    public static CanvasPen Purple               => P(128,   0, 128);
    public static CanvasPen Red                  => P(255,   0,   0);
    public static CanvasPen RosyBrown            => P(188, 143, 143);
    public static CanvasPen RoyalBlue            => P( 65, 105, 225);
    public static CanvasPen SaddleBrown          => P(139,  69,  19);
    public static CanvasPen Salmon               => P(250, 128, 114);
    public static CanvasPen SandyBrown           => P(244, 164,  96);
    public static CanvasPen SeaGreen             => P( 46, 139,  87);
    public static CanvasPen SeaShell             => P(255, 245, 238);
    public static CanvasPen Sienna               => P(160,  82,  45);
    public static CanvasPen Silver               => P(192, 192, 192);
    public static CanvasPen SkyBlue              => P(135, 206, 235);
    public static CanvasPen SlateBlue            => P(106,  90, 205);
    public static CanvasPen SlateGray            => P(112, 128, 144);
    public static CanvasPen Snow                 => P(255, 250, 250);
    public static CanvasPen SpringGreen          => P(  0, 255, 127);
    public static CanvasPen SteelBlue            => P( 70, 130, 180);
    public static CanvasPen Tan                  => P(210, 180, 140);
    public static CanvasPen Teal                 => P(  0, 128, 128);
    public static CanvasPen Thistle              => P(216, 191, 216);
    public static CanvasPen Tomato               => P(255,  99,  71);
    public static CanvasPen Transparent          => new CanvasPen(CanvasColor.Transparent);
    public static CanvasPen Turquoise            => P( 64, 224, 208);
    public static CanvasPen Violet               => P(238, 130, 238);
    public static CanvasPen Wheat                => P(245, 222, 179);
    public static CanvasPen White                => P(255, 255, 255);
    public static CanvasPen WhiteSmoke           => P(245, 245, 245);
    public static CanvasPen Yellow               => P(255, 255,   0);
    public static CanvasPen YellowGreen          => P(154, 205,  50);
}

/// <summary>
/// Provides <see cref="CanvasBrush"/> objects for all named GDI+ colors,
/// matching the <c>System.Drawing.Brushes</c> static class in real WinForms.
/// </summary>
public static class Brushes
{
    private static CanvasBrush B(byte r, byte g, byte b) => new CanvasBrush(CanvasColor.FromArgb(r, g, b));

    public static CanvasBrush AliceBlue            => B(240, 248, 255);
    public static CanvasBrush AntiqueWhite         => B(250, 235, 215);
    public static CanvasBrush Aqua                 => B(  0, 255, 255);
    public static CanvasBrush Aquamarine           => B(127, 255, 212);
    public static CanvasBrush Azure                => B(240, 255, 255);
    public static CanvasBrush Beige                => B(245, 245, 220);
    public static CanvasBrush Bisque               => B(255, 228, 196);
    public static CanvasBrush Black                => B(  0,   0,   0);
    public static CanvasBrush BlanchedAlmond       => B(255, 235, 205);
    public static CanvasBrush Blue                 => B(  0,   0, 255);
    public static CanvasBrush BlueViolet           => B(138,  43, 226);
    public static CanvasBrush Brown                => B(165,  42,  42);
    public static CanvasBrush BurlyWood            => B(222, 184, 135);
    public static CanvasBrush CadetBlue            => B( 95, 158, 160);
    public static CanvasBrush Chartreuse           => B(127, 255,   0);
    public static CanvasBrush Chocolate            => B(210, 105,  30);
    public static CanvasBrush Coral                => B(255, 127,  80);
    public static CanvasBrush CornflowerBlue       => B(100, 149, 237);
    public static CanvasBrush Cornsilk             => B(255, 248, 220);
    public static CanvasBrush Crimson              => B(220,  20,  60);
    public static CanvasBrush Cyan                 => B(  0, 255, 255);
    public static CanvasBrush DarkBlue             => B(  0,   0, 139);
    public static CanvasBrush DarkCyan             => B(  0, 139, 139);
    public static CanvasBrush DarkGoldenrod        => B(184, 134,  11);
    public static CanvasBrush DarkGray             => B(169, 169, 169);
    public static CanvasBrush DarkGreen            => B(  0, 100,   0);
    public static CanvasBrush DarkKhaki            => B(189, 183, 107);
    public static CanvasBrush DarkMagenta          => B(139,   0, 139);
    public static CanvasBrush DarkOliveGreen       => B( 85, 107,  47);
    public static CanvasBrush DarkOrange           => B(255, 140,   0);
    public static CanvasBrush DarkOrchid           => B(153,  50, 204);
    public static CanvasBrush DarkRed              => B(139,   0,   0);
    public static CanvasBrush DarkSalmon           => B(233, 150, 122);
    public static CanvasBrush DarkSeaGreen         => B(143, 188, 143);
    public static CanvasBrush DarkSlateBlue        => B( 72,  61, 139);
    public static CanvasBrush DarkSlateGray        => B( 47,  79,  79);
    public static CanvasBrush DarkTurquoise        => B(  0, 206, 209);
    public static CanvasBrush DarkViolet           => B(148,   0, 211);
    public static CanvasBrush DeepPink             => B(255,  20, 147);
    public static CanvasBrush DeepSkyBlue          => B(  0, 191, 255);
    public static CanvasBrush DimGray              => B(105, 105, 105);
    public static CanvasBrush DodgerBlue           => B( 30, 144, 255);
    public static CanvasBrush Firebrick            => B(178,  34,  34);
    public static CanvasBrush FloralWhite          => B(255, 250, 240);
    public static CanvasBrush ForestGreen          => B( 34, 139,  34);
    public static CanvasBrush Fuchsia              => B(255,   0, 255);
    public static CanvasBrush Gainsboro            => B(220, 220, 220);
    public static CanvasBrush GhostWhite           => B(248, 248, 255);
    public static CanvasBrush Gold                 => B(255, 215,   0);
    public static CanvasBrush Goldenrod            => B(218, 165,  32);
    public static CanvasBrush Gray                 => B(128, 128, 128);
    public static CanvasBrush Green                => B(  0, 128,   0);
    public static CanvasBrush GreenYellow          => B(173, 255,  47);
    public static CanvasBrush Honeydew             => B(240, 255, 240);
    public static CanvasBrush HotPink              => B(255, 105, 180);
    public static CanvasBrush IndianRed            => B(205,  92,  92);
    public static CanvasBrush Indigo               => B( 75,   0, 130);
    public static CanvasBrush Ivory                => B(255, 255, 240);
    public static CanvasBrush Khaki                => B(240, 230, 140);
    public static CanvasBrush Lavender             => B(230, 230, 250);
    public static CanvasBrush LavenderBlush        => B(255, 240, 245);
    public static CanvasBrush LawnGreen            => B(124, 252,   0);
    public static CanvasBrush LemonChiffon         => B(255, 250, 205);
    public static CanvasBrush LightBlue            => B(173, 216, 230);
    public static CanvasBrush LightCoral           => B(240, 128, 128);
    public static CanvasBrush LightCyan            => B(224, 255, 255);
    public static CanvasBrush LightGoldenrodYellow => B(250, 250, 210);
    public static CanvasBrush LightGray            => B(211, 211, 211);
    public static CanvasBrush LightGreen           => B(144, 238, 144);
    public static CanvasBrush LightPink            => B(255, 182, 193);
    public static CanvasBrush LightSalmon          => B(255, 160, 122);
    public static CanvasBrush LightSeaGreen        => B( 32, 178, 170);
    public static CanvasBrush LightSkyBlue         => B(135, 206, 250);
    public static CanvasBrush LightSlateGray       => B(119, 136, 153);
    public static CanvasBrush LightSteelBlue       => B(176, 196, 222);
    public static CanvasBrush LightYellow          => B(255, 255, 224);
    public static CanvasBrush Lime                 => B(  0, 255,   0);
    public static CanvasBrush LimeGreen            => B( 50, 205,  50);
    public static CanvasBrush Linen                => B(250, 240, 230);
    public static CanvasBrush Magenta              => B(255,   0, 255);
    public static CanvasBrush Maroon               => B(128,   0,   0);
    public static CanvasBrush MediumAquamarine     => B(102, 205, 170);
    public static CanvasBrush MediumBlue           => B(  0,   0, 205);
    public static CanvasBrush MediumOrchid         => B(186,  85, 211);
    public static CanvasBrush MediumPurple         => B(147, 112, 219);
    public static CanvasBrush MediumSeaGreen       => B( 60, 179, 113);
    public static CanvasBrush MediumSlateBlue      => B(123, 104, 238);
    public static CanvasBrush MediumSpringGreen    => B(  0, 250, 154);
    public static CanvasBrush MediumTurquoise      => B( 72, 209, 204);
    public static CanvasBrush MediumVioletRed      => B(199,  21, 133);
    public static CanvasBrush MidnightBlue         => B( 25,  25, 112);
    public static CanvasBrush MintCream            => B(245, 255, 250);
    public static CanvasBrush MistyRose            => B(255, 228, 225);
    public static CanvasBrush Moccasin             => B(255, 228, 181);
    public static CanvasBrush NavajoWhite          => B(255, 222, 173);
    public static CanvasBrush Navy                 => B(  0,   0, 128);
    public static CanvasBrush OldLace              => B(253, 245, 230);
    public static CanvasBrush Olive                => B(128, 128,   0);
    public static CanvasBrush OliveDrab            => B(107, 142,  35);
    public static CanvasBrush Orange               => B(255, 165,   0);
    public static CanvasBrush OrangeRed            => B(255,  69,   0);
    public static CanvasBrush Orchid               => B(218, 112, 214);
    public static CanvasBrush PaleGoldenrod        => B(238, 232, 170);
    public static CanvasBrush PaleGreen            => B(152, 251, 152);
    public static CanvasBrush PaleTurquoise        => B(175, 238, 238);
    public static CanvasBrush PaleVioletRed        => B(219, 112, 147);
    public static CanvasBrush PapayaWhip           => B(255, 239, 213);
    public static CanvasBrush PeachPuff            => B(255, 218, 185);
    public static CanvasBrush Peru                 => B(205, 133,  63);
    public static CanvasBrush Pink                 => B(255, 192, 203);
    public static CanvasBrush Plum                 => B(221, 160, 221);
    public static CanvasBrush PowderBlue           => B(176, 224, 230);
    public static CanvasBrush Purple               => B(128,   0, 128);
    public static CanvasBrush Red                  => B(255,   0,   0);
    public static CanvasBrush RosyBrown            => B(188, 143, 143);
    public static CanvasBrush RoyalBlue            => B( 65, 105, 225);
    public static CanvasBrush SaddleBrown          => B(139,  69,  19);
    public static CanvasBrush Salmon               => B(250, 128, 114);
    public static CanvasBrush SandyBrown           => B(244, 164,  96);
    public static CanvasBrush SeaGreen             => B( 46, 139,  87);
    public static CanvasBrush SeaShell             => B(255, 245, 238);
    public static CanvasBrush Sienna               => B(160,  82,  45);
    public static CanvasBrush Silver               => B(192, 192, 192);
    public static CanvasBrush SkyBlue              => B(135, 206, 235);
    public static CanvasBrush SlateBlue            => B(106,  90, 205);
    public static CanvasBrush SlateGray            => B(112, 128, 144);
    public static CanvasBrush Snow                 => B(255, 250, 250);
    public static CanvasBrush SpringGreen          => B(  0, 255, 127);
    public static CanvasBrush SteelBlue            => B( 70, 130, 180);
    public static CanvasBrush Tan                  => B(210, 180, 140);
    public static CanvasBrush Teal                 => B(  0, 128, 128);
    public static CanvasBrush Thistle              => B(216, 191, 216);
    public static CanvasBrush Tomato               => B(255,  99,  71);
    public static CanvasBrush Transparent          => new CanvasBrush(CanvasColor.Transparent);
    public static CanvasBrush Turquoise            => B( 64, 224, 208);
    public static CanvasBrush Violet               => B(238, 130, 238);
    public static CanvasBrush Wheat                => B(245, 222, 179);
    public static CanvasBrush White                => B(255, 255, 255);
    public static CanvasBrush WhiteSmoke           => B(245, 245, 245);
    public static CanvasBrush Yellow               => B(255, 255,   0);
    public static CanvasBrush YellowGreen          => B(154, 205,  50);
}

/// <summary>
/// <c>System.Drawing.SystemColors</c> — delegates to
/// <see cref="System.Windows.Forms.SystemColors"/> so that translated assemblies
/// referencing either namespace resolve to the same values.
/// </summary>
public static class SystemColors
{
    private static Color C(byte r, byte g, byte b) => Color.FromArgb(255, r, g, b);

    public static Color ActiveBorder          => System.Windows.Forms.SystemColors.ActiveBorder;
    public static Color ActiveCaption         => System.Windows.Forms.SystemColors.ActiveCaption;
    public static Color ActiveCaptionText     => System.Windows.Forms.SystemColors.ActiveCaptionText;
    public static Color AppWorkspace          => System.Windows.Forms.SystemColors.AppWorkspace;
    public static Color ButtonFace            => System.Windows.Forms.SystemColors.ButtonFace;
    public static Color ButtonHighlight       => System.Windows.Forms.SystemColors.ButtonHighlight;
    public static Color ButtonShadow          => System.Windows.Forms.SystemColors.ButtonShadow;
    public static Color Control               => System.Windows.Forms.SystemColors.Control;
    public static Color ControlDark           => System.Windows.Forms.SystemColors.ControlDark;
    public static Color ControlDarkDark       => System.Windows.Forms.SystemColors.ControlDarkDark;
    public static Color ControlLight          => System.Windows.Forms.SystemColors.ControlLight;
    public static Color ControlLightLight     => System.Windows.Forms.SystemColors.ControlLightLight;
    public static Color ControlText           => System.Windows.Forms.SystemColors.ControlText;
    public static Color Desktop               => System.Windows.Forms.SystemColors.Desktop;
    public static Color GradientActiveCaption => System.Windows.Forms.SystemColors.GradientActiveCaption;
    public static Color GradientInactiveCaption => System.Windows.Forms.SystemColors.GradientInactiveCaption;
    public static Color GrayText              => System.Windows.Forms.SystemColors.GrayText;
    public static Color Highlight             => System.Windows.Forms.SystemColors.Highlight;
    public static Color HighlightText         => System.Windows.Forms.SystemColors.HighlightText;
    public static Color HotTrack              => System.Windows.Forms.SystemColors.HotTrack;
    public static Color InactiveBorder        => System.Windows.Forms.SystemColors.InactiveBorder;
    public static Color InactiveCaption       => System.Windows.Forms.SystemColors.InactiveCaption;
    public static Color InactiveCaptionText   => System.Windows.Forms.SystemColors.InactiveCaptionText;
    public static Color Info                  => System.Windows.Forms.SystemColors.Info;
    public static Color InfoText              => System.Windows.Forms.SystemColors.InfoText;
    public static Color Menu                  => System.Windows.Forms.SystemColors.Menu;
    public static Color MenuBar               => System.Windows.Forms.SystemColors.MenuBar;
    public static Color MenuHighlight         => System.Windows.Forms.SystemColors.MenuHighlight;
    public static Color MenuText              => System.Windows.Forms.SystemColors.MenuText;
    public static Color ScrollBar             => System.Windows.Forms.SystemColors.ScrollBar;
    public static Color Window                => System.Windows.Forms.SystemColors.Window;
    public static Color WindowFrame           => System.Windows.Forms.SystemColors.WindowFrame;
    public static Color WindowText            => System.Windows.Forms.SystemColors.WindowText;
}

/// <summary>
/// <c>System.Drawing.SystemFonts</c> shim — delegates to
/// <see cref="System.Windows.Forms.SystemFonts"/> so translated assemblies
/// referencing either namespace resolve to the same values.
/// </summary>
public static class SystemFonts
{
    public static System.Drawing.Font DefaultFont      => new System.Drawing.Font("Segoe UI", 9f);
    public static System.Drawing.Font CaptionFont      => new System.Drawing.Font("Segoe UI", 9f);
    public static System.Drawing.Font IconTitleFont    => new System.Drawing.Font("Segoe UI", 9f);
    public static System.Drawing.Font MenuFont         => new System.Drawing.Font("Segoe UI", 9f);
    public static System.Drawing.Font MessageBoxFont   => new System.Drawing.Font("Segoe UI", 9f);
    public static System.Drawing.Font SmallCaptionFont => new System.Drawing.Font("Segoe UI", 8f);
    public static System.Drawing.Font StatusFont       => new System.Drawing.Font("Segoe UI", 9f);
    public static System.Drawing.Font ToolTipFont      => new System.Drawing.Font("Segoe UI", 9f);
    public static System.Drawing.Font? GetFontByName(string systemFontName)
        => systemFontName switch
        {
            "DefaultFont"      => DefaultFont,
            "CaptionFont"      => CaptionFont,
            "IconTitleFont"    => IconTitleFont,
            "MenuFont"         => MenuFont,
            "MessageBoxFont"   => MessageBoxFont,
            "SmallCaptionFont" => SmallCaptionFont,
            "StatusFont"       => StatusFont,
            "ToolTipFont"      => ToolTipFont,
            _                  => null,
        };
}
