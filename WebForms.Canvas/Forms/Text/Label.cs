
namespace System.Windows.Forms;

public class Label : Control
{
    public Label()
    {
        Width = 100;
        Height = 20;
        BackColor = System.Drawing.Color.Transparent;
        ForeColor = System.Drawing.Color.Black;
        Text = "Label";
    }

    public ContentAlignment TextAlign { get; set; } = ContentAlignment.TopLeft;
    public new bool AutoSize { get; set; } = false;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw background if not transparent
        if (BackColor != System.Drawing.Color.Transparent)
        {
            using var bgBrush = new SolidBrush(BackColor);
            g.FillRectangle(bgBrush, 0, 0, Width, Height);
        }

        // Draw text
        if (!string.IsNullOrEmpty(Text))
        {
            var lines = Text.Replace("\r", string.Empty).Split('\n');
            var (x0, y0, charHeight) = GetTextBlockPosition(lines);

            using var textBrush = new SolidBrush(ForeColor);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i] ?? string.Empty;
                var x = GetLineX(line);
                var y = y0 + (i * charHeight);
                g.DrawString(line, Font, textBrush, x, y);
            }
        }

        base.OnPaint(e);
    }

    protected (int x0, int y0, int charHeight) GetTextBlockPosition(string[] lines)
    {
        // Scale character metrics with the current font size.
        // ~0.6× width-to-height ratio is a reasonable proportional-font approximation.
        var charHeight  = Font.Height;                      // Font.Height already adds 2px inter-line
        var charWidth   = (int)Math.Round(Font.Size * 0.6f);
        const int baselineOffset = 2;

        var maxLineLen = 0;
        foreach (var l in lines)
            maxLineLen = Math.Max(maxLineLen, (l ?? string.Empty).Length);

        var textWidth  = maxLineLen * charWidth;
        var textHeight = lines.Length * charHeight;

        var (baseX, baseY) = TextAlign switch
        {
            ContentAlignment.TopLeft     => (0, 0),
            ContentAlignment.TopCenter   => ((Width - textWidth) / 2, 0),
            ContentAlignment.TopRight    => (Width - textWidth, 0),
            ContentAlignment.MiddleLeft  => (0, (Height - textHeight) / 2),
            ContentAlignment.MiddleCenter => ((Width - textWidth) / 2, (Height - textHeight) / 2),
            ContentAlignment.MiddleRight => (Width - textWidth, (Height - textHeight) / 2),
            ContentAlignment.BottomLeft  => (0, Height - textHeight),
            ContentAlignment.BottomCenter => ((Width - textWidth) / 2, Height - textHeight),
            ContentAlignment.BottomRight => (Width - textWidth, Height - textHeight),
            _ => (0, 0)
        };

        return (baseX, baseY + baselineOffset, charHeight);
    }

    protected int GetLineX(string line)
    {
        var charWidth = (int)Math.Round(Font.Size * 0.6f);
        var lineWidth = (line ?? string.Empty).Length * charWidth;

        return TextAlign switch
        {
            ContentAlignment.TopCenter or ContentAlignment.MiddleCenter or ContentAlignment.BottomCenter
                => Math.Max(0, (Width - lineWidth) / 2),
            ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight
                => Math.Max(0, Width - lineWidth),
            _ => 0
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
