using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class BindingSourceDefaultsTests
{
    [Fact]
    public void DefaultCount_IsZero()
    {
        var bs = new BindingSource();
        Assert.Equal(0, bs.Count);
    }

    [Fact]
    public void DefaultPosition_IsMinusOne()
    {
        var bs = new BindingSource();
        Assert.Equal(-1, bs.Position);
    }

    [Fact]
    public void DefaultCurrent_IsNull()
    {
        var bs = new BindingSource();
        Assert.Null(bs.Current);
    }

    [Fact]
    public void DefaultFilter_IsEmpty()
    {
        var bs = new BindingSource();
        Assert.Equal(string.Empty, bs.Filter);
    }

    [Fact]
    public void DefaultSort_IsEmpty()
    {
        var bs = new BindingSource();
        Assert.Equal(string.Empty, bs.Sort);
    }
}

public class BindingSourceMutationTests
{
    [Fact]
    public void Add_IncreasesCount()
    {
        var bs = new BindingSource();
        bs.Add("item1");
        bs.Add("item2");
        Assert.Equal(2, bs.Count);
    }

    [Fact]
    public void Add_FirstItem_SetsPositionToZero()
    {
        var bs = new BindingSource();
        bs.Add("item1");
        Assert.Equal(0, bs.Position);
    }

    [Fact]
    public void Add_FiresListChanged_ItemAdded()
    {
        var bs = new BindingSource();
        ListChangedType? changeType = null;
        bs.ListChanged += (_, e) => changeType = e.ListChangedType;
        bs.Add("item1");
        Assert.Equal(ListChangedType.ItemAdded, changeType);
    }

    [Fact]
    public void Remove_DecreasesCount()
    {
        var bs = new BindingSource();
        bs.Add("item1");
        bs.Add("item2");
        bs.Remove("item1");
        Assert.Equal(1, bs.Count);
    }

    [Fact]
    public void RemoveAt_DecreasesCount()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B"); bs.Add("C");
        bs.RemoveAt(1);
        Assert.Equal(2, bs.Count);
    }

    [Fact]
    public void RemoveAt_FiresListChanged_ItemDeleted()
    {
        var bs = new BindingSource();
        bs.Add("A");
        ListChangedType? changeType = null;
        bs.ListChanged += (_, e) => changeType = e.ListChangedType;
        bs.RemoveAt(0);
        Assert.Equal(ListChangedType.ItemDeleted, changeType);
    }

    [Fact]
    public void Clear_EmptiesCollection()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B");
        bs.Clear();
        Assert.Equal(0, bs.Count);
    }

    [Fact]
    public void Clear_SetsPositionToMinusOne()
    {
        var bs = new BindingSource();
        bs.Add("A");
        bs.Clear();
        Assert.Equal(-1, bs.Position);
    }

    [Fact]
    public void Clear_FiresListChanged_Reset()
    {
        var bs = new BindingSource();
        bs.Add("A");
        ListChangedType? changeType = null;
        bs.ListChanged += (_, e) => changeType = e.ListChangedType;
        bs.Clear();
        Assert.Equal(ListChangedType.Reset, changeType);
    }
}

public class BindingSourceNavigationTests
{
    [Fact]
    public void Current_ReturnsItemAtPosition()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B"); bs.Add("C");
        bs.Position = 1;
        Assert.Equal("B", bs.Current);
    }

    [Fact]
    public void MoveNext_AdvancesPosition()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B");
        bs.Position = 0;
        bs.MoveNext();
        Assert.Equal(1, bs.Position);
    }

    [Fact]
    public void MoveNext_AtEnd_DoesNotAdvanceBeyond()
    {
        var bs = new BindingSource();
        bs.Add("A");
        bs.Position = 0;
        bs.MoveNext();
        Assert.Equal(0, bs.Position); // still 0 – already at end
    }

    [Fact]
    public void MovePrevious_DecrementsPosition()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B");
        bs.Position = 1;
        bs.MovePrevious();
        Assert.Equal(0, bs.Position);
    }

    [Fact]
    public void MovePrevious_AtStart_DoesNotGoBelowZero()
    {
        var bs = new BindingSource();
        bs.Add("A");
        bs.Position = 0;
        bs.MovePrevious();
        Assert.Equal(0, bs.Position);
    }

    [Fact]
    public void MoveFirst_SetsPositionToZero()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B"); bs.Add("C");
        bs.Position = 2;
        bs.MoveFirst();
        Assert.Equal(0, bs.Position);
    }

    [Fact]
    public void MoveLast_SetsPositionToLastIndex()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B"); bs.Add("C");
        bs.MoveLast();
        Assert.Equal(2, bs.Position);
    }

    [Fact]
    public void Position_Set_ClampsToValidRange()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B");
        bs.Position = 100;
        Assert.Equal(1, bs.Position); // clamped to last index
    }

    [Fact]
    public void Position_Changed_FiresPositionChanged()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B");
        bool fired = false;
        bs.PositionChanged += (_, _) => fired = true;
        bs.Position = 1;
        Assert.True(fired);
    }

    [Fact]
    public void Position_Changed_FiresCurrentChanged()
    {
        var bs = new BindingSource();
        bs.Add("A"); bs.Add("B");
        bool fired = false;
        bs.CurrentChanged += (_, _) => fired = true;
        bs.Position = 1;
        Assert.True(fired);
    }
}

public class BindingSourceDataSourceTests
{
    [Fact]
    public void DataSource_List_ExposesItems()
    {
        var list = new List<string> { "X", "Y", "Z" };
        var bs = new BindingSource { DataSource = list };
        Assert.Equal(3, bs.Count);
    }

    [Fact]
    public void DataSource_Set_FiresDataSourceChanged()
    {
        var bs = new BindingSource();
        bool fired = false;
        bs.DataSourceChanged += (_, _) => fired = true;
        bs.DataSource = new List<string> { "A" };
        Assert.True(fired);
    }

    [Fact]
    public void DataMember_Set_FiresDataMemberChanged()
    {
        var bs = new BindingSource();
        bool fired = false;
        bs.DataMemberChanged += (_, _) => fired = true;
        bs.DataMember = "SomeProperty";
        Assert.True(fired);
    }

    [Fact]
    public void Constructor_WithDataSourceAndMember_SetsProperties()
    {
        var list = new List<string> { "A", "B" };
        var bs = new BindingSource(list, string.Empty);
        Assert.Equal(2, bs.Count);
    }
}

public class BindingSourceFilterSortTests
{
    [Fact]
    public void Filter_Set_RoundTrips()
    {
        var bs = new BindingSource();
        bs.Filter = "Name = 'Test'";
        Assert.Equal("Name = 'Test'", bs.Filter);
    }

    [Fact]
    public void Filter_Set_FiresListChanged()
    {
        var bs = new BindingSource();
        ListChangedType? changeType = null;
        bs.ListChanged += (_, e) => changeType = e.ListChangedType;
        bs.Filter = "something";
        Assert.Equal(ListChangedType.Reset, changeType);
    }

    [Fact]
    public void Sort_Set_RoundTrips()
    {
        var bs = new BindingSource();
        bs.Sort = "Name ASC";
        Assert.Equal("Name ASC", bs.Sort);
    }

    [Fact]
    public void Sort_Set_FiresListChanged()
    {
        var bs = new BindingSource();
        ListChangedType? changeType = null;
        bs.ListChanged += (_, e) => changeType = e.ListChangedType;
        bs.Sort = "Name ASC";
        Assert.Equal(ListChangedType.Reset, changeType);
    }

    [Fact]
    public void ResetBindings_FiresListChanged_Reset()
    {
        var bs = new BindingSource();
        ListChangedType? changeType = null;
        bs.ListChanged += (_, e) => changeType = e.ListChangedType;
        bs.ResetBindings();
        Assert.Equal(ListChangedType.Reset, changeType);
    }

    [Fact]
    public void ResetCurrentItem_FiresListChanged_ItemChanged()
    {
        var bs = new BindingSource();
        bs.Add("A");
        ListChangedType? changeType = null;
        bs.ListChanged += (_, e) => changeType = e.ListChangedType;
        bs.ResetCurrentItem();
        Assert.Equal(ListChangedType.ItemChanged, changeType);
    }
}
