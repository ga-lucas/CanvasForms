namespace WebForms.Canvas.Drawing;

public struct Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Point Empty => new(0, 0);

    public bool IsEmpty => X == 0 && Y == 0;
}

public struct PointF
{
    public float X { get; set; }
    public float Y { get; set; }

    public PointF(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static PointF Empty => new(0, 0);
}
