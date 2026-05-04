namespace Canvas.Windows.Forms.Drawing;

public abstract class Brush : IDisposable
{
    public void Dispose()
    {
        // For future resource management
    }
}

public class SolidBrush : Brush
{
    public Color Color { get; set; }

    public SolidBrush(Color color)
    {
        Color = color;
    }
}

/// <summary>
/// Brush that fills with a linear colour gradient between two points.
/// Matches the WinForms <c>System.Drawing.Drawing2D.LinearGradientBrush</c> API surface.
/// </summary>
public class LinearGradientBrush : Brush
{
    public Point Point1 { get; }
    public Point Point2 { get; }
    public Color Color1 { get; set; }
    public Color Color2 { get; set; }

    // WinForms-compatible: LinearGradientBrush(Point, Point, Color, Color)
    public LinearGradientBrush(Point point1, Point point2, Color color1, Color color2)
    {
        Point1 = point1;
        Point2 = point2;
        Color1 = color1;
        Color2 = color2;
    }

    // Convenience: fill a rectangle horizontally (left → right)
    public LinearGradientBrush(Rectangle rect, Color color1, Color color2, LinearGradientMode mode = LinearGradientMode.Horizontal)
    {
        Color1 = color1;
        Color2 = color2;
        switch (mode)
        {
            case LinearGradientMode.Vertical:
                Point1 = new Point(rect.X, rect.Y);
                Point2 = new Point(rect.X, rect.Bottom);
                break;
            case LinearGradientMode.ForwardDiagonal:
                Point1 = new Point(rect.X, rect.Y);
                Point2 = new Point(rect.Right, rect.Bottom);
                break;
            case LinearGradientMode.BackwardDiagonal:
                Point1 = new Point(rect.Right, rect.Y);
                Point2 = new Point(rect.X, rect.Bottom);
                break;
            default: // Horizontal
                Point1 = new Point(rect.X, rect.Y);
                Point2 = new Point(rect.Right, rect.Y);
                break;
        }
    }

    /// <summary>Optional additional colour stops as (offset 0..1, color) pairs.</summary>
    public List<(float Offset, Color Color)>? InterpolationColors { get; set; }
}

public enum LinearGradientMode
{
    Horizontal = 0,
    Vertical   = 1,
    ForwardDiagonal  = 2,
    BackwardDiagonal = 3,
}

/// <summary>
/// Brush that fills with a radial colour gradient emanating from a centre point.
/// Canvas-specific extension (not in real WinForms System.Drawing).
/// </summary>
public class RadialGradientBrush : Brush
{
    public Point Center { get; set; }
    public float Radius { get; set; }
    public Color CenterColor { get; set; }
    public Color SurroundColor { get; set; }

    public RadialGradientBrush(Point center, float radius, Color centerColor, Color surroundColor)
    {
        Center       = center;
        Radius       = radius;
        CenterColor  = centerColor;
        SurroundColor = surroundColor;
    }
}
