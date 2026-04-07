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
