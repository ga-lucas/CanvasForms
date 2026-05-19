using System.Linq;
using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class CheckedListBoxDefaultsTests
{
    [Fact]
    public void DefaultCheckOnClick_IsFalse()
    {
        var clb = new CheckedListBox();
        Assert.False(clb.CheckOnClick);
    }

    [Fact]
    public void DefaultThreeState_IsFalse()
    {
        var clb = new CheckedListBox();
        Assert.False(clb.ThreeState);
    }

    [Fact]
    public void DefaultBorderStyle_IsFixed3D()
    {
        var clb = new CheckedListBox();
        Assert.Equal(BorderStyle.Fixed3D, clb.BorderStyle);
    }
}

public class CheckedListBoxItemsTests
{
    [Fact]
    public void Items_Add_IncreasesCount()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("Alpha");
        clb.Items.Add("Beta");
        Assert.Equal(2, clb.Items.Count);
    }

    [Fact]
    public void Items_Clear_EmptiesCollection()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A"); clb.Items.Add("B");
        clb.Items.Clear();
        Assert.Equal(0, clb.Items.Count);
    }
}

public class CheckedListBoxCheckStateTests
{
    [Fact]
    public void GetItemCheckState_Default_IsUnchecked()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        Assert.Equal(CheckState.Unchecked, clb.GetItemCheckState(0));
    }

    [Fact]
    public void SetItemCheckState_Checked_UpdatesState()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        clb.SetItemCheckState(0, CheckState.Checked);
        Assert.Equal(CheckState.Checked, clb.GetItemCheckState(0));
    }

    [Fact]
    public void SetItemCheckState_Unchecked_UpdatesState()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        clb.SetItemCheckState(0, CheckState.Checked);
        clb.SetItemCheckState(0, CheckState.Unchecked);
        Assert.Equal(CheckState.Unchecked, clb.GetItemCheckState(0));
    }

    [Fact]
    public void SetItemChecked_True_SetsChecked()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        clb.SetItemChecked(0, true);
        Assert.Equal(CheckState.Checked, clb.GetItemCheckState(0));
    }

    [Fact]
    public void SetItemChecked_False_SetsUnchecked()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        clb.SetItemChecked(0, true);
        clb.SetItemChecked(0, false);
        Assert.Equal(CheckState.Unchecked, clb.GetItemCheckState(0));
    }

    [Fact]
    public void GetItemChecked_ReflectsCheckState()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        clb.SetItemChecked(0, true);
        Assert.True(clb.GetItemChecked(0));
    }

    [Fact]
    public void ThreeState_False_Indeterminate_BecomesChecked()
    {
        var clb = new CheckedListBox();
        clb.ThreeState = false;
        clb.Items.Add("A");
        clb.SetItemCheckState(0, CheckState.Indeterminate);
        // When ThreeState=false, Indeterminate is coerced to Checked
        Assert.Equal(CheckState.Checked, clb.GetItemCheckState(0));
    }

    [Fact]
    public void ThreeState_True_AllowsIndeterminate()
    {
        var clb = new CheckedListBox();
        clb.ThreeState = true;
        clb.Items.Add("A");
        clb.SetItemCheckState(0, CheckState.Indeterminate);
        Assert.Equal(CheckState.Indeterminate, clb.GetItemCheckState(0));
    }

    [Fact]
    public void GetItemCheckState_OutOfRange_ThrowsArgumentOutOfRangeException()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        Assert.Throws<ArgumentOutOfRangeException>(() => clb.GetItemCheckState(5));
    }

    [Fact]
    public void SetItemCheckState_OutOfRange_ThrowsArgumentOutOfRangeException()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        Assert.Throws<ArgumentOutOfRangeException>(() => clb.SetItemCheckState(5, CheckState.Checked));
    }
}

public class CheckedListBoxCheckedCollectionTests
{
    [Fact]
    public void CheckedIndices_ReflectsCheckedItems()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A"); clb.Items.Add("B"); clb.Items.Add("C");
        clb.SetItemChecked(0, true);
        clb.SetItemChecked(2, true);
        Assert.Equal(2, clb.CheckedIndices.Count);
        // Verify both checked indices are represented using the integer indexer
        var indices = Enumerable.Range(0, clb.CheckedIndices.Count).Select(i => clb.CheckedIndices[i]).ToList();
        Assert.Contains(0, indices);
        Assert.Contains(2, indices);
    }

    [Fact]
    public void CheckedItems_ReflectsCheckedObjects()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A"); clb.Items.Add("B");
        clb.SetItemChecked(1, true);
        Assert.Equal(1, clb.CheckedItems.Count);
        Assert.Equal("B", clb.CheckedItems[0]);
    }

    [Fact]
    public void ItemCheck_Event_FiresOnStateChange()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        bool fired = false;
        clb.ItemCheck += (_, _) => fired = true;
        clb.SetItemChecked(0, true);
        Assert.True(fired);
    }

    [Fact]
    public void ItemCheck_Event_ReportsOldAndNewState()
    {
        var clb = new CheckedListBox();
        clb.Items.Add("A");
        CheckState? oldState = null, newState = null;
        clb.ItemCheck += (_, e) => { oldState = e.CurrentValue; newState = e.NewValue; };
        clb.SetItemChecked(0, true);
        Assert.Equal(CheckState.Unchecked, oldState);
        Assert.Equal(CheckState.Checked, newState);
    }
}
