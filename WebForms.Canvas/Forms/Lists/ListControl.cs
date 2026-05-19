
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

    private object?  _dataSource;
    private string   _displayMember = "";
    private string   _valueMember   = "";

    // Subscribed IBindingList (to unsubscribe on DataSource change)
    private System.ComponentModel.IBindingList? _subscribedBindingList;

    // Scrollbar constants
    protected const int DefaultItemHeight = 16;

    // Scrollbar dragging state
    protected bool _isDraggingScrollbar = false;
    protected int _scrollbarDragStartY = 0;
    protected int _scrollbarDragStartTopIndex = 0;

    public ListControl()
    {
        _items = new ObjectCollection(this);

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
    public string DisplayMember
    {
        get => _displayMember;
        set
        {
            if (_displayMember != value)
            {
                _displayMember = value ?? "";
                if (_dataSource != null) RefreshItemsFromDataSource();
                else Invalidate(); // re-render display text
            }
        }
    }

    /// <summary>
    /// Gets or sets the property to use as the actual value
    /// </summary>
    public string ValueMember
    {
        get => _valueMember;
        set
        {
            _valueMember = value ?? "";
        }
    }

    /// <summary>
    /// Gets or sets the data source for this control.
    /// Accepts IEnumerable, IBindingList, or BindingSource; populates Items automatically.
    /// </summary>
    public object? DataSource
    {
        get => _dataSource;
        set
        {
            if (_dataSource == value) return;

            // Unsubscribe from old binding list
            if (_subscribedBindingList != null)
            {
                _subscribedBindingList.ListChanged -= OnBoundListChanged;
                _subscribedBindingList = null;
            }

            _dataSource = value;

            // Subscribe to new binding list
            var bindingList = value as System.ComponentModel.IBindingList;
            if (bindingList != null)
            {
                _subscribedBindingList = bindingList;
                _subscribedBindingList.ListChanged += OnBoundListChanged;
            }

            RefreshItemsFromDataSource();
        }
    }

    /// <summary>
    /// Gets or sets the value of the selected item using ValueMember.
    /// Falls back to the item itself when ValueMember is empty.
    /// </summary>
    public object? SelectedValue
    {
        get
        {
            var item = SelectedItem;
            if (item == null) return null;
            if (!string.IsNullOrEmpty(_valueMember))
            {
                var prop = item.GetType().GetProperty(_valueMember);
                if (prop != null) return prop.GetValue(item);
            }
            return item;
        }
        set
        {
            if (value == null) { SelectedIndex = -1; return; }
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                object? itemVal;
                if (!string.IsNullOrEmpty(_valueMember))
                {
                    var prop = item?.GetType().GetProperty(_valueMember);
                    itemVal = prop?.GetValue(item);
                }
                else
                {
                    itemVal = item;
                }
                if (Equals(itemVal, value)) { SelectedIndex = i; return; }
            }
            SelectedIndex = -1;
        }
    }

    // Called by DataSource setter and IBindingList.ListChanged
    private void OnBoundListChanged(object? sender, System.ComponentModel.ListChangedEventArgs e)
        => RefreshItemsFromDataSource();

    /// <summary>
    /// Repopulates Items from the current DataSource (IEnumerable).
    /// Preserves selected value by ValueMember if possible.
    /// </summary>
    protected virtual void RefreshItemsFromDataSource()
    {
        var prevValue = SelectedValue; // save before clearing

        _items.Clear();
        _selectedIndex = -1;
        _topIndex = 0;

        var source = _dataSource;
        // BindingSource implements IEnumerable directly — no special unwrapping needed

        if (source is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
                _items.Add(item);
        }

        // Restore selection by value
        if (prevValue != null)
            SelectedValue = prevValue;

        Invalidate();
    }

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

    /// <summary>Gets the height of each item.</summary>
    protected virtual int ItemHeight => DefaultItemHeight;

    /// <summary>Gets the border width for calculating content bounds.</summary>
    protected virtual int BorderWidth => 2;

    /// <summary>Returns the items-per-page count for the current control size.</summary>
    protected int ItemsPerPage()
    {
        var contentHeight = Height - (BorderWidth * 2);
        return Math.Max(1, contentHeight / ItemHeight);
    }

    /// <summary>Gets whether the control needs a scrollbar.</summary>
    protected virtual bool NeedsScrollbar() => Items.Count > ItemsPerPage();

    /// <summary>Builds a helper for the current scroll state.</summary>
    protected VerticalScrollbarHelper MakeScrollbarHelper()
    {
        var track = new Rectangle(
            Width - VerticalScrollbarHelper.Width - BorderWidth,
            BorderWidth,
            VerticalScrollbarHelper.Width,
            Height - (BorderWidth * 2));
        return new VerticalScrollbarHelper(track, Items.Count, ItemsPerPage(), _topIndex);
    }

    /// <summary>Gets the content bounds (area for items, excluding border and scrollbar).</summary>
    protected virtual Rectangle GetContentBounds()
    {
        return new Rectangle(
            BorderWidth,
            BorderWidth,
            Width - (BorderWidth * 2) - (NeedsScrollbar() ? VerticalScrollbarHelper.Width : 0),
            Height - (BorderWidth * 2));
    }

    /// <summary>Draws the scrollbar.</summary>
    protected virtual void DrawScrollbar(Graphics g)
        => MakeScrollbarHelper().Draw(g);

    /// <summary>Handles scrollbar mouse down. Returns true if the event was handled.</summary>
    protected virtual bool HandleScrollbarMouseDown(MouseEventArgs e)
    {
        if (!NeedsScrollbar()) return false;

        var sb = MakeScrollbarHelper();
        var hit = sb.HitTest(e.X, e.Y);
        if (hit == ScrollbarHit.None) return false;

        if (hit == ScrollbarHit.Thumb)
        {
            _isDraggingScrollbar = true;
            _scrollbarDragStartY = e.Y;
            _scrollbarDragStartTopIndex = _topIndex;
        }
        else if (hit == ScrollbarHit.ArrowUp)
        {
            _topIndex = sb.ClampTopIndex(_topIndex - 1);
            Invalidate();
        }
        else if (hit == ScrollbarHit.ArrowDown)
        {
            _topIndex = sb.ClampTopIndex(_topIndex + 1);
            Invalidate();
        }
        else
        {
            _topIndex = sb.ComputePageTopIndex(e.Y, _topIndex);
            Invalidate();
        }

        return true;
    }

    /// <summary>Handles scrollbar mouse move. Returns true if the event was handled.</summary>
    protected virtual bool HandleScrollbarMouseMove(MouseEventArgs e)
    {
        if (!_isDraggingScrollbar) return false;

        var newTop = MakeScrollbarHelper()
            .ComputeDragTopIndex(e.Y, _scrollbarDragStartY, _scrollbarDragStartTopIndex);

        if (newTop != _topIndex)
        {
            _topIndex = newTop;
            Invalidate();
        }

        return true;
    }

    /// <summary>Handles scrollbar mouse up.</summary>
    protected virtual void HandleScrollbarMouseUp() => _isDraggingScrollbar = false;

    /// <summary>Ensures the specified item index is visible.</summary>
    public virtual void EnsureVisible(int index)
    {
        if (index < 0 || index >= Items.Count) return;

        var page = ItemsPerPage();
        if (index < _topIndex)
        {
            _topIndex = index;
            Invalidate();
        }
        else if (index >= _topIndex + page)
        {
            _topIndex = index - page + 1;
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
