namespace Canvas.Windows.Forms.Drawing;

/// <summary>
/// Encapsulates all drawing and hit-testing logic for a vertical, item-index-based
/// scrollbar. Used by ListControl, ComboBox, AutoCompletePanel, and TextBoxBase.
/// This is a readonly struct — no heap allocation.
/// </summary>
public readonly struct VerticalScrollbarHelper
{
    // ── Appearance constants ───────────────────────────────────────────────────
    public const int Width      = 16;
    public const int MinThumb   = 20;
    public const int ThumbInset = 2;
    public const int ArrowSize  = 16;   // height of each arrow button

    private static readonly Color TrackColor  = Color.FromArgb(240, 240, 240);
    private static readonly Color ThumbColor  = Color.FromArgb(180, 180, 180);
    private static readonly Color ArrowColor  = Color.FromArgb(96,  96,  96);

    // ── State ──────────────────────────────────────────────────────────────────
    public readonly Rectangle Track;       // full scrollbar bounds (includes arrows)
    public readonly int TotalItems;
    public readonly int VisibleItems;
    public readonly int TopIndex;

    public VerticalScrollbarHelper(Rectangle track, int totalItems, int visibleItems, int topIndex)
    {
        Track        = track;
        TotalItems   = totalItems;
        VisibleItems = visibleItems;
        TopIndex     = topIndex;
    }

    // ── Derived geometry ───────────────────────────────────────────────────────

    /// <summary>Arrow-up button rectangle.</summary>
    public Rectangle ArrowUpRect   => new Rectangle(Track.X, Track.Y, Track.Width, ArrowSize);

    /// <summary>Arrow-down button rectangle.</summary>
    public Rectangle ArrowDownRect => new Rectangle(Track.X, Track.Bottom - ArrowSize, Track.Width, ArrowSize);

    /// <summary>Thumb rail — the area between the two arrow buttons.</summary>
    private Rectangle Rail => new Rectangle(
        Track.X, Track.Y + ArrowSize,
        Track.Width, Math.Max(0, Track.Height - ArrowSize * 2));

    private int MaxTopIndex => Math.Max(0, TotalItems - VisibleItems);

    public int ThumbHeight
    {
        get
        {
            if (TotalItems <= 0 || Rail.Height <= 0) return Rail.Height;
            return Math.Max(MinThumb,
                (VisibleItems * Rail.Height) / Math.Max(1, TotalItems));
        }
    }

    public int ThumbTop
    {
        get
        {
            var max = MaxTopIndex;
            if (max <= 0) return 0;
            return Rail.Y + (TopIndex * (Rail.Height - ThumbHeight)) / max;
        }
    }

    public Rectangle ThumbBounds => new Rectangle(
        Track.X + ThumbInset,
        ThumbTop,
        Track.Width - ThumbInset * 2,
        ThumbHeight);

    // ── Drawing ────────────────────────────────────────────────────────────────

    public void Draw(Graphics g)
    {
        // Track background
        using var trackBrush = new SolidBrush(TrackColor);
        g.FillRectangle(trackBrush, Track);

        // Border line on left side
        using var borderPen = new Pen(Color.FromArgb(205, 205, 205));
        g.DrawLine(borderPen, Track.X, Track.Y, Track.X, Track.Bottom);

        // Thumb
        using var thumbBrush = new SolidBrush(ThumbColor);
        g.FillRectangle(thumbBrush, ThumbBounds);

        // Arrow buttons
        DrawArrow(g, ArrowUpRect,   isUp: true);
        DrawArrow(g, ArrowDownRect, isUp: false);
    }

    private static void DrawArrow(Graphics g, Rectangle btn, bool isUp)
    {
        // Button background
        using var bg = new SolidBrush(Color.FromArgb(240, 240, 240));
        g.FillRectangle(bg, btn);

        // Button border
        using var border = new Pen(Color.FromArgb(205, 205, 205));
        g.DrawRectangle(border, btn);

        // Arrow triangle — centred in the button
        var cx = btn.X + btn.Width  / 2;
        var cy = btn.Y + btn.Height / 2;
        const int half = 4;

        using var arrowPen = new Pen(ArrowColor, 1.5f);
        if (isUp)
        {
            // ▲
            g.DrawLine(arrowPen, cx - half, cy + half / 2, cx,          cy - half / 2);
            g.DrawLine(arrowPen, cx,        cy - half / 2, cx + half,   cy + half / 2);
        }
        else
        {
            // ▼
            g.DrawLine(arrowPen, cx - half, cy - half / 2, cx,          cy + half / 2);
            g.DrawLine(arrowPen, cx,        cy + half / 2, cx + half,   cy - half / 2);
        }
    }

    // ── Hit testing ────────────────────────────────────────────────────────────

    public ScrollbarHit HitTest(int x, int y)
    {
        if (!Track.Contains(x, y)) return ScrollbarHit.None;

        if (ArrowUpRect.Contains(x, y))   return ScrollbarHit.ArrowUp;
        if (ArrowDownRect.Contains(x, y)) return ScrollbarHit.ArrowDown;

        var thumb = ThumbBounds;
        if (thumb.Contains(x, y))   return ScrollbarHit.Thumb;
        if (y < thumb.Y)             return ScrollbarHit.Above;
        return ScrollbarHit.Below;
    }

    public bool ContainsPoint(int x, int y) => Track.Contains(x, y);

    // ── Interaction helpers ────────────────────────────────────────────────────

    public int ComputeDragTopIndex(int mouseY, int dragStartY, int dragStartTopIndex)
    {
        var rail      = Rail;
        var thumbH    = ThumbHeight;
        var trackSpan = rail.Height - thumbH;
        if (trackSpan <= 0) return dragStartTopIndex;

        var deltaY     = mouseY - dragStartY;
        var indexDelta = (int)((deltaY * (double)MaxTopIndex) / trackSpan);
        return Math.Clamp(dragStartTopIndex + indexDelta, 0, MaxTopIndex);
    }

    public int ComputePageTopIndex(int clickY, int currentTopIndex)
    {
        var thumb = ThumbBounds;
        if (clickY < thumb.Y)
            return Math.Max(0, currentTopIndex - VisibleItems);
        return Math.Min(MaxTopIndex, currentTopIndex + VisibleItems);
    }

    public int ClampTopIndex(int topIndex) => Math.Clamp(topIndex, 0, MaxTopIndex);
}

/// <summary>Result of VerticalScrollbarHelper.HitTest.</summary>
public enum ScrollbarHit
{
    None,
    ArrowUp,
    ArrowDown,
    Thumb,
    Above,
    Below,
}

