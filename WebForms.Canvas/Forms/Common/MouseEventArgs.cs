namespace WebForms.Canvas.Forms;

public enum MouseButtons
{
    None = 0,
    Left = 1048576,
    Right = 2097152,
    Middle = 4194304
}

public class MouseEventArgs : EventArgs
{
    public int X { get; }
    public int Y { get; }
    public MouseButtons Button { get; }
    public int Clicks { get; }
    public int Delta { get; }

    public MouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta = 0)
    {
        Button = button;
        Clicks = clicks;
        X = x;
        Y = y;
        Delta = delta;
    }

    public Drawing.Point Location => new(X, Y);
}

public delegate void MouseEventHandler(object? sender, MouseEventArgs e);
