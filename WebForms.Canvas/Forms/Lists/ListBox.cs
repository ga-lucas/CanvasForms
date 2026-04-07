using WebForms.Canvas.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms list box control
/// </summary>
public class ListBox : ListControl
{
    private const int ItemPadding = 2;
    private SelectionMode _selectionMode = SelectionMode.One;
    private readonly HashSet<int> _selectedIndices = new();
    private int _mouseDownIndex = -1;

    public ListBox()
    {
        Width = 120;
        Height = 96;
        BackColor = Color.White;
        ForeColor = Color.Black;
        BorderStyle = BorderStyle.Fixed3D;
    }

    /// <summary>
    /// Gets or sets the method of item selection
    /// </summary>
    public SelectionMode SelectionMode
    {
        get => _selectionMode;
        set
        {
            if (_selectionMode != value)
            {
                _selectionMode = value;
                if (_selectionMode == SelectionMode.One)
                {
                    // Clear multi-selection
                    _selectedIndices.Clear();
                    if (_selectedIndex >= 0)
                    {
                        _selectedIndices.Add(_selectedIndex);
                    }
                }
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the border style
    /// </summary>
    public BorderStyle BorderStyle { get; set; }

    /// <summary>
    /// Override border width based on BorderStyle
    /// </summary>
    protected override int BorderWidth => BorderStyle == BorderStyle.None ? 0 : 2;

    /// <summary>
    /// Gets or sets whether the list box should scroll when items are added
    /// </summary>
    public bool ScrollAlwaysVisible { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the list box supports multiple columns
    /// </summary>
    public bool MultiColumn { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the horizontal scrollbar is shown
    /// </summary>
    public bool HorizontalScrollbar { get; set; } = false;

    /// <summary>
    /// Gets or sets whether items are sorted alphabetically
    /// </summary>
    public bool Sorted { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the control should draw its own items
    /// </summary>
    public DrawMode DrawMode { get; set; } = DrawMode.Normal;

    /// <summary>
    /// Gets the collection of selected indices
    /// </summary>
    public SelectedIndexCollection SelectedIndices => new SelectedIndexCollection(_selectedIndices);

    /// <summary>
    /// Gets the collection of selected items
    /// </summary>
    public SelectedObjectCollection SelectedItems => new SelectedObjectCollection(this, _selectedIndices);

    public override int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value && value >= -1 && value < Items.Count)
            {
                _selectedIndex = value;
                _selectedIndices.Clear();
                if (value >= 0)
                {
                    _selectedIndices.Add(value);
                }
                OnSelectedIndexChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);

        // Draw background
        using var bgBrush = new SolidBrush(Enabled ? BackColor : Color.FromArgb(240, 240, 240));
        g.FillRectangle(bgBrush, bounds);

        // Draw border
        DrawBorder(g, bounds);

        // Calculate visible area
        var contentBounds = GetContentBounds();
        var itemsPerPage = contentBounds.Height / ItemHeight;
        var visibleCount = Math.Min(itemsPerPage, Items.Count - _topIndex);

        // Draw items
        for (int i = 0; i < visibleCount; i++)
        {
            var itemIndex = _topIndex + i;
            if (itemIndex >= Items.Count) break;

            var itemBounds = new Rectangle(
                contentBounds.X,
                contentBounds.Y + (i * ItemHeight),
                contentBounds.Width,
                ItemHeight
            );

            DrawItem(g, itemIndex, itemBounds);
        }

        // Draw scrollbar if needed (uses base class method)
        if (NeedsScrollbar())
        {
            DrawScrollbar(g);
        }

        base.OnPaint(e);
    }

    private void DrawBorder(Graphics g, Rectangle bounds)
    {
        switch (BorderStyle)
        {
            case BorderStyle.FixedSingle:
                using (var pen = new Pen(Color.FromArgb(122, 122, 122)))
                {
                    g.DrawRectangle(pen, bounds);
                }
                break;

            case BorderStyle.Fixed3D:
                // Outer dark border
                using (var darkPen = new Pen(Color.FromArgb(122, 122, 122)))
                {
                    g.DrawRectangle(darkPen, bounds);
                }
                // Inner light border
                using (var lightPen = new Pen(Color.FromArgb(240, 240, 240)))
                {
                    g.DrawRectangle(lightPen, new Rectangle(1, 1, Width - 3, Height - 3));
                }
                break;
        }
    }

    private void DrawItem(Graphics g, int index, Rectangle bounds)
    {
        var item = Items[index];
        var isSelected = _selectedIndices.Contains(index);
        var isHovered = _hoveredIndex == index && Enabled;

        // Background
        Color itemBgColor;
        if (isSelected)
        {
            itemBgColor = Enabled ? Color.FromArgb(0, 120, 215) : Color.FromArgb(191, 191, 191);
        }
        else if (isHovered)
        {
            itemBgColor = Color.FromArgb(229, 243, 255);
        }
        else
        {
            itemBgColor = BackColor;
        }

        using var itemBgBrush = new SolidBrush(itemBgColor);
        g.FillRectangle(itemBgBrush, bounds);

        // Text
        var text = GetItemText(item);
        var textColor = isSelected
            ? Color.White
            : (Enabled ? ForeColor : Color.FromArgb(109, 109, 109));

        g.DrawString(text, bounds.X + ItemPadding, bounds.Y + ItemPadding, textColor);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseDown(e);
            return;
        }

        Focus();

        // Check if clicking on scrollbar (use base class method)
        if (HandleScrollbarMouseDown(e))
        {
            base.OnMouseDown(e);
            return;
        }

        // Check if clicking on content area (items)
        var contentArea = GetContentBounds();
        if (e.X >= contentArea.X && e.X < contentArea.Right &&
            e.Y >= contentArea.Y && e.Y < contentArea.Bottom)
        {
            var itemIndex = _topIndex + ((e.Y - contentArea.Y) / ItemHeight);
            if (itemIndex >= 0 && itemIndex < Items.Count)
            {
                _mouseDownIndex = itemIndex;
                HandleItemSelection(itemIndex, ModifierKeys);
            }
        }

        base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        _mouseDownIndex = -1;
        HandleScrollbarMouseUp();
        base.OnMouseUp(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseMove(e);
            return;
        }

        // Handle scrollbar dragging (use base class method)
        if (HandleScrollbarMouseMove(e))
        {
            base.OnMouseMove(e);
            return;
        }

        var contentArea = GetContentBounds();
        var oldHoveredIndex = _hoveredIndex;

        if (e.X >= contentArea.X && e.X < contentArea.Right &&
            e.Y >= contentArea.Y && e.Y < contentArea.Bottom)
        {
            _hoveredIndex = _topIndex + ((e.Y - contentArea.Y) / ItemHeight);
            if (_hoveredIndex >= Items.Count)
            {
                _hoveredIndex = -1;
            }
        }
        else
        {
            _hoveredIndex = -1;
        }

        if (_hoveredIndex != oldHoveredIndex)
        {
            Invalidate();
        }

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_hoveredIndex != -1)
        {
            _hoveredIndex = -1;
            Invalidate();
        }
        base.OnMouseLeave(e);
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (!Enabled)
        {
            base.OnKeyDown(e);
            return;
        }

        var handled = false;

        switch (e.KeyCode)
        {
            case Keys.Up:
                if (_selectedIndex > 0)
                {
                    SelectedIndex = _selectedIndex - 1;
                    EnsureVisible(_selectedIndex);
                    handled = true;
                }
                break;

            case Keys.Down:
                if (_selectedIndex < Items.Count - 1)
                {
                    SelectedIndex = _selectedIndex + 1;
                    EnsureVisible(_selectedIndex);
                    handled = true;
                }
                break;

            case Keys.Home:
                if (Items.Count > 0)
                {
                    SelectedIndex = 0;
                    EnsureVisible(0);
                    handled = true;
                }
                break;

            case Keys.End:
                if (Items.Count > 0)
                {
                    SelectedIndex = Items.Count - 1;
                    EnsureVisible(Items.Count - 1);
                    handled = true;
                }
                break;

            case Keys.PageUp:
                {
                    var itemsPerPage = GetContentBounds().Height / ItemHeight;
                    var newIndex = Math.Max(0, _selectedIndex - itemsPerPage);
                    SelectedIndex = newIndex;
                    EnsureVisible(newIndex);
                    handled = true;
                }
                break;

            case Keys.PageDown:
                {
                    var itemsPerPage = GetContentBounds().Height / ItemHeight;
                    var newIndex = Math.Min(Items.Count - 1, _selectedIndex + itemsPerPage);
                    SelectedIndex = newIndex;
                    EnsureVisible(newIndex);
                    handled = true;
                }
                break;
        }

        if (handled)
        {
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    private void HandleItemSelection(int index, Keys modifiers)
    {
        switch (_selectionMode)
        {
            case SelectionMode.One:
                SelectedIndex = index;
                break;

            case SelectionMode.MultiSimple:
                if (_selectedIndices.Contains(index))
                {
                    _selectedIndices.Remove(index);
                    if (_selectedIndex == index)
                    {
                        _selectedIndex = _selectedIndices.Count > 0 ? _selectedIndices.First() : -1;
                    }
                }
                else
                {
                    _selectedIndices.Add(index);
                    _selectedIndex = index;
                }
                OnSelectedIndexChanged(EventArgs.Empty);
                Invalidate();
                break;

            case SelectionMode.MultiExtended:
                if ((modifiers & Keys.Control) != 0)
                {
                    // Toggle selection
                    if (_selectedIndices.Contains(index))
                    {
                        _selectedIndices.Remove(index);
                    }
                    else
                    {
                        _selectedIndices.Add(index);
                    }
                    _selectedIndex = index;
                }
                else if ((modifiers & Keys.Shift) != 0)
                {
                    // Range selection
                    _selectedIndices.Clear();
                    var start = Math.Min(_selectedIndex, index);
                    var end = Math.Max(_selectedIndex, index);
                    for (int i = start; i <= end; i++)
                    {
                        _selectedIndices.Add(i);
                    }
                    _selectedIndex = index;
                }
                else
                {
                    // Single selection
                    _selectedIndices.Clear();
                    _selectedIndices.Add(index);
                    _selectedIndex = index;
                }
                OnSelectedIndexChanged(EventArgs.Empty);
                Invalidate();
                break;
        }
    }

    /// <summary>
    /// Sets the selected state of the item at the specified index
    /// </summary>
    public void SetSelected(int index, bool value)
    {
        if (index < 0 || index >= Items.Count) return;

        if (value)
        {
            if (_selectionMode == SelectionMode.One)
            {
                SelectedIndex = index;
            }
            else
            {
                _selectedIndices.Add(index);
                Invalidate();
            }
        }
        else
        {
            _selectedIndices.Remove(index);
            if (_selectedIndex == index)
            {
                _selectedIndex = _selectedIndices.Count > 0 ? _selectedIndices.First() : -1;
            }
            Invalidate();
        }
    }

    /// <summary>
    /// Gets whether the item at the specified index is selected
    /// </summary>
    public bool GetSelected(int index)
    {
        return _selectedIndices.Contains(index);
    }

    /// <summary>
    /// Clears all selections
    /// </summary>
    public void ClearSelected()
    {
        _selectedIndices.Clear();
        _selectedIndex = -1;
        OnSelectedIndexChanged(EventArgs.Empty);
        Invalidate();
    }

    /// <summary>
    /// Finds the first item that starts with the specified string
    /// </summary>
    public int FindString(string s)
    {
        return FindString(s, -1);
    }

    /// <summary>
    /// Finds the first item after the specified index that starts with the specified string
    /// </summary>
    public int FindString(string s, int startIndex)
    {
        if (string.IsNullOrEmpty(s)) return -1;

        for (int i = startIndex + 1; i < Items.Count; i++)
        {
            var text = GetItemText(Items[i]);
            if (text.StartsWith(s, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Collection of selected indices
    /// </summary>
    public class SelectedIndexCollection : IList<int>
    {
        private readonly HashSet<int> _indices;

        internal SelectedIndexCollection(HashSet<int> indices)
        {
            _indices = indices;
        }

        public int Count => _indices.Count;
        public bool IsReadOnly => true;
        public int this[int index]
        {
            get => _indices.ElementAt(index);
            set => throw new NotSupportedException();
        }

        public bool Contains(int item) => _indices.Contains(item);
        public void CopyTo(int[] array, int arrayIndex) => _indices.CopyTo(array, arrayIndex);
        public IEnumerator<int> GetEnumerator() => _indices.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(int item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Remove(int item) => throw new NotSupportedException();
        public int IndexOf(int item) => throw new NotSupportedException();
        public void Insert(int index, int item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
    }

    /// <summary>
    /// Collection of selected objects
    /// </summary>
    public class SelectedObjectCollection : IList<object>
    {
        private readonly ListBox _owner;
        private readonly HashSet<int> _indices;

        internal SelectedObjectCollection(ListBox owner, HashSet<int> indices)
        {
            _owner = owner;
            _indices = indices;
        }

        public int Count => _indices.Count;
        public bool IsReadOnly => true;
        public object this[int index]
        {
            get => _owner.Items[_indices.ElementAt(index)];
            set => throw new NotSupportedException();
        }

        public bool Contains(object item) => _indices.Any(i => _owner.Items[i].Equals(item));
        public void CopyTo(object[] array, int arrayIndex)
        {
            var items = _indices.Select(i => _owner.Items[i]).ToArray();
            items.CopyTo(array, arrayIndex);
        }
        public IEnumerator<object> GetEnumerator() => _indices.Select(i => _owner.Items[i]).GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(object item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Remove(object item) => throw new NotSupportedException();
        public int IndexOf(object item) => throw new NotSupportedException();
        public void Insert(int index, object item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
    }
}

/// <summary>
/// Specifies the selection behavior of a list box
/// </summary>
public enum SelectionMode
{
    None,
    One,
    MultiSimple,
    MultiExtended
}

/// <summary>
/// Specifies whether items in a list box are drawn by the operating system or by code
/// </summary>
public enum DrawMode
{
    Normal,
    OwnerDrawFixed,
    OwnerDrawVariable
}
