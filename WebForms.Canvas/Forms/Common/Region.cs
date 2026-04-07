using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class Region
{
    public Rectangle Bounds { get; set; }

    public Region()
    {
        Bounds = Rectangle.Empty;
    }

    public Region(Rectangle bounds)
    {
        Bounds = bounds;
    }

    public bool IsEmpty => Bounds.IsEmpty;
}
