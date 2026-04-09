using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms radio button control
/// </summary>
public class RadioButton : ToggleButtonBase
{
    public RadioButton()
    {
        Width = 100;
        Height = 20;
        BackColor = Color.Transparent;
        ForeColor = Color.Black;
        Text = "RadioButton";
    }

    public Appearance Appearance { get; set; } = Appearance.Normal;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        DrawControlBackground(g);

        const int circleSize = 13;
        var circleBounds = new Rectangle(0, (Height - circleSize) / 2, circleSize, circleSize);

        using (var bgBrush = new SolidBrush(GetIndicatorBackColor()))
            g.FillEllipse(bgBrush, circleBounds);

        using (var borderPen = new Pen(GetIndicatorBorderColor()))
            g.DrawEllipse(borderPen, circleBounds);

        if (Checked)
        {
            var dotBounds = new Rectangle(circleBounds.X + 4, (Height - 5) / 2, 5, 5);
            using var dotBrush = new SolidBrush(GetIndicatorMarkColor());
            g.FillEllipse(dotBrush, dotBounds);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            var textY = (Height - 14) / 2 + 2;
            g.DrawString(Text, circleSize + 4, textY, EffectiveForeColor);
        }

        var textWidth = string.IsNullOrEmpty(Text) ? 0 : Text.Length * 7;
        DrawFocusRect(g, new Rectangle(0, 0, circleSize + 4 + textWidth + 2, Height - 1));

        base.OnPaint(e);
    }

    protected override void OnClick(EventArgs e)
    {
        if (!Checked)
        {
            if (Parent != null)
            {
                foreach (var control in Parent.Controls)
                {
                    if (control is RadioButton rb && rb != this)
                        rb.Checked = false;
                }
            }
            Checked = true;
        }
        base.OnClick(e);
    }

    public new void PerformClick() => OnClick(EventArgs.Empty);
}

