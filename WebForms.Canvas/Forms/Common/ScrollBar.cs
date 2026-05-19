namespace System.Windows.Forms;

/// <summary>
/// Abstract base for horizontal and vertical scroll-bar controls.
/// Hierarchy matches WinForms: ScrollBar : Control.
/// </summary>
public abstract class ScrollBar : Control
{
    // ── Value state ──────────────────────────────────────────────────────────
    private int _minimum      = 0;
    private int _maximum      = 100;
    private int _value        = 0;
    private int _smallChange  = 1;
    private int _largeChange  = 10;

    // ── Drag state ───────────────────────────────────────────────────────────
    private bool _dragging     = false;
    private int  _dragOffset   = 0;   // pixel offset from thumb origin to click point

    // ── Events ───────────────────────────────────────────────────────────────
    public event ScrollEventHandler? Scroll;
    public event EventHandler?       ValueChanged;

    // ── Geometry constants ────────────────────────────────────────────────────
    private const int ArrowSize = 16;

    protected ScrollBar()
    {
        TabStop  = false;
        BackColor = Color.FromArgb(240, 240, 240);
        SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint, true);
    }

    // ── Properties ───────────────────────────────────────────────────────────

    public int Minimum
    {
        get => _minimum;
        set
        {
            if (value > _maximum) throw new ArgumentException("Minimum must be ≤ Maximum.");
            _minimum = value;
            if (_value < _minimum) Value = _minimum;
            else Invalidate();
        }
    }

    public int Maximum
    {
        get => _maximum;
        set
        {
            if (value < _minimum) throw new ArgumentException("Maximum must be ≥ Minimum.");
            _maximum = value;
            if (_value > _maximum) Value = _maximum;
            else Invalidate();
        }
    }

    public int Value
    {
        get => _value;
        set
        {
            // WinForms effective maximum = Maximum - LargeChange + 1
            int effectiveMax = Math.Max(_minimum, _maximum - _largeChange + 1);
            var clamped = Math.Max(_minimum, Math.Min(effectiveMax, value));
            if (_value == clamped) return;
            _value = clamped;
            ValueChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public int SmallChange
    {
        get => _smallChange;
        set => _smallChange = Math.Max(1, value);
    }

    public int LargeChange
    {
        get => _largeChange;
        set => _largeChange = Math.Max(1, value);
    }

    /// <summary>Whether this scroll bar is horizontal (overridden in subclasses).</summary>
    protected abstract bool IsHorizontal { get; }

    // ── Geometry helpers ──────────────────────────────────────────────────────

    /// <summary>The pixel length of the scrollable track (excluding arrow buttons).</summary>
    private int TrackLength => (IsHorizontal ? Width : Height) - ArrowSize * 2;

    /// <summary>The pixel size of the thumb, proportional to LargeChange/range.</summary>
    private int ThumbSize
    {
        get
        {
            int range = _maximum - _minimum + _largeChange;
            if (range <= 0) return TrackLength;
            int sz = (int)((double)_largeChange / range * TrackLength);
            return Math.Max(sz, 16);
        }
    }

    /// <summary>Pixel offset of the thumb from the start of the track.</summary>
    private int ThumbOffset
    {
        get
        {
            int range = _maximum - _minimum;
            if (range <= 0) return 0;
            return (int)((double)(_value - _minimum) / range * (TrackLength - ThumbSize));
        }
    }

    private Rectangle GetThumbRect()
    {
        int offset = ArrowSize + ThumbOffset;
        return IsHorizontal
            ? new Rectangle(offset, 1, ThumbSize, Height - 2)
            : new Rectangle(1, offset, Width - 2, ThumbSize);
    }

    private Rectangle GetDecrArrowRect() =>
        IsHorizontal
            ? new Rectangle(0,           0, ArrowSize, Height)
            : new Rectangle(0,           0, Width,     ArrowSize);

    private Rectangle GetIncrArrowRect() =>
        IsHorizontal
            ? new Rectangle(Width - ArrowSize, 0, ArrowSize, Height)
            : new Rectangle(0, Height - ArrowSize, Width,     ArrowSize);

    // ── Paint ──────────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        using (var bg = new SolidBrush(Color.FromArgb(205, 205, 205)))
            g.FillRectangle(bg, 0, 0, Width, Height);

        // Track area
        var trackBg = Enabled ? Color.FromArgb(230, 230, 230) : Color.FromArgb(220, 220, 220);
        var trackRect = IsHorizontal
            ? new Rectangle(ArrowSize, 0, TrackLength, Height)
            : new Rectangle(0, ArrowSize, Width, TrackLength);
        using (var tb = new SolidBrush(trackBg))
            g.FillRectangle(tb, trackRect);

        // Thumb
        if (Enabled && TrackLength > 0)
        {
            var thumb = GetThumbRect();
            using (var thumbBrush = new SolidBrush(Color.FromArgb(189, 189, 189)))
                g.FillRectangle(thumbBrush, thumb);
            using (var thumbPen = new Pen(Color.FromArgb(160, 160, 160)))
                g.DrawRectangle(thumbPen, thumb);
        }

        // Arrow buttons
        PaintArrow(g, GetDecrArrowRect(), decrement: true);
        PaintArrow(g, GetIncrArrowRect(), decrement: false);

        base.OnPaint(e);
    }

    private void PaintArrow(Graphics g, Rectangle r, bool decrement)
    {
        var arrowBg = Enabled ? Color.FromArgb(205, 205, 205) : Color.FromArgb(220, 220, 220);
        using (var ab = new SolidBrush(arrowBg))
            g.FillRectangle(ab, r);
        using (var ap = new Pen(Color.FromArgb(160, 160, 160)))
            g.DrawRectangle(ap, r);

        // Simple triangle arrow indicator
        using var arrowPen = new Pen(Enabled ? Color.FromArgb(80, 80, 80) : Color.FromArgb(160, 160, 160));
        int cx = r.Left + r.Width / 2;
        int cy = r.Top  + r.Height / 2;
        int hs = 4; // half arrow size

        if (IsHorizontal)
        {
            if (decrement) // left arrow ◄
            {
                g.DrawLine(arrowPen, cx + hs - 1, cy - hs, cx - hs + 1, cy);
                g.DrawLine(arrowPen, cx - hs + 1, cy,      cx + hs - 1, cy + hs);
            }
            else // right arrow ►
            {
                g.DrawLine(arrowPen, cx - hs + 1, cy - hs, cx + hs - 1, cy);
                g.DrawLine(arrowPen, cx + hs - 1, cy,      cx - hs + 1, cy + hs);
            }
        }
        else
        {
            if (decrement) // up arrow ▲
            {
                g.DrawLine(arrowPen, cx - hs, cy + hs - 1, cx,      cy - hs + 1);
                g.DrawLine(arrowPen, cx,      cy - hs + 1, cx + hs, cy + hs - 1);
            }
            else // down arrow ▼
            {
                g.DrawLine(arrowPen, cx - hs, cy - hs + 1, cx,      cy + hs - 1);
                g.DrawLine(arrowPen, cx,      cy + hs - 1, cx + hs, cy - hs + 1);
            }
        }
    }

    // ── Mouse ──────────────────────────────────────────────────────────────────

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && Enabled)
        {
            var decrRect = GetDecrArrowRect();
            var incrRect = GetIncrArrowRect();
            var thumb    = GetThumbRect();

            if (decrRect.Contains(e.X, e.Y))
            {
                RaiseScroll(ScrollEventType.SmallDecrement, Value - _smallChange);
            }
            else if (incrRect.Contains(e.X, e.Y))
            {
                RaiseScroll(ScrollEventType.SmallIncrement, Value + _smallChange);
            }
            else if (thumb.Contains(e.X, e.Y))
            {
                _dragging   = true;
                _dragOffset = IsHorizontal ? e.X - thumb.Left : e.Y - thumb.Top;
                Capture = true;   // lock mouse routing to this control
            }
            else
            {
                // Click on track — page scroll
                bool before = IsHorizontal ? e.X < thumb.Left : e.Y < thumb.Top;
                RaiseScroll(before ? ScrollEventType.LargeDecrement : ScrollEventType.LargeIncrement,
                            before ? Value - _largeChange          : Value + _largeChange);
            }
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragging)
        {
            int range = _maximum - _minimum;
            if (range <= 0) return;
            int trackPx = TrackLength - ThumbSize;
            if (trackPx <= 0) return;

            int px = (IsHorizontal ? e.X : e.Y) - ArrowSize - _dragOffset;
            double ratio = (double)px / trackPx;
            int newVal = _minimum + (int)Math.Round(ratio * range);
            RaiseScroll(ScrollEventType.ThumbTrack, newVal);
        }
        base.OnMouseMove(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        if (_dragging)
        {
            _dragging = false;
            Capture   = false;   // release mouse routing lock
            Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, _value));
            Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.EndScroll, _value));
            Invalidate();
        }
        base.OnMouseUp(e);
    }

    // ── Keyboard ───────────────────────────────────────────────────────────────

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.Up:
                RaiseScroll(ScrollEventType.SmallDecrement, Value - _smallChange);
                e.Handled = true;
                break;
            case Keys.Right:
            case Keys.Down:
                RaiseScroll(ScrollEventType.SmallIncrement, Value + _smallChange);
                e.Handled = true;
                break;
            case Keys.PageUp:
                RaiseScroll(ScrollEventType.LargeDecrement, Value - _largeChange);
                e.Handled = true;
                break;
            case Keys.PageDown:
                RaiseScroll(ScrollEventType.LargeIncrement, Value + _largeChange);
                e.Handled = true;
                break;
            case Keys.Home:
                RaiseScroll(ScrollEventType.First, _minimum);
                e.Handled = true;
                break;
            case Keys.End:
                RaiseScroll(ScrollEventType.Last, EffectiveMaximum);
                e.Handled = true;
                break;
        }
        base.OnKeyDown(e);
    }

    /// <summary>
    /// The WinForms effective maximum: the scroll position cannot exceed Maximum - LargeChange + 1.
    /// This matches real WinForms ScrollBar behaviour where the thumb sits flush at the end when
    /// Value == Maximum - LargeChange + 1, not when Value == Maximum.
    /// </summary>
    private int EffectiveMaximum => Math.Max(_minimum, _maximum - _largeChange + 1);

    protected internal override void OnMouseWheel(MouseEventArgs e)
    {
        if (!Enabled) { base.OnMouseWheel(e); return; }
        int lines = Math.Max(1, Math.Abs(e.Delta) / 120);
        if (e.Delta > 0)
            RaiseScroll(ScrollEventType.SmallDecrement, Value - _smallChange * lines);
        else if (e.Delta < 0)
            RaiseScroll(ScrollEventType.SmallIncrement, Value + _smallChange * lines);
        base.OnMouseWheel(e);
    }

    private void RaiseScroll(ScrollEventType type, int newValue)
    {
        int old = _value;
        // Clamp to [Minimum, EffectiveMaximum] — matches WinForms scrollbar clamping.
        newValue = Math.Max(_minimum, Math.Min(EffectiveMaximum, newValue));
        Value = newValue;
        Scroll?.Invoke(this, new ScrollEventArgs(type, old, _value, IsHorizontal ? ScrollOrientation.HorizontalScroll : ScrollOrientation.VerticalScroll));
    }
}

// ── Concrete subclasses ───────────────────────────────────────────────────────

/// <summary>Horizontal scroll bar control. Hierarchy: HScrollBar : ScrollBar : Control.</summary>
public class HScrollBar : ScrollBar
{
    protected override bool IsHorizontal => true;

    public HScrollBar()
    {
        Width  = 200;
        Height = 17;
    }
}

/// <summary>Vertical scroll bar control. Hierarchy: VScrollBar : ScrollBar : Control.</summary>
public class VScrollBar : ScrollBar
{
    protected override bool IsHorizontal => false;

    public VScrollBar()
    {
        Width  = 17;
        Height = 200;
    }
}

// ── Supporting types ──────────────────────────────────────────────────────────

public delegate void ScrollEventHandler(object? sender, ScrollEventArgs e);

public class ScrollEventArgs : EventArgs
{
    public ScrollEventType   Type        { get; }
    public int               NewValue    { get; }
    public int               OldValue    { get; }
    public ScrollOrientation ScrollOrientation { get; }

    public ScrollEventArgs(ScrollEventType type, int newValue)
        : this(type, 0, newValue, ScrollOrientation.HorizontalScroll) { }

    public ScrollEventArgs(ScrollEventType type, int oldValue, int newValue, ScrollOrientation orientation)
    {
        Type              = type;
        OldValue          = oldValue;
        NewValue          = newValue;
        ScrollOrientation = orientation;
    }
}

public enum ScrollEventType
{
    SmallDecrement  = 0,
    SmallIncrement  = 1,
    LargeDecrement  = 2,
    LargeIncrement  = 3,
    ThumbPosition   = 4,
    ThumbTrack      = 5,
    First           = 6,
    Last            = 7,
    EndScroll       = 8,
}

public enum ScrollOrientation
{
    HorizontalScroll = 0,
    VerticalScroll   = 1,
}
