using System.Collections;

namespace System.Windows.Forms;

// ──────────────────────────────────────────────────────────────
//  DataColumn
// ──────────────────────────────────────────────────────────────

/// <summary>
/// Represents the schema of one column in a DataTable.
/// </summary>
public class DataColumn
{
    public DataColumn() { }
    public DataColumn(string columnName) { ColumnName = columnName; }
    public DataColumn(string columnName, Type dataType) { ColumnName = columnName; DataType = dataType; }

    public string ColumnName { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public Type DataType { get; set; } = typeof(string);
    public object? DefaultValue { get; set; }
    public bool AllowDBNull { get; set; } = true;
    public bool ReadOnly { get; set; } = false;
    public bool Unique { get; set; } = false;
    public int MaxLength { get; set; } = -1;
    public int Ordinal { get; internal set; } = -1;
    public string Expression { get; set; } = string.Empty;

    internal DataTable? Table { get; set; }
}

// ──────────────────────────────────────────────────────────────
//  DataColumnCollection
// ──────────────────────────────────────────────────────────────

public class DataColumnCollection : IEnumerable<DataColumn>
{
    private readonly List<DataColumn> _list = new();
    private readonly DataTable _owner;

    internal DataColumnCollection(DataTable owner) => _owner = owner;

    public int Count => _list.Count;
    public DataColumn this[int index] => _list[index];
    public DataColumn? this[string name] =>
        _list.FirstOrDefault(c => string.Equals(c.ColumnName, name, StringComparison.OrdinalIgnoreCase));

    public DataColumn Add(string columnName)
    {
        var col = new DataColumn(columnName) { Ordinal = _list.Count, Table = _owner };
        _list.Add(col);
        return col;
    }

    public DataColumn Add(string columnName, Type dataType)
    {
        var col = new DataColumn(columnName, dataType) { Ordinal = _list.Count, Table = _owner };
        _list.Add(col);
        return col;
    }

    public void Add(DataColumn column)
    {
        column.Ordinal = _list.Count;
        column.Table = _owner;
        _list.Add(column);
    }

    public void Remove(DataColumn column) { _list.Remove(column); RenumberOrdinals(); }
    public void RemoveAt(int index) { _list.RemoveAt(index); RenumberOrdinals(); }
    public bool Contains(string name) => this[name] != null;

    private void RenumberOrdinals() { for (int i = 0; i < _list.Count; i++) _list[i].Ordinal = i; }

    public IEnumerator<DataColumn> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}

// ──────────────────────────────────────────────────────────────
//  DataRow
// ──────────────────────────────────────────────────────────────

/// <summary>
/// Represents one row of data in a DataTable.
/// </summary>
public class DataRow
{
    private readonly object?[] _values;
    private readonly DataTable _table;

    internal DataRow(DataTable table)
    {
        _table = table;
        _values = new object?[table.Columns.Count];
        for (int i = 0; i < _values.Length; i++)
            _values[i] = table.Columns[i].DefaultValue;
    }

    public object? this[int index]
    {
        get => index >= 0 && index < _values.Length ? _values[index] : null;
        set { if (index >= 0 && index < _values.Length) _values[index] = value; }
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
            _values[col.Ordinal] = value;
        }
    }

    public object? this[DataColumn column]
    {
        get => _values[column.Ordinal];
        set => _values[column.Ordinal] = value;
    }

    public DataRowState RowState { get; internal set; } = DataRowState.Added;
    public DataTable Table => _table;

    public bool IsNull(int index) => _values[index] == null || _values[index] == DBNull.Value;
    public bool IsNull(string columnName) => IsNull(_table.Columns[columnName]!.Ordinal);
    public bool IsNull(DataColumn column) => IsNull(column.Ordinal);

    public object?[] ItemArray
    {
        get => (object?[])_values.Clone();
        set
        {
            int len = Math.Min(value.Length, _values.Length);
            for (int i = 0; i < len; i++) _values[i] = value[i];
        }
    }
}

public enum DataRowState { Detached = 1, Unchanged = 2, Added = 4, Deleted = 8, Modified = 16 }

// ──────────────────────────────────────────────────────────────
//  DataRowCollection
// ──────────────────────────────────────────────────────────────

public class DataRowCollection : IEnumerable<DataRow>
{
    private readonly List<DataRow> _list = new();
    private readonly DataTable _owner;

    internal DataRowCollection(DataTable owner) => _owner = owner;

    public int Count => _list.Count;
    public DataRow this[int index] => _list[index];

    public void Add(DataRow row) { _list.Add(row); _owner.OnRowAdded(row); }
    public DataRow Add(params object?[] values)
    {
        var row = _owner.NewRow();
        row.ItemArray = values;
        Add(row);
        return row;
    }
    public void Remove(DataRow row) { _list.Remove(row); _owner.OnRowRemoved(row); }
    public void RemoveAt(int index) { var row = _list[index]; _list.RemoveAt(index); _owner.OnRowRemoved(row); }
    public void Clear() { _list.Clear(); _owner.OnReset(); }

    public IEnumerator<DataRow> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}

// ──────────────────────────────────────────────────────────────
//  DataTable
// ──────────────────────────────────────────────────────────────

/// <summary>
/// Lightweight in-memory table used as a DataGridView.DataSource.
/// Raises events compatible with WinForms data binding.
/// </summary>
public class DataTable : IDisposable
{
    private readonly DataColumnCollection _columns;
    private readonly DataRowCollection _rows;

    public DataTable()
    {
        _columns = new DataColumnCollection(this);
        _rows = new DataRowCollection(this);
    }

    public DataTable(string tableName) : this() { TableName = tableName; }

    public string TableName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public bool CaseSensitive { get; set; } = false;
    public int MinimumCapacity { get; set; } = 50;

    public DataColumnCollection Columns => _columns;
    public DataRowCollection Rows => _rows;

    public event EventHandler? TableNewRow;
    public event EventHandler? RowChanged;
    public event EventHandler? RowDeleted;
    public event EventHandler? TableCleared;

    public DataRow NewRow()
    {
        var row = new DataRow(this);
        TableNewRow?.Invoke(this, EventArgs.Empty);
        return row;
    }

    public void Clear() { _rows.Clear(); }

    public void AcceptChanges()
    {
        foreach (var row in _rows) row.RowState = DataRowState.Unchanged;
    }

    internal void OnRowAdded(DataRow row) => RowChanged?.Invoke(this, EventArgs.Empty);
    internal void OnRowRemoved(DataRow row) => RowDeleted?.Invoke(this, EventArgs.Empty);
    internal void OnReset() => TableCleared?.Invoke(this, EventArgs.Empty);

    public void Dispose() { }
}

// ──────────────────────────────────────────────────────────────
//  DBNull
// ──────────────────────────────────────────────────────────────

/// <summary>Stub DBNull for null-check compatibility.</summary>
public sealed class DBNull
{
    public static readonly DBNull Value = new DBNull();
    private DBNull() { }
    public override string ToString() => string.Empty;
}
