namespace Canvas.Windows.Forms.Drawing;

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

    public static implicit operator System.Drawing.Point(Point p) => new System.Drawing.Point(p.X, p.Y);
    public static implicit operator Point(System.Drawing.Point p) => new Point(p.X, p.Y);

    public static bool operator ==(Point left, Point right) => left.X == right.X && left.Y == right.Y;
    public static bool operator !=(Point left, Point right) => !(left == right);
    public override bool Equals(object? obj) => obj is Point p && this == p;
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"{{X={X},Y={Y}}}";
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

    public static implicit operator System.Drawing.PointF(PointF p) => new System.Drawing.PointF(p.X, p.Y);
    public static implicit operator PointF(System.Drawing.PointF p) => new PointF(p.X, p.Y);

    public static bool operator ==(PointF left, PointF right) => left.X == right.X && left.Y == right.Y;
    public static bool operator !=(PointF left, PointF right) => !(left == right);
    public override bool Equals(object? obj) => obj is PointF p && this == p;
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"{{X={X},Y={Y}}}";
}
