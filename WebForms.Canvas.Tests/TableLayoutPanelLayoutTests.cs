using System.Windows.Forms;
using Canvas.Windows.Forms.Drawing;

namespace Canvas.Windows.Forms.Tests;

public class TableLayoutPanelLayoutTests
{
    [Fact]
    public void SetCellPosition_GetCellPosition_ShouldRoundTrip()
    {
        var tlp = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Width = 200,
            Height = 100,
            Padding = new Size(0, 0)
        };

        var child = new Panel { Width = 10, Height = 10 };
        tlp.Controls.Add(child);

        tlp.SetCellPosition(child, new TableLayoutPanelCellPosition(1, 1));
        var pos = tlp.GetCellPosition(child);

        Assert.Equal(1, pos.Column);
        Assert.Equal(1, pos.Row);
    }

    [Fact]
    public void SetCellPosition_ShouldBeReflectedInStaticGetRowGetColumn()
    {
        var tlp = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Width = 200,
            Height = 100
        };

        var child = new Panel { Width = 10, Height = 10 };
        tlp.Controls.Add(child);

        tlp.SetCellPosition(child, new TableLayoutPanelCellPosition(1, 0));

        Assert.Equal(1, TableLayoutPanel.GetColumn(child));
        Assert.Equal(0, TableLayoutPanel.GetRow(child));
    }

    [Fact]
    public void StaticSetRowSetColumn_ShouldBeReflectedInInstanceGetCellPosition()
    {
        var tlp = new TableLayoutPanel
        {
            ColumnCount = 3,
            RowCount = 3,
            Width = 300,
            Height = 200
        };

        var child = new Panel { Width = 10, Height = 10 };
        tlp.Controls.Add(child);

        TableLayoutPanel.SetColumn(child, 2);
        TableLayoutPanel.SetRow(child, 1);

        var pos = tlp.GetCellPosition(child);
        Assert.Equal(2, pos.Column);
        Assert.Equal(1, pos.Row);
    }

    [Fact]
    public void DockFill_ShouldSizeChildToCellMinusMargins()
    {
        var tlp = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            Width = 200,
            Height = 50,
            Padding = new Size(0, 0)
        };

        // Force a deterministic split.
        tlp.ColumnStyles[0].SizeType = SizeType.Absolute;
        tlp.ColumnStyles[0].Size = 100;
        tlp.ColumnStyles[1].SizeType = SizeType.Absolute;
        tlp.ColumnStyles[1].Size = 100;

        tlp.RowStyles[0].SizeType = SizeType.Absolute;
        tlp.RowStyles[0].Size = 50;

        var child = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Size(2, 2),
            Width = 1,
            Height = 1
        };

        tlp.Controls.Add(child);
        tlp.SetCellPosition(child, new TableLayoutPanelCellPosition(0, 0));

        tlp.PerformLayout();

        Assert.Equal(2, child.Left);
        Assert.Equal(2, child.Top);
        Assert.Equal(100 - 4, child.Width);
        Assert.Equal(50 - 4, child.Height);
    }

    [Fact]
    public void AnchorLeftRight_ShouldStretchWidthWithinCell()
    {
        var tlp = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 1,
            Width = 120,
            Height = 40,
            Padding = new Size(0, 0)
        };

        tlp.ColumnStyles[0].SizeType = SizeType.Absolute;
        tlp.ColumnStyles[0].Size = 120;
        tlp.RowStyles[0].SizeType = SizeType.Absolute;
        tlp.RowStyles[0].Size = 40;

        var child = new Panel
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Size(3, 3),
            Width = 10,
            Height = 10
        };

        tlp.Controls.Add(child);
        TableLayoutPanel.SetColumn(child, 0);
        TableLayoutPanel.SetRow(child, 0);

        tlp.PerformLayout();

        Assert.Equal(3, child.Left);
        Assert.Equal(120 - 6, child.Width);
    }

    [Fact]
    public void GetControlFromPosition_ShouldReturnControlAtCell()
    {
        var tlp = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Width = 200,
            Height = 100
        };

        var child = new Panel { Width = 10, Height = 10 };
        tlp.Controls.Add(child);
        tlp.SetCellPosition(child, new TableLayoutPanelCellPosition(1, 1));

        Assert.Same(child, tlp.GetControlFromPosition(1, 1));
        Assert.Null(tlp.GetControlFromPosition(0, 0));
    }
}
