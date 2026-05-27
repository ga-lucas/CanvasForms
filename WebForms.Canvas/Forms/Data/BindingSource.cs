using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms;

/// <summary>
/// WinForms-compatible BindingSource: wraps an IList and raises ListChanged
/// so DataGridView (and any control honouring IBindingList) stays in sync.
/// </summary>
public class BindingSource : Component, IList, IBindingList, INotifyPropertyChanged,
    System.ComponentModel.ISupportInitialize
{
    void System.ComponentModel.ISupportInitialize.BeginInit() { }
    void System.ComponentModel.ISupportInitialize.EndInit() { }

    // _source is the raw backing list; _inner is the active (possibly filtered/sorted) view.
    private IList _source = new List<object?>();
    private IList _inner  = new List<object?>();
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
        if (_dataSource == null) { _source = new List<object?>(); ApplyFilterSort(); return; }

        // DataTable — bind through DefaultView (IBindingList)
        if (_dataSource is DataTable dt)
        {
            var view = string.IsNullOrEmpty(_dataMember)
                ? dt.DefaultView
                : (dt.DataSet?.Tables[_dataMember]?.DefaultView ?? dt.DefaultView);
            _source = view;
            ApplyFilterSort();
            return;
        }

        // DataSet — resolve named table
        if (_dataSource is DataSet ds)
        {
            var table = string.IsNullOrEmpty(_dataMember)
                ? ds.Tables[0]
                : ds.Tables[_dataMember];
            _source = (IList?)table?.DefaultView ?? new List<object?>();
            ApplyFilterSort();
            return;
        }

        if (_dataSource is IList list)      { _source = list; ApplyFilterSort(); return; }
        if (_dataSource is IListSource src) { _source = src.GetList(); ApplyFilterSort(); return; }
        if (_dataSource is IEnumerable<object> seq) { _source = seq.ToList(); ApplyFilterSort(); return; }
        // Wrap scalar
        _source = new List<object?> { _dataSource };
        ApplyFilterSort();
    }

    /// <summary>
    /// Rebuilds <c>_inner</c> by applying the current <see cref="Filter"/> and <see cref="Sort"/>
    /// expressions.  For <see cref="DataView"/> sources the native RowFilter/Sort properties are
    /// used; for plain <see cref="IList"/> sources a LINQ-based approach is used.
    /// </summary>
    private void ApplyFilterSort()
    {
        // DataView: delegate natively — DataView already implements IBindingList with filter/sort.
        if (_source is DataView dv)
        {
            if (!string.IsNullOrEmpty(_filter)) dv.RowFilter = _filter;
            if (!string.IsNullOrEmpty(_sort))   dv.Sort      = _sort;
            _inner = dv;
            ClampPosition();
            return;
        }

        // Generic IList: apply filter then sort in-memory.
        IEnumerable<object?> query = _source.Cast<object?>();

        // --- Filter ---
        // Simple equality filters: "PropertyName = 'value'" or "PropertyName = 123"
        if (!string.IsNullOrWhiteSpace(_filter))
        {
            var pred = BuildFilterPredicate(_filter);
            if (pred != null) query = query.Where(pred);
        }

        // --- Sort ---
        // Supports "Property1 ASC, Property2 DESC" (WinForms DataView sort syntax)
        if (!string.IsNullOrWhiteSpace(_sort))
        {
            query = ApplySortExpression(query, _sort);
        }

        _inner = query.ToList();
        ClampPosition();
    }

    private static Func<object?, bool>? BuildFilterPredicate(string filter)
    {
        // Very lightweight parser for "PropName = 'value'" or "PropName = value"
        var eq = filter.IndexOf('=');
        if (eq < 0) return null;
        var propName = filter[..eq].Trim();
        var raw      = filter[(eq + 1)..].Trim().Trim('\'', '"');
        return item =>
        {
            if (item is null) return false;
            var prop = TypeDescriptor.GetProperties(item)[propName];
            if (prop is null) return false;
            var val = prop.GetValue(item);
            return val?.ToString()?.Equals(raw, StringComparison.OrdinalIgnoreCase) == true;
        };
    }

    private static IEnumerable<object?> ApplySortExpression(IEnumerable<object?> source, string sort)
    {
        var segments = sort.Split(',');
        IOrderedEnumerable<object?>? ordered = null;
        foreach (var seg in segments)
        {
            var parts = seg.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            var prop = parts[0];
            bool desc = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);

            if (ordered == null)
            {
                ordered = desc
                    ? source.OrderByDescending(x => GetPropertyValue(x, prop))
                    : source.OrderBy(x => GetPropertyValue(x, prop));
            }
            else
            {
                ordered = desc
                    ? ordered.ThenByDescending(x => GetPropertyValue(x, prop))
                    : ordered.ThenBy(x => GetPropertyValue(x, prop));
            }
        }
        return ordered ?? source;
    }

    private static object? GetPropertyValue(object? item, string propName)
    {
        if (item is null) return null;
        var pd = TypeDescriptor.GetProperties(item)[propName];
        return pd?.GetValue(item);
    }

    private void ClampPosition()
    {
        _position = _inner.Count == 0 ? -1 : Math.Clamp(Math.Max(0, _position), 0, _inner.Count - 1);
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
        _source.Add(item);
        ApplyFilterSort();
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
        // index refers to _inner (view); find and remove from _source
        var item = _inner.Count > index ? _inner[index] : null;
        if (item != null) _source.Remove(item);
        else if (_source.Count > index) _source.RemoveAt(index);
        ApplyFilterSort();
        if (_position >= _inner.Count) _position = _inner.Count - 1;
        OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
    }

    public void Clear()
    {
        _source.Clear();
        _inner = new List<object?>();
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
    /// Filters the view to rows matching the expression (e.g. "Name = 'Alice'").
    /// For <see cref="DataView"/> sources the native RowFilter is used; for plain
    /// IList sources a simple equality predicate is applied.
    /// </summary>
    public string Filter
    {
        get => _filter;
        set
        {
            if (_filter == value) return;
            _filter = value ?? string.Empty;
            ApplyFilterSort();
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
    }

    public string Sort
    {
        get => _sort;
        set
        {
            if (_sort == value) return;
            _sort = value ?? string.Empty;
            ApplyFilterSort();
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
    bool IBindingList.SupportsSorting => true;
    bool IBindingList.IsSorted => !string.IsNullOrEmpty(_sort);
    PropertyDescriptor? IBindingList.SortProperty => null;
    ListSortDirection IBindingList.SortDirection => ListSortDirection.Ascending;
    void IBindingList.AddIndex(PropertyDescriptor property) { }
    void IBindingList.RemoveIndex(PropertyDescriptor property) { }
    void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
    {
        Sort = direction == ListSortDirection.Descending ? $"{property.Name} DESC" : property.Name;
    }
    void IBindingList.RemoveSort() => RemoveSort();
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
