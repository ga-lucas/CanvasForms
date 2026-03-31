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
            buttonColor = Color.FromArgb(240, 240, 240);
            borderColor = Color.FromArgb(173, 173, 173);
        }
        else if (_isPressed)
        {
            buttonColor = Color.FromArgb(204, 228, 247);
            borderColor = Color.FromArgb(0, 84, 153);
        }
        else if (_isHovered)
        {
            buttonColor = Color.FromArgb(229, 241, 251);
            borderColor = Color.FromArgb(0, 120, 215);
        }
        else
        {
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
}
