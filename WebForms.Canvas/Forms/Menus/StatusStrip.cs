namespace System.Windows.Forms;

// ── ToolStripStatusLabelBorderSides ───────────────────────────────────────────
/// <summary>Specifies which sides of a ToolStripStatusLabel have a border.</summary>
[Flags]
public enum ToolStripStatusLabelBorderSides
{
    None   = 0,
    Left   = 1,
    Top    = 2,
    Right  = 4,
    Bottom = 8,
    All    = Left | Top | Right | Bottom
}

// ── ToolStripStatusLabel ──────────────────────────────────────────────────────
/// <summary>
/// A label panel inside a <see cref="StatusStrip"/>.
/// Matches the WinForms <c>ToolStripStatusLabel</c> hierarchy:
///   ToolStripItem → ToolStripLabel → ToolStripStatusLabel.
/// </summary>
public class ToolStripStatusLabel : ToolStripLabel
{
    private bool                           _spring;
    private ToolStripStatusLabelBorderSides _borderSides = ToolStripStatusLabelBorderSides.None;
    private Border3DStyle                  _borderStyle  = Border3DStyle.Flat;
    private LiveSetting                    _liveRegionMode = LiveSetting.Off;

    // ── Constructors ───────────────────────────────────────────────────────────

    public ToolStripStatusLabel() { }

    public ToolStripStatusLabel(string text)
        => Text = text;

    public ToolStripStatusLabel(string text, Image? image)
    {
        Text  = text;
        Image = image;
    }

    public ToolStripStatusLabel(string text, Image? image, EventHandler? onClick)
    {
        Text  = text;
        Image = image;
        if (onClick is not null) Click += onClick;
    }

    public ToolStripStatusLabel(string text, Image? image, EventHandler? onClick, string name)
    {
        Text  = text;
        Image = image;
        Name  = name;
        if (onClick is not null) Click += onClick;
    }

    // ── Spring ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// When true the label fills the remaining space in the StatusStrip
    /// (equivalent to WinForms Spring property).
    /// </summary>
    public bool Spring
    {
        get => _spring;
        set { _spring = value; Owner?.Invalidate(); }
    }

    // ── Border ─────────────────────────────────────────────────────────────────

    /// <summary>Gets or sets which sides of the label show a border.</summary>
    public ToolStripStatusLabelBorderSides BorderSides
    {
        get => _borderSides;
        set { _borderSides = value; Owner?.Invalidate(); }
    }

    /// <summary>Gets or sets the border style of the label.</summary>
    public Border3DStyle BorderStyle
    {
        get => _borderStyle;
        set { _borderStyle = value; Owner?.Invalidate(); }
    }

    // ── Accessibility live region ──────────────────────────────────────────────

    /// <summary>Gets or sets the live-region mode for screen readers (stub).</summary>
    public LiveSetting LiveSetting
    {
        get => _liveRegionMode;
        set => _liveRegionMode = value;
    }
}

// ── LiveSetting (accessibility) ───────────────────────────────────────────────
/// <summary>Specifies the live-region politeness level for screen reader updates.</summary>
public enum LiveSetting
{
    Off      = 0,
    Polite   = 1,
    Assertive = 2
}

// ── StatusStrip ───────────────────────────────────────────────────────────────
/// <summary>
/// A status bar docked at the bottom of a <see cref="Form"/>.
/// Renders its <see cref="ToolStripStatusLabel"/> items left-to-right,
/// with Spring items consuming all remaining space.
/// Matches the WinForms hierarchy: ToolStrip → StatusStrip.
/// </summary>
public class StatusStrip : ToolStrip
{
    // ── Layout / appearance constants ──────────────────────────────────────────
    private const int DefaultHeight   = 22;
    private const int ItemPadH        = 6;   // horizontal padding inside each label
    private const int ItemPadV        = 3;   // vertical padding
    private const int GripSize        = 12;  // sizing-grip area at bottom-right

    private static readonly CanvasColor BgColor      = CanvasColor.FromArgb(240, 240, 240);
    private static readonly CanvasColor BorderTop    = CanvasColor.FromArgb(180, 180, 180);
    private static readonly CanvasColor TextColor    = CanvasColor.FromArgb(0,   0,   0);
    private static readonly CanvasColor SepColor     = CanvasColor.FromArgb(180, 180, 180);
    private static readonly CanvasColor BorderPanel  = CanvasColor.FromArgb(160, 160, 160);
    private static readonly CanvasColor BorderRaised = CanvasColor.FromArgb(255, 255, 255);
    private static readonly CanvasColor GripDot      = CanvasColor.FromArgb(130, 130, 130);

    // ── StatusStrip-specific properties ───────────────────────────────────────

    private bool _sizingGrip = true;

    /// <summary>
    /// Gets or sets whether the sizing grip is displayed in the bottom-right corner.
    /// Matches WinForms <c>StatusStrip.SizingGrip</c>.
    /// </summary>
    public bool SizingGrip
    {
        get => _sizingGrip;
        set { _sizingGrip = value; Invalidate(); }
    }

    // ── ToolStrip overrides ────────────────────────────────────────────────────

    /// <summary>StatusStrip defaults: docked bottom, no grip, stretch enabled.</summary>
    public StatusStrip()
    {
        Dock       = DockStyle.Bottom;
        Height     = DefaultHeight;
        GripStyle  = ToolStripGripStyle.Hidden;
        Stretch    = true;
        LayoutStyle = ToolStripLayoutStyle.Table;
        BackColor  = System.Drawing.Color.FromArgb(240, 240, 240);
    }

    /// <summary>
    /// Creates a default <see cref="ToolStripStatusLabel"/> (or <see cref="ToolStripSeparator"/>
    /// if <paramref name="text"/> is "-"). Matches WinForms <c>StatusStrip.CreateDefaultItem</c>.
    /// </summary>
    protected internal override ToolStripItem CreateDefaultItem(string? text, Image? image, EventHandler? onClick)
    {
        if (text == "-")
            return new ToolStripSeparator();
        return new ToolStripStatusLabel(text ?? string.Empty, image, onClick);
    }

    // ── Painting ──────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        using var bgBrush = new SolidBrush(BgColor);
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        // Top border line (separates status bar from content)
        using var topPen = new Pen(BorderTop);
        g.DrawLine(topPen, 0, 0, Width, 0);

        // Lay out items, resolving Spring widths
        var layout = ComputeLayout();

        int idx = 0;
        foreach (var item in Items)
        {
            if (!item.Visible) { idx++; continue; }

            if (item is ToolStripSeparator)
            {
                // Vertical separator
                using var sepPen = new Pen(SepColor);
                int sx = layout[idx].x + layout[idx].w / 2;
                g.DrawLine(sepPen, sx, 3, sx, Height - 4);
                idx++;
                continue;
            }

            var (ix, iw) = layout[idx];
            var bounds = new Rectangle(ix, 1, iw, Height - 2);

            PaintStatusItem(g, item, bounds);
            item.Bounds = bounds;
            idx++;
        }

        // Sizing grip (3×3 dot matrix in bottom-right corner)
        if (_sizingGrip)
            PaintSizingGrip(g);
    }

    // ── Layout helpers ─────────────────────────────────────────────────────────

    private List<(int x, int w)> ComputeLayout()
    {
        // Separate Left-aligned and Right-aligned items
        var leftItems  = new List<(int idx, ToolStripItem item)>();
        var rightItems = new List<(int idx, ToolStripItem item)>();

        int idx = 0;
        foreach (var item in Items)
        {
            if (!item.Visible) { idx++; continue; }
            if (item.Alignment == ToolStripItemAlignment.Right)
                rightItems.Add((idx, item));
            else
                leftItems.Add((idx, item));
            idx++;
        }

        var result = new (int x, int w)[Items.Count];

        // Count spring items among left-aligned non-separators
        int fixedLeft  = 0;
        int springCount = 0;
        foreach (var (_, item) in leftItems)
        {
            if (item is ToolStripSeparator)
                fixedLeft += 8;
            else if (item is ToolStripStatusLabel { Spring: true })
                springCount++;
            else
                fixedLeft += ItemWidth(item);
        }

        // Fixed width for right-aligned items
        int fixedRight = 0;
        foreach (var (_, item) in rightItems)
            fixedRight += item is ToolStripSeparator ? 8 : ItemWidth(item);

        int gripReserve = _sizingGrip ? GripSize : 0;
        int available   = Math.Max(0, Width - fixedLeft - fixedRight - gripReserve);
        int springW     = springCount > 0 ? available / springCount : 0;

        // Assign positions left-to-right
        int x = 0;
        foreach (var (i, item) in leftItems)
        {
            int w;
            if (item is ToolStripSeparator)
                w = 8;
            else if (item is ToolStripStatusLabel { Spring: true })
                w = springW;
            else
                w = ItemWidth(item);

            result[i] = (x, w);
            x += w;
        }

        // Assign right-aligned items from the right edge (right-to-left)
        int rx = Width - gripReserve;
        for (int i = rightItems.Count - 1; i >= 0; i--)
        {
            var (ri, item) = rightItems[i];
            int w = item is ToolStripSeparator ? 8 : ItemWidth(item);
            rx -= w;
            result[ri] = (rx, w);
        }

        return result.ToList();
    }

    private int ItemWidth(ToolStripItem item)
    {
        bool hasImage = item.Image is not null &&
                        item.DisplayStyle != ToolStripItemDisplayStyle.Text;
        bool hasText  = !string.IsNullOrEmpty(item.Text) &&
                        item.DisplayStyle != ToolStripItemDisplayStyle.Image;

        int w = ItemPadH * 2;
        if (hasImage) w += 16 + (hasText ? 4 : 0);
        if (hasText)  w += EstimateTextWidth(item.Text);
        return Math.Max(w, 20);
    }

    // ── Per-item paint ────────────────────────────────────────────────────────

    private void PaintStatusItem(Graphics g, ToolStripItem item, Rectangle bounds)
    {
        var lbl = item as ToolStripStatusLabel;

        var textColor = item.Enabled ? TextColor : CanvasColor.FromArgb(130, 130, 130);

        // Border (ToolStripStatusLabel only)
        if (lbl is not null && lbl.BorderSides != ToolStripStatusLabelBorderSides.None)
            PaintLabelBorder(g, bounds, lbl.BorderSides, lbl.BorderStyle);

        bool showImage = item.Image is not null &&
                         item.DisplayStyle != ToolStripItemDisplayStyle.Text;
        bool showText  = !string.IsNullOrEmpty(item.Text) &&
                         item.DisplayStyle != ToolStripItemDisplayStyle.Image;

        int midY  = bounds.Y + bounds.Height / 2;
        int contentX = bounds.X + ItemPadH;

        if (showImage)
        {
            int iconSize = Math.Min(bounds.Height - 4, 16);
            int iconY    = midY - iconSize / 2;
            g.DrawImage(item.Image!.Source, contentX, iconY, iconSize, iconSize);
            contentX += iconSize + 4;
        }

        if (showText)
        {
            int textY = midY - 6;   // textBaseline='top', font 12px → center at midY
            g.DrawString(item.Text, contentX, textY, textColor);
        }
    }

    private static void PaintLabelBorder(
        Graphics g, Rectangle bounds,
        ToolStripStatusLabelBorderSides sides,
        Border3DStyle style)
    {
        // Choose light and dark colours based on style
        CanvasColor dark  = BorderPanel;
        CanvasColor light = BorderRaised;
        bool raised   = style == Border3DStyle.Raised || style == Border3DStyle.RaisedInner || style == Border3DStyle.RaisedOuter;
        bool sunken   = style == Border3DStyle.Sunken || style == Border3DStyle.SunkenInner || style == Border3DStyle.SunkenOuter;
        CanvasColor topLeft     = raised ? light : (sunken ? dark  : BorderPanel);
        CanvasColor bottomRight = raised ? dark  : (sunken ? light : BorderPanel);

        using var tlPen = new Pen(topLeft);
        using var brPen = new Pen(bottomRight);

        int x1 = bounds.X, y1 = bounds.Y;
        int x2 = bounds.Right - 1, y2 = bounds.Bottom - 1;

        if ((sides & ToolStripStatusLabelBorderSides.Top)    != 0)
            g.DrawLine(tlPen, x1, y1, x2, y1);
        if ((sides & ToolStripStatusLabelBorderSides.Left)   != 0)
            g.DrawLine(tlPen, x1, y1, x1, y2);
        if ((sides & ToolStripStatusLabelBorderSides.Bottom) != 0)
            g.DrawLine(brPen, x1, y2, x2, y2);
        if ((sides & ToolStripStatusLabelBorderSides.Right)  != 0)
            g.DrawLine(brPen, x2, y1, x2, y2);
    }

    private void PaintSizingGrip(Graphics g)
    {
        // 3×3 dot matrix in bottom-right corner (classic WinForms grip look)
        using var dotBrush = new SolidBrush(GripDot);
        using var litBrush = new SolidBrush(BorderRaised);
        int baseX = Width  - GripSize + 2;
        int baseY = Height - GripSize + 2;
        for (int row = 0; row < 3; row++)
        {
            for (int col = 2 - row; col < 3; col++)
            {
                int dx = baseX + col * 4;
                int dy = baseY + row * 4;
                // Highlight pixel (top-left of dot)
                g.FillRectangle(litBrush, new Rectangle(dx,     dy,     1, 1));
                // Shadow pixel (bottom-right of dot)
                g.FillRectangle(dotBrush, new Rectangle(dx + 1, dy + 1, 2, 2));
            }
        }
    }

    // ── Utility ───────────────────────────────────────────────────────────────

    private static int EstimateTextWidth(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return text.Length * 7;
    }
}
