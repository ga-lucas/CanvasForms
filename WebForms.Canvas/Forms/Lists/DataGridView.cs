using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms;

/// <summary>
/// WinForms-compatible DataGridView rendered to an HTML canvas.
/// Supports in-process DataSource binding (IList, BindingSource, DataTable),
/// auto-column generation, virtual/scrolled rendering, and row/cell selection.
/// </summary>
public class DataGridView : ScrollableControl, System.ComponentModel.ISupportInitialize
{
    void System.ComponentModel.ISupportInitialize.BeginInit() { }
    void System.ComponentModel.ISupportInitialize.EndInit() { }
    // ── Layout constants ────────────────────────────────────────
    private const int HeaderHeight = 26;
    private const int RowHeightDefault = 23;
    private const int RowHeaderWidth = 40;
    private const int ScrollBarW = 17;

    // ── Layout constants (ComboBox dropdown) ─────────────────────
    private const int ComboCellArrowW = 17;
    private const int ComboDropItemH  = 20;

    // ── State ────────────────────────────────────────────────────
    private object? _dataSource;
    private string _dataMember = string.Empty;
    private int _scrollOffsetY = 0;
    private int _scrollOffsetX = 0;
    private int _selectedRowIndex = -1;
    private int _selectedColIndex = -1;
    private (int row, int col) _selectedCell = (-1, -1);
    private int _hoveredRow = -1;
    private (int row, int col)? _invalidCell = null;  // cell failing CellValidating/RowValidating
    private bool _autoGenerateColumns = true;

    // ── ComboBox in-cell dropdown state ──────────────────────────
    private bool _comboOpen     = false;
    private int  _comboRow      = -1;
    private int  _comboCol      = -1;
    private int  _comboHoverIdx = -1;  // hovered item index in open dropdown
    private int  _comboCellX    = 0;   // screen x of the cell left edge
    private int  _comboCellY    = 0;   // screen y of the cell bottom edge (dropdown opens downward)

    // ── Column resize state ───────────────────────────────────────
    private const int ResizeHandleW = 5;   // pixels either side of a column boundary
    private bool _resizingCol     = false;
    private int  _resizeColIndex  = -1;    // column whose right edge is being dragged
    private int  _resizeStartX    = 0;     // mouse X when drag started
    private int  _resizeStartW    = 0;     // column width when drag started

    // ── Inline TextBox cell editing state ────────────────────────
    private bool   _editing        = false;
    private int    _editRow        = -1;
    private int    _editCol        = -1;
    private string _editText       = string.Empty;
    private int    _editCursorPos  = 0;    // caret position inside _editText
    private readonly List<object?[]> _boundRows = new();
    private SortOrder _sortOrder = SortOrder.None;
    private int _sortColIndex = -1;

    // Multi-column sort list (primary first)
    private readonly List<SortedColumnInfo> _sortedColumns = new();

    // ── Collections ─────────────────────────────────────────────
    public DataGridViewColumnCollection Columns { get; }
    public DataGridViewRowCollection Rows { get; }

    // ── Events ───────────────────────────────────────────────────
    public event EventHandler<DataGridViewCellEventArgs>? CellClick;
#pragma warning disable CS0067
    public event EventHandler<DataGridViewCellEventArgs>? CellDoubleClick;
    public event EventHandler<DataGridViewCellEventArgs>? CellValueChanged;
    public event EventHandler<DataGridViewCellMouseEventArgs>? CellMouseClick;
    public event EventHandler<DataGridViewCellMouseEventArgs>? CellMouseDoubleClick;
    public event EventHandler<DataGridViewCellEventArgs>? CellEnter;
    public event EventHandler<DataGridViewCellEventArgs>? CellLeave;
    public event EventHandler<DataGridViewRowEventArgs>? RowEnter;
    public event EventHandler<DataGridViewRowEventArgs>? RowLeave;
    public event EventHandler<DataGridViewColumnEventArgs>? ColumnAdded;
    public event EventHandler<DataGridViewRowEventArgs>? RowsAdded;
    public event EventHandler<DataGridViewRowEventArgs>? RowsRemoved;
    public event EventHandler<DataGridViewRowEventArgs>? UserAddedRow;
    public event EventHandler<DataGridViewRowCancelEventArgs>? UserDeletingRow;
    public event EventHandler<DataGridViewRowEventArgs>? UserDeletedRow;
    public event EventHandler<DataGridViewRowEventArgs>? DefaultValuesNeeded;
    public event EventHandler? SelectionChanged;
    public event EventHandler<DataGridViewCellCancelEventArgs>? CellValidating;
    public event EventHandler<DataGridViewCellCancelEventArgs>? RowValidating;
    public event EventHandler<DataGridViewDataErrorEventArgs>? DataError;
    public event DataGridViewSortCompareEventHandler? SortCompare;
    public event EventHandler<DataGridViewCellEventArgs>? CellBeginEdit;
    public event EventHandler<DataGridViewCellEventArgs>? CellEndEdit;
    /// <summary>
    /// Raised for each cell before it is rendered. Allows customising the display value,
    /// cell style colours, and font — matching WinForms <c>DataGridView.CellFormatting</c>.
    /// </summary>
    public event EventHandler<DataGridViewCellFormattingEventArgs>? CellFormatting;
#pragma warning restore CS0067
    public event EventHandler<DataGridViewColumnEventArgs>? ColumnHeaderMouseClick;

    // ── Properties ───────────────────────────────────────────────
    public bool AutoGenerateColumns
    {
        get => _autoGenerateColumns;
        set { _autoGenerateColumns = value; if (_dataSource != null) RebindDataSource(); }
    }

    public object? DataSource
    {
        get => _dataSource;
        set { _dataSource = value; RebindDataSource(); }
    }

    public string DataMember
    {
        get => _dataMember;
        set { _dataMember = value; RebindDataSource(); }
    }

    public DataGridViewSelectionMode SelectionMode { get; set; } = DataGridViewSelectionMode.RowHeaderSelect;
    public DataGridViewEditMode EditMode { get; set; } = DataGridViewEditMode.EditOnKeystrokeOrF2;
    public DataGridViewScrollBars ScrollBars { get; set; } = DataGridViewScrollBars.Both;
    public bool MultiSelect { get; set; } = true;
    public bool ReadOnly { get; set; } = false;
    public bool ShowCellToolTips { get; set; } = true;
    public bool ShowEditingIcon { get; set; } = true;
    public bool AllowUserToAddRows { get; set; } = true;
    public bool AllowUserToDeleteRows { get; set; } = true;
    public bool AllowUserToOrderColumns { get; set; } = false;
    public bool AllowUserToResizeColumns { get; set; } = true;
    public bool AllowUserToResizeRows { get; set; } = true;
    public bool ColumnHeadersVisible { get; set; } = true;
    public bool RowHeadersVisible { get; set; } = true;
    public int ColumnHeadersHeight { get; set; } = HeaderHeight;
    public int RowHeadersWidth { get; set; } = RowHeaderWidth;
    public DataGridViewColumnHeadersHeightSizeMode ColumnHeadersHeightSizeMode { get; set; } = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
    public DataGridViewRowHeadersWidthSizeMode RowHeadersWidthSizeMode { get; set; } = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
    public DataGridViewCellStyle DefaultCellStyle { get; set; } = new DataGridViewCellStyle();
    public DataGridViewCellStyle ColumnHeadersDefaultCellStyle { get; set; } = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 240, 240) };
    public DataGridViewCellStyle RowHeadersDefaultCellStyle { get; set; } = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 240, 240) };
    public DataGridViewCellStyle AlternatingRowsDefaultCellStyle { get; set; } = new DataGridViewCellStyle();
    public Color GridColor { get; set; } = Color.FromArgb(166, 166, 166);
    public bool EnableHeadersVisualStyles { get; set; } = true;
    public int RowCount => _boundRows.Count > 0 ? _boundRows.Count : Rows.Count;
    public int ColumnCount => Columns.Count;
    public DataGridViewClipboardCopyMode ClipboardCopyMode { get; set; } = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
    public BorderStyle BorderStyle { get; set; } = BorderStyle.Fixed3D;

    public int? SelectedRowIndex => _selectedRowIndex >= 0 ? _selectedRowIndex : null;

    public DataGridView()
    {
        Columns = new DataGridViewColumnCollection(this);
        Rows = new DataGridViewRowCollection(this);
        Width = 400;
        Height = 200;
        BackColor = Color.White;
        ForeColor = Color.Black;
    }

    // ── Inline cell editing API ──────────────────────────────────

    /// <summary>Begins editing the cell at (rowIndex, colIndex) — WinForms compatible.</summary>
    public void BeginEdit(int rowIndex, int colIndex)
    {
        if (ReadOnly) return;
        if (colIndex < 0 || colIndex >= Columns.Count) return;
        var col = Columns[colIndex];
        if (col.ReadOnly || col is DataGridViewCheckBoxColumn || col is DataGridViewComboBoxColumn) return;
        if (_editing && _editRow == rowIndex && _editCol == colIndex) return;

        EndEdit(commit: true);

        _editing       = true;
        _editRow       = rowIndex;
        _editCol       = colIndex;
        _editText      = GetCellText(rowIndex, colIndex);
        _editCursorPos = _editText.Length;
        CellBeginEdit?.Invoke(this, new DataGridViewCellEventArgs(colIndex, rowIndex));
        Invalidate();
    }

    /// <summary>
    /// Commits (or cancels) the current edit and fires <see cref="CellEndEdit"/>.
    /// </summary>
    public void EndEdit(bool commit = true)
    {
        if (!_editing) return;
        int row = _editRow, col = _editCol;
        if (commit)
            SetCellValue(row, col, _editText);
        _editing       = false;
        _editRow       = -1;
        _editCol       = -1;
        _editText      = string.Empty;
        _editCursorPos = 0;
        CellEndEdit?.Invoke(this, new DataGridViewCellEventArgs(col, row));
        Invalidate();
    }

    /// <summary>Cancels the current edit without saving changes.</summary>
    public void CancelEdit() => EndEdit(commit: false);

    // ── Data binding ────────────────────────────────────────────

    private void RebindDataSource()
    {
        _boundRows.Clear();

        if (_dataSource == null) { Invalidate(); return; }

        IList? list = null;

        // BindingSource
        if (_dataSource is BindingSource bs)
        {
            list = bs;
        }
        // DataTable
        else if (_dataSource is DataTable dt)
        {
            if (_autoGenerateColumns) AutoGenerateFromDataTable(dt);
            foreach (DataRow row in dt.Rows)
            {
                var cells = new object?[dt.Columns.Count];
                for (int i = 0; i < dt.Columns.Count; i++) cells[i] = row[i];
                _boundRows.Add(cells);
            }
            Invalidate();
            return;
        }
        // IList / IEnumerable
        else if (_dataSource is IList l) list = l;
        else if (_dataSource is IEnumerable<object> seq) list = seq.ToList();

        if (list == null || list.Count == 0) { Invalidate(); return; }

        if (_autoGenerateColumns) AutoGenerateFromList(list);

        var props = GetProperties(list);
        foreach (object? item in list)
        {
            if (item == null) continue;
            var cells = new object?[props.Count];
            for (int pi = 0; pi < props.Count; pi++)
                cells[pi] = props[pi].GetValue(item);
            _boundRows.Add(cells);
        }
        Invalidate();
    }

    private List<PropertyInfo> GetProperties(IList list)
    {
        if (list.Count == 0) return new();
        var t = list[0]?.GetType();
        if (t == null) return new();
        return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToList();
    }

    private void AutoGenerateFromList(IList list)
    {
        if (Columns.Count > 0 || list.Count == 0) return;
        var props = GetProperties(list);
        foreach (var p in props)
        {
            Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = p.Name,
                HeaderText = p.Name,
                DataPropertyName = p.Name,
                Width = Math.Max(60, p.Name.Length * 9)
            });
        }
    }

    private void AutoGenerateFromDataTable(DataTable dt)
    {
        if (Columns.Count > 0) return;
        foreach (DataColumn dc in dt.Columns)
        {
            Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = dc.ColumnName,
                HeaderText = dc.Caption.Length > 0 ? dc.Caption : dc.ColumnName,
                DataPropertyName = dc.ColumnName,
                Width = Math.Max(60, dc.ColumnName.Length * 9)
            });
        }
    }

    // ── Row data access ─────────────────────────────────────────

    private int GetDisplayRowCount()
    {
        if (_boundRows.Count > 0) return _boundRows.Count;
        return Rows.Count;
    }

    private string GetCellText(int rowIndex, int colIndex)
    {
        if (colIndex < 0 || colIndex >= Columns.Count) return string.Empty;
        var col = Columns[colIndex];

        if (_boundRows.Count > 0)
        {
            if (rowIndex >= _boundRows.Count || colIndex >= _boundRows[rowIndex].Length) return string.Empty;
            var v = _boundRows[rowIndex][colIndex];
            if (v == null) return string.Empty;
            if (!string.IsNullOrEmpty(col.DefaultCellStyle.Format))
            {
                try { return ((IFormattable)v).ToString(col.DefaultCellStyle.Format, null); }
                catch { }
            }
            return v.ToString() ?? string.Empty;
        }
        else
        {
            if (rowIndex >= Rows.Count) return string.Empty;
            var row = Rows[rowIndex];
            if (colIndex < row.Cells.Count) return row.Cells[colIndex].FormattedValue;
            return string.Empty;
        }
    }

    // ── Sort ─────────────────────────────────────────────────────

    /// <summary>
    /// Read-only list of active sort criteria (primary first).
    /// Each entry carries a column index and sort direction.
    /// </summary>
    public IReadOnlyList<SortedColumnInfo> SortedColumns => _sortedColumns;

    /// <summary>
    /// Sets the primary sort column, replacing any existing sort.
    /// </summary>
    public void Sort(DataGridViewColumn col, ListSortDirection direction)
    {
        _sortColIndex = col.Index;
        _sortOrder    = direction == ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;
        _sortedColumns.Clear();
        _sortedColumns.Add(new SortedColumnInfo(col.Index, direction));
        ApplyMultiSort();
    }

    /// <summary>
    /// Appends a secondary sort criterion (Ctrl+click on a header).
    /// If the column is already in the sort list its direction is toggled.
    /// </summary>
    public void AddSort(DataGridViewColumn col, ListSortDirection direction)
    {
        var existing = _sortedColumns.FindIndex(s => s.ColumnIndex == col.Index);
        if (existing >= 0)
            _sortedColumns[existing] = new SortedColumnInfo(col.Index, direction);
        else
            _sortedColumns.Add(new SortedColumnInfo(col.Index, direction));
        ApplyMultiSort();
    }

    /// <summary>Removes all active sort criteria and restores insertion order.</summary>
    public void RemoveSort()
    {
        _sortColIndex = -1;
        _sortOrder    = SortOrder.None;
        _sortedColumns.Clear();
        Invalidate();
    }

    private void ApplyMultiSort()
    {
        if (_sortedColumns.Count == 0 || _boundRows.Count == 0) { Invalidate(); return; }
        _boundRows.Sort((a, b) =>
        {
            foreach (var s in _sortedColumns)
            {
                int ci = s.ColumnIndex;
                if (ci >= a.Length || ci >= b.Length) continue;
                var av = a[ci]; var bv = b[ci];
                int cmp;
                if (av == null && bv == null) cmp = 0;
                else if (av == null) cmp = -1;
                else if (bv == null) cmp = 1;
                else if (av is IComparable ac && av.GetType() == bv.GetType())
                    cmp = ac.CompareTo(bv);
                else
                    cmp = string.Compare(av.ToString(), bv.ToString(), StringComparison.OrdinalIgnoreCase);
                if (cmp != 0)
                    return s.Direction == ListSortDirection.Ascending ? cmp : -cmp;
            }
            return 0;
        });
        Invalidate();
    }

    // ── Paint ────────────────────────────────────────────────────

    private int FrozenColumnsWidth()
    {
        int total = 0;
        foreach (var col in Columns)
            if (col.Visible && col.Frozen) total += col.Width;
        return total;
    }

    /// <summary>Total pixel height of all visible frozen rows.</summary>
    private int FrozenRowsHeight()
    {
        int total = 0;
        int displayCount = GetDisplayRowCount();
        for (int ri = 0; ri < displayCount; ri++)
        {
            if (ri < Rows.Count && Rows[ri].Frozen && Rows[ri].Visible)
                total += RowHeightDefault;
        }
        return total;
    }

    /// <summary>Number of visible frozen rows.</summary>
    private int FrozenRowCount()
    {
        int count = 0;
        for (int ri = 0; ri < Rows.Count; ri++)
            if (Rows[ri].Frozen && Rows[ri].Visible) count++;
        return count;
    }

    private int TotalColumnsWidth()
    {
        int total = 0;
        foreach (var col in Columns) if (col.Visible) total += col.Width;
        return total;
    }

    // Draw one column header cell, clipped to [clipLeft, clipRight).
    private void DrawColumnHeader(Graphics g, int ci, DataGridViewColumn col, int cx, int y0, int colHdrH,
                                   int clipLeft, int clipRight)
    {
        int right = cx + col.Width;
        if (right <= clipLeft || cx >= clipRight) return;
        int visRight = Math.Min(right - 1, clipRight - 1);
        using var hdrPen = new Pen(Color.FromArgb(166, 166, 166));
        g.DrawLine(hdrPen, visRight, y0, visRight, y0 + colHdrH);
        g.DrawLine(hdrPen, clipLeft, y0 + colHdrH - 1, clipRight, y0 + colHdrH - 1);
        string sortIndicator = string.Empty;
        int sortEntry = _sortedColumns.FindIndex(s => s.ColumnIndex == ci);
        if (sortEntry >= 0)
        {
            var se = _sortedColumns[sortEntry];
            string arrow = se.Direction == ListSortDirection.Ascending ? "▲" : "▼";
            sortIndicator = _sortedColumns.Count > 1 ? $" {arrow}{sortEntry + 1}" : $" {arrow}";
        }
        using var textBrush = new SolidBrush(Color.Black);
        g.DrawString(col.HeaderText + sortIndicator, Font, textBrush,
            Math.Max(cx, clipLeft) + 3, y0 + (colHdrH - Font.Height) / 2);
    }

    // Draw one data cell, clipped to [clipLeft, clipRight).
    private void DrawCell(Graphics g, int ri, int ci, DataGridViewColumn col, int cx, int ry, int rowH,
                           bool rowSelected, Color rowBg, int clipLeft, int clipRight)
    {
        int right = cx + col.Width;
        if (right <= clipLeft || cx >= clipRight) return;

        bool cellSelected = (rowSelected && SelectionMode == DataGridViewSelectionMode.FullRowSelect)
                            || _selectedCell == (ri, ci);

        if (cellSelected && !rowSelected)
        {
            Color cellBg = Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(204, 228, 247);
            using var cellBrush = new SolidBrush(cellBg);
            int clipX = Math.Max(cx, clipLeft);
            g.FillRectangle(cellBrush, clipX, ry, Math.Min(right, clipRight) - clipX, rowH);
        }

        string text = GetCellText(ri, ci);
        Color textColor = (rowSelected || cellSelected) && Focused ? Color.White : ForeColor;
        Font cellFont = Font;

        // ── CellFormatting ────────────────────────────────────────────────────
        if (CellFormatting != null && !(col is DataGridViewCheckBoxColumn))
        {
            var rawValue = ri < _boundRows.Count && ci < _boundRows[ri].Length ? _boundRows[ri][ci] : null;
            var fmtArgs = new DataGridViewCellFormattingEventArgs(ci, ri, rawValue, typeof(string), DefaultCellStyle);
            CellFormatting.Invoke(this, fmtArgs);
            if (fmtArgs.FormattingApplied)
            {
                text = fmtArgs.Value?.ToString() ?? string.Empty;
                if (fmtArgs.CellStyle.ForeColor != Color.Empty)
                    textColor = fmtArgs.CellStyle.ForeColor;
                if (fmtArgs.CellStyle.Font != null)
                    cellFont = fmtArgs.CellStyle.Font;
            }
        }

        using var textBrush = new SolidBrush(textColor);
        g.DrawString(text, cellFont, textBrush, Math.Max(cx, clipLeft) + 3, ry + (rowH - Font.Height) / 2);

        if (col is DataGridViewCheckBoxColumn)
        {
            var raw = ri < _boundRows.Count && ci < _boundRows[ri].Length ? _boundRows[ri][ci] : null;
            bool chk = raw is true || (raw is string sv && sv.Equals("true", StringComparison.OrdinalIgnoreCase));
            int cbSize = 13, cbX = cx + (col.Width - cbSize) / 2, cbY = ry + (rowH - cbSize) / 2;
            using var cbPen = new Pen(Color.FromArgb(122, 122, 122));
            g.DrawRectangle(cbPen, cbX, cbY, cbSize, cbSize);
            if (chk)
            {
                using var checkPen = new Pen(Color.FromArgb(0, 120, 215), 2);
                g.DrawLine(checkPen, cbX + 2, cbY + cbSize / 2, cbX + 5, cbY + cbSize - 3);
                g.DrawLine(checkPen, cbX + 5, cbY + cbSize - 3, cbX + cbSize - 2, cbY + 2);
            }
        }

        // ComboBox column: draw dropdown arrow button on the right side of the cell
        if (col is DataGridViewComboBoxColumn)
        {
            int arrowX = Math.Min(right, clipRight) - ComboCellArrowW;
            if (arrowX > Math.Max(cx, clipLeft))
            {
                using var arrowBg = new SolidBrush(Color.FromArgb(220, 220, 220));
                g.FillRectangle(arrowBg, arrowX, ry, ComboCellArrowW, rowH);
                using var arrowPen = new Pen(Color.FromArgb(150, 150, 150));
                g.DrawLine(arrowPen, arrowX, ry, arrowX, ry + rowH);
                // ▼ triangle
                int ax = arrowX + ComboCellArrowW / 2;
                int ay = ry + rowH / 2 - 2;
                using var triPen = new Pen(Color.FromArgb(80, 80, 80), 1);
                g.DrawLine(triPen, ax - 4, ay, ax + 4, ay);
                g.DrawLine(triPen, ax - 4, ay, ax, ay + 4);
                g.DrawLine(triPen, ax + 4, ay, ax, ay + 4);
            }
        }

        using var gridPen = new Pen(GridColor);
        g.DrawLine(gridPen, Math.Min(right - 1, clipRight - 1), ry,
                            Math.Min(right - 1, clipRight - 1), ry + rowH);

        // Red inset border when this cell has a pending validation error
        if (_invalidCell.HasValue && _invalidCell.Value.row == ri && _invalidCell.Value.col == ci)
        {
            int bx = Math.Max(cx, clipLeft);
            int bw2 = Math.Min(right, clipRight) - bx;
            using var errPen = new Pen(Color.Red, 2);
            g.DrawRectangle(errPen, bx + 1, ry + 1, bw2 - 3, rowH - 3);
        }
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        int bw = BorderStyle == BorderStyle.None ? 0 : 2;
        int x0 = bw, y0 = bw;
        int w = Width - bw * 2, h = Height - bw * 2;

        using (var bgBrush = new SolidBrush(BackColor))
            g.FillRectangle(bgBrush, x0, y0, w, h);

        if (BorderStyle != BorderStyle.None)
        {
            using var borderPen = new Pen(Color.FromArgb(122, 122, 122));
            g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }

        int rowHdrW = RowHeadersVisible ? RowHeadersWidth : 0;
        int colHdrH = ColumnHeadersVisible ? ColumnHeadersHeight : 0;
        int frozenW  = FrozenColumnsWidth();
        int frozenH  = FrozenRowsHeight();

        int totalCols  = TotalColumnsWidth();
        int totalRows  = GetDisplayRowCount();
        int rowH       = RowHeightDefault;
        // Scrollable rows exclude the frozen ones
        int scrollableRowsH = (totalRows - FrozenRowCount()) * rowH;

        // Scrollable columns begin after row-header + frozen zone
        int scrollOriginX = x0 + rowHdrW + frozenW;
        // Scrollable rows begin after column header + frozen row zone
        int scrollOriginY = y0 + colHdrH + frozenH;

        bool needScrollV = (ScrollBars == DataGridViewScrollBars.Vertical || ScrollBars == DataGridViewScrollBars.Both)
                           && scrollableRowsH > h - colHdrH - frozenH;
        bool needScrollH = (ScrollBars == DataGridViewScrollBars.Horizontal || ScrollBars == DataGridViewScrollBars.Both)
                           && (totalCols - frozenW) > w - rowHdrW - frozenW;

        int clientW = w - rowHdrW - (needScrollV ? ScrollBarW : 0);
        int clientH = h - colHdrH - (needScrollH ? ScrollBarW : 0);
        int rightEdge = x0 + rowHdrW + clientW;

        // ── Column headers ──────────────────────────────────────
        if (ColumnHeadersVisible)
        {
            using var hdrBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
            g.FillRectangle(hdrBrush, x0 + rowHdrW, y0, clientW, colHdrH);

            // Frozen headers
            int fcx = x0 + rowHdrW;
            for (int ci = 0; ci < Columns.Count; ci++)
            {
                var col = Columns[ci];
                if (!col.Visible || !col.Frozen) continue;
                DrawColumnHeader(g, ci, col, fcx, y0, colHdrH, x0 + rowHdrW, rightEdge);
                fcx += col.Width;
            }
            // Scrollable headers
            int scx = scrollOriginX - _scrollOffsetX;
            for (int ci = 0; ci < Columns.Count; ci++)
            {
                var col = Columns[ci];
                if (!col.Visible || col.Frozen) continue;
                if (scx >= rightEdge) break;
                DrawColumnHeader(g, ci, col, scx, y0, colHdrH, scrollOriginX, rightEdge);
                scx += col.Width;
            }

            // Row header corner
            if (RowHeadersVisible)
            {
                using var cornerBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
                g.FillRectangle(cornerBrush, x0, y0, rowHdrW, colHdrH);
                using var cornerPen = new Pen(Color.FromArgb(166, 166, 166));
                g.DrawLine(cornerPen, x0 + rowHdrW - 1, y0, x0 + rowHdrW - 1, y0 + colHdrH);
                g.DrawLine(cornerPen, x0, y0 + colHdrH - 1, x0 + rowHdrW, y0 + colHdrH - 1);
            }
        }

        // ── Rows ────────────────────────────────────────────────
        // Helper: renders one row at pixel position ry, clipped within [clipTop, clipBottom)
        void DrawRow(int ri, int ry, int clipTop, int clipBottom)
        {
            if (ry + rowH <= clipTop || ry >= clipBottom) return;

            bool rowSelected = ri == _selectedRowIndex;
            bool rowHovered  = ri == _hoveredRow;

            Color rowBg = BackColor;
            if (ri % 2 == 1 && AlternatingRowsDefaultCellStyle.BackColor != Color.Empty)
                rowBg = AlternatingRowsDefaultCellStyle.BackColor;
            if (rowHovered)  rowBg = Color.FromArgb(229, 241, 251);
            if (rowSelected) rowBg = Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(204, 228, 247);

            using (var rowBrush = new SolidBrush(rowBg))
                g.FillRectangle(rowBrush, x0 + rowHdrW, ry, clientW, rowH);

            if (RowHeadersVisible)
            {
                using var rHdrBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
                g.FillRectangle(rHdrBrush, x0, ry, rowHdrW, rowH);
                if (rowSelected)
                {
                    using var triPen = new Pen(Color.FromArgb(0, 90, 158), 2);
                    int mx2 = x0 + rowHdrW / 2, my2 = ry + rowH / 2;
                    g.DrawLine(triPen, mx2 - 4, my2 - 4, mx2 + 4, my2);
                    g.DrawLine(triPen, mx2 + 4, my2, mx2 - 4, my2 + 4);
                }
                using var rHdrPen = new Pen(Color.FromArgb(166, 166, 166));
                g.DrawLine(rHdrPen, x0 + rowHdrW - 1, ry, x0 + rowHdrW - 1, ry + rowH);
                g.DrawLine(rHdrPen, x0, ry + rowH - 1, x0 + rowHdrW, ry + rowH - 1);
            }

            // Frozen cells
            int fCellX = x0 + rowHdrW;
            for (int ci = 0; ci < Columns.Count; ci++)
            {
                var col = Columns[ci];
                if (!col.Visible || !col.Frozen) continue;
                DrawCell(g, ri, ci, col, fCellX, ry, rowH, rowSelected, rowBg,
                         x0 + rowHdrW, rightEdge);
                fCellX += col.Width;
            }

            // Scrollable cells
            int sCellX = scrollOriginX - _scrollOffsetX;
            for (int ci = 0; ci < Columns.Count; ci++)
            {
                var col = Columns[ci];
                if (!col.Visible || col.Frozen) continue;
                if (sCellX >= rightEdge) break;
                DrawCell(g, ri, ci, col, sCellX, ry, rowH, rowSelected, rowBg,
                         scrollOriginX, rightEdge);
                sCellX += col.Width;
            }

            using var rowPen = new Pen(GridColor);
            g.DrawLine(rowPen, x0 + rowHdrW, ry + rowH - 1, x0 + rowHdrW + clientW, ry + rowH - 1);
        }

        int displayCount = GetDisplayRowCount();

        // Pass 1 — frozen rows (fixed Y, not affected by _scrollOffsetY)
        int fry = y0 + colHdrH;
        for (int ri = 0; ri < displayCount; ri++)
        {
            bool rowFrozen = ri < Rows.Count && Rows[ri].Frozen && Rows[ri].Visible;
            if (!rowFrozen) continue;
            DrawRow(ri, fry, y0 + colHdrH, scrollOriginY + clientH);
            fry += rowH;
        }

        // Pass 2 — scrollable rows (offset by _scrollOffsetY, clipped below frozen zone)
        int ry = scrollOriginY - _scrollOffsetY;
        for (int ri = 0; ri < displayCount; ri++)
        {
            bool rowFrozen = ri < Rows.Count && Rows[ri].Frozen && Rows[ri].Visible;
            if (rowFrozen) continue; // already drawn in pass 1
            if (ry + rowH <= scrollOriginY) { ry += rowH; continue; }  // scrolled above frozen zone
            if (ry >= scrollOriginY + clientH) break;
            DrawRow(ri, ry, scrollOriginY, scrollOriginY + clientH);
            ry += rowH;
        }

        // Frozen column separator
        if (frozenW > 0)
        {
            using var frozenPen = new Pen(Color.FromArgb(100, 100, 100));
            g.DrawLine(frozenPen, scrollOriginX, y0, scrollOriginX, y0 + colHdrH + frozenH + clientH);
        }

        // Frozen row separator
        if (frozenH > 0)
        {
            using var frozenRowPen = new Pen(Color.FromArgb(100, 100, 100));
            g.DrawLine(frozenRowPen, x0 + rowHdrW, scrollOriginY, x0 + rowHdrW + clientW, scrollOriginY);
        }

        // ── Inline TextBox cell edit overlay ─────────────────────
        if (_editing && _editRow >= 0 && _editCol >= 0 && _editCol < Columns.Count)
        {
            var (editCellX, editCellBottom, editCellW) = GetCellScreenRect(_editRow, _editCol);
            int editCellTop = editCellBottom - RowHeightDefault;
            int editCellH   = RowHeightDefault;

            // White fill over the cell
            using var editBg = new SolidBrush(Color.White);
            g.FillRectangle(editBg, editCellX, editCellTop, editCellW - 1, editCellH);
            using var editBorder = new Pen(Color.FromArgb(0, 120, 215), 2);
            g.DrawRectangle(editBorder, editCellX, editCellTop, editCellW - 1, editCellH - 1);

            // Draw text and caret
            using var editTextBrush = new SolidBrush(ForeColor);
            int textX = editCellX + 3;
            int textY = editCellTop + (editCellH - Font.Height) / 2;
            g.DrawString(_editText, Font, editTextBrush, textX, textY);

            // Caret — approximate position using average char width
            string textBeforeCursor = _editText[.._editCursorPos];
            int avgCharW = Math.Max(1, Font.Height / 2);
            int caretX = textX + textBeforeCursor.Length * avgCharW;
            using var caretPen = new Pen(ForeColor);
            g.DrawLine(caretPen, caretX, textY, caretX, textY + Font.Height);
        }

        // ── ComboBox in-cell dropdown overlay ────────────────────
        if (_comboOpen && _comboCol >= 0 && _comboCol < Columns.Count
            && Columns[_comboCol] is DataGridViewComboBoxColumn comboCol)
        {
            var items = GetComboItems(comboCol);
            int dropH = items.Count * ComboDropItemH + 2;
            int dropW  = Columns[_comboCol].Width;

            using var dropBg   = new SolidBrush(Color.White);
            using var dropPen  = new Pen(Color.FromArgb(122, 122, 122));
            g.FillRectangle(dropBg, _comboCellX, _comboCellY, dropW, dropH);
            g.DrawRectangle(dropPen, _comboCellX, _comboCellY, dropW - 1, dropH - 1);

            for (int i = 0; i < items.Count; i++)
            {
                int iy = _comboCellY + 1 + i * ComboDropItemH;
                if (i == _comboHoverIdx)
                {
                    using var selBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                    g.FillRectangle(selBrush, _comboCellX + 1, iy, dropW - 2, ComboDropItemH);
                    using var selText = new SolidBrush(Color.White);
                    g.DrawString(items[i].ToString() ?? string.Empty, Font, selText, _comboCellX + 4, iy + 2);
                }
                else
                {
                    using var itemText = new SolidBrush(ForeColor);
                    g.DrawString(items[i].ToString() ?? string.Empty, Font, itemText, _comboCellX + 4, iy + 2);
                }
            }
        }

        // ── Scrollbars ───────────────────────────────────────────
        if (needScrollV)
        {
            int sbX = x0 + w - ScrollBarW;
            int sbH = h - (needScrollH ? ScrollBarW : 0);
            using var sbBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
            g.FillRectangle(sbBrush, sbX, y0, ScrollBarW, sbH);
            using var sbPen = new Pen(Color.FromArgb(166, 166, 166));
            g.DrawRectangle(sbPen, sbX, y0, ScrollBarW - 1, sbH - 1);
            int maxScroll = Math.Max(1, scrollableRowsH - clientH);
            int thumbH = Math.Max(20, (int)((double)clientH / Math.Max(1, scrollableRowsH) * sbH));
            int thumbY = y0 + (int)((double)_scrollOffsetY / maxScroll * (sbH - thumbH));
            using var thumbBrush = new SolidBrush(Color.FromArgb(180, 180, 180));
            g.FillRectangle(thumbBrush, sbX + 2, thumbY + 2, ScrollBarW - 4, thumbH - 4);
        }
    }

    // ── Input ────────────────────────────────────────────────────

    /// <summary>
    /// Fires <see cref="CellValidating"/> then <see cref="RowValidating"/> for the current cell
    /// before a selection change.  Returns <c>false</c> (and marks <see cref="_invalidCell"/>) if
    /// either handler cancels; returns <c>true</c> when validation passes and clears the flag.
    /// </summary>
    private bool TryCommitCell(int fromRow, int fromCol)
    {
        if (fromRow < 0 || fromCol < 0) return true;

        var cellArgs = new DataGridViewCellCancelEventArgs(fromCol, fromRow);
        CellValidating?.Invoke(this, cellArgs);
        if (cellArgs.Cancel)
        {
            _invalidCell = (fromRow, fromCol);
            Invalidate();
            return false;
        }

        var rowArgs = new DataGridViewCellCancelEventArgs(fromCol, fromRow);
        RowValidating?.Invoke(this, rowArgs);
        if (rowArgs.Cancel)
        {
            _invalidCell = (fromRow, fromCol);
            Invalidate();
            return false;
        }

        _invalidCell = null;
        return true;
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        int bw = BorderStyle == BorderStyle.None ? 0 : 2;
        int rowHdrW = RowHeadersVisible ? RowHeadersWidth : 0;
        int colHdrH = ColumnHeadersVisible ? ColumnHeadersHeight : 0;

        int mx = e.X - bw, my = e.Y - bw;

        // ── If a combo dropdown is open, handle clicks inside it first ──
        if (_comboOpen && _comboCol >= 0 && _comboCol < Columns.Count
            && Columns[_comboCol] is DataGridViewComboBoxColumn openComboCol)
        {
            var items = GetComboItems(openComboCol);
            int dropH = items.Count * ComboDropItemH + 2;
            int dropW  = Columns[_comboCol].Width;
            int dx = mx + bw, dy = my + bw;  // e.X, e.Y directly
            if (dx >= _comboCellX && dx < _comboCellX + dropW
                && dy >= _comboCellY && dy < _comboCellY + dropH)
            {
                int clickedItem = (dy - _comboCellY - 1) / ComboDropItemH;
                CommitComboItem(clickedItem);
                return;
            }
            // Click outside the dropdown → close without commit
            CloseComboDropdown();
        }

        // ── Commit any active inline edit on click-away ───────────────────
        if (_editing && e.Button == MouseButtons.Left)
        {
            bool inEditCell = false;
            if (my >= colHdrH)
            {
                int ri2 = GetRowAtY(my - colHdrH + bw);
                int ci2 = GetColAtX(mx - rowHdrW);
                inEditCell = ri2 == _editRow && ci2 == _editCol;
            }
            if (!inEditCell) EndEdit(commit: true);
        }

        // ── Column header area ────────────────────────────────────────────
        if (ColumnHeadersVisible && my < colHdrH)
        {
            // Check for column resize handle first (priority over sort click)
            if (AllowUserToResizeColumns && e.Button == MouseButtons.Left)
            {
                int resizeCol = GetResizeColumnAtX(mx - rowHdrW);
                if (resizeCol >= 0)
                {
                    _resizingCol   = true;
                    _resizeColIndex = resizeCol;
                    _resizeStartX  = e.X;
                    _resizeStartW  = Columns[resizeCol].Width;
                    return;
                }
            }

            int ci = GetColAtX(mx - rowHdrW);
            if (ci >= 0)
            {
                ColumnHeaderMouseClick?.Invoke(this, new DataGridViewColumnEventArgs(Columns[ci]));
                if (Columns[ci].SortMode != DataGridViewColumnSortMode.NotSortable)
                {
                    bool isCtrl = (System.Windows.Forms.Control.ModifierKeys & Keys.Control) != 0;
                    if (isCtrl && _sortedColumns.Count > 0)
                    {
                        var existing = _sortedColumns.FindIndex(s => s.ColumnIndex == ci);
                        var dir2 = (existing >= 0 && _sortedColumns[existing].Direction == ListSortDirection.Ascending)
                            ? ListSortDirection.Descending : ListSortDirection.Ascending;
                        AddSort(Columns[ci], dir2);
                    }
                    else
                    {
                        var dir = (_sortColIndex == ci && _sortOrder == SortOrder.Ascending)
                            ? ListSortDirection.Descending : ListSortDirection.Ascending;
                        Sort(Columns[ci], dir);
                    }
                }
            }
            return;
        }

        // Row click
        int ri = GetRowAtY(my - colHdrH + bw);
        int col = GetColAtX(mx - rowHdrW);
        if (ri >= 0 && ri < GetDisplayRowCount())
        {
            // Validate the current cell before moving selection
            if (!TryCommitCell(_selectedCell.row, _selectedCell.col))
                return; // handler cancelled — keep current selection

            _selectedRowIndex = ri;
            _selectedColIndex = col;
            _selectedCell = (ri, col >= 0 ? col : 0);
            SelectionChanged?.Invoke(this, EventArgs.Empty);
            if (col >= 0) CellClick?.Invoke(this, new DataGridViewCellEventArgs(col, ri));

            // Single-click on a CheckBox column cell toggles the boolean value (WinForms behaviour)
            if (col >= 0 && col < Columns.Count && Columns[col] is DataGridViewCheckBoxColumn && !ReadOnly)
            {
                ToggleCheckBoxCell(ri, col);
                return;
            }

            // Double-click on a ComboBox column cell opens the dropdown
            if (col >= 0 && col < Columns.Count && Columns[col] is DataGridViewComboBoxColumn
                && e.Clicks >= 2)
            {
                OpenComboDropdown(ri, col);
                return;
            }

            // Double-click on a text cell begins inline editing (WinForms: EditOnDoubleClick or EditOnKeystrokeOrF2)
            if (col >= 0 && col < Columns.Count && !ReadOnly
                && Columns[col] is not DataGridViewCheckBoxColumn
                && Columns[col] is not DataGridViewComboBoxColumn
                && (e.Clicks >= 2 || EditMode == DataGridViewEditMode.EditOnEnter))
            {
                BeginEdit(ri, col);
                return;
            }

            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        int bw = BorderStyle == BorderStyle.None ? 0 : 2;
        int rowHdrW = RowHeadersVisible ? RowHeadersWidth : 0;
        int colHdrH = ColumnHeadersVisible ? ColumnHeadersHeight : 0;
        int my = e.Y - bw;

        // ── Active column resize drag ─────────────────────────────────────
        if (_resizingCol)
        {
            int delta = e.X - _resizeStartX;
            int newW  = Math.Max(5, _resizeStartW + delta);
            Columns[_resizeColIndex].Width = newW;
            Invalidate();
            return;
        }

        // Track hover inside open combo dropdown
        if (_comboOpen && _comboCol >= 0 && _comboCol < Columns.Count
            && Columns[_comboCol] is DataGridViewComboBoxColumn openComboCol2)
        {
            var items2 = GetComboItems(openComboCol2);
            int dropW2 = Columns[_comboCol].Width;
            int dx = e.X, dy = e.Y;
            int newHover = -1;
            if (dx >= _comboCellX && dx < _comboCellX + dropW2
                && dy >= _comboCellY && dy < _comboCellY + items2.Count * ComboDropItemH + 2)
            {
                newHover = (dy - _comboCellY - 1) / ComboDropItemH;
                if (newHover < 0 || newHover >= items2.Count) newHover = -1;
            }
            if (newHover != _comboHoverIdx) { _comboHoverIdx = newHover; Invalidate(); }
        }

        // ── Show resize cursor when hovering over a column boundary ───────
        if (ColumnHeadersVisible && AllowUserToResizeColumns && my < colHdrH)
        {
            int mx2 = e.X - bw - rowHdrW;
            Cursor = GetResizeColumnAtX(mx2) >= 0 ? Cursor.SizeWE : Cursor.Default;
        }
        else
        {
            Cursor = Cursor.Default;
        }

        if (my < colHdrH) { if (_hoveredRow != -1) { _hoveredRow = -1; Invalidate(); } return; }
        int ri = GetRowAtY(my - colHdrH + bw);
        if (ri != _hoveredRow)
        base.OnMouseMove(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        if (_resizingCol)
        {
            _resizingCol    = false;
            _resizeColIndex = -1;
            Cursor = Cursor.Default;
            Invalidate();
        }
        base.OnMouseUp(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_hoveredRow != -1) { _hoveredRow = -1; Invalidate(); }
        base.OnMouseLeave(e);
    }

    protected internal override void OnMouseWheel(MouseEventArgs e)
    {
        int rowH = RowHeightDefault;
        int colHdrH = ColumnHeadersVisible ? ColumnHeadersHeight : 0;
        int bw = BorderStyle == BorderStyle.None ? 0 : 2;
        int clientH = Height - bw * 2 - colHdrH - FrozenRowsHeight() - ScrollBarW;
        int scrollableH = (GetDisplayRowCount() - FrozenRowCount()) * rowH;
        int maxScroll = Math.Max(0, scrollableH - clientH);
        _scrollOffsetY = Math.Clamp(_scrollOffsetY - Math.Sign(e.Delta) * rowH * 3, 0, maxScroll);
        Invalidate();
        base.OnMouseWheel(e);
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        // ── Active inline text edit keyboard handling ─────────────────────
        if (_editing)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    EndEdit(commit: false);
                    e.Handled = true;
                    return;
                case Keys.Enter:
                    EndEdit(commit: true);
                    e.Handled = true;
                    return;
                case Keys.Back:
                    if (_editCursorPos > 0)
                    {
                        _editText = _editText.Remove(_editCursorPos - 1, 1);
                        _editCursorPos--;
                        Invalidate();
                    }
                    e.Handled = true;
                    return;
                case Keys.Delete:
                    if (_editCursorPos < _editText.Length)
                    {
                        _editText = _editText.Remove(_editCursorPos, 1);
                        Invalidate();
                    }
                    e.Handled = true;
                    return;
                case Keys.Left:
                    if (_editCursorPos > 0) { _editCursorPos--; Invalidate(); }
                    e.Handled = true;
                    return;
                case Keys.Right:
                    if (_editCursorPos < _editText.Length) { _editCursorPos++; Invalidate(); }
                    e.Handled = true;
                    return;
                case Keys.Home:
                    _editCursorPos = 0;
                    Invalidate();
                    e.Handled = true;
                    return;
                case Keys.End:
                    _editCursorPos = _editText.Length;
                    Invalidate();
                    e.Handled = true;
                    return;
            }
            // Printable key → insert character at cursor
            char ch = KeysToChar(e);
            if (ch != '\0')
            {
                _editText = _editText.Insert(_editCursorPos, ch.ToString());
                _editCursorPos++;
                Invalidate();
                e.Handled = true;
                return;
            }
        }

        // ComboBox dropdown keyboard navigation
        if (_comboOpen && _comboCol >= 0 && _comboCol < Columns.Count
            && Columns[_comboCol] is DataGridViewComboBoxColumn kbComboCol)
        {
            var kbItems = GetComboItems(kbComboCol);
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    CloseComboDropdown();
                    e.Handled = true;
                    return;
                case Keys.Enter:
                    if (_comboHoverIdx >= 0) CommitComboItem(_comboHoverIdx);
                    else CloseComboDropdown();
                    e.Handled = true;
                    return;
                case Keys.Up:
                    _comboHoverIdx = Math.Max(0, _comboHoverIdx <= 0 ? kbItems.Count - 1 : _comboHoverIdx - 1);
                    Invalidate();
                    e.Handled = true;
                    return;
                case Keys.Down:
                    _comboHoverIdx = (_comboHoverIdx + 1) % kbItems.Count;
                    Invalidate();
                    e.Handled = true;
                    return;
            }
        }

        // F2 opens dropdown on focused ComboBox column cell; begins edit for TextBox cells
        if (e.KeyCode == Keys.F2 && _selectedCell.row >= 0 && _selectedCell.col >= 0
            && _selectedCell.col < Columns.Count)
        {
            if (!_comboOpen && Columns[_selectedCell.col] is DataGridViewComboBoxColumn)
            {
                OpenComboDropdown(_selectedCell.row, _selectedCell.col);
                e.Handled = true;
                return;
            }
            if (!_editing
                && Columns[_selectedCell.col] is not DataGridViewCheckBoxColumn
                && Columns[_selectedCell.col] is not DataGridViewComboBoxColumn
                && !ReadOnly)
            {
                BeginEdit(_selectedCell.row, _selectedCell.col);
                e.Handled = true;
                return;
            }
        }

        // Keystroke on a text cell starts editing immediately (EditOnKeystrokeOrF2)
        if (!_editing && !_comboOpen
            && _selectedCell.row >= 0 && _selectedCell.col >= 0
            && _selectedCell.col < Columns.Count && !ReadOnly
            && Columns[_selectedCell.col] is not DataGridViewCheckBoxColumn
            && Columns[_selectedCell.col] is not DataGridViewComboBoxColumn
            && (EditMode == DataGridViewEditMode.EditOnKeystroke
                || EditMode == DataGridViewEditMode.EditOnKeystrokeOrF2))
        {
            char startChar = KeysToChar(e);
            if (startChar != '\0')
            {
                BeginEdit(_selectedCell.row, _selectedCell.col);
                // Replace text with this first character (WinForms: typing replaces cell value)
                _editText = startChar.ToString();
                _editCursorPos = 1;
                Invalidate();
                e.Handled = true;
                return;
            }
        }

        // Space bar toggles a focused CheckBox column cell (WinForms behaviour)
        if (e.KeyCode == Keys.Space
            && _selectedCell.row >= 0 && _selectedCell.col >= 0
            && _selectedCell.col < Columns.Count
            && Columns[_selectedCell.col] is DataGridViewCheckBoxColumn
            && !ReadOnly)
        {
            ToggleCheckBoxCell(_selectedCell.row, _selectedCell.col);
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.C && e.Control)
        {
            string text = GetClipboardContent();
            if (!string.IsNullOrEmpty(text))
                Clipboard.SetText(text);
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    /// <summary>
    /// Converts a <see cref="Keys"/> value to a printable character, respecting Shift.
    /// Returns '\0' for non-printable keys.
    /// </summary>
    private static char KeysToChar(KeyEventArgs e)
    {
        bool shift = e.Shift;
        int k = (int)e.KeyCode;

        if (k >= (int)Keys.A && k <= (int)Keys.Z)
            return shift ? (char)k : char.ToLower((char)k);

        if (k >= (int)Keys.D0 && k <= (int)Keys.D9)
        {
            char[] shiftedDigits = { ')', '!', '@', '#', '$', '%', '^', '&', '*', '(' };
            return shift ? shiftedDigits[k - (int)Keys.D0] : (char)('0' + k - (int)Keys.D0);
        }

        if (k >= (int)Keys.NumPad0 && k <= (int)Keys.NumPad9)
            return (char)('0' + k - (int)Keys.NumPad0);

        return e.KeyCode switch
        {
            Keys.Space    => ' ',
            Keys.Multiply => '*',
            Keys.Add      => '+',
            Keys.Subtract => '-',
            Keys.Divide   => '/',
            Keys.Decimal  => '.',
            _ => '\0'
        };
    }

    /// <summary>
    /// Builds tab-separated clipboard text for the current selection (or all rows if nothing is
    /// selected), honouring <see cref="ClipboardCopyMode"/>.
    /// </summary>
    public string GetClipboardContent()
    {
        if (ClipboardCopyMode == DataGridViewClipboardCopyMode.Disable) return string.Empty;

        bool includeHeaders = ClipboardCopyMode == DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText
            || (ClipboardCopyMode == DataGridViewClipboardCopyMode.EnableWithAutoHeaderText
                && SelectionMode == DataGridViewSelectionMode.FullRowSelect);

        var sb = new System.Text.StringBuilder();

        // Header row
        if (includeHeaders)
        {
            for (int ci = 0; ci < Columns.Count; ci++)
            {
                if (!Columns[ci].Visible) continue;
                if (sb.Length > 0) sb.Append('\t');
                sb.Append(Columns[ci].HeaderText);
            }
            sb.AppendLine();
        }

        // Data rows — copy selected row when available, else all rows
        int rowCount = GetDisplayRowCount();
        bool hasSelection = _selectedRowIndex >= 0 && _selectedRowIndex < rowCount;
        for (int ri = 0; ri < rowCount; ri++)
        {
            if (hasSelection && SelectionMode == DataGridViewSelectionMode.FullRowSelect
                && ri != _selectedRowIndex)
                continue;

            bool firstCell = true;
            for (int ci = 0; ci < Columns.Count; ci++)
            {
                if (!Columns[ci].Visible) continue;
                if (!firstCell) sb.Append('\t');
                firstCell = false;
                sb.Append(GetCellText(ri, ci));
            }
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    private int GetRowAtY(int relY)
    {
        if (relY < 0) return -1;
        int rowH = RowHeightDefault;
        int frozenH = FrozenRowsHeight();
        int displayCount = GetDisplayRowCount();

        // Check frozen rows first (fixed zone above the scrollable area)
        int fy = 0;
        for (int ri = 0; ri < displayCount; ri++)
        {
            bool rowFrozen = ri < Rows.Count && Rows[ri].Frozen && Rows[ri].Visible;
            if (!rowFrozen) continue;
            if (relY >= fy && relY < fy + rowH) return ri;
            fy += rowH;
        }

        // Then scrollable rows (relY is relative to the top of the scrollable zone)
        int scrollableRelY = relY - frozenH + _scrollOffsetY;
        if (scrollableRelY < 0) return -1;
        int scrollableIdx = scrollableRelY / rowH;
        // Map scrollableIdx back to the actual row index (skip frozen rows)
        int skipped = 0;
        for (int ri = 0; ri < displayCount; ri++)
        {
            bool rowFrozen = ri < Rows.Count && Rows[ri].Frozen && Rows[ri].Visible;
            if (rowFrozen) continue;
            if (skipped == scrollableIdx) return ri;
            skipped++;
        }
        return -1;
    }

    // mouseX is relative to the grid interior (bw already removed), rowHdrW already removed.
    // Frozen columns are at fixed positions; scrollable columns are offset by _scrollOffsetX.
    private int GetColAtX(int mouseX)
    {
        // Check frozen columns first (they sit at the left, no scroll offset)
        int fcx = 0;
        for (int ci = 0; ci < Columns.Count; ci++)
        {
            var col = Columns[ci];
            if (!col.Visible || !col.Frozen) continue;
            if (mouseX >= fcx && mouseX < fcx + col.Width) return ci;
            fcx += col.Width;
        }
        // Then scrollable columns
        int scx = fcx - _scrollOffsetX;
        for (int ci = 0; ci < Columns.Count; ci++)
        {
            var col = Columns[ci];
            if (!col.Visible || col.Frozen) continue;
            if (mouseX >= scx && mouseX < scx + col.Width) return ci;
            scx += col.Width;
        }
        return -1;
    }

    /// <summary>
    /// Returns the index of the column whose right edge is within <see cref="ResizeHandleW"/>
    /// pixels of <paramref name="mouseX"/> (relative to left of column area, rowHdrW removed).
    /// Returns -1 if no column boundary is close enough, or if the column is not resizable.
    /// </summary>
    private int GetResizeColumnAtX(int mouseX)
    {
        // Check frozen columns
        int fcx = 0;
        for (int ci = 0; ci < Columns.Count; ci++)
        {
            var col = Columns[ci];
            if (!col.Visible || !col.Frozen) continue;
            int rightEdge = fcx + col.Width;
            if (Math.Abs(mouseX - rightEdge) <= ResizeHandleW && col.Resizable)
                return ci;
            fcx += col.Width;
        }
        // Check scrollable columns
        int scx = fcx - _scrollOffsetX;
        for (int ci = 0; ci < Columns.Count; ci++)
        {
            var col = Columns[ci];
            if (!col.Visible || col.Frozen) continue;
            int rightEdge = scx + col.Width;
            if (Math.Abs(mouseX - rightEdge) <= ResizeHandleW && col.Resizable)
                return ci;
            scx += col.Width;
        }
        return -1;
    }

    // ── ComboBox helpers ─────────────────────────────────────────

    private static List<object> GetComboItems(DataGridViewComboBoxColumn col)
    {
        if (col.DataSource is System.Collections.IEnumerable ds)
        {
            var list = new List<object>();
            foreach (var item in ds) list.Add(item);
            return list;
        }
        return col.Items.Cast<object>().ToList();
    }

    /// <summary>
    /// Returns the pixel X/Y (relative to control top-left) of the named cell's left/bottom edges.
    /// Used to position the combo dropdown.
    /// </summary>
    private (int cellX, int cellBottom, int cellWidth) GetCellScreenRect(int ri, int ci)
    {
        int bw = BorderStyle == BorderStyle.None ? 0 : 2;
        int rowHdrW  = RowHeadersVisible ? RowHeadersWidth : 0;
        int colHdrH  = ColumnHeadersVisible ? ColumnHeadersHeight : 0;
        int frozenH  = FrozenRowsHeight();
        int frozenW  = FrozenColumnsWidth();
        int scrollOriginX = bw + rowHdrW + frozenW;
        int scrollOriginY = bw + colHdrH + frozenH;

        // Column X
        int cx = 0;
        bool frozen = ci < Columns.Count && Columns[ci].Frozen;
        if (frozen)
        {
            int fx = bw + rowHdrW;
            for (int c = 0; c < ci; c++)
                if (Columns[c].Visible && Columns[c].Frozen) fx += Columns[c].Width;
            cx = fx;
        }
        else
        {
            int sx = scrollOriginX - _scrollOffsetX;
            for (int c = 0; c < Columns.Count; c++)
            {
                var col = Columns[c];
                if (!col.Visible || col.Frozen) continue;
                if (c == ci) { cx = sx; break; }
                sx += col.Width;
            }
        }

        // Row Y
        int ry;
        bool rowFrozen = ri < Rows.Count && Rows[ri].Frozen && Rows[ri].Visible;
        if (rowFrozen)
        {
            ry = bw + colHdrH;
            for (int r = 0; r < ri; r++)
                if (r < Rows.Count && Rows[r].Frozen && Rows[r].Visible) ry += RowHeightDefault;
        }
        else
        {
            ry = scrollOriginY - _scrollOffsetY;
            int scrollable = 0;
            for (int r = 0; r < GetDisplayRowCount(); r++)
            {
                bool rf = r < Rows.Count && Rows[r].Frozen && Rows[r].Visible;
                if (rf) continue;
                if (r == ri) break;
                scrollable++;
            }
            ry += scrollable * RowHeightDefault;
        }

        int colW = ci < Columns.Count ? Columns[ci].Width : 100;
        return (cx, ry + RowHeightDefault, colW);
    }

    private void OpenComboDropdown(int ri, int ci)
    {
        if (ci >= Columns.Count || Columns[ci] is not DataGridViewComboBoxColumn) return;
        _comboRow  = ri;
        _comboCol  = ci;
        _comboOpen = true;
        _comboHoverIdx = -1;
        var (cx, cy, _) = GetCellScreenRect(ri, ci);
        _comboCellX = cx;
        _comboCellY = cy;
        Invalidate();
    }

    private void CloseComboDropdown() { _comboOpen = false; Invalidate(); }

    private void CommitComboItem(int itemIdx)
    {
        if (_comboCol < 0 || _comboCol >= Columns.Count) return;
        if (Columns[_comboCol] is not DataGridViewComboBoxColumn comboCol) return;
        var items = GetComboItems(comboCol);
        if (itemIdx < 0 || itemIdx >= items.Count) return;
        SetCellValue(_comboRow, _comboCol, items[itemIdx]);
        CloseComboDropdown();
    }

    // ── Programmatic selection ────────────────────────────────────

    public void ClearSelection()
    {
        _selectedRowIndex = -1;
        _selectedColIndex = -1;
        _selectedCell = (-1, -1);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    /// <summary>
    /// Returns the currently selected rows as DataGridViewRow objects or bound row indices.
    /// </summary>
    public IReadOnlyList<int> SelectedRowIndices =>
        _selectedRowIndex >= 0 ? new[] { _selectedRowIndex } : Array.Empty<int>();

    // ── WinForms selection API ────────────────────────────────────

    /// <summary>Returns a collection of selected <see cref="DataGridViewRow"/> objects.</summary>
    public DataGridViewSelectedRowCollection SelectedRows
    {
        get
        {
            var coll = new DataGridViewSelectedRowCollection();
            if (_selectedRowIndex >= 0 && _selectedRowIndex < Rows.Count)
                coll.Add(Rows[_selectedRowIndex]);
            return coll;
        }
    }

    /// <summary>Returns a collection of selected <see cref="DataGridViewCell"/> objects.</summary>
    public DataGridViewSelectedCellCollection SelectedCells
    {
        get
        {
            var coll = new DataGridViewSelectedCellCollection();
            var (r, c) = _selectedCell;
            if (r >= 0 && r < Rows.Count && c >= 0 && c < Columns.Count)
                coll.Add(Rows[r].Cells[c]);
            return coll;
        }
    }

    /// <summary>Returns a collection of selected <see cref="DataGridViewColumn"/> objects.</summary>
    public DataGridViewSelectedColumnCollection SelectedColumns
    {
        get
        {
            var coll = new DataGridViewSelectedColumnCollection();
            if (_selectedColIndex >= 0 && _selectedColIndex < Columns.Count)
                coll.Add(Columns[_selectedColIndex]);
            return coll;
        }
    }

    /// <summary>Gets the row containing the current cell, or null if no cell is current.</summary>
    public DataGridViewRow? CurrentRow =>
        _selectedCell.row >= 0 && _selectedCell.row < Rows.Count
            ? Rows[_selectedCell.row]
            : null;

    /// <summary>Gets or sets the currently active cell.</summary>
    public DataGridViewCell? CurrentCell
    {
        get
        {
            var (r, c) = _selectedCell;
            if (r >= 0 && r < Rows.Count && c >= 0 && c < Rows[r].Cells.Count)
                return Rows[r].Cells[c];
            return null;
        }
        set
        {
            if (value == null) { _selectedCell = (-1, -1); return; }
            // locate the cell in the rows collection
            for (int ri = 0; ri < Rows.Count; ri++)
            {
                var row = Rows[ri];
                for (int ci = 0; ci < row.Cells.Count; ci++)
                {
                    if (row.Cells[ci] == value)
                    {
                        _selectedCell     = (ri, ci);
                        _selectedRowIndex = ri;
                        _selectedColIndex = ci;
                        SelectionChanged?.Invoke(this, EventArgs.Empty);
                        Invalidate();
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the template used to create new rows.
    /// Setting a custom template stores it for use when new rows are added.
    /// </summary>
    public DataGridViewRow RowTemplate { get; set; } = new DataGridViewRow();

    /// <summary>Selects all cells (or rows, depending on SelectionMode).</summary>
    public void SelectAll()
    {
        if (Rows.Count == 0) return;
        _selectedRowIndex = 0;
        _selectedCell     = (0, 0);
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    /// <summary>Returns the number of selected cells with the given inclusion flag.</summary>
    public int GetCellCount(DataGridViewElementStates includeFilter)
        => (int)(includeFilter & DataGridViewElementStates.Selected) != 0
            ? (_selectedCell.row >= 0 ? 1 : 0)
            : 0;

    /// <summary>Returns true when all cells satisfy the given state mask.</summary>
    public bool AreAllCellsSelected(bool includeInvisible)
        => SelectedCells.Count >= Rows.Count * Columns.Count;

    // ── Column / row auto-sizing ──────────────────────────────────

    /// <summary>Auto-resizes all columns to fit their content (heuristic).</summary>
    public void AutoResizeColumns()
        => AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

    public void AutoResizeColumns(DataGridViewAutoSizeColumnsMode mode)
    {
        foreach (var col in Columns)
            AutoResizeColumn(col.Index);
    }

    public void AutoResizeColumn(int columnIndex)
        => AutoResizeColumn(columnIndex, DataGridViewAutoSizeColumnMode.AllCells);

    public void AutoResizeColumn(int columnIndex, DataGridViewAutoSizeColumnMode mode)
    {
        if (columnIndex < 0 || columnIndex >= Columns.Count) return;
        var col = Columns[columnIndex];
        int maxLen = col.HeaderText?.Length ?? 0;
        foreach (var row in Rows)
        {
            var text = columnIndex < row.Cells.Count ? row.Cells[columnIndex].Value?.ToString() ?? "" : "";
            if (text.Length > maxLen) maxLen = text.Length;
        }
        col.Width = Math.Max(50, maxLen * 8 + 16);
        Invalidate();
    }

    public void AutoResizeRows() => Invalidate(); // row height is fixed in this implementation
    public void AutoResizeRow(int rowIndex) => Invalidate();

    // ── Scrolling helpers ─────────────────────────────────────────

    public int FirstDisplayedScrollingRowIndex
    {
        get => _scrollOffsetY / RowHeightDefault;
        set { _scrollOffsetY = Math.Max(0, value * RowHeightDefault); Invalidate(); }
    }

    public int FirstDisplayedScrollingColumnIndex
    {
        get => 0;
        set { }
    }

    public void ScrollIntoView(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= RowCount) return;
        int y = rowIndex * RowHeightDefault;
        if (y < _scrollOffsetY) _scrollOffsetY = y;
        int visibleH = Height - (ColumnHeadersVisible ? ColumnHeadersHeight : 0);
        if (y + RowHeightDefault > _scrollOffsetY + visibleH)
            _scrollOffsetY = y + RowHeightDefault - visibleH;
        Invalidate();
    }

    public void InvalidateRow(int rowIndex) => Invalidate();
    public void InvalidateCell(int columnIndex, int rowIndex) => Invalidate();
    public void UpdateCellValue(int columnIndex, int rowIndex) => Invalidate();

    public object? GetCellValue(int rowIndex, int colIndex)
    {
        if (_boundRows.Count > 0)
        {
            if (rowIndex >= _boundRows.Count || colIndex >= _boundRows[rowIndex].Length) return null;
            return _boundRows[rowIndex][colIndex];
        }
        if (rowIndex < Rows.Count && colIndex < Rows[rowIndex].Cells.Count)
            return Rows[rowIndex].Cells[colIndex].Value;
        return null;
    }

    /// <summary>
    /// Toggles the boolean value of a DataGridViewCheckBoxColumn cell and fires CellValueChanged.
    /// Matches WinForms single-click / Space-key toggle behaviour.
    /// </summary>
    private void ToggleCheckBoxCell(int rowIndex, int colIndex)
    {
        object? raw = null;
        if (_boundRows.Count > 0)
            raw = rowIndex < _boundRows.Count && colIndex < _boundRows[rowIndex].Length ? _boundRows[rowIndex][colIndex] : null;
        else if (rowIndex < Rows.Count && colIndex < Rows[rowIndex].Cells.Count)
            raw = Rows[rowIndex].Cells[colIndex].Value;

        bool current = raw is true || (raw is string sv && sv.Equals("true", StringComparison.OrdinalIgnoreCase));
        SetCellValue(rowIndex, colIndex, !current);
    }

    public void SetCellValue(int rowIndex, int colIndex, object? value)
    {
        if (_boundRows.Count > 0)
        {
            if (rowIndex < _boundRows.Count && colIndex < _boundRows[rowIndex].Length)
                _boundRows[rowIndex][colIndex] = value;
        }
        else if (rowIndex < Rows.Count && colIndex < Rows[rowIndex].Cells.Count)
        {
            Rows[rowIndex].Cells[colIndex].Value = value;
        }
        CellValueChanged?.Invoke(this, new DataGridViewCellEventArgs(colIndex, rowIndex));
        Invalidate();
    }

    /// <summary>
    /// Refreshes the grid by re-binding the current data source.
    /// Call this when the underlying IList has changed.
    /// </summary>
    public new void Refresh() => RebindDataSource();
}

// ── Supporting event arg types ────────────────────────────────

public class DataGridViewCellEventArgs : EventArgs
{
    public int ColumnIndex { get; }
    public int RowIndex { get; }
    public DataGridViewCellEventArgs(int columnIndex, int rowIndex) { ColumnIndex = columnIndex; RowIndex = rowIndex; }
}

public class DataGridViewCellMouseEventArgs : DataGridViewCellEventArgs
{
    public MouseButtons Button { get; }
    public int X { get; }
    public int Y { get; }
    public int Clicks { get; }
    public DataGridViewCellMouseEventArgs(int columnIndex, int rowIndex, int x, int y, MouseEventArgs e)
        : base(columnIndex, rowIndex) { X = x; Y = y; Button = e.Button; Clicks = e.Clicks; }
}

public class DataGridViewRowEventArgs : EventArgs
{
    public DataGridViewRow Row { get; }
    public DataGridViewRowEventArgs(DataGridViewRow row) => Row = row;
}

public class DataGridViewRowCancelEventArgs : EventArgs
{
    public DataGridViewRow Row { get; }
    public bool Cancel { get; set; }
    public DataGridViewRowCancelEventArgs(DataGridViewRow row) => Row = row;
}

public class DataGridViewColumnEventArgs : EventArgs
{
    public DataGridViewColumn Column { get; }
    public DataGridViewColumnEventArgs(DataGridViewColumn column) => Column = column;
}

public class DataGridViewDataErrorEventArgs : DataGridViewCellEventArgs
{
    public Exception Exception { get; }
    public bool ThrowException { get; set; } = false;
    public DataGridViewDataErrorContext Context { get; }
    public DataGridViewDataErrorEventArgs(Exception exception, int columnIndex, int rowIndex, DataGridViewDataErrorContext context)
        : base(columnIndex, rowIndex) { Exception = exception; Context = context; }
}

public enum DataGridViewDataErrorContext
{
    Formatting, Display, PreferredSize, RowDeletion, Parsing, Commit, Scroll, InitialValueRestoration, LeaveRow
}

public delegate void DataGridViewSortCompareEventHandler(object? sender, DataGridViewSortCompareEventArgs e);

public class DataGridViewSortCompareEventArgs : EventArgs
{
    public int RowIndex1 { get; }
    public int RowIndex2 { get; }
    public object? CellValue1 { get; }
    public object? CellValue2 { get; }
    public int SortResult { get; set; }
    public bool Handled { get; set; }
    public DataGridViewColumn Column { get; }

    public DataGridViewSortCompareEventArgs(DataGridViewColumn column, object? cellValue1, object? cellValue2, int rowIndex1, int rowIndex2)
    {
        Column = column; CellValue1 = cellValue1; CellValue2 = cellValue2; RowIndex1 = rowIndex1; RowIndex2 = rowIndex2;
    }
}

/// <summary>Describes one sort criterion in a multi-column sort.</summary>
public record SortedColumnInfo(int ColumnIndex, ListSortDirection Direction);

/// <summary>
/// Provides data for the <see cref="DataGridView.CellValidating"/> and
/// <see cref="DataGridView.RowValidating"/> events.  Set <see cref="Cancel"/> to
/// <c>true</c> to block the selection change and keep focus on the current cell.
/// </summary>
public class DataGridViewCellCancelEventArgs : CancelEventArgs
{
    public int ColumnIndex { get; }
    public int RowIndex { get; }
    public DataGridViewCellCancelEventArgs(int columnIndex, int rowIndex)
    {
        ColumnIndex = columnIndex;
        RowIndex = rowIndex;
    }
}

/// <summary>
/// Provides data for the <see cref="DataGridView.CellFormatting"/> event.
/// Matches the WinForms <c>DataGridViewCellFormattingEventArgs</c> surface.
/// </summary>
public class DataGridViewCellFormattingEventArgs : DataGridViewCellEventArgs
{
    public DataGridViewCellFormattingEventArgs(
        int columnIndex, int rowIndex,
        object? value, Type? desiredType,
        DataGridViewCellStyle cellStyle)
        : base(columnIndex, rowIndex)
    {
        Value        = value;
        DesiredType  = desiredType;
        CellStyle    = cellStyle ?? new DataGridViewCellStyle();
    }

    /// <summary>Gets or sets the cell value to display. Change this to customise the text shown.</summary>
    public object? Value { get; set; }

    /// <summary>The type that the grid expected for formatting (usually <c>typeof(string)</c>).</summary>
    public Type? DesiredType { get; }

    /// <summary>
    /// The effective cell style. Modify <see cref="DataGridViewCellStyle.ForeColor"/> or
    /// <see cref="DataGridViewCellStyle.Font"/> to override rendering for this cell only.
    /// </summary>
    public DataGridViewCellStyle CellStyle { get; }

    /// <summary>
    /// Set to <c>true</c> in the handler to indicate that the custom formatting was applied
    /// and the grid should use <see cref="Value"/> and the modified <see cref="CellStyle"/>
    /// instead of the default formatting.
    /// </summary>
    public bool FormattingApplied { get; set; }
}

// ── Selection collection types ────────────────────────────────

/// <summary>Read-only collection of selected <see cref="DataGridViewRow"/> objects.</summary>
public class DataGridViewSelectedRowCollection : System.Collections.ObjectModel.Collection<DataGridViewRow>
{
    public DataGridViewSelectedRowCollection() { }
    internal new void Add(DataGridViewRow row) => base.Add(row);
}

/// <summary>Read-only collection of selected <see cref="DataGridViewCell"/> objects.</summary>
public class DataGridViewSelectedCellCollection : System.Collections.ObjectModel.Collection<DataGridViewCell>
{
    public DataGridViewSelectedCellCollection() { }
    internal new void Add(DataGridViewCell cell) => base.Add(cell);
}

/// <summary>Read-only collection of selected <see cref="DataGridViewColumn"/> objects.</summary>
public class DataGridViewSelectedColumnCollection : System.Collections.ObjectModel.Collection<DataGridViewColumn>
{
    public DataGridViewSelectedColumnCollection() { }
    internal new void Add(DataGridViewColumn col) => base.Add(col);
}

// ── Auto-size enums ───────────────────────────────────────────

public enum DataGridViewAutoSizeColumnsMode
{
    None            = 1,
    ColumnHeader    = 2,
    AllCellsExceptHeader = 4,
    AllCells        = 6,
    DisplayedCellsExceptHeader = 8,
    DisplayedCells  = 10,
    Fill            = 16,
}

// DataGridViewAutoSizeColumnMode is defined in DataGridViewColumn.cs

public enum DataGridViewAutoSizeRowsMode
{
    None                  = 0,
    AllHeaders            = 1,
    AllCellsExceptHeaders = 2,
    AllCells              = 3,
    DisplayedHeaders      = 4,
    DisplayedCellsExceptHeaders = 5,
    DisplayedCells        = 6,
    Custom                = 7,
}

// ── Element state flags ───────────────────────────────────────

[Flags]
public enum DataGridViewElementStates
{
    None        = 0x00,
    Displayed   = 0x01,
    Frozen      = 0x02,
    ReadOnly    = 0x04,
    Resizable   = 0x08,
    ResizableSet = 0x10,
    Selected    = 0x20,
    Visible     = 0x40,
}
