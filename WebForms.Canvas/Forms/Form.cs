using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class Form : Control
{
    public string Text { get; set; } = "Form";

    public Form()
    {
        Width = 800;
        Height = 600;
        BackColor = Color.FromArgb(240, 240, 240);
    }

    public Graphics CreateGraphics()
    {
        return new Graphics(Width, Height);
    }

    public void Show()
    {
        Visible = true;
        Invalidate();
    }

    public void Close()
    {
        Visible = false;
    }
}
