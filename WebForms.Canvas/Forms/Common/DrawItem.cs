using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

[Flags]
public enum DrawItemState
{
    None = 0,
    Selected = 1,
    Grayed = 2,
    Disabled = 4,
    Checked = 8,
    Focus = 16,
    Default = 32,
    HotLight = 64,
    Inactive = 128,
    NoAccelerator = 256,
    NoFocusRect = 512,
    ComboBoxEdit = 4096,
}

public delegate void DrawItemEventHandler(object? sender, DrawItemEventArgs e);

public class DrawItemEventArgs : EventArgs
{
    public DrawItemEventArgs(Graphics graphics, Font font, Rectangle bounds, int index, DrawItemState state)
    {
        Graphics = graphics;
        Font = font;
        Bounds = bounds;
        Index = index;
        State = state;

        ForeColor = Color.Black;
        BackColor = Color.White;
    }

    public Graphics Graphics { get; }
    public Font Font { get; }
    public Rectangle Bounds { get; }
    public int Index { get; }
    public DrawItemState State { get; }

    public Color ForeColor { get; set; }
    public Color BackColor { get; set; }

    public void DrawBackground()
    {
        using var b = new SolidBrush(BackColor);
        Graphics.FillRectangle(b, Bounds);
    }

    public void DrawFocusRectangle()
    {
        using var p = new Pen(Color.Black);
        Graphics.DrawRectangle(p, Bounds);
    }
}
