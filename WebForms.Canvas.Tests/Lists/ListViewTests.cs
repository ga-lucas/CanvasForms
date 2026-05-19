using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class ListViewItemTests
{
    [Fact]
    public void Constructor_Text_SetsText()
    {
        var item = new ListViewItem("Hello");
        Assert.Equal("Hello", item.Text);
    }

    [Fact]
    public void Constructor_StringArray_SetsTextAndSubItems()
    {
        var item = new ListViewItem(new[] { "Name", "Value", "Description" });
        Assert.Equal("Name", item.Text);
        Assert.Equal(2, item.SubItems.Count);
        Assert.Equal("Value", item.SubItems[0].Text);
        Assert.Equal("Description", item.SubItems[1].Text);
    }

    [Fact]
    public void Checked_DefaultIsFalse()
    {
        var item = new ListViewItem("A");
        Assert.False(item.Checked);
    }

    [Fact]
    public void Selected_DefaultIsFalse()
    {
        var item = new ListViewItem("A");
        Assert.False(item.Selected);
    }

    [Fact]
    public void Tag_CanBeSet()
    {
        var item = new ListViewItem("A");
        item.Tag = "myTag";
        Assert.Equal("myTag", item.Tag);
    }

    [Fact]
    public void SubItems_Add_IncreasesCount()
    {
        var item = new ListViewItem("Root");
        item.SubItems.Add(new ListViewSubItem(item, "Sub1"));
        Assert.Equal(1, item.SubItems.Count);
    }

    [Fact]
    public void SubItems_AddByText_IncreasesCount()
    {
        var item = new ListViewItem("Root");
        item.SubItems.Add("Sub1");
        Assert.Equal(1, item.SubItems.Count);
        Assert.Equal("Sub1", item.SubItems[0].Text);
    }
}

public class ColumnHeaderTests
{
    [Fact]
    public void Constructor_Text_SetsText()
    {
        var col = new ColumnHeader("Name");
        Assert.Equal("Name", col.Text);
    }

    [Fact]
    public void DefaultWidth_Is60()
    {
        var col = new ColumnHeader();
        Assert.Equal(60, col.Width);
    }

    [Fact]
    public void DefaultTextAlign_IsLeft()
    {
        var col = new ColumnHeader();
        Assert.Equal(HorizontalAlignment.Left, col.TextAlign);
    }
}

public class ListViewCollectionTests
{
    [Fact]
    public void Items_Add_IncreasesCount()
    {
        var lv = new ListView();
        lv.Items.Add("Item1");
        Assert.Equal(1, lv.Items.Count);
    }

    [Fact]
    public void Items_AddListViewItem_IncreasesCount()
    {
        var lv = new ListView();
        lv.Items.Add(new ListViewItem("Item1"));
        Assert.Equal(1, lv.Items.Count);
    }

    [Fact]
    public void Items_Remove_DecreasesCount()
    {
        var lv = new ListView();
        var item = new ListViewItem("A");
        lv.Items.Add(item);
        lv.Items.Remove(item);
        Assert.Equal(0, lv.Items.Count);
    }

    [Fact]
    public void Items_Clear_EmptiesCollection()
    {
        var lv = new ListView();
        lv.Items.Add("A"); lv.Items.Add("B");
        lv.Items.Clear();
        Assert.Equal(0, lv.Items.Count);
    }

    [Fact]
    public void Items_Contains_ReturnsTrue_ForExistingItem()
    {
        var lv = new ListView();
        var item = new ListViewItem("A");
        lv.Items.Add(item);
        Assert.True(lv.Items.Contains(item));
    }

    [Fact]
    public void Columns_Add_AssignsIndex()
    {
        var lv = new ListView();
        lv.Columns.Add("Name", 100);
        lv.Columns.Add("Value", 80);
        Assert.Equal(0, lv.Columns[0].Index);
        Assert.Equal(1, lv.Columns[1].Index);
    }

    [Fact]
    public void Columns_Remove_DecreasesCount()
    {
        var lv = new ListView();
        var col = new ColumnHeader("Name");
        lv.Columns.Add(col);
        lv.Columns.Remove(col);
        Assert.Equal(0, lv.Columns.Count);
    }

    [Fact]
    public void Columns_Clear_EmptiesCollection()
    {
        var lv = new ListView();
        lv.Columns.Add("A"); lv.Columns.Add("B");
        lv.Columns.Clear();
        Assert.Equal(0, lv.Columns.Count);
    }
}

public class ListViewPropertyTests
{
    [Fact]
    public void DefaultView_IsDetails()
    {
        var lv = new ListView();
        Assert.Equal(View.Details, lv.View);
    }

    [Fact]
    public void View_RoundTrips()
    {
        var lv = new ListView();
        lv.View = View.LargeIcon;
        Assert.Equal(View.LargeIcon, lv.View);
    }

    [Fact]
    public void FullRowSelect_DefaultIsFalse()
    {
        var lv = new ListView();
        Assert.False(lv.FullRowSelect);
    }

    [Fact]
    public void FullRowSelect_RoundTrips()
    {
        var lv = new ListView();
        lv.FullRowSelect = true;
        Assert.True(lv.FullRowSelect);
    }

    [Fact]
    public void GridLines_DefaultIsFalse()
    {
        var lv = new ListView();
        Assert.False(lv.GridLines);
    }

    [Fact]
    public void CheckBoxes_DefaultIsFalse()
    {
        var lv = new ListView();
        Assert.False(lv.CheckBoxes);
    }

    [Fact]
    public void CheckBoxes_RoundTrips()
    {
        var lv = new ListView();
        lv.CheckBoxes = true;
        Assert.True(lv.CheckBoxes);
    }

    [Fact]
    public void Sorting_DefaultIsNone()
    {
        var lv = new ListView();
        Assert.Equal(SortOrder.None, lv.Sorting);
    }

    [Fact]
    public void Sorting_RoundTrips()
    {
        var lv = new ListView();
        lv.Sorting = SortOrder.Ascending;
        Assert.Equal(SortOrder.Ascending, lv.Sorting);
    }

    [Fact]
    public void MultiSelect_DefaultIsTrue()
    {
        var lv = new ListView();
        Assert.True(lv.MultiSelect);
    }

    [Fact]
    public void BorderStyle_DefaultIsFixed3D()
    {
        var lv = new ListView();
        Assert.Equal(BorderStyle.Fixed3D, lv.BorderStyle);
    }
}

public class ListViewCheckedItemsTests
{
    [Fact]
    public void CheckBoxes_Enabled_AllowsChecking()
    {
        var lv = new ListView();
        lv.CheckBoxes = true;
        var item = new ListViewItem("A");
        lv.Items.Add(item);
        item.Checked = true;
        Assert.True(item.Checked);
    }

    [Fact]
    public void Item_Selected_CanBeSetDirectly()
    {
        var lv = new ListView();
        var item = new ListViewItem("A");
        lv.Items.Add(item);
        item.Selected = true;
        Assert.True(item.Selected);
    }
}
