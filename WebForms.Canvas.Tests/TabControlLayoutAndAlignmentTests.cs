using System.Windows.Forms;

namespace Canvas.Windows.Forms.Tests;

public class TabControlLayoutAndAlignmentTests
{
    [Fact]
    public void MultilineTop_ShouldIncreaseDisplayRectangleY()
    {
        var tabs = new TabControl
        {
            Width = 200,
            Height = 150,
            Alignment = TabAlignment.Top,
            Multiline = true
        };

        tabs.TabPages.Add(new TabPage("A"));
        tabs.TabPages.Add(new TabPage("This is a long tab name"));
        tabs.TabPages.Add(new TabPage("Another long tab name"));

        tabs.PerformLayout();

        Assert.True(tabs.DisplayRectangle.Y > 0);
        Assert.True(tabs.DisplayRectangle.Height < tabs.Height);
    }

    [Fact]
    public void AlignmentLeft_ShouldMoveDisplayRectangleX()
    {
        var tabs = new TabControl
        {
            Width = 200,
            Height = 150,
            Alignment = TabAlignment.Left,
            Multiline = false
        };

        tabs.TabPages.Add(new TabPage("One"));
        tabs.TabPages.Add(new TabPage("Two"));

        tabs.PerformLayout();

        Assert.True(tabs.DisplayRectangle.X > 0);
        Assert.True(tabs.DisplayRectangle.Width < tabs.Width);
    }

    [Fact]
    public void AlignmentBottom_ShouldReduceDisplayRectangleHeight()
    {
        var tabs = new TabControl
        {
            Width = 200,
            Height = 150,
            Alignment = TabAlignment.Bottom,
            Multiline = false
        };

        tabs.TabPages.Add(new TabPage("One"));
        tabs.TabPages.Add(new TabPage("Two"));

        tabs.PerformLayout();

        Assert.True(tabs.DisplayRectangle.Height < tabs.Height);
    }

    [Fact]
    public void AddingTabPageViaControls_AddsToTabPages()
    {
        var tabs = new TabControl();
        var p1 = new TabPage("One");

        tabs.Controls.Add(p1);

        Assert.Equal(1, tabs.TabPages.Count);
        Assert.Same(p1, tabs.TabPages[0]);
        Assert.Equal(0, tabs.SelectedIndex);
    }

    [Fact]
    public void RemovingTabPageViaControls_RemovesFromTabPages()
    {
        var tabs = new TabControl();
        var p1 = new TabPage("One");
        var p2 = new TabPage("Two");

        tabs.Controls.Add(p1);
        tabs.Controls.Add(p2);

        Assert.Equal(2, tabs.TabPages.Count);

        tabs.Controls.Remove(p1);

        Assert.Equal(1, tabs.TabPages.Count);
        Assert.Same(p2, tabs.TabPages[0]);
    }

    [Fact]
    public void Insert_BeforeSelected_ShouldKeepSameTabSelected()
    {
        var tabs = new TabControl();
        var p1 = new TabPage("One");
        var p2 = new TabPage("Two");

        tabs.TabPages.Add(p1);
        tabs.TabPages.Add(p2);
        tabs.SelectedIndex = 1;

        var inserted = new TabPage("Inserted");
        tabs.TabPages.Insert(0, inserted);

        Assert.Same(p2, tabs.SelectedTab);
        Assert.Equal(2, tabs.SelectedIndex);
    }

    [Fact]
    public void RemoveAt_Selected_ShouldSelectNext()
    {
        var tabs = new TabControl();
        var p1 = new TabPage("One");
        var p2 = new TabPage("Two");
        var p3 = new TabPage("Three");

        tabs.TabPages.Add(p1);
        tabs.TabPages.Add(p2);
        tabs.TabPages.Add(p3);
        tabs.SelectedIndex = 1;

        tabs.TabPages.RemoveAt(1);

        Assert.Same(p3, tabs.SelectedTab);
        Assert.Equal(1, tabs.SelectedIndex);
    }

    [Fact]
    public void KeyIndexing_ShouldFindByName()
    {
        var tabs = new TabControl();
        var p1 = new TabPage("One") { Name = "general" };
        var p2 = new TabPage("Two") { Name = "details" };

        tabs.TabPages.Add(p1);
        tabs.TabPages.Add(p2);

        Assert.True(tabs.TabPages.ContainsKey("DETAILS"));
        Assert.Same(p2, tabs.TabPages["details"]);
        Assert.Equal(1, tabs.TabPages.IndexOfKey("details"));
    }
}
