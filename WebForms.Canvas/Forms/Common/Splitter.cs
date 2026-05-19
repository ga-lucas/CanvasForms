namespace System.Windows.Forms;

/// <summary>
/// Legacy docking splitter control (pre-<see cref="SplitContainer"/>).
/// Docks to an edge of its parent; dragging it resizes the preceding docked control.
/// Matches the WinForms <c>Splitter</c> hierarchy: <c>Control → Splitter</c>.
/// </summary>
public class Splitter : Control
{
    // ── Fields ────────────────────────────────────────────────────────────────

    private int _minSize       = 25;
    private int _minExtra      = 25;
    private int _splitPosition = -1;    // -1 = let WinForms determine
    private bool _dragging;
    private int _dragStart;             // mouse coordinate at drag-start
    private int _targetSizeAtStart;     // target control size at drag-start
    private Control? _targetControl;

    // ── Constructor ────────────────────────────────────────────────────────────

    public Splitter()
    {
        // Default dock + size mimick WinForms defaults
        Dock   = DockStyle.Left;
        Width  = 3;
        Height = 3;
        Cursor = Cursor.VSplit;
        BackColor = Color.FromArgb(212, 208, 200); // WinForms default
        TabStop   = false;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>Minimum size (in pixels) of the control that the splitter resizes.</summary>
    public int MinSize
    {
        get => _minSize;
        set { _minSize = Math.Max(0, value); }
    }

    /// <summary>Minimum size (in pixels) of the space remaining after the splitter.</summary>
    public int MinExtra
    {
        get => _minExtra;
        set { _minExtra = Math.Max(0, value); }
    }

    /// <summary>
    /// Current split position (distance from the docked edge of the parent).
    /// -1 means WinForms should determine the position from the target control.
    /// </summary>
    public int SplitPosition
    {
        get
        {
            var tgt = FindTarget();
            return tgt is null ? _splitPosition : IsVertical ? tgt.Width : tgt.Height;
        }
        set
        {
            _splitPosition = value;
            ApplySplitPosition(value);
        }
    }

    // ── Events ─────────────────────────────────────────────────────────────────

    public event SplitterEventHandler? SplitterMoving;
    public event SplitterEventHandler? SplitterMoved;

    protected virtual void OnSplitterMoving(SplitterEventArgs e) => SplitterMoving?.Invoke(this, e);
    protected virtual void OnSplitterMoved(SplitterEventArgs e)  => SplitterMoved?.Invoke(this, e);

    // ── Cursor follows dock side ───────────────────────────────────────────────

    public new DockStyle Dock
    {
        get => base.Dock;
        set
        {
            base.Dock = value;
            Cursor = (value == DockStyle.Top || value == DockStyle.Bottom)
                ? Cursor.HSplit
                : Cursor.VSplit;
            // Also set default width/height
            if (value == DockStyle.Left || value == DockStyle.Right)
                Width  = 3;
            else if (value == DockStyle.Top || value == DockStyle.Bottom)
                Height = 3;
        }
    }

    // ── Mouse drag ────────────────────────────────────────────────────────────

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) { base.OnMouseDown(e); return; }

        _targetControl = FindTarget();
        if (_targetControl is null) { base.OnMouseDown(e); return; }

        _dragging = true;
        _dragStart = IsVertical ? e.X + Left : e.Y + Top;
        _targetSizeAtStart = IsVertical ? _targetControl.Width : _targetControl.Height;
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        if (!_dragging || _targetControl is null) { base.OnMouseMove(e); return; }

        int current  = IsVertical ? e.X + Left : e.Y + Top;
        int delta    = current - _dragStart;
        int newSize  = ClampSize(_targetSizeAtStart + delta);

        var args = new SplitterEventArgs(e.X, e.Y, IsVertical ? Left + delta : Left, IsVertical ? Top : Top + delta);
        OnSplitterMoving(args);

        ApplyTargetSize(_targetControl, newSize);
        base.OnMouseMove(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        if (_dragging)
        {
            _dragging = false;
            int current = IsVertical ? e.X + Left : e.Y + Top;
            int delta   = current - _dragStart;
            int newSize = ClampSize(_targetSizeAtStart + delta);
            var args = new SplitterEventArgs(e.X, e.Y, IsVertical ? Left + delta : Left, IsVertical ? Top : Top + delta);
            OnSplitterMoved(args);
            ApplyTargetSize(_targetControl!, newSize);
        }
        base.OnMouseUp(e);
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        using var bgBrush = new SolidBrush(Drawing.Color.FromArgb(BackColor.R, BackColor.G, BackColor.B));
        g.FillRectangle(bgBrush, 0, 0, Width, Height);
        base.OnPaint(e);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private bool IsVertical => Dock == DockStyle.Left || Dock == DockStyle.Right;

    /// <summary>
    /// Finds the control immediately before (in z-order / Controls index) this
    /// splitter that is docked to the same edge as us.
    /// </summary>
    private Control? FindTarget()
    {
        if (Parent is null) return null;
        int myIdx = Parent._controls.IndexOf(this);
        // Walk backwards through the Controls collection
        for (int i = myIdx - 1; i >= 0; i--)
        {
            var c = Parent._controls[i];
            if (!c.Visible) continue;
            if (c.Dock == Dock)
                return c;
        }
        return null;
    }

    private int ClampSize(int size)
    {
        int parentSize = IsVertical ? (Parent?.Width ?? 200) : (Parent?.Height ?? 200);
        size = Math.Max(size, _minSize);
        size = Math.Min(size, parentSize - _minExtra);
        return size;
    }

    private static void ApplyTargetSize(Control target, int newSize)
    {
        bool isW = target.Dock == DockStyle.Left || target.Dock == DockStyle.Right;
        if (isW) target.Width = newSize;
        else     target.Height = newSize;
    }

    private void ApplySplitPosition(int pos)
    {
        var tgt = FindTarget();
        if (tgt is null) return;
        ApplyTargetSize(tgt, ClampSize(pos));
    }
}

