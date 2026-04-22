
namespace System.Windows.Forms;

public class ScrollableControl : Control
{
    private const int DefaultWheelScrollLines = 3;
    private const int WheelDelta = 120;
    private const int DefaultLineScrollPixels = 16;

    private bool _autoScroll;
    private System.Drawing.Size _autoScrollMargin = System.Drawing.Size.Empty;
    private System.Drawing.Size _autoScrollMinSize = System.Drawing.Size.Empty;

    // Backing store is the WinForms-style negative scroll offset.
    private System.Drawing.Point _autoScrollPosition = System.Drawing.Point.Empty;

    /// <summary>
    /// Overrides the base to subtract AutoScroll offset so FindChildAt hit-testing
    /// works in content coordinates rather than viewport coordinates.
    /// </summary>
    protected override (int contentX, int contentY) ToContentCoordinates(int x, int y)
    {
        if (!AutoScroll) return (x, y);
        return (x - AutoScrollPosition.X, y - AutoScrollPosition.Y);
    }

    public bool AutoScroll
    {
        get => _autoScroll;
        set
        {
            if (_autoScroll != value)
            {
                _autoScroll = value;
                if (!_autoScroll)
                {
                    // WinForms resets scroll position when AutoScroll is disabled.
                    _autoScrollPosition = System.Drawing.Point.Empty;
                }
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the horizontal scroll bar is visible.
    /// In canvas mode this reflects whether content is wider than the viewport with AutoScroll enabled.
    /// </summary>
    public bool HScroll
    {
        get
        {
            if (!AutoScroll) return false;
            var (contentWidth, _) = GetContentSize();
            return contentWidth > ClientSize.Width;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the vertical scroll bar is visible.
    /// In canvas mode this reflects whether content is taller than the viewport with AutoScroll enabled.
    /// </summary>
    public bool VScroll
    {
        get
        {
            if (!AutoScroll) return false;
            var (_, contentHeight) = GetContentSize();
            return contentHeight > ClientSize.Height;
        }
    }

    public System.Drawing.Size AutoScrollMargin
    {
        get => _autoScrollMargin;
        set
        {
            if (_autoScrollMargin != value)
            {
                _autoScrollMargin = value;
                Invalidate();
            }
        }
    }

    public System.Drawing.Size AutoScrollMinSize
    {
        get => _autoScrollMinSize;
        set
        {
            if (_autoScrollMinSize != value)
            {
                _autoScrollMinSize = value;
                CoerceScrollPosition();
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the location of the visible area of the control.
    /// WinForms returns negative coordinates when scrolled.
    /// </summary>
    public System.Drawing.Point AutoScrollPosition
    {
        get => _autoScrollPosition;
        set
        {
            if (!_autoScroll)
            {
                // WinForms ignores setting this when AutoScroll is false.
                _autoScrollPosition = System.Drawing.Point.Empty;
                return;
            }

            if (_autoScrollPosition != value)
            {
                _autoScrollPosition = value;
                CoerceScrollPosition();
                Invalidate();
            }
        }
    }

    public override Rectangle DisplayRectangle
    {
        get
        {
            // DisplayRectangle is the virtual client area for children.
            // For AutoScroll, it is offset by AutoScrollPosition.
            // In WinForms, AutoScrollPosition is negative when scrolled, so this shifts the display rect accordingly.

            var (contentWidth, contentHeight) = GetContentSize();

            if (!AutoScroll)
            {
                return new Rectangle(0, 0, Math.Max(0, contentWidth), Math.Max(0, contentHeight));
            }

            return new Rectangle(
                AutoScrollPosition.X,
                AutoScrollPosition.Y,
                Math.Max(0, contentWidth),
                Math.Max(0, contentHeight));
        }
    }

    /// <summary>
    /// Scrolls the control so that the specified child control is visible.
    /// </summary>
    public void ScrollControlIntoView(Control activeControl)
    {
        if (!AutoScroll) return;
        if (activeControl == null) return;
        if (!Contains(activeControl)) return;

        // Use child bounds in content coordinates (not including scroll offset)
        var childRect = new Rectangle(activeControl.Left, activeControl.Top, activeControl.Width, activeControl.Height);
        var newOffset = CalculateScrollOffsetForVisibility(childRect);
        if (newOffset != GetScrollOffset())
        {
            SetScrollOffset(newOffset);
        }
    }

    protected internal override void OnMouseWheel(MouseEventArgs e)
    {
        if (AutoScroll)
        {
            var delta = e.Delta;
            if (delta != 0)
            {
                // Positive delta = wheel up => scroll content up => decrease scroll offset Y.
                // Our backing store uses positive scroll offset for content displacement, but AutoScrollPosition is negative.
                var lines = DefaultWheelScrollLines;
                var pixels = lines * DefaultLineScrollPixels;
                var steps = delta / WheelDelta;

                var current = GetScrollOffset();
                var newOffset = new System.Drawing.Point(current.X, current.Y - (steps * pixels));
                SetScrollOffset(newOffset);
            }
        }

        base.OnMouseWheel(e);
    }

    protected override System.Drawing.Size GetPreferredSize(System.Drawing.Size proposedSize)
    {
        if (!AutoScroll)
        {
            return base.GetPreferredSize(proposedSize);
        }

        var (contentWidth, contentHeight) = GetContentSize();
        return new System.Drawing.Size(contentWidth, contentHeight);
    }

    private (int width, int height) GetContentSize()
    {
        // Compute the total scrollable content size.
        // Minimum is ClientSize. AutoScrollMinSize forces a minimum content size.
        // Child bounds extend it.

        var maxRight = 0;
        var maxBottom = 0;

        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

            maxRight = Math.Max(maxRight, child.Left + child.Width);
            maxBottom = Math.Max(maxBottom, child.Top + child.Height);
        }

        maxRight += AutoScrollMargin.Width;
        maxBottom += AutoScrollMargin.Height;

        maxRight = Math.Max(maxRight, AutoScrollMinSize.Width);
        maxBottom = Math.Max(maxBottom, AutoScrollMinSize.Height);

        // Also ensure content at least fits the viewport.
        maxRight = Math.Max(maxRight, ClientSize.Width);
        maxBottom = Math.Max(maxBottom, ClientSize.Height);

        return (maxRight, maxBottom);
    }

    private void CoerceScrollPosition()
    {
        if (!AutoScroll)
        {
            _autoScrollPosition = System.Drawing.Point.Empty;
            return;
        }

        // Convert AutoScrollPosition (negative when scrolled) into internal positive offset.
        var desiredOffset = new System.Drawing.Point(-_autoScrollPosition.X, -_autoScrollPosition.Y);
        desiredOffset = CoerceScrollOffset(desiredOffset);
        _autoScrollPosition = new System.Drawing.Point(-desiredOffset.X, -desiredOffset.Y);
    }

    private System.Drawing.Point GetScrollOffset()
    {
        // internal positive offset.
        return new System.Drawing.Point(-AutoScrollPosition.X, -AutoScrollPosition.Y);
    }

    private void SetScrollOffset(System.Drawing.Point offset)
    {
        if (!AutoScroll) return;

        offset = CoerceScrollOffset(offset);

        var newAutoScrollPosition = new System.Drawing.Point(-offset.X, -offset.Y);
        if (_autoScrollPosition != newAutoScrollPosition)
        {
            _autoScrollPosition = newAutoScrollPosition;
            Invalidate();
        }
    }

    private System.Drawing.Point CoerceScrollOffset(System.Drawing.Point offset)
    {
        var (contentWidth, contentHeight) = GetContentSize();

        var maxX = Math.Max(0, contentWidth - ClientSize.Width);
        var maxY = Math.Max(0, contentHeight - ClientSize.Height);

        var x = Math.Max(0, Math.Min(maxX, offset.X));
        var y = Math.Max(0, Math.Min(maxY, offset.Y));

        return new System.Drawing.Point(x, y);
    }

    private System.Drawing.Point CalculateScrollOffsetForVisibility(Rectangle childRect)
    {
        var currentOffset = GetScrollOffset();
        var (contentWidth, contentHeight) = GetContentSize();

        // viewport in content coords
        var viewLeft = currentOffset.X;
        var viewTop = currentOffset.Y;
        var viewRight = currentOffset.X + ClientSize.Width;
        var viewBottom = currentOffset.Y + ClientSize.Height;

        var newX = currentOffset.X;
        var newY = currentOffset.Y;

        if (childRect.Left < viewLeft)
            newX = childRect.Left;
        else if (childRect.Right > viewRight)
            newX = childRect.Right - ClientSize.Width;

        if (childRect.Top < viewTop)
            newY = childRect.Top;
        else if (childRect.Bottom > viewBottom)
            newY = childRect.Bottom - ClientSize.Height;

        // clamp
        var maxX = Math.Max(0, contentWidth - ClientSize.Width);
        var maxY = Math.Max(0, contentHeight - ClientSize.Height);
        newX = Math.Max(0, Math.Min(maxX, newX));
        newY = Math.Max(0, Math.Min(maxY, newY));

        return new System.Drawing.Point(newX, newY);
    }

    public override object? CreateParams => null;

    // ?? Scrollbar rendering ???????????????????????????????????????????????????
    // Draws simple but visually accurate H/V scrollbars over the content area
    // when AutoScroll is enabled and content overflows the viewport.

    private const int ScrollBarThickness = 14;
    private const int ScrollBarMinThumb  = 20;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!AutoScroll || Width <= 0 || Height <= 0) return;

        var (contentW, contentH) = GetContentSize();
        var needsH = contentW > Width;
        var needsV = contentH > Height;

        // If both bars are needed, shrink each viewport dimension by bar thickness.
        var viewW = needsV ? Width  - ScrollBarThickness : Width;
        var viewH = needsH ? Height - ScrollBarThickness : Height;

        // Re-check after accounting for the bars consuming space.
        needsH = contentW > viewW;
        needsV = contentH > viewH;

        var offset = GetScrollOffset();

        if (needsV) DrawVerticalScrollBar(e.Graphics, offset, contentH, viewW, viewH, needsH);
        if (needsH) DrawHorizontalScrollBar(e.Graphics, offset, contentW, viewW, viewH, needsV);

        // Corner square when both bars are visible.
        if (needsH && needsV)
        {
            using var cornerBrush = new SolidBrush(CanvasColor.FromArgb(240, 240, 240));
            e.Graphics.FillRectangle(cornerBrush,
                viewW, viewH, ScrollBarThickness, ScrollBarThickness);
        }
    }

    private void DrawVerticalScrollBar(Graphics g,
        System.Drawing.Point offset, int contentH,
        int viewW, int viewH, bool hasHBar)
    {
        var trackX = viewW;
        var trackY = 0;
        var trackH = viewH;

        // Track background.
        using var trackBrush = new SolidBrush(CanvasColor.FromArgb(240, 240, 240));
        g.FillRectangle(trackBrush, trackX, trackY, ScrollBarThickness, trackH);

        // Track border.
        using var borderPen = new Pen(CanvasColor.FromArgb(205, 205, 205));
        g.DrawLine(borderPen, trackX, trackY, trackX, trackY + trackH);

        // Thumb.
        var maxOffset = Math.Max(1, contentH - viewH);
        var thumbRatio = Math.Min(1.0, (double)viewH / contentH);
        var thumbH     = Math.Max(ScrollBarMinThumb, (int)(trackH * thumbRatio));
        var thumbY     = trackY + (int)((trackH - thumbH) * Math.Min(1.0, (double)offset.Y / maxOffset));

        using var thumbBrush = new SolidBrush(CanvasColor.FromArgb(180, 180, 180));
        g.FillRectangle(thumbBrush, trackX + 2, thumbY + 1, ScrollBarThickness - 4, thumbH - 2);
    }

    private void DrawHorizontalScrollBar(Graphics g,
        System.Drawing.Point offset, int contentW,
        int viewW, int viewH, bool hasVBar)
    {
        var trackX = 0;
        var trackY = viewH;
        var trackW = viewW;

        // Track background.
        using var trackBrush = new SolidBrush(CanvasColor.FromArgb(240, 240, 240));
        g.FillRectangle(trackBrush, trackX, trackY, trackW, ScrollBarThickness);

        // Track border.
        using var borderPen = new Pen(CanvasColor.FromArgb(205, 205, 205));
        g.DrawLine(borderPen, trackX, trackY, trackX + trackW, trackY);

        // Thumb.
        var maxOffset = Math.Max(1, contentW - viewW);
        var thumbRatio = Math.Min(1.0, (double)viewW / contentW);
        var thumbW     = Math.Max(ScrollBarMinThumb, (int)(trackW * thumbRatio));
        var thumbX     = trackX + (int)((trackW - thumbW) * Math.Min(1.0, (double)offset.X / maxOffset));

        using var thumbBrush = new SolidBrush(CanvasColor.FromArgb(180, 180, 180));
        g.FillRectangle(thumbBrush, thumbX + 1, trackY + 2, thumbW - 2, ScrollBarThickness - 4);
    }
}

