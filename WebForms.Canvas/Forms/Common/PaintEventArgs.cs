using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class PaintEventArgs : EventArgs
{
    public Graphics Graphics { get; }
    public Rectangle ClipRectangle { get; }

    public PaintEventArgs(Graphics graphics, Rectangle clipRectangle)
    {
        Graphics = graphics;
        ClipRectangle = clipRectangle;
    }
}

public delegate void PaintEventHandler(object sender, PaintEventArgs e);
