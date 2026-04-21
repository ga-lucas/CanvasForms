
namespace System.Windows.Forms;

public class GroupBox : Control
{
    private FlatStyle _flatStyle = FlatStyle.Standard;

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
            // Flat: single-line border, no etching
            using var pen = new Pen(borderColor);

            g.DrawLine(pen, rect.X, rect.Y, rect.X, rect.Bottom);
            g.DrawLine(pen, rect.Right, rect.Y, rect.Right, rect.Bottom);
            g.DrawLine(pen, rect.X, rect.Bottom, rect.Right, rect.Bottom);

            if (gapRight <= gapLeft)
            {
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Y);
            }
            else
            {
                g.DrawLine(pen, rect.X, rect.Y, rect.X + gapLeft - 1, rect.Y);
                g.DrawLine(pen, rect.X + gapRight + 1, rect.Y, rect.Right, rect.Y);
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

