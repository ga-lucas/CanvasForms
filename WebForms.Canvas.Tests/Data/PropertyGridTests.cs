using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

// ── Test model types ──────────────────────────────────────────────────────────

file enum Direction { North, South, East, West }

file class Sample
{
    public bool      IsActive  { get; set; } = false;
    public Direction Direction { get; set; } = Direction.North;
    public string    Name      { get; set; } = "Test";
    public int       Count     { get; set; } = 42;
}

file class ReadOnlySample
{
    public bool      IsActive  { get; }        = true;
    public Direction Direction { get; }        = Direction.East;
}

// ── SelectedObject tests ───────────────────────────────────────────────────────

public class PropertyGridSelectedObjectTests
{
    [Fact]
    public void SelectedObject_Null_DoesNotThrow()
    {
        var pg = new PropertyGrid();
        pg.SelectedObject = null;
        Assert.Null(pg.SelectedObject);
    }

    [Fact]
    public void SelectedObject_Set_ReturnsValue()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample();
        pg.SelectedObject = obj;
        Assert.Same(obj, pg.SelectedObject);
    }

    [Fact]
    public void SelectedObjects_MultiSet_IncludesAll()
    {
        var pg   = new PropertyGrid();
        var a    = new Sample { Name = "A" };
        var b    = new Sample { Name = "B" };
        pg.SelectedObjects = new object[] { a, b };
        Assert.Equal(2, pg.SelectedObjects.Length);
    }

    [Fact]
    public void SelectedObject_Change_ClearsSelectedGridItem()
    {
        var pg = new PropertyGrid();
        pg.SelectedObject = new Sample();
        // SelectedGridItem is null after new object set (grid rebuilt)
        Assert.Null(pg.SelectedGridItem);
    }

    [Fact]
    public void PropertySort_Alphabetical_DoesNotThrow()
    {
        var pg = new PropertyGrid();
        pg.SelectedObject  = new Sample();
        pg.PropertySort    = PropertySort.Alphabetical;
        Assert.Equal(PropertySort.Alphabetical, pg.PropertySort);
    }

    [Fact]
    public void PropertySort_CategorizedAlphabetical_DoesNotThrow()
    {
        var pg = new PropertyGrid();
        pg.SelectedObject = new Sample();
        pg.PropertySort   = PropertySort.CategorizedAlphabetical;
        Assert.Equal(PropertySort.CategorizedAlphabetical, pg.PropertySort);
    }
}

// ── Bool toggle tests ─────────────────────────────────────────────────────────

public class PropertyGridBoolTests
{
    private static GridItem? FindProperty(PropertyGrid pg, string name)
    {
        // Walk visible items via SelectedGridItem assignment approach:
        // Since _flatRows is private we trigger the toggle via public API.
        // We can inspect SelectedGridItem after PropertyValueChanged fires.
        return null; // used only in the event-based tests below
    }

    [Fact]
    public void SelectedObject_WithBoolProperty_DoesNotThrow()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample { IsActive = false };
        pg.SelectedObject = obj; // must not throw
        Assert.NotNull(pg.SelectedObject);
    }

    [Fact]
    public void PropertyValueChanged_Bool_FiresOnToggle()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample { IsActive = false };
        pg.SelectedObject = obj;

        PropertyValueChangedEventArgs? fired = null;
        pg.PropertyValueChanged += (s, e) => fired = e;

        // Find the IsActive GridItem by simulating what OpenDropDown does:
        // directly build a GridItem referencing the property and call ToggleBool.
        var prop = typeof(Sample).GetProperty(nameof(Sample.IsActive))!;
        var item = new GridItem
        {
            Label        = "IsActive",
            Value        = false,
            PropertyInfo = prop,
            IsReadOnly   = false,
        };
        // OwnerObject is internal — use reflection to set it
        typeof(GridItem).GetProperty("OwnerObject",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(item, obj);

        // Call ToggleBool via reflection (private method)
        var toggleBool = typeof(PropertyGrid).GetMethod("ToggleBool",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        toggleBool.Invoke(pg, new object[] { item });

        Assert.NotNull(fired);
        Assert.Equal(false, fired!.OldValue);
        Assert.True(obj.IsActive);
    }

    [Fact]
    public void ToggleBool_FlipsValue_TrueToFalse()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample { IsActive = true };
        pg.SelectedObject = obj;

        var prop = typeof(Sample).GetProperty(nameof(Sample.IsActive))!;
        var item = new GridItem { Value = true, PropertyInfo = prop, IsReadOnly = false };
        typeof(GridItem).GetProperty("OwnerObject",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(item, obj);

        typeof(PropertyGrid).GetMethod("ToggleBool",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(pg, new object[] { item });

        Assert.False(obj.IsActive);
    }

    [Fact]
    public void ToggleBool_ReadOnly_DoesNotChangeValue()
    {
        var pg  = new PropertyGrid();
        var obj = new ReadOnlySample();
        pg.SelectedObject = obj;

        var prop = typeof(ReadOnlySample).GetProperty(nameof(ReadOnlySample.IsActive))!;
        var item = new GridItem { Value = true, PropertyInfo = prop, IsReadOnly = true };
        typeof(GridItem).GetProperty("OwnerObject",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(item, obj);

        // ToggleBool checks CanWrite — should not throw and should not change
        typeof(PropertyGrid).GetMethod("ToggleBool",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(pg, new object[] { item });

        Assert.True(obj.IsActive); // unchanged
    }
}

// ── Enum dropdown tests ───────────────────────────────────────────────────────

public class PropertyGridEnumTests
{
    private static void OpenDropDown(PropertyGrid pg, GridItem item, int row)
    {
        typeof(PropertyGrid).GetMethod("OpenDropDown",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(pg, new object[] { item, row });
    }

    private static void CommitDropDown(PropertyGrid pg)
    {
        typeof(PropertyGrid).GetMethod("CommitDropDown",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(pg, null);
    }

    private static GridItem MakeEnumItem(object owner, string propName, object currentValue)
    {
        var prop = owner.GetType().GetProperty(propName)!;
        var item = new GridItem { Value = currentValue, PropertyInfo = prop, IsReadOnly = false };
        typeof(GridItem).GetProperty("OwnerObject",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(item, owner);
        return item;
    }

    [Fact]
    public void OpenDropDown_Enum_PopulatesAllNames()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample { Direction = Direction.North };
        pg.SelectedObject = obj;

        var item = MakeEnumItem(obj, nameof(Sample.Direction), Direction.North);
        OpenDropDown(pg, item, 0);

        // _dropDownItems should contain all enum names
        var field = typeof(PropertyGrid).GetField("_dropDownItems",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var items = (string[])field.GetValue(pg)!;

        Assert.Equal(Enum.GetNames(typeof(Direction)), items);
    }

    [Fact]
    public void OpenDropDown_Enum_PreSelectsCurrentValue()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample { Direction = Direction.East };
        pg.SelectedObject = obj;

        var item = MakeEnumItem(obj, nameof(Sample.Direction), Direction.East);
        OpenDropDown(pg, item, 0);

        var selField = typeof(PropertyGrid).GetField("_dropDownSelected",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        int sel = (int)selField.GetValue(pg)!;

        var names = Enum.GetNames(typeof(Direction));
        Assert.Equal("East", names[sel]);
    }

    [Fact]
    public void CommitDropDown_Enum_ChangesObjectProperty()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample { Direction = Direction.North };
        pg.SelectedObject = obj;

        var item = MakeEnumItem(obj, nameof(Sample.Direction), Direction.North);
        // Simulate selecting "South" (index 1)
        OpenDropDown(pg, item, 0);
        typeof(PropertyGrid).GetField("_dropDownSelected",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(pg, 1); // South
        typeof(PropertyGrid).GetField("_selectedItem",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(pg, item);

        CommitDropDown(pg);

        Assert.Equal(Direction.South, obj.Direction);
    }

    [Fact]
    public void CommitDropDown_Enum_FiresPropertyValueChanged()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample { Direction = Direction.North };
        pg.SelectedObject = obj;

        PropertyValueChangedEventArgs? fired = null;
        pg.PropertyValueChanged += (s, e) => fired = e;

        var item = MakeEnumItem(obj, nameof(Sample.Direction), Direction.North);
        OpenDropDown(pg, item, 0);
        typeof(PropertyGrid).GetField("_dropDownSelected",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(pg, 2); // East
        typeof(PropertyGrid).GetField("_selectedItem",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(pg, item);

        CommitDropDown(pg);

        Assert.NotNull(fired);
        Assert.Equal(Direction.North, fired!.OldValue);
        Assert.Equal(Direction.East, obj.Direction);
    }

    [Fact]
    public void OpenDropDown_Bool_GivesTrueFalseItems()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample { IsActive = false };
        pg.SelectedObject = obj;

        var item = MakeEnumItem(obj, nameof(Sample.IsActive), false);
        OpenDropDown(pg, item, 0);

        var field = typeof(PropertyGrid).GetField("_dropDownItems",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var items = (string[])field.GetValue(pg)!;

        Assert.Equal(new[] { "True", "False" }, items);
    }

    [Fact]
    public void CommitDropDown_Bool_ChangesProperty()
    {
        var pg  = new PropertyGrid();
        var obj = new Sample { IsActive = false };
        pg.SelectedObject = obj;

        var item = MakeEnumItem(obj, nameof(Sample.IsActive), false);
        OpenDropDown(pg, item, 0);
        typeof(PropertyGrid).GetField("_dropDownSelected",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(pg, 0); // "True"
        typeof(PropertyGrid).GetField("_selectedItem",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(pg, item);

        CommitDropDown(pg);

        Assert.True(obj.IsActive);
    }

    [Fact]
    public void OpenDropDown_ReadOnly_DoesNotOpen()
    {
        var pg  = new PropertyGrid();
        var obj = new ReadOnlySample();
        pg.SelectedObject = obj;

        var prop = typeof(ReadOnlySample).GetProperty(nameof(ReadOnlySample.Direction))!;
        var item = new GridItem { Value = Direction.East, PropertyInfo = prop, IsReadOnly = true };
        typeof(GridItem).GetProperty("OwnerObject",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(item, obj);

        OpenDropDown(pg, item, 0);

        var openField = typeof(PropertyGrid).GetField("_dropDownOpen",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        Assert.False((bool)openField.GetValue(pg)!);
    }
}
