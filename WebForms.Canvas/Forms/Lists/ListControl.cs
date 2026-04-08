using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Base class for list-based controls (ListBox, ComboBox, CheckedListBox)
/// Provides common functionality for controls that display lists of items
/// </summary>
public abstract class ListControl : Control
{
    protected int _selectedIndex = -1;
    protected int _topIndex = 0;
    protected int _hoveredIndex = -1;
    protected ObjectCollection _items;

    // Scrollbar constants
    protected const int ScrollbarWidth = 16;
    protected const int DefaultItemHeight = 16;

    // Scrollbar dragging state
    protected bool _isDraggingScrollbar = false;
    protected int _scrollbarDragStartY = 0;
    protected int _scrollbarDragStartTopIndex = 0;

    public ListControl()
    {
        _items = new ObjectCollection(this);
        SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint, true);
        TabStop = true;
    }

    /// <summary>
    /// Gets the collection of items in the list
    /// </summary>
    public ObjectCollection Items => _items;

    /// <summary>
    /// Gets or sets the zero-based index of the currently selected item
    /// </summary>
    public virtual int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value && value >= -1 && value < _items.Count)
            {
                _selectedIndex = value;
                OnSelectedIndexChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the currently selected item
    /// </summary>
    public object? SelectedItem
    {
        get => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;
        set
        {
            if (value == null)
            {
                SelectedIndex = -1;
            }
            else
            {
                SelectedIndex = _items.IndexOf(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the property to display for items
    /// </summary>
    public string DisplayMember { get; set; } = "";

    /// <summary>
    /// Gets or sets the property to use as the actual value
    /// </summary>
    public string ValueMember { get; set; } = "";

    /// <summary>
    /// Gets or sets the data source for this control
    /// </summary>
    public object? DataSource { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the control formats the display values of the items.
    /// WinForms designers commonly set this to true.
    /// </summary>
    public bool FormattingEnabled { get; set; } = true;

    /// <summary>
    /// Occurs when the selected index changes
    /// </summary>
    public event EventHandler? SelectedIndexChanged;

    /// <summary>
    /// Occurs when the selected value changes
    /// </summary>
    public event EventHandler? SelectedValueChanged;

    protected virtual void OnSelectedIndexChanged(EventArgs e)
    {
        SelectedIndexChanged?.Invoke(this, e);
        OnSelectedValueChanged(e);
    }

    protected virtual void OnSelectedValueChanged(EventArgs e)
    {
        SelectedValueChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Gets the display text for an item
    /// </summary>
    protected virtual string GetItemText(object? item)
    {
        if (item == null) return "";

        if (!string.IsNullOrEmpty(DisplayMember))
        {
            var prop = item.GetType().GetProperty(DisplayMember);
            if (prop != null)
            {
                return prop.GetValue(item)?.ToString() ?? "";
            }
        }

        return item.ToString() ?? "";
    }

    #region Scrollbar Support

    /// <summary>
    /// Gets the height of each item. Override in derived classes if needed.
    /// </summary>
    protected virtual int ItemHeight => DefaultItemHeight;

    /// <summary>
    /// Gets the border width for calculating content bounds. Override in derived classes.
    /// </summary>
    protected virtual int BorderWidth => 2;

    /// <summary>
    /// Gets whether the control needs a scrollbar
    /// </summary>
    protected virtual bool NeedsScrollbar()
    {
        var contentHeight = Height - (BorderWidth * 2);
        var itemsPerPage = contentHeight / ItemHeight;
        return Items.Count > itemsPerPage;
    }

    /// <summary>
    /// Gets the content bounds (area available for items, excluding border and scrollbar)
    /// </summary>
    protected virtual Rectangle GetContentBounds()
    {
        return new Rectangle(
            BorderWidth,
            BorderWidth,
            Width - (BorderWidth * 2) - (NeedsScrollbar() ? ScrollbarWidth : 0),
            Height - (BorderWidth * 2)
        );
    }

    /// <summary>
    /// Gets the scrollbar bounds
    /// </summary>
    protected virtual Rectangle GetScrollbarBounds()
    {
        var contentBounds = GetContentBounds();
        return new Rectangle(
            Width - ScrollbarWidth - BorderWidth,
            BorderWidth,
            ScrollbarWidth,
            Height - (BorderWidth * 2)
        );
    }

    /// <summary>
    /// Gets the scrollbar thumb bounds
    /// </summary>
    protected virtual Rectangle GetScrollbarThumbBounds()
    {
        var scrollbarBounds = GetScrollbarBounds();
        var contentBounds = GetContentBounds();
        var itemsPerPage = contentBounds.Height / ItemHeight;
        var thumbHeight = Math.Max(20, (itemsPerPage * scrollbarBounds.Height) / Math.Max(1, Items.Count));
        var maxTopIndex = Math.Max(1, Items.Count - itemsPerPage);
        var thumbTop = (_topIndex * (scrollbarBounds.Height - thumbHeight)) / maxTopIndex;

        return new Rectangle(
            scrollbarBounds.X + 2,
            scrollbarBounds.Y + thumbTop,
            ScrollbarWidth - 4,
            thumbHeight
        );
    }

    /// <summary>
    /// Draws the scrollbar
    /// </summary>
    protected virtual void DrawScrollbar(Graphics g)
    {
        var scrollbarBounds = GetScrollbarBounds();

        // Scrollbar background
        using var scrollBgBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
        g.FillRectangle(scrollBgBrush, scrollbarBounds);

        // Calculate thumb
        var thumbBounds = GetScrollbarThumbBounds();

        // Thumb
        using var thumbBrush = new SolidBrush(Color.FromArgb(205, 205, 205));
        g.FillRectangle(thumbBrush, thumbBounds);
    }

    /// <summary>
    /// Handles scrollbar mouse down. Returns true if the event was handled.
    /// </summary>
    protected virtual bool HandleScrollbarMouseDown(MouseEventArgs e)
    {
        if (!NeedsScrollbar()) return false;

        var scrollbarBounds = GetScrollbarBounds();
        if (e.X < scrollbarBounds.X || e.X >= scrollbarBounds.Right ||
            e.Y < scrollbarBounds.Y || e.Y >= scrollbarBounds.Bottom)
        {
            return false;
        }

        var thumbBounds = GetScrollbarThumbBounds();

        if (e.Y >= thumbBounds.Y && e.Y < thumbBounds.Bottom)
        {
            // Clicking on thumb - start dragging
            _isDraggingScrollbar = true;
            _scrollbarDragStartY = e.Y;
            _scrollbarDragStartTopIndex = _topIndex;
        }
        else if (e.Y < thumbBounds.Y)
        {
            // Clicking above thumb - page up
            var contentBounds = GetContentBounds();
            var itemsPerPage = contentBounds.Height / ItemHeight;
            _topIndex = Math.Max(0, _topIndex - itemsPerPage);
            Invalidate();
        }
        else
        {
            // Clicking below thumb - page down
            var contentBounds = GetContentBounds();
            var itemsPerPage = contentBounds.Height / ItemHeight;
            var maxTopIndex = Math.Max(0, Items.Count - itemsPerPage);
            _topIndex = Math.Min(maxTopIndex, _topIndex + itemsPerPage);
            Invalidate();
        }

        return true;
    }

    /// <summary>
    /// Handles scrollbar mouse move. Returns true if the event was handled.
    /// </summary>
    protected virtual bool HandleScrollbarMouseMove(MouseEventArgs e)
    {
        if (!_isDraggingScrollbar) return false;

        var scrollbarBounds = GetScrollbarBounds();
        var contentBounds = GetContentBounds();
        var itemsPerPage = contentBounds.Height / ItemHeight;
        var maxTopIndex = Math.Max(0, Items.Count - itemsPerPage);

        // Calculate thumb height and track height
        var thumbHeight = Math.Max(20, (itemsPerPage * scrollbarBounds.Height) / Math.Max(1, Items.Count));
        var trackHeight = scrollbarBounds.Height - thumbHeight;

        // Calculate new top index based on mouse movement
        var deltaY = e.Y - _scrollbarDragStartY;
        var indexDelta = (int)((deltaY * maxTopIndex) / Math.Max(1, trackHeight));
        var newTopIndex = Math.Clamp(_scrollbarDragStartTopIndex + indexDelta, 0, maxTopIndex);

        if (newTopIndex != _topIndex)
        {
            _topIndex = newTopIndex;
            Invalidate();
        }

        return true;
    }

    /// <summary>
    /// Handles scrollbar mouse up
    /// </summary>
    protected virtual void HandleScrollbarMouseUp()
    {
        _isDraggingScrollbar = false;
    }

    /// <summary>
    /// Ensures that the specified item index is visible
    /// </summary>
    public virtual void EnsureVisible(int index)
    {
        if (index < 0 || index >= Items.Count) return;

        var contentBounds = GetContentBounds();
        var itemsPerPage = contentBounds.Height / ItemHeight;

        if (index < _topIndex)
        {
            _topIndex = index;
            Invalidate();
        }
        else if (index >= _topIndex + itemsPerPage)
        {
            _topIndex = index - itemsPerPage + 1;
            Invalidate();
        }
    }

    #endregion

    /// <summary>
    /// Represents a collection of items in a list control
    /// </summary>
    public class ObjectCollection : IList<object>
    {
        private readonly List<object> _items = new();
        private readonly ListControl _owner;

        internal ObjectCollection(ListControl owner)
        {
            _owner = owner;
        }

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public object this[int index]
        {
            get => _items[index];
            set
            {
                _items[index] = value;
                _owner.Invalidate();
            }
        }

        public void Add(object item)
        {
            _items.Add(item);
            _owner.Invalidate();
        }

        public void AddRange(object[] items)
        {
            _items.AddRange(items);
            _owner.Invalidate();
        }

        public void Insert(int index, object item)
        {
            _items.Insert(index, item);
            _owner.Invalidate();
        }

        public bool Remove(object item)
        {
            var result = _items.Remove(item);
            if (result)
            {
                // Adjust selected index if needed
                if (_owner._selectedIndex >= _items.Count)
                {
                    _owner._selectedIndex = _items.Count - 1;
                }
                _owner.Invalidate();
            }
            return result;
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
            // Adjust selected index if needed
            if (_owner._selectedIndex == index)
            {
                _owner._selectedIndex = -1;
                _owner.OnSelectedIndexChanged(EventArgs.Empty);
            }
            else if (_owner._selectedIndex > index)
            {
                _owner._selectedIndex--;
            }
            _owner.Invalidate();
        }

        public void Clear()
        {
            _items.Clear();
            _owner._selectedIndex = -1;
            _owner._topIndex = 0;
            _owner.OnSelectedIndexChanged(EventArgs.Empty);
            _owner.Invalidate();
        }

        public bool Contains(object item) => _items.Contains(item);
        public int IndexOf(object item) => _items.IndexOf(item);
        public void CopyTo(object[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public IEnumerator<object> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
