namespace WebForms.Canvas.Drawing;

public class Pen : IDisposable
{
    public Color Color { get; set; }
    public float Width { get; set; }

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
