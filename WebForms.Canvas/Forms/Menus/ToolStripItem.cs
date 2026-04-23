namespace System.Windows.Forms;

// ── ToolStripItem ──────────────────────────────────────────────────────────────
/// <summary>
/// Abstract base for all items that live inside a ToolStrip.
/// Matches the WinForms ToolStripItem class in API surface.
/// This is NOT a Control — it is a lightweight component owned by its parent strip.
/// </summary>
public abstract class ToolStripItem : IDisposable
{
    private string _text = string.Empty;
    private bool   _enabled  = true;
    private bool   _visible  = true;
    private bool   _available = true;
    private Image? _image;
    private System.Drawing.Color _foreColor = System.Drawing.Color.Empty;
    private System.Drawing.Color _backColor = System.Drawing.Color.Empty;
    private Font?  _font;
    private Size   _size = Size.Empty;
    private Padding _margin  = new Padding(0, 1, 0, 2);
    private Padding _padding = new Padding(0);
    private ContentAlignment _imageAlign     = ContentAlignment.MiddleCenter;
    private ContentAlignment _textAlign      = ContentAlignment.MiddleCenter;
    private ToolStripItemDisplayStyle _displayStyle = ToolStripItemDisplayStyle.ImageAndText;
    private ToolStripItemImageScaling _imageScaling = ToolStripItemImageScaling.SizeToFit;
    private ToolStripItemAlignment    _alignment    = ToolStripItemAlignment.Left;
    private ToolStripItemOverflow     _overflow     = ToolStripItemOverflow.AsNeeded;
    private ToolStripTextDirection    _textDirection= ToolStripTextDirection.Horizontal;
    private TextImageRelation         _textImageRelation = TextImageRelation.ImageBeforeText;
    private bool _rightToLeft = false;
    private MergeAction _mergeAction = MergeAction.Append;
    private int _mergeIndex = -1;
    private int _imageIndex = -1;
    private string _imageKey  = string.Empty;
    private System.Drawing.Color _imageTransparentColor = System.Drawing.Color.Empty;
    private bool _autoSize   = true;
    private bool _autoToolTip = true;
    private bool _doubleClickEnabled = false;
    private bool _rightToLeftAutoMirrorImage = false;
    private bool _pressed  = false;
    private bool _isDisposed = false;

    // ── Identity ──────────────────────────────────────────────────────────────

    public string Name { get; set; } = string.Empty;
    public object? Tag  { get; set; }
    public string ToolTipText { get; set; } = string.Empty;

    // ── Accessibility (stubs) ─────────────────────────────────────────────────

    public AccessibleObject? AccessibilityObject    => null;            // stub
    public string AccessibleDefaultActionDescription { get; set; } = string.Empty;
    public string AccessibleDescription             { get; set; } = string.Empty;
    public string AccessibleName                    { get; set; } = string.Empty;
    public AccessibleRole AccessibleRole            { get; set; } = AccessibleRole.Default;

    // ── Text & Image ──────────────────────────────────────────────────────────

    public virtual string Text
    {
        get => _text;
        set
        {
            var newVal = value ?? string.Empty;
            if (_text == newVal) return;
            _text = newVal;
            TextChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }
    }

    public virtual Image? Image
    {
        get => _image;
        set { _image = value; Owner?.Invalidate(); }
    }

    public int ImageIndex
    {
        get => _imageIndex;
        set { _imageIndex = value; Owner?.Invalidate(); }
    }

    public string ImageKey
    {
        get => _imageKey;
        set { _imageKey = value ?? string.Empty; Owner?.Invalidate(); }
    }

    public System.Drawing.Color ImageTransparentColor
    {
        get => _imageTransparentColor;
        set { _imageTransparentColor = value; Owner?.Invalidate(); }
    }

    public ContentAlignment ImageAlign
    {
        get => _imageAlign;
        set { _imageAlign = value; Owner?.Invalidate(); }
    }

    public ToolStripItemImageScaling ImageScaling
    {
        get => _imageScaling;
        set { _imageScaling = value; Owner?.Invalidate(); }
    }

    public ContentAlignment TextAlign
    {
        get => _textAlign;
        set { _textAlign = value; Owner?.Invalidate(); }
    }

    public ToolStripTextDirection TextDirection
    {
        get => _textDirection;
        set { _textDirection = value; Owner?.Invalidate(); }
    }

    public TextImageRelation TextImageRelation
    {
        get => _textImageRelation;
        set { _textImageRelation = value; Owner?.Invalidate(); }
    }

    public ToolStripItemDisplayStyle DisplayStyle
    {
        get => _displayStyle;
        set
        {
            if (_displayStyle == value) return;
            _displayStyle = value;
            DisplayStyleChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }
    }

    // ── Colors & Font ─────────────────────────────────────────────────────────

    public virtual System.Drawing.Color ForeColor
    {
        get => _foreColor.IsEmpty ? (Owner?.ForeColor ?? System.Drawing.Color.Black) : _foreColor;
        set
        {
            if (_foreColor == value) return;
            _foreColor = value;
            ForeColorChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }
    }

    public virtual System.Drawing.Color BackColor
    {
        get => _backColor.IsEmpty ? (Owner?.BackColor ?? System.Drawing.Color.FromArgb(240, 240, 240)) : _backColor;
        set
        {
            if (_backColor == value) return;
            _backColor = value;
            BackColorChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }
    }

    public virtual Font Font
    {
        get => _font ?? Owner?.Font ?? new Font("Segoe UI", 9);
        set { _font = value; Owner?.Invalidate(); }
    }

    public Image? BackgroundImage { get; set; }
    public ImageLayout BackgroundImageLayout { get; set; } = ImageLayout.Tile;

    // ── Size & Layout ─────────────────────────────────────────────────────────

    public virtual Size Size
    {
        get => _size.IsEmpty ? new Size(Width, Height) : _size;
        set { _size = value; Owner?.Invalidate(); }
    }

    public virtual int Width
    {
        get => Bounds.Width;
        set { Bounds = new Rectangle(Bounds.X, Bounds.Y, value, Bounds.Height); }
    }

    public virtual int Height
    {
        get => Bounds.Height;
        set { Bounds = new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, value); }
    }

    public virtual Padding Margin
    {
        get => _margin;
        set { _margin = value; Owner?.Invalidate(); }
    }

    public virtual Padding Padding
    {
        get => _padding;
        set { _padding = value; Owner?.Invalidate(); }
    }

    public bool AutoSize
    {
        get => _autoSize;
        set { _autoSize = value; Owner?.Invalidate(); }
    }

    /// <summary>The computed bounds within the owner strip (set during layout).</summary>
    public Rectangle Bounds { get; internal set; }

    /// <summary>The content area inside the item's padding.</summary>
    public Rectangle ContentRectangle
        => new Rectangle(Padding.Left, Padding.Top,
                         Bounds.Width  - Padding.Horizontal,
                         Bounds.Height - Padding.Vertical);

    public Point Location => new Point(Bounds.X, Bounds.Y);

    // ── State ─────────────────────────────────────────────────────────────────

    public virtual bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;
            EnabledChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }
    }

    public virtual bool Visible
    {
        get => _visible;
        set
        {
            if (_visible == value) return;
            _visible = value;
            VisibleChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }
    }

    /// <summary>
    /// Whether this item is both Visible and Available (not overflowed off the strip).
    /// Setting Available also sets Visible.
    /// </summary>
    public bool Available
    {
        get => _available;
        set
        {
            if (_available == value) return;
            _available = value;
            Visible = value;
            AvailableChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool Selected  { get; internal set; }
    public bool Pressed   { get => _pressed;  internal set => _pressed = value; }
    public bool IsDisposed => _isDisposed;

    /// <summary>True when this item is on a dropdown (not a top-level strip).</summary>
    public bool IsOnDropDown => Owner is ToolStripDropDown;

    /// <summary>True when this item has been placed in the overflow area (stub — always false).</summary>
    public bool IsOnOverflow => Placement == ToolStripItemPlacement.Overflow;

    // ── Alignment & Overflow ──────────────────────────────────────────────────

    public ToolStripItemAlignment Alignment
    {
        get => _alignment;
        set { _alignment = value; Owner?.Invalidate(); }
    }

    public ToolStripItemOverflow Overflow
    {
        get => _overflow;
        set { _overflow = value; Owner?.Invalidate(); }
    }

    public ToolStripItemPlacement Placement { get; internal set; } = ToolStripItemPlacement.Main;

    // ── Misc behavior ─────────────────────────────────────────────────────────

    public bool AutoToolTip
    {
        get => _autoToolTip;
        set => _autoToolTip = value;
    }

    public bool DoubleClickEnabled
    {
        get => _doubleClickEnabled;
        set => _doubleClickEnabled = value;
    }

    public bool AllowDrop { get; set; } = false;

    public bool RightToLeft
    {
        get => _rightToLeft;
        set
        {
            if (_rightToLeft == value) return;
            _rightToLeft = value;
            RightToLeftChanged?.Invoke(this, EventArgs.Empty);
            Owner?.Invalidate();
        }
    }

    public bool RightToLeftAutoMirrorImage
    {
        get => _rightToLeftAutoMirrorImage;
        set { _rightToLeftAutoMirrorImage = value; Owner?.Invalidate(); }
    }

    // ── MDI / Merge ───────────────────────────────────────────────────────────

    public MergeAction MergeAction
    {
        get => _mergeAction;
        set => _mergeAction = value;
    }

    public int MergeIndex
    {
        get => _mergeIndex;
        set => _mergeIndex = value;
    }

    // ── Owner / Parent ────────────────────────────────────────────────────────

    /// <summary>The ToolStrip that owns this item (set when added to a collection).</summary>
    public ToolStrip? Owner { get; internal set; }

    /// <summary>
    /// The immediate parent container — may differ from Owner when the item
    /// is placed in an overflow strip.
    /// </summary>
    public ToolStrip? Parent => Owner;

    public ToolStrip? GetCurrentParent() => Owner;

    /// <summary>
    /// The ToolStripItem that owns this item when it lives inside a dropdown
    /// (e.g. the parent ToolStripMenuItem whose DropDown contains this item).
    /// Returns null for top-level items.
    /// </summary>
    public ToolStripItem? OwnerItem
        => (Owner is ToolStripDropDown dd) ? dd.SourceItem : null;

    // ── Events ────────────────────────────────────────────────────────────────

    public event EventHandler? Click;
    public event EventHandler? DoubleClick;
    public event EventHandler? VisibleChanged;
    public event EventHandler? AvailableChanged;
    public event EventHandler? EnabledChanged;
    public event EventHandler? BackColorChanged;
    public event EventHandler? ForeColorChanged;
    public event EventHandler? DisplayStyleChanged;
    public event EventHandler? RightToLeftChanged;
    public event EventHandler? TextChanged;
    public event EventHandler? LocationChanged;
    public event EventHandler? OwnerChanged;
    public event MouseEventHandler?  MouseDown;
    public event MouseEventHandler?  MouseUp;
    public event MouseEventHandler?  MouseMove;
    public event EventHandler?       MouseHover;
    public event EventHandler?       MouseEnter;
    public event EventHandler?       MouseLeave;
    public event PaintEventHandler?  Paint;
    public event KeyEventHandler?     KeyDown;     // stub — keyboard not routed to items
    public event KeyEventHandler?     KeyUp;       // stub
    public event KeyPressEventHandler? KeyPress;   // stub
    public event PreviewKeyDownEventHandler? PreviewKeyDown; // stub
    public event EventHandler? FontChanged;        // raised when Font changes
    public event EventHandler? SizeChanged;        // raised when Size changes
    public event EventHandler? PaddingChanged;     // raised when Padding changes
    public event EventHandler? MarginChanged;      // raised when Margin changes
    public event EventHandler? ParentChanged;      // raised when Owner/Parent changes
    public event DragEventHandler?   DragDrop;    // stub — drag not implemented
    public event DragEventHandler?   DragEnter;   // stub
    public event EventHandler?       DragLeave;   // stub
    public event DragEventHandler?   DragOver;    // stub
    public event GiveFeedbackEventHandler? GiveFeedback;      // stub
    public event QueryContinueDragEventHandler? QueryContinueDrag; // stub
    public event QueryAccessibilityHelpEventHandler? QueryAccessibilityHelp; // stub

    // ── Virtual event raisers ─────────────────────────────────────────────────

    protected internal virtual void OnClick(EventArgs e) => Click?.Invoke(this, e);
    protected virtual void OnDoubleClick(EventArgs e)    => DoubleClick?.Invoke(this, e);
    protected virtual void OnPaint(PaintEventArgs e)     => Paint?.Invoke(this, e);

    public virtual void OnMouseEnter(EventArgs e)
    {
        Selected = true;
        MouseEnter?.Invoke(this, e);
        Owner?.Invalidate();
    }

    public virtual void OnMouseLeave(EventArgs e)
    {
        Selected = false;
        MouseLeave?.Invoke(this, e);
        Owner?.Invalidate();
    }

    protected virtual void OnMouseHover(EventArgs e) => MouseHover?.Invoke(this, e);
    protected virtual void OnMouseDown(MouseEventArgs e) => MouseDown?.Invoke(this, e);
    protected virtual void OnMouseUp(MouseEventArgs e)   => MouseUp?.Invoke(this, e);
    protected virtual void OnMouseMove(MouseEventArgs e) => MouseMove?.Invoke(this, e);
    protected virtual void OnEnabledChanged(EventArgs e) => EnabledChanged?.Invoke(this, e);
    protected virtual void OnTextChanged(EventArgs e)    => TextChanged?.Invoke(this, e);
    protected virtual void OnOwnerChanged(EventArgs e)   => OwnerChanged?.Invoke(this, e);
    protected virtual void OnLocationChanged(EventArgs e) => LocationChanged?.Invoke(this, e);
    protected virtual void OnRightToLeftChanged(EventArgs e) => RightToLeftChanged?.Invoke(this, e);
    protected virtual void OnAvailableChanged(EventArgs e)   => AvailableChanged?.Invoke(this, e);
    protected virtual void OnVisibleChanged(EventArgs e)     => VisibleChanged?.Invoke(this, e);
    protected virtual void OnDisplayStyleChanged(EventArgs e) => DisplayStyleChanged?.Invoke(this, e);
    protected virtual void OnForeColorChanged(EventArgs e)   => ForeColorChanged?.Invoke(this, e);
    protected virtual void OnBackColorChanged(EventArgs e)   => BackColorChanged?.Invoke(this, e);
    protected virtual void OnFontChanged(EventArgs e)        => FontChanged?.Invoke(this, e);
    protected virtual void OnSizeChanged(EventArgs e)        => SizeChanged?.Invoke(this, e);
    protected virtual void OnPaddingChanged(EventArgs e)     => PaddingChanged?.Invoke(this, e);
    protected virtual void OnMarginChanged(EventArgs e)      => MarginChanged?.Invoke(this, e);
    protected virtual void OnParentChanged(ToolStrip? oldParent, ToolStrip? newParent)
        => ParentChanged?.Invoke(this, EventArgs.Empty);
    protected virtual void OnKeyDown(KeyEventArgs e)         => KeyDown?.Invoke(this, e);
    protected virtual void OnKeyUp(KeyEventArgs e)           => KeyUp?.Invoke(this, e);
    protected virtual void OnKeyPress(KeyPressEventArgs e)   => KeyPress?.Invoke(this, e);
    protected virtual void OnPreviewKeyDown(PreviewKeyDownEventArgs e) => PreviewKeyDown?.Invoke(this, e);

    // ── Misc methods ──────────────────────────────────────────────────────────

    public void PerformClick() => OnClick(EventArgs.Empty);

    /// <summary>Invalidates the owner strip, causing it to repaint.</summary>
    public void Invalidate() => Owner?.Invalidate();

    /// <summary>Requests focus on this item (stub — focuses the owner strip).</summary>
    public void Select() { /* stub */ }

    /// <summary>Returns the preferred size of this item given a constraining size.</summary>
    public virtual Size GetPreferredSize(Size constrainingSize)
        => new Size(constrainingSize.Width, 22); // sensible default

    public override string ToString() => $"{GetType().Name}: {Text}";

    // ── IDisposable ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        _isDisposed = true;
    }
}

// ── ToolStripSeparator ────────────────────────────────────────────────────────
/// <summary>A horizontal rule separator item inside a menu or toolbar.</summary>
public class ToolStripSeparator : ToolStripItem
{
    public ToolStripSeparator()
    {
        Text    = "-";
        Enabled = false;
    }
}

// ── ToolStripItemCollection ───────────────────────────────────────────────────
/// <summary>
/// An ordered, observable collection of ToolStripItems owned by a strip or dropdown.
/// </summary>
public class ToolStripItemCollection : IList<ToolStripItem>
{
    private readonly List<ToolStripItem> _items = new();
    private readonly ToolStrip?          _owner;

    public ToolStripItemCollection(ToolStrip? owner) => _owner = owner;

    public ToolStripItem this[int index]
    {
        get => _items[index];
        set { _items[index] = value; Attach(value); }
    }

    public ToolStripItem? this[string key]
        => _items.Find(i => string.Equals(i.Name, key, StringComparison.Ordinal));

    public int  Count      => _items.Count;
    public bool IsReadOnly => false;

    public void Add(ToolStripItem item)
    {
        Attach(item);
        _items.Add(item);
        _owner?.Invalidate();
    }

    public ToolStripMenuItem Add(string text)
    {
        var item = new ToolStripMenuItem { Text = text };
        Add(item);
        return item;
    }

    public ToolStripMenuItem Add(string text, Image? image)
    {
        var item = new ToolStripMenuItem(text, image);
        Add(item);
        return item;
    }

    public ToolStripMenuItem Add(string text, Image? image, EventHandler onClick)
    {
        var item = new ToolStripMenuItem(text, image, onClick);
        Add(item);
        return item;
    }

    public void AddRange(IEnumerable<ToolStripItem> items)
    {
        foreach (var i in items) Add(i);
    }

    public void AddRange(ToolStripItem[] items) => AddRange((IEnumerable<ToolStripItem>)items);

    /// <summary>
    /// Finds all items whose <see cref="ToolStripItem.Name"/> matches <paramref name="key"/>.
    /// When <paramref name="searchAllChildren"/> is true the search recurses into dropdown items.
    /// </summary>
    public ToolStripItem[] Find(string key, bool searchAllChildren)
    {
        var result = new List<ToolStripItem>();
        FindRecursive(key, searchAllChildren, result);
        return result.ToArray();
    }

    private void FindRecursive(string key, bool recurse, List<ToolStripItem> result)
    {
        foreach (var item in _items)
        {
            if (string.Equals(item.Name, key, StringComparison.OrdinalIgnoreCase))
                result.Add(item);
            if (recurse && item is ToolStripMenuItem mi && mi.HasDropDownItems)
                mi.DropDownItems.FindRecursive(key, true, result);
        }
    }

    public void Insert(int index, ToolStripItem item) { Attach(item); _items.Insert(index, item); _owner?.Invalidate(); }
    public bool Remove(ToolStripItem item)            { var r = _items.Remove(item); if (r) _owner?.Invalidate(); return r; }
    public void RemoveAt(int index)                   { _items.RemoveAt(index); _owner?.Invalidate(); }
    public void Clear()                               { _items.Clear(); _owner?.Invalidate(); }
    public bool Contains(ToolStripItem item)          => _items.Contains(item);
    public int  IndexOf(ToolStripItem item)           => _items.IndexOf(item);
    public void CopyTo(ToolStripItem[] array, int i)  => _items.CopyTo(array, i);

    public IEnumerator<ToolStripItem> GetEnumerator() => _items.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();

    private void Attach(ToolStripItem item) { if (_owner != null) item.Owner = _owner; }
}

