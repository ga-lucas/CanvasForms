using System.Windows.Forms;
using Canvas.Windows.Forms.Drawing;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

/// <summary>
/// Verifies that container Padding is respected during PerformLayout (dock and anchor).
/// </summary>
public class PaddingLayoutTests
{
    // ── Docking ───────────────────────────────────────────────────────────────

    [Fact]
    public void DockTop_WithPadding_ChildStartsAtPaddingTop()
    {
        var parent = new Panel { Width = 200, Height = 200, Padding = new Padding(10) };
        var child  = new Panel { Width = 0, Height = 30, Dock = DockStyle.Top };
        parent.Controls.Add(child);
        parent.PerformLayout();

        Assert.Equal(10, child.Top);
        Assert.Equal(10, child.Left);
        Assert.Equal(180, child.Width);   // 200 - 10 (left) - 10 (right)
    }

    [Fact]
    public void DockLeft_WithPadding_ChildStartsAtPaddingLeft()
    {
        var parent = new Panel { Width = 200, Height = 200, Padding = new Padding(8) };
        var child  = new Panel { Width = 40, Height = 0, Dock = DockStyle.Left };
        parent.Controls.Add(child);
        parent.PerformLayout();

        Assert.Equal(8, child.Left);
        Assert.Equal(8, child.Top);
        Assert.Equal(184, child.Height);  // 200 - 8 (top) - 8 (bottom)
    }

    [Fact]
    public void DockFill_WithPadding_ChildFillsPaddedArea()
    {
        var parent = new Panel { Width = 200, Height = 200, Padding = new Padding(5, 10, 5, 10) };
        var child  = new Panel { Dock = DockStyle.Fill };
        parent.Controls.Add(child);
        parent.PerformLayout();

        Assert.Equal(5,   child.Left);
        Assert.Equal(10,  child.Top);
        Assert.Equal(190, child.Width);   // 200 - 5 - 5
        Assert.Equal(180, child.Height);  // 200 - 10 - 10
    }

    [Fact]
    public void DockFill_NoPadding_ChildFillsEntireParent()
    {
        var parent = new Panel { Width = 200, Height = 200, Padding = Padding.Empty };
        var child  = new Panel { Dock = DockStyle.Fill };
        parent.Controls.Add(child);
        parent.PerformLayout();

        Assert.Equal(0,   child.Left);
        Assert.Equal(0,   child.Top);
        Assert.Equal(200, child.Width);
        Assert.Equal(200, child.Height);
    }

    // ── Anchoring ─────────────────────────────────────────────────────────────

    [Fact]
    public void AnchorDefault_WithPadding_OriginalPositionPreserved()
    {
        // With default Top|Left anchor, changing Padding on resize should not reposition the child.
        var parent = new Panel { Width = 200, Height = 200, Padding = new Padding(10) };
        var child  = new Panel { Left = 20, Top = 20, Width = 50, Height = 30 };
        parent.Controls.Add(child);
        parent.PerformLayout(); // snapshot OriginalBounds

        parent.Width  = 300;
        parent.Height = 300;
        parent.PerformLayout();

        // Only-left anchored: position unchanged from original
        Assert.Equal(20, child.Left);
        Assert.Equal(20, child.Top);
    }

    // ── DisplayRectangle ──────────────────────────────────────────────────────

    [Fact]
    public void DisplayRectangle_WithPadding_ReturnsInsetRect()
    {
        var panel = new Panel { Width = 200, Height = 100, Padding = new Padding(5, 8, 5, 8) };

        var dr = panel.DisplayRectangle;

        Assert.Equal(5,   dr.X);
        Assert.Equal(8,   dr.Y);
        Assert.Equal(190, dr.Width);   // 200 - 5 - 5
        Assert.Equal(84,  dr.Height);  // 100 - 8 - 8
    }

    [Fact]
    public void DisplayRectangle_NoPadding_EqualsClientRectangle()
    {
        var panel = new Panel { Width = 200, Height = 100, Padding = Padding.Empty };

        var dr = panel.DisplayRectangle;
        var cr = panel.ClientRectangle;

        Assert.Equal(cr.X,      dr.X);
        Assert.Equal(cr.Y,      dr.Y);
        Assert.Equal(cr.Width,  dr.Width);
        Assert.Equal(cr.Height, dr.Height);
    }

    // ── Asymmetric padding ────────────────────────────────────────────────────

    [Fact]
    public void DockBottom_WithAsymmetricPadding_CorrectPosition()
    {
        // Padding: left=5, top=0, right=5, bottom=20
        var parent = new Panel { Width = 200, Height = 200, Padding = new Padding(5, 0, 5, 20) };
        var child  = new Panel { Width = 0, Height = 30, Dock = DockStyle.Bottom };
        parent.Controls.Add(child);
        parent.PerformLayout();

        // clientRect starts at (5, 0, 190, 180) due to padding
        // DockBottom: top = clientRect.Y + clientRect.Height - child.Height = 0 + 180 - 30 = 150
        Assert.Equal(5,   child.Left);
        Assert.Equal(190, child.Width);
        Assert.Equal(150, child.Top);
    }
}
