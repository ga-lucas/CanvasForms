using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents an item in a ListView control
/// </summary>
public class ListViewItem
{
    public ListViewItem() { }
    public ListViewItem(string text) { Text = text; }
    public ListViewItem(string[] items)
    {
        if (items.Length > 0) Text = items[0];
        for (int i = 1; i < items.Length; i++)
            SubItems.Add(new ListViewSubItem(this, items[i]));
    }

    public string Text { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public object? Tag { get; set; }
    public bool Checked { get; set; } = false;
    public bool Selected { get; set; } = false;
    public int ImageIndex { get; set; } = -1;
    public Color ForeColor { get; set; } = Color.Transparent;
    public Color BackColor { get; set; } = Color.Transparent;

    public ListViewSubItemCollection SubItems { get; } = new();

    public class ListViewSubItemCollection : List<ListViewSubItem>
    {
        public ListViewSubItem Add(string text)
        {
            var item = new ListViewSubItem(null, text);
            Add(item);
            return item;
        }
    }
}

public class ListViewSubItem
{
    public ListViewSubItem(ListViewItem? owner, string text) { Owner = owner; Text = text; }
    public string Text { get; set; } = string.Empty;
    public ListViewItem? Owner { get; }
}

/// <summary>
/// Represents a column header in a ListView
/// </summary>
public class ColumnHeader
{
    public string Text { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; } = 60;
    public HorizontalAlignment TextAlign { get; set; } = HorizontalAlignment.Left;
    public int Index { get; internal set; } = -1;
    public ColumnHeader() { }
    public ColumnHeader(string text) { Text = text; }
}

/// <summary>
/// Collection of ListViewItem objects
/// </summary>
public class ListViewItemCollection : IEnumerable<ListViewItem>
{
    private readonly List<ListViewItem> _list = new();
    private readonly ListView _owner;
    internal ListViewItemCollection(ListView owner) => _owner = owner;

    public int Count => _list.Count;
    public ListViewItem this[int index] => _list[index];

    public ListViewItem Add(string text) { var i = new ListViewItem(text); Add(i); return i; }
    public void Add(ListViewItem item) { _list.Add(item); _owner.Invalidate(); }
    public void Remove(ListViewItem item) { _list.Remove(item); _owner.Invalidate(); }
    public void RemoveAt(int index) { _list.RemoveAt(index); _owner.Invalidate(); }
    public void Clear() { _list.Clear(); _owner.Invalidate(); }
    public bool Contains(ListViewItem item) => _list.Contains(item);
    public IEnumerator<ListViewItem> GetEnumerator() => _list.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Collection of ColumnHeader objects
/// </summary>
public class ColumnHeaderCollection : IEnumerable<ColumnHeader>
{
    private readonly List<ColumnHeader> _list = new();
    private readonly ListView _owner;
    internal ColumnHeaderCollection(ListView owner) => _owner = owner;

    public int Count => _list.Count;
    public ColumnHeader this[int index] => _list[index];

    public ColumnHeader Add(string text, int width = 60)
    {
        var h = new ColumnHeader { Text = text, Width = width, Index = _list.Count };
        Add(h); return h;
    }
    public void Add(ColumnHeader col) { col.Index = _list.Count; _list.Add(col); _owner.Invalidate(); }
    public void Remove(ColumnHeader col) { _list.Remove(col); _owner.Invalidate(); }
    public void Clear() { _list.Clear(); _owner.Invalidate(); }
    public IEnumerator<ColumnHeader> GetEnumerator() => _list.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Represents a Windows Forms ListView control
/// </summary>
public class ListView : Control
{
    private const int HeaderHeight = 22;
    private const int ItemHeight = 20;
    private const int CheckBoxSize = 13;
    private const int ItemPadding = 3;

    private View _view = View.Details;
    private int _scrollOffset = 0;
    private int _hoveredIndex = -1;
    private bool _fullRowSelect = false;
    private bool _gridLines = false;
    private bool _checkBoxes = false;
    private SortOrder _sorting = SortOrder.None;
    private int _selectedResizeCol = -1;

    public ListViewItemCollection Items { get; }
    public ColumnHeaderCollection Columns { get; }

    public ListView()
    {
        Width = 240; Height = 150;
        BackColor = Color.White; ForeColor = Color.Black;
        TabStop = true;
        SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint, true);
        Items = new ListViewItemCollection(this);
        Columns = new ColumnHeaderCollection(this);
    }

    public View View { get => _view; set { _view = value; Invalidate(); } }
    public bool FullRowSelect { get => _fullRowSelect; set { _fullRowSelect = value; Invalidate(); } }
    public bool GridLines { get => _gridLines; set { _gridLines = value; Invalidate(); } }
    public bool CheckBoxes { get => _checkBoxes; set { _checkBoxes = value; Invalidate(); } }
    public SortOrder Sorting { get => _sorting; set { _sorting = value; Invalidate(); } }
    public BorderStyle BorderStyle { get; set; } = BorderStyle.Fixed3D;
    public bool MultiSelect { get; set; } = true;
    public bool HideSelection { get; set; } = true;
    public bool LabelWrap { get; set; } = true;

    public IEnumerable<ListViewItem> SelectedItems =>
        Items.Where(i => i.Selected);

    public SelectedIndexCollection SelectedIndices => new SelectedIndexCollection(this);

    public IEnumerable<ListViewItem> CheckedItems =>
        Items.Where(i => i.Checked);

    public event EventHandler? ItemChecked;
    public event EventHandler? SelectedIndexChanged;
    public event ColumnClickEventHandler? ColumnClick;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        if (BorderStyle != BorderStyle.None)
        {
            using var borderPen = new Pen(Color.FromArgb(122, 122, 122));
            g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
        }

        if (_view == View.Details)
            PaintDetails(g);
        else if (_view == View.List)
            PaintList(g);
        else
            PaintLargeIcons(g);

        base.OnPaint(e);
    }

    private void PaintDetails(Graphics g)
    {
        int colX = 0;

        // Column headers
        using var headerBg = new SolidBrush(Color.FromArgb(240, 240, 240));
        g.FillRectangle(headerBg, 0, 0, Width, HeaderHeight);
        using var headerBorderPen = new Pen(Color.FromArgb(200, 200, 200));
        g.DrawLine(headerBorderPen, 0, HeaderHeight - 1, Width, HeaderHeight - 1);

        foreach (var col in Columns)
        {
            using var colBorderPen = new Pen(Color.FromArgb(210, 210, 210));
            g.DrawLine(colBorderPen, colX + col.Width - 1, 0, colX + col.Width - 1, HeaderHeight);
            using var headerTextBrush = new SolidBrush(Color.Black);
            g.DrawString(col.Text, "Arial", 12, headerTextBrush, colX + 3, 4);
            colX += col.Width;
        }

        // Items
        var sorted = GetSortedItems();
        int y = HeaderHeight - _scrollOffset;
        foreach (var item in sorted)
        {
            if (y + ItemHeight < HeaderHeight) { y += ItemHeight; continue; }
            if (y > Height) break;

            bool isSelected = item.Selected;
            bool isHovered = sorted.IndexOf(item) == _hoveredIndex;

            if (isSelected)
            {
                var selBg = Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(204, 228, 247);
                int selX = _fullRowSelect ? 0 : 0;
                int selW = _fullRowSelect ? Width : (Columns.Count > 0 ? Columns[0].Width : Width);
                using var selBrush = new SolidBrush(selBg);
                if (_fullRowSelect) g.FillRectangle(selBrush, 0, y, Width, ItemHeight);
            }
            else if (isHovered)
            {
                using var hoverBrush = new SolidBrush(Color.FromArgb(229, 241, 251));
                g.FillRectangle(hoverBrush, 0, y, Width, ItemHeight);
            }

            int ix = 2;

            // Checkbox
            if (_checkBoxes)
            {
                using var cbPen = new Pen(Color.FromArgb(122, 122, 122));
                g.DrawRectangle(cbPen, ix, y + (ItemHeight - CheckBoxSize) / 2, CheckBoxSize, CheckBoxSize);
                if (item.Checked)
                {
                    using var checkPen = new Pen(Color.FromArgb(0, 120, 215), 2);
                    g.DrawLine(checkPen, ix + 2, y + ItemHeight / 2, ix + 5, y + ItemHeight / 2 + 3);
                    g.DrawLine(checkPen, ix + 5, y + ItemHeight / 2 + 3, ix + 11, y + (ItemHeight - CheckBoxSize) / 2 + 2);
                }
                ix += CheckBoxSize + 3;
            }

            // Cells
            int cx = ix;
            var allCols = Columns.ToList();
            for (int ci = 0; ci < allCols.Count || ci == 0; ci++)
            {
                int colW = ci < allCols.Count ? allCols[ci].Width : (Width - cx);
                string cellText = ci == 0 ? item.Text :
                    (ci - 1 < item.SubItems.Count ? item.SubItems[ci - 1].Text : string.Empty);

                var textColor = isSelected && Focused ? Color.White : ForeColor;
                using var textBrush = new SolidBrush(textColor);
                g.DrawString(cellText, "Arial", 12, textBrush, cx + 2, y + 3);

                if (_gridLines && ci < allCols.Count)
                {
                    using var gridPen = new Pen(Color.FromArgb(220, 220, 220));
                    g.DrawLine(gridPen, cx + colW - 1, y, cx + colW - 1, y + ItemHeight);
                }
                cx += ci < allCols.Count ? colW : colW;
                if (ci >= allCols.Count) break;
            }

            if (_gridLines)
            {
                using var gridPen = new Pen(Color.FromArgb(220, 220, 220));
                g.DrawLine(gridPen, 0, y + ItemHeight - 1, Width, y + ItemHeight - 1);
            }

            y += ItemHeight;
        }
    }

    private void PaintList(Graphics g)
    {
        var sorted = GetSortedItems();
        int y = 0; int col = 0;
        int colW = 120;
        foreach (var item in sorted)
        {
            int x = col * colW;
            bool isSelected = item.Selected;
            if (isSelected)
            {
                using var selBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                g.FillRectangle(selBrush, x, y, colW - 2, ItemHeight);
            }
            using var textBrush = new SolidBrush(isSelected && Focused ? Color.White : ForeColor);
            g.DrawString(item.Text, "Arial", 12, textBrush, x + 2, y + 3);
            y += ItemHeight;
            if (y + ItemHeight > Height) { y = 0; col++; }
        }
    }

    private void PaintLargeIcons(Graphics g)
    {
        const int iconSize = 32; const int cellW = 70; const int cellH = 60;
        var sorted = GetSortedItems();
        int x = 4; int y = 4;
        foreach (var item in sorted)
        {
            bool isSelected = item.Selected;
            if (isSelected)
            {
                using var selBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                g.FillRectangle(selBrush, x, y, cellW - 2, cellH - 2);
            }
            // Icon placeholder
            using var iconBrush = new SolidBrush(Color.FromArgb(200, 200, 200));
            g.FillRectangle(iconBrush, x + (cellW - iconSize) / 2, y + 2, iconSize, iconSize);
            using var textBrush = new SolidBrush(isSelected && Focused ? Color.White : ForeColor);
            var tw = item.Text.Length * 6;
            g.DrawString(item.Text, "Arial", 10, textBrush, x + (cellW - tw) / 2, y + iconSize + 4);
            x += cellW;
            if (x + cellW > Width) { x = 4; y += cellH; }
        }
    }

    private List<ListViewItem> GetSortedItems()
    {
        var list = Items.ToList();
        if (_sorting == SortOrder.Ascending) list.Sort((a, b) => string.Compare(a.Text, b.Text, StringComparison.OrdinalIgnoreCase));
        else if (_sorting == SortOrder.Descending) list.Sort((a, b) => string.Compare(b.Text, a.Text, StringComparison.OrdinalIgnoreCase));
        return list;
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        if (_view == View.Details && e.Y < HeaderHeight)
        {
            int colIdx = GetColumnAtX(e.X);
            if (colIdx >= 0) ColumnClick?.Invoke(this, new ColumnClickEventArgs(colIdx));
            return;
        }
        var idx = GetItemAtY(e.Y);
        if (idx >= 0)
        {
            var sorted = GetSortedItems();
            var item = sorted[idx];
            if (e.Button == MouseButtons.Left)
            {
                bool ctrl = false; // No modifier key tracking yet
                if (!MultiSelect) foreach (var i in Items) i.Selected = false;
                item.Selected = !item.Selected;
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();

                // Checkbox toggle
                if (_checkBoxes && e.X < 2 + CheckBoxSize + 3)
                {
                    item.Checked = !item.Checked;
                    ItemChecked?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        int idx = GetItemAtY(e.Y);
        if (idx != _hoveredIndex) { _hoveredIndex = idx; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_hoveredIndex != -1) { _hoveredIndex = -1; Invalidate(); }
        base.OnMouseLeave(e);
    }

    private int GetItemAtY(int y)
    {
        int startY = _view == View.Details ? HeaderHeight : 0;
        int idx = (y - startY + _scrollOffset) / ItemHeight;
        var sorted = GetSortedItems();
        return idx >= 0 && idx < sorted.Count ? idx : -1;
    }

    private int GetColumnAtX(int x)
    {
        int cx = 0;
        int ci = 0;
        foreach (var col in Columns)
        {
            if (x >= cx && x < cx + col.Width) return ci;
            cx += col.Width; ci++;
        }
        return -1;
    }
}

public enum View { LargeIcon, Details, SmallIcon, List, Tile }
public enum SortOrder { None, Ascending, Descending }

public delegate void ColumnClickEventHandler(object? sender, ColumnClickEventArgs e);
public class ColumnClickEventArgs : EventArgs
{
    public int Column { get; }
    public ColumnClickEventArgs(int column) => Column = column;
}

/// <summary>
/// Collection of selected item indices in a ListView
/// </summary>
public class SelectedIndexCollection : IList<int>, IReadOnlyList<int>
{
    private readonly ListView _owner;

    public SelectedIndexCollection(ListView owner) => _owner = owner;

    public int this[int index]
    {
        get
        {
            var selectedIndices = GetIndices();
            if (index < 0 || index >= selectedIndices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return selectedIndices[index];
        }
        set => throw new NotSupportedException("SelectedIndexCollection is read-only.");
    }

    public int Count => GetIndices().Count;
    public bool IsReadOnly => true;

    private List<int> GetIndices()
    {
        var indices = new List<int>();
        for (int i = 0; i < _owner.Items.Count; i++)
        {
            if (_owner.Items[i].Selected)
                indices.Add(i);
        }
        return indices;
    }

    public bool Contains(int index) =>
        index >= 0 && index < _owner.Items.Count && _owner.Items[index].Selected;

    public int IndexOf(int index) => GetIndices().IndexOf(index);

    public void CopyTo(int[] array, int arrayIndex) => GetIndices().CopyTo(array, arrayIndex);

    public IEnumerator<int> GetEnumerator() => GetIndices().GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    // Unsupported mutation methods (collection is read-only)
    public void Add(int item) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();
    public void Insert(int index, int item) => throw new NotSupportedException();
    public bool Remove(int item) => throw new NotSupportedException();
    public void RemoveAt(int index) => throw new NotSupportedException();
}
