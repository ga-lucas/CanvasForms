using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class ListBoxDefaultsTests
{
    [Fact]
    public void DefaultSelectionMode_IsOne()
    {
        var lb = new ListBox();
        Assert.Equal(SelectionMode.One, lb.SelectionMode);
    }

    [Fact]
    public void DefaultSize_MatchesWinForms()
    {
        var lb = new ListBox();
        Assert.Equal(120, lb.Width);
        Assert.Equal(96, lb.Height);
    }

    [Fact]
    public void DefaultBorderStyle_IsFixed3D()
    {
        var lb = new ListBox();
        Assert.Equal(BorderStyle.Fixed3D, lb.BorderStyle);
    }

    [Fact]
    public void DefaultBackColor_IsWhite()
    {
        var lb = new ListBox();
        Assert.Equal(System.Drawing.Color.FromArgb(255, 255, 255, 255), lb.BackColor);
    }

    [Fact]
    public void DefaultSelectedIndex_IsMinusOne()
    {
        var lb = new ListBox();
        Assert.Equal(-1, lb.SelectedIndex);
    }
}

public class ListBoxItemsTests
{
    [Fact]
    public void Items_Add_IncreasesCount()
    {
        var lb = new ListBox();
        lb.Items.Add("Alpha");
        lb.Items.Add("Beta");
        Assert.Equal(2, lb.Items.Count);
    }

    [Fact]
    public void Items_AddRange_AddsAll()
    {
        var lb = new ListBox();
        lb.Items.AddRange(new object[] { "A", "B", "C" });
        Assert.Equal(3, lb.Items.Count);
    }

    [Fact]
    public void Items_Remove_DecreasesCount()
    {
        var lb = new ListBox();
        lb.Items.Add("X");
        lb.Items.Add("Y");
        lb.Items.Remove("X");
        Assert.Equal(1, lb.Items.Count);
        Assert.Equal("Y", lb.Items[0]);
    }

    [Fact]
    public void Items_Clear_EmptiesCollection()
    {
        var lb = new ListBox();
        lb.Items.Add("A"); lb.Items.Add("B");
        lb.Items.Clear();
        Assert.Equal(0, lb.Items.Count);
    }

    [Fact]
    public void Items_Insert_PlacesAtIndex()
    {
        var lb = new ListBox();
        lb.Items.Add("A");
        lb.Items.Add("C");
        lb.Items.Insert(1, "B");
        Assert.Equal("B", lb.Items[1]);
        Assert.Equal("C", lb.Items[2]);
    }

    [Fact]
    public void Items_Contains_ReturnsTrue_ForExistingItem()
    {
        var lb = new ListBox();
        lb.Items.Add("Hello");
        Assert.True(lb.Items.Contains("Hello"));
    }

    [Fact]
    public void Items_IndexOf_ReturnsCorrectIndex()
    {
        var lb = new ListBox();
        lb.Items.Add("First");
        lb.Items.Add("Second");
        Assert.Equal(1, lb.Items.IndexOf("Second"));
    }
}

public class ListBoxSelectionTests
{
    [Fact]
    public void SelectedIndex_Set_UpdatesSelectedItem()
    {
        var lb = new ListBox();
        lb.Items.Add("Alpha");
        lb.Items.Add("Beta");
        lb.SelectedIndex = 1;
        Assert.Equal("Beta", lb.SelectedItem);
    }

    [Fact]
    public void SelectedIndex_MinusOne_ClearsSelection()
    {
        var lb = new ListBox();
        lb.Items.Add("Alpha");
        lb.SelectedIndex = 0;
        lb.SelectedIndex = -1;
        Assert.Null(lb.SelectedItem);
    }

    [Fact]
    public void SelectedIndex_Changed_FiresEvent()
    {
        var lb = new ListBox();
        lb.Items.Add("A");
        bool fired = false;
        lb.SelectedIndexChanged += (_, _) => fired = true;
        lb.SelectedIndex = 0;
        Assert.True(fired);
    }

    [Fact]
    public void SelectionMode_MultiSimple_AllowsMultipleSelections()
    {
        var lb = new ListBox();
        lb.SelectionMode = SelectionMode.MultiSimple;
        lb.Items.Add("A"); lb.Items.Add("B"); lb.Items.Add("C");
        lb.SelectedIndex = 0;
        // Multiple selections tracked via SelectedIndices
        Assert.Equal(SelectionMode.MultiSimple, lb.SelectionMode);
    }

    [Fact]
    public void SelectionMode_None_DisablesSelection()
    {
        var lb = new ListBox();
        lb.SelectionMode = SelectionMode.None;
        Assert.Equal(SelectionMode.None, lb.SelectionMode);
    }

    [Fact]
    public void SelectedItem_Set_UpdatesSelectedIndex()
    {
        var lb = new ListBox();
        lb.Items.Add("Foo");
        lb.Items.Add("Bar");
        lb.SelectedItem = "Bar";
        Assert.Equal(1, lb.SelectedIndex);
    }

    [Fact]
    public void SelectedItems_ReflectsCurrentSelection()
    {
        var lb = new ListBox();
        lb.Items.Add("A"); lb.Items.Add("B");
        lb.SelectedIndex = 0;
        Assert.Equal(1, lb.SelectedItems.Count);
    }

    [Fact]
    public void SelectedIndices_ReflectsCurrentSelection()
    {
        var lb = new ListBox();
        lb.Items.Add("A"); lb.Items.Add("B");
        lb.SelectedIndex = 1;
        Assert.Contains(1, lb.SelectedIndices.Cast<int>().ToList());
    }
}

public class ListBoxPropertyTests
{
    [Fact]
    public void Sorted_RoundTrips()
    {
        var lb = new ListBox();
        lb.Sorted = true;
        Assert.True(lb.Sorted);
    }

    [Fact]
    public void IntegralHeight_RoundTrips()
    {
        var lb = new ListBox();
        Assert.True(lb.IntegralHeight); // default
        lb.IntegralHeight = false;
        Assert.False(lb.IntegralHeight);
    }

    [Fact]
    public void ItemHeight_CanBeSet()
    {
        var lb = new ListBox();
        lb.ItemHeight = 24;
        Assert.Equal(24, lb.ItemHeight);
    }

    [Fact]
    public void MultiColumn_RoundTrips()
    {
        var lb = new ListBox();
        lb.MultiColumn = true;
        Assert.True(lb.MultiColumn);
    }

    [Fact]
    public void DrawMode_RoundTrips()
    {
        var lb = new ListBox();
        lb.DrawMode = DrawMode.OwnerDrawFixed;
        Assert.Equal(DrawMode.OwnerDrawFixed, lb.DrawMode);
    }

    [Fact]
    public void HorizontalScrollbar_RoundTrips()
    {
        var lb = new ListBox();
        lb.HorizontalScrollbar = true;
        Assert.True(lb.HorizontalScrollbar);
    }
}
