using System.ComponentModel;

namespace System.Windows.Forms;

// ── ToolBarButton ─────────────────────────────────────────────────────────────
/// <summary>
/// Legacy pre-<see cref="ToolStrip"/> toolbar button.
/// Wraps <see cref="ToolStripButton"/> so that translated WinForms apps that
/// use <c>ToolBarButton</c> continue to work without modification.
/// </summary>
public class ToolBarButton : System.ComponentModel.Component
{
    internal readonly ToolStripButton         _inner;     // used for PushButton / ToggleButton
    internal readonly ToolStripDropDownButton _innerDrop; // used for DropDownButton

    /// <summary>Returns whichever inner item is active given the current <see cref="Style"/>.</summary>
    internal ToolStripItem ActiveItem => _style == ToolBarButtonStyle.DropDownButton
        ? (ToolStripItem)_innerDrop : _inner;

    // ── Constructors ──────────────────────────────────────────────────────────

    public ToolBarButton() : this(string.Empty) { }

    public ToolBarButton(string text)
    {
        _inner     = new ToolStripButton(text);
        _innerDrop = new ToolStripDropDownButton(text);

        // Both inner items forward Click to the ToolBarButton.Click event.
        _inner.Click     += (s, e) => Click?.Invoke(this, e);
        _innerDrop.Click += (s, e) => Click?.Invoke(this, e);
    }

    // ── Properties ────────────────────────────────────────────────────────────

    public string Text
    {
        get => _inner.Text;
        set { _inner.Text = value; _innerDrop.Text = value; }
    }

    public string ToolTipText
    {
        get => _inner.ToolTipText;
        set { _inner.ToolTipText = value; _innerDrop.ToolTipText = value; }
    }

    public bool Enabled
    {
        get => _inner.Enabled;
        set { _inner.Enabled = value; _innerDrop.Enabled = value; }
    }

    public bool Visible
    {
        get => _inner.Visible;
        set { _inner.Visible = value; _innerDrop.Visible = value; }
    }

    public bool Pushed
    {
        get => _inner.Checked;
        set => _inner.Checked = value;
    }

    public bool PartialPush { get; set; }

    public Image? Image
    {
        get => _inner.Image;
        set { _inner.Image = value; _innerDrop.Image = value; }
    }

    public int ImageIndex { get; set; } = -1;

    public string Name
    {
        get => _inner.Name;
        set { _inner.Name = value; _innerDrop.Name = value; }
    }

    public object? Tag { get; set; }

    private ToolBarButtonStyle _style = ToolBarButtonStyle.PushButton;

    /// <summary>
    /// Gets or sets the button style.
    /// Changing to/from <see cref="ToolBarButtonStyle.DropDownButton"/> swaps the active
    /// inner item if this button has already been added to a <see cref="ToolBar"/>.
    /// </summary>
    public ToolBarButtonStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            _inner.Visible    = value != ToolBarButtonStyle.Separator;
            _innerDrop.Visible = value == ToolBarButtonStyle.DropDownButton;
        }
    }

    private Menu? _dropDownMenu;

    /// <summary>
    /// The dropdown menu shown when <see cref="Style"/> is <see cref="ToolBarButtonStyle.DropDownButton"/>.
    /// Setting this populates <see cref="ToolStripDropDownButton.DropDownItems"/> from the menu.
    /// </summary>
    public Menu? DropDownMenu
    {
        get => _dropDownMenu;
        set
        {
            _dropDownMenu = value;
            SyncDropDownItems();
        }
    }

    /// <summary>Populates the inner dropdown button's items from the assigned <see cref="DropDownMenu"/>.</summary>
    internal void SyncDropDownItems()
    {
        _innerDrop.DropDownItems.Clear();
        if (_dropDownMenu is ContextMenu cm)
        {
            foreach (var mi in cm.MenuItems)
                _innerDrop.DropDownItems.Add(mi._inner);
        }
        else if (_dropDownMenu is MainMenu mm)
        {
            foreach (var mi in mm.MenuItems)
                _innerDrop.DropDownItems.Add(mi._inner);
        }
    }

    // ── Events ────────────────────────────────────────────────────────────────

    public event EventHandler? Click;
}

// ── ToolBarButtonStyle ────────────────────────────────────────────────────────
/// <summary>Legacy button-style enum for <see cref="ToolBarButton"/>.</summary>
public enum ToolBarButtonStyle
{
    PushButton    = 1,
    ToggleButton  = 2,
    Separator     = 3,
    DropDownButton= 4,
}

// ── ToolBarAppearance ─────────────────────────────────────────────────────────
/// <summary>Legacy appearance enum for <see cref="ToolBar"/>.</summary>
public enum ToolBarAppearance
{
    Normal = 0,
    Flat   = 1,
}

// ── ToolBarTextAlign ──────────────────────────────────────────────────────────
/// <summary>Legacy text-alignment enum for <see cref="ToolBar"/>.</summary>
public enum ToolBarTextAlign
{
    Underneath = 0,
    Right      = 1,
}

// ── ToolBarButtonClickEventArgs ───────────────────────────────────────────────
/// <summary>Event arguments for <see cref="ToolBar.ButtonClick"/>.</summary>
public class ToolBarButtonClickEventArgs : EventArgs
{
    public ToolBarButton Button { get; }
    public ToolBarButtonClickEventArgs(ToolBarButton button) => Button = button;
}

public delegate void ToolBarButtonClickEventHandler(object sender, ToolBarButtonClickEventArgs e);

// ── ToolBar ───────────────────────────────────────────────────────────────────
/// <summary>
/// Legacy pre-<see cref="ToolStrip"/> toolbar control.
/// Wraps <see cref="ToolStrip"/> internally so that translated WinForms apps that
/// use <c>ToolBar</c> and <c>ToolBarButton</c> render correctly without modification.
/// </summary>
public class ToolBar : Control
{
    internal readonly ToolStrip _strip;
    private readonly ToolBarButtonCollection _buttons;

    private ToolBarAppearance _appearance = ToolBarAppearance.Normal;
    private ToolBarTextAlign  _textAlign  = ToolBarTextAlign.Underneath;
    private bool   _showToolTips  = true;
    private bool   _wrappable     = true;
    private int    _buttonSize    = 24;
    private int    _imageSize     = 16;

    // ── Constructors ──────────────────────────────────────────────────────────

    public ToolBar()
    {
        _strip   = new ToolStrip();
        _buttons = new ToolBarButtonCollection(this);

        Dock     = DockStyle.Top;
        Height   = 30;

        _strip.ItemClicked += (s, e) =>
        {
            // Map ToolStrip item click → ButtonClick for the matching ToolBarButton.
            var btn = _buttons.FindByInner(e.ClickedItem);
            if (btn != null)
                ButtonClick?.Invoke(this, new ToolBarButtonClickEventArgs(btn));
        };
    }

    // ── Properties ────────────────────────────────────────────────────────────

    public ToolBarButtonCollection Buttons => _buttons;

    public ToolBarAppearance Appearance
    {
        get => _appearance;
        set => _appearance = value;
    }

    public ToolBarTextAlign TextAlign
    {
        get => _textAlign;
        set => _textAlign = value;
    }

    public bool ShowToolTips
    {
        get => _showToolTips;
        set => _showToolTips = value;
    }

    public bool Wrappable
    {
        get => _wrappable;
        set => _wrappable = value;
    }

    /// <summary>Gets or sets the uniform button size (height and width in pixels).</summary>
    public int ButtonSize
    {
        get => _buttonSize;
        set => _buttonSize = Math.Max(1, value);
    }

    /// <summary>Gets or sets the image size used for button icons.</summary>
    public int ImageSize
    {
        get => _imageSize;
        set => _imageSize = Math.Max(1, value);
    }

    private ImageList? _imageList;

    public ImageList? ImageList
    {
        get => _imageList;
        set => _imageList = value;
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when any toolbar button is clicked.</summary>
    public event ToolBarButtonClickEventHandler? ButtonClick;

    /// <summary>
    /// Raised for each button when <see cref="DrawMode"/> is
    /// <see cref="ToolBarDrawMode.OwnerDraw"/>.
    /// </summary>
    public event ToolBarDrawItemEventHandler? DrawItem;

    /// <summary>Gets or sets whether buttons are owner-drawn.</summary>
    public ToolBarDrawMode DrawMode { get; set; } = ToolBarDrawMode.Normal;

    // ── Paint / layout ────────────────────────────────────────────────────────

    /// <summary>
    /// Renders the legacy toolbar by delegating to the wrapped <see cref="ToolStrip"/>,
    /// or by firing <see cref="DrawItem"/> for each button when owner-draw is active.
    /// </summary>
    protected internal override void OnPaint(PaintEventArgs e)
    {
        // Sync bounds so the inner strip fills our client area.
        _strip.Left   = Left;
        _strip.Top    = Top;
        _strip.Width  = Width;
        _strip.Height = Height;

        if (DrawMode == ToolBarDrawMode.OwnerDraw && DrawItem != null)
        {
            // Owner-draw: fill background then fire DrawItem per button.
            using var bgBrush = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(bgBrush, new Rectangle(Left, Top, Width, Height));
            int x = Left;
            foreach (var btn in _buttons)
            {
                if (!btn.Visible) continue;
                int btnW = btn.Style == ToolBarButtonStyle.Separator ? 8 : ButtonSize;
                var rect = new Rectangle(x, Top, btnW, Height);
                DrawItem.Invoke(this, new ToolBarDrawItemEventArgs(e.Graphics, btn, rect));
                x += btnW;
            }
        }
        else
        {
            _strip.OnPaint(e);
        }
    }

    // ── ToolBarButtonCollection ───────────────────────────────────────────────

    public sealed class ToolBarButtonCollection : IEnumerable<ToolBarButton>
    {
        private readonly ToolBar _owner;
        private readonly List<ToolBarButton> _items = new();

        internal ToolBarButtonCollection(ToolBar owner) => _owner = owner;

        public int Count => _items.Count;
        public ToolBarButton this[int index] => _items[index];

        public void Add(ToolBarButton btn)
        {
            _items.Add(btn);
            if (btn.Style == ToolBarButtonStyle.Separator)
            {
                _owner._strip.Items.Add(new ToolStripSeparator());
            }
            else if (btn.Style == ToolBarButtonStyle.DropDownButton)
            {
                btn.SyncDropDownItems();
                _owner._strip.Items.Add(btn._innerDrop);
            }
            else
            {
                _owner._strip.Items.Add(btn._inner);
            }
        }

        public void AddRange(ToolBarButton[] buttons)
        {
            foreach (var b in buttons) Add(b);
        }

        public void Remove(ToolBarButton btn)
        {
            _items.Remove(btn);
            _owner._strip.Items.Remove(btn._inner);
        }

        public void Clear()
        {
            _items.Clear();
            _owner._strip.Items.Clear();
        }

        internal ToolBarButton? FindByInner(ToolStripItem? inner)
        {
            if (inner == null) return null;
            return _items.FirstOrDefault(b => b._inner == inner || b._innerDrop == inner);
        }

        public IEnumerator<ToolBarButton> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}

// ── ToolBarDrawMode ───────────────────────────────────────────────────────────
/// <summary>Controls whether toolbar buttons are owner-drawn.</summary>
public enum ToolBarDrawMode
{
    /// <summary>Buttons are drawn by the system.</summary>
    Normal    = 0,
    /// <summary>All buttons are drawn by the owner via <see cref="ToolBar.DrawItem"/>.</summary>
    OwnerDraw = 1,
}

// ── ToolBarDrawItemEventArgs ──────────────────────────────────────────────────
/// <summary>Event arguments for <see cref="ToolBar.DrawItem"/>.</summary>
public class ToolBarDrawItemEventArgs : EventArgs
{
    public Graphics      Graphics { get; }
    public ToolBarButton Button   { get; }
    public Rectangle     Bounds   { get; }
    public DrawItemState State    { get; }

    public ToolBarDrawItemEventArgs(Graphics g, ToolBarButton button,
        Rectangle bounds, DrawItemState state = DrawItemState.Default)
    {
        Graphics = g;
        Button   = button;
        Bounds   = bounds;
        State    = state;
    }
}

/// <summary>Delegate for <see cref="ToolBar.DrawItem"/>.</summary>
public delegate void ToolBarDrawItemEventHandler(object? sender, ToolBarDrawItemEventArgs e);

// ── Menu (marker interface used by MainMenu / ContextMenu legacy API) ─────────
/// <summary>
/// Marker interface preserved for legacy <c>ToolBarButton.DropDownMenu</c> compatibility.
/// <see cref="MainMenu"/> and <see cref="ContextMenu"/> both implement this.
/// </summary>
public interface Menu { }
