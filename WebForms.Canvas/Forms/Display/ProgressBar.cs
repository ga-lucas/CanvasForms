using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms ProgressBar control
/// </summary>
public class ProgressBar : Control
{
    private int _minimum = 0;
    private int _maximum = 100;
    private int _value = 0;
    private int _step = 10;
    private ProgressBarStyle _style = ProgressBarStyle.Blocks;

    public ProgressBar()
    {
        Width = 100;
        Height = 23;
        BackColor = Color.FromArgb(227, 227, 227);
        ForeColor = Color.FromArgb(6, 176, 37);
        TabStop = false;
    }

    public int Minimum
    {
        get => _minimum;
        set { _minimum = value; if (_value < _minimum) _value = _minimum; Invalidate(); }
    }

    public int Maximum
    {
        get => _maximum;
        set { _maximum = value; if (_value > _maximum) _value = _maximum; Invalidate(); }
    }

    public int Value
    {
        get => _value;
        set
        {
            var clamped = Math.Max(_minimum, Math.Min(_maximum, value));
            if (_value != clamped) { _value = clamped; Invalidate(); }
        }
    }

    public int Step { get => _step; set => _step = value; }

    public ProgressBarStyle Style
    {
        get => _style;
        set { _style = value; Invalidate(); }
    }

    public void PerformStep() => Value = Math.Min(_maximum, _value + _step);

    public void Increment(int value) => Value = Math.Min(_maximum, _value + value);

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        // Border
        using var borderPen = new Pen(Color.FromArgb(188, 188, 188));
        g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        if (_maximum <= _minimum) { base.OnPaint(e); return; }

        double fraction = (double)(_value - _minimum) / (_maximum - _minimum);
        int fillWidth = (int)(fraction * (Width - 2));

        if (fillWidth <= 0) { base.OnPaint(e); return; }

        if (_style == ProgressBarStyle.Marquee)
        {
            // Marquee: draw a moving segment (static representation)
            using var marqueeBrush = new SolidBrush(ForeColor);
            int segW = Math.Max(20, Width / 4);
            g.FillRectangle(marqueeBrush, 1, 1, segW, Height - 2);
        }
        else if (_style == ProgressBarStyle.Blocks)
        {
            // Draw block segments
            const int blockWidth = 10;
            const int blockGap = 2;
            int x = 1;
            using var blockBrush = new SolidBrush(ForeColor);
            while (x + blockWidth <= fillWidth + 1)
            {
                g.FillRectangle(blockBrush, x, 1, blockWidth - blockGap, Height - 2);
                x += blockWidth;
            }
        }
        else
        {
            // Continuous
            using var fillBrush = new SolidBrush(ForeColor);
            g.FillRectangle(fillBrush, 1, 1, fillWidth, Height - 2);
        }

        base.OnPaint(e);
    }
}

public enum ProgressBarStyle { Blocks, Continuous, Marquee }
