using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class ComboBoxDefaultsTests
{
    [Fact]
    public void DefaultDropDownStyle_IsDropDown()
    {
        var cb = new ComboBox();
        Assert.Equal(ComboBoxStyle.DropDown, cb.DropDownStyle);
    }

    [Fact]
    public void DefaultSize_MatchesWinForms()
    {
        var cb = new ComboBox();
        Assert.Equal(121, cb.Width);
        Assert.Equal(23, cb.Height);
    }

    [Fact]
    public void DefaultSelectedIndex_IsMinusOne()
    {
        var cb = new ComboBox();
        Assert.Equal(-1, cb.SelectedIndex);
    }

    [Fact]
    public void DefaultMaxDropDownItems_IsEight()
    {
        var cb = new ComboBox();
        Assert.Equal(8, cb.MaxDropDownItems);
    }

    [Fact]
    public void DefaultDroppedDown_IsFalse()
    {
        var cb = new ComboBox();
        Assert.False(cb.DroppedDown);
    }
}

public class ComboBoxItemsTests
{
    [Fact]
    public void Items_Add_IncreasesCount()
    {
        var cb = new ComboBox();
        cb.Items.Add("One");
        cb.Items.Add("Two");
        Assert.Equal(2, cb.Items.Count);
    }

    [Fact]
    public void Items_AddRange_AddsAll()
    {
        var cb = new ComboBox();
        cb.Items.AddRange(new object[] { "A", "B", "C" });
        Assert.Equal(3, cb.Items.Count);
    }

    [Fact]
    public void Items_Remove_DecreasesCount()
    {
        var cb = new ComboBox();
        cb.Items.Add("X"); cb.Items.Add("Y");
        cb.Items.Remove("X");
        Assert.Equal(1, cb.Items.Count);
    }

    [Fact]
    public void Items_Clear_EmptiesCollection()
    {
        var cb = new ComboBox();
        cb.Items.Add("A"); cb.Items.Add("B");
        cb.Items.Clear();
        Assert.Equal(0, cb.Items.Count);
    }

    [Fact]
    public void Items_Insert_PlacesAtIndex()
    {
        var cb = new ComboBox();
        cb.Items.Add("A"); cb.Items.Add("C");
        cb.Items.Insert(1, "B");
        Assert.Equal("B", cb.Items[1]);
    }
}

public class ComboBoxSelectionTests
{
    [Fact]
    public void SelectedIndex_Set_UpdatesSelectedItem()
    {
        var cb = new ComboBox();
        cb.Items.Add("Alpha"); cb.Items.Add("Beta");
        cb.SelectedIndex = 1;
        Assert.Equal("Beta", cb.SelectedItem);
    }

    [Fact]
    public void SelectedIndex_Changed_FiresEvent()
    {
        var cb = new ComboBox();
        cb.Items.Add("A");
        bool fired = false;
        cb.SelectedIndexChanged += (_, _) => fired = true;
        cb.SelectedIndex = 0;
        Assert.True(fired);
    }

    [Fact]
    public void SelectedIndex_MinusOne_ClearsSelectedItem()
    {
        var cb = new ComboBox();
        cb.Items.Add("A");
        cb.SelectedIndex = 0;
        cb.SelectedIndex = -1;
        Assert.Null(cb.SelectedItem);
    }

    [Fact]
    public void SelectedItem_Set_UpdatesSelectedIndex()
    {
        var cb = new ComboBox();
        cb.Items.Add("Foo"); cb.Items.Add("Bar");
        cb.SelectedItem = "Bar";
        Assert.Equal(1, cb.SelectedIndex);
    }

    [Fact]
    public void DropDownList_Text_ReflectsSelectedItem()
    {
        var cb = new ComboBox();
        cb.DropDownStyle = ComboBoxStyle.DropDownList;
        cb.Items.Add("Apple"); cb.Items.Add("Orange");
        cb.SelectedIndex = 0;
        Assert.Equal("Apple", cb.Text);
    }

    [Fact]
    public void DropDown_Text_CanBeSetManually()
    {
        var cb = new ComboBox();
        cb.DropDownStyle = ComboBoxStyle.DropDown;
        cb.Text = "custom";
        Assert.Equal("custom", cb.Text);
    }
}

public class ComboBoxPropertyTests
{
    [Fact]
    public void DropDownStyle_RoundTrips()
    {
        var cb = new ComboBox();
        cb.DropDownStyle = ComboBoxStyle.Simple;
        Assert.Equal(ComboBoxStyle.Simple, cb.DropDownStyle);
    }

    [Fact]
    public void DropDownWidth_DefaultsToControlWidth()
    {
        var cb = new ComboBox();
        Assert.Equal(cb.Width, cb.DropDownWidth);
    }

    [Fact]
    public void DropDownWidth_CanBeOverridden()
    {
        var cb = new ComboBox();
        cb.DropDownWidth = 200;
        Assert.Equal(200, cb.DropDownWidth);
    }

    [Fact]
    public void MaxDropDownItems_Clamps_ToValidRange()
    {
        var cb = new ComboBox();
        cb.MaxDropDownItems = 0; // below minimum
        Assert.Equal(1, cb.MaxDropDownItems);
        cb.MaxDropDownItems = 200; // above maximum
        Assert.Equal(100, cb.MaxDropDownItems);
    }

    [Fact]
    public void DrawMode_RoundTrips()
    {
        var cb = new ComboBox();
        cb.DrawMode = DrawMode.OwnerDrawFixed;
        Assert.Equal(DrawMode.OwnerDrawFixed, cb.DrawMode);
    }

    [Fact]
    public void DroppedDown_Toggle_FiresDropDownAndDropDownClosed()
    {
        var cb = new ComboBox();
        cb.Items.Add("A");
        bool openFired = false, closedFired = false;
        cb.DropDown += (_, _) => openFired = true;
        cb.DropDownClosed += (_, _) => closedFired = true;
        cb.DroppedDown = true;
        cb.DroppedDown = false;
        Assert.True(openFired);
        Assert.True(closedFired);
    }

    [Fact]
    public void Sorted_RoundTrips()
    {
        var cb = new ComboBox();
        cb.Sorted = true;
        Assert.True(cb.Sorted);
    }
}
