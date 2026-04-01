using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class Button : Control
{
    private bool _isPressed = false;
    private bool _isHovered = false;

    public Button()
    {
        Width = 75;
        Height = 23;
        BackColor = Color.FromArgb(240, 240, 240);
        ForeColor = Color.Black;
        Text = "Button";
    }

    public event EventHandler? Click;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);

        // Determine button state colors
        Color buttonColor;
        Color borderColor;

        if (!Enabled)
        {
            // Disabled: use a grayed-out version of BackColor or default gray
            buttonColor = Color.FromArgb(240, 240, 240);
            borderColor = Color.FromArgb(173, 173, 173);
        }
        else if (_isPressed)
        {
            // Pressed: darken the BackColor
            buttonColor = DarkenColor(BackColor, 0.15f);
            borderColor = Color.FromArgb(0, 84, 153);
        }
        else if (_isHovered)
        {
            // Hovered: lighten the BackColor
            buttonColor = LightenColor(BackColor, 0.15f);
            borderColor = Color.FromArgb(0, 120, 215);
        }
        else
        {
            // Normal state: use BackColor directly
            buttonColor = BackColor;
            borderColor = Color.FromArgb(173, 173, 173);
        }

        // Draw button background
        using var bgBrush = new SolidBrush(buttonColor);
        g.FillRectangle(bgBrush, bounds);

        // Draw border
        using var borderPen = new Pen(borderColor);
        g.DrawRectangle(borderPen, bounds);

        // Draw text (centered)
        if (!string.IsNullOrEmpty(Text))
        {
            var textColor = Enabled ? ForeColor : Color.FromArgb(109, 109, 109);

            // Simple text centering (approximate)
            var textX = (Width - (Text.Length * 7)) / 2;
            var textY = (Height - 14) / 2;

            using var textBrush = new SolidBrush(textColor);
            g.DrawString(Text, "Arial", 12, textBrush, textX, textY);
        }

        // Draw focus rectangle if focused
        if (Focused && Enabled)
        {
            var focusRect = new Rectangle(3, 3, Width - 6, Height - 6);
            using var focusPen = new Pen(Color.Black);
            // Note: Dotted style would be ideal but depends on Graphics implementation
            g.DrawRectangle(focusPen, focusRect);
        }

        base.OnPaint(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled && e.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        if (Enabled && _isPressed && e.Button == MouseButtons.Left)
        {
            _isPressed = false;

            // Check if mouse is still within bounds
            if (e.X >= 0 && e.X < Width && e.Y >= 0 && e.Y < Height)
            {
                OnClick(EventArgs.Empty);
            }

            Invalidate();
        }
        base.OnMouseUp(e);
    }

    protected internal override void OnMouseEnter(EventArgs e)
    {
        if (Enabled)
        {
            _isHovered = true;
            Invalidate();
        }
        base.OnMouseEnter(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_isHovered || _isPressed)
        {
            _isHovered = false;
            _isPressed = false;
            Invalidate();
        }
        base.OnMouseLeave(e);
    }

    protected virtual void OnClick(EventArgs e)
    {
        Click?.Invoke(this, e);
    }

    protected internal override void OnGotFocus(EventArgs e)
    {
        Invalidate();
        base.OnGotFocus(e);
    }

    protected internal override void OnLostFocus(EventArgs e)
    {
        Invalidate();
        base.OnLostFocus(e);
    }

    private static Color LightenColor(Color color, float amount)
    {
        // Lighten by blending with white
        var r = (int)(color.R + (255 - color.R) * amount);
        var g = (int)(color.G + (255 - color.G) * amount);
        var b = (int)(color.B + (255 - color.B) * amount);
        return Color.FromArgb(r, g, b);
    }

    private static Color DarkenColor(Color color, float amount)
    {
        // Darken by reducing RGB values
        var r = (int)(color.R * (1 - amount));
        var g = (int)(color.G * (1 - amount));
        var b = (int)(color.B * (1 - amount));
        return Color.FromArgb(r, g, b);
    }
}
