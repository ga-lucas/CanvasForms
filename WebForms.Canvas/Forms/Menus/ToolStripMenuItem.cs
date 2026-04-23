namespace System.Windows.Forms;

// ── ToolStripMenuItem ─────────────────────────────────────────────────────────
/// <summary>
/// A clickable menu item that can host a submenu dropdown.
/// Matches WinForms ToolStripMenuItem API surface.
/// </summary>
public class ToolStripMenuItem : ToolStripItem
{
    private ToolStripDropDownMenu? _dropDown;
    private bool _checked;
    private CheckState _checkState = CheckState.Unchecked;
    private bool _checkOnClick;
    private Keys _shortcutKeys = Keys.None;
    private bool _showShortcutKeys = true;
    private ToolStripDropDownDirection _dropDownDirection = ToolStripDropDownDirection.Default;

    // ── Dropdown ───────────────────────────────────────────────────────────────

    /// <summary>The dropdown that opens when this item is clicked (lazily created).</summary>
    public ToolStripDropDownMenu DropDown
        => _dropDown ??= CreateDropDown();

    /// <summary>Convenience alias for DropDown.Items.</summary>
    public ToolStripItemCollection DropDownItems => DropDown.Items;

    /// <summary>True when this item has at least one visible dropdown item.</summary>
    public bool HasDropDownItems => _dropDown != null && _dropDown.Items.Count > 0;

    public bool DropDownIsOpen => _dropDown?.IsVisible == true;

    /// <summary>Direction in which the dropdown opens.</summary>
    public ToolStripDropDownDirection DropDownDirection
    {
        get => _dropDownDirection;
        set => _dropDownDirection = value;
    }

    private ToolStripDropDownMenu CreateDropDown()
    {
        var dd = new ToolStripDropDownMenu { SourceItem = this };
        return dd;
    }

    // ── Check state ────────────────────────────────────────────────────────────

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;
            _checked    = value;
            _checkState = value ? CheckState.Checked : CheckState.Unchecked;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            CheckStateChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }
    }

    public CheckState CheckState
    {
        get => _checkState;
        set
        {
            if (_checkState == value) return;
            _checkState = value;
            _checked    = value == CheckState.Checked;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            CheckStateChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }
    }

    public bool CheckOnClick
    {
        get => _checkOnClick;
        set => _checkOnClick = value;
    }

    // ── Shortcut ───────────────────────────────────────────────────────────────

    public Keys ShortcutKeys
    {
        get => _shortcutKeys;
        set => _shortcutKeys = value;
    }

    public bool ShowShortcutKeys
    {
        get => _showShortcutKeys;
        set => _showShortcutKeys = value;
    }

    public string ShortcutKeyDisplayString { get; set; } = string.Empty;

    // ── MDI ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// True when this item is a member of an MDI Window list (stub — always false).
    /// </summary>
    public bool IsMdiWindowListEntry => false;

    /// <summary>
    /// The native menu command ID (stub — returns -1, not applicable in canvas).
    /// </summary>
    public int NativeMenuCommandID => -1;

    // ── Events ─────────────────────────────────────────────────────────────────

    public event EventHandler? DropDownOpening;
    public event EventHandler? DropDownOpened;
    public event EventHandler? DropDownClosed;
    public event EventHandler? CheckedChanged;
    public event EventHandler? CheckStateChanged;

    // ── Constructors ───────────────────────────────────────────────────────────

    public ToolStripMenuItem() { }

    public ToolStripMenuItem(string text) { Text = text; }

    public ToolStripMenuItem(string text, Image? image) { Text = text; Image = image; }

    public ToolStripMenuItem(string text, Image? image, EventHandler onClick)
    {
        Text = text; Image = image;
        Click += onClick;
    }

    public ToolStripMenuItem(string text, Image? image, params ToolStripItem[] dropDownItems)
    {
        Text = text; Image = image;
        foreach (var item in dropDownItems) DropDownItems.Add(item);
    }

    public ToolStripMenuItem(string text, Image? image, EventHandler onClick, Keys shortcutKeys)
    {
        Text = text; Image = image;
        Click += onClick;
        ShortcutKeys = shortcutKeys;
    }

    public ToolStripMenuItem(string text, Image? image, EventHandler onClick, string name)
    {
        Text = text; Image = image;
        Click += onClick;
        Name = name;
    }

    // ── Protected virtual event raisers ───────────────────────────────────────

    protected virtual void OnDropDownOpening(EventArgs e)  => DropDownOpening?.Invoke(this, e);
    protected virtual void OnDropDownOpened(EventArgs e)   => DropDownOpened?.Invoke(this, e);
    protected virtual void OnDropDownClosed(EventArgs e)   => DropDownClosed?.Invoke(this, e);
    protected virtual void OnCheckedChanged(EventArgs e)   => CheckedChanged?.Invoke(this, e);
    protected virtual void OnCheckStateChanged(EventArgs e) => CheckStateChanged?.Invoke(this, e);

    // ── Interaction ────────────────────────────────────────────────────────────

    protected internal override void OnClick(EventArgs e)
    {
        if (CheckOnClick)
        {
            _checked    = !_checked;
            _checkState = _checked ? CheckState.Checked : CheckState.Unchecked;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            CheckStateChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }

        base.OnClick(e);
    }

    /// <summary>Opens the dropdown at the specified form-absolute location.</summary>
    public void OpenDropDown(Point formLocation)
    {
        if (!HasDropDownItems) return;

        DropDownOpening?.Invoke(this, EventArgs.Empty);
        DropDown.PopupLocation = formLocation;
        DropDown.IsVisible     = true;
        DropDownOpened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Closes the dropdown (does not cascade up to parent).</summary>
    public void CloseDropDown()
    {
        if (_dropDown == null) return;
        _dropDown.CloseChain();
        DropDownClosed?.Invoke(this, EventArgs.Empty);
    }
}

