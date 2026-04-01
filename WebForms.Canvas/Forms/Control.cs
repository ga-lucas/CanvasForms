using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public abstract class Control
{
    private Control? _parent;
    private readonly List<Control> _controls = new();
    private string _text = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                Invalidate();
            }
        }
    }

    private int _left;
    private int _top;
    private int _width = 100;
    private int _height = 20;
    private DockStyle _dock = DockStyle.None;
    private AnchorStyles _anchor = AnchorStyles.Top | AnchorStyles.Left;

    // Original bounds before docking/anchoring (for anchor calculations)
    internal int OriginalLeft;
    internal int OriginalTop;
    internal int OriginalWidth;
    internal int OriginalHeight;
    internal int OriginalParentWidth;
    internal int OriginalParentHeight;
    internal bool OriginalBoundsSet = false;

    public int Left 
    { 
        get => _left;
        set
        {
            if (_left != value)
            {
                _left = value;
                Invalidate();
            }
        }
    }

    public int Top 
    { 
        get => _top;
        set
        {
            if (_top != value)
            {
                _top = value;
                Invalidate();
            }
        }
    }

    public int Width 
    { 
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                Invalidate();
            }
        }
    }

    public int Height 
    { 
        get => _height;
        set
        {
            if (_height != value)
            {
                _height = value;
                Invalidate();
            }
        }
    }

    public DockStyle Dock
    {
        get => _dock;
        set
        {
            if (_dock != value)
            {
                _dock = value;
                _parent?.PerformLayout();
                Invalidate();
            }
        }
    }

    public AnchorStyles Anchor
    {
        get => _anchor;
        set
        {
            if (_anchor != value)
            {
                _anchor = value;
                Invalidate();
            }
        }
    }

    public Color BackColor { get; set; } = Color.White;
    public Color ForeColor { get; set; } = Color.Black;
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public object? Tag { get; set; }

    // Calculated position properties
    public int Right => Left + Width;
    public int Bottom => Top + Height;

    // Client area properties
    public Rectangle ClientRectangle => new Rectangle(0, 0, Width, Height);
    public Size ClientSize
    {
        get => new Size(Width, Height);
        set { Width = value.Width; Height = value.Height; }
    }

    public Rectangle DisplayRectangle => ClientRectangle;

    // Font properties
    private Font? _font;
    public Font Font
    {
        get => _font ?? DefaultFont;
        set
        {
            if (_font != value)
            {
                _font = value;
                Invalidate();
            }
        }
    }

    public int FontHeight => Font.Height;

    // Size constraints
    private Size _minimumSize = Size.Empty;
    private Size _maximumSize = Size.Empty;
    private Size _margin = new Size(3, 3);
    private Size _padding = Size.Empty;

    public Size MinimumSize
    {
        get => _minimumSize;
        set
        {
            if (_minimumSize != value)
            {
                _minimumSize = value;
                Invalidate();
            }
        }
    }

    public Size MaximumSize
    {
        get => _maximumSize;
        set
        {
            if (_maximumSize != value)
            {
                _maximumSize = value;
                Invalidate();
            }
        }
    }

    public Size Margin
    {
        get => _margin;
        set
        {
            if (_margin != value)
            {
                _margin = value;
                Invalidate();
            }
        }
    }

    public Size Padding
    {
        get => _padding;
        set
        {
            if (_padding != value)
            {
                _padding = value;
                Invalidate();
            }
        }
    }

    // Tab and focus properties
    private int _tabIndex = 0;
    private bool _tabStop = true;

    public int TabIndex
    {
        get => _tabIndex;
        set
        {
            if (_tabIndex != value)
            {
                _tabIndex = value;
            }
        }
    }

    public bool TabStop
    {
        get => _tabStop;
        set
        {
            if (_tabStop != value)
            {
                _tabStop = value;
            }
        }
    }

    public bool Focused { get; internal set; }
    public bool CanFocus => Visible && Enabled && TabStop;
    public bool CanSelect => CanFocus;
    public bool ContainsFocus => Focused || _controls.Any(c => c.ContainsFocus);

    // Child controls
    public bool HasChildren => _controls.Count > 0;

    // State properties
    private bool _isDisposed = false;
    public bool IsDisposed => _isDisposed;
    public bool Disposing { get; private set; }

    // Auto size and scroll
    public bool AutoSize { get; set; } = false;
    public Point AutoScrollOffset { get; set; } = Point.Empty;

    // Background image
    private Image? _backgroundImage;
    private ImageLayout _backgroundImageLayout = ImageLayout.Tile;

    public Image? BackgroundImage
    {
        get => _backgroundImage;
        set
        {
            if (_backgroundImage != value)
            {
                _backgroundImage = value;
                Invalidate();
            }
        }
    }

    public ImageLayout BackgroundImageLayout
    {
        get => _backgroundImageLayout;
        set
        {
            if (_backgroundImageLayout != value)
            {
                _backgroundImageLayout = value;
                Invalidate();
            }
        }
    }

    // Validation
    public bool CausesValidation { get; set; } = true;

    // Drag and drop
    public bool AllowDrop { get; set; } = false;

    // Cursor
    private Cursor? _cursor;
    public Cursor Cursor
    {
        get => _cursor ?? DefaultCursor;
        set
        {
            if (_cursor != value)
            {
                _cursor = value;
            }
        }
    }

    public bool UseWaitCursor { get; set; } = false;

    // Right to left
    public bool RightToLeft { get; set; } = false;

    // Region
    private Region? _region;
    public Region? Region
    {
        get => _region;
        set
        {
            if (_region != value)
            {
                _region = value;
                Invalidate();
            }
        }
    }

    // Mirroring
    public bool IsMirrored { get; protected set; } = false;

    // Accessibility
    private AccessibleObject? _accessibilityObject;
    private string? _accessibleName;
    private string? _accessibleDescription;
    private string? _accessibleDefaultActionDescription;
    private AccessibleRole _accessibleRole = AccessibleRole.Default;
    private bool _isAccessible = false;

    public AccessibleObject? AccessibilityObject
    {
        get => _accessibilityObject;
    }

    public string? AccessibleName
    {
        get => _accessibleName;
        set => _accessibleName = value;
    }

    public string? AccessibleDescription
    {
        get => _accessibleDescription;
        set => _accessibleDescription = value;
    }

    public string? AccessibleDefaultActionDescription
    {
        get => _accessibleDefaultActionDescription;
        set => _accessibleDefaultActionDescription = value;
    }

    public AccessibleRole AccessibleRole
    {
        get => _accessibleRole;
        set => _accessibleRole = value;
    }

    public bool IsAccessible
    {
        get => _isAccessible;
        set => _isAccessible = value;
    }

    // Handle-related properties (stub implementations for canvas-based controls)
    public IntPtr Handle { get; private set; } = IntPtr.Zero;
    public bool IsHandleCreated => Handle != IntPtr.Zero;
    public bool Created => IsHandleCreated;
    public bool RecreatingHandle { get; private set; } = false;

    // Mouse capture
    public bool Capture { get; set; } = false;

    // Painting optimizations
    public bool DoubleBuffered { get; set; } = false;
    public bool ResizeRedraw { get; set; } = false;

    // DPI
    private int _deviceDpi = 96;
    public int DeviceDpi => _deviceDpi;

    // Hierarchy
    public Control? TopLevelControl
    {
        get
        {
            var control = this;
            while (control.Parent != null)
            {
                control = control.Parent;
            }
            return control;
        }
    }

    // Context menus (obsolete/stub)
    [Obsolete("Use ContextMenuStrip instead")]
    public object? ContextMenu { get; set; }
    public object? ContextMenuStrip { get; set; }

    // Data binding (stubs)
    public object? BindingContext { get; set; }
    public object? DataBindings { get; private set; }
    public object? DataContext { get; set; }

    // Site (for design-time support)
    public object? Site { get; set; }
    public bool IsAncestorSiteInDesignMode { get; protected set; } = false;

    // IME support (stubs)
    public ImeMode ImeMode { get; set; } = ImeMode.NoControl;
    public ImeMode ImeModeBase
    {
        get => ImeMode;
        set => ImeMode = value;
    }
    public bool CanEnableIme => false;
    public ImeMode PropagatingImeMode => ImeMode;

    // Layout
    public object? LayoutEngine { get; protected set; }

    // Static input state properties
    public static Keys ModifierKeys { get; internal set; } = Keys.None;
    public static MouseButtons MouseButtons { get; internal set; } = MouseButtons.None;
    public static Point MousePosition { get; internal set; } = Point.Empty;

    // Thread safety (stubs for canvas-based controls)
    public bool InvokeRequired => false;
    public static bool CheckForIllegalCrossThreadCalls { get; set; } = false;

    // Events raising capability
    public bool CanRaiseEvents => true;

    // UI state
    public bool ShowFocusCues => true;
    public bool ShowKeyboardCues => true;
    public bool ScaleChildren => true;

    // Preferred size
    public Size PreferredSize => GetPreferredSize(Size.Empty);

    protected virtual Size GetPreferredSize(Size proposedSize)
    {
        return Size;
    }

    // Assembly info
    public string ProductName => "WebForms Canvas";
    public string ProductVersion => "1.0.0";
    public string CompanyName => "WebForms Canvas";

    // Create params (stub)
    public virtual object? CreateParams => null;

    // Obsolete properties
    [Obsolete("This property is obsolete")]
    public bool RenderRightToLeft => false;

    [Obsolete("This property is not relevant for this class")]
    public object? WindowTarget { get; set; }

    // Default static properties
    public static Color DefaultBackColor => Color.White;
    public static Color DefaultForeColor => Color.Black;
    public static Font DefaultFont => new Font("Segoe UI", 9);
    public static Cursor DefaultCursor => Cursor.Default;
    public static ImeMode DefaultImeMode => ImeMode.NoControl;
    public static Size DefaultMargin => new Size(3, 3);
    public static Size DefaultMaximumSize => Size.Empty;
    public static Size DefaultMinimumSize => Size.Empty;
    public static Size DefaultPadding => Size.Empty;
    public virtual Size DefaultSize => new Size(100, 20);

    // Location and Size helpers
    public Point Location
    {
        get => new Point(Left, Top);
        set { Left = value.X; Top = value.Y; }
    }

    public Size Size
    {
        get => new Size(Width, Height);
        set { Width = value.Width; Height = value.Height; }
    }

    public Rectangle Bounds
    {
        get => new Rectangle(Left, Top, Width, Height);
        set { Left = value.X; Top = value.Y; Width = value.Width; Height = value.Height; }
    }

    // Parent/child relationships
    public Control? Parent
    {
        get => _parent;
        internal set
        {
            if (_parent != value)
            {
                _parent = value;
                OnParentChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Raised when the control's Parent property value changes
    /// </summary>
    public event EventHandler? ParentChanged;

    /// <summary>
    /// Called when the Parent property changes
    /// </summary>
    protected virtual void OnParentChanged(EventArgs e)
    {
        ParentChanged?.Invoke(this, e);
    }

    public ControlCollection Controls => new ControlCollection(this, _controls);

    // Paint events
    public event PaintEventHandler? Paint;

    // Mouse events
    public event MouseEventHandler? MouseDown;
    public event MouseEventHandler? MouseUp;
    public event MouseEventHandler? MouseMove;
    public event MouseEventHandler? MouseClick;
    public event MouseEventHandler? MouseDoubleClick;
    public event MouseEventHandler? MouseEnter;
    public event MouseEventHandler? MouseLeave;

    // Keyboard events
    public event KeyEventHandler? KeyDown;
    public event KeyEventHandler? KeyUp;
    public event KeyPressEventHandler? KeyPress;

    // Focus events
    public event EventHandler? GotFocus;
    public event EventHandler? LostFocus;
    public event EventHandler? Enter;
    public event EventHandler? Leave;

    protected internal virtual void OnPaint(PaintEventArgs e)
    {
        Paint?.Invoke(this, e);
    }

    protected internal virtual void OnMouseDown(MouseEventArgs e)
    {
        MouseDown?.Invoke(this, e);
    }

    protected internal virtual void OnMouseUp(MouseEventArgs e)
    {
        MouseUp?.Invoke(this, e);
    }

    protected internal virtual void OnMouseMove(MouseEventArgs e)
    {
        MouseMove?.Invoke(this, e);
    }

    protected internal virtual void OnMouseClick(MouseEventArgs e)
    {
        MouseClick?.Invoke(this, e);
    }

    protected internal virtual void OnMouseDoubleClick(MouseEventArgs e)
    {
        MouseDoubleClick?.Invoke(this, e);
    }

    protected internal virtual void OnMouseEnter(EventArgs e)
    {
        MouseEnter?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, 0, 0));
    }

    protected internal virtual void OnMouseLeave(EventArgs e)
    {
        MouseLeave?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, 0, 0));
    }

    protected internal virtual void OnKeyDown(KeyEventArgs e)
    {
        // Handle Tab key for focus navigation
        if (e.KeyCode == Keys.Tab && !e.Handled)
        {
            if (ProcessTabKey(!e.Shift))
            {
                e.Handled = true;
                return;
            }
        }

        KeyDown?.Invoke(this, e);
    }

    protected internal virtual void OnKeyUp(KeyEventArgs e)
    {
        KeyUp?.Invoke(this, e);
    }

    protected internal virtual void OnKeyPress(KeyPressEventArgs e)
    {
        KeyPress?.Invoke(this, e);
    }

    protected internal virtual void OnGotFocus(EventArgs e)
    {
        GotFocus?.Invoke(this, e);
    }

    protected internal virtual void OnLostFocus(EventArgs e)
    {
        LostFocus?.Invoke(this, e);
    }

    protected internal virtual void OnEnter(EventArgs e)
    {
        Enter?.Invoke(this, e);
    }

    protected internal virtual void OnLeave(EventArgs e)
    {
        Leave?.Invoke(this, e);
    }

    /// <summary>
    /// Sets input focus to the control
    /// </summary>
    /// <returns>true if focus was set successfully; otherwise, false</returns>
    public bool Focus()
    {
        if (!CanFocus)
            return false;

        // Get the top-level control (form)
        var topLevel = TopLevelControl;
        if (topLevel == null)
            topLevel = this;

        // Remove focus from currently focused control
        var currentlyFocused = FindFocusedControl(topLevel);
        if (currentlyFocused != null && currentlyFocused != this)
        {
            currentlyFocused.Focused = false;
            currentlyFocused.OnLostFocus(EventArgs.Empty);
            currentlyFocused.OnLeave(EventArgs.Empty);
        }

        // Set focus to this control
        Focused = true;
        OnEnter(EventArgs.Empty);
        OnGotFocus(EventArgs.Empty);

        return true;
    }

    /// <summary>
    /// Activates the control
    /// </summary>
    public void Select()
    {
        Focus();
    }

    /// <summary>
    /// Selects the next control in tab order
    /// </summary>
    /// <param name="forward">true to move forward; false to move backward</param>
    /// <returns>true if the next control was selected; otherwise, false</returns>
    public bool SelectNextControl(Control? ctl, bool forward, bool tabStopOnly, bool nested, bool wrap)
    {
        if (ctl == null)
            ctl = this;

        var controls = GetTabOrderedControls(this, nested);

        if (controls.Count == 0)
            return false;

        // Find current control index
        int currentIndex = controls.IndexOf(ctl);

        // If control not found, start from beginning
        if (currentIndex == -1)
            currentIndex = forward ? -1 : controls.Count;

        // Search for next focusable control
        int step = forward ? 1 : -1;
        int index = currentIndex + step;
        int attempts = 0;

        while (attempts < controls.Count)
        {
            // Wrap around if needed
            if (index >= controls.Count)
            {
                if (wrap)
                    index = 0;
                else
                    return false;
            }
            else if (index < 0)
            {
                if (wrap)
                    index = controls.Count - 1;
                else
                    return false;
            }

            var nextControl = controls[index];

            // Check if control can receive focus
            if ((!tabStopOnly || nextControl.TabStop) && nextControl.CanFocus)
            {
                return nextControl.Focus();
            }

            index += step;
            attempts++;
        }

        return false;
    }

    /// <summary>
    /// Gets all controls in tab order
    /// </summary>
    private List<Control> GetTabOrderedControls(Control parent, bool nested)
    {
        var controls = new List<Control>();

        void AddControlsRecursive(Control container)
        {
            var sortedControls = container._controls
                .Where(c => c.Visible)
                .OrderBy(c => c.TabIndex)
                .ThenBy(c => container._controls.IndexOf(c));

            foreach (var control in sortedControls)
            {
                // If nested navigation is enabled and the control has children,
                // recurse into children instead of adding the container itself
                if (nested && control.HasChildren)
                {
                    AddControlsRecursive(control);
                }
                else
                {
                    // Add leaf controls (controls without children)
                    controls.Add(control);
                }
            }
        }

        AddControlsRecursive(parent);
        return controls;
    }

    /// <summary>
    /// Finds the currently focused control in the hierarchy
    /// </summary>
    private static Control? FindFocusedControl(Control root)
    {
        if (root.Focused)
            return root;

        foreach (var child in root._controls)
        {
            var focused = FindFocusedControl(child);
            if (focused != null)
                return focused;
        }

        return null;
    }

    /// <summary>
    /// Processes a Tab key press for focus navigation
    /// </summary>
    /// <param name="forward">true for Tab, false for Shift+Tab</param>
    /// <returns>true if the key was processed; otherwise, false</returns>
    protected virtual bool ProcessTabKey(bool forward)
    {
        var topLevel = TopLevelControl ?? this;
        return topLevel.SelectNextControl(this, forward, tabStopOnly: true, nested: true, wrap: true);
    }

    public void Invalidate()
    {
        // Async fire-and-forget - render will happen asynchronously
        var task = RequestRender?.Invoke();
    }

    /// <summary>
    /// Performs layout logic for docked and anchored controls
    /// </summary>
    public virtual void PerformLayout()
    {
        if (_controls.Count == 0) return;

        // First, store original bounds for anchored controls
        foreach (var control in _controls)
        {
            if (!control.OriginalBoundsSet && control.Dock == DockStyle.None)
            {
                control.OriginalLeft = control.Left;
                control.OriginalTop = control.Top;
                control.OriginalWidth = control.Width;
                control.OriginalHeight = control.Height;
                control.OriginalParentWidth = this.Width;
                control.OriginalParentHeight = this.Height;
                control.OriginalBoundsSet = true;
            }
        }

        // Available client area
        var clientRect = new Rectangle(0, 0, Width, Height);

        // Process docked controls in order: Top, Bottom, Left, Right, then Fill
        var dockedControls = _controls.Where(c => c.Visible && c.Dock != DockStyle.None).ToList();
        var anchoredControls = _controls.Where(c => c.Visible && c.Dock == DockStyle.None).ToList();

        // Apply docking in priority order
        foreach (var dockStyle in new[] { DockStyle.Top, DockStyle.Bottom, DockStyle.Left, DockStyle.Right, DockStyle.Fill })
        {
            foreach (var control in dockedControls.Where(c => c.Dock == dockStyle))
            {
                switch (control.Dock)
                {
                    case DockStyle.Top:
                        control.Left = clientRect.X;
                        control.Top = clientRect.Y;
                        control.Width = clientRect.Width;
                        // Height stays as set by user
                        clientRect.Y += control.Height;
                        clientRect.Height -= control.Height;
                        break;

                    case DockStyle.Bottom:
                        control.Left = clientRect.X;
                        control.Width = clientRect.Width;
                        clientRect.Height -= control.Height;
                        control.Top = clientRect.Y + clientRect.Height;
                        break;

                    case DockStyle.Left:
                        control.Left = clientRect.X;
                        control.Top = clientRect.Y;
                        control.Height = clientRect.Height;
                        // Width stays as set by user
                        clientRect.X += control.Width;
                        clientRect.Width -= control.Width;
                        break;

                    case DockStyle.Right:
                        control.Top = clientRect.Y;
                        control.Height = clientRect.Height;
                        clientRect.Width -= control.Width;
                        control.Left = clientRect.X + clientRect.Width;
                        break;

                    case DockStyle.Fill:
                        control.Left = clientRect.X;
                        control.Top = clientRect.Y;
                        control.Width = clientRect.Width;
                        control.Height = clientRect.Height;
                        break;
                }
            }
        }

        // Apply anchoring to non-docked controls
        foreach (var control in anchoredControls)
        {
            if (!control.OriginalBoundsSet) continue;

            var anchor = control.Anchor;
            var deltaWidth = Width - control.OriginalParentWidth;
            var deltaHeight = Height - control.OriginalParentHeight;

            // Calculate new position and size based on anchoring
            var left = control.OriginalLeft;
            var top = control.OriginalTop;
            var width = control.OriginalWidth;
            var height = control.OriginalHeight;

            bool anchoredLeft = (anchor & AnchorStyles.Left) != 0;
            bool anchoredRight = (anchor & AnchorStyles.Right) != 0;
            bool anchoredTop = (anchor & AnchorStyles.Top) != 0;
            bool anchoredBottom = (anchor & AnchorStyles.Bottom) != 0;

            if (anchoredLeft && anchoredRight)
            {
                // Stretch horizontally
                width = control.OriginalWidth + deltaWidth;
            }
            else if (anchoredRight && !anchoredLeft)
            {
                // Move with right edge
                left = control.OriginalLeft + deltaWidth;
            }
            // else if only left is anchored (default), position stays the same

            if (anchoredTop && anchoredBottom)
            {
                // Stretch vertically
                height = control.OriginalHeight + deltaHeight;
            }
            else if (anchoredBottom && !anchoredTop)
            {
                // Move with bottom edge
                top = control.OriginalTop + deltaHeight;
            }
            // else if only top is anchored (default), position stays the same

            control.Left = left;
            control.Top = top;
            control.Width = width;
            control.Height = height;
        }

        Invalidate();
    }

    internal Func<Task>? RequestRender { get; set; }

    // Propagate RequestRender to all children
    internal void PropagateRequestRender(Func<Task>? requestRender)
    {
        RequestRender = requestRender;
        foreach (var child in _controls)
        {
            child.PropagateRequestRender(requestRender);
        }
    }
}

// Control collection for managing child controls
public class ControlCollection : IEnumerable<Control>
{
    private readonly Control _owner;
    private readonly List<Control> _list;

    internal ControlCollection(Control owner, List<Control> list)
    {
        _owner = owner;
        _list = list;
    }

    public int Count => _list.Count;

    public Control this[int index] => _list[index];

    public void Add(Control control)
    {
        control.Parent = _owner;
        control.RequestRender = _owner.RequestRender;
        _list.Add(control);
        _owner.Invalidate();
    }

    public void Remove(Control control)
    {
        if (_list.Remove(control))
        {
            control.Parent = null;
            _owner.Invalidate();
        }
    }

    public void Clear()
    {
        foreach (var control in _list)
        {
            control.Parent = null;
        }
        _list.Clear();
        _owner.Invalidate();
    }

    public IEnumerator<Control> GetEnumerator() => _list.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
