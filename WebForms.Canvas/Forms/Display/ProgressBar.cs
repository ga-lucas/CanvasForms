
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
    private int _marqueeAnimationSpeed = 100;
    private bool _rightToLeftLayout = false;
    private int _marqueePos = 0;
    private System.Threading.Timer? _marqueeTimer;

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

    public bool RightToLeftLayout
    {
        get => _rightToLeftLayout;
        set { _rightToLeftLayout = value; Invalidate(); }
    }

    public int MarqueeAnimationSpeed
    {
        get => _marqueeAnimationSpeed;
        set
        {
            _marqueeAnimationSpeed = Math.Max(0, value);
            if (_style == ProgressBarStyle.Marquee) RestartMarqueeTimer();
        }
    }

    public ProgressBarStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            if (_style == ProgressBarStyle.Marquee)
                StartMarqueeTimer();
            else
                StopMarqueeTimer();
            Invalidate();
        }
    }

    private void StartMarqueeTimer()
    {
        StopMarqueeTimer();
        if (_marqueeAnimationSpeed <= 0) return;
        _marqueeTimer = new System.Threading.Timer(_ =>
        {
            _marqueePos = (_marqueePos + 4) % Math.Max(1, Width);
            Invalidate();
        }, null, 0, _marqueeAnimationSpeed);
    }

    private void StopMarqueeTimer()
    {
        _marqueeTimer?.Dispose();
        _marqueeTimer = null;
    }

    private void RestartMarqueeTimer() { StopMarqueeTimer(); StartMarqueeTimer(); }

    public void PerformStep() => Value = Math.Min(_maximum, _value + _step);

    public void Increment(int value) => Value = Math.Min(_maximum, _value + value);

    private const int TrackRadius = 3;
    private const int FillRadius  = 2;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var trackRect = new Rectangle(0, 0, Width, Height);
        var innerRect  = new Rectangle(1, 1, Width - 2, Height - 2);

        // Rounded track background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRoundRect(bgBrush, trackRect, TrackRadius);

        // Rounded border
        using var borderPen = new Pen(Color.FromArgb(188, 188, 188));
        g.DrawRoundRect(borderPen, trackRect, TrackRadius);

        if (_maximum <= _minimum && _style != ProgressBarStyle.Marquee) { base.OnPaint(e); return; }

        double fraction = (_maximum > _minimum)
            ? (double)(_value - _minimum) / (_maximum - _minimum)
            : 0;
        int fillWidth = (int)(fraction * innerRect.Width);

        // Build gradient colors: lighter highlight on top, base color on bottom
        var fillTop    = LightenColor(ForeColor, 0.25f);
        var fillBottom = DarkenColor(ForeColor, 0.10f);

        if (_style == ProgressBarStyle.Marquee)
        {
            int segW  = Math.Max(20, Width / 4);
            int pos   = _marqueePos % Math.Max(1, Width + segW);
            int drawX = _rightToLeftLayout ? (Width - 1 - pos - segW) : (1 + pos);
            drawX = Math.Max(1, Math.Min(Width - 2, drawX));
            int drawW = Math.Min(segW, Width - 2 - (drawX - 1));
            if (drawW > 0)
            {
                var fillRect = new Rectangle(drawX, 1, drawW, innerRect.Height);
                using var marqueeBrush = new LinearGradientBrush(
                    new Point(fillRect.Left, fillRect.Top),
                    new Point(fillRect.Left, fillRect.Bottom),
                    fillTop, fillBottom);
                g.FillRoundRect(marqueeBrush, fillRect, FillRadius);
            }
        }
        else if (fillWidth > 0)
        {
            int startX = _rightToLeftLayout ? (Width - 1 - fillWidth) : 1;
            if (_style == ProgressBarStyle.Blocks)
            {
                const int blockWidth = 10;
                const int blockGap   = 2;
                int x = startX;
                while (_rightToLeftLayout ? x + blockWidth - blockGap <= startX + fillWidth
                                          : x + blockWidth <= startX + fillWidth + 1)
                {
                    var blockRect = new Rectangle(x, 1, blockWidth - blockGap, innerRect.Height);
                    using var blockBrush = new LinearGradientBrush(
                        new Point(blockRect.Left, blockRect.Top),
                        new Point(blockRect.Left, blockRect.Bottom),
                        fillTop, fillBottom);
                    g.FillRoundRect(blockBrush, blockRect, FillRadius);
                    x += blockWidth;
                }
            }
            else
            {
                var fillRect = new Rectangle(startX, 1, fillWidth, innerRect.Height);
                using var fillBrush = new LinearGradientBrush(
                    new Point(fillRect.Left, fillRect.Top),
                    new Point(fillRect.Left, fillRect.Bottom),
                    fillTop, fillBottom);
                g.FillRoundRect(fillBrush, fillRect, FillRadius);
            }
        }

        base.OnPaint(e);
    }

    private static System.Drawing.Color LightenColor(System.Drawing.Color c, float amount)
    {
        return System.Drawing.Color.FromArgb(c.A,
            Math.Min(255, (int)(c.R + (255 - c.R) * amount)),
            Math.Min(255, (int)(c.G + (255 - c.G) * amount)),
            Math.Min(255, (int)(c.B + (255 - c.B) * amount)));
    }

    private static System.Drawing.Color DarkenColor(System.Drawing.Color c, float amount)
    {
        return System.Drawing.Color.FromArgb(c.A,
            Math.Max(0, (int)(c.R * (1f - amount))),
            Math.Max(0, (int)(c.G * (1f - amount))),
            Math.Max(0, (int)(c.B * (1f - amount))));
    }
}

public enum ProgressBarStyle { Blocks, Continuous, Marquee }
