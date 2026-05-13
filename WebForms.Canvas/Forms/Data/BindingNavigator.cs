namespace System.Windows.Forms;

/// <summary>
/// A <see cref="ToolStrip"/>-based record-navigation bar that wraps a <see cref="BindingSource"/>.
/// Provides First / Previous / position label / Next / Last / Add / Delete buttons
/// that delegate to the bound <see cref="BindingSource"/> — matching the WinForms
/// <c>BindingNavigator</c> public API surface used by designer-generated apps.
/// </summary>
public class BindingNavigator : ToolStrip
{
    private BindingSource? _bindingSource;

    // ── Exposed navigation items (match WinForms property names) ─────────────
    public ToolStripButton   MoveFirstItem    { get; } = new("◀◀") { ToolTipText = "Move first",    Name = "bindingNavigatorMoveFirstItem" };
    public ToolStripButton   MovePreviousItem { get; } = new("◀")  { ToolTipText = "Move previous", Name = "bindingNavigatorMovePreviousItem" };
    public ToolStripTextBox  PositionItem     { get; } = new()      { Name = "bindingNavigatorPositionItem",    AutoSize = false, Width = 50 };
    public ToolStripLabel    CountItem        { get; } = new()      { Name = "bindingNavigatorCountItem",       AutoSize = true };
    public ToolStripButton   MoveNextItem     { get; } = new("▶")  { ToolTipText = "Move next",     Name = "bindingNavigatorMoveNextItem" };
    public ToolStripButton   MoveLastItem     { get; } = new("▶▶") { ToolTipText = "Move last",     Name = "bindingNavigatorMoveLastItem" };
    public ToolStripButton   AddNewItem       { get; } = new("＋") { ToolTipText = "Add new",       Name = "bindingNavigatorAddNewItem" };
    public ToolStripButton   DeleteItem       { get; } = new("✕")  { ToolTipText = "Delete",        Name = "bindingNavigatorDeleteItem" };

    // ── Constructors ──────────────────────────────────────────────────────────

    /// <summary>Initialises a <see cref="BindingNavigator"/> with no bound source.</summary>
    public BindingNavigator() : this(null) { }

    /// <summary>Initialises a <see cref="BindingNavigator"/> bound to <paramref name="bindingSource"/>.</summary>
    public BindingNavigator(BindingSource? bindingSource)
    {
        // Wire default click handlers.
        MoveFirstItem.Click    += (_, _) => _bindingSource?.MoveFirst();
        MovePreviousItem.Click += (_, _) => _bindingSource?.MovePrevious();
        MoveNextItem.Click     += (_, _) => _bindingSource?.MoveNext();
        MoveLastItem.Click     += (_, _) => _bindingSource?.MoveLast();

        AddNewItem.Click  += (_, _) => OnAddNew();
        DeleteItem.Click  += (_, _) => OnDeleteCurrent();

        // Allow the user to type a record number and press Enter to navigate.
        PositionItem.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter && _bindingSource != null)
            {
                if (int.TryParse(PositionItem.Text, out int idx))
                {
                    int zero = idx - 1;  // PositionItem is 1-based like WinForms
                    if (zero >= 0 && zero < _bindingSource.Count)
                        _bindingSource.Position = zero;
                }
                // Revert to the actual position if input was invalid
                RefreshState();
                e.Handled = true;
            }
        };

        // Build default item layout (matches Visual Studio designer output).
        Items.Add(MoveFirstItem);
        Items.Add(MovePreviousItem);
        Items.Add(new ToolStripSeparator());
        Items.Add(PositionItem);
        Items.Add(CountItem);
        Items.Add(new ToolStripSeparator());
        Items.Add(MoveNextItem);
        Items.Add(MoveLastItem);
        Items.Add(new ToolStripSeparator());
        Items.Add(AddNewItem);
        Items.Add(DeleteItem);

        BindingSource = bindingSource;
    }

    // ── BindingSource binding ─────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the <see cref="BindingSource"/> this navigator controls.
    /// Matches WinForms <c>BindingNavigator.BindingSource</c>.
    /// </summary>
    public BindingSource? BindingSource
    {
        get => _bindingSource;
        set
        {
            if (_bindingSource != null)
            {
                _bindingSource.PositionChanged -= OnPositionChanged;
                _bindingSource.ListChanged     -= OnListChanged;
            }

            _bindingSource = value;

            if (_bindingSource != null)
            {
                _bindingSource.PositionChanged += OnPositionChanged;
                _bindingSource.ListChanged     += OnListChanged;
            }

            RefreshState();
        }
    }

    // ── AddNew / Delete virtuals (override to customise) ─────────────────────

    /// <summary>Called when the user clicks the Add button. Override to supply custom logic.</summary>
    protected virtual void OnAddNew()
    {
        if (_bindingSource == null) return;
        _bindingSource.Add(null);
        _bindingSource.MoveLast();
    }

    /// <summary>Called when the user clicks the Delete button. Override to supply custom logic.</summary>
    protected virtual void OnDeleteCurrent()
    {
        if (_bindingSource == null || _bindingSource.Count == 0) return;
        _bindingSource.RemoveAt(_bindingSource.Position);
    }

    // ── State refresh ─────────────────────────────────────────────────────────

    private void OnPositionChanged(object? sender, EventArgs e) => RefreshState();
    private void OnListChanged(object? sender, ComponentModel.ListChangedEventArgs e) => RefreshState();

    private void RefreshState()
    {
        var bs    = _bindingSource;
        var count = bs?.Count ?? 0;
        var pos   = bs != null && count > 0 ? bs.Position + 1 : 0;

        PositionItem.Text = pos.ToString();
        CountItem.Text    = $"/ {count}";

        var hasCurrent = count > 0 && bs != null;
        MoveFirstItem.Enabled    = hasCurrent && bs!.Position > 0;
        MovePreviousItem.Enabled = hasCurrent && bs!.Position > 0;
        MoveNextItem.Enabled     = hasCurrent && bs!.Position < count - 1;
        MoveLastItem.Enabled     = hasCurrent && bs!.Position < count - 1;
        DeleteItem.Enabled       = hasCurrent;

        Invalidate();
    }
}
