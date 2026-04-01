using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class CheckBox : Control
{
    private bool _isHovered = false;

    public CheckBox()
    {
        Width = 100;
        Height = 20;
        BackColor = Color.Transparent;
        ForeColor = Color.Black;
        Text = "CheckBox";
        Checked = false;
    }

    public bool Checked { get; set; }
    public event EventHandler? CheckedChanged;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw background if not transparent
        if (BackColor != Color.Transparent)
        {
            using var bgBrush = new SolidBrush(BackColor);
            g.FillRectangle(bgBrush, 0, 0, Width, Height);
        }

        // Draw checkbox box (13x13 square)
        const int boxSize = 13;
        var boxBounds = new Rectangle(0, (Height - boxSize) / 2, boxSize, boxSize);

        // Background
        var bgColor = Enabled ? Color.White : Color.FromArgb(240, 240, 240);
        g.FillRectangle(new SolidBrush(bgColor), boxBounds);

        // Border
        var borderColor = _isHovered && Enabled ? Color.FromArgb(0, 120, 215) : Color.FromArgb(122, 122, 122);
        g.DrawRectangle(new Pen(borderColor), boxBounds);

        // Draw checkmark if checked
        if (Checked)
        {
            var checkColor = Enabled ? Color.FromArgb(0, 120, 215) : Color.FromArgb(109, 109, 109);
            var pen = new Pen(checkColor, 2);

            // Draw checkmark (simple lines)
            g.DrawLine(pen, 2, boxSize / 2, boxSize / 2 - 1, boxSize - 3);
            g.DrawLine(pen, boxSize / 2 - 1, boxSize - 3, boxSize - 2, 2);
        }

        // Draw text
        if (!string.IsNullOrEmpty(Text))
        {
            var textColor = Enabled ? ForeColor : Color.FromArgb(109, 109, 109);
            g.DrawString(Text, boxSize + 4, (Height - 14) / 2, textColor);
        }

        // Draw focus rectangle if focused
        if (Focused && Enabled)
        {
            var textWidth = string.IsNullOrEmpty(Text) ? 0 : Text.Length * 7;
            var focusRect = new Rectangle(0, 0, boxSize + 4 + textWidth + 2, Height);
            using var focusPen = new Pen(Color.Black);
            g.DrawRectangle(focusPen, focusRect);
        }

        base.OnPaint(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled && e.Button == MouseButtons.Left)
        {
            Checked = !Checked;
            OnCheckedChanged(EventArgs.Empty);
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseEnter(EventArgs e)
    {
        if (Enabled)
        {
            _isHovered = true;
            Invalidate();
        }
        base.OnMouseEnter(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_isHovered)
        {
            _isHovered = false;
            Invalidate();
        }
        base.OnMouseLeave(e);
    }

    protected virtual void OnCheckedChanged(EventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
    }

    protected internal override void OnGotFocus(EventArgs e)
    {
        Invalidate();
        base.OnGotFocus(e);
    }

    protected internal override void OnLostFocus(EventArgs e)
    {
        Invalidate();
        base.OnLostFocus(e);
    }
}
