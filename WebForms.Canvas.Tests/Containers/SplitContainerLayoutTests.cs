using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class SplitContainerLayoutTests
{
    [Fact]
    public void Vertical_SplitterDistance_IsCoerced_ByMinSizes()
    {
        var sc = new SplitContainer
        {
            Width = 200,
            Height = 100,
            Orientation = Orientation.Vertical,
            SplitterWidth = 4,
            Panel1MinSize = 25,
            Panel2MinSize = 25
        };

        sc.SplitterDistance = 0;
        sc.PerformLayout();
        Assert.Equal(25, sc.SplitterDistance);

        sc.SplitterDistance = 1000;
        sc.PerformLayout();
        Assert.Equal(200 - 4 - 25, sc.SplitterDistance);
    }

    [Fact]
    public void Vertical_Layout_AssignsPanelBounds()
    {
        var sc = new SplitContainer
        {
            Width = 200,
            Height = 100,
            Orientation = Orientation.Vertical,
            SplitterWidth = 4,
            SplitterDistance = 60,
            Panel1MinSize = 0,
            Panel2MinSize = 0
        };

        sc.PerformLayout();

        Assert.Equal(0, sc.Panel1.Left);
        Assert.Equal(0, sc.Panel1.Top);
        Assert.Equal(60, sc.Panel1.Width);
        Assert.Equal(100, sc.Panel1.Height);

        Assert.Equal(60 + 4, sc.Panel2.Left);
        Assert.Equal(0, sc.Panel2.Top);
        Assert.Equal(200 - (60 + 4), sc.Panel2.Width);
        Assert.Equal(100, sc.Panel2.Height);
    }

    [Fact]
    public void Horizontal_Layout_AssignsPanelBounds()
    {
        var sc = new SplitContainer
        {
            Width = 200,
            Height = 100,
            Orientation = Orientation.Horizontal,
            SplitterWidth = 4,
            SplitterDistance = 40,
            Panel1MinSize = 0,
            Panel2MinSize = 0
        };

        sc.PerformLayout();

        Assert.Equal(0, sc.Panel1.Left);
        Assert.Equal(0, sc.Panel1.Top);
        Assert.Equal(200, sc.Panel1.Width);
        Assert.Equal(40, sc.Panel1.Height);

        Assert.Equal(0, sc.Panel2.Left);
        Assert.Equal(40 + 4, sc.Panel2.Top);
        Assert.Equal(200, sc.Panel2.Width);
        Assert.Equal(100 - (40 + 4), sc.Panel2.Height);
    }

    [Fact]
    public void SplitterIncrement_SnapsSplitterDistance()
    {
        var sc = new SplitContainer
        {
            Width = 200,
            Height = 100,
            Orientation = Orientation.Vertical,
            SplitterWidth = 4,
            Panel1MinSize = 0,
            Panel2MinSize = 0,
            SplitterIncrement = 10
        };

        sc.SplitterDistance = 53;
        sc.PerformLayout();

        Assert.Equal(50, sc.SplitterDistance);
    }
}
