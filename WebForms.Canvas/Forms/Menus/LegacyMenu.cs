using System.ComponentModel;

namespace System.Windows.Forms;

/// <summary>Internal interface so MenuItem can reach its siblings for RadioCheck.</summary>
internal interface IMenuItemOwner
{
    IEnumerable<MenuItem> GetItems();
}

// ── MenuItem ──────────────────────────────────────────────────────────────────
/// <summary>
/// Legacy pre-<see cref="MenuStrip"/> menu item.
/// Wraps <see cref="ToolStripMenuItem"/> so that translated WinForms apps that
/// use <c>MenuItem</c> continue to work without modification.
/// </summary>
public class MenuItem : System.ComponentModel.Component
{
    // Internal bridge to the modern item used for actual rendering.
    internal readonly ToolStripMenuItem _inner;

    // Back-reference to the collection that owns this item (set by MenuItemCollection/MainMenuItemCollection).
    // Used by RadioCheck to uncheck siblings.
    internal IMenuItemOwner? _ownerCollection;

    // ── Sub-items ─────────────────────────────────────────────────────────────

    private MenuItemCollection? _menuItems;

    /// <summary>Gets the collection of sub-items (child menu entries).</summary>
    public MenuItemCollection MenuItems => _menuItems ??= new MenuItemCollection(this);

    // ── Constructors ──────────────────────────────────────────────────────────

    public MenuItem() : this(string.Empty) { }

    public MenuItem(string text)
    {
        _inner = new ToolStripMenuItem(text);
        _inner.Click         += (s, e) => Click?.Invoke(this, e);
        // Popup fires when the sub-menu is about to open (DropDownOpening is the ToolStrip equivalent).
        _inner.DropDownOpening += (s, e) => Popup?.Invoke(this, e);
        // Select fires when the item is highlighted (mouse enter on the ToolStrip item).
        _inner.MouseEnter    += (s, e) => Select?.Invoke(this, e);
    }

    public MenuItem(string text, EventHandler onClick) : this(text)
    {
        Click += onClick;
    }

    public MenuItem(string text, MenuItem[] items) : this(text)
    {
        foreach (var item in items) MenuItems.Add(item);
    }

    // ── Properties ────────────────────────────────────────────────────────────

    public string Text
    {
        get => _inner.Text;
        set => _inner.Text = value;
    }

    public bool Enabled
    {
        get => _inner.Enabled;
        set => _inner.Enabled = value;
    }

    public bool Visible
    {
        get => _inner.Visible;
        set => _inner.Visible = value;
    }

    public bool Checked
    {
        get => _inner.Checked;
        set
        {
            _inner.Checked = value;
            // WinForms RadioCheck: when this item becomes checked, uncheck siblings in the same collection.
            if (value && RadioCheck && _ownerCollection != null)
            {
                foreach (var sibling in _ownerCollection.GetItems())
                {
                    if (sibling != this && sibling.RadioCheck)
                        sibling._inner.Checked = false;
                }
            }
        }
    }

    public bool RadioCheck { get; set; }

    public bool OwnerDraw { get; set; }

    /// <summary>The keyboard shortcut associated with this item.</summary>
    public Shortcut Shortcut
    {
        get => (Shortcut)(int)_inner.ShortcutKeys;
        set => _inner.ShortcutKeys = (Keys)(int)value;
    }

    public bool ShowShortcut
    {
        get => _inner.ShowShortcutKeys;
        set => _inner.ShowShortcutKeys = value;
    }

    /// <summary>Index within the parent menu's MenuItems collection (-1 if not parented).</summary>
    public int Index { get; internal set; } = -1;

    // ── Events ────────────────────────────────────────────────────────────────

    public event EventHandler? Click;

    /// <summary>Raised when the menu item's popup (dropdown) is about to display.</summary>
    public event EventHandler? Popup;

    /// <summary>Raised when the item is selected (highlighted) by mouse or keyboard.</summary>
    public event EventHandler? Select;

    // ── Methods ───────────────────────────────────────────────────────────────

    public void PerformClick() => _inner.PerformClick();

    /// <summary>Raises the <see cref="Popup"/> event (WinForms pattern).</summary>
    public void RaisePopup() => Popup?.Invoke(this, EventArgs.Empty);

    // ── MenuItemCollection ────────────────────────────────────────────────────

    public sealed class MenuItemCollection : IEnumerable<MenuItem>, IMenuItemOwner
    {
        private readonly MenuItem _owner;
        private readonly List<MenuItem> _items = new();

        internal MenuItemCollection(MenuItem owner) => _owner = owner;

        public int Count => _items.Count;

        public MenuItem this[int index] => _items[index];

        public void Add(MenuItem item)
        {
            item.Index            = _items.Count;
            item._ownerCollection = this;
            _items.Add(item);
            _owner._inner.DropDownItems.Add(item._inner);
        }

        public IEnumerable<MenuItem> GetItems() => _items;

        public void AddRange(MenuItem[] items)
        {
            foreach (var item in items) Add(item);
        }

        public void Remove(MenuItem item)
        {
            _items.Remove(item);
            _owner._inner.DropDownItems.Remove(item._inner);
            RenumberFrom(0);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _items.Count) return;
            Remove(_items[index]);
        }

        public void Clear()
        {
            _items.Clear();
            _owner._inner.DropDownItems.Clear();
        }

        private void RenumberFrom(int start)
        {
            for (int i = start; i < _items.Count; i++)
                _items[i].Index = i;
        }

        public IEnumerator<MenuItem> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}

// ── Shortcut enum (legacy) ────────────────────────────────────────────────────
/// <summary>
/// Pre-<see cref="Keys"/> shortcut enum used by <see cref="MenuItem"/>.
/// Values are identical to the <see cref="Keys"/> int representation so a
/// direct cast is safe.
/// </summary>
public enum Shortcut
{
    None       = 0,
    Ins        = 0x2D,
    Del        = 0x2E,
    F1         = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73,
    F5         = 0x74, F6 = 0x75, F7 = 0x76, F8 = 0x77,
    F9         = 0x78, F10 = 0x79, F11 = 0x7A, F12 = 0x7B,
    CtrlA      = 0x20041, CtrlB = 0x20042, CtrlC = 0x20043,
    CtrlD      = 0x20044, CtrlE = 0x20045, CtrlF = 0x20046,
    CtrlG      = 0x20047, CtrlH = 0x20048, CtrlI = 0x20049,
    CtrlJ      = 0x2004A, CtrlK = 0x2004B, CtrlL = 0x2004C,
    CtrlM      = 0x2004D, CtrlN = 0x2004E, CtrlO = 0x2004F,
    CtrlP      = 0x20050, CtrlQ = 0x20051, CtrlR = 0x20052,
    CtrlS      = 0x20053, CtrlT = 0x20054, CtrlU = 0x20055,
    CtrlV      = 0x20056, CtrlW = 0x20057, CtrlX = 0x20058,
    CtrlY      = 0x20059, CtrlZ = 0x2005A,
    CtrlF1     = 0x20070, CtrlF2 = 0x20071, CtrlF3 = 0x20072,
    CtrlF4     = 0x20073, CtrlF5 = 0x20074, CtrlF6 = 0x20075,
    CtrlF7     = 0x20076, CtrlF8 = 0x20077, CtrlF9 = 0x20078,
    CtrlF10    = 0x20079, CtrlF11 = 0x2007A, CtrlF12 = 0x2007B,
    CtrlShiftA = 0x30041, CtrlShiftB = 0x30042, CtrlShiftC = 0x30043,
    CtrlShiftD = 0x30044, CtrlShiftE = 0x30045, CtrlShiftF = 0x30046,
    CtrlShiftG = 0x30047, CtrlShiftH = 0x30048, CtrlShiftI = 0x30049,
    CtrlShiftJ = 0x3004A, CtrlShiftK = 0x3004B, CtrlShiftL = 0x3004C,
    CtrlShiftM = 0x3004D, CtrlShiftN = 0x3004E, CtrlShiftO = 0x3004F,
    CtrlShiftP = 0x30050, CtrlShiftQ = 0x30051, CtrlShiftR = 0x30052,
    CtrlShiftS = 0x30053, CtrlShiftT = 0x30054, CtrlShiftU = 0x30055,
    CtrlShiftV = 0x30056, CtrlShiftW = 0x30057, CtrlShiftX = 0x30058,
    CtrlShiftY = 0x30059, CtrlShiftZ = 0x3005A,
    AltF4      = 0x10073,
    AltBksp    = 0x10008,
}

// ── MainMenu ──────────────────────────────────────────────────────────────────
/// <summary>
/// Legacy pre-<see cref="MenuStrip"/> top-level menu bar.
/// Wraps a <see cref="MenuStrip"/> internally so that translated apps
/// assigning <c>this.Menu = mainMenu1</c> render correctly.
/// </summary>
public class MainMenu : System.ComponentModel.Component, Menu
{
    internal readonly MenuStrip _menuStrip;
    private readonly MainMenuItemCollection _items;

    // ── Constructor ───────────────────────────────────────────────────────────

    public MainMenu()
    {
        _menuStrip = new MenuStrip();
        _items     = new MainMenuItemCollection(this);
    }

    public MainMenu(MenuItem[] items) : this()
    {
        foreach (var item in items) MenuItems.Add(item);
    }

    // ── Items ─────────────────────────────────────────────────────────────────

    public MainMenuItemCollection MenuItems => _items;

    // ── RightToLeft ───────────────────────────────────────────────────────────

    public RightToLeft RightToLeft
    {
        get => _menuStrip.RightToLeft;
        set => _menuStrip.RightToLeft = value;
    }

    // ── MainMenuItemCollection ────────────────────────────────────────────────

    public sealed class MainMenuItemCollection : IEnumerable<MenuItem>, IMenuItemOwner
    {
        private readonly MainMenu _owner;
        private readonly List<MenuItem> _items = new();

        internal MainMenuItemCollection(MainMenu owner) => _owner = owner;

        public int Count => _items.Count;
        public MenuItem this[int index] => _items[index];

        public void Add(MenuItem item)
        {
            item.Index            = _items.Count;
            item._ownerCollection = this;
            _items.Add(item);
            _owner._menuStrip.Items.Add(item._inner);
        }

        public IEnumerable<MenuItem> GetItems() => _items;

        public void AddRange(MenuItem[] items)
        {
            foreach (var item in items) Add(item);
        }

        public void Remove(MenuItem item)
        {
            _items.Remove(item);
            _owner._menuStrip.Items.Remove(item._inner);
        }

        public void Clear()
        {
            _items.Clear();
            _owner._menuStrip.Items.Clear();
        }

        public IEnumerator<MenuItem> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}

// ── Form.Menu bridge ──────────────────────────────────────────────────────────
// The Form class exposes a `Menu` property that accepts a MainMenu.
// That property is defined in Form.cs.  We extend it via a partial class here
// so the property compiles without modifying Form.cs directly.
// (See Form.cs for the actual setter wiring into the Controls collection.)

// ── ContextMenu ───────────────────────────────────────────────────────────────
/// <summary>
/// Legacy pre-<see cref="ContextMenuStrip"/> floating context menu.
/// Wraps <see cref="ContextMenuStrip"/> so that translated WinForms apps that
/// assign <c>control.ContextMenu = contextMenu1</c> continue to work.
/// </summary>
public class ContextMenu : System.ComponentModel.Component, Menu
{
    internal readonly ContextMenuStrip _strip;
    private readonly ContextMenuItemCollection _items;

    // ── Constructor ───────────────────────────────────────────────────────────

    public ContextMenu()
    {
        _strip = new ContextMenuStrip();
        _items = new ContextMenuItemCollection(this);
    }

    public ContextMenu(MenuItem[] items) : this()
    {
        foreach (var item in items) MenuItems.Add(item);
    }

    // ── Items ─────────────────────────────────────────────────────────────────

    public ContextMenuItemCollection MenuItems => _items;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised before the menu is displayed (mirrors WinForms ContextMenu.Popup).</summary>
    public event EventHandler? Popup;

    // ── Show ──────────────────────────────────────────────────────────────────

    public void Show(Control control, Point pos)
    {
        Popup?.Invoke(this, EventArgs.Empty);
        _strip.Show(control, pos);
    }

    // ── ContextMenuItemCollection ─────────────────────────────────────────────

    public sealed class ContextMenuItemCollection : IEnumerable<MenuItem>, IMenuItemOwner
    {
        private readonly ContextMenu _owner;
        private readonly List<MenuItem> _items = new();

        internal ContextMenuItemCollection(ContextMenu owner) => _owner = owner;

        public int Count => _items.Count;
        public MenuItem this[int index] => _items[index];

        public void Add(MenuItem item)
        {
            item.Index            = _items.Count;
            item._ownerCollection = this;
            _items.Add(item);
            _owner._strip.Items.Add(item._inner);
        }

        public IEnumerable<MenuItem> GetItems() => _items;

        public void AddRange(MenuItem[] items)
        {
            foreach (var item in items) Add(item);
        }

        public void Remove(MenuItem item)
        {
            _items.Remove(item);
            _owner._strip.Items.Remove(item._inner);
        }

        public void Clear()
        {
            _items.Clear();
            _owner._strip.Items.Clear();
        }

        public IEnumerator<MenuItem> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}
