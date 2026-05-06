namespace System.Windows.Forms;

// ── ToolStripPanel ────────────────────────────────────────────────────────────
// A docking band that holds ToolStrips along one edge of a ToolStripContainer.
// Children (ToolStrips) are stacked in a row matching the panel Orientation;
// the panel auto-resizes to the tallest/widest child and becomes visible
// automatically when the first child is added (matching WinForms behaviour).

public class ToolStripPanel : ContainerControl
{
    // Gap between stacked ToolStrips on the same band.
    private const int Strip_Gap = 1;

    public ToolStripPanel()
    {
        IsMouseRoutingContainer = true;
        TabStop = false;
        BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        // Hidden until a ToolStrip is docked — matches WinForms default.
        base.Visible = false;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>
    /// Orientation of the docking band (Horizontal for Top/Bottom, Vertical for Left/Right).
    /// </summary>
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    // ── Auto-visibility ───────────────────────────────────────────────────────

    /// <summary>
    /// Becomes visible when the first child is added; hidden when the last is removed.
    /// The property can still be set explicitly to override this behaviour.
    /// </summary>
    public new bool Visible
    {
        get => base.Visible;
        set => base.Visible = value;
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        if (Controls.Count > 0)
            base.Visible = true;
        PerformLayout();
        Parent?.PerformLayout();
    }

    protected override void OnControlRemoved(ControlEventArgs e)
    {
        base.OnControlRemoved(e);
        if (Controls.Count == 0)
            base.Visible = false;
        PerformLayout();
        Parent?.PerformLayout();
    }

    // ── Layout ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Positions children in a row (horizontal band) or column (vertical band).
    /// Resizes the panel to the minimum bounding size for all visible children.
    /// </summary>
    public override void PerformLayout()
    {
        base.PerformLayout();

        var visible = Controls.Where(c => c.Visible).ToList();
        if (visible.Count == 0) return;

        if (Orientation == Orientation.Horizontal)
        {
            // Stack left-to-right; panel height = tallest child.
            int x = 0;
            int maxH = 0;
            foreach (var c in visible)
            {
                c.Left = x;
                c.Top  = 0;
                // ToolStrip children that fill width will respect Width set by LayoutBands;
                // others keep their own width.
                x  += c.Width + Strip_Gap;
                maxH = Math.Max(maxH, c.Height);
            }
            if (maxH > 0 && Height != maxH)
                Height = maxH;
        }
        else
        {
            // Stack top-to-bottom; panel width = widest child.
            int y = 0;
            int maxW = 0;
            foreach (var c in visible)
            {
                c.Left = 0;
                c.Top  = y;
                y  += c.Height + Strip_Gap;
                maxW = Math.Max(maxW, c.Width);
            }
            if (maxW > 0 && Width != maxW)
                Width = maxW;
        }
    }

    // ── Painting ──────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        if (!Visible || Width <= 0 || Height <= 0) return;
        DrawControlBackground(e.Graphics);
        base.OnPaint(e);
    }
}

// ── ToolStripContentPanel ─────────────────────────────────────────────────────
// The centre region of a ToolStripContainer that hosts the main content.

public class ToolStripContentPanel : Panel
{
    public ToolStripContentPanel()
    {
        // Matches WinForms default.
        BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
    }

    /// <summary>
    /// Called when the content panel is first rendered.  Override in derived classes.
    /// </summary>
    public event PaintEventHandler? RenderedChanged;

    protected virtual void OnRenderedChanged(PaintEventArgs e) => RenderedChanged?.Invoke(this, e);
}

// ── ToolStripContainer ────────────────────────────────────────────────────────
// A container that wraps a central ToolStripContentPanel with four docking bands
// (Top, Bottom, Left, Right) that host ToolStrips.
//
// Designer-generated code does:
//   toolStripContainer.TopToolStripPanel.Controls.Add(menuStrip);
//   toolStripContainer.ContentPanel.Controls.Add(mainPanel);
// This stub makes that code compile and route mouse events correctly.

public class ToolStripContainer : ContainerControl
{
    private readonly ToolStripPanel       _top;
    private readonly ToolStripPanel       _bottom;
    private readonly ToolStripPanel       _left;
    private readonly ToolStripPanel       _right;
    private readonly ToolStripContentPanel _content;

    // Minimum band size — used only when a panel is visible but has no measurable children yet.
    private const int MinBandThickness = 24;

    public ToolStripContainer()
    {
        IsMouseRoutingContainer = true;
        TabStop = false;

        _top     = new ToolStripPanel { Orientation = Orientation.Horizontal };
        _bottom  = new ToolStripPanel { Orientation = Orientation.Horizontal };
        _left    = new ToolStripPanel { Orientation = Orientation.Vertical };
        _right   = new ToolStripPanel { Orientation = Orientation.Vertical };
        _content = new ToolStripContentPanel();

        base.Controls.Add(_top);
        base.Controls.Add(_bottom);
        base.Controls.Add(_left);
        base.Controls.Add(_right);
        base.Controls.Add(_content);
    }

    // ── Public accessors ──────────────────────────────────────────────────────

    public ToolStripPanel        TopToolStripPanel    => _top;
    public ToolStripPanel        BottomToolStripPanel => _bottom;
    public ToolStripPanel        LeftToolStripPanel   => _left;
    public ToolStripPanel        RightToolStripPanel  => _right;
    public ToolStripContentPanel ContentPanel         => _content;

    // ── Layout ────────────────────────────────────────────────────────────────

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutBands();
    }

    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        LayoutBands();
    }

    private void LayoutBands()
    {
        if (Width <= 0 || Height <= 0) return;

        // ── Pass 1: give horizontal bands their full container width so child
        //   ToolStrips can stretch, then measure their natural height from children.
        if (_top.Visible)
        {
            _top.Left  = 0; _top.Top = 0; _top.Width = Width;
            _top.PerformLayout();
        }
        if (_bottom.Visible)
        {
            _bottom.Left = 0; _bottom.Width = Width;
            _bottom.PerformLayout();
        }

        // Height from children; fall back to minimum if panel has no children yet.
        int topH    = _top.Visible    ? Math.Max(MinBandThickness, _top.Height)    : 0;
        int bottomH = _bottom.Visible ? Math.Max(MinBandThickness, _bottom.Height) : 0;

        int innerH = Math.Max(0, Height - topH - bottomH);

        // ── Pass 2: give vertical bands their full inner height, measure width.
        if (_left.Visible)
        {
            _left.Left = 0; _left.Top = topH; _left.Height = innerH;
            _left.PerformLayout();
        }
        if (_right.Visible)
        {
            _right.Top = topH; _right.Height = innerH;
            _right.PerformLayout();
        }

        int leftW  = _left.Visible  ? Math.Max(MinBandThickness, _left.Width)  : 0;
        int rightW = _right.Visible ? Math.Max(MinBandThickness, _right.Width) : 0;

        // ── Pass 3: commit final positions now that all sizes are known.
        _top.Left = 0; _top.Top = 0; _top.Width = Width; _top.Height = topH;

        _bottom.Left = 0; _bottom.Top = Height - bottomH;
        _bottom.Width = Width; _bottom.Height = bottomH;

        _left.Left = 0; _left.Top = topH; _left.Width = leftW; _left.Height = innerH;

        _right.Left = Width - rightW; _right.Top = topH;
        _right.Width = rightW; _right.Height = innerH;

        _content.Left   = leftW;
        _content.Top    = topH;
        _content.Width  = Math.Max(0, Width  - leftW - rightW);
        _content.Height = Math.Max(0, innerH);

        _content.PerformLayout();
    }

    // ── Painting ──────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        DrawControlBackground(e.Graphics);
        base.OnPaint(e);
    }
}
