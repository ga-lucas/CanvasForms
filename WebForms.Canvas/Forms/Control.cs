using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public abstract class Control
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; } = 300;
    public int Height { get; set; } = 200;
    public Color BackColor { get; set; } = Color.White;
    public bool Visible { get; set; } = true;

    public event PaintEventHandler? Paint;

    protected internal virtual void OnPaint(PaintEventArgs e)
    {
        Paint?.Invoke(this, e);
    }

    public void Invalidate()
    {
        RequestRender?.Invoke();
    }

    internal Func<Task>? RequestRender { get; set; }
}
