
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

    private const int CornerRadius = 4;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);
        var state = GetButtonState();

        // Determine button state colors
        Color colorTop;
        Color colorBottom;
        Color borderColor;

        switch (state)
        {
            case ButtonState.Disabled:
                colorTop    = Color.FromArgb(240, 240, 240);
                colorBottom = Color.FromArgb(240, 240, 240);
                borderColor = Color.FromArgb(173, 173, 173);
                break;

            case ButtonState.Pushed:
                colorTop    = DarkenColor(BackColor, 0.18f);
                colorBottom = DarkenColor(BackColor, 0.08f);
                borderColor = Color.FromArgb(0, 84, 153);
                break;

            case ButtonState.Hot:
                colorTop    = LightenColor(BackColor, 0.22f);
                colorBottom = LightenColor(BackColor, 0.08f);
                borderColor = Color.FromArgb(0, 120, 215);
                break;

            default: // Normal or Focused
                colorTop    = LightenColor(BackColor, 0.10f);
                colorBottom = DarkenColor(BackColor, 0.05f);
                borderColor = Color.FromArgb(173, 173, 173);
                break;
        }

        // Draw rounded gradient background
        var topLeft    = new Point(bounds.Left, bounds.Top);
        var bottomLeft = new Point(bounds.Left, bounds.Bottom);
        using var bgBrush = new LinearGradientBrush(topLeft, bottomLeft, colorTop, colorBottom);
        g.FillRoundRect(bgBrush, bounds, CornerRadius);

        // Draw rounded border
        using var borderPen = new Pen(borderColor);
        g.DrawRoundRect(borderPen, bounds, CornerRadius);

        // Draw text (centered)
        if (!string.IsNullOrEmpty(Text))
        {
            var textColor = Enabled ? ForeColor : System.Drawing.Color.FromArgb(109, 109, 109);

            var measureService = FindForm()?.TextMeasurementService;
            var fontSize = Font != null ? (int)Font.Size : 12;
            var fontFamily = Font?.Family ?? "Arial";
            var textWidth = measureService?.MeasureTextEstimate(Text, fontFamily, fontSize)
                            ?? (Text.Length * 7);
            var textHeight = measureService?.GetFontHeightEstimate(fontFamily, fontSize) ?? 14;
            var textX = (Width - textWidth) / 2;
            var textY = (Height - textHeight) / 2;

            using var textBrush = new SolidBrush(textColor);
            g.DrawString(Text, fontFamily, fontSize, textBrush, textX, textY);
        }

        // Draw rounded focus rectangle if focused
        if (Focused && Enabled)
        {
            var focusRect = new Rectangle(3, 3, Width - 6, Height - 6);
            using var focusPen = new Pen(Color.FromArgb(80, 80, 80)) { DashStyle = DashStyle.Dot };
            g.DrawRoundRect(focusPen, focusRect, CornerRadius - 1);
        }

        base.OnPaint(e);
    }
}
