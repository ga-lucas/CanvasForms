using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

public class GroupBox : Control
{
    private Control? _mouseCaptureChild;

    public GroupBox()
    {
        TabStop = false;
        BackColor = System.Drawing.Color.Transparent;
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

        // Clip child painting to the inside of the border.
        var textHeight = Font.Height;
        var borderTop = Math.Max(0, textHeight / 2);
        var inner = new Rectangle(
            1,
            borderTop + 1,
            Math.Max(0, Width - 2),
            Math.Max(0, Height - (borderTop + 2))
        );

        g.Save();
        g.SetClip(inner);

        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

            g.TranslateTransform(child.Left, child.Top);

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

            g.TranslateTransform(-child.Left, -child.Top);
        }

        g.Restore();

        // Paint overlays on top of everything (drop-downs, autocomplete)
        // These must not be clipped to the GroupBox client area; WinForms drop-downs render outside.
        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

            if (child is ComboBox comboBox && comboBox.DroppedDown)
            {
                g.TranslateTransform(child.Left, child.Top);
                var ddArgs = new PaintEventArgs(g, new Rectangle(0, 0, child.Width, child.Height));
                comboBox.PaintDropDownOnly(ddArgs);
                g.TranslateTransform(-child.Left, -child.Top);
            }
            else if (child is DateTimePicker dateTimePicker && dateTimePicker.HasVisibleDropDown)
            {
                g.TranslateTransform(child.Left, child.Top);
                var ddArgs = new PaintEventArgs(g, new Rectangle(0, 0, child.Width, child.Height));
                dateTimePicker.PaintDropDownOnly(ddArgs);
                g.TranslateTransform(-child.Left, -child.Top);
            }
            else if (child is TextBox textBox && textBox.HasVisibleAutoComplete)
            {
                g.TranslateTransform(child.Left, child.Top);
                var acArgs = new PaintEventArgs(g, new Rectangle(0, 0, child.Width, child.Height));
                textBox.PaintAutoCompleteOnly(acArgs);
                g.TranslateTransform(-child.Left, -child.Top);
            }
        }
    }

    private void DrawGroupBoxBorderAndText(Graphics g)
    {
        var text = Text ?? string.Empty;
        var textHeight = Font.Height;
        var borderTop = Math.Max(0, textHeight / 2);

        var borderColor = Color.FromArgb(122, 122, 122);
        using var pen = new Pen(borderColor);

        // Outer rectangle (top starts below text baseline area)
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

        // Left, right, bottom
        g.DrawLine(pen, rect.X, rect.Y, rect.X, rect.Bottom);
        g.DrawLine(pen, rect.Right, rect.Y, rect.Right, rect.Bottom);
        g.DrawLine(pen, rect.X, rect.Bottom, rect.Right, rect.Bottom);

        // Top line with a gap for the caption
        if (gapRight <= gapLeft)
        {
            g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Y);
        }
        else
        {
            // Left segment
            g.DrawLine(pen, rect.X, rect.Y, rect.X + gapLeft - 1, rect.Y);
            // Right segment
            g.DrawLine(pen, rect.X + gapRight + 1, rect.Y, rect.Right, rect.Y);
        }

        if (!string.IsNullOrEmpty(text))
        {
            // Clear background behind caption to match WinForms (uses parent back color).
            var bg = BackColor != System.Drawing.Color.Transparent ? BackColor : (Parent?.BackColor ?? System.Drawing.Color.White);
            using var bgBrush = new SolidBrush(bg);
            g.FillRectangle(bgBrush, gapLeft, 0, Math.Max(0, textWidth + (textPad * 2)), textHeight);

            g.DrawString(text, Font, ForeColor, gapLeft + textPad, 0);
        }
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

            var args = new MouseEventArgs(e.Button, e.Clicks, e.X - child.Left, e.Y - child.Top);
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
            var args = new MouseEventArgs(e.Button, e.Clicks, e.X - child.Left, e.Y - child.Top);
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
            var args = new MouseEventArgs(e.Button, e.Clicks, e.X - child.Left, e.Y - child.Top);
            child.OnMouseUp(args);
        }
        else
        {
            base.OnMouseUp(e);
        }

        _mouseCaptureChild = null;
    }

    protected internal override void OnMouseWheel(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseWheel(e);
            return;
        }

        var child = FindChildAt(e.X, e.Y);
        if (child != null)
        {
            var args = new MouseEventArgs(e.Button, e.Clicks, e.X - child.Left, e.Y - child.Top, e.Delta);
            child.OnMouseWheel(args);
            return;
        }

        base.OnMouseWheel(e);
    }

    private void SetFormFocusedControl(Control control)
    {
        if (FindForm() is Form form)
        {
            form.FocusedControl = control;
        }

        // Match WinForms behavior: clicking a child control focuses it so it can receive keyboard input.
        // This is required for TextBox editing/caret and consistent ComboBox activation.
        control.Focus();
    }

    private Control? FindChildAt(int x, int y)
    {
        for (var i = Controls.Count - 1; i >= 0; i--)
        {
            var child = Controls[i];
            if (!child.Visible || !child.Enabled) continue;

            if (HitTest(child, x, y))
            {
                return child;
            }
        }

        return null;
    }

    private bool HitTest(Control child, int x, int y)
    {
        var inNormalBounds = x >= child.Left && x < child.Left + child.Width &&
                         y >= child.Top && y < child.Top + child.Height;

        if (inNormalBounds)
            return true;

        if (child is ComboBox comboBox && comboBox.DroppedDown)
        {
            var dropDownHeight = comboBox.DropDownHeight;
            var dropDownWidth = comboBox.DropDownWidth;

            return x >= child.Left && x < child.Left + dropDownWidth &&
                   y >= child.Top + child.Height && y < child.Top + child.Height + dropDownHeight;
        }

        if (child is DateTimePicker dateTimePicker && dateTimePicker.DroppedDown)
        {
            var dd = dateTimePicker.GetDropDownBounds();

            return x >= child.Left + dd.X && x < child.Left + dd.Right &&
                   y >= child.Top + dd.Y && y < child.Top + dd.Bottom;
        }

        if (child is TextBox textBox && textBox.HasVisibleAutoComplete)
        {
            var panelBounds = textBox.GetAutoCompletePanelBounds();

            return x >= child.Left + panelBounds.X && x < child.Left + panelBounds.Right &&
                   y >= child.Top + panelBounds.Y && y < child.Top + panelBounds.Bottom;
        }

        return false;
    }
}
