using WebForms.Canvas.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms button control
/// </summary>
public class Button : ButtonBase
{
    public Button()
    {
        Width = 75;
        Height = 23;
        BackColor = Color.FromArgb(240, 240, 240);
        ForeColor = Color.Black;
        Text = "Button";
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);
        var state = GetButtonState();

        // Determine button state colors
        Color buttonColor;
        Color borderColor;

        switch (state)
        {
            case ButtonState.Disabled:
                buttonColor = Color.FromArgb(240, 240, 240);
                borderColor = Color.FromArgb(173, 173, 173);
                break;

            case ButtonState.Pushed:
                buttonColor = DarkenColor(BackColor, 0.15f);
                borderColor = Color.FromArgb(0, 84, 153);
                break;

            case ButtonState.Hot:
                buttonColor = LightenColor(BackColor, 0.15f);
                borderColor = Color.FromArgb(0, 120, 215);
                break;

            default: // Normal or Focused
                buttonColor = BackColor;
                borderColor = Color.FromArgb(173, 173, 173);
                break;
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
            var textY = (Height - 14) / 2 + 2; // +2 for baseline offset

            using var textBrush = new SolidBrush(textColor);
            g.DrawString(Text, "Arial", 12, textBrush, textX, textY);
        }

        // Draw focus rectangle if focused
        if (Focused && Enabled)
        {
            var focusRect = new Rectangle(3, 3, Width - 6, Height - 6);
            using var focusPen = new Pen(Color.Black);
            g.DrawRectangle(focusPen, focusRect);
        }

        base.OnPaint(e);
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
}
