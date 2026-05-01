namespace System.Windows.Forms;

// ──────────────────────────────────────────────────────────────
//  Cell value / style
// ──────────────────────────────────────────────────────────────

/// <summary>
/// Represents the value and formatting of a single cell.
/// </summary>
public class DataGridViewCell
{
    public object? Value { get; set; }
    public string? ToolTipText { get; set; }
    public DataGridViewCellStyle? Style { get; set; }
    public string FormattedValue => Value?.ToString() ?? string.Empty;
}

/// <summary>
/// Per-cell / per-column / per-row style information.
/// </summary>
public class DataGridViewCellStyle
{
    public Color BackColor { get; set; } = Color.Empty;
    public Color ForeColor { get; set; } = Color.Empty;
    public Color SelectionBackColor { get; set; } = Color.Empty;
    public Color SelectionForeColor { get; set; } = Color.Empty;
    public Font? Font { get; set; }
    public DataGridViewContentAlignment Alignment { get; set; } = DataGridViewContentAlignment.MiddleLeft;
    public string? Format { get; set; }
    public string? NullValue { get; set; }
    public Padding Padding { get; set; } = Padding.Empty;
    public bool WrapMode { get; set; }

    public DataGridViewCellStyle Clone() => (DataGridViewCellStyle)MemberwiseClone();
}

// ──────────────────────────────────────────────────────────────
//  Column base
// ──────────────────────────────────────────────────────────────

/// <summary>
/// Base class for all DataGridView column types (WinForms-compatible API).
/// </summary>
public class DataGridViewColumn
{
    private int _width = 100;
    private int _minWidth = 5;
    private int _fillWeight = 100;

    public DataGridViewColumn() { }
    public DataGridViewColumn(string headerText) { HeaderText = headerText; }

    public string HeaderText { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataPropertyName { get; set; } = string.Empty;
    public string ToolTipText { get; set; } = string.Empty;

    public int Width
    {
        get => _width;
        set => _width = Math.Max(_minWidth, value);
    }

    public int MinimumWidth
    {
        get => _minWidth;
        set { _minWidth = Math.Max(2, value); if (_width < _minWidth) _width = _minWidth; }
    }

    public int FillWeight
    {
        get => _fillWeight;
        set => _fillWeight = Math.Max(1, value);
    }

    public int Index { get; internal set; } = -1;
    public int DisplayIndex { get; set; } = -1;

    public bool Visible { get; set; } = true;
    public bool ReadOnly { get; set; } = false;
    public bool Frozen { get; set; } = false;
    public bool Resizable { get; set; } = true;
    public bool SortMode_Automatic { get; set; } = true;

    public DataGridViewAutoSizeColumnMode AutoSizeMode { get; set; } = DataGridViewAutoSizeColumnMode.None;
    public DataGridViewColumnSortMode SortMode { get; set; } = DataGridViewColumnSortMode.Automatic;
    public HorizontalAlignment DefaultCellStyle_Alignment { get; set; } = HorizontalAlignment.Left;

    public DataGridViewCellStyle DefaultCellStyle { get; set; } = new DataGridViewCellStyle();
    public DataGridViewCellStyle HeaderCell_Style { get; set; } = new DataGridViewCellStyle();

    public object? Tag { get; set; }

    public DataGridView? DataGridView { get; internal set; }
}

// ──────────────────────────────────────────────────────────────
//  Concrete column types
// ──────────────────────────────────────────────────────────────

public class DataGridViewTextBoxColumn : DataGridViewColumn
{
    public int MaxInputLength { get; set; } = 32767;
    public DataGridViewTextBoxColumn() { }
    public DataGridViewTextBoxColumn(string headerText) : base(headerText) { }
}

public class DataGridViewCheckBoxColumn : DataGridViewColumn
{
    public bool ThreeState { get; set; } = false;
    public object? TrueValue { get; set; }
    public object? FalseValue { get; set; }
    public object? IndeterminateValue { get; set; }
    public DataGridViewCheckBoxColumn() { }
    public DataGridViewCheckBoxColumn(string headerText) : base(headerText) { }
}

public class DataGridViewButtonColumn : DataGridViewColumn
{
    public string? Text { get; set; }
    public bool UseColumnTextForButtonValue { get; set; } = false;
    public FlatStyle FlatStyle { get; set; } = FlatStyle.Standard;
    public DataGridViewButtonColumn() { }
    public DataGridViewButtonColumn(string headerText) : base(headerText) { }
}

public class DataGridViewComboBoxColumn : DataGridViewColumn
{
    public ObjectCollection Items { get; } = new ObjectCollection();
    public string DisplayMember { get; set; } = string.Empty;
    public string ValueMember { get; set; } = string.Empty;
    public object? DataSource { get; set; }
    public bool FlatStyle_Flat { get; set; }
    public DataGridViewComboBoxColumn() { }
    public DataGridViewComboBoxColumn(string headerText) : base(headerText) { }

    public class ObjectCollection : System.Collections.ObjectModel.Collection<object>
    {
        public void AddRange(object[] values) { foreach (var v in values) Add(v); }
    }
}

public class DataGridViewImageColumn : DataGridViewColumn
{
    public object? Image { get; set; } // System.Drawing.Image not available cross-platform; use image path string or icon key
    public string ImageLayout_Zoom { get; set; } = "Normal";
    public DataGridViewImageColumn() { }
    public DataGridViewImageColumn(string headerText) : base(headerText) { }
}

public class DataGridViewLinkColumn : DataGridViewColumn
{
    public string? Text { get; set; }
    public bool UseColumnTextForLinkValue { get; set; } = false;
    public LinkBehavior LinkBehavior { get; set; } = LinkBehavior.SystemDefault;
    public Color LinkColor { get; set; } = Color.Blue;
    public Color VisitedLinkColor { get; set; } = Color.Purple;
    public Color ActiveLinkColor { get; set; } = Color.Red;
    public DataGridViewLinkColumn() { }
    public DataGridViewLinkColumn(string headerText) : base(headerText) { }
}

// ──────────────────────────────────────────────────────────────
//  Column collection
// ──────────────────────────────────────────────────────────────

public class DataGridViewColumnCollection : System.Collections.ObjectModel.Collection<DataGridViewColumn>
{
    private readonly DataGridView _owner;
    internal DataGridViewColumnCollection(DataGridView owner) => _owner = owner;

    public DataGridViewColumn? this[string name] =>
        this.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

    protected override void InsertItem(int index, DataGridViewColumn item)
    {
        item.Index = index;
        item.DataGridView = _owner;
        if (item.DisplayIndex < 0) item.DisplayIndex = index;
        base.InsertItem(index, item);
        // Fix indices after insertion
        for (int i = index + 1; i < Count; i++) this[i].Index = i;
        _owner.Invalidate();
    }

    protected override void RemoveItem(int index)
    {
        this[index].DataGridView = null;
        base.RemoveItem(index);
        for (int i = index; i < Count; i++) this[i].Index = i;
        _owner.Invalidate();
    }

    public int Add(string columnName, string headerText)
    {
        var col = new DataGridViewTextBoxColumn { Name = columnName, HeaderText = headerText };
        Add(col);
        return col.Index;
    }
}

// ──────────────────────────────────────────────────────────────
//  Row / row header
// ──────────────────────────────────────────────────────────────

public class DataGridViewRow
{
    private readonly List<DataGridViewCell> _cells = new();

    public DataGridViewRow() { }

    public DataGridViewCellCollection Cells { get; } = new DataGridViewCellCollection();

    public DataGridViewCellStyle DefaultCellStyle { get; set; } = new DataGridViewCellStyle();
    public int Height { get; set; } = 23;
    public int Index { get; internal set; } = -1;
    public bool Visible { get; set; } = true;
    public bool Selected { get; set; } = false;
    public bool ReadOnly { get; set; } = false;
    public object? Tag { get; set; }
    public DataGridView? DataGridView { get; internal set; }

    public class DataGridViewCellCollection : System.Collections.ObjectModel.Collection<DataGridViewCell>
    {
        public DataGridViewCell? this[string columnName] =>
            this.FirstOrDefault(c => c.ToolTipText == columnName);
    }
}

public class DataGridViewRowCollection : System.Collections.ObjectModel.Collection<DataGridViewRow>
{
    private readonly DataGridView _owner;
    internal DataGridViewRowCollection(DataGridView owner) => _owner = owner;

    protected override void InsertItem(int index, DataGridViewRow item)
    {
        item.Index = index;
        item.DataGridView = _owner;
        base.InsertItem(index, item);
        for (int i = index + 1; i < Count; i++) this[i].Index = i;
        _owner.Invalidate();
    }

    protected override void RemoveItem(int index)
    {
        this[index].DataGridView = null;
        base.RemoveItem(index);
        for (int i = index; i < Count; i++) this[i].Index = i;
        _owner.Invalidate();
    }

    public int Add(DataGridViewRow row) { Add((object)row); return row.Index; }
    private void Add(object row) => Add((DataGridViewRow)row);
}

// ──────────────────────────────────────────────────────────────
//  Enums
// ──────────────────────────────────────────────────────────────

public enum DataGridViewContentAlignment
{
    NotSet = 0,
    TopLeft = 1, TopCenter = 2, TopRight = 4,
    MiddleLeft = 16, MiddleCenter = 32, MiddleRight = 64,
    BottomLeft = 256, BottomCenter = 512, BottomRight = 1024,
}

public enum DataGridViewAutoSizeColumnMode
{
    NotSet = 0,
    None = 1,
    ColumnHeader = 2,
    AllCellsExceptHeader = 4,
    AllCells = 6,
    DisplayedCellsExceptHeader = 8,
    DisplayedCells = 10,
    Fill = 16,
}

public enum DataGridViewColumnSortMode { NotSortable, Automatic, Programmatic }

public enum DataGridViewSelectionMode
{
    CellSelect,
    FullRowSelect,
    FullColumnSelect,
    RowHeaderSelect,
    ColumnHeaderSelect,
}

public enum DataGridViewEditMode
{
    EditOnEnter,
    EditOnKeystroke,
    EditOnKeystrokeOrF2,
    EditOnF2,
    EditProgrammatically,
}

public enum DataGridViewClipboardCopyMode
{
    Disable,
    EnableWithAutoHeaderText,
    EnableWithoutHeaderText,
    EnableAlwaysIncludeHeaderText,
}

public enum DataGridViewRowHeadersWidthSizeMode
{
    EnableResizing,
    DisableResizing,
    AutoSizeToAllHeaders,
    AutoSizeToDisplayedHeaders,
    AutoSizeToFirstHeader,
}

public enum DataGridViewColumnHeadersHeightSizeMode
{
    EnableResizing,
    DisableResizing,
    AutoSize,
}

public enum DataGridViewScrollBars { None, Horizontal, Vertical, Both }
