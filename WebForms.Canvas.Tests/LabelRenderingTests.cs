using Canvas.Windows.Forms.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Canvas.Windows.Forms.Tests;

public class LabelRenderingTests
{
    private sealed class TestLabel : Label
    {
        public void PaintTo(Graphics graphics)
        {
            var args = new PaintEventArgs(graphics, new Rectangle(0, 0, Width, Height));
            OnPaint(args);
        }
    }

    [Fact]
    public void OnPaint_TransparentBackColor_DoesNotEmitFillRect()
    {
        var label = new TestLabel
        {
            BackColor = System.Drawing.Color.Transparent,
            Width = 100,
            Height = 20,
            Text = string.Empty
        };

        using var g = new Graphics(label.Width, label.Height);
        label.PaintTo(g);

        var fillRects = g.GetCommands().OfType<FillRectangleCommand>().ToList();
        Assert.Empty(fillRects);
    }

    [Fact]
    public void OnPaint_NonTransparentBackColor_EmitsFillRectCoveringControl()
    {
        var backColor = System.Drawing.Color.FromArgb(10, 20, 30);

        var label = new TestLabel
        {
            BackColor = backColor,
            Width = 123,
            Height = 45,
            Text = string.Empty
        };

        using var g = new Graphics(label.Width, label.Height);
        label.PaintTo(g);

        var fillRect = Assert.Single(g.GetCommands().OfType<FillRectangleCommand>());

        Assert.Equal(0, fillRect.X);
        Assert.Equal(0, fillRect.Y);
        Assert.Equal(label.Width, fillRect.Width);
        Assert.Equal(label.Height, fillRect.Height);

        var brush = Assert.IsType<SolidBrush>(fillRect.Brush);
        Assert.Equal((Color)backColor, brush.Color);
    }

    [Fact]
    public void OnPaint_EmptyText_DoesNotEmitDrawText()
    {
        var label = new TestLabel
        {
            Width = 100,
            Height = 20,
            Text = ""
        };

        using var g = new Graphics(label.Width, label.Height);
        label.PaintTo(g);

        var texts = g.GetCommands().OfType<DrawStringCommand>().ToList();
        Assert.Empty(texts);
    }

    [Fact]
    public void OnPaint_TopLeftTextAlign_UsesExpectedPosition()
    {
        var label = new TestLabel
        {
            Width = 100,
            Height = 20,
            Text = "A",
            TextAlign = ContentAlignment.TopLeft
        };

        using var g = new Graphics(label.Width, label.Height);
        label.PaintTo(g);

        var text = Assert.Single(g.GetCommands().OfType<DrawStringCommand>());

        Assert.Equal("A", text.Text);
        Assert.Equal(0, text.X);
        Assert.Equal(2, text.Y); // baseline offset
    }

    [Fact]
    public void OnPaint_TopCenterTextAlign_CentersTextHorizontally()
    {
        var label = new TestLabel
        {
            Width = 100,
            Height = 20,
            Text = "AB",
            TextAlign = ContentAlignment.TopCenter
        };

        using var g = new Graphics(label.Width, label.Height);
        label.PaintTo(g);

        var text = Assert.Single(g.GetCommands().OfType<DrawStringCommand>());

        // textWidth = 2 * charWidth(5) = 10 => (100 - 10) / 2 = 45  (charWidth = round(Font.Size * 0.6) = round(9*0.6) = 5)
        Assert.Equal(45, text.X);
        Assert.Equal(2, text.Y);
    }

    [Fact]
    public void OnPaint_TopRightTextAlign_RightAlignsTextHorizontally()
    {
        var label = new TestLabel
        {
            Width = 100,
            Height = 20,
            Text = "AB",
            TextAlign = ContentAlignment.TopRight
        };

        using var g = new Graphics(label.Width, label.Height);
        label.PaintTo(g);

        var text = Assert.Single(g.GetCommands().OfType<DrawStringCommand>());

        // textWidth = 2 * charWidth(5) = 10 => 100 - 10 = 90
        Assert.Equal(90, text.X);
        Assert.Equal(2, text.Y);
    }

    [Fact]
    public void OnPaint_MiddleLeftTextAlign_CentersTextVertically()
    {
        var label = new TestLabel
        {
            Width = 100,
            Height = 40,
            Text = "A",
            TextAlign = ContentAlignment.MiddleLeft
        };

        using var g = new Graphics(label.Width, label.Height);
        label.PaintTo(g);

        var text = Assert.Single(g.GetCommands().OfType<DrawStringCommand>());

        // textHeight = 1 * charHeight(Font.Height=11) = 11 => (40 - 11) / 2 = 14; + baseline offset 2 => 16
        Assert.Equal(0, text.X);
        Assert.Equal(16, text.Y);
    }

    [Fact]
    public void OnPaint_MultiLineText_EmitsOneDrawTextPerLine_WithExpectedYOffsets()
    {
        var label = new TestLabel
        {
            Width = 100,
            Height = 60,
            Text = "A\nB",
            TextAlign = ContentAlignment.TopLeft
        };

        using var g = new Graphics(label.Width, label.Height);
        label.PaintTo(g);

        var texts = g.GetCommands().OfType<DrawStringCommand>().ToList();
        Assert.Equal(2, texts.Count);

        Assert.Equal("A", texts[0].Text);
        Assert.Equal(0, texts[0].X);
        Assert.Equal(2, texts[0].Y);

        Assert.Equal("B", texts[1].Text);
        Assert.Equal(0, texts[1].X);
        Assert.Equal(2 + 11, texts[1].Y);  // 2 (baseline offset) + charHeight (Font.Height = 11)
    }
}
