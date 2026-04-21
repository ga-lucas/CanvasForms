
namespace System.Windows.Forms;

public class Panel : ScrollableControl
{
    private BorderStyle _borderStyle = BorderStyle.None;

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

        // Use DisplayRectangle (WinForms) to obtain scroll offset.
        var scrollX = AutoScroll ? DisplayRectangle.X : 0;
        var scrollY = AutoScroll ? DisplayRectangle.Y : 0;

        // Let user code paint first (events etc.)
        base.OnPaint(e);

        // Paint child controls
        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

            g.TranslateTransform(scrollX + child.Left, scrollY + child.Top);

            var childArgs = new PaintEventArgs(
                g,
                new Rectangle(0, 0, child.Width, child.Height)
            );

            if (child is ComboBox comboBox)
            {
                comboBox.PaintWithoutDropDown(childArgs);
            }
            else if (child is DateTimePicker dateTimePicker)
            {
                dateTimePicker.PaintWithoutDropDown(childArgs);
            }
            else if (child is TextBox textBox)
            {
                textBox.PaintWithoutAutoComplete(childArgs);
            }
            else
            {
                child.OnPaint(childArgs);
            }

            g.TranslateTransform(-(scrollX + child.Left), -(scrollY + child.Top));
        }

        // Overlays (ComboBox drop-down, DateTimePicker popup, TextBox autocomplete) are painted
        // by the Form in a final pass so they always appear top-most, even for nested controls.

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
