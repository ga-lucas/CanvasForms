using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms checkbox control
/// </summary>
public class CheckBox : ToggleButtonBase
{
    public CheckBox()
    {
        Width = 100;
        Height = 20;
        BackColor = Color.Transparent;
        ForeColor = Color.Black;
        Text = "CheckBox";
    }

    public Appearance Appearance { get; set; } = Appearance.Normal;
    public bool ThreeState { get; set; } = false;
    public CheckState CheckState { get; set; } = CheckState.Unchecked;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        DrawControlBackground(g);

        const int boxSize = 13;
        var boxBounds = new Rectangle(0, (Height - boxSize) / 2, boxSize, boxSize);

        using (var bgBrush = new SolidBrush(GetIndicatorBackColor()))
            g.FillRectangle(bgBrush, boxBounds);

        using (var borderPen = new Pen(GetIndicatorBorderColor()))
            g.DrawRectangle(borderPen, boxBounds);

        if (Checked)
        {
            using var pen = new Pen(GetIndicatorMarkColor(), 2);
            var boxY = boxBounds.Y;
            g.DrawLine(pen, boxBounds.X + 3,          boxY + boxSize / 2,
                            boxBounds.X + boxSize / 2 - 1, boxY + boxSize - 3);
            g.DrawLine(pen, boxBounds.X + boxSize / 2 - 1, boxY + boxSize - 3,
                            boxBounds.X + boxSize - 3,      boxY + 3);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            var textY = (Height - 14) / 2 + 2;
            g.DrawString(Text, boxSize + 4, textY, EffectiveForeColor);
        }

        var textWidth = string.IsNullOrEmpty(Text) ? 0 : Text.Length * 7;
        DrawFocusRect(g, new Rectangle(0, 0, boxSize + 4 + textWidth + 2, Height - 1));

        base.OnPaint(e);
    }

    protected override void OnClick(EventArgs e)
    {
        if (AutoCheck) Checked = !Checked;
        base.OnClick(e);
    }
}

/// <summary>Specifies the appearance of a check box</summary>
public enum Appearance { Normal, Button }

/// <summary>Specifies the state of a check box</summary>
public enum CheckState { Unchecked, Checked, Indeterminate }

