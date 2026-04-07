using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms radio button control
/// </summary>
public class RadioButton : ButtonBase
{
    private bool _checked = false;

    public RadioButton()
    {
        Width = 100;
        Height = 20;
        BackColor = Color.Transparent;
        ForeColor = Color.Black;
        Text = "RadioButton";
    }

    /// <summary>
    /// Gets or sets a value indicating whether the radio button is checked
    /// </summary>
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked != value)
            {
                _checked = value;
                OnCheckedChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

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

        // Border - use hover state from ButtonBase
        var state = GetButtonState();
        var borderColor = (state == ButtonState.Hot || state == ButtonState.Pushed) && Enabled
            ? Color.FromArgb(0, 120, 215)
            : Color.FromArgb(122, 122, 122);
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
            // Align text vertically with the radio button - account for font baseline being 'top'
            var textY = (Height - 14) / 2 + 2; // +2 to account for typical font baseline offset
            g.DrawString(Text, circleSize + 4, textY, textColor);
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

    protected override void OnClick(EventArgs e)
    {
        // Only check if not already checked
        if (!Checked)
        {
            // Uncheck other radio buttons in the same parent
            if (Parent != null)
            {
                foreach (var control in Parent.Controls)
                {
                    if (control is RadioButton rb && rb != this)
                    {
                        rb.Checked = false;
                    }
                }
            }

            Checked = true;
        }

        base.OnClick(e);
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

    /// <summary>
    /// Gets or sets the appearance of the radio button
    /// </summary>
    public Appearance Appearance { get; set; } = Appearance.Normal;

    /// <summary>
    /// Gets or sets a value indicating whether the radio button is automatically checked on click
    /// </summary>
    public bool AutoCheck { get; set; } = true;

    /// <summary>
    /// Simulates a click on the radio button
    /// </summary>
    public void PerformClick()
    {
        OnClick(EventArgs.Empty);
    }
}
