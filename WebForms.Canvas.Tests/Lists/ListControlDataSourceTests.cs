using System.ComponentModel;
using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

// ── Helpers ───────────────────────────────────────────────────────────────────

file class Person
{
    public string Name  { get; set; } = "";
    public int    Id    { get; set; }
    public override string ToString() => Name;
}

// ── ListBox DataSource ────────────────────────────────────────────────────────

public class ListBoxDataSourceTests
{
    [Fact]
    public void DataSource_IEnumerable_PopulatesItems()
    {
        var lb = new ListBox();
        lb.DataSource = new[] { "Alpha", "Beta", "Gamma" };
        Assert.Equal(3, lb.Items.Count);
    }

    [Fact]
    public void DataSource_List_PopulatesItems()
    {
        var lb = new ListBox();
        lb.DataSource = new List<string> { "A", "B" };
        Assert.Equal(2, lb.Items.Count);
    }

    [Fact]
    public void DataSource_ObjectList_DisplayMember_ShowsProperty()
    {
        var lb = new ListBox();
        lb.DisplayMember = "Name";
        lb.DataSource = new List<Person>
        {
            new() { Name = "Alice", Id = 1 },
            new() { Name = "Bob",   Id = 2 }
        };
        // GetItemText is protected; verify via reflection
        var method = typeof(ListBox).GetMethod("GetItemText",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public)!;
        Assert.Equal("Alice", (string)method.Invoke(lb, new[] { lb.Items[0] })!);
        Assert.Equal("Bob",   (string)method.Invoke(lb, new[] { lb.Items[1] })!);
    }

    [Fact]
    public void DataSource_ValueMember_SelectedValue_ReturnsCorrectValue()
    {
        var lb = new ListBox();
        lb.DisplayMember = "Name";
        lb.ValueMember   = "Id";
        lb.DataSource = new List<Person>
        {
            new() { Name = "Alice", Id = 10 },
            new() { Name = "Bob",   Id = 20 }
        };
        lb.SelectedIndex = 1;
        Assert.Equal(20, lb.SelectedValue);
    }

    [Fact]
    public void DataSource_SelectedValue_Setter_ChangesSelectedIndex()
    {
        var lb = new ListBox();
        lb.ValueMember = "Id";
        lb.DataSource = new List<Person>
        {
            new() { Name = "Alice", Id = 10 },
            new() { Name = "Bob",   Id = 20 }
        };
        lb.SelectedValue = 10;
        Assert.Equal(0, lb.SelectedIndex);
    }

    [Fact]
    public void DataSource_Replace_RefreshesItems()
    {
        var lb = new ListBox();
        lb.DataSource = new[] { "X" };
        Assert.Equal(1, lb.Items.Count);

        lb.DataSource = new[] { "A", "B", "C" };
        Assert.Equal(3, lb.Items.Count);
    }

    [Fact]
    public void DataSource_Null_ClearsItems()
    {
        var lb = new ListBox();
        lb.DataSource = new[] { "A", "B" };
        lb.DataSource = null;
        Assert.Equal(0, lb.Items.Count);
    }

    [Fact]
    public void DataSource_BindingList_ListChanged_RefreshesItems()
    {
        var list = new BindingList<string> { "A", "B" };
        var lb   = new ListBox();
        lb.DataSource = list;
        Assert.Equal(2, lb.Items.Count);

        list.Add("C");
        Assert.Equal(3, lb.Items.Count);
    }

    [Fact]
    public void DataSource_BindingList_RemoveItem_RefreshesItems()
    {
        var list = new BindingList<string> { "A", "B", "C" };
        var lb   = new ListBox();
        lb.DataSource = list;

        list.RemoveAt(1);
        Assert.Equal(2, lb.Items.Count);
    }

    [Fact]
    public void DataSource_ChangeDataSource_UnsubscribesOldBindingList()
    {
        var list1 = new BindingList<string> { "A" };
        var list2 = new BindingList<string> { "X", "Y", "Z" };

        var lb = new ListBox();
        lb.DataSource = list1;
        lb.DataSource = list2;

        // Mutating list1 should no longer affect the control
        list1.Add("B");
        Assert.Equal(3, lb.Items.Count); // only list2 items
    }

    [Fact]
    public void DataSource_SelectedValue_NoValueMember_ReturnsItem()
    {
        var lb = new ListBox();
        lb.DataSource = new[] { "Alpha", "Beta" };
        lb.SelectedIndex = 0;
        Assert.Equal("Alpha", lb.SelectedValue);
    }

    [Fact]
    public void DataSource_SelectedValue_UnknownValue_SetsMinusOne()
    {
        var lb = new ListBox();
        lb.ValueMember = "Id";
        lb.DataSource = new List<Person> { new() { Name = "Alice", Id = 1 } };
        lb.SelectedValue = 999;
        Assert.Equal(-1, lb.SelectedIndex);
    }
}

// ── ComboBox DataSource ───────────────────────────────────────────────────────

public class ComboBoxDataSourceTests
{
    [Fact]
    public void DataSource_IEnumerable_PopulatesItems()
    {
        var cb = new ComboBox();
        cb.DataSource = new[] { "One", "Two", "Three" };
        Assert.Equal(3, cb.Items.Count);
    }

    [Fact]
    public void DataSource_DisplayMember_GetItemText()
    {
        var cb = new ComboBox();
        cb.DisplayMember = "Name";
        cb.DataSource = new List<Person>
        {
            new() { Name = "Carol", Id = 3 },
            new() { Name = "Dave",  Id = 4 }
        };
        // GetItemText is protected; verify via reflection
        var method = typeof(ComboBox).GetMethod("GetItemText",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public)!;
        Assert.Equal("Carol", (string)method.Invoke(cb, new[] { cb.Items[0] })!);
    }

    [Fact]
    public void DataSource_ValueMember_SelectedValue()
    {
        var cb = new ComboBox();
        cb.DisplayMember = "Name";
        cb.ValueMember   = "Id";
        cb.DataSource = new List<Person>
        {
            new() { Name = "Eve",  Id = 5 },
            new() { Name = "Frank", Id = 6 }
        };
        cb.SelectedIndex = 0;
        Assert.Equal(5, cb.SelectedValue);
    }

    [Fact]
    public void DataSource_BindingList_ListChanged_Updates()
    {
        var list = new BindingList<string> { "P", "Q" };
        var cb   = new ComboBox();
        cb.DataSource = list;

        list.Add("R");
        Assert.Equal(3, cb.Items.Count);
    }
}
