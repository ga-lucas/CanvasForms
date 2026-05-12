using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms;

/// <summary>
/// WinForms-compatible DataGridView rendered to an HTML canvas.
/// Supports in-process DataSource binding (IList, BindingSource, DataTable),
/// auto-column generation, virtual/scrolled rendering, and row/cell selection.
/// </summary>
public class DataGridView : ScrollableControl
{
    // ── Layout constants ────────────────────────────────────────
    private const int HeaderHeight = 26;
    private const int RowHeightDefault = 23;
    private const int RowHeaderWidth = 40;
    private const int ScrollBarW = 17;

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
    public event EventHandler? SelectionChanged;
    public event EventHandler<DataGridViewCellCancelEventArgs>? CellValidating;
    public event EventHandler<DataGridViewCellCancelEventArgs>? RowValidating;
    public event EventHandler<DataGridViewDataErrorEventArgs>? DataError;
    public event DataGridViewSortCompareEventHandler? SortCompare;
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
        using var textBrush = new SolidBrush(textColor);
        g.DrawString(text, Font, textBrush, Math.Max(cx, clipLeft) + 3, ry + (rowH - Font.Height) / 2);

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

        // Column header click
        if (ColumnHeadersVisible && my < colHdrH)
        {
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
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        int bw = BorderStyle == BorderStyle.None ? 0 : 2;
        int colHdrH = ColumnHeadersVisible ? ColumnHeadersHeight : 0;
        int my = e.Y - bw;
        if (my < colHdrH) { if (_hoveredRow != -1) { _hoveredRow = -1; Invalidate(); } return; }
        int ri = GetRowAtY(my - colHdrH + bw);
        if (ri != _hoveredRow)
        base.OnMouseMove(e);
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
