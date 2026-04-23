using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Canvas.Windows.Forms.Drawing;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripItem (abstract base) — tested via ToolStripMenuItem concrete subclass
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripItemTests
{
    // ── Properties ───────────────────────────────────────────────────────────────

    [Fact]
    public void Name_DefaultsToEmpty()
    {
        var item = new ToolStripMenuItem();
        Assert.Equal(string.Empty, item.Name);
    }

    [Fact]
    public void Name_RoundTrips()
    {
        var item = new ToolStripMenuItem { Name = "miFile" };
        Assert.Equal("miFile", item.Name);
    }

    [Fact]
    public void Tag_DefaultsToNull()
    {
        var item = new ToolStripMenuItem();
        Assert.Null(item.Tag);
    }

    [Fact]
    public void Tag_RoundTrips()
    {
        var item = new ToolStripMenuItem { Tag = 42 };
        Assert.Equal(42, item.Tag);
    }

    [Fact]
    public void Text_DefaultsToEmpty()
    {
        var item = new ToolStripMenuItem();
        Assert.Equal(string.Empty, item.Text);
    }

    [Fact]
    public void Text_RoundTrips()
    {
        var item = new ToolStripMenuItem { Text = "File" };
        Assert.Equal("File", item.Text);
    }

    [Fact]
    public void Text_NullAssignmentBecomesEmpty()
    {
        var item = new ToolStripMenuItem { Text = null! };
        Assert.Equal(string.Empty, item.Text);
    }

    [Fact]
    public void Enabled_DefaultsToTrue()
    {
        var item = new ToolStripMenuItem();
        Assert.True(item.Enabled);
    }

    [Fact]
    public void Enabled_RoundTrips()
    {
        var item = new ToolStripMenuItem { Enabled = false };
        Assert.False(item.Enabled);
    }

    [Fact]
    public void Visible_DefaultsToTrue()
    {
        var item = new ToolStripMenuItem();
        Assert.True(item.Visible);
    }

    [Fact]
    public void Visible_RoundTrips()
    {
        var item = new ToolStripMenuItem { Visible = false };
        Assert.False(item.Visible);
    }

    [Fact]
    public void Image_DefaultsToNull()
    {
        var item = new ToolStripMenuItem();
        Assert.Null(item.Image);
    }

    [Fact]
    public void Image_RoundTrips()
    {
        var img  = new Canvas.Windows.Forms.Drawing.Image();
        var item = new ToolStripMenuItem { Image = img };
        Assert.Same(img, item.Image);
    }

    [Fact]
    public void Owner_DefaultsToNull()
    {
        var item = new ToolStripMenuItem();
        Assert.Null(item.Owner);
    }

    [Fact]
    public void Owner_SetWhenAddedToStrip()
    {
        var strip = new ToolStrip();
        var item  = new ToolStripMenuItem("File");
        strip.Items.Add(item);
        Assert.Same(strip, item.Owner);
    }

    [Fact]
    public void Selected_DefaultsToFalse()
    {
        var item = new ToolStripMenuItem();
        Assert.False(item.Selected);
    }

    [Fact]
    public void OnMouseEnter_SetsSelectedTrue()
    {
        var item = new ToolStripMenuItem();
        item.OnMouseEnter(EventArgs.Empty);
        Assert.True(item.Selected);
    }

    [Fact]
    public void OnMouseLeave_SetsSelectedFalse()
    {
        var item = new ToolStripMenuItem();
        item.OnMouseEnter(EventArgs.Empty);
        item.OnMouseLeave(EventArgs.Empty);
        Assert.False(item.Selected);
    }

    [Fact]
    public void ForeColor_FallsBackToBlackWithNoOwner()
    {
        var item = new ToolStripMenuItem();
        Assert.Equal(System.Drawing.Color.Black, item.ForeColor);
    }

    [Fact]
    public void ForeColor_RoundTrips()
    {
        var item = new ToolStripMenuItem();
        item.ForeColor = System.Drawing.Color.Red;
        Assert.Equal(System.Drawing.Color.Red, item.ForeColor);
    }

    [Fact]
    public void BackColor_FallsBackToDefaultWithNoOwner()
    {
        var item = new ToolStripMenuItem();
        Assert.Equal(System.Drawing.Color.FromArgb(240, 240, 240), item.BackColor);
    }

    [Fact]
    public void BackColor_RoundTrips()
    {
        var item = new ToolStripMenuItem();
        item.BackColor = System.Drawing.Color.Navy;
        Assert.Equal(System.Drawing.Color.Navy, item.BackColor);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    [Fact]
    public void Click_FiredByOnClick()
    {
        var item  = new ToolStripMenuItem();
        int fired = 0;
        item.Click += (_, _) => fired++;
        item.PerformClick();
        Assert.Equal(1, fired);
    }

    [Fact]
    public void PerformClick_FiresClickEvent()
    {
        var item  = new ToolStripMenuItem();
        int fired = 0;
        item.Click += (_, _) => fired++;
        item.PerformClick();
        Assert.Equal(1, fired);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripSeparator
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripSeparatorTests
{
    [Fact]
    public void DefaultText_IsDash()
    {
        var sep = new ToolStripSeparator();
        Assert.Equal("-", sep.Text);
    }

    [Fact]
    public void DefaultEnabled_IsFalse()
    {
        var sep = new ToolStripSeparator();
        Assert.False(sep.Enabled);
    }

    [Fact]
    public void IsToolStripItem()
    {
        Assert.IsAssignableFrom<ToolStripItem>(new ToolStripSeparator());
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripItemCollection
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripItemCollectionTests
{
    private static ToolStripItemCollection MakeCollection()
        => new ToolStripItemCollection(null);

    [Fact]
    public void NewCollection_IsEmpty()
    {
        var col = MakeCollection();
        Assert.Equal(0, col.Count);
    }

    [Fact]
    public void IsReadOnly_IsFalse()
    {
        Assert.False(MakeCollection().IsReadOnly);
    }

    [Fact]
    public void Add_ToolStripItem_IncreasesCount()
    {
        var col  = MakeCollection();
        var item = new ToolStripMenuItem("A");
        col.Add(item);
        Assert.Equal(1, col.Count);
    }

    [Fact]
    public void Add_String_ReturnsToolStripMenuItemWithCorrectText()
    {
        var col    = MakeCollection();
        var result = col.Add("Open");
        Assert.IsType<ToolStripMenuItem>(result);
        Assert.Equal("Open", result.Text);
    }

    [Fact]
    public void AddRange_Array_AddsAll()
    {
        var col = MakeCollection();
        col.AddRange(new ToolStripItem[]
        {
            new ToolStripMenuItem("A"),
            new ToolStripMenuItem("B"),
            new ToolStripSeparator()
        });
        Assert.Equal(3, col.Count);
    }

    [Fact]
    public void AddRange_IEnumerable_AddsAll()
    {
        var col   = MakeCollection();
        var items = new List<ToolStripItem> { new ToolStripMenuItem("X"), new ToolStripMenuItem("Y") };
        col.AddRange(items);
        Assert.Equal(2, col.Count);
    }

    [Fact]
    public void Insert_AtIndex_PutsItemAtCorrectPosition()
    {
        var col = MakeCollection();
        col.Add("A");
        col.Add("C");
        col.Insert(1, new ToolStripMenuItem("B"));
        Assert.Equal("B", col[1].Text);
    }

    [Fact]
    public void Remove_ExistingItem_ReturnsTrueAndDecreasesCount()
    {
        var col  = MakeCollection();
        var item = new ToolStripMenuItem("X");
        col.Add(item);
        var removed = col.Remove(item);
        Assert.True(removed);
        Assert.Equal(0, col.Count);
    }

    [Fact]
    public void Remove_NonExistingItem_ReturnsFalse()
    {
        var col    = MakeCollection();
        var absent = new ToolStripMenuItem("Z");
        Assert.False(col.Remove(absent));
    }

    [Fact]
    public void RemoveAt_RemovesCorrectItem()
    {
        var col = MakeCollection();
        col.Add("A");
        col.Add("B");
        col.RemoveAt(0);
        Assert.Equal("B", col[0].Text);
    }

    [Fact]
    public void Clear_EmptiesCollection()
    {
        var col = MakeCollection();
        col.Add("A");
        col.Add("B");
        col.Clear();
        Assert.Equal(0, col.Count);
    }

    [Fact]
    public void Contains_ReturnsTrueForAddedItem()
    {
        var col  = MakeCollection();
        var item = new ToolStripMenuItem("A");
        col.Add(item);
        Assert.True(col.Contains(item));
    }

    [Fact]
    public void Contains_ReturnsFalseForAbsentItem()
    {
        var col = MakeCollection();
        Assert.False(col.Contains(new ToolStripMenuItem("Z")));
    }

    [Fact]
    public void IndexOf_ReturnsCorrectIndex()
    {
        var col  = MakeCollection();
        var item = new ToolStripMenuItem("B");
        col.Add(new ToolStripMenuItem("A"));
        col.Add(item);
        Assert.Equal(1, col.IndexOf(item));
    }

    [Fact]
    public void StringIndexer_FindsByName()
    {
        var col  = MakeCollection();
        var item = new ToolStripMenuItem { Name = "miSave", Text = "Save" };
        col.Add(item);
        Assert.Same(item, col["miSave"]);
    }

    [Fact]
    public void StringIndexer_ReturnsNullForMissingName()
    {
        var col = MakeCollection();
        Assert.Null(col["nope"]);
    }

    [Fact]
    public void Indexer_SetReplacesItem()
    {
        var col      = MakeCollection();
        col.Add("Old");
        var newItem  = new ToolStripMenuItem("New");
        col[0]       = newItem;
        Assert.Same(newItem, col[0]);
    }

    [Fact]
    public void CopyTo_FillsArray()
    {
        var col = MakeCollection();
        col.Add("A");
        col.Add("B");
        var arr = new ToolStripItem[2];
        col.CopyTo(arr, 0);
        Assert.Equal("A", arr[0].Text);
        Assert.Equal("B", arr[1].Text);
    }

    [Fact]
    public void Enumeration_IteratesAllItems()
    {
        var col = MakeCollection();
        col.Add("A");
        col.Add("B");
        col.Add("C");
        var texts = new List<string>();
        foreach (var item in col) texts.Add(item.Text);
        Assert.Equal(new[] { "A", "B", "C" }, texts);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStrip
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripTests
{
    [Fact]
    public void IsControl()
    {
        Assert.IsAssignableFrom<Control>(new ToolStrip());
    }

    [Fact]
    public void DefaultHeight_Is24()
    {
        Assert.Equal(24, new ToolStrip().Height);
    }

    [Fact]
    public void TabStop_DefaultsFalse()
    {
        Assert.False(new ToolStrip().TabStop);
    }

    [Fact]
    public void BackColor_DefaultIsLightGray()
    {
        Assert.Equal(System.Drawing.Color.FromArgb(240, 240, 240), new ToolStrip().BackColor);
    }

    [Fact]
    public void ForeColor_DefaultIsBlack()
    {
        Assert.Equal(System.Drawing.Color.Black, new ToolStrip().ForeColor);
    }

    [Fact]
    public void Items_NotNullOnFirstAccess()
    {
        Assert.NotNull(new ToolStrip().Items);
    }

    [Fact]
    public void Items_StartsEmpty()
    {
        Assert.Equal(0, new ToolStrip().Items.Count);
    }

    [Fact]
    public void Font_DefaultIsSegoeUI9()
    {
        var strip = new ToolStrip();
        Assert.Equal("Segoe UI", strip.Font.Family);
        Assert.Equal(9f, strip.Font.Size);
    }

    [Fact]
    public void Font_RoundTrips()
    {
        var strip = new ToolStrip();
        var font  = new Canvas.Windows.Forms.Drawing.Font("Arial", 12);
        strip.Font = font;
        Assert.Same(font, strip.Font);
    }

    [Fact]
    public void Font_NullAssignmentRestoresDefault()
    {
        var strip = new ToolStrip();
        strip.Font = null!;
        Assert.Equal("Segoe UI", strip.Font.Family);
    }

    [Fact]
    public void AddingItem_SetsOwner()
    {
        var strip = new ToolStrip();
        var item  = new ToolStripMenuItem("A");
        strip.Items.Add(item);
        Assert.Same(strip, item.Owner);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripDropDown
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripDropDownTests
{
    [Fact]
    public void InheritsToolStrip()
    {
        Assert.IsAssignableFrom<ToolStrip>(new ToolStripDropDown());
    }

    [Fact]
    public void IsVisible_DefaultsFalse()
    {
        Assert.False(new ToolStripDropDown().IsVisible);
    }

    [Fact]
    public void IsVisible_RoundTrips()
    {
        var dd = new ToolStripDropDown { IsVisible = true };
        Assert.True(dd.IsVisible);
    }

    [Fact]
    public void PopupLocation_DefaultsToOrigin()
    {
        var dd = new ToolStripDropDown();
        Assert.Equal(new Point(0, 0), dd.PopupLocation);
    }

    [Fact]
    public void PopupLocation_RoundTrips()
    {
        var dd  = new ToolStripDropDown { PopupLocation = new Point(50, 100) };
        Assert.Equal(new Point(50, 100), dd.PopupLocation);
    }

    [Fact]
    public void SourceItem_DefaultsToNull()
    {
        Assert.Null(new ToolStripDropDown().SourceItem);
    }

    // ── Geometry ─────────────────────────────────────────────────────────────

    [Fact]
    public void ComputeDropWidth_AtLeastMinDropWidth()
    {
        var dd = new ToolStripDropDown();
        Assert.True(dd.ComputeDropWidth() >= 140);
    }

    [Fact]
    public void ComputeDropWidth_GrowsWithLongItemText()
    {
        var narrow = new ToolStripDropDown();
        narrow.Items.Add("Hi");

        var wide = new ToolStripDropDown();
        wide.Items.Add("A very long menu item text label");

        Assert.True(wide.ComputeDropWidth() > narrow.ComputeDropWidth());
    }

    [Fact]
    public void ComputeDropHeight_EmptyDropDown_IsJustBorder()
    {
        var dd = new ToolStripDropDown();
        Assert.Equal(2, dd.ComputeDropHeight()); // BorderThick * 2
    }

    [Fact]
    public void ComputeDropHeight_TwoItems_IsCorrect()
    {
        var dd = new ToolStripDropDown();
        dd.Items.Add("A");
        dd.Items.Add("B");
        // 2 * BorderThick + 2 * ItemHeight = 2 + 44 = 46
        Assert.Equal(2 + 22 + 22, dd.ComputeDropHeight());
    }

    [Fact]
    public void ComputeDropHeight_SeparatorCountedAsSeparatorH()
    {
        var dd = new ToolStripDropDown();
        dd.Items.Add("A");
        dd.Items.Add(new ToolStripSeparator());
        dd.Items.Add("B");
        // 2 + 22 + 8 + 22 = 54
        Assert.Equal(2 + 22 + 8 + 22, dd.ComputeDropHeight());
    }

    [Fact]
    public void ComputeDropHeight_InvisibleItemsExcluded()
    {
        var dd   = new ToolStripDropDown();
        var item = new ToolStripMenuItem("A") { Visible = false };
        dd.Items.Add(item);
        Assert.Equal(2, dd.ComputeDropHeight()); // invisible item not counted
    }

    [Fact]
    public void GetDropDownBounds_ReturnsRectangleRelativeToOwner()
    {
        var dd = new ToolStripDropDown { PopupLocation = new Point(100, 50) };
        dd.Items.Add("A");
        var bounds = dd.GetDropDownBounds(ownerAbsLeft: 10, ownerAbsTop: 5);
        Assert.Equal(100 - 10, bounds.X);
        Assert.Equal(50  -  5, bounds.Y);
        Assert.Equal(dd.ComputeDropWidth(),  bounds.Width);
        Assert.Equal(dd.ComputeDropHeight(), bounds.Height);
    }

    // ── Hit-testing ──────────────────────────────────────────────────────────

    [Fact]
    public void GetItemIndexAt_ReturnsMinusOneForEmptyDropDown()
    {
        var dd = new ToolStripDropDown();
        Assert.Equal(-1, dd.GetItemIndexAt(5));
    }

    [Fact]
    public void GetItemIndexAt_FirstItemHitAtTop()
    {
        var dd = new ToolStripDropDown();
        dd.Items.Add("A");
        // BorderThick=1, ItemHeight=22 → first item occupies y=[1,23)
        Assert.Equal(0, dd.GetItemIndexAt(1));
        Assert.Equal(0, dd.GetItemIndexAt(22));
    }

    [Fact]
    public void GetItemIndexAt_SecondItemHitCorrectly()
    {
        var dd = new ToolStripDropDown();
        dd.Items.Add("A");
        dd.Items.Add("B");
        // Second item starts at y = 1 + 22 = 23
        Assert.Equal(1, dd.GetItemIndexAt(23));
        Assert.Equal(1, dd.GetItemIndexAt(44));
    }

    [Fact]
    public void GetItemIndexAt_SeparatorCountedCorrectly()
    {
        var dd = new ToolStripDropDown();
        dd.Items.Add("A");                      // y=[1,23)
        dd.Items.Add(new ToolStripSeparator()); // y=[23,31)
        dd.Items.Add("B");                      // y=[31,53)
        Assert.Equal(2, dd.GetItemIndexAt(31));
    }

    [Fact]
    public void GetItemIndexAt_BeyondAllItems_ReturnsMinus1()
    {
        var dd = new ToolStripDropDown();
        dd.Items.Add("A"); // ends at y=23
        Assert.Equal(-1, dd.GetItemIndexAt(999));
    }

    // ── Mouse interaction ────────────────────────────────────────────────────

    [Fact]
    public void HandleMouseMove_SetsSelectedOnHoveredItem()
    {
        var dd   = new ToolStripDropDown();
        var item = new ToolStripMenuItem("A");
        dd.Items.Add(item);
        dd.HandleMouseMove(0, 5); // inside first item
        Assert.True(item.Selected);
    }

    [Fact]
    public void HandleMouseMove_ClearsSelectionOnPreviousItem()
    {
        var dd = new ToolStripDropDown();
        var a  = new ToolStripMenuItem("A");
        var b  = new ToolStripMenuItem("B");
        dd.Items.Add(a);
        dd.Items.Add(b);
        dd.HandleMouseMove(0, 5);  // hover A
        dd.HandleMouseMove(0, 25); // hover B
        Assert.False(a.Selected);
        Assert.True(b.Selected);
    }

    [Fact]
    public void HandleMouseDown_FiresClickOnLeafItem()
    {
        var dd    = new ToolStripDropDown { IsVisible = true };
        var item  = new ToolStripMenuItem("Save");
        int fired = 0;
        item.Click += (_, _) => fired++;
        dd.Items.Add(item);
        dd.HandleMouseDown(0, 5);
        Assert.Equal(1, fired);
    }

    [Fact]
    public void HandleMouseDown_ClosesDropDownAfterLeafClick()
    {
        var dd   = new ToolStripDropDown { IsVisible = true };
        var item = new ToolStripMenuItem("Close me");
        dd.Items.Add(item);
        dd.HandleMouseDown(0, 5);
        Assert.False(dd.IsVisible);
    }

    [Fact]
    public void HandleMouseDown_DisabledItem_DoesNotFireClick()
    {
        var dd    = new ToolStripDropDown { IsVisible = true };
        var item  = new ToolStripMenuItem("Disabled") { Enabled = false };
        int fired = 0;
        item.Click += (_, _) => fired++;
        dd.Items.Add(item);
        dd.HandleMouseDown(0, 5);
        Assert.Equal(0, fired);
    }

    [Fact]
    public void HandleMouseDown_SeparatorDoesNotFireClick()
    {
        var dd  = new ToolStripDropDown { IsVisible = true };
        var sep = new ToolStripSeparator();
        dd.Items.Add(sep);
        // Should not throw
        dd.HandleMouseDown(0, 4);
    }

    [Fact]
    public void HandleMouseDown_ItemWithSubMenu_OpensSubMenu()
    {
        var dd     = new ToolStripDropDown { IsVisible = true, PopupLocation = new Point(0, 0) };
        var parent = new ToolStripMenuItem("Parent");
        parent.DropDownItems.Add("Child");
        dd.Items.Add(parent);
        dd.HandleMouseDown(0, 5);
        Assert.True(parent.DropDownIsOpen);
    }

    // ── CloseChain ───────────────────────────────────────────────────────────

    [Fact]
    public void CloseChain_SetsIsVisibleFalse()
    {
        var dd = new ToolStripDropDown { IsVisible = true };
        dd.CloseChain();
        Assert.False(dd.IsVisible);
    }

    [Fact]
    public void CloseChain_ClosesOpenSubMenus()
    {
        var root  = new ToolStripDropDown { IsVisible = true };
        var mi    = new ToolStripMenuItem("Sub");
        mi.DropDownItems.Add("Child");
        root.Items.Add(mi);
        mi.OpenDropDown(new Point(0, 0));
        Assert.True(mi.DropDownIsOpen);

        root.CloseChain();
        Assert.False(mi.DropDownIsOpen);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripDropDownMenu
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripDropDownMenuTests
{
    [Fact]
    public void InheritsToolStripDropDown()
    {
        Assert.IsAssignableFrom<ToolStripDropDown>(new ToolStripDropDownMenu());
    }

    [Fact]
    public void CanAddItems()
    {
        var dd = new ToolStripDropDownMenu();
        dd.Items.Add("Test");
        Assert.Equal(1, dd.Items.Count);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripMenuItem
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripMenuItemTests
{
    // ── Constructors ─────────────────────────────────────────────────────────

    [Fact]
    public void DefaultCtor_TextIsEmpty()
    {
        Assert.Equal(string.Empty, new ToolStripMenuItem().Text);
    }

    [Fact]
    public void TextCtor_SetsText()
    {
        Assert.Equal("File", new ToolStripMenuItem("File").Text);
    }

    [Fact]
    public void TextImageCtor_SetsTextAndImage()
    {
        var img  = new Canvas.Windows.Forms.Drawing.Image();
        var item = new ToolStripMenuItem("Edit", img);
        Assert.Equal("Edit", item.Text);
        Assert.Same(img, item.Image);
    }

    [Fact]
    public void TextImageClickCtor_AttachesClickHandler()
    {
        int fired = 0;
        var item  = new ToolStripMenuItem("Exit", null, (_, _) => fired++);
        item.PerformClick();
        Assert.Equal(1, fired);
    }

    [Fact]
    public void TextImageDropDownItemsCtor_AddsChildren()
    {
        var child1 = new ToolStripMenuItem("A");
        var child2 = new ToolStripMenuItem("B");
        var item   = new ToolStripMenuItem("Parent", null, child1, child2);
        Assert.Equal(2, item.DropDownItems.Count);
        Assert.True(item.HasDropDownItems);
    }

    [Fact]
    public void TextImageClickShortcutCtor_SetsShortcut()
    {
        var item = new ToolStripMenuItem("Save", null, (_, _) => { }, Keys.Control | Keys.S);
        Assert.Equal(Keys.Control | Keys.S, item.ShortcutKeys);
    }

    // ── DropDown / DropDownItems ──────────────────────────────────────────────

    [Fact]
    public void DropDown_LazilyCreated_NotNull()
    {
        Assert.NotNull(new ToolStripMenuItem().DropDown);
    }

    [Fact]
    public void DropDown_IsToolStripDropDownMenu()
    {
        Assert.IsType<ToolStripDropDownMenu>(new ToolStripMenuItem().DropDown);
    }

    [Fact]
    public void DropDown_SourceItemSetToSelf()
    {
        var mi = new ToolStripMenuItem();
        Assert.Same(mi, mi.DropDown.SourceItem);
    }

    [Fact]
    public void DropDownItems_IsDropDownItemsCollection()
    {
        var mi = new ToolStripMenuItem();
        Assert.Same(mi.DropDown.Items, mi.DropDownItems);
    }

    [Fact]
    public void HasDropDownItems_FalseWhenEmpty()
    {
        Assert.False(new ToolStripMenuItem().HasDropDownItems);
    }

    [Fact]
    public void HasDropDownItems_TrueAfterAddingChild()
    {
        var mi = new ToolStripMenuItem();
        mi.DropDownItems.Add("Child");
        Assert.True(mi.HasDropDownItems);
    }

    [Fact]
    public void DropDownIsOpen_FalseByDefault()
    {
        Assert.False(new ToolStripMenuItem().DropDownIsOpen);
    }

    [Fact]
    public void OpenDropDown_SetsIsVisible()
    {
        var mi = new ToolStripMenuItem();
        mi.DropDownItems.Add("Child");
        mi.OpenDropDown(new Point(0, 0));
        Assert.True(mi.DropDownIsOpen);
    }

    [Fact]
    public void OpenDropDown_SetsPopupLocation()
    {
        var mi = new ToolStripMenuItem();
        mi.DropDownItems.Add("Child");
        mi.OpenDropDown(new Point(50, 100));
        Assert.Equal(new Point(50, 100), mi.DropDown.PopupLocation);
    }

    [Fact]
    public void OpenDropDown_DoesNothingWhenNoChildren()
    {
        var mi = new ToolStripMenuItem(); // no children
        mi.OpenDropDown(new Point(0, 0));
        Assert.False(mi.DropDownIsOpen);
    }

    [Fact]
    public void CloseDropDown_SetsIsVisibleFalse()
    {
        var mi = new ToolStripMenuItem();
        mi.DropDownItems.Add("Child");
        mi.OpenDropDown(new Point(0, 0));
        mi.CloseDropDown();
        Assert.False(mi.DropDownIsOpen);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    [Fact]
    public void DropDownOpening_FiredOnOpen()
    {
        var mi    = new ToolStripMenuItem();
        mi.DropDownItems.Add("Child");
        int fired = 0;
        mi.DropDownOpening += (_, _) => fired++;
        mi.OpenDropDown(new Point(0, 0));
        Assert.Equal(1, fired);
    }

    [Fact]
    public void DropDownOpened_FiredOnOpen()
    {
        var mi    = new ToolStripMenuItem();
        mi.DropDownItems.Add("Child");
        int fired = 0;
        mi.DropDownOpened += (_, _) => fired++;
        mi.OpenDropDown(new Point(0, 0));
        Assert.Equal(1, fired);
    }

    [Fact]
    public void DropDownClosed_FiredOnClose()
    {
        var mi    = new ToolStripMenuItem();
        mi.DropDownItems.Add("Child");
        int fired = 0;
        mi.DropDownClosed += (_, _) => fired++;
        mi.OpenDropDown(new Point(0, 0));
        mi.CloseDropDown();
        Assert.Equal(1, fired);
    }

    // ── Check state ───────────────────────────────────────────────────────────

    [Fact]
    public void Checked_DefaultsFalse()
    {
        Assert.False(new ToolStripMenuItem().Checked);
    }

    [Fact]
    public void Checked_RoundTrips()
    {
        var mi = new ToolStripMenuItem { Checked = true };
        Assert.True(mi.Checked);
    }

    [Fact]
    public void Checked_True_SetsCheckStateToChecked()
    {
        var mi = new ToolStripMenuItem { Checked = true };
        Assert.Equal(CheckState.Checked, mi.CheckState);
    }

    [Fact]
    public void Checked_False_SetsCheckStateToUnchecked()
    {
        var mi = new ToolStripMenuItem { Checked = true };
        mi.Checked = false;
        Assert.Equal(CheckState.Unchecked, mi.CheckState);
    }

    [Fact]
    public void CheckState_DefaultsUnchecked()
    {
        Assert.Equal(CheckState.Unchecked, new ToolStripMenuItem().CheckState);
    }

    [Fact]
    public void CheckState_Checked_SetsCheckedTrue()
    {
        var mi = new ToolStripMenuItem { CheckState = CheckState.Checked };
        Assert.True(mi.Checked);
    }

    [Fact]
    public void CheckState_Unchecked_SetsCheckedFalse()
    {
        var mi = new ToolStripMenuItem { CheckState = CheckState.Checked };
        mi.CheckState = CheckState.Unchecked;
        Assert.False(mi.Checked);
    }

    [Fact]
    public void CheckState_Indeterminate_DoesNotSetCheckedTrue()
    {
        var mi = new ToolStripMenuItem { CheckState = CheckState.Indeterminate };
        Assert.False(mi.Checked); // Indeterminate ≠ Checked
    }

    [Fact]
    public void CheckOnClick_DefaultsFalse()
    {
        Assert.False(new ToolStripMenuItem().CheckOnClick);
    }

    [Fact]
    public void CheckOnClick_True_TogglesCheckedOnClick()
    {
        var mi = new ToolStripMenuItem { CheckOnClick = true };
        mi.PerformClick();
        Assert.True(mi.Checked);
        mi.PerformClick();
        Assert.False(mi.Checked);
    }

    [Fact]
    public void CheckedChanged_FiredByCheckOnClick()
    {
        var mi    = new ToolStripMenuItem { CheckOnClick = true };
        int fired = 0;
        mi.CheckedChanged += (_, _) => fired++;
        mi.PerformClick();
        Assert.Equal(1, fired);
    }

    [Fact]
    public void CheckStateChanged_FiredByCheckOnClick()
    {
        var mi    = new ToolStripMenuItem { CheckOnClick = true };
        int fired = 0;
        mi.CheckStateChanged += (_, _) => fired++;
        mi.PerformClick();
        Assert.Equal(1, fired);
    }

    // ── Shortcut keys ─────────────────────────────────────────────────────────

    [Fact]
    public void ShortcutKeys_DefaultsToNone()
    {
        Assert.Equal(Keys.None, new ToolStripMenuItem().ShortcutKeys);
    }

    [Fact]
    public void ShortcutKeys_RoundTrips()
    {
        var mi = new ToolStripMenuItem { ShortcutKeys = Keys.Control | Keys.Z };
        Assert.Equal(Keys.Control | Keys.Z, mi.ShortcutKeys);
    }

    [Fact]
    public void ShowShortcutKeys_DefaultsTrue()
    {
        Assert.True(new ToolStripMenuItem().ShowShortcutKeys);
    }

    [Fact]
    public void ShowShortcutKeys_RoundTrips()
    {
        var mi = new ToolStripMenuItem { ShowShortcutKeys = false };
        Assert.False(mi.ShowShortcutKeys);
    }

    [Fact]
    public void ShortcutKeyDisplayString_DefaultsEmpty()
    {
        Assert.Equal(string.Empty, new ToolStripMenuItem().ShortcutKeyDisplayString);
    }

    [Fact]
    public void ShortcutKeyDisplayString_RoundTrips()
    {
        var mi = new ToolStripMenuItem { ShortcutKeyDisplayString = "Ctrl+Z" };
        Assert.Equal("Ctrl+Z", mi.ShortcutKeyDisplayString);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// MenuStrip
// ════════════════════════════════════════════════════════════════════════════════
public class MenuStripTests
{
    [Fact]
    public void InheritsToolStrip()
    {
        Assert.IsAssignableFrom<ToolStrip>(new MenuStrip());
    }

    [Fact]
    public void DefaultDock_IsTop()
    {
        Assert.Equal(DockStyle.Top, new MenuStrip().Dock);
    }

    [Fact]
    public void DefaultHeight_Is24()
    {
        Assert.Equal(24, new MenuStrip().Height);
    }

    [Fact]
    public void BackColor_DefaultIsLightGray()
    {
        Assert.Equal(System.Drawing.Color.FromArgb(240, 240, 240), new MenuStrip().BackColor);
    }

    [Fact]
    public void Items_StartsEmpty()
    {
        Assert.Equal(0, new MenuStrip().Items.Count);
    }

    [Fact]
    public void AddItem_IncreasesCount()
    {
        var ms = new MenuStrip();
        ms.Items.Add(new ToolStripMenuItem("File"));
        Assert.Equal(1, ms.Items.Count);
    }

    [Fact]
    public void AddMultipleItems_AllPresent()
    {
        var ms = new MenuStrip();
        ms.Items.Add(new ToolStripMenuItem("File"));
        ms.Items.Add(new ToolStripMenuItem("Edit"));
        ms.Items.Add(new ToolStripMenuItem("Help"));
        Assert.Equal(3, ms.Items.Count);
    }

    [Fact]
    public void ItemOwner_SetToMenuStrip()
    {
        var ms   = new MenuStrip();
        var item = new ToolStripMenuItem("File");
        ms.Items.Add(item);
        Assert.Same(ms, item.Owner);
    }

    [Fact]
    public void ClickOnLeafItem_WithNoDropDown_FiresClick()
    {
        var ms    = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 };
        var item  = new ToolStripMenuItem("About");
        int fired = 0;
        item.Click += (_, _) => fired++;
        ms.Items.Add(item);

        // Simulate click at x=12 (inside first item, 12 = ItemPadH offset),y=5
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5));
        Assert.Equal(1, fired);
    }

    [Fact]
    public void ClickOnItemWithDropDown_OpensDropDown()
    {
        var ms   = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 };
        var item = new ToolStripMenuItem("File");
        item.DropDownItems.Add("Open");
        ms.Items.Add(item);

        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5));
        Assert.True(item.DropDownIsOpen);
    }

    [Fact]
    public void ClickOnSameOpenItem_ClosesDropDown()
    {
        var ms   = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 };
        var item = new ToolStripMenuItem("File");
        item.DropDownItems.Add("Open");
        ms.Items.Add(item);

        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5)); // open
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5)); // close
        Assert.False(item.DropDownIsOpen);
    }

    [Fact]
    public void RightClick_DoesNotOpenDropDown()
    {
        var ms   = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 };
        var item = new ToolStripMenuItem("File");
        item.DropDownItems.Add("Open");
        ms.Items.Add(item);

        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Right, 1, 12, 5));
        Assert.False(item.DropDownIsOpen);
    }

    [Fact]
    public void DisabledItem_ClickDoesNotOpenDropDown()
    {
        var ms   = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 };
        var item = new ToolStripMenuItem("File") { Enabled = false };
        item.DropDownItems.Add("Open");
        ms.Items.Add(item);

        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5));
        Assert.False(item.DropDownIsOpen);
    }

    [Fact]
    public void MouseLeave_ClearsHover()
    {
        var ms = new MenuStrip { Width = 200, Height = 24 };
        ms.Items.Add(new ToolStripMenuItem("File"));
        ms.SimulateMouseMove(new MouseEventArgs(MouseButtons.None, 0, 10, 5));
        ms.SimulateMouseLeave(); // should not throw
    }

    [Fact]
    public void OpeningOneItem_ClosesOtherOpenItem()
    {
        var ms    = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 };
        var file  = new ToolStripMenuItem("File");
        var edit  = new ToolStripMenuItem("Edit");
        file.DropDownItems.Add("Open");
        edit.DropDownItems.Add("Copy");
        ms.Items.Add(file);
        ms.Items.Add(edit);

        // Open File (x=12)
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5));
        Assert.True(file.DropDownIsOpen);

        // Open Edit — File text width ~28px → Edit starts around x=52
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 55, 5));
        Assert.False(file.DropDownIsOpen);
        Assert.True(edit.DropDownIsOpen);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ContextMenuStrip
// ════════════════════════════════════════════════════════════════════════════════
public class ContextMenuStripTests
{
    [Fact]
    public void InheritsToolStripDropDownMenu()
    {
        Assert.IsAssignableFrom<ToolStripDropDownMenu>(new ContextMenuStrip());
    }

    [Fact]
    public void DefaultCtor_IsNotVisible()
    {
        Assert.False(new ContextMenuStrip().IsVisible);
    }

    [Fact]
    public void IContainerCtor_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
            new ContextMenuStrip(new System.ComponentModel.Container()));
        Assert.Null(exception);
    }

    // ── Open / Close ─────────────────────────────────────────────────────────

    [Fact]
    public void Open_SetsIsVisibleTrue()
    {
        var cms = new ContextMenuStrip();
        cms.Open();
        Assert.True(cms.IsVisible);
    }

    [Fact]
    public void Close_SetsIsVisibleFalse()
    {
        var cms = new ContextMenuStrip();
        cms.Open();
        cms.Close();
        Assert.False(cms.IsVisible);
    }

    [Fact]
    public void Opening_EventFiredOnOpen()
    {
        var cms   = new ContextMenuStrip();
        int fired = 0;
        cms.Opening += (_, _) => fired++;
        cms.Open();
        Assert.Equal(1, fired);
    }

    [Fact]
    public void Opened_EventFiredOnOpen()
    {
        var cms   = new ContextMenuStrip();
        int fired = 0;
        cms.Opened += (_, _) => fired++;
        cms.Open();
        Assert.Equal(1, fired);
    }

    [Fact]
    public void Closing_EventFiredOnClose()
    {
        var cms   = new ContextMenuStrip();
        int fired = 0;
        cms.Closing += (_, _) => fired++;
        cms.Open();
        cms.Close();
        Assert.Equal(1, fired);
    }

    [Fact]
    public void Closed_EventFiredOnClose()
    {
        var cms   = new ContextMenuStrip();
        int fired = 0;
        cms.Closed += (_, _) => fired++;
        cms.Open();
        cms.Close();
        Assert.Equal(1, fired);
    }

    [Fact]
    public void OpeningBeforeOpenedInCorrectOrder()
    {
        var cms   = new ContextMenuStrip();
        var order = new List<string>();
        cms.Opening += (_, _) => order.Add("opening");
        cms.Opened  += (_, _) => order.Add("opened");
        cms.Open();
        Assert.Equal(new[] { "opening", "opened" }, order);
    }

    [Fact]
    public void ClosingBeforeClosedInCorrectOrder()
    {
        var cms   = new ContextMenuStrip();
        var order = new List<string>();
        cms.Closing += (_, _) => order.Add("closing");
        cms.Closed  += (_, _) => order.Add("closed");
        cms.Open();
        cms.Close();
        Assert.Equal(new[] { "closing", "closed" }, order);
    }

    // ── Show overloads ────────────────────────────────────────────────────────

    [Fact]
    public void Show_XY_SetsPopupLocationAndOpens()
    {
        var cms = new ContextMenuStrip();
        cms.Show(30, 80);
        Assert.Equal(new Point(30, 80), cms.PopupLocation);
        Assert.True(cms.IsVisible);
    }

    [Fact]
    public void Show_ControlPoint_SetsPopupLocationAndOpens()
    {
        var control = new Panel { Left = 10, Top = 20 };
        var cms     = new ContextMenuStrip();
        cms.Show(control, new Point(5, 5));
        // With no parent, GetControlFormPosition returns (10,20)
        // so PopupLocation = (10+5, 20+5) = (15, 25)
        Assert.Equal(new Point(15, 25), cms.PopupLocation);
        Assert.True(cms.IsVisible);
    }

    [Fact]
    public void Show_ControlXY_SetsPopupLocationAndOpens()
    {
        var control = new Panel { Left = 0, Top = 0 };
        var cms     = new ContextMenuStrip();
        cms.Show(control, 50, 60);
        Assert.Equal(new Point(50, 60), cms.PopupLocation);
        Assert.True(cms.IsVisible);
    }

    // ── Items ────────────────────────────────────────────────────────────────

    [Fact]
    public void CanAddItems()
    {
        var cms = new ContextMenuStrip();
        cms.Items.Add("Cut");
        cms.Items.Add("Copy");
        cms.Items.Add("Paste");
        Assert.Equal(3, cms.Items.Count);
    }

    [Fact]
    public void ItemClickWhileOpen_FiresEventAndCloses()
    {
        var cms   = new ContextMenuStrip { PopupLocation = new Point(0, 0) };
        var item  = new ToolStripMenuItem("Cut");
        int fired = 0;
        item.Click += (_, _) => fired++;
        cms.Items.Add(item);
        cms.Open();

        // First item starts at y=BorderThick=1, hit at y=5
        cms.HandleMouseDown(0, 5);
        Assert.Equal(1, fired);
        Assert.False(cms.IsVisible);
    }

    [Fact]
    public void Close_AlsoCascadesOpenSubMenus()
    {
        var cms    = new ContextMenuStrip();
        var parent = new ToolStripMenuItem("Parent");
        parent.DropDownItems.Add("Child");
        cms.Items.Add(parent);
        cms.Open();
        parent.OpenDropDown(new Point(0, 0));
        Assert.True(parent.DropDownIsOpen);

        cms.Close();
        Assert.False(cms.IsVisible);
        Assert.False(parent.DropDownIsOpen);
    }

    // ── Control.ContextMenuStrip assignment ───────────────────────────────────

    [Fact]
    public void Control_ContextMenuStrip_RoundTrips()
    {
        var control = new Panel();
        var cms     = new ContextMenuStrip();
        control.ContextMenuStrip = cms;
        Assert.Same(cms, control.ContextMenuStrip);
    }

    [Fact]
    public void Control_ContextMenuStrip_CanBeNull()
    {
        var control = new Panel();
        control.ContextMenuStrip = null;
        Assert.Null(control.ContextMenuStrip);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// Integration: full menu structure
// ════════════════════════════════════════════════════════════════════════════════
public class MenuIntegrationTests
{
    [Fact]
    public void FullMenuHierarchy_CanBeConstructedAndQueried()
    {
        var ms   = new MenuStrip();
        var file = new ToolStripMenuItem("File");
        var open = new ToolStripMenuItem("Open");
        var save = new ToolStripMenuItem("Save");
        file.DropDownItems.Add(open);
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(save);
        ms.Items.Add(file);

        Assert.Equal(1, ms.Items.Count);
        Assert.Equal(3, file.DropDownItems.Count);
        Assert.True(file.HasDropDownItems);
        Assert.IsType<ToolStripSeparator>(file.DropDownItems[1]);
    }

    [Fact]
    public void NestedSubMenu_CanBeOpened()
    {
        var root   = new ToolStripMenuItem("Root");
        var sub    = new ToolStripMenuItem("Sub");
        var leaf   = new ToolStripMenuItem("Leaf");
        sub.DropDownItems.Add(leaf);
        root.DropDownItems.Add(sub);

        root.OpenDropDown(new Point(0, 0));
        Assert.True(root.DropDownIsOpen);

        // Simulate opening sub from root dropdown
        var rootDD = root.DropDown;
        rootDD.HandleMouseDown(0, 5); // click first item (sub), which has children → opens sub
        Assert.True(sub.DropDownIsOpen);
    }

    [Fact]
    public void CloseChainFromRoot_ClosesEntireTree()
    {
        var root = new ToolStripMenuItem("Root");
        var sub  = new ToolStripMenuItem("Sub");
        sub.DropDownItems.Add(new ToolStripMenuItem("Leaf"));
        root.DropDownItems.Add(sub);

        root.OpenDropDown(new Point(0, 0));
        sub.OpenDropDown(new Point(0, 0));
        Assert.True(root.DropDownIsOpen);
        Assert.True(sub.DropDownIsOpen);

        root.DropDown.CloseChain();
        Assert.False(root.DropDownIsOpen);
        Assert.False(sub.DropDownIsOpen);
    }

    [Fact]
    public void ContextMenuStrip_FullFlow_AddItemsOpenClickClose()
    {
        var cms   = new ContextMenuStrip();
        var cut   = new ToolStripMenuItem("Cut");
        var paste = new ToolStripMenuItem("Paste");
        int cuts  = 0;
        cut.Click += (_, _) => cuts++;
        cms.Items.Add(cut);
        cms.Items.Add(new ToolStripSeparator());
        cms.Items.Add(paste);

        cms.Show(100, 200);
        Assert.True(cms.IsVisible);
        Assert.Equal(new Point(100, 200), cms.PopupLocation);

        // Click "Cut" (y=5, inside first item)
        cms.HandleMouseDown(0, 5);
        Assert.Equal(1, cuts);
        Assert.False(cms.IsVisible);
    }

    [Fact]
    public void MenuStrip_ItemsAddedViaAddString_HaveCorrectText()
    {
        var ms = new MenuStrip();
        ms.Items.Add("File");
        ms.Items.Add("Edit");
        ms.Items.Add("View");
        Assert.Equal("File", ms.Items[0].Text);
        Assert.Equal("Edit", ms.Items[1].Text);
        Assert.Equal("View", ms.Items[2].Text);
    }

    [Fact]
    public void ToolStripItemCollection_AddRange_PreservesOrder()
    {
        var ms    = new MenuStrip();
        var items = new ToolStripItem[]
        {
            new ToolStripMenuItem("A"),
            new ToolStripMenuItem("B"),
            new ToolStripMenuItem("C"),
        };
        ms.Items.AddRange(items);
        Assert.Equal(new[] { "A", "B", "C" }, ms.Items.Select(i => i.Text).ToArray());
    }
}
