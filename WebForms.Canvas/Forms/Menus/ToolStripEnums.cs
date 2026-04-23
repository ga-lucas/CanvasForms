namespace System.Windows.Forms;

// ── ToolStripItemDisplayStyle ────────────────────────────────────────────────
/// <summary>Specifies what content is displayed on a ToolStripItem.</summary>
public enum ToolStripItemDisplayStyle
{
    None        = 0,
    Text        = 1,
    Image       = 2,
    ImageAndText = 3
}

// ── ToolStripItemImageScaling ────────────────────────────────────────────────
/// <summary>Specifies whether the image on a ToolStripItem is scaled.</summary>
public enum ToolStripItemImageScaling
{
    None     = 0,
    SizeToFit = 1
}

// ── ToolStripItemAlignment ────────────────────────────────────────────────────
/// <summary>Specifies whether a ToolStripItem is aligned to the left or right.</summary>
public enum ToolStripItemAlignment
{
    Left  = 0,
    Right = 1
}

// ── ToolStripItemOverflow ─────────────────────────────────────────────────────
/// <summary>Specifies how a ToolStripItem participates in overflow behavior.</summary>
public enum ToolStripItemOverflow
{
    Never  = 0,
    Always = 1,
    AsNeeded = 2
}

// ── ToolStripItemPlacement ────────────────────────────────────────────────────
/// <summary>Specifies where a ToolStripItem is placed.</summary>
public enum ToolStripItemPlacement
{
    Main     = 0,
    Overflow = 1,
    None     = 2
}

// ── ToolStripGripStyle ────────────────────────────────────────────────────────
/// <summary>Specifies whether the grip (move handle) of a ToolStrip is visible.</summary>
public enum ToolStripGripStyle
{
    Hidden  = 0,
    Visible = 1
}

// ── ToolStripGripDisplayStyle ─────────────────────────────────────────────────
/// <summary>Specifies whether the grip of a ToolStrip is horizontal or vertical.</summary>
public enum ToolStripGripDisplayStyle
{
    Horizontal = 0,
    Vertical   = 1
}

// ── ToolStripLayoutStyle ──────────────────────────────────────────────────────
/// <summary>Specifies the layout of items on a ToolStrip.</summary>
public enum ToolStripLayoutStyle
{
    StackWithOverflow  = 0,
    HorizontalStackWithOverflow = 1,
    VerticalStackWithOverflow   = 2,
    Flow               = 3,
    Table              = 4
}

// ── ToolStripTextDirection ────────────────────────────────────────────────────
/// <summary>Specifies the direction in which text is drawn on a ToolStripItem.</summary>
public enum ToolStripTextDirection
{
    Inherit    = 0,
    Horizontal = 2,
    Vertical90 = 3,
    Vertical270 = 4
}

// ── ToolStripRenderMode ───────────────────────────────────────────────────────
/// <summary>Specifies the painting style applied to a ToolStrip.</summary>
public enum ToolStripRenderMode
{
    Custom      = 0,
    System      = 1,
    Professional = 2,
    ManagerRenderMode = 3
}

// ── ToolStripDropDownDirection ─────────────────────────────────────────────────
/// <summary>Specifies the direction in which a ToolStripDropDown opens.</summary>
public enum ToolStripDropDownDirection
{
    AboveLeft  = 0,
    AboveRight = 1,
    BelowLeft  = 2,
    BelowRight = 3,
    Left       = 4,
    Right      = 5,
    Default    = 7
}

// ── ToolStripDropDownCloseReason ──────────────────────────────────────────────
/// <summary>Specifies the reason a ToolStripDropDown was closed.</summary>
public enum ToolStripDropDownCloseReason
{
    AppClicked       = 0,
    AppFocusChange   = 1,
    CloseCalled      = 2,
    Keyboard         = 3,
    ItemClicked      = 4,
    Keyboard_Escape  = 5
}

// ── ArrowDirection ────────────────────────────────────────────────────────────
/// <summary>Specifies the direction of an arrow on a ToolStripItem.</summary>
public enum ArrowDirection
{
    Left  = 0,
    Up    = 1,
    Right = 16,
    Down  = 17
}

// ── MergeAction ───────────────────────────────────────────────────────────────
/// <summary>Specifies how ToolStripMenuItems are merged in MDI scenarios.</summary>
public enum MergeAction
{
    Append        = 0,
    Insert        = 1,
    Replace       = 2,
    Remove        = 3,
    MatchOnly     = 4
}

// ── ToolStripDropDownClosingEventArgs ─────────────────────────────────────────
/// <summary>Provides data for the ToolStripDropDown.Closing event.</summary>
public class ToolStripDropDownClosingEventArgs : CancelEventArgs
{
    public ToolStripDropDownCloseReason CloseReason { get; }

    public ToolStripDropDownClosingEventArgs(ToolStripDropDownCloseReason reason)
    {
        CloseReason = reason;
    }
}

public delegate void ToolStripDropDownClosingEventHandler(object? sender, ToolStripDropDownClosingEventArgs e);

// ── ToolStripItemClickedEventArgs ─────────────────────────────────────────────
/// <summary>Provides data for the ToolStrip.ItemClicked event.</summary>
public class ToolStripItemClickedEventArgs : EventArgs
{
    public ToolStripItem ClickedItem { get; }

    public ToolStripItemClickedEventArgs(ToolStripItem clickedItem)
    {
        ClickedItem = clickedItem;
    }
}

public delegate void ToolStripItemClickedEventHandler(object? sender, ToolStripItemClickedEventArgs e);

// ── ToolStripRenderEventArgs ──────────────────────────────────────────────────
/// <summary>Stub event args for ToolStrip rendering events.</summary>
public class ToolStripRenderEventArgs : EventArgs
{
    public Graphics Graphics { get; }
    public ToolStrip ToolStrip { get; }
    public Rectangle AffectedBounds { get; }
    public System.Drawing.Color BackColor { get; }

    public ToolStripRenderEventArgs(Graphics g, ToolStrip toolStrip)
    {
        Graphics  = g;
        ToolStrip = toolStrip;
        AffectedBounds = Rectangle.Empty;
        BackColor = System.Drawing.Color.Empty;
    }

    public ToolStripRenderEventArgs(Graphics g, ToolStrip toolStrip, Rectangle affectedBounds, System.Drawing.Color backColor)
    {
        Graphics       = g;
        ToolStrip      = toolStrip;
        AffectedBounds = affectedBounds;
        BackColor      = backColor;
    }
}

public delegate void ToolStripRenderEventHandler(object? sender, ToolStripRenderEventArgs e);

// ── ToolStripItemRenderEventArgs ──────────────────────────────────────────────
/// <summary>Stub event args for ToolStripItem rendering events.</summary>
public class ToolStripItemRenderEventArgs : EventArgs
{
    public Graphics       Graphics { get; }
    public ToolStripItem  Item     { get; }
    public ToolStrip      ToolStrip => Item.Owner!;

    public ToolStripItemRenderEventArgs(Graphics g, ToolStripItem item)
    {
        Graphics = g;
        Item     = item;
    }
}

public delegate void ToolStripItemRenderEventHandler(object? sender, ToolStripItemRenderEventArgs e);

// ── ToolStripRenderer (stub) ──────────────────────────────────────────────────
/// <summary>
/// Stub base class for ToolStrip renderers.  Canvas rendering is performed
/// directly in OnPaint; this class exists for API compatibility only.
/// </summary>
public abstract class ToolStripRenderer
{
    public event ToolStripRenderEventHandler? RenderToolStripBackground;
    public event ToolStripRenderEventHandler? RenderToolStripBorder;
    public event ToolStripRenderEventHandler? RenderGrip;
    public event ToolStripItemRenderEventHandler? RenderButtonBackground;
    public event ToolStripItemRenderEventHandler? RenderDropDownButtonBackground;
    public event ToolStripItemRenderEventHandler? RenderItemBackground;
    public event ToolStripItemRenderEventHandler? RenderItemImage;
    public event ToolStripItemRenderEventHandler? RenderItemText;
    public event ToolStripItemRenderEventHandler? RenderLabelBackground;
    public event ToolStripItemRenderEventHandler? RenderMenuItemBackground;
    public event ToolStripItemRenderEventHandler? RenderOverflowButtonBackground;
    public event ToolStripItemRenderEventHandler? RenderSeparator;
    public event ToolStripItemRenderEventHandler? RenderSplitButtonBackground;
    public event ToolStripItemRenderEventHandler? RenderStatusStripSizingGrip;
    public event ToolStripItemRenderEventHandler? RenderToolStripContentPanelBackground;
    public event ToolStripItemRenderEventHandler? RenderToolStripPanelBackground;
    public event ToolStripItemRenderEventHandler? RenderToolStripStatusLabelBackground;
    protected virtual void Initialize(ToolStrip toolStrip) { }
    protected virtual void InitializeItem(ToolStripItem item) { }
    protected virtual void InitializeContentPanel(ToolStripContentPanel contentPanel) { }
    protected virtual void InitializePanel(ToolStripPanel toolStripPanel) { }
}

/// <summary>Stub professional renderer for API compatibility.</summary>
public class ToolStripProfessionalRenderer : ToolStripRenderer
{
    public ToolStripProfessionalRenderer() { }
}

/// <summary>Stub system renderer for API compatibility.</summary>
public class ToolStripSystemRenderer : ToolStripRenderer
{
    public ToolStripSystemRenderer() { }
}

// ── ToolStripManager (stub) ───────────────────────────────────────────────────
/// <summary>
/// Stub for WinForms ToolStripManager.  Provides static renderer/merge APIs.
/// </summary>
public static class ToolStripManager
{
    private static ToolStripRenderMode _renderMode = ToolStripRenderMode.ManagerRenderMode;
    private static ToolStripRenderer?  _renderer;

    public static ToolStripRenderMode RenderMode
    {
        get => _renderMode;
        set => _renderMode = value;
    }

    public static ToolStripRenderer Renderer
    {
        get => _renderer ??= new ToolStripProfessionalRenderer();
        set => _renderer = value;
    }

    public static bool VisualStylesEnabled { get; set; } = true;

    /// <summary>Merges source strip into target (stub — no-op).</summary>
    public static bool Merge(ToolStrip sourceToolStrip, ToolStrip targetToolStrip) => false;
    public static bool Merge(ToolStrip sourceToolStrip, string targetName) => false;
    public static bool RevertMerge(ToolStrip targetToolStrip) => false;
    public static bool RevertMerge(ToolStrip targetToolStrip, ToolStrip sourceToolStrip) => false;

    public static event EventHandler? RendererChanged;
}
