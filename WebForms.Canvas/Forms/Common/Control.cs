
namespace System.Windows.Forms;

public abstract class Control
{
    private Control? _parent;
    protected internal readonly List<Control> _controls = new();
    private ControlCollection? _controlsCollection;
    private string _text = string.Empty;

    public string Name { get; set; } = string.Empty;

    public virtual string Text
    {
        get => _text;
        set
        {
            var newValue = value ?? string.Empty;
            if (_text != newValue)
            {
                _text = newValue;
                OnTextChanged(EventArgs.Empty);
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
                OnLocationChanged(EventArgs.Empty);
                Invalidate();
                _parent?.Invalidate();
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
                OnLocationChanged(EventArgs.Empty);
                Invalidate();
                _parent?.Invalidate();
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
                OnResize(EventArgs.Empty);
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
                OnResize(EventArgs.Empty);
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
                OnDockChanged(EventArgs.Empty);
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


    private System.Drawing.Color _backColor = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ControlBackColor;
    private System.Drawing.Color _foreColor = Canvas.Windows.Forms.Theming.CanvasTheme.Current.ControlForeColor;

    public System.Drawing.Color BackColor
    {
        get => _backColor;
        set
        {
            if (_backColor != value)
            {
                _backColor = value;
                OnBackColorChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public System.Drawing.Color ForeColor
    {
        get => _foreColor;
        set
        {
            if (_foreColor != value)
            {
                _foreColor = value;
                OnForeColorChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }
    private bool _visible = true;
    private bool _enabled = true;

    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;
                OnVisibleChanged(EventArgs.Empty);
                Invalidate();
                _parent?.Invalidate();
            }
        }
    }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                OnEnabledChanged(EventArgs.Empty);
                // Propagate enabled state to children (WinForms behaviour)
                foreach (var child in _controls)
                    child.OnEnabledChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }
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

    public virtual Rectangle DisplayRectangle => new Rectangle(
        Padding.Left,
        Padding.Top,
        Width  - Padding.Horizontal,
        Height - Padding.Vertical);

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
                OnFontChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public int FontHeight => Font.Height;

    // Size constraints
    private Canvas.Windows.Forms.Drawing.Size _minimumSize = Canvas.Windows.Forms.Drawing.Size.Empty;
    private Canvas.Windows.Forms.Drawing.Size _maximumSize = Canvas.Windows.Forms.Drawing.Size.Empty;
    private Padding _margin = new Padding(3);
    private Padding _padding = Padding.Empty;

    public Canvas.Windows.Forms.Drawing.Size MinimumSize
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

    public Canvas.Windows.Forms.Drawing.Size MaximumSize
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

    public Padding Margin
    {
        get => _margin;
        set
        {
            if (_margin != value)
            {
                _margin = value;
                OnMarginChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public Padding Padding
    {
        get => _padding;
        set
        {
            if (_padding != value)
            {
                _padding = value;
                OnPaddingChanged(EventArgs.Empty);
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
                OnTabIndexChanged(EventArgs.Empty);
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
                OnTabStopChanged(EventArgs.Empty);
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
                OnBackgroundImageChanged(EventArgs.Empty);
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
                OnBackgroundImageLayoutChanged(EventArgs.Empty);
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
                OnCursorChanged(EventArgs.Empty);
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
                OnRegionChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    // Mirroring
    public bool IsMirrored { get; protected set; } = false;

    // Accessibility
    private string? _accessibleName;
    private string? _accessibleDescription;
    private string? _accessibleDefaultActionDescription;
    private AccessibleRole _accessibleRole = AccessibleRole.Default;
    private bool _isAccessible = false;

    public AccessibleObject? AccessibilityObject => null;

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
    private bool _capture;
    public bool Capture
    {
        get => _capture;
        set
        {
            if (_capture == value) return;
            _capture = value;
            OnMouseCaptureChanged(EventArgs.Empty);
        }
    }

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

    // Context menus
    private ContextMenu? _legacyContextMenu;

    /// <summary>Legacy pre-ContextMenuStrip context menu. Setting this also wires the underlying ContextMenuStrip.</summary>
    [Obsolete("Use ContextMenuStrip instead")]
    public ContextMenu? ContextMenu
    {
        get => _legacyContextMenu;
        set
        {
            _legacyContextMenu = value;
            ContextMenuStrip   = value?._strip;
        }
    }

    public ContextMenuStrip? ContextMenuStrip { get; set; }

    // Data binding
    public BindingContext? BindingContext { get; set; }
    private ControlBindingsCollection? _dataBindings;
    public ControlBindingsCollection DataBindings => _dataBindings ??= new ControlBindingsCollection(this);
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
    public static System.Drawing.Point MousePosition { get; internal set; } = System.Drawing.Point.Empty;

    // Thread safety (stubs for canvas-based controls - WASM is single-threaded)
    public bool InvokeRequired => false;
    public static bool CheckForIllegalCrossThreadCalls { get; set; } = false;

    /// <summary>
    /// Executes the specified delegate on the thread that owns the control.
    /// In WASM this is a no-op since everything runs on the same thread.
    /// </summary>
    public object? Invoke(Delegate method)
    {
        return method.DynamicInvoke();
    }

    /// <summary>
    /// Executes the specified delegate on the thread that owns the control.
    /// In WASM this is a no-op since everything runs on the same thread.
    /// </summary>
    public object? Invoke(Delegate method, params object?[]? args)
    {
        return method.DynamicInvoke(args);
    }

    /// <summary>
    /// Executes the specified action on the thread that owns the control.
    /// </summary>
    public void Invoke(Action action) => action();

    /// <summary>
    /// Executes the specified function on the thread that owns the control and returns the result.
    /// </summary>
    public T Invoke<T>(Func<T> func) => func();

    /// <summary>
    /// Executes the specified delegate asynchronously on the thread that owns the control.
    /// Posts via <see cref="SynchronizationContext"/> when available (Blazor Server),
    /// otherwise executes immediately (WASM single-threaded).
    /// </summary>
    public IAsyncResult BeginInvoke(Delegate method)
    {
        return BeginInvoke(method, null);
    }

    /// <summary>
    /// Executes the specified delegate asynchronously on the thread that owns the control.
    /// Posts via <see cref="SynchronizationContext"/> when available (Blazor Server),
    /// otherwise executes immediately (WASM single-threaded).
    /// </summary>
    public IAsyncResult BeginInvoke(Delegate method, params object?[]? args)
    {
        var tcs = new TaskCompletionSource<object?>();
        var ctx = SynchronizationContext.Current;
        if (ctx != null)
        {
            ctx.Post(_ =>
            {
                try   { tcs.SetResult(method.DynamicInvoke(args)); }
                catch (Exception ex) { tcs.SetException(ex); }
            }, null);
        }
        else
        {
            try   { tcs.SetResult(method.DynamicInvoke(args)); }
            catch (Exception ex) { tcs.SetException(ex); }
        }
        return new TaskAsyncResult(tcs.Task);
    }

    /// <summary>
    /// Executes the specified action asynchronously on the thread that owns the control.
    /// </summary>
    public IAsyncResult BeginInvoke(Action action) => BeginInvoke((Delegate)action);

    /// <summary>
    /// Retrieves the return value of the asynchronous operation.
    /// </summary>
    public object? EndInvoke(IAsyncResult asyncResult)
    {
        if (asyncResult is TaskAsyncResult tar)
            return tar.GetResult();
        return null;
    }

    // Simple IAsyncResult implementation for completed operations
    private class CompletedAsyncResult : IAsyncResult
    {
        public bool IsCompleted => true;
        public WaitHandle AsyncWaitHandle => new ManualResetEvent(true);
        public object? AsyncState => null;
        public bool CompletedSynchronously => true;
    }

    // IAsyncResult backed by a Task (used by BeginInvoke with SynchronizationContext)
    private class TaskAsyncResult : IAsyncResult
    {
        private readonly Task<object?> _task;
        public TaskAsyncResult(Task<object?> task) { _task = task; }
        public bool IsCompleted => _task.IsCompleted;
        public WaitHandle AsyncWaitHandle => ((IAsyncResult)_task).AsyncWaitHandle;
        public object? AsyncState => _task.AsyncState;
        public bool CompletedSynchronously => _task.IsCompleted;
        public object? GetResult() => _task.GetAwaiter().GetResult();
    }



    // Events raising capability
    public bool CanRaiseEvents => true;

    // UI state
    public bool ShowFocusCues => true;
    public bool ShowKeyboardCues => true;
    public bool ScaleChildren => true;

    // Preferred size
    public System.Drawing.Size PreferredSize => new System.Drawing.Size(DefaultSize.Width, DefaultSize.Height);

    protected virtual Canvas.Windows.Forms.Drawing.Size GetPreferredSize(Canvas.Windows.Forms.Drawing.Size proposedSize)
    {
        return DefaultSize;
    }

    // Assembly info
    public string ProductName => "Canvas.Windows.Forms";
    public string ProductVersion => "1.0.0";
    public string CompanyName => "Canvas.Windows.Forms";

    // Create params (stub)
    public virtual object? CreateParams => null;

    // Obsolete properties
    [Obsolete("This property is obsolete")]
    public bool RenderRightToLeft => false;

    [Obsolete("This property is not relevant for this class")]
    public object? WindowTarget { get; set; }

    // Default static properties
    public static System.Drawing.Color DefaultBackColor => Canvas.Windows.Forms.Theming.CanvasTheme.Current.ControlBackColor;
    public static System.Drawing.Color DefaultForeColor => Canvas.Windows.Forms.Theming.CanvasTheme.Current.ControlForeColor;
    public static Font DefaultFont => new Font("Segoe UI", 12);
    public static Cursor DefaultCursor => Cursor.Default;
    public static ImeMode DefaultImeMode => ImeMode.NoControl;
    public static Padding DefaultMargin => new Padding(3);
    public static Canvas.Windows.Forms.Drawing.Size DefaultMaximumSize => Canvas.Windows.Forms.Drawing.Size.Empty;
    public static Canvas.Windows.Forms.Drawing.Size DefaultMinimumSize => Canvas.Windows.Forms.Drawing.Size.Empty;
    public static Padding DefaultPadding => Padding.Empty;
    public virtual Canvas.Windows.Forms.Drawing.Size DefaultSize => new Canvas.Windows.Forms.Drawing.Size(100, 20);

    // Location and Size helpers
    public System.Drawing.Point Location
    {
        get => new System.Drawing.Point(Left, Top);
        set { Left = value.X; Top = value.Y; }
    }

    public System.Drawing.Size Size
    {
        get => new System.Drawing.Size(Width, Height);
        set { Width = value.Width; Height = value.Height; }
    }

    public System.Drawing.Rectangle Bounds
    {
        get => new System.Drawing.Rectangle(Left, Top, Width, Height);
        set { Left = value.X; Top = value.Y; Width = value.Width; Height = value.Height; }
    }

    /// <summary>
    /// Sets the bounds of the control.
    /// </summary>
    public void SetBounds(int x, int y, int width, int height)
    {
        SetBounds(x, y, width, height, BoundsSpecified.All);
    }

    /// <summary>
    /// Sets the specified bounds of the control.
    /// </summary>
    public void SetBounds(int x, int y, int width, int height, BoundsSpecified specified)
    {
        if ((specified & BoundsSpecified.X) != 0) Left = x;
        if ((specified & BoundsSpecified.Y) != 0) Top = y;
        if ((specified & BoundsSpecified.Width) != 0) Width = width;
        if ((specified & BoundsSpecified.Height) != 0) Height = height;
    }

    /// <summary>
    /// Computes the location of the specified client point in screen coordinates.
    /// Note: In canvas-based rendering, screen coordinates are relative to the browser viewport.
    /// </summary>
    public System.Drawing.Point PointToScreen(System.Drawing.Point p)
    {
        // Calculate position relative to top-level form
        var offsetX = p.X;
        var offsetY = p.Y;
        var current = this;

        while (current != null)
        {
            offsetX += current.Left;
            offsetY += current.Top;
            current = current.Parent;
        }

        return new System.Drawing.Point(offsetX, offsetY);
    }

    /// <summary>
    /// Computes the location of the specified screen point in client coordinates.
    /// Note: In canvas-based rendering, screen coordinates are relative to the browser viewport.
    /// </summary>
    public System.Drawing.Point PointToClient(System.Drawing.Point p)
    {
        // Calculate position relative to this control
        var offsetX = p.X;
        var offsetY = p.Y;
        var current = this;

        while (current != null)
        {
            offsetX -= current.Left;
            offsetY -= current.Top;
            current = current.Parent;
        }

        return new System.Drawing.Point(offsetX, offsetY);
    }

    /// <summary>
    /// Computes the size and location of the specified client rectangle in screen coordinates.
    /// </summary>
    public System.Drawing.Rectangle RectangleToScreen(System.Drawing.Rectangle r)
    {
        var pt = PointToScreen(new System.Drawing.Point(r.X, r.Y));
        return new System.Drawing.Rectangle(pt.X, pt.Y, r.Width, r.Height);
    }

    /// <summary>
    /// Computes the size and location of the specified screen rectangle in client coordinates.
    /// </summary>
    public System.Drawing.Rectangle RectangleToClient(System.Drawing.Rectangle r)
    {
        var pt = PointToClient(new System.Drawing.Point(r.X, r.Y));
        return new System.Drawing.Rectangle(pt.X, pt.Y, r.Width, r.Height);
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

    public ControlCollection Controls => _controlsCollection ??= new ControlCollection(this, _controls);

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
        if (IsMouseRoutingContainer && Enabled)
        {
            var child = FindChildAt(e.X, e.Y);
            if (child != null)
            {
                _mouseCaptureChild = child;
                SetFormFocusedControl(child);
                var (cx, cy) = ToChildCoordinates(child, e.X, e.Y);
                child.OnMouseDown(new MouseEventArgs(e.Button, e.Clicks, cx, cy));
                MouseDown?.Invoke(this, e);
                return;
            }
        }
        MouseDown?.Invoke(this, e);
    }

    protected internal virtual void OnMouseUp(MouseEventArgs e)
    {
        if (IsMouseRoutingContainer && Enabled)
        {
            var child = _mouseCaptureChild ?? FindChildAt(e.X, e.Y);
            _mouseCaptureChild = null;
            if (child != null)
            {
                var (cx, cy) = ToChildCoordinates(child, e.X, e.Y);
                child.OnMouseUp(new MouseEventArgs(e.Button, e.Clicks, cx, cy));
                MouseUp?.Invoke(this, e);
                return;
            }
        }
        MouseUp?.Invoke(this, e);
    }

    protected internal virtual void OnMouseMove(MouseEventArgs e)
    {
        if (IsMouseRoutingContainer && Enabled)
        {
            var child = _mouseCaptureChild ?? FindChildAt(e.X, e.Y);
            if (child != null)
            {
                var (cx, cy) = ToChildCoordinates(child, e.X, e.Y);
                child.OnMouseMove(new MouseEventArgs(e.Button, e.Clicks, cx, cy));
                MouseMove?.Invoke(this, e);
                return;
            }
        }
        MouseMove?.Invoke(this, e);
    }

    protected internal virtual void OnMouseClick(MouseEventArgs e)
    {
        MouseClick?.Invoke(this, e);
    }

    protected internal virtual void OnMouseDoubleClick(MouseEventArgs e)
    {
        if (IsMouseRoutingContainer && Enabled)
        {
            var child = FindChildAt(e.X, e.Y);
            if (child != null)
            {
                var (cx, cy) = ToChildCoordinates(child, e.X, e.Y);
                child.OnMouseDoubleClick(new MouseEventArgs(e.Button, e.Clicks, cx, cy));
                MouseDoubleClick?.Invoke(this, e);
                return;
            }
        }
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
        Invalidate();
        GotFocus?.Invoke(this, e);
    }

    protected internal virtual void OnLostFocus(EventArgs e)
    {
        Invalidate();
        LostFocus?.Invoke(this, e);
    }

    // ========== SHARED RENDERING HELPERS ==========

    /// <summary>
    /// Returns the foreground color to use when the control is disabled.
    /// Derived from ForeColor so custom colors are respected.
    /// </summary>
    protected System.Drawing.Color DisabledForeColor =>
        System.Drawing.Color.FromArgb((int)(ForeColor.R * 0.43f), (int)(ForeColor.G * 0.43f), (int)(ForeColor.B * 0.43f));

    /// <summary>
    /// Returns ForeColor when enabled, DisabledForeColor when disabled.
    /// </summary>
    protected System.Drawing.Color EffectiveForeColor => Enabled ? ForeColor : DisabledForeColor;

    /// <summary>
    /// Fills the entire control bounds with BackColor (skips fill when Transparent).
    /// </summary>
    protected void DrawControlBackground(Graphics g)
    {
        if (BackColor == System.Drawing.Color.Transparent) return;
        using var brush = new SolidBrush(BackColor);
        g.FillRectangle(brush, 0, 0, Width, Height);
    }

    /// <summary>
    /// Draws a standard 1-pixel dotted focus rectangle inset by <paramref name="inset"/> pixels.
    /// Only draws when Focused and Enabled.
    /// </summary>
    protected void DrawFocusRect(Graphics g, int inset = 2)
    {
        if (!Focused || !Enabled) return;
        using var pen = new Pen(Color.Black) { DashStyle = DashStyle.Dot };
        g.DrawRectangle(pen, inset, inset, Width - inset * 2 - 1, Height - inset * 2 - 1);
    }

    /// <summary>
    /// Draws a standard 1-pixel dotted focus rectangle around an explicit bounds.
    /// Only draws when Focused and Enabled.
    /// </summary>
    protected void DrawFocusRect(Graphics g, Rectangle bounds)
    {
        if (!Focused || !Enabled) return;
        using var pen = new Pen(Color.Black) { DashStyle = DashStyle.Dot };
        g.DrawRectangle(pen, bounds);
    }

    protected internal virtual void OnEnter(EventArgs e)
    {
        Enter?.Invoke(this, e);
    }

    protected internal virtual void OnLeave(EventArgs e)
    {
        Leave?.Invoke(this, e);
    }

    // ========== SHARED CONTAINER MOUSE-ROUTING ==========
    // Centralised here so Panel, GroupBox, SplitContainer etc. don't each
    // duplicate the same capture/find/route logic.

    private Control? _mouseCaptureChild;

    /// <summary>
    /// Returns the content-space coordinates for a raw event point.
    /// Base implementation is identity (no scroll). ScrollableControl overrides
    /// to subtract the AutoScroll offset.
    /// </summary>
    protected virtual (int contentX, int contentY) ToContentCoordinates(int x, int y) => (x, y);

    /// <summary>
    /// Returns the child-local coordinates for a raw event point.
    /// </summary>
    protected (int x, int y) ToChildCoordinates(Control child, int x, int y)
    {
        var (cx, cy) = ToContentCoordinates(x, y);
        return (cx - child.Left, cy - child.Top);
    }

    /// <summary>
    /// Returns true when (x,y) — in content coordinates — hits the child,
    /// including any popup/overlay regions the child may extend into.
    /// </summary>
    protected virtual bool ChildHitTest(Control child, int x, int y)
    {
        // Normal bounds
        if (x >= child.Left && x < child.Left + child.Width &&
            y >= child.Top  && y < child.Top  + child.Height)
            return true;

        // ComboBox drop-down
        if (child is ComboBox cb && cb.DroppedDown)
            return x >= child.Left && x < child.Left + cb.DropDownWidth &&
                   y >= child.Top + child.Height && y < child.Top + child.Height + cb.DropDownHeight;

        // DateTimePicker calendar
        if (child is DateTimePicker dtp && dtp.DroppedDown)
        {
            var dd = dtp.GetDropDownBounds();
            return x >= child.Left + dd.X && x < child.Left + dd.Right &&
                   y >= child.Top  + dd.Y && y < child.Top  + dd.Bottom;
        }

        // TextBox autocomplete panel
        if (child is TextBox tb && tb.HasVisibleAutoComplete)
        {
            var ac = tb.GetAutoCompletePanelBounds();
            return x >= child.Left + ac.X && x < child.Left + ac.Right &&
                   y >= child.Top  + ac.Y && y < child.Top  + ac.Bottom;
        }

        return false;
    }

    /// <summary>
    /// Returns the top-most enabled+visible child hit at (x,y) in this control's
    /// coordinate space. Scroll offset is handled by ToContentCoordinates.
    /// </summary>
    protected Control? FindChildAt(int x, int y)
    {
        var (cx, cy) = ToContentCoordinates(x, y);
        for (var i = Controls.Count - 1; i >= 0; i--)
        {
            var child = Controls[i];
            if (!child.Visible || !child.Enabled) continue;
            if (ChildHitTest(child, cx, cy)) return child;
        }
        return null;
    }

    /// <summary>
    /// Focuses a child and updates Form.FocusedControl.
    /// </summary>
    protected void SetFormFocusedControl(Control control)
    {
        if (FindForm() is Form form)
            form.FocusedControl = control;
        control.Focus();
    }

    /// <summary>
    /// Whether this control acts as a routing container for its children's mouse events.
    /// True for Panel, GroupBox and similar containers; false for leaf controls.
    /// Derived classes set this in their constructor.
    /// </summary>
    protected bool IsMouseRoutingContainer { get; set; } = false;

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

    protected internal virtual void OnMouseWheel(MouseEventArgs e)
    {
        if (IsMouseRoutingContainer && Enabled)
        {
            var child = FindChildAt(e.X, e.Y);
            if (child != null)
            {
                var (cx, cy) = ToChildCoordinates(child, e.X, e.Y);
                child.OnMouseWheel(new MouseEventArgs(e.Button, e.Clicks, cx, cy, e.Delta));
                MouseWheel?.Invoke(this, e);
                return;
            }
        }
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

    /// <summary>
    /// Fires <see cref="Validating"/>; if not cancelled, fires <see cref="Validated"/> and
    /// returns <c>true</c>.  Returns <c>false</c> if a <see cref="Validating"/> handler sets
    /// <c>e.Cancel = true</c>.
    /// </summary>
    public bool Validate()
    {
        if (!CausesValidation) return true;
        var args = new CancelEventArgs();
        OnValidating(args);
        if (args.Cancel) return false;
        OnValidated(EventArgs.Empty);
        return true;
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

    protected internal virtual void OnDragDrop(DragEventArgs e)
    {
        DragDrop?.Invoke(this, e);
    }

    protected internal virtual void OnDragEnter(DragEventArgs e)
    {
        DragEnter?.Invoke(this, e);
    }

    protected internal virtual void OnDragLeave(EventArgs e)
    {
        DragLeave?.Invoke(this, e);
    }

    protected internal virtual void OnDragOver(DragEventArgs e)
    {
        DragOver?.Invoke(this, e);
    }

    protected internal virtual void OnGiveFeedback(GiveFeedbackEventArgs e)
    {
        GiveFeedback?.Invoke(this, e);
    }

    protected internal virtual void OnQueryContinueDrag(QueryContinueDragEventArgs e)
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
            // Fire Validating on the losing control; if cancelled, deny the focus move.
            if (currentlyFocused.CausesValidation && !currentlyFocused.Validate())
                return false;

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
        if (_layoutSuspendCount > 0)
        {
            _invalidatePending = true;
            return;
        }
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
    public Control? GetChildAtPoint(System.Drawing.Point pt)
    {
        return GetChildAtPoint(pt, GetChildAtPointSkip.None);
    }

    /// <summary>
    /// Gets the child control at the specified point, with skip options
    /// </summary>
    public Control? GetChildAtPoint(System.Drawing.Point pt, GetChildAtPointSkip skipValue)
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
    private bool _invalidatePending = false;

    protected bool IsLayoutSuspended => _layoutSuspendCount > 0;

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
            if (_layoutSuspendCount == 0)
            {
                if (performLayout)
                    PerformLayout();

                // Flush any Invalidate() calls that were deferred during suspension.
                if (_invalidatePending)
                {
                    _invalidatePending = false;
                    _ = RequestRender?.Invoke();
                }
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
    /// Initiates a drag-and-drop operation.
    /// <para>
    /// Registers a session with <see cref="DragDropManager"/> and fires
    /// <see cref="DragDropManager.DragStarted"/> so the renderer can set
    /// <c>draggable=true</c> on the canvas element.  Because WebAssembly runs on a
    /// single thread, this method returns <see cref="DragDropEffects.None"/>
    /// immediately — it cannot block waiting for the drop.  The actual resulting
    /// effect is delivered asynchronously through <see cref="DragDropManager.LastResult"/>
    /// once <c>HandleDrop</c> fires in <c>FormRenderer</c>.
    /// </para>
    /// <para>
    /// In standard WinForms the method blocks until the drag ends and returns the
    /// effect.  Translated apps that use the return value must read
    /// <see cref="DragDropManager.LastResult"/> after the drop instead.
    /// </para>
    /// </summary>
    public DragDropEffects DoDragDrop(object data, DragDropEffects allowedEffects)
    {
        // Cancel any stale prior session before starting a new one.
        if (DragDropManager.IsDragging)
            DragDropManager.CancelDrag();

        // BeginDrag fires DragStarted so FormRenderer can enable draggable on the canvas.
        DragDropManager.BeginDrag(this, data, allowedEffects);

        // Notify source that the drag has started.
        OnQueryContinueDrag(new QueryContinueDragEventArgs(0, false, DragAction.Continue));

        // WASM constraint: cannot block here — the UI thread must remain free so
        // HandleDrop/HandleDragLeave can fire.  The result will be available via
        // DragDropManager.LastResult after the drop completes.
        return DragDropEffects.None;
    }

    /// <summary>
    /// Begins a drag operation with a custom drag image (browser environment: image is ignored).
    /// </summary>
    public void DoDragDrop(object data, DragDropEffects allowedEffects, Bitmap? dragImage, Point cursorOffset, bool useDefaultDragImage)
    {
        DoDragDrop(data, allowedEffects);
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

        // Available client area for layout — inset by this container's Padding (WinForms parity)
        var clientRect = new Rectangle(
            Padding.Left,
            Padding.Top,
            layoutWidth  - Padding.Horizontal,
            layoutHeight - Padding.Vertical);

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

        // Recursively apply layout to child containers so nested layout panels
        // (e.g., FlowLayoutPanel/TableLayoutPanel) lay out their children after being sized/positioned.
        foreach (var control in _controls)
        {
            if (!control.Visible) continue;
            control.PerformLayout();
        }

        Invalidate();
    }

    public Func<Task>? RequestRender { get; set; }

    // Propagate RequestRender to all children
    public void PropagateRequestRender(Func<Task>? requestRender)
    {
        RequestRender = requestRender;
        foreach (var child in _controls)
        {
            child.PropagateRequestRender(requestRender);
        }
    }

    // Matches WinForms: System.Windows.Forms.Control.ControlCollection
    public class ControlCollection : IEnumerable<Control>
    {
        private readonly Control _owner;
        private readonly List<Control> _list;

        internal ControlCollection(Control owner, List<Control> list)
        {
            _owner = owner;
            _list = list;
        }

        public virtual int Count => _list.Count;

        public virtual Control this[int index] => _list[index];

        public virtual void Add(Control control)
        {
            control.Parent = _owner;
            control.RequestRender = _owner.RequestRender;
            _list.Add(control);
            _owner.OnControlAdded(new ControlEventArgs(control));
            _owner.Invalidate();
        }

        public virtual void AddRange(Control[] controls)
        {
            foreach (var c in controls)
            {
                Add(c);
            }
        }

        public virtual void Remove(Control control)
        {
            if (_list.Remove(control))
            {
                control.Parent = null;
                _owner.OnControlRemoved(new ControlEventArgs(control));
                _owner.Invalidate();
            }
        }

        public virtual void Clear()
        {
            var removed = _list.ToList();
            foreach (var control in removed)
            {
                control.Parent = null;
                _owner.OnControlRemoved(new ControlEventArgs(control));
            }
            _list.Clear();
            _owner.Invalidate();
        }

        public virtual bool Contains(Control control) => _list.Contains(control);

        public virtual int IndexOf(Control control) => _list.IndexOf(control);

        public virtual IEnumerator<Control> GetEnumerator() => _list.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
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
