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
    [Fact] public void Name_DefaultsToEmpty() => Assert.Equal(string.Empty, new ToolStripMenuItem().Name);
    [Fact] public void Name_RoundTrips() { var i = new ToolStripMenuItem { Name = "miFile" }; Assert.Equal("miFile", i.Name); }
    [Fact] public void Tag_DefaultsToNull() => Assert.Null(new ToolStripMenuItem().Tag);
    [Fact] public void Tag_RoundTrips() { var i = new ToolStripMenuItem { Tag = 42 }; Assert.Equal(42, i.Tag); }
    [Fact] public void Text_DefaultsToEmpty() => Assert.Equal(string.Empty, new ToolStripMenuItem().Text);
    [Fact] public void Text_RoundTrips() { var i = new ToolStripMenuItem { Text = "File" }; Assert.Equal("File", i.Text); }
    [Fact] public void Text_NullAssignmentBecomesEmpty() { var i = new ToolStripMenuItem { Text = null! }; Assert.Equal(string.Empty, i.Text); }
    [Fact] public void Enabled_DefaultsToTrue() => Assert.True(new ToolStripMenuItem().Enabled);
    [Fact] public void Enabled_RoundTrips() { var i = new ToolStripMenuItem { Enabled = false }; Assert.False(i.Enabled); }
    [Fact] public void Visible_DefaultsToTrue() => Assert.True(new ToolStripMenuItem().Visible);
    [Fact] public void Visible_RoundTrips() { var i = new ToolStripMenuItem { Visible = false }; Assert.False(i.Visible); }
    [Fact] public void Image_DefaultsToNull() => Assert.Null(new ToolStripMenuItem().Image);
    [Fact] public void Image_RoundTrips() { var img = new Canvas.Windows.Forms.Drawing.Image(); var i = new ToolStripMenuItem { Image = img }; Assert.Same(img, i.Image); }
    [Fact] public void Owner_DefaultsToNull() => Assert.Null(new ToolStripMenuItem().Owner);

    [Fact]
    public void Owner_SetWhenAddedToStrip()
    {
        var strip = new ToolStrip();
        var item  = new ToolStripMenuItem("File");
        strip.Items.Add(item);
        Assert.Same(strip, item.Owner);
    }

    [Fact] public void Selected_DefaultsToFalse() => Assert.False(new ToolStripMenuItem().Selected);
    [Fact] public void OnMouseEnter_SetsSelectedTrue() { var i = new ToolStripMenuItem(); i.OnMouseEnter(EventArgs.Empty); Assert.True(i.Selected); }
    [Fact] public void OnMouseLeave_SetsSelectedFalse() { var i = new ToolStripMenuItem(); i.OnMouseEnter(EventArgs.Empty); i.OnMouseLeave(EventArgs.Empty); Assert.False(i.Selected); }
    [Fact] public void ForeColor_FallsBackToBlackWithNoOwner() => Assert.Equal(System.Drawing.Color.Black, new ToolStripMenuItem().ForeColor);
    [Fact] public void ForeColor_RoundTrips() { var i = new ToolStripMenuItem(); i.ForeColor = System.Drawing.Color.Red; Assert.Equal(System.Drawing.Color.Red, i.ForeColor); }
    [Fact] public void BackColor_FallsBackToDefaultWithNoOwner() => Assert.Equal(System.Drawing.Color.FromArgb(240, 240, 240), new ToolStripMenuItem().BackColor);
    [Fact] public void BackColor_RoundTrips() { var i = new ToolStripMenuItem(); i.BackColor = System.Drawing.Color.Navy; Assert.Equal(System.Drawing.Color.Navy, i.BackColor); }
    [Fact] public void Click_FiredByOnClick() { var i = new ToolStripMenuItem(); int n = 0; i.Click += (_, _) => n++; i.PerformClick(); Assert.Equal(1, n); }
    [Fact] public void PerformClick_FiresClickEvent() { var i = new ToolStripMenuItem(); int n = 0; i.Click += (_, _) => n++; i.PerformClick(); Assert.Equal(1, n); }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripSeparator
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripSeparatorTests
{
    [Fact] public void DefaultText_IsDash() => Assert.Equal("-", new ToolStripSeparator().Text);
    [Fact] public void DefaultEnabled_IsFalse() => Assert.False(new ToolStripSeparator().Enabled);
    [Fact] public void IsToolStripItem() => Assert.IsAssignableFrom<ToolStripItem>(new ToolStripSeparator());
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripItemCollection
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripItemCollectionTests
{
    private static ToolStripItemCollection MakeCollection() => new ToolStripItemCollection(null);

    [Fact] public void NewCollection_IsEmpty() => Assert.Empty(MakeCollection());
    [Fact] public void IsReadOnly_IsFalse() => Assert.False(MakeCollection().IsReadOnly);

    [Fact]
    public void Add_ToolStripItem_IncreasesCount()
    {
        var col = MakeCollection(); col.Add(new ToolStripMenuItem("A")); Assert.Single(col);
    }

    [Fact]
    public void Add_String_ReturnsToolStripMenuItemWithCorrectText()
    {
        var result = MakeCollection().Add("Open");
        Assert.IsType<ToolStripMenuItem>(result);
        Assert.Equal("Open", result.Text);
    }

    [Fact]
    public void AddRange_Array_AddsAll()
    {
        var col = MakeCollection();
        col.AddRange(new ToolStripItem[] { new ToolStripMenuItem("A"), new ToolStripMenuItem("B"), new ToolStripSeparator() });
        Assert.Equal(3, col.Count);
    }

    [Fact]
    public void AddRange_IEnumerable_AddsAll()
    {
        var col = MakeCollection();
        col.AddRange(new List<ToolStripItem> { new ToolStripMenuItem("X"), new ToolStripMenuItem("Y") });
        Assert.Equal(2, col.Count);
    }

    [Fact]
    public void Insert_AtIndex_PutsItemAtCorrectPosition()
    {
        var col = MakeCollection(); col.Add("A"); col.Add("C");
        col.Insert(1, new ToolStripMenuItem("B"));
        Assert.Equal("B", col[1].Text);
    }

    [Fact]
    public void Remove_ExistingItem_ReturnsTrueAndDecreasesCount()
    {
        var col = MakeCollection(); var item = new ToolStripMenuItem("X"); col.Add(item);
        Assert.True(col.Remove(item)); Assert.Empty(col);
    }

    [Fact] public void Remove_NonExistingItem_ReturnsFalse() => Assert.False(MakeCollection().Remove(new ToolStripMenuItem("Z")));

    [Fact]
    public void RemoveAt_RemovesCorrectItem()
    {
        var col = MakeCollection(); col.Add("A"); col.Add("B"); col.RemoveAt(0);
        Assert.Equal("B", col[0].Text);
    }

    [Fact]
    public void Clear_EmptiesCollection()
    {
        var col = MakeCollection(); col.Add("A"); col.Add("B"); col.Clear(); Assert.Empty(col);
    }

    [Fact]
    public void Contains_ReturnsTrueForAddedItem()
    {
        var col = MakeCollection(); var item = new ToolStripMenuItem("A"); col.Add(item); Assert.Contains(item, col);
    }

    [Fact] public void Contains_ReturnsFalseForAbsentItem() => Assert.DoesNotContain(new ToolStripMenuItem("Z"), MakeCollection());

    [Fact]
    public void IndexOf_ReturnsCorrectIndex()
    {
        var col = MakeCollection(); var item = new ToolStripMenuItem("B"); col.Add(new ToolStripMenuItem("A")); col.Add(item);
        Assert.Equal(1, col.IndexOf(item));
    }

    [Fact]
    public void StringIndexer_FindsByName()
    {
        var col = MakeCollection(); var item = new ToolStripMenuItem { Name = "miSave", Text = "Save" }; col.Add(item);
        Assert.Same(item, col["miSave"]);
    }

    [Fact] public void StringIndexer_ReturnsNullForMissingName() => Assert.Null(MakeCollection()["nope"]);

    [Fact]
    public void Indexer_SetReplacesItem()
    {
        var col = MakeCollection(); col.Add("Old"); var newItem = new ToolStripMenuItem("New"); col[0] = newItem;
        Assert.Same(newItem, col[0]);
    }

    [Fact]
    public void CopyTo_FillsArray()
    {
        var col = MakeCollection(); col.Add("A"); col.Add("B");
        var arr = new ToolStripItem[2]; col.CopyTo(arr, 0);
        Assert.Equal("A", arr[0].Text); Assert.Equal("B", arr[1].Text);
    }

    [Fact]
    public void Enumeration_IteratesAllItems()
    {
        var col = MakeCollection(); col.Add("A"); col.Add("B"); col.Add("C");
        var texts = new List<string>(); foreach (var item in col) texts.Add(item.Text);
        Assert.Equal(new[] { "A", "B", "C" }, texts);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStrip
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripTests
{
    [Fact] public void IsControl() => Assert.IsAssignableFrom<Control>(new ToolStrip());
    [Fact] public void DefaultHeight_Is24() => Assert.Equal(24, new ToolStrip().Height);
    [Fact] public void TabStop_DefaultsFalse() => Assert.False(new ToolStrip().TabStop);
    [Fact] public void BackColor_DefaultIsLightGray() => Assert.Equal(System.Drawing.Color.FromArgb(240, 240, 240), new ToolStrip().BackColor);
    [Fact] public void ForeColor_DefaultIsBlack() => Assert.Equal(System.Drawing.Color.Black, new ToolStrip().ForeColor);
    [Fact] public void Items_NotNullOnFirstAccess() => Assert.NotNull(new ToolStrip().Items);
    [Fact] public void Items_StartsEmpty() => Assert.Empty(new ToolStrip().Items);
    [Fact] public void Font_DefaultIsSegoeUI9() { var s = new ToolStrip(); Assert.Equal("Segoe UI", s.Font.Family); Assert.Equal(9f, s.Font.Size); }
    [Fact] public void Font_RoundTrips() { var s = new ToolStrip(); var f = new Canvas.Windows.Forms.Drawing.Font("Arial", 12); s.Font = f; Assert.Same(f, s.Font); }
    [Fact] public void Font_NullAssignmentRestoresDefault() { var s = new ToolStrip(); s.Font = null!; Assert.Equal("Segoe UI", s.Font.Family); }

    [Fact]
    public void AddingItem_SetsOwner()
    {
        var strip = new ToolStrip(); var item = new ToolStripMenuItem("A"); strip.Items.Add(item);
        Assert.Same(strip, item.Owner);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripDropDown
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripDropDownTests
{
    [Fact] public void InheritsToolStrip() => Assert.IsAssignableFrom<ToolStrip>(new ToolStripDropDown());
    [Fact] public void IsVisible_DefaultsFalse() => Assert.False(new ToolStripDropDown().IsVisible);
    [Fact] public void IsVisible_RoundTrips() { var dd = new ToolStripDropDown { IsVisible = true }; Assert.True(dd.IsVisible); }
    [Fact] public void PopupLocation_DefaultsToOrigin() => Assert.Equal(new Point(0, 0), new ToolStripDropDown().PopupLocation);
    [Fact] public void PopupLocation_RoundTrips() { var dd = new ToolStripDropDown { PopupLocation = new Point(50, 100) }; Assert.Equal(new Point(50, 100), dd.PopupLocation); }
    [Fact] public void SourceItem_DefaultsToNull() => Assert.Null(new ToolStripDropDown().SourceItem);
    [Fact] public void ComputeDropWidth_AtLeastMinDropWidth() => Assert.True(new ToolStripDropDown().ComputeDropWidth() >= 140);

    [Fact]
    public void ComputeDropWidth_GrowsWithLongItemText()
    {
        var narrow = new ToolStripDropDown(); narrow.Items.Add("Hi");
        var wide   = new ToolStripDropDown(); wide.Items.Add("A very long menu item text label");
        Assert.True(wide.ComputeDropWidth() > narrow.ComputeDropWidth());
    }

    [Fact] public void ComputeDropHeight_EmptyDropDown_IsJustBorder() => Assert.Equal(2, new ToolStripDropDown().ComputeDropHeight());

    [Fact]
    public void ComputeDropHeight_TwoItems_IsCorrect()
    {
        var dd = new ToolStripDropDown(); dd.Items.Add("A"); dd.Items.Add("B");
        Assert.Equal(2 + 22 + 22, dd.ComputeDropHeight());
    }

    [Fact]
    public void ComputeDropHeight_SeparatorCountedAsSeparatorH()
    {
        var dd = new ToolStripDropDown(); dd.Items.Add("A"); dd.Items.Add(new ToolStripSeparator()); dd.Items.Add("B");
        Assert.Equal(2 + 22 + 8 + 22, dd.ComputeDropHeight());
    }

    [Fact]
    public void ComputeDropHeight_InvisibleItemsExcluded()
    {
        var dd = new ToolStripDropDown(); dd.Items.Add(new ToolStripMenuItem("A") { Visible = false });
        Assert.Equal(2, dd.ComputeDropHeight());
    }

    [Fact]
    public void GetDropDownBounds_ReturnsRectangleRelativeToOwner()
    {
        var dd = new ToolStripDropDown { PopupLocation = new Point(100, 50) }; dd.Items.Add("A");
        var bounds = dd.GetDropDownBounds(ownerAbsLeft: 10, ownerAbsTop: 5);
        Assert.Equal(100 - 10, bounds.X); Assert.Equal(50 - 5, bounds.Y);
        Assert.Equal(dd.ComputeDropWidth(), bounds.Width); Assert.Equal(dd.ComputeDropHeight(), bounds.Height);
    }

    [Fact] public void GetItemIndexAt_ReturnsMinusOneForEmptyDropDown() => Assert.Equal(-1, new ToolStripDropDown().GetItemIndexAt(5));

    [Fact]
    public void GetItemIndexAt_FirstItemHitAtTop()
    {
        var dd = new ToolStripDropDown(); dd.Items.Add("A");
        Assert.Equal(0, dd.GetItemIndexAt(1)); Assert.Equal(0, dd.GetItemIndexAt(22));
    }

    [Fact]
    public void GetItemIndexAt_SecondItemHitCorrectly()
    {
        var dd = new ToolStripDropDown(); dd.Items.Add("A"); dd.Items.Add("B");
        Assert.Equal(1, dd.GetItemIndexAt(23)); Assert.Equal(1, dd.GetItemIndexAt(44));
    }

    [Fact]
    public void GetItemIndexAt_SeparatorCountedCorrectly()
    {
        var dd = new ToolStripDropDown(); dd.Items.Add("A"); dd.Items.Add(new ToolStripSeparator()); dd.Items.Add("B");
        Assert.Equal(2, dd.GetItemIndexAt(31));
    }

    [Fact]
    public void GetItemIndexAt_BeyondAllItems_ReturnsMinus1()
    {
        var dd = new ToolStripDropDown(); dd.Items.Add("A");
        Assert.Equal(-1, dd.GetItemIndexAt(999));
    }

    [Fact]
    public void HandleMouseMove_SetsSelectedOnHoveredItem()
    {
        var dd = new ToolStripDropDown(); var item = new ToolStripMenuItem("A"); dd.Items.Add(item);
        dd.HandleMouseMove(0, 5); Assert.True(item.Selected);
    }

    [Fact]
    public void HandleMouseMove_ClearsSelectionOnPreviousItem()
    {
        var dd = new ToolStripDropDown(); var a = new ToolStripMenuItem("A"); var b = new ToolStripMenuItem("B");
        dd.Items.Add(a); dd.Items.Add(b);
        dd.HandleMouseMove(0, 5); dd.HandleMouseMove(0, 25);
        Assert.False(a.Selected); Assert.True(b.Selected);
    }

    [Fact]
    public void HandleMouseDown_FiresClickOnLeafItem()
    {
        var dd = new ToolStripDropDown { IsVisible = true }; var item = new ToolStripMenuItem("Save");
        int fired = 0; item.Click += (_, _) => fired++; dd.Items.Add(item);
        dd.HandleMouseDown(0, 5); Assert.Equal(1, fired);
    }

    [Fact]
    public void HandleMouseDown_ClosesDropDownAfterLeafClick()
    {
        var dd = new ToolStripDropDown { IsVisible = true }; dd.Items.Add(new ToolStripMenuItem("Close me"));
        dd.HandleMouseDown(0, 5); Assert.False(dd.IsVisible);
    }

    [Fact]
    public void HandleMouseDown_DisabledItem_DoesNotFireClick()
    {
        var dd = new ToolStripDropDown { IsVisible = true }; var item = new ToolStripMenuItem("Disabled") { Enabled = false };
        int fired = 0; item.Click += (_, _) => fired++; dd.Items.Add(item);
        dd.HandleMouseDown(0, 5); Assert.Equal(0, fired);
    }

    [Fact]
    public void HandleMouseDown_SeparatorDoesNotFireClick()
    {
        var dd = new ToolStripDropDown { IsVisible = true }; dd.Items.Add(new ToolStripSeparator());
        dd.HandleMouseDown(0, 4); // must not throw
    }

    [Fact]
    public void HandleMouseDown_ItemWithSubMenu_OpensSubMenu()
    {
        var dd = new ToolStripDropDown { IsVisible = true, PopupLocation = new Point(0, 0) };
        var parent = new ToolStripMenuItem("Parent"); parent.DropDownItems.Add("Child"); dd.Items.Add(parent);
        dd.HandleMouseDown(0, 5); Assert.True(parent.DropDownIsOpen);
    }

    [Fact]
    public void CloseChain_SetsIsVisibleFalse()
    {
        var dd = new ToolStripDropDown { IsVisible = true }; dd.CloseChain(); Assert.False(dd.IsVisible);
    }

    [Fact]
    public void CloseChain_ClosesOpenSubMenus()
    {
        var root = new ToolStripDropDown { IsVisible = true };
        var mi   = new ToolStripMenuItem("Sub"); mi.DropDownItems.Add("Child"); root.Items.Add(mi);
        mi.OpenDropDown(new Point(0, 0)); Assert.True(mi.DropDownIsOpen);
        root.CloseChain(); Assert.False(mi.DropDownIsOpen);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripDropDownMenu
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripDropDownMenuTests
{
    [Fact] public void InheritsToolStripDropDown() => Assert.IsAssignableFrom<ToolStripDropDown>(new ToolStripDropDownMenu());
    [Fact] public void CanAddItems() { var dd = new ToolStripDropDownMenu(); dd.Items.Add("Test"); Assert.Single(dd.Items); }
}

// ════════════════════════════════════════════════════════════════════════════════
// ToolStripMenuItem
// ════════════════════════════════════════════════════════════════════════════════
public class ToolStripMenuItemTests
{
    [Fact] public void DefaultCtor_TextIsEmpty() => Assert.Equal(string.Empty, new ToolStripMenuItem().Text);
    [Fact] public void TextCtor_SetsText() => Assert.Equal("File", new ToolStripMenuItem("File").Text);

    [Fact]
    public void TextImageCtor_SetsTextAndImage()
    {
        var img = new Canvas.Windows.Forms.Drawing.Image(); var item = new ToolStripMenuItem("Edit", img);
        Assert.Equal("Edit", item.Text); Assert.Same(img, item.Image);
    }

    [Fact]
    public void TextImageClickCtor_AttachesClickHandler()
    {
        int fired = 0; var item = new ToolStripMenuItem("Exit", null, (_, _) => fired++);
        item.PerformClick(); Assert.Equal(1, fired);
    }

    [Fact]
    public void TextImageDropDownItemsCtor_AddsChildren()
    {
        var item = new ToolStripMenuItem("Parent", null, new ToolStripMenuItem("A"), new ToolStripMenuItem("B"));
        Assert.Equal(2, item.DropDownItems.Count); Assert.True(item.HasDropDownItems);
    }

    [Fact]
    public void TextImageClickShortcutCtor_SetsShortcut()
    {
        var item = new ToolStripMenuItem("Save", null, (_, _) => { }, Keys.Control | Keys.S);
        Assert.Equal(Keys.Control | Keys.S, item.ShortcutKeys);
    }

    [Fact] public void DropDown_LazilyCreated_NotNull() => Assert.NotNull(new ToolStripMenuItem().DropDown);
    [Fact] public void DropDown_IsToolStripDropDownMenu() => Assert.IsType<ToolStripDropDownMenu>(new ToolStripMenuItem().DropDown);
    [Fact] public void DropDown_SourceItemSetToSelf() { var mi = new ToolStripMenuItem(); Assert.Same(mi, mi.DropDown.SourceItem); }
    [Fact] public void DropDownItems_IsDropDownItemsCollection() { var mi = new ToolStripMenuItem(); Assert.Same(mi.DropDown.Items, mi.DropDownItems); }
    [Fact] public void HasDropDownItems_FalseWhenEmpty() => Assert.False(new ToolStripMenuItem().HasDropDownItems);
    [Fact] public void HasDropDownItems_TrueAfterAddingChild() { var mi = new ToolStripMenuItem(); mi.DropDownItems.Add("Child"); Assert.True(mi.HasDropDownItems); }
    [Fact] public void DropDownIsOpen_FalseByDefault() => Assert.False(new ToolStripMenuItem().DropDownIsOpen);

    [Fact]
    public void OpenDropDown_SetsIsVisible()
    {
        var mi = new ToolStripMenuItem(); mi.DropDownItems.Add("Child"); mi.OpenDropDown(new Point(0, 0));
        Assert.True(mi.DropDownIsOpen);
    }

    [Fact]
    public void OpenDropDown_SetsPopupLocation()
    {
        var mi = new ToolStripMenuItem(); mi.DropDownItems.Add("Child"); mi.OpenDropDown(new Point(50, 100));
        Assert.Equal(new Point(50, 100), mi.DropDown.PopupLocation);
    }

    [Fact]
    public void OpenDropDown_DoesNothingWhenNoChildren()
    {
        var mi = new ToolStripMenuItem(); mi.OpenDropDown(new Point(0, 0)); Assert.False(mi.DropDownIsOpen);
    }

    [Fact]
    public void CloseDropDown_SetsIsVisibleFalse()
    {
        var mi = new ToolStripMenuItem(); mi.DropDownItems.Add("Child"); mi.OpenDropDown(new Point(0, 0));
        mi.CloseDropDown(); Assert.False(mi.DropDownIsOpen);
    }

    [Fact]
    public void DropDownOpening_FiredOnOpen()
    {
        var mi = new ToolStripMenuItem(); mi.DropDownItems.Add("Child"); int fired = 0;
        mi.DropDownOpening += (_, _) => fired++; mi.OpenDropDown(new Point(0, 0)); Assert.Equal(1, fired);
    }

    [Fact]
    public void DropDownOpened_FiredOnOpen()
    {
        var mi = new ToolStripMenuItem(); mi.DropDownItems.Add("Child"); int fired = 0;
        mi.DropDownOpened += (_, _) => fired++; mi.OpenDropDown(new Point(0, 0)); Assert.Equal(1, fired);
    }

    [Fact]
    public void DropDownClosed_FiredOnClose()
    {
        var mi = new ToolStripMenuItem(); mi.DropDownItems.Add("Child"); int fired = 0;
        mi.DropDownClosed += (_, _) => fired++; mi.OpenDropDown(new Point(0, 0)); mi.CloseDropDown(); Assert.Equal(1, fired);
    }

    [Fact] public void Checked_DefaultsFalse() => Assert.False(new ToolStripMenuItem().Checked);
    [Fact] public void Checked_RoundTrips() { var mi = new ToolStripMenuItem { Checked = true }; Assert.True(mi.Checked); }
    [Fact] public void Checked_True_SetsCheckStateToChecked() { var mi = new ToolStripMenuItem { Checked = true }; Assert.Equal(CheckState.Checked, mi.CheckState); }
    [Fact] public void Checked_False_SetsCheckStateToUnchecked() { var mi = new ToolStripMenuItem { Checked = true }; mi.Checked = false; Assert.Equal(CheckState.Unchecked, mi.CheckState); }
    [Fact] public void CheckState_DefaultsUnchecked() => Assert.Equal(CheckState.Unchecked, new ToolStripMenuItem().CheckState);
    [Fact] public void CheckState_Checked_SetsCheckedTrue() { var mi = new ToolStripMenuItem { CheckState = CheckState.Checked }; Assert.True(mi.Checked); }
    [Fact] public void CheckState_Unchecked_SetsCheckedFalse() { var mi = new ToolStripMenuItem { CheckState = CheckState.Checked }; mi.CheckState = CheckState.Unchecked; Assert.False(mi.Checked); }
    [Fact] public void CheckState_Indeterminate_DoesNotSetCheckedTrue() => Assert.False(new ToolStripMenuItem { CheckState = CheckState.Indeterminate }.Checked);
    [Fact] public void CheckOnClick_DefaultsFalse() => Assert.False(new ToolStripMenuItem().CheckOnClick);

    [Fact]
    public void CheckOnClick_True_TogglesCheckedOnClick()
    {
        var mi = new ToolStripMenuItem { CheckOnClick = true }; mi.PerformClick(); Assert.True(mi.Checked);
        mi.PerformClick(); Assert.False(mi.Checked);
    }

    [Fact]
    public void CheckedChanged_FiredByCheckOnClick()
    {
        var mi = new ToolStripMenuItem { CheckOnClick = true }; int fired = 0;
        mi.CheckedChanged += (_, _) => fired++; mi.PerformClick(); Assert.Equal(1, fired);
    }

    [Fact]
    public void CheckStateChanged_FiredByCheckOnClick()
    {
        var mi = new ToolStripMenuItem { CheckOnClick = true }; int fired = 0;
        mi.CheckStateChanged += (_, _) => fired++; mi.PerformClick(); Assert.Equal(1, fired);
    }

    [Fact] public void ShortcutKeys_DefaultsToNone() => Assert.Equal(Keys.None, new ToolStripMenuItem().ShortcutKeys);
    [Fact] public void ShortcutKeys_RoundTrips() { var mi = new ToolStripMenuItem { ShortcutKeys = Keys.Control | Keys.Z }; Assert.Equal(Keys.Control | Keys.Z, mi.ShortcutKeys); }
    [Fact] public void ShowShortcutKeys_DefaultsTrue() => Assert.True(new ToolStripMenuItem().ShowShortcutKeys);
    [Fact] public void ShowShortcutKeys_RoundTrips() { var mi = new ToolStripMenuItem { ShowShortcutKeys = false }; Assert.False(mi.ShowShortcutKeys); }
    [Fact] public void ShortcutKeyDisplayString_DefaultsEmpty() => Assert.Equal(string.Empty, new ToolStripMenuItem().ShortcutKeyDisplayString);
    [Fact] public void ShortcutKeyDisplayString_RoundTrips() { var mi = new ToolStripMenuItem { ShortcutKeyDisplayString = "Ctrl+Z" }; Assert.Equal("Ctrl+Z", mi.ShortcutKeyDisplayString); }
}

// ════════════════════════════════════════════════════════════════════════════════
// MenuStrip
// ════════════════════════════════════════════════════════════════════════════════
public class MenuStripTests
{
    [Fact] public void InheritsToolStrip() => Assert.IsAssignableFrom<ToolStrip>(new MenuStrip());
    [Fact] public void DefaultDock_IsTop() => Assert.Equal(DockStyle.Top, new MenuStrip().Dock);
    [Fact] public void DefaultHeight_Is24() => Assert.Equal(24, new MenuStrip().Height);
    [Fact] public void BackColor_DefaultIsLightGray() => Assert.Equal(System.Drawing.Color.FromArgb(240, 240, 240), new MenuStrip().BackColor);
    [Fact] public void Items_StartsEmpty() => Assert.Empty(new MenuStrip().Items);

    [Fact] public void AddItem_IncreasesCount() { var ms = new MenuStrip(); ms.Items.Add(new ToolStripMenuItem("File")); Assert.Single(ms.Items); }

    [Fact]
    public void AddMultipleItems_AllPresent()
    {
        var ms = new MenuStrip(); ms.Items.Add(new ToolStripMenuItem("File")); ms.Items.Add(new ToolStripMenuItem("Edit")); ms.Items.Add(new ToolStripMenuItem("Help"));
        Assert.Equal(3, ms.Items.Count);
    }

    [Fact]
    public void ItemOwner_SetToMenuStrip()
    {
        var ms = new MenuStrip(); var item = new ToolStripMenuItem("File"); ms.Items.Add(item); Assert.Same(ms, item.Owner);
    }

    [Fact]
    public void ClickOnLeafItem_WithNoDropDown_FiresClick()
    {
        var ms = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 }; var item = new ToolStripMenuItem("About");
        int fired = 0; item.Click += (_, _) => fired++; ms.Items.Add(item);
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5)); Assert.Equal(1, fired);
    }

    [Fact]
    public void ClickOnItemWithDropDown_OpensDropDown()
    {
        var ms = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 }; var item = new ToolStripMenuItem("File");
        item.DropDownItems.Add("Open"); ms.Items.Add(item);
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5)); Assert.True(item.DropDownIsOpen);
    }

    [Fact]
    public void ClickOnSameOpenItem_ClosesDropDown()
    {
        var ms = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 }; var item = new ToolStripMenuItem("File");
        item.DropDownItems.Add("Open"); ms.Items.Add(item);
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5));
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5));
        Assert.False(item.DropDownIsOpen);
    }

    [Fact]
    public void RightClick_DoesNotOpenDropDown()
    {
        var ms = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 }; var item = new ToolStripMenuItem("File");
        item.DropDownItems.Add("Open"); ms.Items.Add(item);
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Right, 1, 12, 5)); Assert.False(item.DropDownIsOpen);
    }

    [Fact]
    public void DisabledItem_ClickDoesNotOpenDropDown()
    {
        var ms = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 }; var item = new ToolStripMenuItem("File") { Enabled = false };
        item.DropDownItems.Add("Open"); ms.Items.Add(item);
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5)); Assert.False(item.DropDownIsOpen);
    }

    [Fact]
    public void MouseLeave_ClearsHover()
    {
        var ms = new MenuStrip { Width = 200, Height = 24 }; ms.Items.Add(new ToolStripMenuItem("File"));
        ms.SimulateMouseMove(new MouseEventArgs(MouseButtons.None, 0, 10, 5)); ms.SimulateMouseLeave();
    }

    [Fact]
    public void OpeningOneItem_ClosesOtherOpenItem()
    {
        var ms = new MenuStrip { Left = 0, Top = 0, Width = 400, Height = 24 };
        var file = new ToolStripMenuItem("File"); var edit = new ToolStripMenuItem("Edit");
        file.DropDownItems.Add("Open"); edit.DropDownItems.Add("Copy");
        ms.Items.Add(file); ms.Items.Add(edit);

        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 12, 5));
        Assert.True(file.DropDownIsOpen);
        ms.SimulateMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 55, 5));
        Assert.False(file.DropDownIsOpen); Assert.True(edit.DropDownIsOpen);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ContextMenuStrip
// ════════════════════════════════════════════════════════════════════════════════
public class ContextMenuStripTests
{
    [Fact] public void InheritsToolStripDropDownMenu() => Assert.IsAssignableFrom<ToolStripDropDownMenu>(new ContextMenuStrip());
    [Fact] public void DefaultCtor_IsNotVisible() => Assert.False(new ContextMenuStrip().IsVisible);
    [Fact] public void IContainerCtor_DoesNotThrow() => Assert.Null(Record.Exception(() => new ContextMenuStrip(new System.ComponentModel.Container())));
    [Fact] public void Open_SetsIsVisibleTrue() { var cms = new ContextMenuStrip(); cms.Open(); Assert.True(cms.IsVisible); }
    [Fact] public void Close_SetsIsVisibleFalse() { var cms = new ContextMenuStrip(); cms.Open(); cms.Close(); Assert.False(cms.IsVisible); }

    [Fact] public void Opening_EventFiredOnOpen() { var cms = new ContextMenuStrip(); int fired = 0; cms.Opening += (_, _) => fired++; cms.Open(); Assert.Equal(1, fired); }
    [Fact] public void Opened_EventFiredOnOpen()  { var cms = new ContextMenuStrip(); int fired = 0; cms.Opened  += (_, _) => fired++; cms.Open(); Assert.Equal(1, fired); }
    [Fact] public void Closing_EventFiredOnClose() { var cms = new ContextMenuStrip(); int fired = 0; cms.Closing += (_, _) => fired++; cms.Open(); cms.Close(); Assert.Equal(1, fired); }
    [Fact] public void Closed_EventFiredOnClose()  { var cms = new ContextMenuStrip(); int fired = 0; cms.Closed  += (_, _) => fired++; cms.Open(); cms.Close(); Assert.Equal(1, fired); }

    [Fact]
    public void OpeningBeforeOpenedInCorrectOrder()
    {
        var cms = new ContextMenuStrip(); var order = new List<string>();
        cms.Opening += (_, _) => order.Add("opening"); cms.Opened += (_, _) => order.Add("opened");
        cms.Open(); Assert.Equal(new[] { "opening", "opened" }, order);
    }

    [Fact]
    public void ClosingBeforeClosedInCorrectOrder()
    {
        var cms = new ContextMenuStrip(); var order = new List<string>();
        cms.Closing += (_, _) => order.Add("closing"); cms.Closed += (_, _) => order.Add("closed");
        cms.Open(); cms.Close(); Assert.Equal(new[] { "closing", "closed" }, order);
    }

    [Fact]
    public void Show_XY_SetsPopupLocationAndOpens()
    {
        var cms = new ContextMenuStrip(); cms.Show(30, 80);
        Assert.Equal(new Point(30, 80), cms.PopupLocation); Assert.True(cms.IsVisible);
    }

    [Fact]
    public void Show_ControlPoint_SetsPopupLocationAndOpens()
    {
        var control = new Panel { Left = 10, Top = 20 }; var cms = new ContextMenuStrip();
        cms.Show(control, new Point(5, 5));
        Assert.Equal(new Point(15, 25), cms.PopupLocation); Assert.True(cms.IsVisible);
    }

    [Fact]
    public void Show_ControlXY_SetsPopupLocationAndOpens()
    {
        var control = new Panel { Left = 0, Top = 0 }; var cms = new ContextMenuStrip();
        cms.Show(control, 50, 60);
        Assert.Equal(new Point(50, 60), cms.PopupLocation); Assert.True(cms.IsVisible);
    }

    [Fact]
    public void CanAddItems()
    {
        var cms = new ContextMenuStrip(); cms.Items.Add("Cut"); cms.Items.Add("Copy"); cms.Items.Add("Paste");
        Assert.Equal(3, cms.Items.Count);
    }

    [Fact]
    public void ItemClickWhileOpen_FiresEventAndCloses()
    {
        var cms = new ContextMenuStrip { PopupLocation = new Point(0, 0) }; var item = new ToolStripMenuItem("Cut");
        int fired = 0; item.Click += (_, _) => fired++; cms.Items.Add(item); cms.Open();
        cms.HandleMouseDown(0, 5); Assert.Equal(1, fired); Assert.False(cms.IsVisible);
    }

    [Fact]
    public void Close_AlsoCascadesOpenSubMenus()
    {
        var cms = new ContextMenuStrip(); var parent = new ToolStripMenuItem("Parent"); parent.DropDownItems.Add("Child");
        cms.Items.Add(parent); cms.Open(); parent.OpenDropDown(new Point(0, 0)); Assert.True(parent.DropDownIsOpen);
        cms.Close(); Assert.False(cms.IsVisible); Assert.False(parent.DropDownIsOpen);
    }

    [Fact] public void Control_ContextMenuStrip_RoundTrips() { var ctrl = new Panel(); var cms = new ContextMenuStrip(); ctrl.ContextMenuStrip = cms; Assert.Same(cms, ctrl.ContextMenuStrip); }
    [Fact] public void Control_ContextMenuStrip_CanBeNull() { var ctrl = new Panel(); ctrl.ContextMenuStrip = null; Assert.Null(ctrl.ContextMenuStrip); }
}

// ════════════════════════════════════════════════════════════════════════════════
// StatusStrip + ToolStripStatusLabel
// ════════════════════════════════════════════════════════════════════════════════
public class StatusStripTests
{
    [Fact] public void StatusStrip_DefaultDock_IsBottom() => Assert.Equal(DockStyle.Bottom, new StatusStrip().Dock);
    [Fact] public void StatusStrip_DefaultGripStyle_IsHidden() => Assert.Equal(ToolStripGripStyle.Hidden, new StatusStrip().GripStyle);
    [Fact] public void StatusStrip_DefaultStretch_IsTrue() => Assert.True(new StatusStrip().Stretch);
    [Fact] public void StatusStrip_DefaultSizingGrip_IsTrue() => Assert.True(new StatusStrip().SizingGrip);
    [Fact] public void StatusStrip_SizingGrip_RoundTrips() { var s = new StatusStrip { SizingGrip = false }; Assert.False(s.SizingGrip); }
    [Fact] public void StatusStrip_Items_StartsEmpty() => Assert.Empty(new StatusStrip().Items);

    [Fact]
    public void StatusStrip_CanAddStatusLabel()
    {
        var s = new StatusStrip(); var lbl = new ToolStripStatusLabel { Text = "Ready" }; s.Items.Add(lbl);
        Assert.Single(s.Items); Assert.Equal("Ready", ((ToolStripStatusLabel)s.Items[0]).Text);
    }

    [Fact]
    public void StatusStrip_CreateDefaultItem_ReturnsStatusLabel()
    {
        var s = new StatusStrip(); var item = s.CreateDefaultItem("Hello", null, null);
        Assert.IsType<ToolStripStatusLabel>(item); Assert.Equal("Hello", item.Text);
    }

    [Fact]
    public void StatusStrip_CreateDefaultItem_Separator_ReturnsSeparator()
    {
        Assert.IsType<ToolStripSeparator>(new StatusStrip().CreateDefaultItem("-", null, null));
    }

    [Fact] public void StatusLabel_DefaultSpring_IsFalse() => Assert.False(new ToolStripStatusLabel().Spring);
    [Fact] public void StatusLabel_DefaultBorderSides_IsNone() => Assert.Equal(ToolStripStatusLabelBorderSides.None, new ToolStripStatusLabel().BorderSides);
    [Fact] public void StatusLabel_DefaultBorderStyle_IsFlat() => Assert.Equal(Border3DStyle.Flat, new ToolStripStatusLabel().BorderStyle);
    [Fact] public void StatusLabel_DefaultLiveSetting_IsOff() => Assert.Equal(LiveSetting.Off, new ToolStripStatusLabel().LiveSetting);
    [Fact] public void StatusLabel_Spring_RoundTrips() { var l = new ToolStripStatusLabel { Spring = true }; Assert.True(l.Spring); }

    [Fact]
    public void StatusLabel_BorderSides_RoundTrips()
    {
        var l = new ToolStripStatusLabel { BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right };
        Assert.Equal(ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right, l.BorderSides);
    }

    [Fact] public void StatusLabel_BorderStyle_RoundTrips() { var l = new ToolStripStatusLabel { BorderStyle = Border3DStyle.Sunken }; Assert.Equal(Border3DStyle.Sunken, l.BorderStyle); }
    [Fact] public void StatusLabel_LiveSetting_RoundTrips() { var l = new ToolStripStatusLabel { LiveSetting = LiveSetting.Polite }; Assert.Equal(LiveSetting.Polite, l.LiveSetting); }
    [Fact] public void StatusLabel_Text_RoundTrips() => Assert.Equal("Status: OK", new ToolStripStatusLabel("Status: OK").Text);
    [Fact] public void StatusLabel_DefaultCtor_TextIsEmpty() => Assert.Equal(string.Empty, new ToolStripStatusLabel().Text);
    [Fact] public void StatusLabel_TextCtor_SetsText() => Assert.Equal("Ready", new ToolStripStatusLabel("Ready").Text);

    [Fact]
    public void StatusLabel_TextImageCtor_SetsTextAndImage()
    {
        var img = new Canvas.Windows.Forms.Drawing.Image { Source = "/img.png" }; var l = new ToolStripStatusLabel("Ready", img);
        Assert.Equal("Ready", l.Text); Assert.Same(img, l.Image);
    }

    [Fact]
    public void StatusLabel_ClickCtor_WiresHandler()
    {
        bool clicked = false; var l = new ToolStripStatusLabel("x", null, (_, __) => clicked = true);
        l.PerformClick(); Assert.True(clicked);
    }

    [Fact] public void StatusLabel_NameCtor_SetsName() => Assert.Equal("myLabel", new ToolStripStatusLabel("x", null, null, "myLabel").Name);

    [Fact]
    public void BorderSides_All_ContainsAllSides()
    {
        const ToolStripStatusLabelBorderSides all = ToolStripStatusLabelBorderSides.All;
        Assert.True((all & ToolStripStatusLabelBorderSides.Left)   != 0);
        Assert.True((all & ToolStripStatusLabelBorderSides.Top)    != 0);
        Assert.True((all & ToolStripStatusLabelBorderSides.Right)  != 0);
        Assert.True((all & ToolStripStatusLabelBorderSides.Bottom) != 0);
    }

    [Fact]
    public void Border3DStyle_HasExpectedValues()
    {
        _ = Border3DStyle.Flat; _ = Border3DStyle.Raised; _ = Border3DStyle.RaisedInner;
        _ = Border3DStyle.RaisedOuter; _ = Border3DStyle.Sunken; _ = Border3DStyle.SunkenInner;
        _ = Border3DStyle.SunkenOuter; _ = Border3DStyle.Etched; _ = Border3DStyle.Bump;
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
        var ms = new MenuStrip(); var file = new ToolStripMenuItem("File");
        file.DropDownItems.Add(new ToolStripMenuItem("Open")); file.DropDownItems.Add(new ToolStripSeparator()); file.DropDownItems.Add(new ToolStripMenuItem("Save"));
        ms.Items.Add(file);
        Assert.Single(ms.Items); Assert.Equal(3, file.DropDownItems.Count); Assert.True(file.HasDropDownItems);
        Assert.IsType<ToolStripSeparator>(file.DropDownItems[1]);
    }

    [Fact]
    public void NestedSubMenu_CanBeOpened()
    {
        var root = new ToolStripMenuItem("Root"); var sub = new ToolStripMenuItem("Sub"); var leaf = new ToolStripMenuItem("Leaf");
        sub.DropDownItems.Add(leaf); root.DropDownItems.Add(sub);
        root.OpenDropDown(new Point(0, 0)); Assert.True(root.DropDownIsOpen);
        root.DropDown.HandleMouseDown(0, 5); Assert.True(sub.DropDownIsOpen);
    }

    [Fact]
    public void CloseChainFromRoot_ClosesEntireTree()
    {
        var root = new ToolStripMenuItem("Root"); var sub = new ToolStripMenuItem("Sub");
        sub.DropDownItems.Add(new ToolStripMenuItem("Leaf")); root.DropDownItems.Add(sub);
        root.OpenDropDown(new Point(0, 0)); sub.OpenDropDown(new Point(0, 0));
        root.DropDown.CloseChain();
        Assert.False(root.DropDownIsOpen); Assert.False(sub.DropDownIsOpen);
    }

    [Fact]
    public void ContextMenuStrip_FullFlow_AddItemsOpenClickClose()
    {
        var cms = new ContextMenuStrip(); var cut = new ToolStripMenuItem("Cut"); var paste = new ToolStripMenuItem("Paste");
        int cuts = 0; cut.Click += (_, _) => cuts++;
        cms.Items.Add(cut); cms.Items.Add(new ToolStripSeparator()); cms.Items.Add(paste);
        cms.Show(100, 200); Assert.True(cms.IsVisible); Assert.Equal(new Point(100, 200), cms.PopupLocation);
        cms.HandleMouseDown(0, 5); Assert.Equal(1, cuts); Assert.False(cms.IsVisible);
    }

    [Fact]
    public void MenuStrip_ItemsAddedViaAddString_HaveCorrectText()
    {
        var ms = new MenuStrip(); ms.Items.Add("File"); ms.Items.Add("Edit"); ms.Items.Add("View");
        Assert.Equal("File", ms.Items[0].Text); Assert.Equal("Edit", ms.Items[1].Text); Assert.Equal("View", ms.Items[2].Text);
    }

    [Fact]
    public void ToolStripItemCollection_AddRange_PreservesOrder()
    {
        var ms = new MenuStrip();
        ms.Items.AddRange(new ToolStripItem[] { new ToolStripMenuItem("A"), new ToolStripMenuItem("B"), new ToolStripMenuItem("C") });
        Assert.Equal(new[] { "A", "B", "C" }, ms.Items.Select(i => i.Text).ToArray());
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// MenuStrip — shortcut key dispatch (ProcessShortcut)
// ════════════════════════════════════════════════════════════════════════════════
public class MenuStripShortcutTests
{
    [Fact]
    public void ProcessShortcut_MatchingItem_FiresClickAndReturnsTrue()
    {
        var ms   = new MenuStrip();
        var item = new ToolStripMenuItem { Text = "New", ShortcutKeys = Keys.Control | Keys.N };
        ms.Items.Add(item);

        int clicked = 0;
        item.Click += (_, _) => clicked++;

        bool handled = ms.ProcessShortcut(Keys.Control | Keys.N);

        Assert.True(handled);
        Assert.Equal(1, clicked);
    }

    [Fact]
    public void ProcessShortcut_NonMatchingKey_ReturnsFalse()
    {
        var ms   = new MenuStrip();
        var item = new ToolStripMenuItem { Text = "Open", ShortcutKeys = Keys.Control | Keys.O };
        ms.Items.Add(item);

        bool handled = ms.ProcessShortcut(Keys.Control | Keys.S);

        Assert.False(handled);
    }

    [Fact]
    public void ProcessShortcut_DisabledItem_IsNotFired()
    {
        var ms   = new MenuStrip();
        var item = new ToolStripMenuItem { Text = "Save", ShortcutKeys = Keys.Control | Keys.S, Enabled = false };
        ms.Items.Add(item);

        int clicked = 0;
        item.Click += (_, _) => clicked++;

        bool handled = ms.ProcessShortcut(Keys.Control | Keys.S);

        Assert.False(handled);
        Assert.Equal(0, clicked);
    }

    [Fact]
    public void ProcessShortcut_InvisibleItem_IsNotFired()
    {
        var ms   = new MenuStrip();
        var item = new ToolStripMenuItem { Text = "Save", ShortcutKeys = Keys.Control | Keys.S, Visible = false };
        ms.Items.Add(item);

        int clicked = 0;
        item.Click += (_, _) => clicked++;

        bool handled = ms.ProcessShortcut(Keys.Control | Keys.S);

        Assert.False(handled);
        Assert.Equal(0, clicked);
    }

    [Fact]
    public void ProcessShortcut_NestedDropDownItem_FiresClick()
    {
        var ms   = new MenuStrip();
        var file = new ToolStripMenuItem { Text = "File" };
        var save = new ToolStripMenuItem { Text = "Save", ShortcutKeys = Keys.Control | Keys.S };
        file.DropDownItems.Add(save);
        ms.Items.Add(file);

        int clicked = 0;
        save.Click += (_, _) => clicked++;

        bool handled = ms.ProcessShortcut(Keys.Control | Keys.S);

        Assert.True(handled);
        Assert.Equal(1, clicked);
    }

    [Fact]
    public void ProcessShortcut_KeysNone_ReturnsFalse()
    {
        var ms = new MenuStrip();
        Assert.False(ms.ProcessShortcut(Keys.None));
    }

    [Fact]
    public void ProcessShortcut_EmptyStrip_ReturnsFalse()
    {
        Assert.False(new MenuStrip().ProcessShortcut(Keys.Control | Keys.Z));
    }

    [Fact]
    public void ProcessShortcut_FirstMatchWins_OtherNotFired()
    {
        var ms = new MenuStrip();
        var a  = new ToolStripMenuItem { Text = "A", ShortcutKeys = Keys.Control | Keys.X };
        var b  = new ToolStripMenuItem { Text = "B", ShortcutKeys = Keys.Control | Keys.X };
        ms.Items.Add(a);
        ms.Items.Add(b);

        int aClicked = 0, bClicked = 0;
        a.Click += (_, _) => aClicked++;
        b.Click += (_, _) => bClicked++;

        ms.ProcessShortcut(Keys.Control | Keys.X);

        Assert.Equal(1, aClicked);
        Assert.Equal(0, bClicked);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// ContextMenuStrip — shortcut key dispatch (ProcessShortcut)
// ════════════════════════════════════════════════════════════════════════════════
public class ContextMenuStripShortcutTests
{
    [Fact]
    public void ProcessShortcut_MatchingItem_FiresClickAndReturnsTrue()
    {
        var cms  = new ContextMenuStrip();
        var item = new ToolStripMenuItem { Text = "Cut", ShortcutKeys = Keys.Control | Keys.X };
        cms.Items.Add(item);

        int clicked = 0;
        item.Click += (_, _) => clicked++;

        bool handled = cms.ProcessShortcut(Keys.Control | Keys.X);

        Assert.True(handled);
        Assert.Equal(1, clicked);
    }

    [Fact]
    public void ProcessShortcut_NonMatchingKey_ReturnsFalse()
    {
        var cms  = new ContextMenuStrip();
        var item = new ToolStripMenuItem { Text = "Copy", ShortcutKeys = Keys.Control | Keys.C };
        cms.Items.Add(item);

        Assert.False(cms.ProcessShortcut(Keys.Control | Keys.V));
    }

    [Fact]
    public void ProcessShortcut_NestedItem_FiresClick()
    {
        var cms  = new ContextMenuStrip();
        var sub  = new ToolStripMenuItem { Text = "Sub" };
        var deep = new ToolStripMenuItem { Text = "Deep", ShortcutKeys = Keys.Alt | Keys.D };
        sub.DropDownItems.Add(deep);
        cms.Items.Add(sub);

        int clicked = 0;
        deep.Click += (_, _) => clicked++;

        bool handled = cms.ProcessShortcut(Keys.Alt | Keys.D);

        Assert.True(handled);
        Assert.Equal(1, clicked);
    }

    [Fact]
    public void ProcessShortcut_KeysNone_ReturnsFalse()
    {
        Assert.False(new ContextMenuStrip().ProcessShortcut(Keys.None));
    }
}
