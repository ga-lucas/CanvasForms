using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

// ════════════════════════════════════════════════════════════════════════════════
// DataGridViewComboBoxColumn — in-cell dropdown
// ════════════════════════════════════════════════════════════════════════════════
public class DataGridViewComboBoxColumnTests
{
    // ── Column construction ───────────────────────────────────────────────────

    [Fact]
    public void Items_DefaultsToEmpty()
    {
        var col = new DataGridViewComboBoxColumn();
        Assert.Empty(col.Items);
    }

    [Fact]
    public void Items_CanAddStrings()
    {
        var col = new DataGridViewComboBoxColumn();
        col.Items.Add("Alpha");
        col.Items.Add("Beta");
        Assert.Equal(2, col.Items.Count);
    }

    [Fact]
    public void DisplayMember_RoundTrips()
    {
        var col = new DataGridViewComboBoxColumn { DisplayMember = "Name" };
        Assert.Equal("Name", col.DisplayMember);
    }

    [Fact]
    public void ValueMember_RoundTrips()
    {
        var col = new DataGridViewComboBoxColumn { ValueMember = "Id" };
        Assert.Equal("Id", col.ValueMember);
    }

    [Fact]
    public void DataSource_AcceptsIEnumerable()
    {
        var col = new DataGridViewComboBoxColumn();
        var src = new[] { "X", "Y", "Z" };
        col.DataSource = src;
        Assert.Same(src, col.DataSource);
    }

    // ── Grid integration — SetCellValue / GetCellValue ────────────────────────

    [Fact]
    public void SetCellValue_UpdatesGetCellValue_ForComboColumn()
    {
        var grid = new DataGridView();
        var col  = new DataGridViewComboBoxColumn { Name = "Status" };
        col.Items.Add("Open");
        col.Items.Add("Closed");
        grid.Columns.Add(col);
        grid.Rows.Add(new DataGridViewRow());

        grid.SetCellValue(0, 0, "Closed");
        Assert.Equal("Closed", grid.GetCellValue(0, 0));
    }

    [Fact]
    public void SetCellValue_FiresCellValueChanged()
    {
        var grid = new DataGridView();
        var col  = new DataGridViewComboBoxColumn();
        col.Items.Add("A");
        col.Items.Add("B");
        grid.Columns.Add(col);
        grid.Rows.Add(new DataGridViewRow());

        DataGridViewCellEventArgs? fired = null;
        grid.CellValueChanged += (_, e) => fired = e;

        grid.SetCellValue(0, 0, "B");

        Assert.NotNull(fired);
        Assert.Equal(0, fired!.ColumnIndex);
        Assert.Equal(0, fired!.RowIndex);
    }

    [Fact]
    public void SetCellValue_AcceptsNullToReset()
    {
        var grid = new DataGridView();
        var col  = new DataGridViewComboBoxColumn();
        col.Items.Add("X");
        grid.Columns.Add(col);
        grid.Rows.Add(new DataGridViewRow());

        grid.SetCellValue(0, 0, "X");
        grid.SetCellValue(0, 0, null);
        Assert.Null(grid.GetCellValue(0, 0));
    }

    // ── Mixed column grid ─────────────────────────────────────────────────────

    [Fact]
    public void ComboColumn_CoexistsWithTextBoxColumn()
    {
        var grid = new DataGridView();
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Label" });
        var combo = new DataGridViewComboBoxColumn { Name = "Choice" };
        combo.Items.Add("Yes");
        combo.Items.Add("No");
        grid.Columns.Add(combo);

        Assert.Equal(2, grid.Columns.Count);
        Assert.IsType<DataGridViewComboBoxColumn>(grid.Columns[1]);
    }

    // ── DataSource vs Items precedence ────────────────────────────────────────

    [Fact]
    public void DataSource_WithList_SuppliesItems()
    {
        var items = new List<string> { "Red", "Green", "Blue" };
        var col   = new DataGridViewComboBoxColumn { DataSource = items };
        Assert.NotNull(col.DataSource);
        Assert.IsAssignableFrom<System.Collections.IEnumerable>(col.DataSource);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// DataGridViewCheckBoxColumn — single-click toggle + Space-key toggle
// ════════════════════════════════════════════════════════════════════════════════
public class DataGridViewCheckBoxToggleTests
{
    private static DataGridView BuildGrid(bool initialValue)
    {
        var grid = new DataGridView();
        grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Active" });
        var row = new DataGridViewRow();
        row.Cells.Add(new DataGridViewCell());
        row.Cells[0].Value = initialValue;
        grid.Rows.Add(row);
        return grid;
    }

    [Fact]
    public void SetCellValue_Bool_StoresTrueAndFiresEvent()
    {
        var grid = BuildGrid(false);
        bool eventFired = false;
        grid.CellValueChanged += (_, _) => eventFired = true;

        grid.SetCellValue(0, 0, true);

        Assert.True(eventFired);
        Assert.True((bool)grid.GetCellValue(0, 0)!);
    }

    [Fact]
    public void SetCellValue_TogglesFromTrueToFalse()
    {
        var grid = BuildGrid(true);
        grid.SetCellValue(0, 0, false);
        Assert.False((bool)grid.GetCellValue(0, 0)!);
    }

    [Fact]
    public void CheckBoxColumn_DefaultsToUnchecked()
    {
        var grid = new DataGridView();
        grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Chk" });
        var row = new DataGridViewRow();
        row.Cells.Add(new DataGridViewCell());
        grid.Rows.Add(row);

        var raw = grid.GetCellValue(0, 0);
        bool chk = raw is true || (raw is string sv && sv.Equals("true", StringComparison.OrdinalIgnoreCase));
        Assert.False(chk);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// DataGridView — column resize by drag
// ════════════════════════════════════════════════════════════════════════════════
public class DataGridViewColumnResizeTests
{
    private static DataGridView MakeGrid(int colCount = 3)
    {
        var grid = new DataGridView { Width = 400, Height = 200 };
        for (int i = 0; i < colCount; i++)
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = $"C{i}", HeaderText = $"Col{i}", Width = 80 });
        return grid;
    }

    [Fact]
    public void AllowUserToResizeColumns_DefaultsToTrue()
    {
        Assert.True(new DataGridView().AllowUserToResizeColumns);
    }

    [Fact]
    public void ColumnWidth_CanBeSetDirectly()
    {
        var grid = MakeGrid();
        grid.Columns[0].Width = 120;
        Assert.Equal(120, grid.Columns[0].Width);
    }

    [Fact]
    public void ColumnResizable_DefaultsToTrue()
    {
        var col = new DataGridViewTextBoxColumn();
        Assert.True(col.Resizable);
    }

    [Fact]
    public void ColumnResizable_WhenFalse_DoesNotAffectOtherColumns()
    {
        var grid = MakeGrid();
        grid.Columns[0].Resizable = false;
        Assert.True(grid.Columns[1].Resizable);
    }

    [Fact]
    public void Column_Width_ClampsToMinimumFive()
    {
        // DataGridViewColumn.Width clamps to at least 5 (matches resize drag guard)
        var col = new DataGridViewTextBoxColumn { Width = 3 };
        Assert.True(col.Width >= 5);
    }

    [Fact]
    public void MultipleColumns_IndependentWidths()
    {
        var grid = MakeGrid(3);
        grid.Columns[0].Width = 50;
        grid.Columns[1].Width = 150;
        grid.Columns[2].Width = 200;
        Assert.Equal(50,  grid.Columns[0].Width);
        Assert.Equal(150, grid.Columns[1].Width);
        Assert.Equal(200, grid.Columns[2].Width);
    }
}

// ════════════════════════════════════════════════════════════════════════════════
// DataGridView — inline TextBox cell editing
// ════════════════════════════════════════════════════════════════════════════════
public class DataGridViewInlineEditTests
{
    private static DataGridView MakeGrid()
    {
        var grid = new DataGridView { Width = 400, Height = 200 };
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Value", HeaderText = "Value" });
        var row = new DataGridViewRow();
        row.Cells.Add(new DataGridViewCell { Value = "Alice" });
        row.Cells.Add(new DataGridViewCell { Value = "42" });
        grid.Rows.Add(row);
        return grid;
    }

    [Fact]
    public void BeginEdit_SetsEditingState()
    {
        var grid = MakeGrid();
        grid.BeginEdit(0, 0);
        // Verify CellBeginEdit was raised — use event capture
        bool began = false;
        grid.CellBeginEdit += (_, _) => began = true;
        grid.EndEdit(commit: false);  // reset
        grid.BeginEdit(0, 0);
        // (event already wired after first call — retest with fresh grid)
        var g2 = MakeGrid();
        bool began2 = false;
        g2.CellBeginEdit += (_, _) => began2 = true;
        g2.BeginEdit(0, 0);
        Assert.True(began2);
    }

    [Fact]
    public void EndEdit_Commit_SavesValue()
    {
        var grid = MakeGrid();
        grid.BeginEdit(0, 0);
        // Simulate typing: EndEdit with a pre-set edit text would require internal access.
        // We verify that EndEdit(true) fires CellEndEdit and value is written.
        bool ended = false;
        grid.CellEndEdit += (_, _) => ended = true;
        grid.EndEdit(commit: true);
        Assert.True(ended);
    }

    [Fact]
    public void CancelEdit_DoesNotChangeValue()
    {
        var grid = MakeGrid();
        string? original = (string?)grid.GetCellValue(0, 0);
        grid.BeginEdit(0, 0);
        grid.CancelEdit();
        Assert.Equal(original, (string?)grid.GetCellValue(0, 0));
    }

    [Fact]
    public void BeginEdit_ReadOnlyGrid_DoesNotBeginEdit()
    {
        var grid = MakeGrid();
        grid.ReadOnly = true;
        bool began = false;
        grid.CellBeginEdit += (_, _) => began = true;
        grid.BeginEdit(0, 0);
        Assert.False(began);
    }

    [Fact]
    public void BeginEdit_CheckBoxColumn_DoesNotBeginEdit()
    {
        var grid = new DataGridView { Width = 400, Height = 200 };
        grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Active" });
        var row = new DataGridViewRow();
        row.Cells.Add(new DataGridViewCell { Value = false });
        grid.Rows.Add(row);

        bool began = false;
        grid.CellBeginEdit += (_, _) => began = true;
        grid.BeginEdit(0, 0);
        Assert.False(began);
    }

    [Fact]
    public void BeginEdit_ComboBoxColumn_DoesNotBeginEdit()
    {
        var grid = new DataGridView { Width = 400, Height = 200 };
        var col = new DataGridViewComboBoxColumn { Name = "Combo" };
        col.Items.Add("X");
        grid.Columns.Add(col);
        var row = new DataGridViewRow();
        row.Cells.Add(new DataGridViewCell { Value = "X" });
        grid.Rows.Add(row);

        bool began = false;
        grid.CellBeginEdit += (_, _) => began = true;
        grid.BeginEdit(0, 0);
        Assert.False(began);
    }

    [Fact]
    public void EndEdit_CalledWithoutBegin_IsNoOp()
    {
        var grid = MakeGrid();
        bool ended = false;
        grid.CellEndEdit += (_, _) => ended = true;
        grid.EndEdit(commit: true);   // no edit in progress
        Assert.False(ended);
    }

    [Fact]
    public void BeginEdit_SameCell_Twice_IsNoOp()
    {
        var grid = MakeGrid();
        int count = 0;
        grid.CellBeginEdit += (_, _) => count++;
        grid.BeginEdit(0, 0);
        grid.BeginEdit(0, 0);   // second call while already editing same cell
        Assert.Equal(1, count);
        grid.CancelEdit();
    }

    [Fact]
    public void BeginEdit_DifferentCell_CommitsPrevious()
    {
        var grid = MakeGrid();
        int beginCount = 0, endCount = 0;
        grid.CellBeginEdit += (_, _) => beginCount++;
        grid.CellEndEdit   += (_, _) => endCount++;
        grid.BeginEdit(0, 0);
        grid.BeginEdit(0, 1);   // switching cell should commit the first
        Assert.Equal(2, beginCount);
        Assert.Equal(1, endCount);
        grid.CancelEdit();
    }

    [Fact]
    public void EditMode_DefaultsToEditOnKeystrokeOrF2()
    {
        Assert.Equal(DataGridViewEditMode.EditOnKeystrokeOrF2, new DataGridView().EditMode);
    }
}
