using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

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
