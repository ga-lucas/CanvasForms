namespace System.Windows.Forms;

// ── ToolStripPanel ────────────────────────────────────────────────────────────
// A docking band that holds ToolStrips along one edge of a ToolStripContainer.
// Currently a structural stub — real ToolStrip hosting is implemented with menus/toolbars.

public class ToolStripPanel : ContainerControl
{
    public ToolStripPanel()
    {
        IsMouseRoutingContainer = true;
        TabStop = false;
        BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
    }

    /// <summary>
    /// Orientation of the docking band (Horizontal for Top/Bottom, Vertical for Left/Right).
    /// </summary>
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    /// <summary>
    /// Whether this panel is visible even when it contains no ToolStrips.
    /// Matches WinForms default of false.
    /// </summary>
    public bool Visible { get; set; } = false;

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

    // Typical ToolStrip heights/widths matching WinForms defaults.
    private const int BandThickness = 25;

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

        // Measure each band: visible bands use BandThickness, hidden ones collapse to 0.
        int topH    = _top.Visible    ? Math.Max(BandThickness, _top.Height)    : 0;
        int bottomH = _bottom.Visible ? Math.Max(BandThickness, _bottom.Height) : 0;
        int leftW   = _left.Visible   ? Math.Max(BandThickness, _left.Width)    : 0;
        int rightW  = _right.Visible  ? Math.Max(BandThickness, _right.Width)   : 0;

        // Top band — full width.
        _top.Left   = 0;
        _top.Top    = 0;
        _top.Width  = Width;
        _top.Height = topH;

        // Bottom band — full width.
        _bottom.Left   = 0;
        _bottom.Top    = Height - bottomH;
        _bottom.Width  = Width;
        _bottom.Height = bottomH;

        // Left band — between top and bottom.
        _left.Left   = 0;
        _left.Top    = topH;
        _left.Width  = leftW;
        _left.Height = Math.Max(0, Height - topH - bottomH);

        // Right band.
        _right.Left   = Width - rightW;
        _right.Top    = topH;
        _right.Width  = rightW;
        _right.Height = Math.Max(0, Height - topH - bottomH);

        // Content panel fills the remaining centre area.
        _content.Left   = leftW;
        _content.Top    = topH;
        _content.Width  = Math.Max(0, Width  - leftW - rightW);
        _content.Height = Math.Max(0, Height - topH  - bottomH);

        _top.PerformLayout();
        _bottom.PerformLayout();
        _left.PerformLayout();
        _right.PerformLayout();
        _content.PerformLayout();
    }

    // ── Painting ──────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        DrawControlBackground(e.Graphics);
        base.OnPaint(e);
    }
}
