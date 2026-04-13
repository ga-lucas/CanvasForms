using System.Windows.Forms;
using Canvas.Windows.Forms.Drawing;

namespace Canvas.Windows.Forms.Tests;

public class FlowLayoutPanelLayoutTests
{
    [Fact]
    public void SetFlowBreak_ShouldWrapToNextRow_LeftToRight()
    {
        var p = new FlowLayoutPanel
        {
            Width = 100,
            Height = 100,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Size(0, 0)
        };

        var c1 = new Panel { Width = 40, Height = 10, Margin = new Size(0, 0) };
        var c2 = new Panel { Width = 40, Height = 10, Margin = new Size(0, 0) };

        p.Controls.Add(c1);
        p.Controls.Add(c2);

        FlowLayoutPanel.SetFlowBreak(c1, true);

        p.PerformLayout();

        Assert.Equal(0, c1.Left);
        Assert.Equal(0, c1.Top);
        Assert.Equal(0, c2.Left);
        Assert.True(c2.Top > c1.Top);
    }

    [Fact]
    public void SetFlowBreak_ShouldWrapToNextColumn_TopDown()
    {
        var p = new FlowLayoutPanel
        {
            Width = 100,
            Height = 40,
            WrapContents = true,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Size(0, 0)
        };

        var c1 = new Panel { Width = 10, Height = 20, Margin = new Size(0, 0) };
        var c2 = new Panel { Width = 10, Height = 20, Margin = new Size(0, 0) };

        p.Controls.Add(c1);
        p.Controls.Add(c2);

        FlowLayoutPanel.SetFlowBreak(c1, true);

        p.PerformLayout();

        Assert.Equal(0, c1.Left);
        Assert.Equal(0, c1.Top);
        Assert.True(c2.Left > c1.Left);
        Assert.Equal(0, c2.Top);
    }
}
