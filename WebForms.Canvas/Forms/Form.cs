using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class Form : Control
{
    private static int _nextZIndex = 1;
    private const int TitleBarHeight = 32; // Height of the title bar

    public string Text { get; set; } = "Form";
    public bool AllowResize { get; set; } = true;
    public bool AllowMove { get; set; } = true;
    public int MinimumWidth { get; set; } = 100;
    public int MinimumHeight { get; set; } = 50;
    public int MaximumWidth { get; set; } = 0; // 0 = no limit
    public int MaximumHeight { get; set; } = 0; // 0 = no limit

    // Position on screen (for dragging)
    public int Left { get; set; } = 50;
    public int Top { get; set; } = 50;

    // Z-order for stacking
    public int ZIndex { get; set; } = 0;

    // Client area dimensions (excluding title bar)
    public int ClientWidth => Width;
    public int ClientHeight => Math.Max(0, Height - TitleBarHeight);

    public Form()
    {
        Width = 800;
        Height = 600;
        BackColor = Color.FromArgb(240, 240, 240);
        ZIndex = _nextZIndex++;
    }

    public void BringToFront()
    {
        ZIndex = _nextZIndex++;
        Invalidate();
    }

    public Graphics CreateGraphics()
    {
        return new Graphics(ClientWidth, ClientHeight);
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
