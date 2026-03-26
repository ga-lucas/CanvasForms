namespace WebForms.Canvas.Drawing;

public struct Rectangle
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public Rectangle(Point location, Size size)
    {
        X = location.X;
        Y = location.Y;
        Width = size.Width;
        Height = size.Height;
    }

    public int Left => X;
    public int Top => Y;
    public int Right => X + Width;
    public int Bottom => Y + Height;
    public Point Location => new(X, Y);
    public Size Size => new(Width, Height);

    public static Rectangle Empty => new(0, 0, 0, 0);

    public bool Contains(Point point)
    {
        return point.X >= X && point.X < X + Width &&
               point.Y >= Y && point.Y < Y + Height;
    }

    public bool Contains(int x, int y)
    {
        return x >= X && x < X + Width &&
               y >= Y && y < Y + Height;
    }
}

public struct RectangleF
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public RectangleF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;
}
