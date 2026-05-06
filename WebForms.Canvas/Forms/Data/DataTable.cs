using System.Collections;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace System.Windows.Forms;

// ??????????????????????????????????????????????????????????????
//  DataColumn
// ??????????????????????????????????????????????????????????????

/// <summary>Represents the schema of one column in a DataTable.</summary>
public class DataColumn
{
    public DataColumn() { }
    public DataColumn(string columnName) { ColumnName = columnName; }
    public DataColumn(string columnName, Type dataType) { ColumnName = columnName; DataType = dataType; }

    public string  ColumnName   { get; set; } = string.Empty;
    public string  Caption      { get; set; } = string.Empty;
    public Type    DataType     { get; set; } = typeof(string);
    public object? DefaultValue { get; set; }
    public bool    AllowDBNull  { get; set; } = true;
    public bool    ReadOnly     { get; set; } = false;
    public bool    Unique       { get; set; } = false;
    public int     MaxLength    { get; set; } = -1;
    public int     Ordinal      { get; internal set; } = -1;
    public string  Expression   { get; set; } = string.Empty;
    public MappingType ColumnMapping { get; set; } = MappingType.Element;

    internal DataTable? Table { get; set; }
}

public enum MappingType { Element = 1, Attribute = 2, SimpleContent = 3, Hidden = 4 }

// ??????????????????????????????????????????????????????????????
//  DataColumnCollection
// ??????????????????????????????????????????????????????????????

public class DataColumnCollection : IEnumerable<DataColumn>
{
    private readonly List<DataColumn> _list = new();
    private readonly DataTable _owner;

    internal DataColumnCollection(DataTable owner) => _owner = owner;

    public int      Count          => _list.Count;
    public DataColumn this[int index] => _list[index];
    public DataColumn? this[string name] =>
        _list.FirstOrDefault(c => string.Equals(c.ColumnName, name, StringComparison.OrdinalIgnoreCase));

    public DataColumn Add(string columnName)
    {
        var col = new DataColumn(columnName) { Ordinal = _list.Count, Table = _owner };
        _list.Add(col);
        _owner.OnColumnsChanged();
        return col;
    }

    public DataColumn Add(string columnName, Type dataType)
    {
        var col = new DataColumn(columnName, dataType) { Ordinal = _list.Count, Table = _owner };
        _list.Add(col);
        _owner.OnColumnsChanged();
        return col;
    }

    public void Add(DataColumn column)
    {
        column.Ordinal = _list.Count;
        column.Table   = _owner;
        _list.Add(column);
        _owner.OnColumnsChanged();
    }

    public void Remove(DataColumn column) { _list.Remove(column); RenumberOrdinals(); _owner.OnColumnsChanged(); }
    public void RemoveAt(int index)       { _list.RemoveAt(index); RenumberOrdinals(); _owner.OnColumnsChanged(); }
    public bool Contains(string name)     => this[name] != null;

    private void RenumberOrdinals() { for (int i = 0; i < _list.Count; i++) _list[i].Ordinal = i; }

    public IEnumerator<DataColumn> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}

// ??????????????????????????????????????????????????????????????
//  DataRow
// ??????????????????????????????????????????????????????????????

/// <summary>Represents one row of data in a DataTable.</summary>
public class DataRow
{
    private readonly object?[] _values;
    private readonly DataTable _table;

    internal DataRow(DataTable table)
    {
        _table  = table;
        _values = new object?[table.Columns.Count];
        for (int i = 0; i < _values.Length; i++)
            _values[i] = table.Columns[i].DefaultValue;
    }

    public object? this[int index]
    {
        get => index >= 0 && index < _values.Length ? _values[index] : null;
        set
        {
            if (index < 0 || index >= _values.Length) return;
            var old = _values[index];
            _values[index] = value;
            _table.OnCellChanged(this, index, old, value);
        }
    }

    public object? this[string columnName]
    {
        get
        {
            var col = _table.Columns[columnName] ?? throw new ArgumentException($"Column '{columnName}' not found.");
            return _values[col.Ordinal];
        }
        set
        {
            var col = _table.Columns[columnName] ?? throw new ArgumentException($"Column '{columnName}' not found.");
            var old = _values[col.Ordinal];
            _values[col.Ordinal] = value;
            _table.OnCellChanged(this, col.Ordinal, old, value);
        }
    }

    public object? this[DataColumn column]
    {
        get => _values[column.Ordinal];
        set
        {
            var old = _values[column.Ordinal];
            _values[column.Ordinal] = value;
            _table.OnCellChanged(this, column.Ordinal, old, value);
        }
    }

    public DataRowState RowState { get; internal set; } = DataRowState.Added;
    public DataTable    Table    => _table;

    public bool IsNull(int index)         => _values[index] == null || _values[index] == DBNull.Value;
    public bool IsNull(string columnName) => IsNull(_table.Columns[columnName]!.Ordinal);
    public bool IsNull(DataColumn column) => IsNull(column.Ordinal);

    public object?[] ItemArray
    {
        get => (object?[])_values.Clone();
        set
        {
            int len = Math.Min(value.Length, _values.Length);
            for (int i = 0; i < len; i++)
            {
                var old = _values[i];
                _values[i] = value[i];
                _table.OnCellChanged(this, i, old, value[i]);
            }
        }
    }

    public void BeginEdit()  { /* WinForms compat no-op */ }
    public void EndEdit()    { RowState = DataRowState.Modified; }
    public void CancelEdit() { /* WinForms compat no-op */ }
    public void Delete()     { RowState = DataRowState.Deleted; _table.Rows.Remove(this); }
}

public enum DataRowState { Detached = 1, Unchanged = 2, Added = 4, Deleted = 8, Modified = 16 }

// ??????????????????????????????????????????????????????????????
//  DataRowCollection
// ??????????????????????????????????????????????????????????????

public class DataRowCollection : IEnumerable<DataRow>
{
    private readonly List<DataRow> _list = new();
    private readonly DataTable _owner;

    internal DataRowCollection(DataTable owner) => _owner = owner;

    public int     Count          => _list.Count;
    public DataRow this[int index] => _list[index];

    public void Add(DataRow row)
    {
        _list.Add(row);
        _owner.OnRowAdded(row);
    }

    public DataRow Add(params object?[] values)
    {
        var row = _owner.NewRow();
        row.ItemArray = values;
        Add(row);
        return row;
    }

    public void Remove(DataRow row)
    {
        int idx = _list.IndexOf(row);
        if (idx < 0) return;
        _list.RemoveAt(idx);
        _owner.OnRowRemoved(row, idx);
    }

    public void RemoveAt(int index)
    {
        var row = _list[index];
        _list.RemoveAt(index);
        _owner.OnRowRemoved(row, index);
    }

    public void  Clear()                  { _list.Clear(); _owner.OnReset(); }
    public bool  Contains(DataRow row)    => _list.Contains(row);
    public int   IndexOf(DataRow row)     => _list.IndexOf(row);

    public IEnumerator<DataRow> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}

// ??????????????????????????????????????????????????????????????
//  DataRowView  (IBindingList item)
// ??????????????????????????????????????????????????????????????

/// <summary>
/// Wraps a <see cref="DataRow"/> for consumption by data-bound controls.
/// Implements <see cref="ICustomTypeDescriptor"/> so property grid / binding
/// infrastructure can discover columns dynamically.
/// </summary>
public class DataRowView : ICustomTypeDescriptor, INotifyPropertyChanged
{
    internal readonly DataRow Row;
    private readonly DataView _view;

    internal DataRowView(DataView view, DataRow row) { _view = view; Row = row; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public object? this[int index]
    {
        get => Row[index];
        set { Row[index] = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Row.Table.Columns[index].ColumnName)); }
    }

    public object? this[string column]
    {
        get => Row[column];
        set { Row[column] = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(column)); }
    }

    public DataRow      DataRow   => Row;
    public DataView     DataView  => _view;
    public DataRowState RowState  => Row.RowState;

    public void BeginEdit()  => Row.BeginEdit();
    public void EndEdit()    => Row.EndEdit();
    public void CancelEdit() => Row.CancelEdit();
    public void Delete()     => Row.Delete();

    // ?? ICustomTypeDescriptor ?????????????????????????????????????????????????

    AttributeCollection           ICustomTypeDescriptor.GetAttributes()           => AttributeCollection.Empty;
    string?                       ICustomTypeDescriptor.GetClassName()             => nameof(DataRowView);
    string?                       ICustomTypeDescriptor.GetComponentName()         => null;
    TypeConverter                 ICustomTypeDescriptor.GetConverter()             => new TypeConverter();
    EventDescriptor?              ICustomTypeDescriptor.GetDefaultEvent()          => null;
    PropertyDescriptor?           ICustomTypeDescriptor.GetDefaultProperty()      => null;
    object?                       ICustomTypeDescriptor.GetEditor(Type t)          => null;
    EventDescriptorCollection     ICustomTypeDescriptor.GetEvents()                => EventDescriptorCollection.Empty;
    EventDescriptorCollection     ICustomTypeDescriptor.GetEvents(Attribute[]? a)  => EventDescriptorCollection.Empty;
    object                        ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor? pd) => this;

    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()              => BuildDescriptors();
    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[]? a) => BuildDescriptors();

    private PropertyDescriptorCollection BuildDescriptors()
    {
        var descs = Row.Table.Columns
            .Select(col => (PropertyDescriptor)new DataColumnPropertyDescriptor(col))
            .ToArray();
        return new PropertyDescriptorCollection(descs);
    }
}

// ??????????????????????????????????????????????????????????????
//  DataColumnPropertyDescriptor
// ??????????????????????????????????????????????????????????????

internal sealed class DataColumnPropertyDescriptor : PropertyDescriptor
{
    private readonly DataColumn _col;
    public DataColumnPropertyDescriptor(DataColumn col) : base(col.ColumnName, []) { _col = col; }

    public override Type  ComponentType               => typeof(DataRowView);
    public override bool  IsReadOnly                  => _col.ReadOnly;
    public override Type  PropertyType                => _col.DataType;
    public override bool  CanResetValue(object c)     => false;
    public override void  ResetValue(object c)        { }
    public override bool  ShouldSerializeValue(object c) => false;

    public override object? GetValue(object? component)
        => component is DataRowView drv ? drv[_col.ColumnName] : null;

    public override void SetValue(object? component, object? value)
    { if (component is DataRowView drv) drv[_col.ColumnName] = value; }
}

// ??????????????????????????????????????????????????????????????
//  Row/Column change events
// ??????????????????????????????????????????????????????????????

public class DataRowChangeEventArgs : EventArgs
{
    public DataRowChangeEventArgs(DataRow row, DataRowAction action) { Row = row; Action = action; }
    public DataRow       Row    { get; }
    public DataRowAction Action { get; }
}

public class DataColumnChangeEventArgs : EventArgs
{
    public DataColumnChangeEventArgs(DataRow row, DataColumn column, object? proposedValue)
    { Row = row; Column = column; ProposedValue = proposedValue; }
    public DataRow    Row           { get; }
    public DataColumn Column        { get; }
    public object?    ProposedValue { get; set; }
}

public enum DataRowAction { Nothing = 0, Delete = 1, Change = 2, Rollback = 4, Commit = 8, Add = 16 }

public delegate void DataRowChangeEventHandler(object sender, DataRowChangeEventArgs e);
public delegate void DataColumnChangeEventHandler(object sender, DataColumnChangeEventArgs e);

// ??????????????????????????????????????????????????????????????
//  DataView
// ??????????????????????????????????????????????????????????????

/// <summary>
/// Provides a sorted, filtered, bindable view over a <see cref="DataTable"/>.
/// Implements <see cref="IBindingList"/> so controls can bind to it directly.
/// Supports simple filter expressions: =, &lt;&gt;, !=, &gt;, &lt;, &gt;=, &lt;=, LIKE, IS NULL, IS NOT NULL, AND, OR.
/// </summary>
public class DataView : IBindingList, IList, IDisposable
{
    private DataTable _table;
    private string    _rowFilter    = string.Empty;
    private string    _sort         = string.Empty;
    private List<DataRowView> _filtered = new();
    private bool _allowNew    = true;
    private bool _allowEdit   = true;
    private bool _allowDelete = true;

    public DataView() : this(new DataTable()) { }

    public DataView(DataTable table)
    {
        _table = table;
        _table.RowChanged    += (_, _) => Refresh();
        _table.RowDeleted    += (_, _) => Refresh();
        _table.TableCleared  += (_, _) => Refresh();
        _table.ColumnsChanged += (_, _) => Refresh();
        Refresh();
    }

    // ?? Configuration ?????????????????????????????????????????????????????????

    public DataTable Table
    {
        get => _table;
        set { _table = value ?? throw new ArgumentNullException(nameof(value)); Refresh(); }
    }

    public string RowFilter
    {
        get => _rowFilter;
        set { _rowFilter = value ?? string.Empty; Refresh(); }
    }

    public string Sort
    {
        get => _sort;
        set { _sort = value ?? string.Empty; Refresh(); }
    }

    public bool AllowNew    { get => _allowNew;    set => _allowNew    = value; }
    public bool AllowEdit   { get => _allowEdit;   set => _allowEdit   = value; }
    public bool AllowDelete { get => _allowDelete; set => _allowDelete = value; }

    // ?? View rows ?????????????????????????????????????????????????????????????

    public int         Count          => _filtered.Count;
    public DataRowView this[int index] => _filtered[index];

    // ?? IBindingList ??????????????????????????????????????????????????????????

    public event ListChangedEventHandler? ListChanged;

    bool IBindingList.AllowNew               => _allowNew;
    bool IBindingList.AllowEdit              => _allowEdit;
    bool IBindingList.AllowRemove            => _allowDelete;
    bool IBindingList.SupportsChangeNotification => true;
    bool IBindingList.SupportsSearching      => true;
    bool IBindingList.SupportsSorting        => true;
    bool IBindingList.IsSorted               => !string.IsNullOrEmpty(_sort);
    ListSortDirection IBindingList.SortDirection  => ListSortDirection.Ascending;
    PropertyDescriptor? IBindingList.SortProperty => null;

    object? IBindingList.AddNew()
    {
        if (!_allowNew) throw new InvalidOperationException("AllowNew is false.");
        var row = _table.NewRow();
        _table.Rows.Add(row);
        return _filtered.LastOrDefault();
    }

    void IBindingList.AddIndex(PropertyDescriptor property)    { }
    void IBindingList.RemoveIndex(PropertyDescriptor property) { }
    void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) { }
    void IBindingList.RemoveSort() { _sort = string.Empty; Refresh(); }

    int IBindingList.Find(PropertyDescriptor property, object key)
    {
        for (int i = 0; i < _filtered.Count; i++)
        {
            var val = _filtered[i][property.Name];
            if (val?.ToString() == key?.ToString()) return i;
        }
        return -1;
    }

    // ?? IList ?????????????????????????????????????????????????????????????????

    bool IList.IsFixedSize              => false;
    bool IList.IsReadOnly               => false;
    bool ICollection.IsSynchronized     => false;
    object ICollection.SyncRoot         => this;

    object? IList.this[int index]
    {
        get => _filtered[index];
        set => throw new NotSupportedException();
    }

    int  IList.Add(object? value)        => throw new NotSupportedException("Use Table.Rows.Add.");
    void IList.Insert(int i, object? v)  => throw new NotSupportedException();
    void IList.Remove(object? value)     { if (value is DataRowView drv) drv.Delete(); }
    void IList.RemoveAt(int index)       { _filtered[index].Delete(); }
    void IList.Clear()                   { _table.Rows.Clear(); }
    bool IList.Contains(object? value)   => value is DataRowView drv && _filtered.Contains(drv);
    int  IList.IndexOf(object? value)    => value is DataRowView drv ? _filtered.IndexOf(drv) : -1;

    void ICollection.CopyTo(Array array, int index) => ((ICollection)_filtered).CopyTo(array, index);
    IEnumerator IEnumerable.GetEnumerator()         => _filtered.GetEnumerator();

    // ?? Refresh / filter / sort ???????????????????????????????????????????????

    public void Refresh()
    {
        IEnumerable<DataRow> rows = _table.Rows;

        if (!string.IsNullOrWhiteSpace(_rowFilter))
            rows = rows.Where(r => MatchesFilter(r, _rowFilter));

        if (!string.IsNullOrWhiteSpace(_sort))
            rows = ApplySort(rows, _sort);

        _filtered = rows.Select(r => new DataRowView(this, r)).ToList();
        ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    // ?? Filter evaluator ??????????????????????????????????????????????????????

    private bool MatchesFilter(DataRow row, string filter)
    {
        try   { return EvaluateExpression(row, filter.Trim()); }
        catch { return true; }
    }

    private bool EvaluateExpression(DataRow row, string expr)
    {
        var orParts = SplitOutsideParens(expr, " OR ");
        if (orParts.Length > 1)
            return orParts.Any(p => EvaluateExpression(row, p.Trim()));

        var andParts = SplitOutsideParens(expr, " AND ");
        if (andParts.Length > 1)
            return andParts.All(p => EvaluateExpression(row, p.Trim()));

        if (expr.StartsWith("(") && expr.EndsWith(")"))
            return EvaluateExpression(row, expr[1..^1].Trim());

        return EvaluateComparison(row, expr);
    }

    private bool EvaluateComparison(DataRow row, string expr)
    {
        // LIKE
        var likeM = Regex.Match(expr, @"^(\w+)\s+LIKE\s+'([^']*)'$", RegexOptions.IgnoreCase);
        if (likeM.Success)
        {
            var val = row[likeM.Groups[1].Value]?.ToString() ?? string.Empty;
            var pat = Regex.Escape(likeM.Groups[2].Value).Replace(@"\%", ".*").Replace(@"\_", ".");
            return Regex.IsMatch(val, "^" + pat + "$", RegexOptions.IgnoreCase);
        }

        // IS NULL / IS NOT NULL
        var nullM = Regex.Match(expr, @"^(\w+)\s+IS\s+(NOT\s+)?NULL$", RegexOptions.IgnoreCase);
        if (nullM.Success)
        {
            bool notNull = nullM.Groups[2].Success;
            bool isNull  = row.IsNull(nullM.Groups[1].Value);
            return notNull ? !isNull : isNull;
        }

        // Comparison operators
        var cmpM = Regex.Match(expr, @"^(\w+)\s*(=|<>|!=|>=|<=|>|<)\s*'?([^']*?)'?$");
        if (!cmpM.Success) return true;

        var col = cmpM.Groups[1].Value;
        var op  = cmpM.Groups[2].Value;
        var rhs = cmpM.Groups[3].Value.Trim('\'');

        if (!row.Table.Columns.Contains(col)) return true;
        var lhsVal = row[col];
        if (lhsVal == null || lhsVal == DBNull.Value)
            return op == "<>" || op == "!=";

        var lhsStr = lhsVal.ToString() ?? string.Empty;

        if (double.TryParse(lhsStr, out double lhsNum) && double.TryParse(rhs, out double rhsNum))
            return op switch { "=" => lhsNum == rhsNum, "<>" or "!=" => lhsNum != rhsNum,
                ">" => lhsNum > rhsNum, "<" => lhsNum < rhsNum,
                ">=" => lhsNum >= rhsNum, "<=" => lhsNum <= rhsNum, _ => true };

        int cmp = string.Compare(lhsStr, rhs, StringComparison.OrdinalIgnoreCase);
        return op switch { "=" => cmp == 0, "<>" or "!=" => cmp != 0,
            ">" => cmp > 0, "<" => cmp < 0, ">=" => cmp >= 0, "<=" => cmp <= 0, _ => true };
    }

    private static string[] SplitOutsideParens(string expr, string delimiter)
    {
        var parts = new List<string>();
        int depth = 0, start = 0, dlen = delimiter.Length;
        for (int i = 0; i <= expr.Length - dlen; i++)
        {
            if      (expr[i] == '(') depth++;
            else if (expr[i] == ')') depth--;
            else if (depth == 0 && string.Compare(expr, i, delimiter, 0, dlen,
                     StringComparison.OrdinalIgnoreCase) == 0)
            {
                parts.Add(expr[start..i]);
                start = i + dlen;
                i    += dlen - 1;
            }
        }
        parts.Add(expr[start..]);
        return parts.ToArray();
    }

    // ?? Sort ??????????????????????????????????????????????????????????????????

    private static IEnumerable<DataRow> ApplySort(IEnumerable<DataRow> rows, string sort)
    {
        var clauses = sort.Split(',');
        IOrderedEnumerable<DataRow>? ordered = null;
        foreach (var clause in clauses)
        {
            var parts = clause.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var col   = parts[0];
            var desc  = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
            ordered   = ordered == null
                ? (desc ? rows.OrderByDescending(r => r[col]) : rows.OrderBy(r => r[col]))
                : (desc ? ordered.ThenByDescending(r => r[col]) : ordered.ThenBy(r => r[col]));
        }
        return ordered ?? rows;
    }

    public void Dispose() { }
}

public enum DataViewRowState
{
    None = 0, Unchanged = 2, Added = 4, Deleted = 8,
    ModifiedCurrent = 16, ModifiedOriginal = 32,
    CurrentRows = 22, OriginalRows = 42
}

// ??????????????????????????????????????????????????????????????
//  DataTable
// ??????????????????????????????????????????????????????????????

/// <summary>
/// Lightweight in-memory table used as a DataGridView.DataSource.
/// Raises typed row/column change events and exposes
/// <see cref="DefaultView"/>, <see cref="Select(string)"/>, and
/// <see cref="IListSource"/> so controls can bind directly.
/// </summary>
public class DataTable : IDisposable, IListSource
{
    private readonly DataColumnCollection _columns;
    private readonly DataRowCollection    _rows;
    private DataView? _defaultView;

    public DataTable()
    {
        _columns = new DataColumnCollection(this);
        _rows    = new DataRowCollection(this);
    }

    public DataTable(string tableName) : this() { TableName = tableName; }

    public string   TableName       { get; set; } = string.Empty;
    public string   Namespace       { get; set; } = string.Empty;
    public bool     CaseSensitive   { get; set; } = false;
    public int      MinimumCapacity { get; set; } = 50;
    public DataSet? DataSet         { get; internal set; }

    public DataColumnCollection Columns => _columns;
    public DataRowCollection    Rows    => _rows;

    // ?? DefaultView ???????????????????????????????????????????????????????????

    /// <summary>
    /// Returns the default <see cref="DataView"/> for this table.
    /// Suitable for direct binding: <c>grid.DataSource = table.DefaultView;</c>
    /// </summary>
    public DataView DefaultView => _defaultView ??= new DataView(this);

    // ?? Events ????????????????????????????????????????????????????????????????

    public event DataRowChangeEventHandler?    RowChanging;
    public event DataRowChangeEventHandler?    RowChanged;
    public event DataRowChangeEventHandler?    RowDeleting;
    public event DataRowChangeEventHandler?    RowDeleted;
    public event DataColumnChangeEventHandler? ColumnChanged;
    public event EventHandler?                 TableCleared;
    public event EventHandler?                 TableNewRow;
    public event EventHandler?                 ColumnsChanged;

    // ?? Row factory ???????????????????????????????????????????????????????????

    public DataRow NewRow()
    {
        var row = new DataRow(this);
        TableNewRow?.Invoke(this, EventArgs.Empty);
        return row;
    }

    // ?? Select ????????????????????????????????????????????????????????????????

    /// <summary>Returns all rows matching the filter expression.</summary>
    public DataRow[] Select(string filterExpression)
    {
        var view = new DataView(this) { RowFilter = filterExpression };
        return view.Cast<DataRowView>().Select(v => v.DataRow).ToArray();
    }

    /// <summary>Returns rows matching the filter ordered by <paramref name="sort"/>.</summary>
    public DataRow[] Select(string filterExpression, string sort)
    {
        var view = new DataView(this) { RowFilter = filterExpression, Sort = sort };
        return view.Cast<DataRowView>().Select(v => v.DataRow).ToArray();
    }

    /// <summary>Returns rows matching filter + sort, filtered to the given row state.</summary>
    public DataRow[] Select(string filterExpression, string sort, DataViewRowState recordStates)
        => Select(filterExpression, sort);

    /// <summary>Returns all rows (no filter).</summary>
    public DataRow[] Select() => _rows.ToArray();

    // ?? Mutations ?????????????????????????????????????????????????????????????

    public void Clear()         { _rows.Clear(); }
    public void AcceptChanges() { foreach (var row in _rows) row.RowState = DataRowState.Unchanged; }
    public void RejectChanges() { /* stub – no row versioning in this layer */ }

    // ?? Cloning ???????????????????????????????????????????????????????????????

    /// <summary>Creates a copy with the same schema but no rows.</summary>
    public DataTable Clone()
    {
        var dt = new DataTable(TableName) { Namespace = Namespace, CaseSensitive = CaseSensitive };
        foreach (var col in _columns)
            dt.Columns.Add(col.ColumnName, col.DataType);
        return dt;
    }

    /// <summary>Creates a full copy including all rows.</summary>
    public DataTable Copy()
    {
        var dt = Clone();
        foreach (var row in _rows)
            dt.Rows.Add(row.ItemArray);
        dt.AcceptChanges();
        return dt;
    }

    // ?? IListSource ???????????????????????????????????????????????????????????

    bool  IListSource.ContainsListCollection => false;
    IList IListSource.GetList()              => DefaultView;

    // ?? Internal event helpers ????????????????????????????????????????????????

    internal void OnRowAdded(DataRow row)
    {
        var args = new DataRowChangeEventArgs(row, DataRowAction.Add);
        RowChanging?.Invoke(this, args);
        RowChanged?.Invoke(this, args);
        _defaultView?.Refresh();
    }

    internal void OnRowRemoved(DataRow row, int index)
    {
        var args = new DataRowChangeEventArgs(row, DataRowAction.Delete);
        RowDeleting?.Invoke(this, args);
        RowDeleted?.Invoke(this, new DataRowChangeEventArgs(row, DataRowAction.Delete));
        _defaultView?.Refresh();
    }

    internal void OnReset()
    {
        TableCleared?.Invoke(this, EventArgs.Empty);
        _defaultView?.Refresh();
    }

    internal void OnColumnsChanged()
    {
        ColumnsChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void OnCellChanged(DataRow row, int colIndex, object? oldValue, object? newValue)
    {
        if (colIndex >= 0 && colIndex < _columns.Count)
        {
            var args = new DataColumnChangeEventArgs(row, _columns[colIndex], newValue);
            ColumnChanged?.Invoke(this, args);
        }
        RowChanged?.Invoke(this, new DataRowChangeEventArgs(row, DataRowAction.Change));
        _defaultView?.Refresh();
    }

    public void Dispose() { _defaultView?.Dispose(); }
}

// ??????????????????????????????????????????????????????????????
//  DataSet
// ??????????????????????????????????????????????????????????????

/// <summary>
/// In-process collection of <see cref="DataTable"/> objects.
/// Provides the same surface that typed DataSet classes generated by the
/// WinForms designer extend.
/// </summary>
public class DataSet : IDisposable
{
    private readonly DataTableCollection    _tables;
    private readonly DataRelationCollection _relations;

    public DataSet()
    {
        _tables    = new DataTableCollection(this);
        _relations = new DataRelationCollection();
    }

    public DataSet(string dataSetName) : this() { DataSetName = dataSetName; }

    public string DataSetName   { get; set; } = string.Empty;
    public string Namespace     { get; set; } = string.Empty;
    public bool   CaseSensitive { get; set; } = false;

    public DataTableCollection    Tables    => _tables;
    public DataRelationCollection Relations => _relations;

    public void Clear()         { foreach (var t in _tables) t.Clear(); }
    public void AcceptChanges() { foreach (var t in _tables) t.AcceptChanges(); }
    public void RejectChanges() { foreach (var t in _tables) t.RejectChanges(); }

    public DataSet Clone()
    {
        var ds = new DataSet(DataSetName) { Namespace = Namespace };
        foreach (var t in _tables) ds.Tables.Add(t.Clone());
        return ds;
    }

    public void Dispose() { foreach (var t in _tables) t.Dispose(); }
}

// ??????????????????????????????????????????????????????????????
//  DataTableCollection
// ??????????????????????????????????????????????????????????????

public class DataTableCollection : IEnumerable<DataTable>
{
    private readonly List<DataTable> _list  = new();
    private readonly DataSet         _owner;

    internal DataTableCollection(DataSet owner) => _owner = owner;

    public int       Count           => _list.Count;
    public DataTable this[int index]  => _list[index];
    public DataTable? this[string name] =>
        _list.FirstOrDefault(t => string.Equals(t.TableName, name, StringComparison.OrdinalIgnoreCase));

    public DataTable Add(string tableName)
    {
        var t = new DataTable(tableName) { DataSet = _owner };
        _list.Add(t);
        return t;
    }

    public void Add(DataTable table)    { table.DataSet = _owner; _list.Add(table); }
    public void Remove(DataTable table) { _list.Remove(table); table.DataSet = null; }
    public void RemoveAt(int index)     { var t = _list[index]; _list.RemoveAt(index); t.DataSet = null; }
    public bool Contains(string name)   => this[name] != null;

    public IEnumerator<DataTable> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}

// ??????????????????????????????????????????????????????????????
//  DataRelation / DataRelationCollection
// ??????????????????????????????????????????????????????????????

/// <summary>Describes a parent-child relationship between two tables.</summary>
public class DataRelation
{
    public DataRelation(string relationName, DataColumn parentColumn, DataColumn childColumn)
    { RelationName = relationName; ParentColumns = [parentColumn]; ChildColumns = [childColumn]; }

    public DataRelation(string relationName, DataColumn[] parentColumns, DataColumn[] childColumns)
    { RelationName = relationName; ParentColumns = parentColumns; ChildColumns = childColumns; }

    public string       RelationName  { get; set; }
    public DataColumn[] ParentColumns { get; }
    public DataColumn[] ChildColumns  { get; }
    public DataTable?   ParentTable   => ParentColumns.FirstOrDefault()?.Table;
    public DataTable?   ChildTable    => ChildColumns.FirstOrDefault()?.Table;
}

public class DataRelationCollection : IEnumerable<DataRelation>
{
    private readonly List<DataRelation> _list = new();

    public int           Count            => _list.Count;
    public DataRelation  this[int index]  => _list[index];
    public DataRelation? this[string name] =>
        _list.FirstOrDefault(r => string.Equals(r.RelationName, name, StringComparison.OrdinalIgnoreCase));

    public void Add(DataRelation relation) => _list.Add(relation);
    public DataRelation Add(string name, DataColumn parent, DataColumn child)
    {
        var rel = new DataRelation(name, parent, child);
        _list.Add(rel);
        return rel;
    }
    public void Remove(DataRelation relation) => _list.Remove(relation);
    public bool Contains(string name)         => this[name] != null;

    public IEnumerator<DataRelation> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}

// ??????????????????????????????????????????????????????????????
//  DBNull
// ??????????????????????????????????????????????????????????????

/// <summary>Stub DBNull for null-check compatibility.</summary>
public sealed class DBNull
{
    public static readonly DBNull Value = new DBNull();
    private DBNull() { }
    public override string ToString() => string.Empty;
}
