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

    // ── Button ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws a button control in the specified state.
    /// </summary>
    public static void DrawButton(Graphics g, System.Drawing.Rectangle bounds, ButtonState state)
    {
        bool pressed  = (state & ButtonState.Pushed)    != 0;
        bool disabled = (state & ButtonState.Inactive)  != 0;

        var face  = disabled ? System.Drawing.Color.FromArgb(0xD4, 0xD0, 0xC8) : System.Drawing.Color.FromArgb(0xF0, 0xF0, 0xF0);
        var light = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF);
        var dark  = System.Drawing.Color.FromArgb(0x80, 0x80, 0x80);

        using var faceBrush = new SolidBrush((CanvasColor)face);
        g.FillRectangle(faceBrush, bounds.X, bounds.Y, bounds.Width, bounds.Height);

        // Highlight / shadow edges
        using var penLight = new Pen(pressed ? dark : light);
        using var penDark  = new Pen(pressed ? light : dark);
        g.DrawLine(penLight, bounds.X, bounds.Bottom - 1, bounds.X, bounds.Y);
        g.DrawLine(penLight, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
        g.DrawLine(penDark,  bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
        g.DrawLine(penDark,  bounds.Right - 1, bounds.Bottom - 1, bounds.Right - 1, bounds.Y);
    }

    /// <summary>Draws a button with an explicit rectangle overload.</summary>
    public static void DrawButton(Graphics g, int x, int y, int width, int height, ButtonState state)
        => DrawButton(g, new System.Drawing.Rectangle(x, y, width, height), state);

    // ── CheckBox ──────────────────────────────────────────────────────────────

    /// <summary>Draws a check box control.</summary>
    public static void DrawCheckBox(Graphics g, System.Drawing.Rectangle bounds, ButtonState state)
    {
        bool checked_  = (state & ButtonState.Checked)   != 0;
        bool disabled  = (state & ButtonState.Inactive)  != 0;

        var borderColor = disabled ? System.Drawing.Color.FromArgb(0xAA, 0xAA, 0xAA)
                                   : System.Drawing.Color.FromArgb(0x33, 0x33, 0x33);
        var bgColor     = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF);

        using var bg  = new SolidBrush((CanvasColor)bgColor);
        using var pen = new Pen(borderColor);
        g.FillRectangle(bg, bounds.X, bounds.Y, bounds.Width, bounds.Height);
        g.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

        if (checked_)
        {
            using var checkPen = new Pen(disabled
                ? System.Drawing.Color.FromArgb(0x99, 0x99, 0x99)
                : System.Drawing.Color.FromArgb(0x00, 0x00, 0x00));
            checkPen.Width = 2;
            int cx = bounds.X + bounds.Width / 2;
            int cy = bounds.Y + bounds.Height / 2;
            g.DrawLine(checkPen, bounds.X + 2, cy, cx - 1, bounds.Bottom - 3);
            g.DrawLine(checkPen, cx - 1, bounds.Bottom - 3, bounds.Right - 2, bounds.Y + 2);
        }
    }

    /// <summary>Draws a check box with explicit coordinates.</summary>
    public static void DrawCheckBox(Graphics g, int x, int y, int width, int height, ButtonState state)
        => DrawCheckBox(g, new System.Drawing.Rectangle(x, y, width, height), state);

    // ── RadioButton ───────────────────────────────────────────────────────────

    /// <summary>Draws a radio button control.</summary>
    public static void DrawRadioButton(Graphics g, System.Drawing.Rectangle bounds, ButtonState state)
    {
        bool checked_  = (state & ButtonState.Checked)  != 0;
        bool disabled  = (state & ButtonState.Inactive) != 0;

        var borderColor = disabled ? System.Drawing.Color.FromArgb(0xAA, 0xAA, 0xAA)
                                   : System.Drawing.Color.FromArgb(0x33, 0x33, 0x33);
        var bgColor     = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF);

        using var bg  = new SolidBrush((CanvasColor)bgColor);
        using var pen = new Pen(borderColor);
        g.FillEllipse(bg,  bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        g.DrawEllipse(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

        if (checked_)
        {
            int inset = bounds.Width / 4;
            using var dotBrush = new SolidBrush(disabled
                ? (CanvasColor)System.Drawing.Color.FromArgb(0x99, 0x99, 0x99)
                : (CanvasColor)System.Drawing.Color.FromArgb(0x00, 0x00, 0x00));
            g.FillEllipse(dotBrush,
                bounds.X + inset, bounds.Y + inset,
                bounds.Width - inset * 2 - 1, bounds.Height - inset * 2 - 1);
        }
    }

    /// <summary>Draws a radio button with explicit coordinates.</summary>
    public static void DrawRadioButton(Graphics g, int x, int y, int width, int height, ButtonState state)
        => DrawRadioButton(g, new System.Drawing.Rectangle(x, y, width, height), state);

    // ── Scroll button ─────────────────────────────────────────────────────────

    /// <summary>Draws a scroll arrow button.</summary>
    public static void DrawScrollButton(Graphics g, System.Drawing.Rectangle bounds, ScrollButton button, ButtonState state)
    {
        DrawButton(g, bounds, state);

        using var arrowPen = new Pen(System.Drawing.Color.FromArgb(0x00, 0x00, 0x00));
        int cx = bounds.X + bounds.Width / 2;
        int cy = bounds.Y + bounds.Height / 2;
        int s  = Math.Min(bounds.Width, bounds.Height) / 4;

        switch (button)
        {
            case ScrollButton.Up:
                g.DrawLine(arrowPen, cx, cy - s, cx - s, cy + s);
                g.DrawLine(arrowPen, cx, cy - s, cx + s, cy + s);
                g.DrawLine(arrowPen, cx - s, cy + s, cx + s, cy + s);
                break;
            case ScrollButton.Down:
                g.DrawLine(arrowPen, cx, cy + s, cx - s, cy - s);
                g.DrawLine(arrowPen, cx, cy + s, cx + s, cy - s);
                g.DrawLine(arrowPen, cx - s, cy - s, cx + s, cy - s);
                break;
            case ScrollButton.Left:
                g.DrawLine(arrowPen, cx - s, cy, cx + s, cy - s);
                g.DrawLine(arrowPen, cx - s, cy, cx + s, cy + s);
                g.DrawLine(arrowPen, cx + s, cy - s, cx + s, cy + s);
                break;
            case ScrollButton.Right:
                g.DrawLine(arrowPen, cx + s, cy, cx - s, cy - s);
                g.DrawLine(arrowPen, cx + s, cy, cx - s, cy + s);
                g.DrawLine(arrowPen, cx - s, cy - s, cx - s, cy + s);
                break;
        }
    }

    /// <summary>Draws a scroll button with explicit coordinates.</summary>
    public static void DrawScrollButton(Graphics g, int x, int y, int width, int height,
        ScrollButton button, ButtonState state)
        => DrawScrollButton(g, new System.Drawing.Rectangle(x, y, width, height), button, state);

    // ── Size grip ─────────────────────────────────────────────────────────────

    /// <summary>Draws a size grip in the lower-right corner of the specified bounds.</summary>
    public static void DrawSizeGrip(Graphics g, System.Drawing.Color backColor, System.Drawing.Rectangle bounds)
    {
        using var light = new Pen(Light(backColor));
        using var dark  = new Pen(Dark(backColor));
        for (int i = 0; i < 3; i++)
        {
            int x = bounds.Right  - 2 - i * 4;
            int y = bounds.Bottom - 2 - i * 4;
            g.DrawLine(light, x - 1, bounds.Bottom - 2, bounds.Right - 2, y - 1);
            g.DrawLine(dark,  x,     bounds.Bottom - 2, bounds.Right - 2, y);
        }
    }

    /// <summary>Draws a size grip with explicit coordinates.</summary>
    public static void DrawSizeGrip(Graphics g, System.Drawing.Color backColor,
        int x, int y, int width, int height)
        => DrawSizeGrip(g, backColor, new System.Drawing.Rectangle(x, y, width, height));

    // ── Disabled string ───────────────────────────────────────────────────────

    /// <summary>
    /// Draws the specified string in a disabled state (etched look).
    /// </summary>
    public static void DrawStringDisabled(Graphics g, string s, Font font,
        System.Drawing.Color background, System.Drawing.RectangleF layoutRectangle,
        TextFormatFlags format = TextFormatFlags.Default)
    {
        // Draw highlight offset first, then dark shadow — classic Win32 disabled text (etched look).
        using var highlight = new SolidBrush((CanvasColor)Light(background));
        using var shadow    = new SolidBrush((CanvasColor)System.Drawing.Color.FromArgb(0x80, 0x80, 0x80));
        g.DrawString(s, font, highlight, (int)(layoutRectangle.X + 1), (int)(layoutRectangle.Y + 1));
        g.DrawString(s, font, shadow,    (int)layoutRectangle.X,       (int)layoutRectangle.Y);
    }
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


/// <summary>Specifies the type of scroll arrow to draw on a scroll bar.</summary>
public enum ScrollButton
{
    Up    = 0,
    Down  = 1,
    Left  = 2,
    Right = 3,
    Min   = 0,
    Max   = 3,
}
