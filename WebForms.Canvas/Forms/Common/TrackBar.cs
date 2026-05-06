namespace System.Windows.Forms;

/// <summary>
/// Represents a standard Windows TrackBar (slider) control.
/// Hierarchy matches WinForms: TrackBar : Control.
/// </summary>
public class TrackBar : Control
{
    // ── Value state ──────────────────────────────────────────────────────────
    private int _minimum      = 0;
    private int _maximum      = 10;
    private int _value        = 0;
    private int _smallChange  = 1;
    private int _largeChange  = 5;
    private int _tickFrequency = 1;
    private Orientation _orientation = Orientation.Horizontal;
    private TickStyle   _tickStyle   = TickStyle.BottomRight;
    private bool        _autoSize    = true;

    // ── Drag state ───────────────────────────────────────────────────────────
    private bool _dragging     = false;
    private int  _dragStartPx  = 0;
    private int  _dragStartVal = 0;

    // ── Events ───────────────────────────────────────────────────────────────
    public event EventHandler?    ValueChanged;
    public event EventHandler?    Scroll;

    // ── Geometry constants ───────────────────────────────────────────────────
    private const int TrackPad   = 10;   // padding on each side of the track
    private const int ThumbW     = 11;   // thumb width (horizontal)
    private const int ThumbH     = 22;   // thumb height (horizontal)
    private const int TrackThick = 4;    // track channel thickness

    public TrackBar()
    {
        Width    = 104;
        Height   = 45;
        TabStop  = true;
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
            var clamped = Math.Max(_minimum, Math.Min(_maximum, value));
            if (_value == clamped) return;
            _value = clamped;
            ValueChanged?.Invoke(this, EventArgs.Empty);
            Scroll?.Invoke(this, EventArgs.Empty);
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

    public int TickFrequency
    {
        get => _tickFrequency;
        set { _tickFrequency = Math.Max(1, value); Invalidate(); }
    }

    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            if (_orientation == value) return;
            _orientation = value;
            // Swap dimensions to maintain sensible default size
            (Width, Height) = (Height, Width);
            Invalidate();
        }
    }

    public TickStyle TickStyle
    {
        get => _tickStyle;
        set { _tickStyle = value; Invalidate(); }
    }

    public bool AutoSize
    {
        get => _autoSize;
        set { _autoSize = value; Invalidate(); }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Returns the pixel position of the thumb centre along the track axis.</summary>
    private int ValueToPixel(int v)
    {
        if (_maximum == _minimum) return TrackPad + ThumbW / 2;
        bool horiz = _orientation == Orientation.Horizontal;
        int trackLen = (horiz ? Width : Height) - TrackPad * 2 - ThumbW;
        double ratio = (double)(v - _minimum) / (_maximum - _minimum);
        return TrackPad + ThumbW / 2 + (int)Math.Round(ratio * trackLen);
    }

    private int PixelToValue(int px)
    {
        bool horiz = _orientation == Orientation.Horizontal;
        int trackLen = (horiz ? Width : Height) - TrackPad * 2 - ThumbW;
        if (trackLen <= 0) return _minimum;
        double ratio = (double)(px - TrackPad - ThumbW / 2) / trackLen;
        ratio = Math.Max(0.0, Math.Min(1.0, ratio));
        return _minimum + (int)Math.Round(ratio * (_maximum - _minimum));
    }

    private Rectangle GetThumbRect()
    {
        int cx = ValueToPixel(_value);
        bool horiz = _orientation == Orientation.Horizontal;
        if (horiz)
        {
            int cy = Height / 2;
            return new Rectangle(cx - ThumbW / 2, cy - ThumbH / 2, ThumbW, ThumbH);
        }
        else
        {
            int cy = Height - ValueToPixel(_value); // invert Y for vertical
            int ch = Width / 2;
            return new Rectangle(ch - ThumbH / 2, cy - ThumbW / 2, ThumbH, ThumbW);
        }
    }

    // ── Paint ─────────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        bool horiz = _orientation == Orientation.Horizontal;

        // Background
        using (var bg = new SolidBrush(BackColor))
            g.FillRectangle(bg, 0, 0, Width, Height);

        // ── Track channel ──────────────────────────────────────────────────
        int trackStart = TrackPad + ThumbW / 2;
        var trackColor = Enabled ? Color.FromArgb(200, 200, 200) : Color.FromArgb(210, 210, 210);

        Rectangle trackRect;
        if (horiz)
        {
            int cy = Height / 2 - TrackThick / 2;
            trackRect = new Rectangle(trackStart, cy, Width - TrackPad * 2 - ThumbW, TrackThick);
        }
        else
        {
            int trackEnd = Height - TrackPad - ThumbW / 2;
            int cx = Width / 2 - TrackThick / 2;
            trackRect = new Rectangle(cx, trackStart, TrackThick, trackEnd - trackStart);
        }

        using (var trackBrush = new SolidBrush(trackColor))
            g.FillRectangle(trackBrush, trackRect);
        using (var trackPen = new Pen(Color.FromArgb(167, 167, 167)))
            g.DrawRectangle(trackPen, trackRect);

        // ── Tick marks ──────────────────────────────────────────────────────
        if (_tickStyle != TickStyle.None && _maximum > _minimum)
        {
            using var tickPen = new Pen(Color.FromArgb(100, 100, 100));
            int range = _maximum - _minimum;
            int tickCount = range / _tickFrequency;
            for (int i = 0; i <= tickCount; i++)
            {
                int tv = _minimum + i * _tickFrequency;
                int tp = ValueToPixel(tv);
                if (horiz)
                {
                    int cy = Height / 2;
                    bool bottom = _tickStyle == TickStyle.BottomRight || _tickStyle == TickStyle.Both;
                    bool top    = _tickStyle == TickStyle.TopLeft     || _tickStyle == TickStyle.Both;
                    if (bottom) g.DrawLine(tickPen, tp, cy + ThumbH / 2 - 1, tp, cy + ThumbH / 2 + 3);
                    if (top)    g.DrawLine(tickPen, tp, cy - ThumbH / 2 + 1, tp, cy - ThumbH / 2 - 3);
                }
                else
                {
                    int cy = Height - tp; // invert
                    int cx = Width / 2;
                    bool right = _tickStyle == TickStyle.BottomRight || _tickStyle == TickStyle.Both;
                    bool left  = _tickStyle == TickStyle.TopLeft     || _tickStyle == TickStyle.Both;
                    if (right) g.DrawLine(tickPen, cx + ThumbH / 2 - 1, cy, cx + ThumbH / 2 + 3, cy);
                    if (left)  g.DrawLine(tickPen, cx - ThumbH / 2 + 1, cy, cx - ThumbH / 2 - 3, cy);
                }
            }
        }

        // ── Thumb ────────────────────────────────────────────────────────────
        var thumb = GetThumbRect();
        var thumbFill  = _dragging ? Color.FromArgb(0, 84, 166) :
                         Enabled   ? Color.FromArgb(0, 120, 215) :
                                     Color.FromArgb(188, 188, 188);
        using (var tb = new SolidBrush(thumbFill))
            g.FillRectangle(tb, thumb);
        using (var tp2 = new Pen(Focused ? Color.FromArgb(0, 84, 166) : Color.FromArgb(0, 90, 158)))
            g.DrawRectangle(tp2, thumb);

        // Focus outline
        if (Focused)
            DrawFocusRect(g, new Rectangle(1, 1, Width - 2, Height - 2));

        base.OnPaint(e);
    }

    // ── Mouse ─────────────────────────────────────────────────────────────────

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && Enabled)
        {
            Focus();
            var thumb = GetThumbRect();
            bool horiz = _orientation == Orientation.Horizontal;
            int px = horiz ? e.X : e.Y;
            if (thumb.Contains(e.X, e.Y))
            {
                _dragging    = true;
                _dragStartPx  = px;
                _dragStartVal = _value;
                Capture = true;   // lock mouse routing to this control
            }
            else
            {
                // Click on track — jump to nearest value
                Value = PixelToValue(horiz ? e.X : (Height - e.Y));
            }
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        if (_dragging)
        {
            bool horiz = _orientation == Orientation.Horizontal;
            int px = horiz ? e.X : (Height - e.Y);
            Value = PixelToValue(px);
        }
        base.OnMouseMove(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        if (_dragging)
        {
            _dragging = false;
            Capture   = false;   // release mouse routing lock
            Invalidate();
        }
        base.OnMouseUp(e);
    }

    // ── Keyboard ──────────────────────────────────────────────────────────────

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        bool horiz = _orientation == Orientation.Horizontal;
        switch (e.KeyCode)
        {
            case Keys.Left:
            case Keys.Down:
                Value -= _smallChange;
                e.Handled = true;
                break;
            case Keys.Right:
            case Keys.Up:
                Value += _smallChange;
                e.Handled = true;
                break;
            case Keys.PageDown:
                Value -= _largeChange;
                e.Handled = true;
                break;
            case Keys.PageUp:
                Value += _largeChange;
                e.Handled = true;
                break;
            case Keys.Home:
                Value = _minimum;
                e.Handled = true;
                break;
            case Keys.End:
                Value = _maximum;
                e.Handled = true;
                break;
        }
        base.OnKeyDown(e);
    }

    // ── WinForms API stubs ────────────────────────────────────────────────────

    /// <summary>Sets the tick range without raising events (WinForms compat).</summary>
    public void SetRange(int minValue, int maxValue)
    {
        _minimum = minValue;
        _maximum = maxValue;
        _value = Math.Max(_minimum, Math.Min(_maximum, _value));
        Invalidate();
    }
}

/// <summary>Specifies the location of tick marks on a TrackBar.</summary>
public enum TickStyle
{
    None        = 0,
    TopLeft     = 1,
    BottomRight = 2,
    Both        = 3,
}
