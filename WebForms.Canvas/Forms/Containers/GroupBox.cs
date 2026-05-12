
namespace System.Windows.Forms;

public class GroupBox : Control
{
    private FlatStyle _flatStyle = FlatStyle.Standard;
    private bool _autoSize = false;
    private AutoSizeMode _autoSizeMode = AutoSizeMode.GrowOnly;

    public GroupBox()
    {
        TabStop = false;
        BackColor = System.Drawing.Color.Transparent;
        IsMouseRoutingContainer = true;
    }

    /// <summary>
    /// Gets or sets the flat style of the GroupBox border.
    /// Matches WinForms: Standard draws the etched recessed border; Flat draws a single flat line;
    /// System defers to the OS theme (rendered the same as Standard here).
    /// </summary>
    public FlatStyle FlatStyle
    {
        get => _flatStyle;
        set
        {
            if (_flatStyle != value)
            {
                _flatStyle = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the GroupBox resizes to wrap its children.
    /// Matches WinForms <c>GroupBox.AutoSize</c>.
    /// </summary>
    public new bool AutoSize
    {
        get => _autoSize;
        set
        {
            if (_autoSize != value)
            {
                _autoSize = value;
                if (_autoSize) PerformAutoSize();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether AutoSize only grows, or also shrinks.
    /// Matches WinForms <c>GroupBox.AutoSizeMode</c>.
    /// </summary>
    public AutoSizeMode AutoSizeMode
    {
        get => _autoSizeMode;
        set { _autoSizeMode = value; if (_autoSize) PerformAutoSize(); }
    }

    public override void PerformLayout()
    {
        base.PerformLayout();
        if (_autoSize) PerformAutoSize();
    }

    private void PerformAutoSize()
    {
        const int padding = 8;
        var captionHeight = Math.Max(0, Font.Height);

        var maxRight  = 0;
        var maxBottom = 0;

        foreach (var child in Controls)
        {
            if (!child.Visible) continue;
            maxRight  = Math.Max(maxRight,  child.Left + child.Width);
            maxBottom = Math.Max(maxBottom, child.Top  + child.Height);
        }

        var preferredW = maxRight  + padding * 2;
        var preferredH = maxBottom + captionHeight + padding;

        // GrowOnly: never shrink below current explicit size.
        var newW = _autoSizeMode == AutoSizeMode.GrowOnly ? Math.Max(Width,  preferredW) : preferredW;
        var newH = _autoSizeMode == AutoSizeMode.GrowOnly ? Math.Max(Height, preferredH) : preferredH;

        if (newW != Width || newH != Height)
        {
            Width  = newW;
            Height = newH;
            Invalidate();
        }
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);

        // GroupBox is typically transparent; only paint background if explicitly set.
        if (BackColor != System.Drawing.Color.Transparent)
        {
            using var bgBrush = new SolidBrush(BackColor);
            g.FillRectangle(bgBrush, bounds);
        }

        DrawGroupBoxBorderAndText(g);

        // Let user code handle Paint event.
        base.OnPaint(e);

        // Child controls are painted by Form.PaintControlsRecursive — do not paint them here.
        // Overlays (drop-downs, autocomplete) are handled by Form.PaintOverlaysRecursive.
    }

    private void DrawGroupBoxBorderAndText(Graphics g)
    {
        var text = Text ?? string.Empty;
        var textHeight = Font.Height;
        var borderTop = Math.Max(0, textHeight / 2);

        CanvasColor borderColor = FlatStyle == FlatStyle.Flat
            ? (CanvasColor)ForeColor
            : CanvasColor.FromArgb(122, 122, 122);

        var rect = new Rectangle(0, borderTop, Width - 1, Height - borderTop - 1);

        // Text measurements
        var measureService = FindForm()?.TextMeasurementService;
        var textWidth = 0;
        if (!string.IsNullOrEmpty(text))
        {
            if (measureService != null)
            {
                textWidth = measureService.MeasureTextEstimate(text, Font.Family, (int)Font.Size);
            }
            else
            {
                textWidth = (int)Math.Ceiling(text.Length * Font.Size * 0.55f);
            }
        }

        const int leftPadding = 8;
        const int textPad = 3;
        var gapLeft = leftPadding;
        var gapRight = leftPadding + (textWidth > 0 ? textWidth + (textPad * 2) : 0);

        if (FlatStyle == FlatStyle.Flat)
        {
            // Flat: rounded border; caption gap is handled by the bg-erase + text draw below
            using var pen = new Pen(borderColor);
            g.DrawRoundRect(pen, rect, 4);

            // Erase the top segment over the caption gap
            if (gapRight > gapLeft)
            {
                var bg = BackColor != System.Drawing.Color.Transparent ? BackColor : (Parent?.BackColor ?? System.Drawing.Color.White);
                using var erasePen = new Pen(bg);
                g.DrawLine(erasePen, rect.X + gapLeft, rect.Y, rect.X + gapRight, rect.Y);
            }
        }
        else
        {
            // Standard / System: etched (two-line) border
            using var darkPen = new Pen(CanvasColor.FromArgb(122, 122, 122));
            using var lightPen = new Pen(CanvasColor.FromArgb(255, 255, 255));

            // Draw sides and bottom twice (dark offset + light offset) for etched look
            // Left
            g.DrawLine(darkPen, rect.X, rect.Y, rect.X, rect.Bottom);
            g.DrawLine(lightPen, rect.X + 1, rect.Y + 1, rect.X + 1, rect.Bottom - 1);
            // Right
            g.DrawLine(darkPen, rect.Right, rect.Y, rect.Right, rect.Bottom);
            g.DrawLine(lightPen, rect.Right + 1, rect.Y + 1, rect.Right + 1, rect.Bottom - 1);
            // Bottom
            g.DrawLine(darkPen, rect.X, rect.Bottom, rect.Right, rect.Bottom);
            g.DrawLine(lightPen, rect.X + 1, rect.Bottom + 1, rect.Right + 1, rect.Bottom + 1);

            // Top with caption gap
            if (gapRight <= gapLeft)
            {
                g.DrawLine(darkPen, rect.X, rect.Y, rect.Right, rect.Y);
                g.DrawLine(lightPen, rect.X + 1, rect.Y + 1, rect.Right + 1, rect.Y + 1);
            }
            else
            {
                g.DrawLine(darkPen, rect.X, rect.Y, rect.X + gapLeft - 1, rect.Y);
                g.DrawLine(lightPen, rect.X + 1, rect.Y + 1, rect.X + gapLeft, rect.Y + 1);
                g.DrawLine(darkPen, rect.X + gapRight + 1, rect.Y, rect.Right, rect.Y);
                g.DrawLine(lightPen, rect.X + gapRight + 2, rect.Y + 1, rect.Right + 1, rect.Y + 1);
            }
        }

        if (!string.IsNullOrEmpty(text))
        {
            // Clear background behind caption
            var bg = BackColor != System.Drawing.Color.Transparent ? BackColor : (Parent?.BackColor ?? System.Drawing.Color.White);
            using var bgBrush = new SolidBrush(bg);
            g.FillRectangle(bgBrush, gapLeft, 0, Math.Max(0, textWidth + (textPad * 2)), textHeight);

            CanvasColor textColor = Enabled ? (CanvasColor)ForeColor : CanvasColor.FromArgb(122, 122, 122);
            g.DrawString(text, Font, textColor, gapLeft + textPad, 0);
        }
    }
}

