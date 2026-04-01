using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class RadioButton : Control
{
    private bool _isHovered = false;

    public RadioButton()
    {
        Width = 100;
        Height = 20;
        BackColor = Color.Transparent;
        ForeColor = Color.Black;
        Text = "RadioButton";
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

        // Draw radio button circle (13x13)
        const int circleSize = 13;
        var circleBounds = new Rectangle(0, (Height - circleSize) / 2, circleSize, circleSize);

        // Background
        var bgColor = Enabled ? Color.White : Color.FromArgb(240, 240, 240);
        g.FillEllipse(new SolidBrush(bgColor), circleBounds);

        // Border
        var borderColor = _isHovered && Enabled ? Color.FromArgb(0, 120, 215) : Color.FromArgb(122, 122, 122);
        g.DrawEllipse(new Pen(borderColor), circleBounds);

        // Draw inner dot if checked
        if (Checked)
        {
            var dotColor = Enabled ? Color.FromArgb(0, 120, 215) : Color.FromArgb(109, 109, 109);
            var dotBounds = new Rectangle(4, (Height - 5) / 2, 5, 5);
            g.FillEllipse(new SolidBrush(dotColor), dotBounds);
        }

        // Draw text
        if (!string.IsNullOrEmpty(Text))
        {
            var textColor = Enabled ? ForeColor : Color.FromArgb(109, 109, 109);
            g.DrawString(Text, circleSize + 4, (Height - 14) / 2, textColor);
        }

        // Draw focus rectangle if focused
        if (Focused && Enabled)
        {
            var textWidth = string.IsNullOrEmpty(Text) ? 0 : Text.Length * 7;
            var focusRect = new Rectangle(0, 0, circleSize + 4 + textWidth + 2, Height);
            using var focusPen = new Pen(Color.Black);
            g.DrawRectangle(focusPen, focusRect);
        }

        base.OnPaint(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled && e.Button == MouseButtons.Left && !Checked)
        {
            // Uncheck other radio buttons in the same parent
            if (Parent != null)
            {
                foreach (var control in Parent.Controls)
                {
                    if (control is RadioButton rb && rb != this)
                    {
                        rb.Checked = false;
                        rb.Invalidate();
                    }
                }
            }

            Checked = true;
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
