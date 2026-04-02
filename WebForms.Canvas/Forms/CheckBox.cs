using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

/// <summary>
/// Represents a Windows Forms checkbox control
/// </summary>
public class CheckBox : ButtonBase
{
    private bool _checked = false;

    public CheckBox()
    {
        Width = 100;
        Height = 20;
        BackColor = Color.Transparent;
        ForeColor = Color.Black;
        Text = "CheckBox";
    }

    /// <summary>
    /// Gets or sets a value indicating whether the check box is checked
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

        // Draw checkbox box (13x13 square)
        const int boxSize = 13;
        var boxBounds = new Rectangle(0, (Height - boxSize) / 2, boxSize, boxSize);

        // Background
        var bgColor = Enabled ? Color.White : Color.FromArgb(240, 240, 240);
        g.FillRectangle(new SolidBrush(bgColor), boxBounds);

        // Border - use hover state from ButtonBase
        var state = GetButtonState();
        var borderColor = (state == ButtonState.Hot || state == ButtonState.Pushed) && Enabled
            ? Color.FromArgb(0, 120, 215)
            : Color.FromArgb(122, 122, 122);
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

    protected override void OnClick(EventArgs e)
    {
        // Toggle checked state on click
        Checked = !Checked;
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
    /// Gets or sets the appearance of the check box
    /// </summary>
    public Appearance Appearance { get; set; } = Appearance.Normal;

    /// <summary>
    /// Gets or sets a value indicating whether the check box will allow three check states
    /// </summary>
    public bool ThreeState { get; set; } = false;

    /// <summary>
    /// Gets or sets the state of the check box
    /// </summary>
    public CheckState CheckState { get; set; } = CheckState.Unchecked;

    /// <summary>
    /// Gets or sets a value indicating whether the check box is automatically checked on click
    /// </summary>
    public bool AutoCheck { get; set; } = true;
}

/// <summary>
/// Specifies the appearance of a check box
/// </summary>
public enum Appearance
{
    Normal,
    Button
}

/// <summary>
/// Specifies the state of a check box
/// </summary>
public enum CheckState
{
    Unchecked,
    Checked,
    Indeterminate
}
