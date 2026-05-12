
namespace System.Windows.Forms;

public class Panel : ScrollableControl
{
    private BorderStyle _borderStyle = BorderStyle.None;
    private bool _autoSize = false;
    private AutoSizeMode _autoSizeMode = AutoSizeMode.GrowOnly;

    public Panel()
    {
        TabStop = false;
        IsMouseRoutingContainer = true;
    }

    public BorderStyle BorderStyle
    {
        get => _borderStyle;
        set
        {
            if (_borderStyle != value)
            {
                _borderStyle = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the Panel resizes itself to wrap its children.
    /// Matches WinForms <c>Panel.AutoSize</c>.
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
    /// Gets or sets whether AutoSize only grows, or can also shrink.
    /// Matches WinForms <c>Panel.AutoSizeMode</c>.
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
        var border  = GetBorderWidth();
        var padding = Padding.Left + Padding.Right;

        var maxRight  = 0;
        var maxBottom = 0;

        foreach (var child in Controls)
        {
            if (!child.Visible) continue;
            maxRight  = Math.Max(maxRight,  child.Left + child.Width  + Padding.Right  + border);
            maxBottom = Math.Max(maxBottom, child.Top  + child.Height + Padding.Bottom + border);
        }

        var preferredW = Math.Max(1, maxRight  + border + Padding.Left);
        var preferredH = Math.Max(1, maxBottom + border + Padding.Top);

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

        using (var bgBrush = new SolidBrush(BackColor))
        {
            g.FillRectangle(bgBrush, bounds);
        }

        DrawBorder(g, bounds);

        var borderWidth = GetBorderWidth();
        var clientRect = new Rectangle(
            borderWidth,
            borderWidth,
            Math.Max(0, Width - (borderWidth * 2)),
            Math.Max(0, Height - (borderWidth * 2))
        );

        g.Save();
        g.SetClip(clientRect);

        // Let user code handle Paint event (events, etc.)
        base.OnPaint(e);

        // Child controls are painted by Form.PaintControlsRecursive — do not paint them here.

        g.Restore();
    }

    private int GetBorderWidth()
    {
        return _borderStyle switch
        {
            BorderStyle.Fixed3D => 2,
            BorderStyle.FixedSingle => 1,
            _ => 0
        };
    }

    private void DrawBorder(Graphics g, Rectangle bounds)
    {
        switch (_borderStyle)
        {
            case BorderStyle.FixedSingle:
                using (var pen = new Pen(CanvasColor.FromArgb(122, 122, 122)))
                {
                    g.DrawRectangle(pen, bounds);
                }
                break;

            case BorderStyle.Fixed3D:
                // Inset 3D: dark outer top/left, light outer bottom/right;
                // then lighter inner top/left, white inner bottom/right.
                using (var darkOuter = new Pen(CanvasColor.FromArgb(100, 100, 100)))
                using (var lightOuter = new Pen(CanvasColor.FromArgb(255, 255, 255)))
                using (var darkInner = new Pen(CanvasColor.FromArgb(160, 160, 160)))
                using (var lightInner = new Pen(CanvasColor.FromArgb(227, 227, 227)))
                {
                    // Outer top + left
                    g.DrawLine(darkOuter, bounds.X, bounds.Y, bounds.Right - 1, bounds.Y);
                    g.DrawLine(darkOuter, bounds.X, bounds.Y, bounds.X, bounds.Bottom - 1);
                    // Outer bottom + right
                    g.DrawLine(lightOuter, bounds.X, bounds.Bottom - 1, bounds.Right - 1, bounds.Bottom - 1);
                    g.DrawLine(lightOuter, bounds.Right - 1, bounds.Y, bounds.Right - 1, bounds.Bottom - 1);
                    // Inner top + left
                    g.DrawLine(darkInner, bounds.X + 1, bounds.Y + 1, bounds.Right - 2, bounds.Y + 1);
                    g.DrawLine(darkInner, bounds.X + 1, bounds.Y + 1, bounds.X + 1, bounds.Bottom - 2);
                    // Inner bottom + right
                    g.DrawLine(lightInner, bounds.X + 1, bounds.Bottom - 2, bounds.Right - 2, bounds.Bottom - 2);
                    g.DrawLine(lightInner, bounds.Right - 2, bounds.Y + 1, bounds.Right - 2, bounds.Bottom - 2);
                }
                break;
        }
    }
}
