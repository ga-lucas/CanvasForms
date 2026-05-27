namespace Canvas.Windows.Forms.Drawing;

/// <summary>
/// Specifies the style of dashed lines drawn with a <see cref="Pen"/>.
/// Matches <c>System.Drawing.Drawing2D.DashStyle</c>.
/// </summary>
public enum DashStyle
{
    Solid      = 0,
    Dash       = 1,
    Dot        = 2,
    DashDot    = 3,
    DashDotDot = 4,
    Custom     = 5,
}

public class Pen : IDisposable
{
    public Color Color { get; set; }
    public float Width { get; set; }
    public DashStyle DashStyle { get; set; } = DashStyle.Solid;

    /// <summary>Custom dash pattern used when <see cref="DashStyle"/> is <see cref="DashStyle.Custom"/>.</summary>
    public float[]? DashPattern { get; set; }
    public float DashOffset { get; set; } = 0f;
    public System.Drawing.Drawing2D.LineJoin LineJoin { get; set; } = System.Drawing.Drawing2D.LineJoin.Miter;
    public System.Drawing.Drawing2D.LineCap StartCap { get; set; } = System.Drawing.Drawing2D.LineCap.Flat;
    public System.Drawing.Drawing2D.LineCap EndCap { get; set; } = System.Drawing.Drawing2D.LineCap.Flat;
    public float MiterLimit { get; set; } = 10f;
    public System.Drawing.Drawing2D.PenAlignment Alignment { get; set; } = System.Drawing.Drawing2D.PenAlignment.Center;
    public Brush? Brush { get; set; }

    public Pen(Color color) : this(color, 1.0f) { }

    public Pen(Color color, float width)
    {
        Color = color;
        Width = width;
    }

    public void Dispose()
    {
        // For future resource management
    }
}
