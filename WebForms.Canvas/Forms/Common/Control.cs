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

    /// <summary>
    /// Gets the width available for laying out child controls.
    /// Override in derived classes to account for chrome (e.g., title bar in Forms).
    /// </summary>
    protected virtual int LayoutWidth => Width;

    /// <summary>
    /// Gets the height available for laying out child controls.
    /// Override in derived classes to account for chrome (e.g., title bar in Forms).
    /// </summary>
    protected virtual int LayoutHeight => Height;

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

    // Control styles
    private ControlStyles _controlStyles = ControlStyles.None;

    /// <summary>
    /// Sets a specified ControlStyles flag to either true or false
    /// </summary>
    protected void SetStyle(ControlStyles flag, bool value)
    {
        if (value)
            _controlStyles |= flag;
        else
            _controlStyles &= ~flag;
    }

    /// <summary>
    /// Gets the value of the specified control style bit
    /// </summary>
    protected bool GetStyle(ControlStyles flag)
    {
        return (_controlStyles & flag) == flag;
    }

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

    // ========== EVENTS ==========

    // Paint events
    public event PaintEventHandler? Paint;

    // Click events (in addition to MouseClick)
    public event EventHandler? Click;
    public event EventHandler? DoubleClick;

    // Mouse events
    public event MouseEventHandler? MouseDown;
    public event MouseEventHandler? MouseUp;
    public event MouseEventHandler? MouseMove;
    public event MouseEventHandler? MouseClick;
    public event MouseEventHandler? MouseDoubleClick;
    public event EventHandler? MouseEnter;
    public event EventHandler? MouseLeave;
    public event MouseEventHandler? MouseHover;
    public event MouseEventHandler? MouseWheel;
    public event EventHandler? MouseCaptureChanged;

    // Keyboard events
    public event KeyEventHandler? KeyDown;
    public event KeyEventHandler? KeyUp;
    public event KeyPressEventHandler? KeyPress;
    public event PreviewKeyDownEventHandler? PreviewKeyDown;

    // Focus events
    public event EventHandler? GotFocus;
    public event EventHandler? LostFocus;
    public event EventHandler? Enter;
    public event EventHandler? Leave;
    public event EventHandler? Validated;
    public event CancelEventHandler? Validating;

    // Layout events
    public event EventHandler? Layout;
    public event EventHandler? Resize;
    public event EventHandler? SizeChanged;
    public event EventHandler? LocationChanged;
    public event EventHandler? Move;

    // Property changed events
    public event EventHandler? TextChanged;
    public event EventHandler? VisibleChanged;
    public event EventHandler? EnabledChanged;
    public event EventHandler? BackColorChanged;
    public event EventHandler? ForeColorChanged;
    public event EventHandler? FontChanged;
    public event EventHandler? TabIndexChanged;
    public event EventHandler? TabStopChanged;
    public event EventHandler? RightToLeftChanged;
    public event EventHandler? CursorChanged;
    public event EventHandler? RegionChanged;
    public event EventHandler? MarginChanged;
    public event EventHandler? PaddingChanged;
    public event EventHandler? DockChanged;
    public event EventHandler? BackgroundImageChanged;
    public event EventHandler? BackgroundImageLayoutChanged;
    public event EventHandler? ControlAdded;
    public event EventHandler? ControlRemoved;

    // Drag and drop events
    public event DragEventHandler? DragDrop;
    public event DragEventHandler? DragEnter;
    public event EventHandler? DragLeave;
    public event DragEventHandler? DragOver;
    public event GiveFeedbackEventHandler? GiveFeedback;
    public event QueryContinueDragEventHandler? QueryContinueDrag;

    // Help events
    public event HelpEventHandler? HelpRequested;

    // Context menu events
    public event EventHandler? ContextMenuStripChanged;

    // Change events
    public event EventHandler? ChangeUICues;
    public event EventHandler? ImeModeChanged;
    public event EventHandler? StyleChanged;
    public event EventHandler? SystemColorsChanged;

    // Query events
    public event QueryAccessibilityHelpEventHandler? QueryAccessibilityHelp;

    // Cause validation events
    public event EventHandler? CausesValidationChanged;

    // Client size changed
    public event EventHandler? ClientSizeChanged;

    // Invalidated
    public event InvalidateEventHandler? Invalidated;

    // Handle events
    public event EventHandler? HandleCreated;
    public event EventHandler? HandleDestroyed;

    // Auto size changed
    public event EventHandler? AutoSizeChanged;

    // DPI changed
    public event EventHandler? DpiChanged;
    public event EventHandler? DpiChangedBeforeParent;
    public event EventHandler? DpiChangedAfterParent;

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

    // ========== ADDITIONAL EVENT HANDLERS ==========

    protected virtual void OnClick(EventArgs e)
    {
        Click?.Invoke(this, e);
    }

    protected virtual void OnDoubleClick(EventArgs e)
    {
        DoubleClick?.Invoke(this, e);
    }

    protected virtual void OnMouseHover(EventArgs e)
    {
        MouseHover?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, 0, 0));
    }

    protected virtual void OnMouseWheel(MouseEventArgs e)
    {
        MouseWheel?.Invoke(this, e);
    }

    protected virtual void OnMouseCaptureChanged(EventArgs e)
    {
        MouseCaptureChanged?.Invoke(this, e);
    }

    protected virtual void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
    {
        PreviewKeyDown?.Invoke(this, e);
    }

    protected virtual void OnValidated(EventArgs e)
    {
        Validated?.Invoke(this, e);
    }

    protected virtual void OnValidating(CancelEventArgs e)
    {
        Validating?.Invoke(this, e);
    }

    protected virtual void OnLayout(LayoutEventArgs e)
    {
        Layout?.Invoke(this, e);
    }

    protected virtual void OnResize(EventArgs e)
    {
        Resize?.Invoke(this, e);
        OnSizeChanged(e);
    }

    protected virtual void OnSizeChanged(EventArgs e)
    {
        SizeChanged?.Invoke(this, e);
    }

    protected virtual void OnLocationChanged(EventArgs e)
    {
        LocationChanged?.Invoke(this, e);
        OnMove(e);
    }

    protected virtual void OnMove(EventArgs e)
    {
        Move?.Invoke(this, e);
    }

    protected virtual void OnTextChanged(EventArgs e)
    {
        TextChanged?.Invoke(this, e);
    }

    protected virtual void OnVisibleChanged(EventArgs e)
    {
        VisibleChanged?.Invoke(this, e);
    }

    protected virtual void OnEnabledChanged(EventArgs e)
    {
        EnabledChanged?.Invoke(this, e);
    }

    protected virtual void OnBackColorChanged(EventArgs e)
    {
        BackColorChanged?.Invoke(this, e);
    }

    protected virtual void OnForeColorChanged(EventArgs e)
    {
        ForeColorChanged?.Invoke(this, e);
    }

    protected virtual void OnFontChanged(EventArgs e)
    {
        FontChanged?.Invoke(this, e);
    }

    protected virtual void OnTabIndexChanged(EventArgs e)
    {
        TabIndexChanged?.Invoke(this, e);
    }

    protected virtual void OnTabStopChanged(EventArgs e)
    {
        TabStopChanged?.Invoke(this, e);
    }

    protected virtual void OnRightToLeftChanged(EventArgs e)
    {
        RightToLeftChanged?.Invoke(this, e);
    }

    protected virtual void OnCursorChanged(EventArgs e)
    {
        CursorChanged?.Invoke(this, e);
    }

    protected virtual void OnRegionChanged(EventArgs e)
    {
        RegionChanged?.Invoke(this, e);
    }

    protected virtual void OnMarginChanged(EventArgs e)
    {
        MarginChanged?.Invoke(this, e);
    }

    protected virtual void OnPaddingChanged(EventArgs e)
    {
        PaddingChanged?.Invoke(this, e);
    }

    protected virtual void OnDockChanged(EventArgs e)
    {
        DockChanged?.Invoke(this, e);
    }

    protected virtual void OnBackgroundImageChanged(EventArgs e)
    {
        BackgroundImageChanged?.Invoke(this, e);
    }

    protected virtual void OnBackgroundImageLayoutChanged(EventArgs e)
    {
        BackgroundImageLayoutChanged?.Invoke(this, e);
    }

    protected virtual void OnControlAdded(ControlEventArgs e)
    {
        ControlAdded?.Invoke(this, e);
    }

    protected virtual void OnControlRemoved(ControlEventArgs e)
    {
        ControlRemoved?.Invoke(this, e);
    }

    protected virtual void OnDragDrop(DragEventArgs e)
    {
        DragDrop?.Invoke(this, e);
    }

    protected virtual void OnDragEnter(DragEventArgs e)
    {
        DragEnter?.Invoke(this, e);
    }

    protected virtual void OnDragLeave(EventArgs e)
    {
        DragLeave?.Invoke(this, e);
    }

    protected virtual void OnDragOver(DragEventArgs e)
    {
        DragOver?.Invoke(this, e);
    }

    protected virtual void OnGiveFeedback(GiveFeedbackEventArgs e)
    {
        GiveFeedback?.Invoke(this, e);
    }

    protected virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e)
    {
        QueryContinueDrag?.Invoke(this, e);
    }

    protected virtual void OnHelpRequested(HelpEventArgs e)
    {
        HelpRequested?.Invoke(this, e);
    }

    protected virtual void OnContextMenuStripChanged(EventArgs e)
    {
        ContextMenuStripChanged?.Invoke(this, e);
    }

    protected virtual void OnChangeUICues(UICuesEventArgs e)
    {
        ChangeUICues?.Invoke(this, e);
    }

    protected virtual void OnImeModeChanged(EventArgs e)
    {
        ImeModeChanged?.Invoke(this, e);
    }

    protected virtual void OnStyleChanged(EventArgs e)
    {
        StyleChanged?.Invoke(this, e);
    }

    protected virtual void OnSystemColorsChanged(EventArgs e)
    {
        SystemColorsChanged?.Invoke(this, e);
    }

    protected virtual void OnQueryAccessibilityHelp(QueryAccessibilityHelpEventArgs e)
    {
        QueryAccessibilityHelp?.Invoke(this, e);
    }

    protected virtual void OnCausesValidationChanged(EventArgs e)
    {
        CausesValidationChanged?.Invoke(this, e);
    }

    protected virtual void OnClientSizeChanged(EventArgs e)
    {
        ClientSizeChanged?.Invoke(this, e);
    }

    protected virtual void OnInvalidated(InvalidateEventArgs e)
    {
        Invalidated?.Invoke(this, e);
    }

    protected virtual void OnHandleCreated(EventArgs e)
    {
        HandleCreated?.Invoke(this, e);
    }

    protected virtual void OnHandleDestroyed(EventArgs e)
    {
        HandleDestroyed?.Invoke(this, e);
    }

    protected virtual void OnAutoSizeChanged(EventArgs e)
    {
        AutoSizeChanged?.Invoke(this, e);
    }

    protected virtual void OnDpiChanged(EventArgs e)
    {
        DpiChanged?.Invoke(this, e);
    }

    protected virtual void OnDpiChangedBeforeParent(EventArgs e)
    {
        DpiChangedBeforeParent?.Invoke(this, e);
    }

    protected virtual void OnDpiChangedAfterParent(EventArgs e)
    {
        DpiChangedAfterParent?.Invoke(this, e);
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
    /// Forces the control to invalidate and immediately repaint
    /// </summary>
    public void Refresh()
    {
        Invalidate();
    }

    /// <summary>
    /// Alias for Refresh() - forces immediate repaint
    /// </summary>
    public void Update()
    {
        Invalidate();
    }

    /// <summary>
    /// Finds the form that the control is on
    /// </summary>
    /// <returns>The Form that contains this control, or null if not on a form</returns>
    public Form? FindForm()
    {
        var control = this;
        while (control != null)
        {
            if (control is Form form)
                return form;
            control = control.Parent;
        }
        return null;
    }

    /// <summary>
    /// Determines if the control is a child (direct or nested) of this control
    /// </summary>
    public bool Contains(Control? control)
    {
        while (control != null)
        {
            control = control.Parent;
            if (control == this)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the child control at the specified point
    /// </summary>
    public Control? GetChildAtPoint(Point pt)
    {
        return GetChildAtPoint(pt, GetChildAtPointSkip.None);
    }

    /// <summary>
    /// Gets the child control at the specified point, with skip options
    /// </summary>
    public Control? GetChildAtPoint(Point pt, GetChildAtPointSkip skipValue)
    {
        foreach (var control in _controls)
        {
            if ((skipValue & GetChildAtPointSkip.Invisible) != 0 && !control.Visible)
                continue;
            if ((skipValue & GetChildAtPointSkip.Disabled) != 0 && !control.Enabled)
                continue;
            if ((skipValue & GetChildAtPointSkip.Transparent) != 0 && control.BackColor.A == 0)
                continue;

            if (control.Visible &&
                pt.X >= control.Left && pt.X < control.Right &&
                pt.Y >= control.Top && pt.Y < control.Bottom)
            {
                return control;
            }
        }
        return null;
    }

    /// <summary>
    /// Converts screen coordinates to client coordinates
    /// </summary>
    public Point PointToClient(Point p)
    {
        var result = new Point(p.X, p.Y);
        var control = this;
        while (control != null)
        {
            result.X -= control.Left;
            result.Y -= control.Top;
            control = control.Parent;
        }
        return result;
    }

    /// <summary>
    /// Converts client coordinates to screen coordinates
    /// </summary>
    public Point PointToScreen(Point p)
    {
        var result = new Point(p.X, p.Y);
        var control = this;
        while (control != null)
        {
            result.X += control.Left;
            result.Y += control.Top;
            control = control.Parent;
        }
        return result;
    }

    /// <summary>
    /// Converts a rectangle from screen coordinates to client coordinates
    /// </summary>
    public Rectangle RectangleToClient(Rectangle r)
    {
        var pt = PointToClient(new Point(r.X, r.Y));
        return new Rectangle(pt.X, pt.Y, r.Width, r.Height);
    }

    /// <summary>
    /// Converts a rectangle from client coordinates to screen coordinates
    /// </summary>
    public Rectangle RectangleToScreen(Rectangle r)
    {
        var pt = PointToScreen(new Point(r.X, r.Y));
        return new Rectangle(pt.X, pt.Y, r.Width, r.Height);
    }

    /// <summary>
    /// Sets the bounds of the control
    /// </summary>
    public void SetBounds(int x, int y, int width, int height)
    {
        Left = x;
        Top = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Sets the bounds of the control with specified parameters
    /// </summary>
    public void SetBounds(int x, int y, int width, int height, BoundsSpecified specified)
    {
        if ((specified & BoundsSpecified.X) != 0) Left = x;
        if ((specified & BoundsSpecified.Y) != 0) Top = y;
        if ((specified & BoundsSpecified.Width) != 0) Width = width;
        if ((specified & BoundsSpecified.Height) != 0) Height = height;
    }

    /// <summary>
    /// Scales the control and its children
    /// </summary>
    public void Scale(float ratio)
    {
        Scale(new SizeF(ratio, ratio));
    }

    /// <summary>
    /// Scales the control and its children by the specified factors
    /// </summary>
    public void Scale(SizeF factor)
    {
        Width = (int)(Width * factor.Width);
        Height = (int)(Height * factor.Height);

        foreach (var child in _controls)
        {
            child.Left = (int)(child.Left * factor.Width);
            child.Top = (int)(child.Top * factor.Height);
            child.Scale(factor);
        }
    }

    private int _layoutSuspendCount = 0;

    /// <summary>
    /// Temporarily suspends the layout logic for the control
    /// </summary>
    public void SuspendLayout()
    {
        _layoutSuspendCount++;
    }

    /// <summary>
    /// Resumes usual layout logic
    /// </summary>
    public void ResumeLayout()
    {
        ResumeLayout(true);
    }

    /// <summary>
    /// Resumes usual layout logic, optionally forcing an immediate layout
    /// </summary>
    public void ResumeLayout(bool performLayout)
    {
        if (_layoutSuspendCount > 0)
        {
            _layoutSuspendCount--;
            if (_layoutSuspendCount == 0 && performLayout)
            {
                PerformLayout();
            }
        }
    }

    // ========== ADDITIONAL PUBLIC METHODS ==========

    /// <summary>
    /// Shows the control to the user
    /// </summary>
    public void Show()
    {
        Visible = true;
    }

    /// <summary>
    /// Hides the control from the user
    /// </summary>
    public void Hide()
    {
        Visible = false;
    }

    /// <summary>
    /// Retrieves the next control forward or backward in the tab order
    /// </summary>
    public Control? GetNextControl(Control? ctl, bool forward)
    {
        if (ctl == null)
            return null;

        var controls = GetTabOrderedControls(this, nested: true);
        int currentIndex = controls.IndexOf(ctl);

        if (currentIndex < 0)
            return null;

        if (forward)
        {
            return currentIndex < controls.Count - 1 ? controls[currentIndex + 1] : null;
        }
        else
        {
            return currentIndex > 0 ? controls[currentIndex - 1] : null;
        }
    }

    /// <summary>
    /// Forces the control to apply layout logic to all its child controls
    /// </summary>
    public void PerformLayout(Control? affectedControl, string? affectedProperty)
    {
        PerformLayout();
    }

    /// <summary>
    /// Executes the specified delegate on the thread that owns the control's handle
    /// </summary>
    public object? Invoke(Delegate method)
    {
        return method.DynamicInvoke();
    }

    /// <summary>
    /// Executes the specified delegate with the specified arguments on the thread that owns the control's handle
    /// </summary>
    public object? Invoke(Delegate method, params object?[] args)
    {
        return method.DynamicInvoke(args);
    }

    /// <summary>
    /// Executes the specified delegate asynchronously on the thread that owns the control's handle
    /// </summary>
    public IAsyncResult BeginInvoke(Delegate method)
    {
        return Task.Run(() => method.DynamicInvoke());
    }

    /// <summary>
    /// Executes the specified delegate asynchronously with the specified arguments on the thread that owns the control's handle
    /// </summary>
    public IAsyncResult BeginInvoke(Delegate method, params object?[] args)
    {
        return Task.Run(() => method.DynamicInvoke(args));
    }

    /// <summary>
    /// Retrieves the return value of the asynchronous operation represented by the IAsyncResult passed
    /// </summary>
    public object? EndInvoke(IAsyncResult asyncResult)
    {
        if (asyncResult is Task task)
        {
            task.Wait();
            if (task.GetType().IsGenericType)
            {
                return task.GetType().GetProperty("Result")?.GetValue(task);
            }
        }
        return null;
    }

    /// <summary>
    /// Initiates a drag-and-drop operation
    /// </summary>
    public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects)
    {
        // Stub implementation - would need browser drag-drop API integration
        return DragDropEffects.None;
    }

    /// <summary>
    /// Begins a drag operation
    /// </summary>
    public void DoDragDrop(object data, DragDropEffects allowedEffects, Bitmap? dragImage, Point cursorOffset, bool useDefaultDragImage)
    {
        // Stub implementation
    }

    /// <summary>
    /// Retrieves the form that the control is on
    /// </summary>
    public Control? GetContainerControl()
    {
        var control = Parent;
        while (control != null)
        {
            if (control is Form)
                return control;
            control = control.Parent;
        }
        return null;
    }

    /// <summary>
    /// Invalidates a specific region of the control
    /// </summary>
    public void Invalidate(Rectangle rc)
    {
        Invalidate();
    }

    /// <summary>
    /// Invalidates a specific region of the control and causes a paint message to be sent to the control
    /// </summary>
    public void Invalidate(Rectangle rc, bool invalidateChildren)
    {
        Invalidate();
        if (invalidateChildren)
        {
            foreach (var child in _controls)
            {
                child.Invalidate();
            }
        }
    }

    /// <summary>
    /// Invalidates a specific region of the control
    /// </summary>
    public void Invalidate(Region? region)
    {
        Invalidate();
    }

    /// <summary>
    /// Invalidates the specified region of the control, optionally invalidating child controls
    /// </summary>
    public void Invalidate(Region? region, bool invalidateChildren)
    {
        Invalidate();
        if (invalidateChildren)
        {
            foreach (var child in _controls)
            {
                child.Invalidate();
            }
        }
    }

    /// <summary>
    /// Invalidates the control, optionally invalidating child controls
    /// </summary>
    public void Invalidate(bool invalidateChildren)
    {
        Invalidate();
        if (invalidateChildren)
        {
            foreach (var child in _controls)
            {
                child.Invalidate();
            }
        }
    }

    /// <summary>
    /// Brings the control to the front of the z-order
    /// </summary>
    public void BringToFront()
    {
        if (Parent != null)
        {
            var index = Parent._controls.IndexOf(this);
            if (index >= 0 && index < Parent._controls.Count - 1)
            {
                Parent._controls.RemoveAt(index);
                Parent._controls.Add(this);
                Parent.Invalidate();
            }
        }
    }

    /// <summary>
    /// Sends the control to the back of the z-order
    /// </summary>
    public void SendToBack()
    {
        if (Parent != null)
        {
            var index = Parent._controls.IndexOf(this);
            if (index > 0)
            {
                Parent._controls.RemoveAt(index);
                Parent._controls.Insert(0, this);
                Parent.Invalidate();
            }
        }
    }

    /// <summary>
    /// Resets the BackColor property to its default value
    /// </summary>
    public virtual void ResetBackColor()
    {
        BackColor = DefaultBackColor;
    }

    /// <summary>
    /// Resets the ForeColor property to its default value
    /// </summary>
    public virtual void ResetForeColor()
    {
        ForeColor = DefaultForeColor;
    }

    /// <summary>
    /// Resets the Font property to its default value
    /// </summary>
    public virtual void ResetFont()
    {
        Font = DefaultFont;
    }

    /// <summary>
    /// Resets the Cursor property to its default value
    /// </summary>
    public virtual void ResetCursor()
    {
        Cursor = DefaultCursor;
    }

    /// <summary>
    /// Resets the Text property to its default value
    /// </summary>
    public virtual void ResetText()
    {
        Text = string.Empty;
    }

    /// <summary>
    /// Determines if the BackColor property needs to be persisted
    /// </summary>
    protected virtual bool ShouldSerializeBackColor()
    {
        return BackColor != DefaultBackColor;
    }

    /// <summary>
    /// Determines if the ForeColor property needs to be persisted
    /// </summary>
    protected virtual bool ShouldSerializeForeColor()
    {
        return ForeColor != DefaultForeColor;
    }

    /// <summary>
    /// Determines if the Font property needs to be persisted
    /// </summary>
    protected virtual bool ShouldSerializeFont()
    {
        return _font != null;
    }

    /// <summary>
    /// Determines if the Cursor property needs to be persisted
    /// </summary>
    protected virtual bool ShouldSerializeCursor()
    {
        return _cursor != null;
    }

    /// <summary>
    /// Determines if the Text property needs to be persisted
    /// </summary>
    protected virtual bool ShouldSerializeText()
    {
        return !string.IsNullOrEmpty(Text);
    }

    /// <summary>
    /// Creates a Graphics object for the control
    /// </summary>
    public Graphics CreateGraphics()
    {
        return new Graphics(Width, Height);
    }

    /// <summary>
    /// Supports rendering to the specified bitmap
    /// </summary>
    public void DrawToBitmap(Bitmap bitmap, Rectangle targetBounds)
    {
        // Stub implementation - would need actual bitmap rendering
    }

    /// <summary>
    /// Retrieves the control that contains the specified handle
    /// </summary>
    public static Control? FromHandle(IntPtr handle)
    {
        // Stub implementation - no handles in canvas environment
        return null;
    }

    /// <summary>
    /// Retrieves the control that contains the specified child control
    /// </summary>
    public static Control? FromChildHandle(IntPtr handle)
    {
        // Stub implementation - no handles in canvas environment
        return null;
    }

    /// <summary>
    /// Returns a value indicating whether the specified control is a child of this control
    /// </summary>
    public bool IsChild(Control ctl)
    {
        return Contains(ctl);
    }

    /// <summary>
    /// Notifies the control that its layout must be performed
    /// </summary>
    protected void NotifyInvalidate(Rectangle invalidatedArea)
    {
        OnInvalidated(new InvalidateEventArgs(invalidatedArea));
    }

    /// <summary>
    /// Raises the Paint event
    /// </summary>
    protected void RaisePaintEvent(object key, PaintEventArgs e)
    {
        OnPaint(e);
    }

    /// <summary>
    /// Raises the specified event
    /// </summary>
    protected void RaiseMouseEvent(object key, MouseEventArgs e)
    {
        // Stub - for compatibility
    }

    /// <summary>
    /// Raises the specified event
    /// </summary>
    protected void RaiseKeyEvent(object key, KeyEventArgs e)
    {
        // Stub - for compatibility
    }

    /// <summary>
    /// Processes a command key
    /// </summary>
    protected virtual bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Stub implementation
        return false;
    }

    /// <summary>
    /// Processes a dialog key
    /// </summary>
    protected virtual bool ProcessDialogKey(Keys keyData)
    {
        // Handle Tab, Enter, Escape, Arrow keys
        if (keyData == Keys.Tab)
        {
            return ProcessTabKey(forward: true);
        }
        return false;
    }

    /// <summary>
    /// Processes a dialog character
    /// </summary>
    protected virtual bool ProcessDialogChar(char charCode)
    {
        // Stub implementation - for mnemonic support
        return false;
    }

    /// <summary>
    /// Processes a keyboard message
    /// </summary>
    protected virtual bool ProcessKeyMessage(ref Message msg)
    {
        // Stub implementation
        return false;
    }

    /// <summary>
    /// Processes a key preview
    /// </summary>
    protected virtual bool ProcessKeyPreview(ref Message msg)
    {
        // Stub implementation
        return false;
    }

    /// <summary>
    /// Previews a keyboard message
    /// </summary>
    protected virtual bool ProcessKeyEventArgs(ref Message msg)
    {
        // Stub implementation
        return false;
    }

    /// <summary>
    /// Processes a mnemonic character
    /// </summary>
    protected internal virtual bool ProcessMnemonic(char charCode)
    {
        // Stub implementation - for Alt+Key shortcuts
        return false;
    }

    /// <summary>
    /// Scales the control and child controls
    /// </summary>
    protected virtual void ScaleControl(SizeF factor, BoundsSpecified specified)
    {
        if ((specified & BoundsSpecified.Width) != 0)
            Width = (int)(Width * factor.Width);
        if ((specified & BoundsSpecified.Height) != 0)
            Height = (int)(Height * factor.Height);
    }

    /// <summary>
    /// Scales a control's location, size, padding and margin
    /// </summary>
    protected virtual void ScaleCore(float dx, float dy)
    {
        Scale(new SizeF(dx, dy));
    }

    /// <summary>
    /// Sets the specified bounds of the control
    /// </summary>
    protected virtual void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        SetBounds(x, y, width, height, specified);
    }

    /// <summary>
    /// Performs the work of setting the specified bounds of this control
    /// </summary>
    protected virtual void SetClientSizeCore(int x, int y)
    {
        ClientSize = new Size(x, y);
    }

    /// <summary>
    /// Sets the control to the specified visible state
    /// </summary>
    protected virtual void SetVisibleCore(bool value)
    {
        if (Visible != value)
        {
            Visible = value;
            OnVisibleChanged(EventArgs.Empty);
        }
    }

    /// <summary>
    /// Activates the control
    /// </summary>
    protected virtual void Select(bool directed, bool forward)
    {
        Select();
    }

    /// <summary>
    /// Processes Windows messages
    /// </summary>
    protected virtual void WndProc(ref Message m)
    {
        // Stub implementation - no Windows messages in canvas environment
    }

    /// <summary>
    /// Determines if a character is an input character that the control recognizes
    /// </summary>
    protected virtual bool IsInputChar(char charCode)
    {
        return true;
    }

    /// <summary>
    /// Determines if a key is an input key or a special key that requires preprocessing
    /// </summary>
    protected virtual bool IsInputKey(Keys keyData)
    {
        return true;
    }

    /// <summary>
    /// Determines if the specified client coordinate is within the control's boundaries
    /// </summary>
    public bool ContainsPoint(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    /// Determines if the specified point is within the control's boundaries
    /// </summary>
    public bool ContainsPoint(Point pt)
    {
        return pt.X >= 0 && pt.X < Width && pt.Y >= 0 && pt.Y < Height;
    }

    /// <summary>
    /// Initializes the control
    /// </summary>
    protected virtual void InitLayout()
    {
        // Stub - called when control is added to container
    }

    /// <summary>
    /// Raises the create control event
    /// </summary>
    protected virtual void OnCreateControl()
    {
        // Stub - for compatibility
    }

    /// <summary>
    /// Destroys the control
    /// </summary>
    protected virtual void DestroyHandle()
    {
        // Stub - no handles in canvas environment
    }

    /// <summary>
    /// Recreates the handle for the control
    /// </summary>
    protected void RecreateHandle()
    {
        // Stub - no handles in canvas environment
    }

    /// <summary>
    /// Raises the HandleCreated event
    /// </summary>
    protected void CreateHandle()
    {
        // Stub - no handles in canvas environment
        OnHandleCreated(EventArgs.Empty);
    }

    /// <summary>
    /// Performs cleanup of resources
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            Disposing = true;

            // Remove from parent
            if (Parent != null)
            {
                Parent._controls.Remove(this);
            }

            // Dispose children
            foreach (var child in _controls.ToList())
            {
                child.Dispose(true);
            }
            _controls.Clear();

            Disposing = false;
            _isDisposed = true;
        }
    }

    /// <summary>
    /// Releases all resources used by the control
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs layout logic for docked and anchored controls
    /// </summary>
    public virtual void PerformLayout()
    {
        // Respect layout suspension
        if (_layoutSuspendCount > 0) return;

        if (_controls.Count == 0) return;

        // Use virtual LayoutWidth/LayoutHeight to account for chrome (e.g., title bar in Forms)
        var layoutWidth = LayoutWidth;
        var layoutHeight = LayoutHeight;

        // First, store original bounds for anchored controls
        foreach (var control in _controls)
        {
            if (!control.OriginalBoundsSet && control.Dock == DockStyle.None)
            {
                control.OriginalLeft = control.Left;
                control.OriginalTop = control.Top;
                control.OriginalWidth = control.Width;
                control.OriginalHeight = control.Height;
                control.OriginalParentWidth = layoutWidth;
                control.OriginalParentHeight = layoutHeight;
                control.OriginalBoundsSet = true;
            }
        }

        // Available client area for layout
        var clientRect = new Rectangle(0, 0, layoutWidth, layoutHeight);

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
            var deltaWidth = layoutWidth - control.OriginalParentWidth;
            var deltaHeight = layoutHeight - control.OriginalParentHeight;

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

/// <summary>
/// Specifies which bounds of the control to use when defining its size and position
/// </summary>
[Flags]
public enum BoundsSpecified
{
    None = 0,
    X = 1,
    Y = 2,
    Width = 4,
    Height = 8,
    Location = X | Y,
    Size = Width | Height,
    All = Location | Size
}

/// <summary>
/// Specifies constants that define which child controls to skip
/// </summary>
[Flags]
public enum GetChildAtPointSkip
{
    None = 0,
    Invisible = 1,
    Disabled = 2,
    Transparent = 4
}
