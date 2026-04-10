using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

public class Panel : ScrollableControl
{
    private BorderStyle _borderStyle = BorderStyle.None;

    private Control? _mouseCaptureChild;

    public Panel()
    {
        TabStop = false;
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

        // Paint overlays on top of everything (drop-downs, autocomplete)
        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

            if (child is ComboBox comboBox && comboBox.DroppedDown)
            {
                g.TranslateTransform(scrollX + child.Left, scrollY + child.Top);
                var ddArgs = new PaintEventArgs(g, new Rectangle(0, 0, child.Width, child.Height));
                comboBox.PaintDropDownOnly(ddArgs);
                g.TranslateTransform(-(scrollX + child.Left), -(scrollY + child.Top));
            }
            else if (child is DateTimePicker dateTimePicker && dateTimePicker.HasVisibleDropDown)
            {
                g.TranslateTransform(scrollX + child.Left, scrollY + child.Top);
                var ddArgs = new PaintEventArgs(g, new Rectangle(0, 0, child.Width, child.Height));
                dateTimePicker.PaintDropDownOnly(ddArgs);
                g.TranslateTransform(-(scrollX + child.Left), -(scrollY + child.Top));
            }
            else if (child is TextBox textBox && textBox.HasVisibleAutoComplete)
            {
                g.TranslateTransform(scrollX + child.Left, scrollY + child.Top);
                var acArgs = new PaintEventArgs(g, new Rectangle(0, 0, child.Width, child.Height));
                textBox.PaintAutoCompleteOnly(acArgs);
                g.TranslateTransform(-(scrollX + child.Left), -(scrollY + child.Top));
            }
        }

        g.Restore();
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseDown(e);
            return;
        }

        var child = FindChildAt(e.X, e.Y);
        if (child != null)
        {
            _mouseCaptureChild = child;
            SetFormFocusedControl(child);

            var (cx, cy) = ToChildCoordinates(child, e.X, e.Y);
            var args = new MouseEventArgs(e.Button, e.Clicks, cx, cy);
            child.OnMouseDown(args);
            return;
        }

        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseMove(e);
            return;
        }

        var child = _mouseCaptureChild ?? FindChildAt(e.X, e.Y);
        if (child != null)
        {
            var (cx, cy) = ToChildCoordinates(child, e.X, e.Y);
            var args = new MouseEventArgs(e.Button, e.Clicks, cx, cy);
            child.OnMouseMove(args);
            return;
        }

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseUp(e);
            return;
        }

        var child = _mouseCaptureChild ?? FindChildAt(e.X, e.Y);
        if (child != null)
        {
            var (cx, cy) = ToChildCoordinates(child, e.X, e.Y);
            var args = new MouseEventArgs(e.Button, e.Clicks, cx, cy);
            child.OnMouseUp(args);
        }
        else
        {
            base.OnMouseUp(e);
        }

        _mouseCaptureChild = null;
    }

    private void SetFormFocusedControl(Control control)
    {
        // Keep behavior consistent with Form (it uses FocusedControl for routing keyboard and caret painting)
        if (GetContainerControl() is Form form)
        {
            form.FocusedControl = control;
        }
    }

    private Control? FindChildAt(int x, int y)
    {
        // Convert point from viewport coords to content coords.
        var scrollX = AutoScroll ? DisplayRectangle.X : 0;
        var scrollY = AutoScroll ? DisplayRectangle.Y : 0;

        var contentX = x - scrollX;
        var contentY = y - scrollY;

        // Check children from top-most to bottom-most.
        for (var i = Controls.Count - 1; i >= 0; i--)
        {
            var child = Controls[i];
            if (!child.Visible || !child.Enabled) continue;

            if (HitTest(child, contentX, contentY))
            {
                return child;
            }
        }

        return null;
    }

    private (int x, int y) ToChildCoordinates(Control child, int x, int y)
    {
        var scrollX = AutoScroll ? DisplayRectangle.X : 0;
        var scrollY = AutoScroll ? DisplayRectangle.Y : 0;

        var contentX = x - scrollX;
        var contentY = y - scrollY;

        return (contentX - child.Left, contentY - child.Top);
    }

    private bool HitTest(Control child, int x, int y)
    {
        // Normal bounds check
        var inNormalBounds = x >= child.Left && x < child.Left + child.Width &&
                             y >= child.Top && y < child.Top + child.Height;

        if (inNormalBounds)
            return true;

        // Special case: ComboBox with drop-down open
        if (child is ComboBox comboBox && comboBox.DroppedDown)
        {
            var dropDownHeight = comboBox.DropDownHeight;
            var dropDownWidth = comboBox.DropDownWidth;

            return x >= child.Left && x < child.Left + dropDownWidth &&
                   y >= child.Top + child.Height && y < child.Top + child.Height + dropDownHeight;
        }

        // Special case: DateTimePicker with drop-down open
        if (child is DateTimePicker dateTimePicker && dateTimePicker.DroppedDown)
        {
            var dd = dateTimePicker.GetDropDownBounds();

            return x >= child.Left + dd.X && x < child.Left + dd.Right &&
                   y >= child.Top + dd.Y && y < child.Top + dd.Bottom;
        }

        // Special case: TextBox with autocomplete panel open
        if (child is TextBox textBox && textBox.HasVisibleAutoComplete)
        {
            var panelBounds = textBox.GetAutoCompletePanelBounds();

            return x >= child.Left + panelBounds.X && x < child.Left + panelBounds.Right &&
                   y >= child.Top + panelBounds.Y && y < child.Top + panelBounds.Bottom;
        }

        return false;
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
            case BorderStyle.Fixed3D:
                using (var pen = new Pen(Color.FromArgb(122, 122, 122)))
                {
                    g.DrawRectangle(pen, bounds);
                }
                break;
        }
    }
}
