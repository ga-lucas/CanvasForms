namespace Canvas.Windows.Forms.Theming;

/// <summary>
/// Named theme registry for CanvasForms.
/// Built-in themes (Classic/Light/Dark) are pre-registered from embedded JSON.
/// Call <see cref="Register"/> to add themes loaded from external sources (e.g. server files).
/// Call <see cref="Apply"/> to set <see cref="CanvasTheme.Current"/> from a named preset.
/// </summary>
public static class CanvasThemeRegistry
{
    // ── Public theme names ────────────────────────────────────────────────────

    public const string Classic = "Classic";
    public const string Light   = "Light";
    public const string Dark    = "Dark";

    // Mutable registry: name → raw JSON.  Seeded with embedded fallbacks.
    private static readonly Dictionary<string, string> _registry = new(StringComparer.OrdinalIgnoreCase)
    {
        [Classic] = ClassicJson,
        [Light]   = LightJson,
        [Dark]    = DarkJson,
    };

    /// <summary>
    /// All currently registered theme names, in registration order.
    /// Starts with the three built-ins; grows as <see cref="Register"/> is called.
    /// </summary>
    public static IReadOnlyList<string> BuiltInThemes => _registry.Keys.ToList();

    // ── Registration ──────────────────────────────────────────────────────────

    /// <summary>
    /// Registers (or replaces) a theme by name from a raw JSON string.
    /// The name comparison is case-insensitive.
    /// </summary>
    public static void Register(string name, string json)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Theme name required.", nameof(name));
        if (string.IsNullOrWhiteSpace(json)) throw new ArgumentException("Theme JSON required.", nameof(json));
        _registry[name] = json;
    }

    // ── Apply ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Applies the named preset to <see cref="CanvasTheme.Current"/>.
    /// Returns <c>false</c> if the name was not found.
    /// </summary>
    public static bool Apply(string themeName)
    {
        if (!_registry.TryGetValue(themeName, out var json)) return false;
        CanvasThemeLoader.LoadFromJson(json);
        return true;
    }

    /// <summary>
    /// Returns a <see cref="CanvasTheme"/> loaded from the named preset without
    /// changing <see cref="CanvasTheme.Current"/> or firing <see cref="CanvasTheme.ThemeChanged"/>.
    /// Returns <c>null</c> if the name was not found.
    /// </summary>
    public static CanvasTheme? Peek(string themeName)
    {
        if (!_registry.TryGetValue(themeName, out var json)) return null;
        return CanvasThemeLoader.LoadFromJsonWithoutApplying(json);
    }

    // ── Embedded fallback JSON ────────────────────────────────────────────────
    // These are used when the server theme files cannot be fetched (e.g. offline,
    // first render before async load completes).  Edit the external .json files
    // in Canvas.Windows.Forms.Host.Server/themes/ for runtime changes.

    private const string ClassicJson = """
    {
      "controlBackColor":           "#F0F0F0",
      "controlForeColor":           "#000000",
      "windowBackColor":            "#FFFFFF",
      "windowForeColor":            "#000000",

      "titleBarGradientTop":        "#4A90E2",
      "titleBarGradientBottom":     "#357ABD",
      "titleBarText":               "#FFFFFF",
      "formBorderColor":            "rgba(74,144,226,0.5)",
      "titleBarCloseHover":         "rgba(204,0,0,0.7)",
      "titleBarButtonHover":        "rgba(255,255,255,0.3)",

      "buttonBackColor":            "#F0F0F0",
      "buttonForeColor":            "#000000",
      "buttonBorderNormal":         "#ADADAD",
      "buttonBorderHover":          "#0078D7",
      "buttonBorderPressed":        "#005499",
      "buttonDisabledForeColor":    "#6D6D6D",

      "focusRectColor":             "#505050",
      "selectionBackColor":         "#0078D7",
      "selectionForeColor":         "#FFFFFF",

      "menuBackColor":              "#F0F0F0",
      "menuForeColor":              "#000000",
      "menuItemHoverBackColor":     "#0078D7",
      "menuItemHoverForeColor":     "#FFFFFF",
      "menuSeparatorColor":         "#C8C8C8",

      "borderColor":                "#C8C8C8",
      "disabledBorderColor":        "#ADADAD",

      "imagePlaceholderBackColor":  "#F0F0F0",
      "imagePlaceholderBorderColor":"#CCCCCC",
      "imagePlaceholderTextColor":  "#999999",
      "imageErrorBackColor":        "#FFE0E0",
      "imageErrorBorderColor":      "#FF0000",
      "imageErrorTextColor":        "#CC0000",

      "scrollBarTrackColor":        "#F0F0F0",
      "scrollBarThumbColor":        "#BEBEBE",
      "scrollBarThumbHoverColor":   "#828282",

      "desktopBackColor":           "#008080",
      "desktopBackgroundImage":     "",
      "desktopBackgroundSize":      "cover",
      "desktopBackgroundPosition":  "center",

      "taskbarGradientTop":               "#F0F0F0",
      "taskbarGradientBottom":            "#D0D0D0",
      "taskbarBorderColor":               "#A0A0A0",
      "taskbarButtonActiveTop":           "#E3F2FD",
      "taskbarButtonActiveBottom":        "#BBDEFB",
      "taskbarButtonActiveBorder":        "#2196F3",
      "taskbarButtonActiveForeColor":     "#1565C0",
      "taskbarButtonInactiveTop":         "#F5F5F5",
      "taskbarButtonInactiveBottom":      "#D5D5D5",
      "taskbarButtonInactiveBorder":      "#A0A0A0",
      "taskbarButtonInactiveForeColor":   "#000000",
      "taskbarButtonMinimizedForeColor":  "#808080",
      "windowCornerRadius":               0
    }
    """;

    // ── Light (bright whites, soft blue accents) ──────────────────────────────

    private const string LightJson = """
    {
      "controlBackColor":           "#FFFFFF",
      "controlForeColor":           "#1A1A1A",
      "windowBackColor":            "#FAFAFA",
      "windowForeColor":            "#1A1A1A",

      "titleBarGradientTop":        "#E8F0FE",
      "titleBarGradientBottom":     "#C7D9FC",
      "titleBarText":               "#1A3A6E",
      "formBorderColor":            "rgba(100,149,237,0.4)",
      "titleBarCloseHover":         "rgba(220,50,50,0.75)",
      "titleBarButtonHover":        "rgba(100,149,237,0.25)",

      "buttonBackColor":            "#FFFFFF",
      "buttonForeColor":            "#1A1A1A",
      "buttonBorderNormal":         "#C8C8C8",
      "buttonBorderHover":          "#4A90D9",
      "buttonBorderPressed":        "#2A6FB0",
      "buttonDisabledForeColor":    "#AAAAAA",

      "focusRectColor":             "#4A90D9",
      "selectionBackColor":         "#4A90D9",
      "selectionForeColor":         "#FFFFFF",

      "menuBackColor":              "#FFFFFF",
      "menuForeColor":              "#1A1A1A",
      "menuItemHoverBackColor":     "#4A90D9",
      "menuItemHoverForeColor":     "#FFFFFF",
      "menuSeparatorColor":         "#E0E0E0",

      "borderColor":                "#DEDEDE",
      "disabledBorderColor":        "#C8C8C8",

      "imagePlaceholderBackColor":  "#F5F5F5",
      "imagePlaceholderBorderColor":"#DDDDDD",
      "imagePlaceholderTextColor":  "#BBBBBB",
      "imageErrorBackColor":        "#FFF0F0",
      "imageErrorBorderColor":      "#FF6666",
      "imageErrorTextColor":        "#CC3333",

      "scrollBarTrackColor":        "#F0F0F0",
      "scrollBarThumbColor":        "#C0C0C0",
      "scrollBarThumbHoverColor":   "#909090",

      "desktopBackColor":           "#D6E4F0",
      "desktopBackgroundImage":     "",
      "desktopBackgroundSize":      "cover",
      "desktopBackgroundPosition":  "center",

      "taskbarGradientTop":               "#EEF3FB",
      "taskbarGradientBottom":            "#D8E5F8",
      "taskbarBorderColor":               "#B0C4DE",
      "taskbarButtonActiveTop":           "#FFFFFF",
      "taskbarButtonActiveBottom":        "#D0E4FF",
      "taskbarButtonActiveBorder":        "#4A90D9",
      "taskbarButtonActiveForeColor":     "#1A3A6E",
      "taskbarButtonInactiveTop":         "#F5F8FF",
      "taskbarButtonInactiveBottom":      "#E0EAFA",
      "taskbarButtonInactiveBorder":      "#B0C4DE",
      "taskbarButtonInactiveForeColor":   "#333333",
      "taskbarButtonMinimizedForeColor":  "#7090B0",
      "windowCornerRadius":               8
    }
    """;

    // ── Dark (near-black surfaces, cyan/blue accents) ─────────────────────────

    private const string DarkJson = """
    {
      "controlBackColor":           "#2D2D2D",
      "controlForeColor":           "#E0E0E0",
      "windowBackColor":            "#1E1E1E",
      "windowForeColor":            "#E0E0E0",

      "titleBarGradientTop":        "#3A3A3A",
      "titleBarGradientBottom":     "#252525",
      "titleBarText":               "#E0E0E0",
      "formBorderColor":            "rgba(80,80,80,0.7)",
      "titleBarCloseHover":         "rgba(196,43,28,0.85)",
      "titleBarButtonHover":        "rgba(255,255,255,0.15)",

      "buttonBackColor":            "#3C3C3C",
      "buttonForeColor":            "#E0E0E0",
      "buttonBorderNormal":         "#5A5A5A",
      "buttonBorderHover":          "#4FC3F7",
      "buttonBorderPressed":        "#0288D1",
      "buttonDisabledForeColor":    "#666666",

      "focusRectColor":             "#4FC3F7",
      "selectionBackColor":         "#0D47A1",
      "selectionForeColor":         "#E0E0E0",

      "menuBackColor":              "#2D2D2D",
      "menuForeColor":              "#E0E0E0",
      "menuItemHoverBackColor":     "#0D47A1",
      "menuItemHoverForeColor":     "#FFFFFF",
      "menuSeparatorColor":         "#444444",

      "borderColor":                "#555555",
      "disabledBorderColor":        "#444444",

      "imagePlaceholderBackColor":  "#2A2A2A",
      "imagePlaceholderBorderColor":"#444444",
      "imagePlaceholderTextColor":  "#666666",
      "imageErrorBackColor":        "#3A1A1A",
      "imageErrorBorderColor":      "#993333",
      "imageErrorTextColor":        "#CC6666",

      "scrollBarTrackColor":        "#252525",
      "scrollBarThumbColor":        "#555555",
      "scrollBarThumbHoverColor":   "#777777",

      "desktopBackColor":           "#1A1A2E",
      "desktopBackgroundImage":     "",
      "desktopBackgroundSize":      "cover",
      "desktopBackgroundPosition":  "center",

      "taskbarGradientTop":               "#2A2A2A",
      "taskbarGradientBottom":            "#1A1A1A",
      "taskbarBorderColor":               "#444444",
      "taskbarButtonActiveTop":           "#2A3A4A",
      "taskbarButtonActiveBottom":        "#1A2A3A",
      "taskbarButtonActiveBorder":        "#4FC3F7",
      "taskbarButtonActiveForeColor":     "#4FC3F7",
      "taskbarButtonInactiveTop":         "#303030",
      "taskbarButtonInactiveBottom":      "#222222",
      "taskbarButtonInactiveBorder":      "#484848",
      "taskbarButtonInactiveForeColor":   "#B0B0B0",
      "taskbarButtonMinimizedForeColor":  "#707070",
      "windowCornerRadius":               8
    }
    """;
}
