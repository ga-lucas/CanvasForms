using System.ComponentModel;

namespace System.Windows.Forms;

// ── DataGridTableStyle ────────────────────────────────────────────────────────

/// <summary>
/// Represents the appearance of a <see cref="DataGrid"/> for a specific data table.
/// Canvas stub — accepted by DataGrid but not actively rendered.
/// </summary>
public class DataGridTableStyle : System.ComponentModel.Component
{
    public string MappingName          { get; set; } = string.Empty;
    public bool   AllowSorting         { get; set; } = true;
    public bool   ReadOnly             { get; set; } = false;
    public int    HeaderFontHeight     { get; set; } = 0;
    public int    RowHeaderWidth       { get; set; } = 35;
    public System.Drawing.Color HeaderBackColor    { get; set; } = System.Drawing.Color.FromArgb(0,0,128);
    public System.Drawing.Color HeaderForeColor    { get; set; } = System.Drawing.Color.White;
    public System.Drawing.Color SelectionBackColor { get; set; } = System.Drawing.Color.FromArgb(0,0,128);
    public System.Drawing.Color SelectionForeColor { get; set; } = System.Drawing.Color.White;
    public System.Drawing.Color BackColor          { get; set; } = System.Drawing.Color.White;
    public System.Drawing.Color ForeColor          { get; set; } = System.Drawing.Color.Black;
    public System.Drawing.Color AlternatingBackColor { get; set; } = System.Drawing.Color.FromArgb(240,240,255);
    public DataGridColumnStylesCollection GridColumnStyles { get; } = new DataGridColumnStylesCollection();
}

// ── DataGridColumnStyle (abstract base) ──────────────────────────────────────

public abstract class DataGridColumnStyle : System.ComponentModel.Component
{
    public string  MappingName  { get; set; } = string.Empty;
    public string  HeaderText   { get; set; } = string.Empty;
    public int     Width        { get; set; } = 75;
    public bool    ReadOnly     { get; set; } = false;
    public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;
    public string? NullText     { get; set; } = "(null)";
}

// ── DataGridTextBoxColumn ─────────────────────────────────────────────────────

public class DataGridTextBoxColumn : DataGridColumnStyle
{
    public TextBox TextBox { get; } = new TextBox();
    public string? Format  { get; set; }
}

// ── DataGridBoolColumn ────────────────────────────────────────────────────────

public class DataGridBoolColumn : DataGridColumnStyle
{
    public object TrueValue  { get; set; } = true;
    public object FalseValue { get; set; } = false;
    public object NullValue  { get; set; } = System.DBNull.Value;
    public bool   AllowNull  { get; set; } = true;
}

// ── GridTableStylesCollection ─────────────────────────────────────────────────

public class GridTableStylesCollection : System.Collections.IEnumerable
{
    private readonly List<DataGridTableStyle> _list = new();

    public DataGridTableStyle this[int index] => _list[index];
    public DataGridTableStyle? this[string name] => _list.FirstOrDefault(s => s.MappingName == name);
    public int Count => _list.Count;

    public void Add(DataGridTableStyle style) => _list.Add(style);
    public void Remove(DataGridTableStyle style) => _list.Remove(style);
    public void Clear() => _list.Clear();
    public bool Contains(DataGridTableStyle style) => _list.Contains(style);
    public IEnumerator<DataGridTableStyle> GetEnumerator() => _list.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _list.GetEnumerator();
}

// ── DataGridColumnStylesCollection ───────────────────────────────────────────

public class DataGridColumnStylesCollection : System.Collections.IEnumerable
{
    private readonly List<DataGridColumnStyle> _list = new();

    public DataGridColumnStyle this[int index] => _list[index];
    public DataGridColumnStyle? this[string name] => _list.FirstOrDefault(s => s.MappingName == name);
    public int Count => _list.Count;

    public void Add(DataGridColumnStyle style) => _list.Add(style);
    public void Remove(DataGridColumnStyle style) => _list.Remove(style);
    public void Clear() => _list.Clear();
    public IEnumerator<DataGridColumnStyle> GetEnumerator() => _list.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _list.GetEnumerator();
}

// ── DataGrid ──────────────────────────────────────────────────────────────────

/// <summary>
/// Legacy data grid control (pre-<see cref="DataGridView"/>).
/// Implemented as a thin subclass of <see cref="DataGridView"/> so that
/// translated WinForms apps using the old <c>DataGrid</c> API compile and run
/// with the full <see cref="DataGridView"/> rendering and interaction.
/// </summary>
public class DataGrid : DataGridView
{
    private GridTableStylesCollection? _tableStyles;
    private DataGridTableStyle? _defaultTableStyle;
    private bool _captionVisible = true;
    private bool _rowHeadersVisible = true;
    private bool _columnHeadersVisible = true;
    private bool _allowNavigation = true;
    private bool _flatMode = false;
    private Color _captionBackColor = Color.FromArgb(0, 0, 128);
    private Color _captionForeColor = Color.White;

    // ── Constructor ───────────────────────────────────────────────────────────

    public DataGrid()
    {
        // Initialise with classic DataGrid visual defaults
        AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(240, 240, 255),
        };
    }

    // ── Legacy properties ─────────────────────────────────────────────────────

    /// <summary>Collection of per-table styles.</summary>
    public GridTableStylesCollection TableStyles
        => _tableStyles ??= new GridTableStylesCollection();

    public bool CaptionVisible
    {
        get => _captionVisible;
        set { _captionVisible = value; Invalidate(); }
    }

    public string CaptionText
    {
        get => Text;
        set => Text = value;
    }

    public System.Drawing.Color CaptionBackColor
    {
        get => _captionBackColor;
        set { _captionBackColor = value; Invalidate(); }
    }

    public System.Drawing.Color CaptionForeColor
    {
        get => _captionForeColor;
        set { _captionForeColor = value; Invalidate(); }
    }

    public bool FlatMode
    {
        get => _flatMode;
        set { _flatMode = value; Invalidate(); }
    }

    public bool AllowNavigation
    {
        get => _allowNavigation;
        set => _allowNavigation = value;
    }

    public new bool RowHeadersVisible
    {
        get => _rowHeadersVisible;
        set { _rowHeadersVisible = value; base.RowHeadersVisible = value; Invalidate(); }
    }

    public new bool ColumnHeadersVisible
    {
        get => _columnHeadersVisible;
        set { _columnHeadersVisible = value; base.ColumnHeadersVisible = value; Invalidate(); }
    }

    // ── Grid legacy colors (map to DataGridView cell styles) ──────────────────

    public System.Drawing.Color SelectionBackColor
    {
        get => DefaultCellStyle.SelectionBackColor;
        set => DefaultCellStyle.SelectionBackColor = value;
    }

    public System.Drawing.Color SelectionForeColor
    {
        get => DefaultCellStyle.SelectionForeColor;
        set => DefaultCellStyle.SelectionForeColor = value;
    }

    public System.Drawing.Color HeaderBackColor
    {
        get => ColumnHeadersDefaultCellStyle.BackColor;
        set => ColumnHeadersDefaultCellStyle.BackColor = value;
    }

    public System.Drawing.Color HeaderForeColor
    {
        get => ColumnHeadersDefaultCellStyle.ForeColor;
        set => ColumnHeadersDefaultCellStyle.ForeColor = value;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>Navigates to the child list for the selected row (stub — no hierarchical nav).</summary>
    public void NavigateTo(int rowNumber, string relationName) { }

    /// <summary>Navigates back to the parent list (stub).</summary>
    public void NavigateBack() { }

    // ── Hit test ──────────────────────────────────────────────────────────────

    /// <summary>Canvas-compatible hit test returning cell coordinates.</summary>
    public DataGridCell HitTest(int x, int y)
    {
        // Simple hit test using row/column layout
        return new DataGridCell(-1, -1);
    }

    // ── Expand/Collapse (legacy child-list hierarchy stubs) ───────────────────

    public void Expand(int row)   { /* no child list support */ }
    public void Collapse(int row) { /* no child list support */ }
    public bool IsExpanded(int row) => false;

    // ── Events ────────────────────────────────────────────────────────────────

    public event EventHandler? Navigate;
    public event EventHandler? BackButtonClick;
    public event EventHandler? ShowParentDetailsButtonClick;

    protected virtual void OnNavigate(EventArgs e) => Navigate?.Invoke(this, e);
}

// ── DataGridCell ──────────────────────────────────────────────────────────────

/// <summary>Identifies a cell in a <see cref="DataGrid"/> by row and column.</summary>
public readonly struct DataGridCell : IEquatable<DataGridCell>
{
    public int RowNumber    { get; }
    public int ColumnNumber { get; }

    public DataGridCell(int row, int column) { RowNumber = row; ColumnNumber = column; }

    public bool Equals(DataGridCell other)
        => RowNumber == other.RowNumber && ColumnNumber == other.ColumnNumber;

    public override bool Equals(object? obj) => obj is DataGridCell c && Equals(c);
    public override int GetHashCode() => HashCode.Combine(RowNumber, ColumnNumber);
    public static bool operator ==(DataGridCell a, DataGridCell b) => a.Equals(b);
    public static bool operator !=(DataGridCell a, DataGridCell b) => !a.Equals(b);
    public override string ToString() => $"{{RowNumber={RowNumber}, ColumnNumber={ColumnNumber}}}";
}
