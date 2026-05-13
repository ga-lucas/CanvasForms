using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System.Windows.Forms;

// ── Supporting types ──────────────────────────────────────────────────────────

/// <summary>Controls how properties are ordered in a <see cref="PropertyGrid"/>.</summary>
public enum PropertySort
{
    NoSort         = 0,
    Alphabetical   = 1,
    Categorized    = 2,
    CategorizedAlphabetical = 3,
}

/// <summary>Event arguments for <see cref="PropertyGrid.PropertyValueChanged"/>.</summary>
public class PropertyValueChangedEventArgs : EventArgs
{
    public GridItem ChangedItem { get; }
    public object?  OldValue    { get; }
    public PropertyValueChangedEventArgs(GridItem item, object? oldValue)
    { ChangedItem = item; OldValue = oldValue; }
}

public delegate void PropertyValueChangedEventHandler(object sender, PropertyValueChangedEventArgs e);

/// <summary>
/// Represents a single row (property entry) in a <see cref="PropertyGrid"/>.
/// Matches the WinForms <c>GridItem</c> API.
/// </summary>
public class GridItem
{
    public string          Label        { get; internal set; } = string.Empty;
    public object?         Value        { get; internal set; }
    public PropertyInfo?   PropertyInfo { get; internal set; }
    public bool            IsCategory   { get; internal set; }
    public string          Category     { get; internal set; } = string.Empty;
    public string          Description  { get; internal set; } = string.Empty;
    public GridItemType    GridItemType => IsCategory ? GridItemType.Category : GridItemType.Property;
    public bool            Expandable   { get; internal set; }
    public bool            Expanded     { get; internal set; }
    public bool            IsReadOnly   { get; internal set; }
    public bool            IsNonDefault { get; internal set; }
    public int             Depth        { get; internal set; }   // 0 = top-level, 1 = sub-property
    public List<GridItem>  Children     { get; }              = new();
    public GridItem?       Parent       { get; internal set; }

    internal object? OwnerObject { get; set; }
}

/// <summary>Matches WinForms <c>GridItemType</c> enum.</summary>
public enum GridItemType { Property, Category, ArrayValue, Root }

// ── PropertyGrid ──────────────────────────────────────────────────────────────

/// <summary>
/// A canvas-rendered property browser that uses reflection to enumerate and
/// edit public properties of <see cref="SelectedObject"/>.
///
/// Matches the WinForms <c>System.Windows.Forms.PropertyGrid</c> API surface
/// used by designer-generated apps: <see cref="SelectedObject"/>,
/// <see cref="PropertySort"/>, <see cref="HelpVisible"/>,
/// <see cref="ToolbarVisible"/>, <see cref="SelectedGridItem"/>,
/// <see cref="SelectedGridItemChanged"/>, <see cref="PropertyValueChanged"/>.
/// </summary>
public class PropertyGrid : Control
{
    // ── Layout constants ──────────────────────────────────────────────────────
    private const int RowHeight       = 20;
    private const int HeaderHeight    = 24;   // toolbar area
    private const int DescPanelHeight = 56;   // description panel at bottom
    private const int SplitterX_Def  = 120;  // default name/value column split
    private const int IndentWidth     = 12;   // indent per expand level
    private const int ExpandBoxSize   = 9;

    // ── Colors ────────────────────────────────────────────────────────────────
    private static readonly Drawing.Color ColCategoryBg  = Drawing.Color.FromArgb(236, 236, 240);
    private static readonly Drawing.Color ColCategoryFg  = Drawing.Color.FromArgb(40,  40,  40);
    private static readonly Drawing.Color ColSelBg       = Drawing.Color.FromArgb(0,   120, 215);
    private static readonly Drawing.Color ColSelFg       = Drawing.Color.White;
    private static readonly Drawing.Color ColBorder      = Drawing.Color.FromArgb(200, 200, 200);
    private static readonly Drawing.Color ColDescBg      = Drawing.Color.FromArgb(248, 248, 248);
    private static readonly Drawing.Color ColValueFg     = Drawing.Color.FromArgb(30,  100, 180);

    // ── State ─────────────────────────────────────────────────────────────────
    private object?           _selectedObject;
    private object[]          _selectedObjects  = Array.Empty<object>();
    private PropertySort      _propertySort     = PropertySort.CategorizedAlphabetical;
    private bool              _helpVisible      = true;
    private bool              _toolbarVisible   = true;
    private int               _splitterX;
    private int               _scrollOffset;
    private GridItem?         _selectedItem;
    private bool              _editing;
    private string            _editBuffer       = string.Empty;
    private int               _editCaret;
    private bool              _splitterDragging;
    private int               _splitterDragStart;

    private List<GridItem>    _flatRows         = new();
    private List<GridItem>    _roots            = new();

    // ── Constructor ───────────────────────────────────────────────────────────

    public PropertyGrid()
    {
        _splitterX = SplitterX_Def;
        BackColor  = Drawing.Color.White;
        TabStop    = true;
        Width      = 240;
        Height     = 300;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the object whose properties are displayed.</summary>
    public object? SelectedObject
    {
        get => _selectedObject;
        set
        {
            _selectedObject  = value;
            _selectedObjects = value != null ? new[] { value } : Array.Empty<object>();
            _selectedItem    = null;
            _scrollOffset    = 0;
            RebuildRows();
            Invalidate();
        }
    }

    /// <summary>Gets or sets multiple selected objects (shows intersection of properties).</summary>
    public object[] SelectedObjects
    {
        get => _selectedObjects;
        set
        {
            _selectedObjects = value ?? Array.Empty<object>();
            _selectedObject  = _selectedObjects.Length == 1 ? _selectedObjects[0] : null;
            _selectedItem    = null;
            _scrollOffset    = 0;
            RebuildRows();
            Invalidate();
        }
    }

    public PropertySort PropertySort
    {
        get => _propertySort;
        set { _propertySort = value; RebuildRows(); Invalidate(); }
    }

    public bool HelpVisible
    {
        get => _helpVisible;
        set { _helpVisible = value; Invalidate(); }
    }

    public bool ToolbarVisible
    {
        get => _toolbarVisible;
        set { _toolbarVisible = value; Invalidate(); }
    }

    /// <summary>Gets or sets the currently selected grid row.</summary>
    public GridItem? SelectedGridItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem == value) return;
            var old = _selectedItem;
            _selectedItem = value;
            SelectedGridItemChanged?.Invoke(this, new SelectedGridItemChangedEventArgs(old, value));
            Invalidate();
        }
    }

    // ── Events ────────────────────────────────────────────────────────────────

    public event SelectedGridItemChangedEventHandler? SelectedGridItemChanged;
    public event PropertyValueChangedEventHandler?    PropertyValueChanged;

    // ── Public methods ────────────────────────────────────────────────────────

    /// <summary>Forces a refresh of the displayed properties from the current object.</summary>
    public void Refresh()
    {
        RebuildRows();
        Invalidate();
    }

    /// <summary>Collapses all category and expandable property rows.</summary>
    public void CollapseAllGridItems()
    {
        SetExpandedRecursive(_roots, false);
        RebuildFlat();
        Invalidate();
    }

    /// <summary>Expands all category and expandable property rows.</summary>
    public void ExpandAllGridItems()
    {
        SetExpandedRecursive(_roots, true);
        RebuildFlat();
        Invalidate();
    }

    private static void SetExpandedRecursive(IEnumerable<GridItem> items, bool expanded)
    {
        foreach (var item in items)
        {
            if (item.IsCategory || item.Expandable)
                item.Expanded = expanded;
            if (item.Children.Count > 0)
                SetExpandedRecursive(item.Children, expanded);
        }
    }

    // ── Row building ──────────────────────────────────────────────────────────

    private void RebuildRows()
    {
        _roots    = new List<GridItem>();
        _flatRows = new List<GridItem>();
        if (_selectedObject == null && _selectedObjects.Length == 0) return;

        var target = _selectedObject ?? _selectedObjects.FirstOrDefault();
        if (target == null) return;

        var props = target.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToList();

        var isCategorized = _propertySort == PropertySort.Categorized
                         || _propertySort == PropertySort.CategorizedAlphabetical;
        var isAlpha       = _propertySort == PropertySort.Alphabetical
                         || _propertySort == PropertySort.CategorizedAlphabetical;

        if (isCategorized)
        {
            var groups = props
                .GroupBy(p => GetCategory(p))
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                var catItem = new GridItem
                {
                    Label      = group.Key,
                    IsCategory = true,
                    Expanded   = true,
                    OwnerObject = target,
                };

                var children = isAlpha
                    ? group.OrderBy(p => p.Name).ToList()
                    : group.ToList();

                foreach (var prop in children)
                {
                    var child = MakePropertyItem(prop, target);
                    child.Parent   = catItem;
                    child.Category = group.Key;
                    catItem.Children.Add(child);
                }

                _roots.Add(catItem);
            }
        }
        else
        {
            var ordered = isAlpha ? props.OrderBy(p => p.Name) : (IEnumerable<PropertyInfo>)props;
            foreach (var prop in ordered)
            {
                var item = MakePropertyItem(prop, target);
                _roots.Add(item);
            }
        }

        RebuildFlat();
    }

    private static bool IsExpandableComplexType(Type t)
    {
        if (t == typeof(string) || t.IsPrimitive || t.IsEnum) return false;
        if (t == typeof(decimal) || t == typeof(DateTime) || t == typeof(TimeSpan)) return false;
        if (t == typeof(object)) return false;
        if (Nullable.GetUnderlyingType(t) != null) return false;
        // Only expand if the type has public instance properties we can read
        return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Any(p => p.CanRead && p.GetIndexParameters().Length == 0);
    }

    private static GridItem MakePropertyItem(PropertyInfo prop, object owner, int depth = 0)
    {
        object? val = null;
        try { val = prop.GetValue(owner); } catch { }

        var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
        var catAttr  = prop.GetCustomAttribute<CategoryAttribute>();

        // Read-only: no public setter or decorated with ReadOnlyAttribute(true)
        bool isReadOnly = prop.SetMethod == null || prop.SetMethod.IsPrivate
            || (prop.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly == true);

        // Non-default: value differs from DefaultValueAttribute or from default(T)
        bool isNonDefault = false;
        var defAttr = prop.GetCustomAttribute<DefaultValueAttribute>();
        if (defAttr != null)
        {
            isNonDefault = !Equals(val, defAttr.Value);
        }
        else if (val != null && prop.PropertyType.IsValueType)
        {
            try { isNonDefault = !val.Equals(Activator.CreateInstance(prop.PropertyType)); } catch { }
        }

        bool expandable = val != null && depth < 2 && IsExpandableComplexType(prop.PropertyType);

        var item = new GridItem
        {
            Label        = prop.Name,
            Value        = val,
            PropertyInfo = prop,
            IsCategory   = false,
            Description  = descAttr?.Description ?? string.Empty,
            Category     = catAttr?.Category ?? "Misc",
            Expandable   = expandable,
            OwnerObject  = owner,
            IsReadOnly   = isReadOnly,
            IsNonDefault = isNonDefault,
            Depth        = depth,
        };

        if (expandable && val != null)
        {
            var subProps = val.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .OrderBy(p => p.Name);
            foreach (var sub in subProps)
            {
                var child = MakePropertyItem(sub, val, depth + 1);
                child.Parent = item;
                item.Children.Add(child);
            }
        }

        return item;
    }

    private void RebuildFlat()
    {
        _flatRows = new List<GridItem>();
        foreach (var root in _roots)
            AddFlatItem(_flatRows, root);
    }

    private static void AddFlatItem(List<GridItem> list, GridItem item)
    {
        list.Add(item);
        if (!item.Expanded) return;
        foreach (var child in item.Children)
            AddFlatItem(list, child);
    }

    private static string GetCategory(PropertyInfo p)
    {
        var attr = p.GetCustomAttribute<CategoryAttribute>();
        return attr?.Category ?? "Misc";
    }

    // ── Geometry helpers ──────────────────────────────────────────────────────

    private int GridTop    => _toolbarVisible ? HeaderHeight : 0;
    private int DescBottom => _helpVisible    ? DescPanelHeight : 0;
    private int GridHeight => Height - GridTop - DescBottom;
    private int VisibleRows => Math.Max(1, GridHeight / RowHeight);

    private Rectangle GridBounds => new(0, GridTop, Width, GridHeight);

    private int RowY(int flatIndex) => GridTop + flatIndex * RowHeight - _scrollOffset;

    // ── Paint ─────────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);

        // Background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, bounds);

        // Toolbar strip
        if (_toolbarVisible)
        {
            using var tbBrush = new SolidBrush(Drawing.Color.FromArgb(245, 245, 245));
            g.FillRectangle(tbBrush, 0, 0, Width, HeaderHeight);
            using var tbPen = new Pen(ColBorder);
            g.DrawLine(tbPen, 0, HeaderHeight - 1, Width, HeaderHeight - 1);

            // Sort buttons (decorative — clicking handled below)
            DrawToolbarButton(g, 4,  4, "⊞", _propertySort == PropertySort.Categorized || _propertySort == PropertySort.CategorizedAlphabetical);
            DrawToolbarButton(g, 28, 4, "AZ", _propertySort == PropertySort.Alphabetical);
        }

        // Description panel
        if (_helpVisible)
        {
            int dy = Height - DescBottom;
            using var descBrush = new SolidBrush(ColDescBg);
            g.FillRectangle(descBrush, 0, dy, Width, DescBottom);
            using var descPen = new Pen(ColBorder);
            g.DrawLine(descPen, 0, dy, Width, dy);

            if (_selectedItem != null && !_selectedItem.IsCategory)
            {
                using var boldBrush = new SolidBrush(ForeColor);
                g.DrawString(_selectedItem.Label, "Segoe UI", 11, boldBrush, 4, dy + 4);
                if (!string.IsNullOrEmpty(_selectedItem.Description))
                {
                    using var descTextBrush = new SolidBrush(Drawing.Color.FromArgb(80, 80, 80));
                    g.DrawString(_selectedItem.Description, "Segoe UI", 10, descTextBrush, 4, dy + 18);
                }
            }
        }

        // Grid rows
        var clip = GridBounds;
        for (int i = 0; i < _flatRows.Count; i++)
        {
            int rowY = RowY(i);
            if (rowY + RowHeight < clip.Top) continue;
            if (rowY > clip.Bottom) break;

            var item    = _flatRows[i];
            bool isSel  = item == _selectedItem;
            // Indent: categories at 2, properties get IndentWidth per depth level + expand-box space
            int  indent = item.IsCategory ? 2 : IndentWidth * (1 + item.Depth) + 2;

            // Row background
            Drawing.Color bg;
            if (isSel)             bg = ColSelBg;
            else if (item.IsCategory) bg = ColCategoryBg;
            else                   bg = BackColor;

            using var rowBrush = new SolidBrush(bg);
            g.FillRectangle(rowBrush, 0, rowY, Width, RowHeight);

            // Expand/collapse box for categories and expandable properties
            if (item.IsCategory || item.Expandable)
            {
                int bx = item.IsCategory ? 2 : IndentWidth * item.Depth + 2;
                int by = rowY + (RowHeight - ExpandBoxSize) / 2;
                using var boxPen = new Pen(Drawing.Color.FromArgb(120, 120, 120));
                g.DrawRectangle(boxPen, bx, by, ExpandBoxSize, ExpandBoxSize);
                using var signBrush = new SolidBrush(Drawing.Color.FromArgb(80, 80, 80));
                g.DrawString(item.Expanded ? "−" : "+", "Segoe UI", 9, signBrush, bx + 1, by);
            }

            // Name column — bold if non-default, grey if read-only
            Drawing.Color nameFgColor;
            if (isSel)              nameFgColor = ColSelFg;
            else if (item.IsCategory) nameFgColor = ColCategoryFg;
            else if (item.IsReadOnly) nameFgColor = Drawing.Color.FromArgb(128, 128, 128);
            else                    nameFgColor = ForeColor;
            int nameFontSize = 11;
            string nameFontStyle = (!item.IsCategory && item.IsNonDefault) ? "Segoe UI Bold" : "Segoe UI";
            using var nameBrush = new SolidBrush(nameFgColor);
            g.DrawString(item.Label, nameFontStyle, nameFontSize, nameBrush, indent, rowY + 3);

            // Splitter line
            using var splPen = new Pen(ColBorder);
            g.DrawLine(splPen, _splitterX, rowY, _splitterX, rowY + RowHeight);

            // Value column (skip for category headers)
            if (!item.IsCategory)
            {
                int vx = _splitterX + 3;
                int vw = Width - vx - 2;

                if (_editing && isSel)
                {
                    // Inline editor
                    using var edBrush = new SolidBrush(Drawing.Color.White);
                    g.FillRectangle(edBrush, _splitterX + 1, rowY + 1, Width - _splitterX - 2, RowHeight - 2);
                    using var edPen = new Pen(ColSelBg);
                    g.DrawRectangle(edPen, _splitterX + 1, rowY + 1, Width - _splitterX - 2, RowHeight - 2);
                    using var edTextBrush = new SolidBrush(ForeColor);
                    g.DrawString(_editBuffer, "Segoe UI", 11, edTextBrush, vx, rowY + 3);
                    // Caret
                    int caretX = vx + Math.Min(_editCaret, _editBuffer.Length) * 7;
                    using var caretPen = new Pen(ForeColor);
                    g.DrawLine(caretPen, caretX, rowY + 3, caretX, rowY + RowHeight - 3);
                }
                else
                {
                    var valText  = FormatValue(item.Value);
                    var valColor = isSel ? ColSelFg : ColValueFg;
                    using var valBrush = new SolidBrush(valColor);
                    g.DrawString(valText, "Segoe UI", 11, valBrush, vx, rowY + 3);
                }
            }

            // Bottom border for each row
            using var borderPen = new Pen(ColBorder);
            g.DrawLine(borderPen, 0, rowY + RowHeight - 1, Width, rowY + RowHeight - 1);
        }

        // Outer border
        using var outerPen = new Pen(ColBorder);
        g.DrawRectangle(outerPen, 0, GridTop, Width - 1, GridHeight - 1);

        // Splitter full height line
        using var splFullPen = new Pen(ColBorder);
        g.DrawLine(splFullPen, _splitterX, GridTop, _splitterX, GridTop + GridHeight);
    }

    private void DrawToolbarButton(Graphics g, int x, int y, string label, bool active)
    {
        int bw = 22, bh = 16;
        var bg = active
            ? Drawing.Color.FromArgb(204, 228, 247)
            : Drawing.Color.FromArgb(245, 245, 245);
        using var bb = new SolidBrush(bg);
        g.FillRectangle(bb, x, y, bw, bh);
        using var bp = new Pen(Drawing.Color.FromArgb(180, 180, 180));
        g.DrawRectangle(bp, x, y, bw, bh);
        using var tb = new SolidBrush(Drawing.Color.FromArgb(40, 40, 40));
        g.DrawString(label, "Segoe UI", 9, tb, x + 2, y + 2);
    }

    private static string FormatValue(object? val)
    {
        if (val == null) return "(null)";
        if (val is bool b) return b ? "True" : "False";
        if (val is string s) return s;
        return val.ToString() ?? string.Empty;
    }

    // ── Mouse ─────────────────────────────────────────────────────────────────

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();

        // Toolbar buttons
        if (_toolbarVisible && e.Y < HeaderHeight)
        {
            if (e.X >= 4 && e.X < 26)
            {
                PropertySort = PropertySort.CategorizedAlphabetical;
                return;
            }
            if (e.X >= 28 && e.X < 50)
            {
                PropertySort = PropertySort.Alphabetical;
                return;
            }
        }

        // Splitter drag
        if (Math.Abs(e.X - _splitterX) <= 3 && e.Y >= GridTop && e.Y < GridTop + GridHeight)
        {
            _splitterDragging  = true;
            _splitterDragStart = e.X;
            return;
        }

        // Row click
        if (e.Y >= GridTop && e.Y < GridTop + GridHeight)
        {
            CommitEdit();
            int flatIdx = (e.Y - GridTop + _scrollOffset) / RowHeight;
            if (flatIdx < 0 || flatIdx >= _flatRows.Count) return;

            var item = _flatRows[flatIdx];

            // Expand/collapse toggle — categories (click on expand box) or expandable properties
            if (item.IsCategory && e.X <= IndentWidth + 2)
            {
                item.Expanded = !item.Expanded;
                RebuildFlat();
                Invalidate();
                return;
            }
            if (item.Expandable && e.X >= IndentWidth * item.Depth + 2
                                 && e.X <= IndentWidth * item.Depth + 2 + ExpandBoxSize + 2)
            {
                item.Expanded = !item.Expanded;
                RebuildFlat();
                Invalidate();
                return;
            }

            SelectedGridItem = item;

            // Double-click or value-column click starts editing
            if (!item.IsCategory && e.X > _splitterX)
            {
                StartEdit(item);
            }

            Invalidate();
        }
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_splitterDragging)
        {
            _splitterX = Math.Max(60, Math.Min(Width - 60, _splitterX + (e.X - _splitterDragStart)));
            _splitterDragStart = e.X;
            Invalidate();
        }
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _splitterDragging = false;
    }

    protected internal override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        int delta = e.Delta > 0 ? -RowHeight * 3 : RowHeight * 3;
        int maxScroll = Math.Max(0, _flatRows.Count * RowHeight - GridHeight);
        _scrollOffset = Math.Max(0, Math.Min(maxScroll, _scrollOffset + delta));
        Invalidate();
    }

    // ── Keyboard ──────────────────────────────────────────────────────────────

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (_editing)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    CancelEdit();
                    e.Handled = true;
                    return;
                case Keys.Enter:
                    CommitEdit();
                    e.Handled = true;
                    return;
                case Keys.Back:
                    if (_editCaret > 0 && _editBuffer.Length > 0)
                    {
                        _editBuffer = _editBuffer[..(_editCaret - 1)] + _editBuffer[_editCaret..];
                        _editCaret--;
                    }
                    Invalidate();
                    e.Handled = true;
                    return;
                case Keys.Delete:
                    if (_editCaret < _editBuffer.Length)
                    {
                        _editBuffer = _editBuffer[.._editCaret] + _editBuffer[(_editCaret + 1)..];
                    }
                    Invalidate();
                    e.Handled = true;
                    return;
                case Keys.Left:
                    if (_editCaret > 0) _editCaret--;
                    Invalidate();
                    e.Handled = true;
                    return;
                case Keys.Right:
                    if (_editCaret < _editBuffer.Length) _editCaret++;
                    Invalidate();
                    e.Handled = true;
                    return;
                case Keys.Home:
                    _editCaret = 0;
                    Invalidate();
                    e.Handled = true;
                    return;
                case Keys.End:
                    _editCaret = _editBuffer.Length;
                    Invalidate();
                    e.Handled = true;
                    return;
            }
            return;
        }

        // Navigation
        if (_flatRows.Count == 0) return;
        int cur = _selectedItem != null ? _flatRows.IndexOf(_selectedItem) : -1;

        switch (e.KeyCode)
        {
            case Keys.Up:
                if (cur > 0) SelectRow(cur - 1);
                e.Handled = true;
                break;
            case Keys.Down:
                if (cur < _flatRows.Count - 1) SelectRow(cur + 1);
                e.Handled = true;
                break;
            case Keys.PageUp:
                SelectRow(Math.Max(0, cur - VisibleRows));
                e.Handled = true;
                break;
            case Keys.PageDown:
                SelectRow(Math.Min(_flatRows.Count - 1, cur + VisibleRows));
                e.Handled = true;
                break;
            case Keys.Home:
                SelectRow(0);
                e.Handled = true;
                break;
            case Keys.End:
                SelectRow(_flatRows.Count - 1);
                e.Handled = true;
                break;
            case Keys.Enter:
            case Keys.F2:
                if (_selectedItem != null && !_selectedItem.IsCategory)
                    StartEdit(_selectedItem);
                e.Handled = true;
                break;
            case Keys.Left:
                if (_selectedItem?.IsCategory == true)
                {
                    _selectedItem.Expanded = false;
                    RebuildFlat();
                    Invalidate();
                }
                e.Handled = true;
                break;
            case Keys.Right:
                if (_selectedItem?.IsCategory == true)
                {
                    _selectedItem.Expanded = true;
                    RebuildFlat();
                    Invalidate();
                }
                e.Handled = true;
                break;
        }
    }

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        if (_editing && !char.IsControl(e.KeyChar))
        {
            _editBuffer = _editBuffer[.._editCaret] + e.KeyChar + _editBuffer[_editCaret..];
            _editCaret++;
            Invalidate();
            e.Handled = true;
        }
        else if (!_editing && _selectedItem != null && !_selectedItem.IsCategory && !char.IsControl(e.KeyChar))
        {
            // Start editing immediately on printable key press
            StartEdit(_selectedItem, e.KeyChar.ToString());
            e.Handled = true;
        }
    }

    // ── Editing ───────────────────────────────────────────────────────────────

    private void StartEdit(GridItem item, string? initialText = null)
    {
        if (item.PropertyInfo?.CanWrite != true) return;
        _editing    = true;
        _editBuffer = initialText ?? FormatValue(item.Value);
        _editCaret  = _editBuffer.Length;
        Invalidate();
    }

    private void CommitEdit()
    {
        if (!_editing || _selectedItem == null) return;
        _editing = false;

        var prop = _selectedItem.PropertyInfo;
        if (prop?.CanWrite != true) { Invalidate(); return; }

        var oldValue = _selectedItem.Value;
        try
        {
            object? newValue = ConvertValue(_editBuffer, prop.PropertyType);
            prop.SetValue(_selectedItem.OwnerObject, newValue);
            _selectedItem.Value = newValue;
            PropertyValueChanged?.Invoke(this, new PropertyValueChangedEventArgs(_selectedItem, oldValue));
        }
        catch
        {
            // Restore display to current value on conversion failure
            _selectedItem.Value = oldValue;
        }

        Invalidate();
    }

    private void CancelEdit()
    {
        _editing = false;
        Invalidate();
    }

    private static object? ConvertValue(string text, Type targetType)
    {
        if (targetType == typeof(string))  return text;
        if (targetType == typeof(bool))    return bool.Parse(text);
        if (targetType == typeof(int))     return int.Parse(text);
        if (targetType == typeof(float))   return float.Parse(text);
        if (targetType == typeof(double))  return double.Parse(text);
        if (targetType == typeof(decimal)) return decimal.Parse(text);
        if (targetType == typeof(long))    return long.Parse(text);
        if (targetType == typeof(short))   return short.Parse(text);
        if (targetType == typeof(byte))    return byte.Parse(text);
        if (targetType.IsEnum)             return Enum.Parse(targetType, text, ignoreCase: true);

        // Fallback: TypeConverter
        var converter = TypeDescriptor.GetConverter(targetType);
        if (converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFromInvariantString(text);

        return text;
    }

    // ── Scroll helpers ────────────────────────────────────────────────────────

    private void SelectRow(int flatIdx)
    {
        if (flatIdx < 0 || flatIdx >= _flatRows.Count) return;
        SelectedGridItem = _flatRows[flatIdx];

        // Ensure the row is scrolled into view.
        int rowY = flatIdx * RowHeight;
        if (rowY < _scrollOffset)
            _scrollOffset = rowY;
        else if (rowY + RowHeight > _scrollOffset + GridHeight)
            _scrollOffset = rowY + RowHeight - GridHeight;

        Invalidate();
    }
}

// ── SelectedGridItemChangedEventArgs ─────────────────────────────────────────

/// <summary>Event arguments for <see cref="PropertyGrid.SelectedGridItemChanged"/>.</summary>
public class SelectedGridItemChangedEventArgs : EventArgs
{
    public GridItem? OldSelection { get; }
    public GridItem? NewSelection { get; }
    public SelectedGridItemChangedEventArgs(GridItem? oldSel, GridItem? newSel)
    { OldSelection = oldSel; NewSelection = newSel; }
}

public delegate void SelectedGridItemChangedEventHandler(object sender, SelectedGridItemChangedEventArgs e);
