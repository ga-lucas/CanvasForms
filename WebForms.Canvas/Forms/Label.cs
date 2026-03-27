using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

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
    public bool AutoSize { get; set; } = false;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw background if not transparent
        if (BackColor != Color.Transparent)
        {
            g.FillRectangle(new SolidBrush(BackColor), new Rectangle(0, 0, Width, Height));
        }

        // Draw text
        if (!string.IsNullOrEmpty(Text))
        {
            var (x, y) = GetTextPosition();
            g.DrawString(Text, x, y, ForeColor);
        }

        base.OnPaint(e);
    }

    private (int x, int y) GetTextPosition()
    {
        // Approximate character width and height
        const int charWidth = 7;
        const int charHeight = 14;

        var textWidth = Text.Length * charWidth;
        var textHeight = charHeight;

        return TextAlign switch
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
