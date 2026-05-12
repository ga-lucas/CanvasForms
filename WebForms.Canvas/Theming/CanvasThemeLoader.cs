using SDColor = System.Drawing.Color;
using System.Text.Json;

namespace Canvas.Windows.Forms.Theming;

/// <summary>
/// Loads a <see cref="CanvasTheme"/> from a JSON file or stream.
/// The JSON format is a flat object whose keys are camelCase slot names and
/// values are CSS-style SDColor strings: "#RRGGBB", "#AARRGGBB", or "rgb(r,g,b)".
///
/// Unknown keys are silently ignored so partial theme files work fine —
/// only the slots present in the file are overridden.
/// </summary>
public static class CanvasThemeLoader
{
    /// <summary>
    /// Loads a theme from <paramref name="filePath"/> and sets it as
    /// <see cref="CanvasTheme.Current"/>.  Does nothing if the file does not exist.
    /// </summary>
    public static void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        using var stream = File.OpenRead(filePath);
        LoadFromStream(stream);
    }

    /// <summary>
    /// Loads a theme from an arbitrary <paramref name="stream"/> and sets it as
    /// <see cref="CanvasTheme.Current"/>.
    /// </summary>
    public static void LoadFromStream(Stream stream)
    {
        using var doc = JsonDocument.Parse(stream);
        CanvasTheme.Current = ApplyJson(CanvasTheme.Current, doc.RootElement);
    }

    /// <summary>
    /// Parses <paramref name="json"/> (UTF-8 JSON text) and applies it on top of a fresh
    /// <see cref="CanvasTheme"/> instance, then sets <see cref="CanvasTheme.Current"/>.
    /// </summary>
    public static void LoadFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        CanvasTheme.Current = ApplyJson(new CanvasTheme(), doc.RootElement);
    }

    /// <summary>
    /// Parses <paramref name="json"/> and returns a new <see cref="CanvasTheme"/> without
    /// touching <see cref="CanvasTheme.Current"/> or firing <see cref="CanvasTheme.ThemeChanged"/>.
    /// </summary>
    public static CanvasTheme LoadFromJsonWithoutApplying(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return ApplyJson(new CanvasTheme(), doc.RootElement);
    }

    // ── Internal ───────────────────────────────────────────────────────────────

    private static CanvasTheme ApplyJson(CanvasTheme theme, JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object) return theme;

        foreach (var prop in root.EnumerateObject())
        {
            // Handle numeric slots first (non-string ValueKind)
            if (prop.Value.ValueKind == JsonValueKind.Number)
            {
                switch (prop.Name)
                {
                    case "windowCornerRadius":
                        theme.WindowCornerRadius = prop.Value.GetInt32(); break;
                }
                continue;
            }

            if (prop.Value.ValueKind != JsonValueKind.String) continue;

            var strVal = prop.Value.GetString();
            if (strVal is null) continue;

            // Handle non-color string slots first
            switch (prop.Name)
            {
                case "desktopBackgroundImage":    theme.DesktopBackgroundImage    = strVal; continue;
                case "desktopBackgroundSize":     theme.DesktopBackgroundSize     = strVal; continue;
                case "desktopBackgroundPosition": theme.DesktopBackgroundPosition = strVal; continue;
                case "windowCornerRadius":
                    if (int.TryParse(strVal, out var cr)) theme.WindowCornerRadius = cr;
                    continue;
            }

            if (!TryParseColor(strVal, out var SDColor)) continue;

            switch (prop.Name)
            {
                case "controlBackColor":          theme.ControlBackColor          = SDColor; break;
                case "controlForeColor":          theme.ControlForeColor          = SDColor; break;
                case "windowBackColor":           theme.WindowBackColor           = SDColor; break;
                case "windowForeColor":           theme.WindowForeColor           = SDColor; break;
                case "titleBarGradientTop":       theme.TitleBarGradientTop       = SDColor; break;
                case "titleBarGradientBottom":    theme.TitleBarGradientBottom    = SDColor; break;
                case "titleBarText":              theme.TitleBarText              = SDColor; break;
                case "formBorderColor":           theme.FormBorderColor           = SDColor; break;
                case "titleBarCloseHover":        theme.TitleBarCloseHover        = SDColor; break;
                case "titleBarButtonHover":       theme.TitleBarButtonHover       = SDColor; break;
                case "buttonBackColor":           theme.ButtonBackColor           = SDColor; break;
                case "buttonForeColor":           theme.ButtonForeColor           = SDColor; break;
                case "buttonBorderNormal":        theme.ButtonBorderNormal        = SDColor; break;
                case "buttonBorderHover":         theme.ButtonBorderHover         = SDColor; break;
                case "buttonBorderPressed":       theme.ButtonBorderPressed       = SDColor; break;
                case "buttonDisabledForeColor":   theme.ButtonDisabledForeColor   = SDColor; break;
                case "focusRectColor":            theme.FocusRectColor            = SDColor; break;
                case "selectionBackColor":        theme.SelectionBackColor        = SDColor; break;
                case "selectionForeColor":        theme.SelectionForeColor        = SDColor; break;
                case "menuBackColor":             theme.MenuBackColor             = SDColor; break;
                case "menuForeColor":             theme.MenuForeColor             = SDColor; break;
                case "menuItemHoverBackColor":    theme.MenuItemHoverBackColor    = SDColor; break;
                case "menuItemHoverForeColor":    theme.MenuItemHoverForeColor    = SDColor; break;
                case "menuSeparatorColor":        theme.MenuSeparatorColor        = SDColor; break;
                case "borderColor":              theme.BorderColor               = SDColor; break;
                case "disabledBorderColor":      theme.DisabledBorderColor       = SDColor; break;
                case "imagePlaceholderBackColor": theme.ImagePlaceholderBackColor = SDColor; break;
                case "imagePlaceholderBorderColor": theme.ImagePlaceholderBorderColor = SDColor; break;
                case "imagePlaceholderTextColor": theme.ImagePlaceholderTextColor = SDColor; break;
                case "imageErrorBackColor":       theme.ImageErrorBackColor       = SDColor; break;
                case "imageErrorBorderColor":     theme.ImageErrorBorderColor     = SDColor; break;
                case "imageErrorTextColor":       theme.ImageErrorTextColor       = SDColor; break;
                case "scrollBarTrackColor":       theme.ScrollBarTrackColor       = SDColor; break;
                case "scrollBarThumbColor":       theme.ScrollBarThumbColor       = SDColor; break;
                case "scrollBarThumbHoverColor":  theme.ScrollBarThumbHoverColor  = SDColor; break;
                case "desktopBackColor":                  theme.DesktopBackColor                  = SDColor; break;
                case "taskbarGradientTop":                theme.TaskbarGradientTop                = SDColor; break;
                case "taskbarGradientBottom":             theme.TaskbarGradientBottom             = SDColor; break;
                case "taskbarBorderColor":                theme.TaskbarBorderColor                = SDColor; break;
                case "taskbarButtonActiveTop":            theme.TaskbarButtonActiveTop            = SDColor; break;
                case "taskbarButtonActiveBottom":         theme.TaskbarButtonActiveBottom         = SDColor; break;
                case "taskbarButtonActiveBorder":         theme.TaskbarButtonActiveBorder         = SDColor; break;
                case "taskbarButtonActiveForeColor":      theme.TaskbarButtonActiveForeColor      = SDColor; break;
                case "taskbarButtonInactiveTop":          theme.TaskbarButtonInactiveTop          = SDColor; break;
                case "taskbarButtonInactiveBottom":       theme.TaskbarButtonInactiveBottom       = SDColor; break;
                case "taskbarButtonInactiveBorder":       theme.TaskbarButtonInactiveBorder       = SDColor; break;
                case "taskbarButtonInactiveForeColor":    theme.TaskbarButtonInactiveForeColor    = SDColor; break;
                case "taskbarButtonMinimizedForeColor":   theme.TaskbarButtonMinimizedForeColor   = SDColor; break;
            }
        }

        return theme;
    }

    /// <summary>
    /// Parses CSS-style SDColor strings:
    ///   #RGB, #RRGGBB, #AARRGGBB, rgb(r,g,b), rgba(r,g,b,a)
    /// </summary>
    public static bool TryParseColor(string value, out SDColor color)
    {
        color = SDColor.Empty;
        if (string.IsNullOrWhiteSpace(value)) return false;

        value = value.Trim();

        if (value.StartsWith('#'))
        {
            var hex = value[1..];
            switch (hex.Length)
            {
                case 3:
                    // #RGB → #RRGGBB
                    hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
                    goto case 6;
                case 6:
                    if (uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var rgb6))
                    {
                        color = SDColor.FromArgb(255, (int)((rgb6 >> 16) & 0xFF), (int)((rgb6 >> 8) & 0xFF), (int)(rgb6 & 0xFF));
                        return true;
                    }
                    break;
                case 8:
                    // #AARRGGBB
                    if (uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var argb8))
                    {
                        color = SDColor.FromArgb((int)((argb8 >> 24) & 0xFF), (int)((argb8 >> 16) & 0xFF), (int)((argb8 >> 8) & 0xFF), (int)(argb8 & 0xFF));
                        return true;
                    }
                    break;
            }
            return false;
        }

        // rgb(r,g,b) or rgba(r,g,b,a)
        if (value.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase) && value.EndsWith(')'))
        {
            var inner = value[5..^1];
            var parts = inner.Split(',');
            if (parts.Length == 4
                && int.TryParse(parts[0].Trim(), out int r)
                && int.TryParse(parts[1].Trim(), out int g)
                && int.TryParse(parts[2].Trim(), out int b)
                && double.TryParse(parts[3].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double a))
            {
                color = SDColor.FromArgb((int)Math.Round(a * 255), r, g, b);
                return true;
            }
        }

        if (value.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase) && value.EndsWith(')'))
        {
            var inner = value[4..^1];
            var parts = inner.Split(',');
            if (parts.Length == 3
                && int.TryParse(parts[0].Trim(), out int r)
                && int.TryParse(parts[1].Trim(), out int g)
                && int.TryParse(parts[2].Trim(), out int b))
            {
                color = SDColor.FromArgb(255, r, g, b);
                return true;
            }
        }

        return false;
    }
}


