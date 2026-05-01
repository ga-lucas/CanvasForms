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
    private bool _autoGenerateColumns = true;
    private readonly List<object?[]> _boundRows = new();
    private SortOrder _sortOrder = SortOrder.None;
    private int _sortColIndex = -1;

    // ── Collections ─────────────────────────────────────────────
    public DataGridViewColumnCollection Columns { get; }
    public DataGridViewRowCollection Rows { get; }

    // ── Events ───────────────────────────────────────────────────
    public event EventHandler<DataGridViewCellEventArgs>? CellClick;
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
    public event EventHandler<DataGridViewDataErrorEventArgs>? DataError;
    public event DataGridViewSortCompareEventHandler? SortCompare;
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

    public void Sort(DataGridViewColumn col, ListSortDirection direction)
    {
        _sortColIndex = col.Index;
        _sortOrder = direction == ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;
        if (_boundRows.Count > 0 && col.Index < (_boundRows.FirstOrDefault()?.Length ?? 0))
        {
            _boundRows.Sort((a, b) =>
            {
                var av = a[col.Index]?.ToString() ?? string.Empty;
                var bv = b[col.Index]?.ToString() ?? string.Empty;
                int cmp = string.Compare(av, bv, StringComparison.OrdinalIgnoreCase);
                return direction == ListSortDirection.Ascending ? cmp : -cmp;
            });
        }
        Invalidate();
    }

    // ── Paint ────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        int bw = BorderStyle == BorderStyle.None ? 0 : 2;
        int x0 = bw, y0 = bw;
        int w = Width - bw * 2, h = Height - bw * 2;

        // Background
        using (var bgBrush = new SolidBrush(BackColor))
            g.FillRectangle(bgBrush, x0, y0, w, h);

        // Border
        if (BorderStyle != BorderStyle.None)
        {
            using var borderPen = new Pen(Color.FromArgb(122, 122, 122));
            g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }

        int rowHdrW = RowHeadersVisible ? RowHeadersWidth : 0;
        int colHdrH = ColumnHeadersVisible ? ColumnHeadersHeight : 0;

        int totalCols = TotalColumnsWidth();
        int totalRows = GetDisplayRowCount();
        int rowH = RowHeightDefault;
        int totalRowsH = totalRows * rowH;

        bool needScrollV = (ScrollBars == DataGridViewScrollBars.Vertical || ScrollBars == DataGridViewScrollBars.Both)
                           && totalRowsH > h - colHdrH;
        bool needScrollH = (ScrollBars == DataGridViewScrollBars.Horizontal || ScrollBars == DataGridViewScrollBars.Both)
                           && totalCols > w - rowHdrW;

        int clientW = w - rowHdrW - (needScrollV ? ScrollBarW : 0);
        int clientH = h - colHdrH - (needScrollH ? ScrollBarW : 0);

        // ── Column headers ──────────────────────────────────────
        if (ColumnHeadersVisible)
        {
            using var hdrBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
            g.FillRectangle(hdrBrush, x0 + rowHdrW, y0, w - rowHdrW - (needScrollV ? ScrollBarW : 0), colHdrH);

            int cx = x0 + rowHdrW - _scrollOffsetX;
            for (int ci = 0; ci < Columns.Count; ci++)
            {
                var col = Columns[ci];
                if (!col.Visible) continue;
                int right = cx + col.Width;
                if (cx >= x0 + rowHdrW + clientW) break;
                if (right > x0 + rowHdrW)
                {
                    // Clip header to visible area
                    using var hdrPen = new Pen(Color.FromArgb(166, 166, 166));
                    g.DrawLine(hdrPen, right - 1, y0, right - 1, y0 + colHdrH);
                    g.DrawLine(hdrPen, x0 + rowHdrW, y0 + colHdrH - 1,
                        x0 + w - (needScrollV ? ScrollBarW : 0), y0 + colHdrH - 1);

                    bool sorted = _sortColIndex == ci;
                    using var textBrush = new SolidBrush(Color.Black);
                    int tx = Math.Max(cx, x0 + rowHdrW) + 3;
                    g.DrawString(col.HeaderText + (sorted ? (_sortOrder == SortOrder.Ascending ? " ▲" : " ▼") : ""),
                        Font, textBrush, tx, y0 + (colHdrH - Font.Height) / 2);
                }
                cx += col.Width;
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
        int ry = y0 + colHdrH - _scrollOffsetY;
        int displayCount = GetDisplayRowCount();
        for (int ri = 0; ri < displayCount; ri++)
        {
            if (ry + rowH < y0 + colHdrH) { ry += rowH; continue; }
            if (ry >= y0 + colHdrH + clientH) break;

            bool rowSelected = ri == _selectedRowIndex;
            bool rowHovered = ri == _hoveredRow;

            Color rowBg = BackColor;
            if (ri % 2 == 1 && AlternatingRowsDefaultCellStyle.BackColor != Color.Empty)
                rowBg = AlternatingRowsDefaultCellStyle.BackColor;
            if (rowHovered) rowBg = Color.FromArgb(229, 241, 251);
            if (rowSelected) rowBg = Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(204, 228, 247);

            // Row background
            using (var rowBrush = new SolidBrush(rowBg))
                g.FillRectangle(rowBrush, x0 + rowHdrW, ry, clientW, rowH);

            // Row header
            if (RowHeadersVisible)
            {
                using var rHdrBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
                g.FillRectangle(rHdrBrush, x0, ry, rowHdrW, rowH);
                if (rowSelected)
                {
                    // Draw selection triangle in row header
                    using var triPen = new Pen(Color.FromArgb(0, 90, 158), 2);
                    int mx = x0 + rowHdrW / 2;
                    int my = ry + rowH / 2;
                    g.DrawLine(triPen, mx - 4, my - 4, mx + 4, my);
                    g.DrawLine(triPen, mx + 4, my, mx - 4, my + 4);
                }
                using var rHdrPen = new Pen(Color.FromArgb(166, 166, 166));
                g.DrawLine(rHdrPen, x0 + rowHdrW - 1, ry, x0 + rowHdrW - 1, ry + rowH);
                g.DrawLine(rHdrPen, x0, ry + rowH - 1, x0 + rowHdrW, ry + rowH - 1);
            }

            // Cells
            int cx = x0 + rowHdrW - _scrollOffsetX;
            for (int ci = 0; ci < Columns.Count; ci++)
            {
                var col = Columns[ci];
                if (!col.Visible) { continue; }
                int right = cx + col.Width;
                if (cx >= x0 + rowHdrW + clientW) break;
                if (right > x0 + rowHdrW)
                {
                    bool cellSelected = rowSelected && SelectionMode == DataGridViewSelectionMode.FullRowSelect
                                        || (_selectedCell == (ri, ci));

                    // Cell background override
                    Color cellBg = rowBg;
                    if (cellSelected && !rowSelected)
                    {
                        cellBg = Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(204, 228, 247);
                        using var cellBrush = new SolidBrush(cellBg);
                        int clipX = Math.Max(cx, x0 + rowHdrW);
                        g.FillRectangle(cellBrush, clipX, ry, right - clipX, rowH);
                    }

                    // Cell text
                    string text = GetCellText(ri, ci);
                    Color textColor = (rowSelected || cellSelected) && Focused ? Color.White : ForeColor;
                    using var textBrush = new SolidBrush(textColor);
                    int tx = Math.Max(cx, x0 + rowHdrW) + 3;
                    int ty = ry + (rowH - Font.Height) / 2;
                    g.DrawString(text, Font, textBrush, tx, ty);

                    // Checkbox column
                    if (col is DataGridViewCheckBoxColumn)
                    {
                        bool chk = false;
                        var raw = _boundRows.Count > 0 && ri < _boundRows.Count && ci < _boundRows[ri].Length
                            ? _boundRows[ri][ci] : null;
                        chk = raw is true || (raw is string s && s.Equals("true", StringComparison.OrdinalIgnoreCase));
                        int cbSize = 13;
                        int cbX = cx + (col.Width - cbSize) / 2;
                        int cbY = ry + (rowH - cbSize) / 2;
                        using var cbPen = new Pen(Color.FromArgb(122, 122, 122));
                        g.DrawRectangle(cbPen, cbX, cbY, cbSize, cbSize);
                        if (chk)
                        {
                            using var checkPen = new Pen(Color.FromArgb(0, 120, 215), 2);
                            g.DrawLine(checkPen, cbX + 2, cbY + cbSize / 2, cbX + 5, cbY + cbSize - 3);
                            g.DrawLine(checkPen, cbX + 5, cbY + cbSize - 3, cbX + cbSize - 2, cbY + 2);
                        }
                    }

                    // Grid lines
                    using var gridPen = new Pen(GridColor);
                    g.DrawLine(gridPen, right - 1, ry, right - 1, ry + rowH);
                }
                cx += col.Width;
            }

            // Row bottom line
            using var rowPen = new Pen(GridColor);
            g.DrawLine(rowPen, x0 + rowHdrW, ry + rowH - 1, x0 + rowHdrW + clientW, ry + rowH - 1);

            ry += rowH;
        }

        // ── Scrollbars (simple rendered indicators) ─────────────
        if (needScrollV)
        {
            int sbX = x0 + w - ScrollBarW;
            int sbH = h - (needScrollH ? ScrollBarW : 0);
            using var sbBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
            g.FillRectangle(sbBrush, sbX, y0, ScrollBarW, sbH);
            using var sbPen = new Pen(Color.FromArgb(166, 166, 166));
            g.DrawRectangle(sbPen, sbX, y0, ScrollBarW - 1, sbH - 1);

            // Thumb
            int maxScroll = Math.Max(1, totalRowsH - clientH);
            int thumbH = Math.Max(20, (int)((double)clientH / Math.Max(1, totalRowsH) * sbH));
            int thumbY = y0 + (int)((double)_scrollOffsetY / maxScroll * (sbH - thumbH));
            using var thumbBrush = new SolidBrush(Color.FromArgb(180, 180, 180));
            g.FillRectangle(thumbBrush, sbX + 2, thumbY + 2, ScrollBarW - 4, thumbH - 4);
        }
    }

    private int TotalColumnsWidth()
    {
        int total = 0;
        foreach (var col in Columns) if (col.Visible) total += col.Width;
        return total;
    }

    // ── Input ────────────────────────────────────────────────────

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
            int ci = GetColAtX(mx - rowHdrW + _scrollOffsetX + bw);
            if (ci >= 0)
            {
                ColumnHeaderMouseClick?.Invoke(this, new DataGridViewColumnEventArgs(Columns[ci]));
                if (Columns[ci].SortMode != DataGridViewColumnSortMode.NotSortable)
                {
                    var dir = (_sortColIndex == ci && _sortOrder == SortOrder.Ascending)
                        ? ListSortDirection.Descending : ListSortDirection.Ascending;
                    Sort(Columns[ci], dir);
                }
            }
            return;
        }

        // Row click
        int ri = GetRowAtY(my - colHdrH + _scrollOffsetY + bw);
        int col = GetColAtX(mx - rowHdrW + _scrollOffsetX + bw);
        if (ri >= 0 && ri < GetDisplayRowCount())
        {
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
        int ri = GetRowAtY(my - colHdrH + _scrollOffsetY + bw);
        if (ri != _hoveredRow) { _hoveredRow = ri; Invalidate(); }
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
        int clientH = Height - bw * 2 - colHdrH - ScrollBarW;
        int totalH = GetDisplayRowCount() * rowH;
        int maxScroll = Math.Max(0, totalH - clientH);
        _scrollOffsetY = Math.Clamp(_scrollOffsetY - Math.Sign(e.Delta) * rowH * 3, 0, maxScroll);
        Invalidate();
        base.OnMouseWheel(e);
    }

    private int GetRowAtY(int relY) => relY < 0 ? -1 : relY / RowHeightDefault;

    private int GetColAtX(int relX)
    {
        if (relX < 0) return -1;
        int cx = 0;
        for (int ci = 0; ci < Columns.Count; ci++)
        {
            var col = Columns[ci];
            if (!col.Visible) continue;
            if (relX < cx + col.Width) return ci;
            cx += col.Width;
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
    public void Refresh() => RebindDataSource();
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
