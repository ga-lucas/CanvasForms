using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms;

/// <summary>
/// WinForms-compatible BindingSource: wraps an IList and raises ListChanged
/// so DataGridView (and any control honouring IBindingList) stays in sync.
/// </summary>
public class BindingSource : Component, IList, IBindingList, INotifyPropertyChanged
{
    private IList _inner = new List<object?>();
    private string _dataMember = string.Empty;
    private object? _dataSource;
    private int _position = -1;
    private string _filter = string.Empty;
    private string _sort   = string.Empty;

    public BindingSource() { }
    public BindingSource(IContainer container) { container.Add(this); }
    public BindingSource(object dataSource, string dataMember) { DataSource = dataSource; DataMember = dataMember; }

    // ── Events ───────────────────────────────────────────────────
    public event ListChangedEventHandler? ListChanged;
    public event EventHandler? CurrentChanged;
#pragma warning disable CS0067
    public event EventHandler? CurrentItemChanged;
    public event BindingCompleteEventHandler? BindingComplete;
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
    public event EventHandler? DataSourceChanged;
    public event EventHandler? DataMemberChanged;
    public event EventHandler? PositionChanged;
    public event EventHandler<AddingNewEventArgs>? AddingNew;

    // ── DataSource / DataMember ──────────────────────────────────
    public object? DataSource
    {
        get => _dataSource;
        set
        {
            if (_dataSource == value) return;
            _dataSource = value;
            RebindInner();
            DataSourceChanged?.Invoke(this, EventArgs.Empty);
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

    public string DataMember
    {
        get => _dataMember;
        set
        {
            if (_dataMember == value) return;
            _dataMember = value;
            RebindInner();
            DataMemberChanged?.Invoke(this, EventArgs.Empty);
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

    private void RebindInner()
    {
        if (_dataSource == null) { _inner = new List<object?>(); return; }

        // DataTable — bind through DefaultView (IBindingList)
        if (_dataSource is DataTable dt)
        {
            var view = string.IsNullOrEmpty(_dataMember)
                ? dt.DefaultView
                : (dt.DataSet?.Tables[_dataMember]?.DefaultView ?? dt.DefaultView);
            _inner = view;
            return;
        }

        // DataSet — resolve named table
        if (_dataSource is DataSet ds)
        {
            var table = string.IsNullOrEmpty(_dataMember)
                ? ds.Tables[0]
                : ds.Tables[_dataMember];
            _inner = (IList?)table?.DefaultView ?? new List<object?>();
            return;
        }

        if (_dataSource is IList list)      { _inner = list; return; }
        if (_dataSource is IListSource src) { _inner = src.GetList(); return; }
        if (_dataSource is IEnumerable<object> seq) { _inner = seq.ToList(); return; }
        // Wrap scalar
        _inner = new List<object?> { _dataSource };
    }


    // ── Current item / position ──────────────────────────────────
    public int Position
    {
        get => _position;
        set
        {
            int clamped = _inner.Count == 0 ? -1 : Math.Clamp(value, 0, _inner.Count - 1);
            if (clamped == _position) return;
            _position = clamped;
            PositionChanged?.Invoke(this, EventArgs.Empty);
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public object? Current => _position >= 0 && _position < _inner.Count ? _inner[_position] : null;
    public int Count => _inner.Count;

    public void MoveFirst() => Position = 0;
    public void MoveLast() => Position = _inner.Count - 1;
    public void MoveNext() { if (_position < _inner.Count - 1) Position++; }
    public void MovePrevious() { if (_position > 0) Position--; }

    // ── List mutation ────────────────────────────────────────────
    public void Add(object? item)
    {
        _inner.Add(item);
        if (_position < 0 && _inner.Count == 1) _position = 0;
        OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, _inner.Count - 1));
    }

    public void Remove(object? item)
    {
        int idx = _inner.IndexOf(item);
        if (idx >= 0) RemoveAt(idx);
    }

    public void RemoveAt(int index)
    {
        _inner.RemoveAt(index);
        if (_position >= _inner.Count) _position = _inner.Count - 1;
        OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
    }

    public void Clear()
    {
        _inner.Clear();
        _position = -1;
        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    public void ResetBindings(bool metadataChanged = false)
        => OnListChanged(new ListChangedEventArgs(metadataChanged ? ListChangedType.PropertyDescriptorChanged : ListChangedType.Reset, -1));

    public void ResetCurrentItem()
    {
        if (_position >= 0)
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, _position));
    }

    // ── Filter / Sort ────────────────────────────────────────────
    /// <summary>
    /// Gets or sets the expression used to filter which rows are viewed.
    /// Stub: stores the value and raises ListChanged; actual row filtering
    /// must be implemented by the caller or a derived class.
    /// </summary>
    public string Filter
    {
        get => _filter;
        set
        {
            if (_filter == value) return;
            _filter = value ?? string.Empty;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

    /// <summary>
    /// Gets or sets the column name(s) and sort direction used to sort rows.
    /// Stub: stores the value and raises ListChanged; actual sort ordering
    /// must be implemented by the caller or a derived class.
    /// </summary>
    public string Sort
    {
        get => _sort;
        set
        {
            if (_sort == value) return;
            _sort = value ?? string.Empty;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

    /// <summary>
    /// Searches for the index of the item whose property named
    /// <paramref name="propertyName"/> equals <paramref name="key"/>.
    /// Returns -1 if no match is found.
    /// </summary>
    public int Find(string propertyName, object key)
    {
        for (int i = 0; i < _inner.Count; i++)
        {
            var item = _inner[i];
            if (item is null) continue;
            var prop = System.ComponentModel.TypeDescriptor.GetProperties(item)[propertyName];
            if (prop is null) continue;
            var val = prop.GetValue(item);
            if (Equals(val, key)) return i;
        }
        return -1;
    }

    // ── RemoveFilter / RemoveSort ────────────────────────────────
    public void RemoveFilter() => Filter = string.Empty;
    public void RemoveSort()   => Sort   = string.Empty;

    private void OnListChanged(ListChangedEventArgs e)
    {
        ListChanged?.Invoke(this, e);
    }

    // ── IList ────────────────────────────────────────────────────
    bool IList.IsReadOnly => _inner.IsReadOnly;
    bool IList.IsFixedSize => _inner.IsFixedSize;
    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => this;

    public object? this[int index]
    {
        get => _inner[index];
        set { _inner[index] = value; OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index)); }
    }

    int IList.Add(object? value) { Add(value); return _inner.Count - 1; }
    bool IList.Contains(object? value) => _inner.Contains(value);
    int IList.IndexOf(object? value) => _inner.IndexOf(value);
    void IList.Insert(int index, object? value) { _inner.Insert(index, value); OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index)); }
    void IList.Remove(object? value) => Remove(value);
    void IList.RemoveAt(int index) => RemoveAt(index);
    void IList.Clear() => Clear();
    void ICollection.CopyTo(Array array, int index) => _inner.CopyTo(array, index);
    public IEnumerator GetEnumerator() => _inner.GetEnumerator();

    // ── IBindingList ──────────────────────────────────────────────
    bool IBindingList.AllowEdit => !_inner.IsReadOnly;
    bool IBindingList.AllowNew => !_inner.IsReadOnly && !_inner.IsFixedSize;
    bool IBindingList.AllowRemove => !_inner.IsReadOnly && !_inner.IsFixedSize;
    bool IBindingList.SupportsChangeNotification => true;
    bool IBindingList.SupportsSearching => true;
    bool IBindingList.SupportsSorting => false;
    bool IBindingList.IsSorted => false;
    PropertyDescriptor? IBindingList.SortProperty => null;
    ListSortDirection IBindingList.SortDirection => ListSortDirection.Ascending;
    void IBindingList.AddIndex(PropertyDescriptor property) { }
    void IBindingList.RemoveIndex(PropertyDescriptor property) { }
    void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) => throw new NotSupportedException();
    void IBindingList.RemoveSort() => throw new NotSupportedException();
    int IBindingList.Find(PropertyDescriptor property, object key) => Find(property.Name, key);
    object? IBindingList.AddNew()
    {
        var args = new AddingNewEventArgs();
        AddingNew?.Invoke(this, args);
        var newItem = args.NewObject ?? Activator.CreateInstance(_inner.Count > 0 ? _inner[0]!.GetType() : typeof(object));
        Add(newItem);
        return newItem;
    }
}

public delegate void BindingCompleteEventHandler(object? sender, BindingCompleteEventArgs e);
public class BindingCompleteEventArgs : EventArgs
{
    public Exception? Exception { get; }
    public bool Cancel { get; set; }
    public BindingCompleteContext BindingCompleteContext { get; }
    public string ErrorText { get; }
    public BindingCompleteEventArgs(object? binding, BindingCompleteContext context, string errorText = "", Exception? exception = null)
    {
        BindingCompleteContext = context; ErrorText = errorText; Exception = exception;
    }
}
public enum BindingCompleteContext { DataSourceUpdate, ControlUpdate }

public class AddingNewEventArgs : EventArgs
{
    public object? NewObject { get; set; }
}
