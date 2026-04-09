using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

public class Label : Control
{
    public Label()
    {
        Width = 100;
        Height = 20;
        BackColor = Color.Transparent;
        ForeColor = Color.Black;
        Text = "Label";
    }

    public ContentAlignment TextAlign { get; set; } = ContentAlignment.TopLeft;
    public new bool AutoSize { get; set; } = false;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw background if not transparent
        if (BackColor != Color.Transparent)
        {
            using var bgBrush = new SolidBrush(BackColor);
            g.FillRectangle(bgBrush, 0, 0, Width, Height);
        }

        // Draw text
        if (!string.IsNullOrEmpty(Text))
        {
            var (x, y) = GetTextPosition();
            using var textBrush = new SolidBrush(ForeColor);
            g.DrawString(Text, "Arial", 12, textBrush, x, y);
        }

        base.OnPaint(e);
    }

    protected (int x, int y) GetTextPosition()
    {
        // Approximate character width and height
        const int charWidth = 7;
        const int charHeight = 14;
        const int baselineOffset = 2; // Offset to account for 'top' baseline in canvas

        var textWidth = Text.Length * charWidth;
        var textHeight = charHeight;

        var (baseX, baseY) = TextAlign switch
        {
            ContentAlignment.TopLeft => (0, 0),
            ContentAlignment.TopCenter => ((Width - textWidth) / 2, 0),
            ContentAlignment.TopRight => (Width - textWidth, 0),
            ContentAlignment.MiddleLeft => (0, (Height - textHeight) / 2),
            ContentAlignment.MiddleCenter => ((Width - textWidth) / 2, (Height - textHeight) / 2),
            ContentAlignment.MiddleRight => (Width - textWidth, (Height - textHeight) / 2),
            ContentAlignment.BottomLeft => (0, Height - textHeight),
            ContentAlignment.BottomCenter => ((Width - textWidth) / 2, Height - textHeight),
            ContentAlignment.BottomRight => (Width - textWidth, Height - textHeight),
            _ => (0, 0)
        };

        // Add baseline offset to Y coordinate for better vertical alignment
        return (baseX, baseY + baselineOffset);
    }
}

public enum ContentAlignment
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}
