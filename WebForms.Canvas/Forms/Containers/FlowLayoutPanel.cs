using System.Runtime.CompilerServices;

namespace System.Windows.Forms;

public class FlowLayoutPanel : Panel
{
  private static readonly ConditionalWeakTable<Control, FlowLayoutInfo> s_layoutInfo = new();

    private FlowDirection _flowDirection = FlowDirection.LeftToRight;
    private bool _wrapContents = true;

    public FlowDirection FlowDirection
    {
        get => _flowDirection;
        set
        {
            if (_flowDirection != value)
            {
                _flowDirection = value;
                PerformLayout();
                Invalidate();
            }
        }
    }

    public static void SetFlowBreak(Control control, bool value)
    {
        if (control == null) return;
        var info = s_layoutInfo.GetOrCreateValue(control);
        info.FlowBreak = value;
        (control.Parent as FlowLayoutPanel)?.PerformLayout();
        control.Parent?.Invalidate();
    }

    public static bool GetFlowBreak(Control control)
    {
        if (control == null) return false;
        return s_layoutInfo.TryGetValue(control, out var info) && info.FlowBreak;
    }

    public bool WrapContents
    {
        get => _wrapContents;
        set
        {
            if (_wrapContents != value)
            {
                _wrapContents = value;
                PerformLayout();
                Invalidate();
            }
        }
    }

    public override void PerformLayout()
    {
        if (IsLayoutSuspended) return;

        if (Controls.Count == 0)
        {
            return;
        }

        var borderWidth = GetBorderWidth();

        var paddingX = Padding.Width;
        var paddingY = Padding.Height;

        var innerLeft = borderWidth + paddingX;
        var innerTop = borderWidth + paddingY;
        var innerWidth = System.Math.Max(0, Width - (borderWidth * 2) - (paddingX * 2));
        var innerHeight = System.Math.Max(0, Height - (borderWidth * 2) - (paddingY * 2));

        switch (_flowDirection)
        {
            case FlowDirection.LeftToRight:
                LayoutLeftToRight(innerLeft, innerTop, innerWidth);
                break;
            case FlowDirection.RightToLeft:
                LayoutRightToLeft(innerLeft, innerTop, innerWidth);
                break;
            case FlowDirection.TopDown:
                LayoutTopDown(innerLeft, innerTop, innerHeight);
                break;
            case FlowDirection.BottomUp:
                LayoutBottomUp(innerLeft, innerTop, innerHeight);
                break;
        }
    }

    private void LayoutLeftToRight(int x0, int y0, int width)
    {
        var x = x0;
        var y = y0;
        var rowHeight = 0;

        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

            var marginX = child.Margin.Width;
            var marginY = child.Margin.Height;

            var cw = child.Width;
            var ch = child.Height;

            var neededWidth = cw + (marginX * 2);

            if (_wrapContents && x != x0 && x - x0 + neededWidth > width)
            {
                x = x0;
                y += rowHeight;
                rowHeight = 0;
            }

            child.Left = x + marginX;
            child.Top = y + marginY;

            x += neededWidth;
            rowHeight = System.Math.Max(rowHeight, ch + (marginY * 2));

            if (GetFlowBreak(child))
            {
                x = x0;
                y += rowHeight;
                rowHeight = 0;
            }
        }
    }

    private void LayoutRightToLeft(int x0, int y0, int width)
    {
        var x = x0 + width;
        var y = y0;
        var rowHeight = 0;

        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

            var marginX = child.Margin.Width;
            var marginY = child.Margin.Height;

            var cw = child.Width;
            var ch = child.Height;

            var neededWidth = cw + (marginX * 2);

            if (_wrapContents && x != x0 + width && (x0 + width) - x + neededWidth > width)
            {
                x = x0 + width;
                y += rowHeight;
                rowHeight = 0;
            }

            x -= neededWidth;
            child.Left = x + marginX;
            child.Top = y + marginY;

            rowHeight = System.Math.Max(rowHeight, ch + (marginY * 2));

            if (GetFlowBreak(child))
            {
                x = x0 + width;
                y += rowHeight;
                rowHeight = 0;
            }
        }
    }

    private void LayoutTopDown(int x0, int y0, int height)
    {
        var x = x0;
        var y = y0;
        var colWidth = 0;

        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

            var marginX = child.Margin.Width;
            var marginY = child.Margin.Height;

            var cw = child.Width;
            var ch = child.Height;

            var neededHeight = ch + (marginY * 2);

            if (_wrapContents && y != y0 && y - y0 + neededHeight > height)
            {
                y = y0;
                x += colWidth;
                colWidth = 0;
            }

            child.Left = x + marginX;
            child.Top = y + marginY;

            y += neededHeight;
            colWidth = System.Math.Max(colWidth, cw + (marginX * 2));

            if (GetFlowBreak(child))
            {
                y = y0;
                x += colWidth;
                colWidth = 0;
            }
        }
    }

    private void LayoutBottomUp(int x0, int y0, int height)
    {
        var x = x0;
        var y = y0 + height;
        var colWidth = 0;

        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

            var marginX = child.Margin.Width;
            var marginY = child.Margin.Height;

            var cw = child.Width;
            var ch = child.Height;

            var neededHeight = ch + (marginY * 2);

            if (_wrapContents && y != y0 + height && (y0 + height) - y + neededHeight > height)
            {
                y = y0 + height;
                x += colWidth;
                colWidth = 0;
            }

            y -= neededHeight;
            child.Left = x + marginX;
            child.Top = y + marginY;

            colWidth = System.Math.Max(colWidth, cw + (marginX * 2));

            if (GetFlowBreak(child))
            {
                y = y0 + height;
                x += colWidth;
                colWidth = 0;
            }
        }
    }

    private sealed class FlowLayoutInfo
    {
        public bool FlowBreak { get; set; }
    }

    private int GetBorderWidth()
    {
        return BorderStyle switch
        {
            BorderStyle.Fixed3D => 2,
            BorderStyle.FixedSingle => 1,
            _ => 0
        };
    }
}

public enum FlowDirection
{
    LeftToRight = 0,
    TopDown = 1,
    RightToLeft = 2,
    BottomUp = 3
}
