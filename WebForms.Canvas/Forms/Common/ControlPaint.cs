namespace System.Windows.Forms;

/// <summary>
/// Provides methods for drawing common controls and their elements.
/// Canvas-layer stub — drawing is performed through the canvas pipeline rather than GDI+.
/// </summary>
public static class ControlPaint
{
    /// <summary>
    /// Draws a focus rectangle on the specified graphics surface and within the specified bounds.
    /// </summary>
    public static void DrawFocusRectangle(Graphics g, System.Drawing.Rectangle rectangle)
    {
        using var pen = new Pen(System.Drawing.Color.Black);
        pen.DashStyle = DashStyle.Dot;
        g.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
    }

    /// <summary>
    /// Draws a focus rectangle using the given foreground and background colors.
    /// </summary>
    public static void DrawFocusRectangle(Graphics g, System.Drawing.Rectangle rectangle,
        System.Drawing.Color foreColor, System.Drawing.Color backColor)
        => DrawFocusRectangle(g, rectangle);

    /// <summary>
    /// Draws a 3D border with the given style.
    /// </summary>
    public static void DrawBorder3D(Graphics g, System.Drawing.Rectangle rectangle,
        Border3DStyle style = Border3DStyle.Etched)
    {
        using var outer = new Pen(System.Drawing.Color.FromArgb(128, 128, 128));
        using var inner = new Pen(System.Drawing.Color.FromArgb(200, 200, 200));
        g.DrawRectangle(outer, rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
        g.DrawRectangle(inner, rectangle.X + 1, rectangle.Y + 1, rectangle.Width - 3, rectangle.Height - 3);
    }

    /// <summary>
    /// Draws a flat border with the specified style.
    /// </summary>
    public static void DrawBorder(Graphics g, System.Drawing.Rectangle bounds,
        System.Drawing.Color color, ButtonBorderStyle style)
    {
        if (style == ButtonBorderStyle.None) return;
        using var pen = new Pen(color);
        if (style == ButtonBorderStyle.Dashed)
            pen.DashStyle = DashStyle.Dash;
        else if (style == ButtonBorderStyle.Dotted)
            pen.DashStyle = DashStyle.Dot;
        g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
    }

    /// <summary>Creates a lighter version of the specified color.</summary>
    public static System.Drawing.Color Light(System.Drawing.Color baseColor, float percOfLightLight = 0.5f)
    {
        int r = Math.Min(255, baseColor.R + (int)((255 - baseColor.R) * percOfLightLight));
        int gv = Math.Min(255, baseColor.G + (int)((255 - baseColor.G) * percOfLightLight));
        int b = Math.Min(255, baseColor.B + (int)((255 - baseColor.B) * percOfLightLight));
        return System.Drawing.Color.FromArgb(baseColor.A, r, gv, b);
    }

    /// <summary>Creates a darker version of the specified color.</summary>
    public static System.Drawing.Color Dark(System.Drawing.Color baseColor, float percOfDarkDark = 0.5f)
    {
        int r = Math.Max(0, baseColor.R - (int)(baseColor.R * percOfDarkDark));
        int gv = Math.Max(0, baseColor.G - (int)(baseColor.G * percOfDarkDark));
        int b = Math.Max(0, baseColor.B - (int)(baseColor.B * percOfDarkDark));
        return System.Drawing.Color.FromArgb(baseColor.A, r, gv, b);
    }

    /// <summary>Returns a very light variant of the specified color.</summary>
    public static System.Drawing.Color LightLight(System.Drawing.Color baseColor) => Light(baseColor, 0.75f);

    /// <summary>Returns a very dark variant of the specified color.</summary>
    public static System.Drawing.Color DarkDark(System.Drawing.Color baseColor) => Dark(baseColor, 0.75f);
}

/// <summary>Specifies the border style for a button control.</summary>
public enum ButtonBorderStyle
{
    None = 0,
    Dotted = 1,
    Dashed = 2,
    Solid = 3,
    Inset = 4,
    Outset = 5,
}
