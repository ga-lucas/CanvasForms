using SDColor = System.Drawing.Color;

namespace Canvas.Windows.Forms.Theming;

/// <summary>
/// Holds all named SDColor tokens for CanvasForms theming.
/// Load from JSON via <see cref="CanvasThemeLoader"/> and assign to <see cref="Current"/>.
/// </summary>
public sealed class CanvasTheme
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    private static CanvasTheme _current = new();
    public static CanvasTheme Current
    {
        get => _current;
        set
        {
            _current = value ?? new CanvasTheme();
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>Raised whenever <see cref="Current"/> is replaced (e.g. user picks a new theme).</summary>
    public static event EventHandler? ThemeChanged;

    // ── Control defaults ───────────────────────────────────────────────────────

    /// <summary>Default background SDColor for most controls (WinForms: SystemColors.Control = #F0F0F0).</summary>
    public SDColor ControlBackColor { get; set; } = SDColor.FromArgb(240, 240, 240);

    /// <summary>Default foreground/text SDColor for most controls (WinForms: SystemColors.ControlText = #000000).</summary>
    public SDColor ControlForeColor { get; set; } = SDColor.Black;

    /// <summary>Default background for editable text controls (TextBox, RichTextBox, ListBox …).</summary>
    public SDColor WindowBackColor { get; set; } = SDColor.White;

    /// <summary>Default text SDColor inside editable text controls.</summary>
    public SDColor WindowForeColor { get; set; } = SDColor.Black;

    // ── Form chrome ────────────────────────────────────────────────────────────

    /// <summary>Form title bar gradient top SDColor.</summary>
    public SDColor TitleBarGradientTop { get; set; } = SDColor.FromArgb(74, 144, 226);

    /// <summary>Form title bar gradient bottom SDColor.</summary>
    public SDColor TitleBarGradientBottom { get; set; } = SDColor.FromArgb(53, 122, 189);

    /// <summary>Form title bar text SDColor.</summary>
    public SDColor TitleBarText { get; set; } = SDColor.White;

    /// <summary>Form outer border SDColor.</summary>
    public SDColor FormBorderColor { get; set; } = SDColor.FromArgb(74, 144, 226);

    /// <summary>Close button hover background SDColor.</summary>
    public SDColor TitleBarCloseHover { get; set; } = SDColor.FromArgb(204, 0, 0);

    /// <summary>Minimize/maximize button hover background.</summary>
    public SDColor TitleBarButtonHover { get; set; } = SDColor.FromArgb(255, 255, 255, 77); // rgba(255,255,255,0.3)

    /// <summary>Corner radius (px) applied to window chrome. 0 = square (classic), 8 = Win11-style rounded.</summary>
    public int WindowCornerRadius { get; set; } = 4;

    // ── Button ─────────────────────────────────────────────────────────────────

    /// <summary>Default Button background (same as ControlBackColor in standard WinForms).</summary>
    public SDColor ButtonBackColor { get; set; } = SDColor.FromArgb(240, 240, 240);

    /// <summary>Default Button text SDColor.</summary>
    public SDColor ButtonForeColor { get; set; } = SDColor.Black;

    /// <summary>Button border SDColor in normal state.</summary>
    public SDColor ButtonBorderNormal { get; set; } = SDColor.FromArgb(173, 173, 173);

    /// <summary>Button border SDColor when hovered.</summary>
    public SDColor ButtonBorderHover { get; set; } = SDColor.FromArgb(0, 120, 215);

    /// <summary>Button border SDColor when pressed.</summary>
    public SDColor ButtonBorderPressed { get; set; } = SDColor.FromArgb(0, 84, 153);

    /// <summary>Button text SDColor when disabled.</summary>
    public SDColor ButtonDisabledForeColor { get; set; } = SDColor.FromArgb(109, 109, 109);

    // ── Focus ──────────────────────────────────────────────────────────────────

    /// <summary>Focus rectangle SDColor.</summary>
    public SDColor FocusRectColor { get; set; } = SDColor.FromArgb(80, 80, 80);

    /// <summary>Text selection / focus highlight (used by ListBox, TreeView, etc.).</summary>
    public SDColor SelectionBackColor { get; set; } = SDColor.FromArgb(0, 120, 215);

    /// <summary>Text SDColor inside selected items.</summary>
    public SDColor SelectionForeColor { get; set; } = SDColor.White;

    // ── Menu / ToolStrip ───────────────────────────────────────────────────────

    /// <summary>MenuStrip and ToolStrip background SDColor.</summary>
    public SDColor MenuBackColor { get; set; } = SDColor.FromArgb(240, 240, 240);

    /// <summary>MenuStrip and ToolStrip foreground/text SDColor.</summary>
    public SDColor MenuForeColor { get; set; } = SDColor.Black;

    /// <summary>Hovered menu item background.</summary>
    public SDColor MenuItemHoverBackColor { get; set; } = SDColor.FromArgb(0, 120, 215);

    /// <summary>Hovered menu item text SDColor.</summary>
    public SDColor MenuItemHoverForeColor { get; set; } = SDColor.White;

    /// <summary>Menu separator SDColor.</summary>
    public SDColor MenuSeparatorColor { get; set; } = SDColor.FromArgb(200, 200, 200);

    // ── Borders / separators ───────────────────────────────────────────────────

    /// <summary>Generic border SDColor used by panels, group boxes, splitters, etc.</summary>
    public SDColor BorderColor { get; set; } = SDColor.FromArgb(200, 200, 200);

    /// <summary>Disabled control border SDColor.</summary>
    public SDColor DisabledBorderColor { get; set; } = SDColor.FromArgb(173, 173, 173);

    // ── Placeholder / error images (JS renderer) ───────────────────────────────

    /// <summary>Background fill for image placeholders when image load fails.</summary>
    public SDColor ImagePlaceholderBackColor { get; set; } = SDColor.FromArgb(240, 240, 240);

    /// <summary>Border SDColor for image placeholders.</summary>
    public SDColor ImagePlaceholderBorderColor { get; set; } = SDColor.FromArgb(204, 204, 204);

    /// <summary>Text SDColor for image placeholder labels.</summary>
    public SDColor ImagePlaceholderTextColor { get; set; } = SDColor.FromArgb(153, 153, 153);

    /// <summary>Background fill for error image placeholders.</summary>
    public SDColor ImageErrorBackColor { get; set; } = SDColor.FromArgb(255, 224, 224);

    /// <summary>Border SDColor for error image placeholders.</summary>
    public SDColor ImageErrorBorderColor { get; set; } = SDColor.FromArgb(255, 0, 0);

    /// <summary>Text SDColor for error image placeholder labels.</summary>
    public SDColor ImageErrorTextColor { get; set; } = SDColor.FromArgb(204, 0, 0);

    // ── Scrollbar ──────────────────────────────────────────────────────────────

    /// <summary>Scrollbar track background SDColor.</summary>
    public SDColor ScrollBarTrackColor { get; set; } = SDColor.FromArgb(240, 240, 240);

    /// <summary>Scrollbar thumb SDColor.</summary>
    public SDColor ScrollBarThumbColor { get; set; } = SDColor.FromArgb(190, 190, 190);

    /// <summary>Scrollbar thumb hover SDColor.</summary>
    public SDColor ScrollBarThumbHoverColor { get; set; } = SDColor.FromArgb(130, 130, 130);

    // ── Desktop ────────────────────────────────────────────────────────────────

    /// <summary>Desktop background fill color (shown when no image is set or as fallback).</summary>
    public SDColor DesktopBackColor { get; set; } = SDColor.FromArgb(0, 128, 128); // classic teal

    /// <summary>
    /// URL or relative path to the desktop wallpaper image.
    /// Set to null or empty string for solid color only.
    /// Served from wwwroot, e.g. "images/wallpaper.jpg".
    /// </summary>
    public string? DesktopBackgroundImage { get; set; } = null;

    /// <summary>
    /// CSS background-size value for the desktop wallpaper.
    /// Common values: "cover", "contain", "auto", "100% 100%".
    /// </summary>
    public string DesktopBackgroundSize { get; set; } = "cover";

    /// <summary>
    /// CSS background-position value for the desktop wallpaper.
    /// </summary>
    public string DesktopBackgroundPosition { get; set; } = "center";

    // ── Taskbar ────────────────────────────────────────────────────────────────

    /// <summary>Taskbar background gradient top color.</summary>
    public SDColor TaskbarGradientTop { get; set; } = SDColor.FromArgb(240, 240, 240);

    /// <summary>Taskbar background gradient bottom color.</summary>
    public SDColor TaskbarGradientBottom { get; set; } = SDColor.FromArgb(208, 208, 208);

    /// <summary>Taskbar bottom border color.</summary>
    public SDColor TaskbarBorderColor { get; set; } = SDColor.FromArgb(160, 160, 160);

    /// <summary>Active window button gradient top color in the taskbar.</summary>
    public SDColor TaskbarButtonActiveTop { get; set; } = SDColor.FromArgb(227, 242, 253);

    /// <summary>Active window button gradient bottom color in the taskbar.</summary>
    public SDColor TaskbarButtonActiveBottom { get; set; } = SDColor.FromArgb(187, 222, 251);

    /// <summary>Active window button border color.</summary>
    public SDColor TaskbarButtonActiveBorder { get; set; } = SDColor.FromArgb(33, 150, 243);

    /// <summary>Active window button text color.</summary>
    public SDColor TaskbarButtonActiveForeColor { get; set; } = SDColor.FromArgb(21, 101, 192);

    /// <summary>Inactive window button gradient top color.</summary>
    public SDColor TaskbarButtonInactiveTop { get; set; } = SDColor.FromArgb(245, 245, 245);

    /// <summary>Inactive window button gradient bottom color.</summary>
    public SDColor TaskbarButtonInactiveBottom { get; set; } = SDColor.FromArgb(213, 213, 213);

    /// <summary>Inactive window button border color.</summary>
    public SDColor TaskbarButtonInactiveBorder { get; set; } = SDColor.FromArgb(160, 160, 160);

    /// <summary>Inactive window button text color.</summary>
    public SDColor TaskbarButtonInactiveForeColor { get; set; } = SDColor.Black;

    /// <summary>Minimized window button text color (italic in taskbar).</summary>
    public SDColor TaskbarButtonMinimizedForeColor { get; set; } = SDColor.FromArgb(128, 128, 128);

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>Serialize all named SDColor slots as a dictionary keyed by slot name (hex #RRGGBB or #AARRGGBB).</summary>
    public Dictionary<string, string> ToJsDictionary()
    {
        return new Dictionary<string, string>
        {
            ["controlBackColor"]          = ToHex(ControlBackColor),
            ["controlForeColor"]          = ToHex(ControlForeColor),
            ["windowBackColor"]           = ToHex(WindowBackColor),
            ["windowForeColor"]           = ToHex(WindowForeColor),
            ["titleBarGradientTop"]       = ToHex(TitleBarGradientTop),
            ["titleBarGradientBottom"]    = ToHex(TitleBarGradientBottom),
            ["titleBarText"]              = ToHex(TitleBarText),
            ["formBorderColor"]           = ToRgba(FormBorderColor),
            ["titleBarCloseHover"]        = ToRgba(TitleBarCloseHover),
            ["titleBarButtonHover"]       = ToRgba(TitleBarButtonHover),
            ["buttonBackColor"]           = ToHex(ButtonBackColor),
            ["buttonForeColor"]           = ToHex(ButtonForeColor),
            ["buttonBorderNormal"]        = ToHex(ButtonBorderNormal),
            ["buttonBorderHover"]         = ToHex(ButtonBorderHover),
            ["buttonBorderPressed"]       = ToHex(ButtonBorderPressed),
            ["buttonDisabledForeColor"]   = ToHex(ButtonDisabledForeColor),
            ["focusRectColor"]            = ToHex(FocusRectColor),
            ["selectionBackColor"]        = ToHex(SelectionBackColor),
            ["selectionForeColor"]        = ToHex(SelectionForeColor),
            ["menuBackColor"]             = ToHex(MenuBackColor),
            ["menuForeColor"]             = ToHex(MenuForeColor),
            ["menuItemHoverBackColor"]    = ToHex(MenuItemHoverBackColor),
            ["menuItemHoverForeColor"]    = ToHex(MenuItemHoverForeColor),
            ["menuSeparatorColor"]        = ToHex(MenuSeparatorColor),
            ["borderColor"]              = ToHex(BorderColor),
            ["disabledBorderColor"]      = ToHex(DisabledBorderColor),
            ["imagePlaceholderBackColor"] = ToHex(ImagePlaceholderBackColor),
            ["imagePlaceholderBorderColor"] = ToHex(ImagePlaceholderBorderColor),
            ["imagePlaceholderTextColor"] = ToHex(ImagePlaceholderTextColor),
            ["imageErrorBackColor"]       = ToHex(ImageErrorBackColor),
            ["imageErrorBorderColor"]     = ToHex(ImageErrorBorderColor),
            ["imageErrorTextColor"]       = ToHex(ImageErrorTextColor),
            ["scrollBarTrackColor"]       = ToHex(ScrollBarTrackColor),
            ["scrollBarThumbColor"]       = ToHex(ScrollBarThumbColor),
            ["scrollBarThumbHoverColor"]  = ToHex(ScrollBarThumbHoverColor),
            ["desktopBackColor"]                  = ToHex(DesktopBackColor),
            ["taskbarGradientTop"]                = ToHex(TaskbarGradientTop),
            ["taskbarGradientBottom"]             = ToHex(TaskbarGradientBottom),
            ["taskbarBorderColor"]                = ToHex(TaskbarBorderColor),
            ["taskbarButtonActiveTop"]            = ToHex(TaskbarButtonActiveTop),
            ["taskbarButtonActiveBottom"]         = ToHex(TaskbarButtonActiveBottom),
            ["taskbarButtonActiveBorder"]         = ToHex(TaskbarButtonActiveBorder),
            ["taskbarButtonActiveForeColor"]      = ToHex(TaskbarButtonActiveForeColor),
            ["taskbarButtonInactiveTop"]          = ToHex(TaskbarButtonInactiveTop),
            ["taskbarButtonInactiveBottom"]       = ToHex(TaskbarButtonInactiveBottom),
            ["taskbarButtonInactiveBorder"]       = ToHex(TaskbarButtonInactiveBorder),
            ["taskbarButtonInactiveForeColor"]    = ToHex(TaskbarButtonInactiveForeColor),
            ["taskbarButtonMinimizedForeColor"]   = ToHex(TaskbarButtonMinimizedForeColor),
            ["windowCornerRadius"]                 = WindowCornerRadius.ToString(),
        };
    }

    private static string ToHex(SDColor c)
        => c.A == 255
            ? $"#{c.R:X2}{c.G:X2}{c.B:X2}"
            : $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

    private static string ToRgba(SDColor c)
        => $"rgba({c.R},{c.G},{c.B},{c.A / 255.0:F3})";
}

