namespace System.Windows.Forms;

// ── ToolStripDropDown ─────────────────────────────────────────────────────────
/// <summary>
/// A floating vertical list of ToolStripItems.  Used as the dropdown for
/// ToolStripMenuItems (both from MenuStrip and ContextMenuStrip).
///
/// Not rendered as a child Control — it is painted as an overlay by Form.cs
/// using GetDropDownBounds() + PaintDropDown(), exactly like ComboBox.
/// </summary>
public class ToolStripDropDown : ToolStrip
{
    // ── Layout constants ───────────────────────────────────────────────────────
    internal const int ItemHeight    = 22;
    internal const int SeparatorH    = 8;
    internal const int CheckColW     = 20;   // width of check/image column
    internal const int ArrowColW     = 16;   // width of submenu-arrow column
    internal const int HorzPad       = 6;    // text horizontal padding
    internal const int MinDropWidth  = 140;
    internal const int BorderThick   = 1;

    // ── Appearance ─────────────────────────────────────────────────────────────
    private static readonly CanvasColor BgColor       = CanvasColor.FromArgb(255, 255, 255);
    private static readonly CanvasColor BorderColor   = CanvasColor.FromArgb(160, 160, 160);
    private static readonly CanvasColor HoverBg       = CanvasColor.FromArgb(0,   120, 215);
    private static readonly CanvasColor HoverText     = CanvasColor.FromArgb(255, 255, 255);
    private static readonly CanvasColor NormalText    = CanvasColor.FromArgb(0,   0,   0);
    private static readonly CanvasColor DisabledText  = CanvasColor.FromArgb(109, 109, 109);
    private static readonly CanvasColor SepColor      = CanvasColor.FromArgb(210, 210, 210);

    // ── State ──────────────────────────────────────────────────────────────────
    private bool _isVisible;
    private int  _hoveredIndex = -1;
    private bool _autoClose    = true;
    private bool _dropShadowEnabled = true;
    private double _opacity = 1.0;

    /// <summary>Owner item that opened this dropdown (null for ContextMenuStrip root).</summary>
    public ToolStripItem?  SourceItem    { get; internal set; }

    /// <summary>
    /// The ToolStripItem that owns this dropdown (alias for SourceItem, matches WinForms API).
    /// </summary>
    public ToolStripItem? OwnerItem => SourceItem;

    /// <summary>Position (in form coordinates) where the dropdown top-left is anchored.</summary>
    public Point           PopupLocation { get; set; }

    public bool IsVisible
    {
        get => _isVisible;
        set { _isVisible = value; FindForm()?.Invalidate(); }
    }

    /// <summary>
    /// Whether the dropdown closes automatically when the user clicks outside it.
    /// Functional: maps to close-on-outside-click behaviour in Form.cs.
    /// </summary>
    public bool AutoClose
    {
        get => _autoClose;
        set => _autoClose = value;
    }

    /// <summary>Whether a drop shadow is drawn beneath the dropdown (stub — canvas uses box-shadow via CSS).</summary>
    public bool DropShadowEnabled
    {
        get => _dropShadowEnabled;
        set => _dropShadowEnabled = value;
    }

    /// <summary>Opacity of the dropdown (stub — canvas rendering is always fully opaque).</summary>
    public double Opacity
    {
        get => _opacity;
        set => _opacity = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>Whether this control can be a top-level window (always false — overlay model).</summary>
    public bool TopLevel { get; set; } = false;

    /// <summary>Whether the window is always on top (always true for dropdowns — stub).</summary>
    public bool TopMost { get; set; } = true;

    /// <summary>Whether the dropdown is constrained to the working area (stub — always true).</summary>
    public bool WorkingAreaConstrained { get; set; } = true;

    /// <summary>Transparency key colour (stub — not used in canvas rendering).</summary>
    public System.Drawing.Color TransparencyKey { get; set; } = System.Drawing.Color.Empty;

    /// <summary>Whether the dropdown supports per-pixel transparency (stub — canvas is always opaque).</summary>
    public bool AllowTransparency { get; set; } = false;

    // ── ToolStrip overrides ────────────────────────────────────────────────────

    public override bool IsDropDown => true;

    public ToolStripDropDown() : base()
    {
        TabStop = false;
    }

    // ── Geometry ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Computes the bounding rectangle of this dropdown in owner-control-relative
    /// coordinates (the same coordinate system used by ComboBox.GetDropDownBounds).
    /// The caller (Form.cs) adds the owner's absolute offset.
    /// </summary>
    public Rectangle GetDropDownBounds(int ownerAbsLeft, int ownerAbsTop)
    {
        var w = ComputeDropWidth();
        var h = ComputeDropHeight();
        return new Rectangle(
            PopupLocation.X - ownerAbsLeft,
            PopupLocation.Y - ownerAbsTop,
            w, h);
    }

    public int ComputeDropWidth()
    {
        int maxText = 0;
        foreach (var item in Items)
        {
            if (!item.Visible) continue;
            maxText = Math.Max(maxText, EstimateTextWidth(item.Text));
        }
        return Math.Max(MinDropWidth, CheckColW + HorzPad + maxText + HorzPad + ArrowColW) + BorderThick * 2;
    }

    public int ComputeDropHeight()
    {
        int h = BorderThick * 2;
        foreach (var item in Items)
        {
            if (!item.Visible) continue;
            h += item is ToolStripSeparator ? SeparatorH : ItemHeight;
        }
        return h;
    }

    // ── Drawing ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Paints the dropdown at (0,0) within a Graphics clipped to the dropdown bounds.
    /// </summary>
    public void PaintDropDown(Graphics g)
    {
        var w = ComputeDropWidth();
        var h = ComputeDropHeight();

        // Background + border
        using var bgBrush     = new SolidBrush(BgColor);
        using var borderPen   = new Pen(BorderColor);
        g.FillRectangle(bgBrush, 0, 0, w, h);
        g.DrawRectangle(borderPen, 0, 0, w - 1, h - 1);

        int y = BorderThick;
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (!item.Visible) continue;

            if (item is ToolStripSeparator)
            {
                // Separator line
                using var sepPen = new Pen(SepColor);
                int midY = y + SeparatorH / 2;
                g.DrawLine(sepPen, CheckColW, midY, w - BorderThick, midY);
                y += SeparatorH;
                continue;
            }

            var itemRect = new Rectangle(BorderThick, y, w - BorderThick * 2, ItemHeight);
            bool isHovered  = i == _hoveredIndex && item.Enabled;
            bool isDisabled = !item.Enabled;

            // Hover background
            if (isHovered)
            {
                using var hoverBrush = new SolidBrush(HoverBg);
                g.FillRectangle(hoverBrush, itemRect);
            }

            var textColor = isDisabled ? DisabledText : (isHovered ? HoverText : NormalText);

            // Check mark (if ToolStripMenuItem is checked)
            if (item is ToolStripMenuItem { Checked: true })
            {
                using var checkPen = new Pen(textColor, 1.5f);
                int cx = BorderThick + CheckColW / 2 - 5;
                int cy = y + (ItemHeight - 10) / 2;
                g.DrawLine(checkPen, cx,     cy + 5,  cx + 4,  cy + 9);
                g.DrawLine(checkPen, cx + 4, cy + 9,  cx + 10, cy + 1);
            }

            // Item text — vertically centered within the item row
            g.DrawString(item.Text, CheckColW + HorzPad, y + (ItemHeight - 14) / 2, textColor);

            // Submenu arrow ▶
            if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
            {
                using var arrowPen = new Pen(textColor, 1.5f);
                int ax = w - ArrowColW;
                int ay = y + ItemHeight / 2;
                g.DrawLine(arrowPen, ax,     ay - 4, ax + 4,  ay);
                g.DrawLine(arrowPen, ax + 4, ay,     ax,      ay + 4);
            }

            y += ItemHeight;
        }
    }

    // ── Show overloads ─────────────────────────────────────────────────────────

    /// <summary>Shows the dropdown at the given screen/form coordinates.</summary>
    public void Show(Point screenLocation)
    {
        PopupLocation = screenLocation;
        IsVisible     = true;
    }

    /// <summary>Shows the dropdown at an absolute position.</summary>
    public void Show(int x, int y) => Show(new Point(x, y));

    /// <summary>Shows the dropdown relative to a control with an optional direction.</summary>
    public void Show(Control control, Point offset, ToolStripDropDownDirection direction = ToolStripDropDownDirection.BelowRight)
    {
        var formPt = GetControlFormPosition(control);
        PopupLocation = new Point(formPt.X + offset.X, formPt.Y + offset.Y);
        IsVisible     = true;
    }

    /// <summary>Shows the dropdown relative to a control.</summary>
    public void Show(Control control, int x, int y)
        => Show(control, new Point(x, y));

    private static Point GetControlFormPosition(Control control)
    {
        int x = control.Left, y = control.Top;
        var p = control.Parent;
        while (p is not null and not Form) { x += p.Left; y += p.Top; p = p.Parent; }
        return new Point(x, y);
    }

    // ── Close overloads ────────────────────────────────────────────────────────

    /// <summary>Closes the dropdown (CloseReason = CloseCalled).</summary>
    public void Close() => Close(ToolStripDropDownCloseReason.CloseCalled);

    /// <summary>Closes the dropdown with the specified reason, raising the Closing event.</summary>
    public void Close(ToolStripDropDownCloseReason reason)
    {
        if (!IsVisible) return;
        var args = new ToolStripDropDownClosingEventArgs(reason);
        OnClosing(args);
        if (!args.Cancel)
            CloseChain();
    }

    // ── Mouse interaction ──────────────────────────────────────────────────────

    /// <summary>Index of the item at local-y coordinate, or -1.</summary>
    public int GetItemIndexAt(int localY)
    {
        int y = BorderThick;
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (!item.Visible) continue;
            int h = item is ToolStripSeparator ? SeparatorH : ItemHeight;
            if (localY >= y && localY < y + h) return i;
            y += h;
        }
        return -1;
    }

    public void HandleMouseMove(int localX, int localY)
    {
        var idx = GetItemIndexAt(localY);
        if (idx == _hoveredIndex) return;

        if (_hoveredIndex >= 0 && _hoveredIndex < Items.Count)
            Items[_hoveredIndex].OnMouseLeave(EventArgs.Empty);

        _hoveredIndex = idx;

        if (idx >= 0 && idx < Items.Count)
            Items[idx].OnMouseEnter(EventArgs.Empty);

        FindForm()?.Invalidate();
    }

    public void HandleMouseDown(int localX, int localY)
    {
        var idx = GetItemIndexAt(localY);
        if (idx < 0 || idx >= Items.Count) return;

        var item = Items[idx];
        if (!item.Enabled || item is ToolStripSeparator) return;

        if (item is ToolStripMenuItem mi && mi.HasDropDownItems)
        {
            // Open submenu (position to the right)
            var w = ComputeDropWidth();
            mi.OpenDropDown(new Point(PopupLocation.X + w - BorderThick, PopupLocation.Y + GetItemTopY(idx)));
        }
        else
        {
            item.OnClick(EventArgs.Empty);
            // Close this dropdown and all parents after item click
            CloseChain();
        }
    }

    internal int GetItemTopY(int index)
    {
        int y = BorderThick;
        for (int i = 0; i < index && i < Items.Count; i++)
        {
            var item = Items[i];
            if (!item.Visible) continue;
            y += item is ToolStripSeparator ? SeparatorH : ItemHeight;
        }
        return y;
    }

    public void CloseChain()
    {
        IsVisible = false;
        _hoveredIndex = -1;

        // Cascade-close any open submenus in our items
        foreach (var item in Items)
        {
            if (item is ToolStripMenuItem mi && mi.DropDown.IsVisible)
                mi.DropDown.CloseChain();
        }
    }

    // ── Events ─────────────────────────────────────────────────────────────────

    public event ToolStripDropDownClosingEventHandler? Closing;
    public event EventHandler?                         Opened;
    public event EventHandler?                         Opening;
    public event EventHandler? Scroll; // stub — canvas does not scroll dropdowns

    protected virtual void OnClosing(ToolStripDropDownClosingEventArgs e)
        => Closing?.Invoke(this, e);

    protected virtual void OnOpened(EventArgs e)  => Opened?.Invoke(this, e);
    protected virtual void OnOpening(EventArgs e) => Opening?.Invoke(this, e);
    protected virtual void OnScroll(EventArgs e) => Scroll?.Invoke(this, e);

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static int EstimateTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return text.Length * 7; // approximate; good enough for width calculation
    }
}

// ── ToolStripDropDownMenu ─────────────────────────────────────────────────────
/// <summary>
/// The specific dropdown used by MenuStrip items and ContextMenuStrip.
/// Inherits ToolStripDropDown and adds no extra behaviour — present for
/// WinForms API compatibility (typeof checks, designer assignments).
/// </summary>
public class ToolStripDropDownMenu : ToolStripDropDown { }
