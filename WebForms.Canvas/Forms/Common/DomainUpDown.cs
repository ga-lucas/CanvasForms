namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms DomainUpDown control for cycling through a string list.
/// Hierarchy matches WinForms: DomainUpDown : UpDownBase : ContainerControl : ScrollableControl : Control.
/// </summary>
public class DomainUpDown : UpDownBase
{
    private int  _selectedIndex = -1;
    private bool _sorted        = false;
    private bool _wrap          = false;

    public event EventHandler? SelectedItemChanged;

    // ── Items collection ──────────────────────────────────────────────────────

    public DomainUpDownItemCollection Items { get; } = new DomainUpDownItemCollection();

    public DomainUpDown()
    {
        Width  = 120;
        Height = 23;
        Items.CollectionChanged += (_, _) =>
        {
            if (_sorted) Items.SortInternal();
            _selectedIndex = Items.Count > 0 ? 0 : -1;
            Invalidate();
        };
    }

    // ── Properties ───────────────────────────────────────────────────────────

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (Items.Count == 0) { _selectedIndex = -1; return; }
            int clamped = Math.Max(-1, Math.Min(Items.Count - 1, value));
            if (_selectedIndex == clamped) return;
            _selectedIndex = clamped;
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public object? SelectedItem
    {
        get => _selectedIndex >= 0 && _selectedIndex < Items.Count ? Items[_selectedIndex] : null;
        set
        {
            if (value is null) { SelectedIndex = -1; return; }
            int idx = Items.IndexOf(value.ToString() ?? string.Empty);
            SelectedIndex = idx;
        }
    }

    public bool Sorted
    {
        get => _sorted;
        set
        {
            _sorted = value;
            if (_sorted) { Items.SortInternal(); Invalidate(); }
        }
    }

    public bool Wrap
    {
        get => _wrap;
        set => _wrap = value;
    }

    public HorizontalAlignment TextAlign { get; set; } = HorizontalAlignment.Left;

    // ── UpDownBase implementation ─────────────────────────────────────────────

    public override void UpButton()
    {
        if (Items.Count == 0) return;
        if (_selectedIndex < Items.Count - 1)
            SelectedIndex++;
        else if (_wrap)
            SelectedIndex = 0;
    }

    public override void DownButton()
    {
        if (Items.Count == 0) return;
        if (_selectedIndex > 0)
            SelectedIndex--;
        else if (_wrap)
            SelectedIndex = Items.Count - 1;
    }

    protected override string GetValueText()
    {
        if (_selectedIndex >= 0 && _selectedIndex < Items.Count)
            return Items[_selectedIndex] ?? string.Empty;
        return string.Empty;
    }

    // ── Keyboard ──────────────────────────────────────────────────────────────

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Up:
                UpButton();
                e.Handled = true;
                break;
            case Keys.Down:
                DownButton();
                e.Handled = true;
                break;
        }
        base.OnKeyDown(e);
    }
}

// ── Items collection ──────────────────────────────────────────────────────────

/// <summary>
/// Collection of string items for DomainUpDown.
/// Matches WinForms DomainUpDown.DomainUpDownItemCollection behavior.
/// </summary>
public class DomainUpDownItemCollection
{
    private readonly List<string> _items = new();

    public event EventHandler? CollectionChanged;

    public string? this[int index] => _items[index];

    public int Count => _items.Count;

    public void Add(object? item)
    {
        _items.Add(item?.ToString() ?? string.Empty);
        CollectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Insert(int index, object? item)
    {
        _items.Insert(index, item?.ToString() ?? string.Empty);
        CollectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Remove(object? item)
    {
        _items.Remove(item?.ToString() ?? string.Empty);
        CollectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveAt(int index)
    {
        _items.RemoveAt(index);
        CollectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        _items.Clear();
        CollectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public int IndexOf(string item) => _items.IndexOf(item);

    public bool Contains(string item) => _items.Contains(item);

    internal void SortInternal() => _items.Sort(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> AsReadOnly() => _items.AsReadOnly();
}
