namespace System.Windows.Forms;

// ── MenuStrip ─────────────────────────────────────────────────────────────────
/// <summary>
/// A horizontal menu bar docked at the top of a Form.
/// Draws its top-level ToolStripMenuItems as clickable tabs;
/// their dropdowns are painted as overlays by Form.cs.
/// </summary>
public class MenuStrip : ToolStrip
{
    // ── Appearance ─────────────────────────────────────────────────────────────
    private const int ItemPadH    = 12;  // horizontal padding around each top-level item text
    private const int ItemHeight  = 24;  // matches control height

    private static readonly CanvasColor BgColor     = CanvasColor.FromArgb(240, 240, 240);
    private static readonly CanvasColor HoverBg     = CanvasColor.FromArgb(0,   120, 215);
    private static readonly CanvasColor OpenBg      = CanvasColor.FromArgb(255, 255, 255);
    private static readonly CanvasColor NormalText  = CanvasColor.FromArgb(0,   0,   0);
    private static readonly CanvasColor HoverText   = CanvasColor.FromArgb(255, 255, 255);
    private static readonly CanvasColor OpenText    = CanvasColor.FromArgb(0,   0,   0);
    private static readonly CanvasColor BottomBorder= CanvasColor.FromArgb(200, 200, 200);

    private int _hoveredIndex = -1;

    // ── Constructor ────────────────────────────────────────────────────────────

    public MenuStrip()
    {
        Dock      = DockStyle.Top;
        Height    = ItemHeight;
        BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
    }

    // ── MDI ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// The ToolStripMenuItem used as the MDI Window-list placeholder (stub — always null).
    /// </summary>
    public ToolStripMenuItem? MdiWindowListItem { get; set; }

    // ── Events ─────────────────────────────────────────────────────────────────

    /// <summary>Raised when the user activates the menu (clicks any top-level item).</summary>
    public event EventHandler? MenuActivate;

    /// <summary>Raised when the menu is deactivated (all dropdowns closed).</summary>
    public event EventHandler? MenuDeactivate;

    protected virtual void OnMenuActivate(EventArgs e)   => MenuActivate?.Invoke(this, e);
    protected virtual void OnMenuDeactivate(EventArgs e) => MenuDeactivate?.Invoke(this, e);

    // ── Activation ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Activates the MenuStrip — gives it keyboard focus and raises MenuActivate.
    /// </summary>
    public void Activate()
    {
        Focus();
        OnMenuActivate(EventArgs.Empty);
    }

    // ── Layout helpers ─────────────────────────────────────────────────────────

    /// <summary>Computes the left-edge X of each top-level item.</summary>
    private List<(int x, int w)> ComputeItemLayout()
    {
        var result = new List<(int, int)>();
        int x = 2;
        foreach (var item in Items)
        {
            if (!item.Visible) { result.Add((x, 0)); continue; }
            int w = EstimateTextWidth(item.Text) + ItemPadH * 2;
            result.Add((x, w));
            x += w;
        }
        return result;
    }

    /// <summary>Returns the item index hit by (localX, localY), or -1.</summary>
    private int GetItemIndexAt(int localX, int localY)
    {
        if (localY < 0 || localY >= Height) return -1;
        var layout = ComputeItemLayout();
        for (int i = 0; i < layout.Count; i++)
        {
            var (lx, lw) = layout[i];
            if (lw > 0 && localX >= lx && localX < lx + lw) return i;
        }
        return -1;
    }

    // ── Painting ───────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        using var bgBrush = new SolidBrush(BgColor);
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        // Bottom border
        using var borderPen = new Pen(BottomBorder);
        g.DrawLine(borderPen, 0, Height - 1, Width, Height - 1);

        var layout = ComputeItemLayout();
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (!item.Visible) continue;

            var (lx, lw) = layout[i];
            if (lw == 0) continue;

            bool isOpen    = item is ToolStripMenuItem mi && mi.DropDownIsOpen;
            bool isHovered = i == _hoveredIndex && item.Enabled;

            CanvasColor bg = isOpen   ? OpenBg
                            : isHovered ? HoverBg
                            : BgColor;

            using var itemBrush = new SolidBrush(bg);
            g.FillRectangle(itemBrush, lx, 0, lw, Height - 1);

            if (isOpen)
            {
                // Draw a border around open item (tab effect)
                using var openPen = new Pen(BottomBorder);
                g.DrawLine(openPen, lx,      0,          lx,       Height - 1);
                g.DrawLine(openPen, lx + lw, 0,          lx + lw,  Height - 1);
                g.DrawLine(openPen, lx,      0,          lx + lw,  0);
                // Erase bottom border under open item so it "connects" to dropdown
                using var erasePen = new Pen(OpenBg);
                g.DrawLine(erasePen, lx + 1, Height - 1, lx + lw - 1, Height - 1);
            }

            CanvasColor textColor = isOpen ? OpenText : (isHovered ? HoverText : NormalText);
            if (!item.Enabled)
                textColor = CanvasColor.FromArgb(109, 109, 109);

            g.DrawString(item.Text, lx + ItemPadH, (Height - 14) / 2, textColor);
        }
    }

    // ── Mouse interaction ──────────────────────────────────────────────────────

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        int idx = GetItemIndexAt(e.X, e.Y);
        if (idx == _hoveredIndex) return;
        _hoveredIndex = idx;
        Invalidate();
        base.OnMouseMove(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) { base.OnMouseDown(e); return; }

        int idx = GetItemIndexAt(e.X, e.Y);
        if (idx < 0 || idx >= Items.Count) { base.OnMouseDown(e); return; }

        var item = Items[idx];
        if (!item.Enabled || item is not ToolStripMenuItem mi) { base.OnMouseDown(e); return; }

        bool wasOpen = mi.DropDownIsOpen;
        CloseAllTopLevelDropDowns();

        if (!wasOpen && mi.HasDropDownItems)
        {
            OnMenuActivate(EventArgs.Empty);
            var layout   = ComputeItemLayout();
            var (lx, lw) = layout[idx];
            var formPt   = GetFormPosition();
            mi.OpenDropDown(new Point(formPt.X + lx, formPt.Y + Height));
        }
        else if (!mi.HasDropDownItems)
        {
            mi.OnClick(EventArgs.Empty);
        }
        else
        {
            // Toggled closed — fire deactivate
            OnMenuDeactivate(EventArgs.Empty);
        }

        Invalidate();
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        _hoveredIndex = -1;
        Invalidate();
        base.OnMouseLeave(e);
    }

    // ── Public simulation helpers (used by tests and host routing) ─────────────

    /// <summary>Simulates a mouse-down event on the strip (delegates to OnMouseDown).</summary>
    public void SimulateMouseDown(MouseEventArgs e) => OnMouseDown(e);

    /// <summary>Simulates a mouse-move event on the strip.</summary>
    public void SimulateMouseMove(MouseEventArgs e) => OnMouseMove(e);

    /// <summary>Simulates a mouse-leave event on the strip.</summary>
    public void SimulateMouseLeave() => OnMouseLeave(EventArgs.Empty);

    // ── Internal helpers ───────────────────────────────────────────────────────

    private void CloseAllTopLevelDropDowns()
    {
        foreach (var item in Items)
        {
            if (item is ToolStripMenuItem mi && mi.DropDownIsOpen)
                mi.CloseDropDown();
        }
    }

    /// <summary>Returns the control's top-left corner in Form coordinates.</summary>
    private Point GetFormPosition()
    {
        int x = Left, y = Top;
        var p = Parent;
        while (p != null && p is not Form)
        {
            x += p.Left;
            y += p.Top;
            p  = p.Parent;
        }
        return new Point(x, y);
    }

    private static int EstimateTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return text.Length * 7;
    }
}
