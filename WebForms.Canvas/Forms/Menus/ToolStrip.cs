namespace System.Windows.Forms;
// ── ToolStrip ─────────────────────────────────────────────────────────────────
/// <summary>
/// Base class for MenuStrip, ToolStripDropDown, and ContextMenuStrip.
/// Owns a ToolStripItemCollection and provides common layout/font settings.
/// Matches the WinForms ToolStrip hierarchy.
/// </summary>
public class ToolStrip : ScrollableControl
{
    private ToolStripItemCollection? _items;
    private Font _font = new Font("Segoe UI", 9);
    private ToolStripGripStyle  _gripStyle    = ToolStripGripStyle.Visible;
    private ToolStripLayoutStyle _layoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
    private ToolStripRenderMode  _renderMode  = ToolStripRenderMode.ManagerRenderMode;
    private ToolStripRenderer?   _renderer;
    private ToolStripTextDirection _textDirection = ToolStripTextDirection.Horizontal;
    private ToolStripDropDownDirection _defaultDropDownDirection = ToolStripDropDownDirection.BelowRight;
    private Padding _gripMargin = new Padding(2);
    private Size    _imageScalingSize = new Size(16, 16);
    private bool    _allowItemReorder  = false;
    private bool    _allowMerge        = true;
    private bool    _canOverflow       = true;
    private bool    _showItemToolTips  = true;
    private bool    _stretch           = false;

    public ToolStrip()
    {
        TabStop  = false;
        BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        ForeColor = System.Drawing.Color.Black;
        Height    = 24;
    }

    // ── Items ──────────────────────────────────────────────────────────────────

    public virtual ToolStripItemCollection Items
        => _items ??= new ToolStripItemCollection(this);

    // ── Font ───────────────────────────────────────────────────────────────────

    public new Font Font
    {
        get => _font;
        set { _font = value ?? new Font("Segoe UI", 9); Invalidate(); }
    }

    // ── Grip ───────────────────────────────────────────────────────────────────

    public ToolStripGripStyle GripStyle
    {
        get => _gripStyle;
        set { _gripStyle = value; Invalidate(); }
    }

    public ToolStripGripDisplayStyle GripDisplayStyle
        => (LayoutStyle == ToolStripLayoutStyle.HorizontalStackWithOverflow ||
            LayoutStyle == ToolStripLayoutStyle.StackWithOverflow)
            ? ToolStripGripDisplayStyle.Vertical
            : ToolStripGripDisplayStyle.Horizontal;

    public Padding GripMargin
    {
        get => _gripMargin;
        set { _gripMargin = value; Invalidate(); }
    }

    public Rectangle GripRectangle
    {
        get
        {
            if (GripStyle == ToolStripGripStyle.Hidden) return Rectangle.Empty;
            return GripDisplayStyle == ToolStripGripDisplayStyle.Vertical
                ? new Rectangle(GripMargin.Left, GripMargin.Top, 3, Height - GripMargin.Vertical)
                : new Rectangle(GripMargin.Left, GripMargin.Top, Width - GripMargin.Horizontal, 3);
        }
    }

    // ── Layout ─────────────────────────────────────────────────────────────────

    public ToolStripLayoutStyle LayoutStyle
    {
        get => _layoutStyle;
        set { _layoutStyle = value; Invalidate(); }
    }

    public ToolStripTextDirection TextDirection
    {
        get => _textDirection;
        set { _textDirection = value; Invalidate(); }
    }

    public ToolStripDropDownDirection DefaultDropDownDirection
    {
        get => _defaultDropDownDirection;
        set => _defaultDropDownDirection = value;
    }

    public Size ImageScalingSize
    {
        get => _imageScalingSize;
        set { _imageScalingSize = value; Invalidate(); }
    }

    public bool Stretch
    {
        get => _stretch;
        set { _stretch = value; Invalidate(); }
    }

    public bool CanOverflow
    {
        get => _canOverflow;
        set => _canOverflow = value;
    }

    public bool AllowItemReorder
    {
        get => _allowItemReorder;
        set => _allowItemReorder = value;
    }

    public bool AllowMerge
    {
        get => _allowMerge;
        set => _allowMerge = value;
    }

    public bool ShowItemToolTips
    {
        get => _showItemToolTips;
        set => _showItemToolTips = value;
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    public ToolStripRenderMode RenderMode
    {
        get => _renderMode;
        set
        {
            if (value == ToolStripRenderMode.Custom)
                throw new ArgumentException("Use the Renderer property to set a custom renderer.");
            _renderMode = value;
            _renderer   = null;
            RendererChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public ToolStripRenderer Renderer
    {
        get => _renderer ??= ToolStripManager.Renderer;
        set
        {
            _renderer   = value;
            _renderMode = value is null ? ToolStripRenderMode.ManagerRenderMode : ToolStripRenderMode.Custom;
            RendererChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // ── Overflow button (stub) ─────────────────────────────────────────────────

    /// <summary>Overflow button for when items don't fit (stub — always null).</summary>
    public ToolStripOverflowButton? OverflowButton => null;

    /// <summary>Returns true when the strip is currently being dragged (always false in canvas).</summary>
    public bool IsCurrentlyDragging => false;

    /// <summary>True if this strip is a dropdown (overridden in ToolStripDropDown).</summary>
    public virtual bool IsDropDown => false;

    // ── Hit-testing ────────────────────────────────────────────────────────────

    /// <summary>Returns the item at the given point in strip-local coordinates, or null.</summary>
    public ToolStripItem? GetItemAt(Point point) => GetItemAt(point.X, point.Y);

    public ToolStripItem? GetItemAt(int x, int y)
    {
        foreach (var item in Items)
        {
            if (item.Visible && item.Bounds.Contains(x, y))
                return item;
        }
        return null;
    }

    /// <summary>
    /// Returns the next ToolStripItem in the tab order relative to <paramref name="start"/>.
    /// </summary>
    public ToolStripItem? GetNextItem(ToolStripItem? start, ArrowDirection direction)
    {
        if (Items.Count == 0) return null;
        int startIdx = start is null ? -1 : Items.IndexOf(start);
        return direction switch
        {
            ArrowDirection.Right or ArrowDirection.Down =>
                FindNextVisible(startIdx + 1, 1),
            ArrowDirection.Left or ArrowDirection.Up =>
                FindNextVisible(startIdx - 1, -1),
            _ => null
        };

        ToolStripItem? FindNextVisible(int idx, int delta)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                int k = ((idx + i * delta) % Items.Count + Items.Count) % Items.Count;
                if (Items[k].Visible && Items[k].Enabled) return Items[k];
            }
            return null;
        }
    }

    // ── Events ─────────────────────────────────────────────────────────────────

    public event ToolStripItemClickedEventHandler? ItemClicked;
    public event EventHandler?                     ItemAdded;
    public event EventHandler?                     ItemRemoved;
    public event EventHandler?                     LayoutCompleted;
    public event EventHandler?                     LayoutStyleChanged;
    public event EventHandler?                     RendererChanged;
    public event PaintEventHandler?                PaintGrip;
    public event EventHandler?                     BeginDrag;    // stub
    public event EventHandler?                     EndDrag;      // stub

    protected virtual void OnItemClicked(ToolStripItemClickedEventArgs e)
        => ItemClicked?.Invoke(this, e);
    protected virtual void OnItemAdded(EventArgs e)      => ItemAdded?.Invoke(this, e);
    protected virtual void OnItemRemoved(EventArgs e)    => ItemRemoved?.Invoke(this, e);
    protected virtual void OnLayoutCompleted(EventArgs e) => LayoutCompleted?.Invoke(this, e);
    protected virtual void OnLayoutStyleChanged(EventArgs e) => LayoutStyleChanged?.Invoke(this, e);
    protected virtual void OnRendererChanged(EventArgs e) => RendererChanged?.Invoke(this, e);
    protected virtual void OnPaintGrip(PaintEventArgs e) => PaintGrip?.Invoke(this, e);
    protected virtual void OnBeginDrag(EventArgs e)      => BeginDrag?.Invoke(this, e);
    protected virtual void OnEndDrag(EventArgs e)        => EndDrag?.Invoke(this, e);

    /// <summary>
    /// Creates the default item type for this strip when adding via text/image/handler.
    /// Override in derived classes to change the default item type
    /// (e.g. <see cref="StatusStrip"/> returns <see cref="ToolStripStatusLabel"/>).
    /// </summary>
    protected internal virtual ToolStripItem CreateDefaultItem(string? text, Image? image, EventHandler? onClick)
    {
        if (text == "-")
            return new ToolStripSeparator();
        var btn = new ToolStripButton(text ?? string.Empty, image);
        if (onClick is not null) btn.Click += onClick;
        return btn;
    }

    // ── Hover tracking ──────────────────────────────────────────────────────

    private int _hoveredIndex = -1;

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        int idx = HitTestItem(e.X, e.Y);
        if (idx != _hoveredIndex) { _hoveredIndex = idx; Invalidate(); }
        base.OnMouseMove(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_hoveredIndex != -1) { _hoveredIndex = -1; Invalidate(); }
        base.OnMouseLeave(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            int idx = HitTestItem(e.X, e.Y);
            if (idx >= 0)
            {
                var item = Items[idx];
                if (item.Enabled && item is not ToolStripSeparator && item is not ToolStripLabel)
                {
                    if (item is ToolStripButton btn && btn.CheckOnClick)
                    {
                        btn.Checked = !btn.Checked;
                        Invalidate();
                    }
                    item.PerformClick();
                    OnItemClicked(new ToolStripItemClickedEventArgs(item));
                }
            }
        }
        base.OnMouseDown(e);
    }

    private int HitTestItem(int x, int y)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (item.Visible && !(item is ToolStripSeparator) && item.Bounds.Contains(x, y))
                return i;
        }
        return -1;
    }

    // ── Painting ──────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        var bg = BackColor;
        using var bgBrush = new SolidBrush(CanvasColor.FromArgb(bg.R, bg.G, bg.B));
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        // Bottom border
        using var borderPen = new Pen(CanvasColor.FromArgb(210, 210, 210));
        g.DrawLine(borderPen, 0, Height - 1, Width, Height - 1);

        // Grip dots (2 columns of dots at x=2, x=4)
        if (GripStyle == ToolStripGripStyle.Visible)
        {
            using var dotBrush = new SolidBrush(CanvasColor.FromArgb(160, 160, 160));
            for (int dy = 4; dy < Height - 4; dy += 4)
            {
                g.FillRectangle(dotBrush, new Rectangle(2, dy, 2, 2));
                g.FillRectangle(dotBrush, new Rectangle(5, dy + 2, 2, 2));
            }
        }

        // Paint each item left-to-right
        int x = GripStyle == ToolStripGripStyle.Visible ? 10 : 2;
        int idx2 = 0;
        foreach (var item in Items)
        {
            if (!item.Visible) { idx2++; continue; }

            if (item is ToolStripSeparator)
            {
                using var sepPen = new Pen(CanvasColor.FromArgb(180, 180, 180));
                int midX = x + 3;
                g.DrawLine(sepPen, midX, 3, midX, Height - 4);
                x += 8;
                idx2++;
                continue;
            }

            int itemW = item is ToolStripLabel ? Math.Max(EstimateItemWidth(item.Text, item), 20)
                      : Math.Max(EstimateItemWidth(item.Text, item), Height);
            var bounds = new Rectangle(x, 1, itemW, Height - 2);
            bool hovered = idx2 == _hoveredIndex;
            PaintItem(g, item, bounds, hovered);
            item.Bounds = bounds;
            x += itemW + 1;
            idx2++;
        }

        base.OnPaint(e);
    }

    private static int EstimateItemWidth(string? text, ToolStripItem? item = null)
    {
        // Hosted-control items: use the inner control's Width when available.
        if (item is ToolStripTextBox stb && stb.TextBox.Width > 0)
            return stb.TextBox.Width;
        if (item is ToolStripComboBox scb && scb.ComboBox.Width > 0)
            return scb.ComboBox.Width;
        if (item is ToolStripProgressBar spb && spb.ProgressBar.Width > 0)
            return spb.ProgressBar.Width;

        bool hasImage = item?.Image is not null &&
                        item.DisplayStyle != ToolStripItemDisplayStyle.Text;
        bool hasText  = !string.IsNullOrEmpty(text) &&
                        item?.DisplayStyle != ToolStripItemDisplayStyle.Image;

        if (hasImage && hasText)
            return 16 + 4 + (text!.Length * 7 + 14);   // icon + gap + text
        if (hasImage)
            return 28;                                   // square icon button
        if (hasText)
            return text!.Length * 7 + 14;
        return 24;
    }

    // ── Rendering helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Paints a single ToolStripItem at the given bounds within this strip's Graphics.
    /// Used by derived classes that draw their own item layout.
    /// </summary>
    protected void PaintItem(Graphics g, ToolStripItem item, Rectangle bounds, bool hovered)
    {
        if (!item.Visible) return;

        // ── ToolStripProgressBar ───────────────────────────────────────────────
        if (item is ToolStripProgressBar tpb)
        {
            int pbH = Math.Min(14, bounds.Height - 4);
            int pbY = bounds.Y + (bounds.Height - pbH) / 2;
            var pbR = new Rectangle(bounds.X + 2, pbY, bounds.Width - 4, pbH);
            using var trackBrush = new SolidBrush(CanvasColor.FromArgb(230, 230, 230));
            using var trackPen   = new Pen(CanvasColor.FromArgb(160, 160, 160));
            g.FillRectangle(trackBrush, pbR);
            g.DrawRectangle(trackPen, pbR.X, pbR.Y, pbR.Width - 1, pbR.Height - 1);
            int range = Math.Max(1, tpb.Maximum - tpb.Minimum);
            int fillW = (int)((double)(tpb.Value - tpb.Minimum) / range * (pbR.Width - 2));
            if (fillW > 0)
            {
                using var fillBrush = new SolidBrush(CanvasColor.FromArgb(6, 176, 37));
                g.FillRectangle(fillBrush, new Rectangle(pbR.X + 1, pbR.Y + 1, fillW, pbR.Height - 2));
            }
            item.Bounds = bounds;
            return;
        }

        // ── ToolStripTextBox ───────────────────────────────────────────────────
        if (item is ToolStripTextBox ttb)
        {
            int tbH = Math.Min(20, bounds.Height - 4);
            int tbY = bounds.Y + (bounds.Height - tbH) / 2;
            var tbR = new Rectangle(bounds.X + 2, tbY, bounds.Width - 4, tbH);
            using var whBrush = new SolidBrush(CanvasColor.White);
            using var brdPen  = new Pen(CanvasColor.FromArgb(120, 120, 120));
            g.FillRectangle(whBrush, tbR);
            g.DrawRectangle(brdPen, tbR.X, tbR.Y, tbR.Width - 1, tbR.Height - 1);
            g.DrawString(ttb.Text ?? string.Empty, tbR.X + 3, tbR.Y + 3, CanvasColor.Black);
            item.Bounds = bounds;
            return;
        }

        // ── ToolStripComboBox ──────────────────────────────────────────────────
        if (item is ToolStripComboBox tcb)
        {
            int cbH = Math.Min(20, bounds.Height - 4);
            int cbY = bounds.Y + (bounds.Height - cbH) / 2;
            var cbR = new Rectangle(bounds.X + 2, cbY, bounds.Width - 4, cbH);
            using var whBrush = new SolidBrush(CanvasColor.White);
            using var brdPen  = new Pen(CanvasColor.FromArgb(120, 120, 120));
            g.FillRectangle(whBrush, cbR);
            g.DrawRectangle(brdPen, cbR.X, cbR.Y, cbR.Width - 1, cbR.Height - 1);
            // Arrow button on right
            int arrW = 14;
            var arrR = new Rectangle(cbR.Right - arrW, cbR.Y, arrW, cbR.Height);
            using var arrBrush = new SolidBrush(CanvasColor.FromArgb(220, 220, 220));
            g.FillRectangle(arrBrush, arrR);
            g.DrawString(tcb.ComboBox.Text ?? string.Empty, cbR.X + 3, cbR.Y + 3, CanvasColor.Black);
            using var arrPen = new Pen(CanvasColor.FromArgb(80, 80, 80));
            int ax = arrR.X + 4, ay = arrR.Y + 5;
            g.DrawLine(arrPen, ax, ay, ax + 3, ay + 4);
            g.DrawLine(arrPen, ax + 3, ay + 4, ax + 6, ay);
            item.Bounds = bounds;
            return;
        }

        // ── ToolStripSplitButton ───────────────────────────────────────────────
        if (item is ToolStripSplitButton tsb)
        {
            const int arrowAreaW = 14;
            int faceW = bounds.Width - arrowAreaW;
            var faceR = new Rectangle(bounds.X, bounds.Y, faceW, bounds.Height);
            var arrR2 = new Rectangle(bounds.X + faceW, bounds.Y, arrowAreaW, bounds.Height);

            // Face background
            CanvasColor faceBg = hovered && item.Enabled
                ? CanvasColor.FromArgb(0, 120, 215)
                : CanvasColor.FromArgb(240, 240, 240);
            using var faceBrush = new SolidBrush(faceBg);
            g.FillRectangle(faceBrush, faceR);

            // Arrow area background
            CanvasColor arrBg = CanvasColor.FromArgb(220, 220, 220);
            using var arrBrush2 = new SolidBrush(arrBg);
            g.FillRectangle(arrBrush2, arrR2);

            // Divider line
            using var divPen = new Pen(CanvasColor.FromArgb(160, 160, 160));
            g.DrawLine(divPen, arrR2.X, arrR2.Y + 2, arrR2.X, arrR2.Bottom - 2);

            // Face content
            var textColor2 = !item.Enabled
                ? CanvasColor.FromArgb(160, 160, 160)
                : hovered ? CanvasColor.White : CanvasColor.FromArgb(30, 30, 30);

            bool showImg2 = tsb.Image is not null && tsb.DisplayStyle != ToolStripItemDisplayStyle.Text;
            bool showTxt2 = !string.IsNullOrEmpty(tsb.Text) && tsb.DisplayStyle != ToolStripItemDisplayStyle.Image;

            if (showImg2 && showTxt2)
            {
                int is2 = Math.Min(faceR.Height - 6, 16);
                int my2 = faceR.Y + faceR.Height / 2;
                g.DrawImage(tsb.Image!.Source ?? string.Empty, faceR.X + 4, my2 - is2 / 2, is2, is2);
                g.DrawString(tsb.Text, faceR.X + is2 + 8, my2 - 6, textColor2);
            }
            else if (showImg2)
            {
                int is2 = Math.Min(faceR.Height - 6, 16);
                g.DrawImage(tsb.Image!.Source ?? string.Empty,
                    faceR.X + (faceR.Width - is2) / 2, faceR.Y + (faceR.Height - is2) / 2, is2, is2);
            }
            else if (showTxt2)
            {
                g.DrawString(tsb.Text, faceR.X + 6, faceR.Y + (faceR.Height - 12) / 2, textColor2);
            }

            // Down-arrow in arrow area
            using var arrowPen2 = new Pen(CanvasColor.FromArgb(80, 80, 80));
            int ax2 = arrR2.X + 3, ay2 = arrR2.Y + (arrR2.Height - 4) / 2;
            g.DrawLine(arrowPen2, ax2, ay2, ax2 + 3, ay2 + 4);
            g.DrawLine(arrowPen2, ax2 + 3, ay2 + 4, ax2 + 6, ay2);

            item.Bounds = bounds;
            return;
        }

        bool isChecked = item is ToolStripButton btn2 && btn2.Checked;

        // Background
        CanvasColor bg;
        if (isChecked && !hovered)
            bg = CanvasColor.FromArgb(204, 228, 247);      // light-blue checked
        else if (hovered && item.Enabled)
            bg = CanvasColor.FromArgb(0, 120, 215);
        else
            bg = CanvasColor.FromArgb(240, 240, 240);

        using var bgBrush = new SolidBrush(bg);
        g.FillRectangle(bgBrush, bounds);

        // Checked border
        if (isChecked)
        {
            using var chkPen = new Pen(CanvasColor.FromArgb(0, 120, 215));
            g.DrawRectangle(chkPen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        }

        var textColor = !item.Enabled
            ? CanvasColor.FromArgb(160, 160, 160)
            : hovered
                ? CanvasColor.FromArgb(255, 255, 255)
                : CanvasColor.FromArgb(30, 30, 30);

        bool showImage = item.Image is not null &&
                         item.DisplayStyle != ToolStripItemDisplayStyle.Text;
        bool showText  = !string.IsNullOrEmpty(item.Text) &&
                         item.DisplayStyle != ToolStripItemDisplayStyle.Image;

        if (showImage && showText)
        {
            // Icon left, text right — both vertically centered on the same midline
            int iconSize = Math.Min(bounds.Height - 6, 16);
            int midY     = bounds.Y + bounds.Height / 2;
            int iconX    = bounds.X + 4;
            int iconY    = midY - iconSize / 2;
            g.DrawImage(item.Image!.Source ?? string.Empty, iconX, iconY, iconSize, iconSize);
            // textBaseline='top' in JS, so offset by half the font size (12px) to center
            int textX = bounds.X + iconSize + 8;
            int textY = midY - 6;
            g.DrawString(item.Text, textX, textY, textColor);
        }
        else if (showImage)
        {
            // Icon only — centered
            int iconSize = Math.Min(bounds.Height - 6, 16);
            int iconX    = bounds.X + (bounds.Width  - iconSize) / 2;
            int iconY    = bounds.Y + (bounds.Height - iconSize) / 2;
            g.DrawImage(item.Image!.Source ?? string.Empty, iconX, iconY, iconSize, iconSize);
        }
        else if (showText)
        {
            // Text only — vertically centered (textBaseline='top', font 12px)
            int textY = bounds.Y + (bounds.Height - 12) / 2;
            g.DrawString(item.Text, bounds.X + 6, textY, textColor);
        }
    }

    // ── AutoSize & Layout ──────────────────────────────────────────────────────

    private bool _autoSize = true;

    public new bool AutoSize
    {
        get => _autoSize;
        set { _autoSize = value; Invalidate(); }
    }

    /// <summary>
    /// Forces an immediate layout pass on the strip (stub — canvas lays out during paint).
    /// </summary>
    public new void PerformLayout() { /* layout happens in OnPaint; no-op here */ }

    // ── FindForm forwarding ────────────────────────────────────────────────────

    // ToolStripDropDown is not a direct child of the Form, so FindForm() must
    // walk up through SourceItem.Owner.FindForm() if Parent is null.
    // Override is in ToolStripDropDown.
}

// ── ToolStripOverflowButton (stub) ────────────────────────────────────────────
/// <summary>Stub overflow button — canvas layouts do not overflow.</summary>
public class ToolStripOverflowButton : ToolStripDropDownButton
{
    internal ToolStripOverflowButton() { }
}

// ── ToolStripDropDownButton (stub) ────────────────────────────────────────────
/// <summary>A ToolStripItem that opens a dropdown when clicked.</summary>
public class ToolStripDropDownButton : ToolStripItem
{
    private ToolStripDropDown? _dropDown;

    public ToolStripDropDown DropDown
        => _dropDown ??= new ToolStripDropDownMenu();

    public ToolStripItemCollection DropDownItems => DropDown.Items;

    public bool HasDropDownItems => _dropDown is { } dd && dd.Items.Count > 0;

    public ToolStripDropDownDirection DropDownDirection { get; set; }
        = ToolStripDropDownDirection.Default;

    public bool ShowDropDownArrow { get; set; } = true;

    public event EventHandler? DropDownOpening;
    public event EventHandler? DropDownOpened;
    public event EventHandler? DropDownClosed;

    public ToolStripDropDownButton() { }
    public ToolStripDropDownButton(string text) { Text = text; }
    public ToolStripDropDownButton(string text, Image? image) { Text = text; Image = image; }
    public ToolStripDropDownButton(string text, Image? image, EventHandler onClick) { Text = text; Image = image; Click += onClick; }
    public ToolStripDropDownButton(string text, Image? image, params ToolStripItem[] items)
    {
        Text  = text; Image = image;
        foreach (var i in items) DropDownItems.Add(i);
    }

    protected internal override void OnClick(EventArgs e)
    {
        if (HasDropDownItems)
        {
            DropDownOpening?.Invoke(this, EventArgs.Empty);
            DropDown.Closed += OnDropDownClosed;
            DropDown.IsVisible = true;
            DropDownOpened?.Invoke(this, EventArgs.Empty);
        }
        base.OnClick(e);
    }

    private void OnDropDownClosed(object? sender, EventArgs e)
    {
        DropDown.Closed -= OnDropDownClosed;
        DropDownClosed?.Invoke(this, EventArgs.Empty);
    }
}

// ── ToolStripSplitButton ──────────────────────────────────────────────────────
/// <summary>
/// A ToolStrip item that consists of a button face and a separate drop-down arrow.
/// Clicking the face fires <see cref="ButtonClick"/>; clicking the arrow opens
/// <see cref="DropDown"/> just like <see cref="ToolStripDropDownButton"/>.
/// </summary>
public class ToolStripSplitButton : ToolStripItem
{
    private ToolStripDropDown? _dropDown;
    private ToolStripItem? _defaultItem;
    private bool _dropDownPressed;

    // ── Drop-down ─────────────────────────────────────────────────────────────

    public ToolStripDropDown DropDown
        => _dropDown ??= new ToolStripDropDownMenu();

    public ToolStripItemCollection DropDownItems => DropDown.Items;

    public bool HasDropDownItems
        => _dropDown is { } dd && dd.Items.Count > 0;

    public ToolStripDropDownDirection DropDownDirection { get; set; }
        = ToolStripDropDownDirection.Default;

    public bool ShowDropDownArrow { get; set; } = true;

    // ── DefaultItem ───────────────────────────────────────────────────────────

    /// <summary>
    /// The item whose action is invoked when the button face is clicked.
    /// If <c>null</c>, <see cref="ButtonClick"/> is raised instead.
    /// </summary>
    public ToolStripItem? DefaultItem
    {
        get => _defaultItem;
        set => _defaultItem = value;
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the button face (not the arrow) is clicked.</summary>
    public event EventHandler? ButtonClick;

    /// <summary>Raised when the drop-down portion is clicked and opens.</summary>
    public event EventHandler? ButtonDoubleClick;

    public event EventHandler? DropDownOpening;
    public event EventHandler? DropDownOpened;
    public event EventHandler? DropDownClosed;
    public event EventHandler? DropDownItemClicked;

    // ── Constructors ──────────────────────────────────────────────────────────

    public ToolStripSplitButton() { }
    public ToolStripSplitButton(string text) { Text = text; }
    public ToolStripSplitButton(Image? image) { Image = image; }
    public ToolStripSplitButton(string text, Image? image) { Text = text; Image = image; }
    public ToolStripSplitButton(string text, Image? image, EventHandler onClick)
    {
        Text = text; Image = image; ButtonClick += onClick;
    }
    public ToolStripSplitButton(string text, Image? image, EventHandler onClick, string name)
    {
        Text = text; Image = image; ButtonClick += onClick; Name = name;
    }
    public ToolStripSplitButton(string text, Image? image, params ToolStripItem[] items)
    {
        Text = text; Image = image;
        foreach (var i in items) DropDownItems.Add(i);
    }

    // ── Click routing ─────────────────────────────────────────────────────────

    protected internal override void OnClick(EventArgs e)
    {
        if (_dropDownPressed)
        {
            // Arrow area clicked — open drop-down
            _dropDownPressed = false;
            if (HasDropDownItems)
            {
                DropDownOpening?.Invoke(this, EventArgs.Empty);
                DropDown.Closed += OnDropDownClosed;
                DropDown.IsVisible = true;
                DropDownOpened?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            // Button face clicked
            if (_defaultItem is not null)
                _defaultItem.PerformClick();
            else
                ButtonClick?.Invoke(this, e);
            base.OnClick(e);
        }
    }

    /// <summary>
    /// Allows the renderer to signal that the drop-down arrow portion was pressed
    /// before routing the click event.
    /// </summary>
    public void SignalDropDownPress() => _dropDownPressed = true;

    /// <summary>Programmatically opens the drop-down.</summary>
    public void ShowDropDown()
    {
        if (HasDropDownItems)
        {
            DropDownOpening?.Invoke(this, EventArgs.Empty);
            DropDown.Closed += OnDropDownClosed;
            DropDown.IsVisible = true;
            DropDownOpened?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Programmatically closes the drop-down.</summary>
    public void HideDropDown()
    {
        if (_dropDown is not null)
            _dropDown.Close(ToolStripDropDownCloseReason.Keyboard);
    }

    private void OnDropDownClosed(object? sender, EventArgs e)
    {
        DropDown.Closed -= OnDropDownClosed;
        DropDownClosed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Performs a button-face click.</summary>
    public void PerformButtonClick()
    {
        if (_defaultItem is not null)
            _defaultItem.PerformClick();
        else
            ButtonClick?.Invoke(this, EventArgs.Empty);
    }
}

// ── ToolStripButton (stub) ────────────────────────────────────────────────────
/// <summary>A clickable button item on a ToolStrip.</summary>
public class ToolStripButton : ToolStripItem
{
    private bool _checked;
    private CheckState _checkState = CheckState.Unchecked;

    public bool Checked
    {
        get => _checked;
        set { _checked = value; _checkState = value ? CheckState.Checked : CheckState.Unchecked; Owner?.Invalidate(); }
    }

    public CheckState CheckState
    {
        get => _checkState;
        set { _checkState = value; _checked = value == CheckState.Checked; Owner?.Invalidate(); }
    }

    public bool CheckOnClick { get; set; }

    public FlatStyle FlatStyle { get; set; } = FlatStyle.Standard;

    public ToolStripButton() { }
    public ToolStripButton(string text) { Text = text; }
    public ToolStripButton(Image? image) { Image = image; }
    public ToolStripButton(string text, Image? image) { Text = text; Image = image; }
    public ToolStripButton(string text, Image? image, EventHandler onClick) { Text = text; Image = image; Click += onClick; }
    public ToolStripButton(string text, Image? image, EventHandler onClick, string name) { Text = text; Image = image; Click += onClick; Name = name; }
}

// ── ToolStripLabel ────────────────────────────────────────────────────────────
/// <summary>A non-interactive label item on a ToolStrip.</summary>
public class ToolStripLabel : ToolStripItem
{
    public bool IsLink { get; set; }
    public bool LinkVisited { get; set; }
    public System.Drawing.Color LinkColor        { get; set; } = System.Drawing.Color.FromArgb(0,   0, 255);
    public System.Drawing.Color ActiveLinkColor  { get; set; } = System.Drawing.Color.Red;
    public System.Drawing.Color VisitedLinkColor { get; set; } = System.Drawing.Color.FromArgb(128, 0, 128);

    public ToolStripLabel() { }
    public ToolStripLabel(string text) { Text = text; }
    public ToolStripLabel(Image? image) { Image = image; }
    public ToolStripLabel(string text, Image? image) { Text = text; Image = image; }
    public ToolStripLabel(string text, Image? image, bool isLink, EventHandler? onClick = null)
    {
        Text = text; Image = image; IsLink = isLink;
        if (onClick is not null) Click += onClick;
    }
}

// ── ToolStripComboBox (stub) ──────────────────────────────────────────────────
/// <summary>A ComboBox hosted on a ToolStrip.</summary>
public class ToolStripComboBox : ToolStripItem
{
    public ComboBox ComboBox { get; } = new ComboBox { Width = 100 };
    public System.Windows.Forms.ComboBoxStyle DropDownStyle
    {
        get => ComboBox.DropDownStyle;
        set => ComboBox.DropDownStyle = value;
    }
    public int DropDownWidth  { get => ComboBox.DropDownWidth;  set => ComboBox.DropDownWidth  = value; }
    public int DropDownHeight { get => ComboBox.DropDownHeight; set => ComboBox.DropDownHeight = value; }
    public int MaxDropDownItems { get => ComboBox.MaxDropDownItems; set => ComboBox.MaxDropDownItems = value; }
    public bool Sorted { get => ComboBox.Sorted; set => ComboBox.Sorted = value; }
    public object? SelectedItem { get => ComboBox.SelectedItem; set => ComboBox.SelectedItem = value; }
    public int SelectedIndex { get => ComboBox.SelectedIndex; set => ComboBox.SelectedIndex = value; }
    public string SelectedText { get => ComboBox.Text; set => ComboBox.Text = value; }
    public ListControl.ObjectCollection Items => ComboBox.Items;

    public ToolStripComboBox() { }
    public ToolStripComboBox(string name) { Name = name; }
}

// ── ToolStripTextBox (stub) ───────────────────────────────────────────────────
/// <summary>A TextBox hosted on a ToolStrip.</summary>
public class ToolStripTextBox : ToolStripItem
{
    public TextBox TextBox { get; } = new TextBox { Width = 100 };
    public override string Text { get => TextBox.Text; set => TextBox.Text = value ?? string.Empty; }
    public bool AcceptsReturn { get => TextBox.AcceptsReturn; set => TextBox.AcceptsReturn = value; }
    public bool AcceptsTab    { get => TextBox.AcceptsTab;    set => TextBox.AcceptsTab    = value; }
    public bool Multiline     { get => TextBox.Multiline;     set => TextBox.Multiline     = value; }
    public bool ReadOnly      { get => TextBox.ReadOnly;      set => TextBox.ReadOnly      = value; }
    public int MaxLength      { get => TextBox.MaxLength;     set => TextBox.MaxLength     = value; }
    public new HorizontalAlignment TextAlign { get => TextBox.TextAlign; set => TextBox.TextAlign = value; }
    public ScrollBars ScrollBars { get => TextBox.ScrollBars; set => TextBox.ScrollBars   = value; }

    public ToolStripTextBox() { }
    public ToolStripTextBox(string name) { Name = name; }
}

// ── ToolStripProgressBar (stub) ───────────────────────────────────────────────
/// <summary>A ProgressBar hosted on a ToolStrip.</summary>
public class ToolStripProgressBar : ToolStripItem
{
    public ProgressBar ProgressBar { get; } = new ProgressBar { Width = 75 };
    public int Value   { get => ProgressBar.Value;   set => ProgressBar.Value   = value; }
    public int Minimum { get => ProgressBar.Minimum; set => ProgressBar.Minimum = value; }
    public int Maximum { get => ProgressBar.Maximum; set => ProgressBar.Maximum = value; }
    public int Step    { get => ProgressBar.Step;    set => ProgressBar.Step    = value; }
    public ProgressBarStyle Style { get => ProgressBar.Style; set => ProgressBar.Style = value; }
    public void PerformStep() => ProgressBar.PerformStep();
    public void Increment(int value) => ProgressBar.Increment(value);
}

// ── ToolStripControlHost ──────────────────────────────────────────────────────
/// <summary>
/// Hosts any <see cref="Control"/> as a <see cref="ToolStripItem"/>.
/// The hosted control participates in normal ToolStrip layout using its Width/Height.
/// </summary>
public class ToolStripControlHost : ToolStripItem
{
    public Control Control { get; }

    public ToolStripControlHost(Control c)
    {
        Control = c;
        Width   = c.Width;
        Height  = c.Height;
    }

    public ToolStripControlHost(Control c, string name) : this(c)
    {
        Name = name;
    }

    public override string Text
    {
        get => Control.Text;
        set => Control.Text = value;
    }

    public bool Focused => Control.Focused;

    public void Focus() => Control.Focus();
}
