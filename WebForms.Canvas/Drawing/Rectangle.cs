namespace Canvas.Windows.Forms.Drawing;

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

    public int Left   => X;
    public int Top    => Y;
    public int Right  => X + Width;
    public int Bottom => Y + Height;
    public Point Location => new(X, Y);
    public Size  Size     => new(Width, Height);

    public static Rectangle Empty => new(0, 0, 0, 0);
    public bool IsEmpty => Width == 0 && Height == 0;

    // ── Contains overloads ────────────────────────────────────────

    public bool Contains(Point point)   => Contains(point.X, point.Y);
    public bool Contains(int x, int y)  => x >= X && x < X + Width && y >= Y && y < Y + Height;
    public bool Contains(Rectangle rect)
        => rect.X >= X && rect.Right <= Right && rect.Y >= Y && rect.Bottom <= Bottom;

    // ── Intersection / Union ──────────────────────────────────────

    /// <summary>Returns true when this rectangle intersects with <paramref name="rect"/>.</summary>
    public bool IntersectsWith(Rectangle rect)
        => rect.X < Right && rect.Right > X && rect.Y < Bottom && rect.Bottom > Y;

    /// <summary>Returns the intersection of two rectangles; Empty if they don't intersect.</summary>
    public static Rectangle Intersect(Rectangle a, Rectangle b)
    {
        int x1 = Math.Max(a.X, b.X);
        int y1 = Math.Max(a.Y, b.Y);
        int x2 = Math.Min(a.Right, b.Right);
        int y2 = Math.Min(a.Bottom, b.Bottom);
        return x2 > x1 && y2 > y1 ? new Rectangle(x1, y1, x2 - x1, y2 - y1) : Empty;
    }

    /// <summary>Replaces this rectangle with its intersection with <paramref name="rect"/>.</summary>
    public void Intersect(Rectangle rect)
    {
        var r = Intersect(this, rect);
        X = r.X; Y = r.Y; Width = r.Width; Height = r.Height;
    }

    /// <summary>Returns the smallest rectangle that contains both <paramref name="a"/> and <paramref name="b"/>.</summary>
    public static Rectangle Union(Rectangle a, Rectangle b)
    {
        int x1 = Math.Min(a.X, b.X);
        int y1 = Math.Min(a.Y, b.Y);
        int x2 = Math.Max(a.Right, b.Right);
        int y2 = Math.Max(a.Bottom, b.Bottom);
        return new Rectangle(x1, y1, x2 - x1, y2 - y1);
    }

    // ── Inflate ───────────────────────────────────────────────────

    /// <summary>Returns a new rectangle enlarged by <paramref name="width"/> and <paramref name="height"/>.</summary>
    public static Rectangle Inflate(Rectangle rect, int width, int height)
        => new(rect.X - width, rect.Y - height, rect.Width + 2 * width, rect.Height + 2 * height);

    /// <summary>Enlarges this rectangle by the given amounts.</summary>
    public void Inflate(int width, int height)
    {
        X -= width; Y -= height; Width += 2 * width; Height += 2 * height;
    }

    public void Inflate(Size size) => Inflate(size.Width, size.Height);

    // ── Offset ────────────────────────────────────────────────────

    public void Offset(int dx, int dy) { X += dx; Y += dy; }
    public void Offset(Point p) => Offset(p.X, p.Y);

    // ── FromLTRB / Round ──────────────────────────────────────────

    /// <summary>Creates a rectangle from left/top/right/bottom edge coordinates.</summary>
    public static Rectangle FromLTRB(int left, int top, int right, int bottom)
        => new(left, top, right - left, bottom - top);

    /// <summary>Converts a <see cref="RectangleF"/> to a <see cref="Rectangle"/> by rounding.</summary>
    public static Rectangle Round(RectangleF rect)
        => new((int)Math.Round(rect.X), (int)Math.Round(rect.Y),
               (int)Math.Round(rect.Width), (int)Math.Round(rect.Height));

    /// <summary>Converts a <see cref="RectangleF"/> to a <see cref="Rectangle"/> by truncating.</summary>
    public static Rectangle Truncate(RectangleF rect)
        => new((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

    /// <summary>Converts a <see cref="RectangleF"/> to a <see cref="Rectangle"/> by ceiling.</summary>
    public static Rectangle Ceiling(RectangleF rect)
        => new((int)Math.Ceiling(rect.X), (int)Math.Ceiling(rect.Y),
               (int)Math.Ceiling(rect.Width), (int)Math.Ceiling(rect.Height));

    /// <summary>Converts to a <see cref="RectangleF"/>.</summary>
    public RectangleF ToRectangleF() => new(X, Y, Width, Height);

    public static implicit operator RectangleF(Rectangle r) => new(r.X, r.Y, r.Width, r.Height);
    public static implicit operator System.Drawing.Rectangle(Rectangle r) => new System.Drawing.Rectangle(r.X, r.Y, r.Width, r.Height);
    public static implicit operator Rectangle(System.Drawing.Rectangle r) => new Rectangle(r.X, r.Y, r.Width, r.Height);

    public static bool operator ==(Rectangle left, Rectangle right) =>
        left.X == right.X && left.Y == right.Y && left.Width == right.Width && left.Height == right.Height;
    public static bool operator !=(Rectangle left, Rectangle right) => !(left == right);

    public override bool Equals(object? obj) => obj is Rectangle rect && this == rect;
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    public override string ToString() => $"{{X={X},Y={Y},Width={Width},Height={Height}}}";
}

public struct RectangleF
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public RectangleF(float x, float y, float width, float height)
    {
        X = x; Y = y; Width = width; Height = height;
    }

    public float Left   => X;
    public float Top    => Y;
    public float Right  => X + Width;
    public float Bottom => Y + Height;

    public static RectangleF Empty => new(0, 0, 0, 0);
    public bool IsEmpty => Width == 0 && Height == 0;

    public bool Contains(float x, float y) => x >= X && x < X + Width && y >= Y && y < Y + Height;
    public bool Contains(PointF pt) => Contains(pt.X, pt.Y);
    public bool Contains(RectangleF rect)
        => rect.X >= X && rect.Right <= Right && rect.Y >= Y && rect.Bottom <= Bottom;

    public bool IntersectsWith(RectangleF rect)
        => rect.X < Right && rect.Right > X && rect.Y < Bottom && rect.Bottom > Y;

    public static RectangleF Intersect(RectangleF a, RectangleF b)
    {
        float x1 = Math.Max(a.X, b.X), y1 = Math.Max(a.Y, b.Y);
        float x2 = Math.Min(a.Right, b.Right), y2 = Math.Min(a.Bottom, b.Bottom);
        return x2 > x1 && y2 > y1 ? new RectangleF(x1, y1, x2 - x1, y2 - y1) : Empty;
    }

    public static RectangleF Union(RectangleF a, RectangleF b)
    {
        float x1 = Math.Min(a.X, b.X), y1 = Math.Min(a.Y, b.Y);
        float x2 = Math.Max(a.Right, b.Right), y2 = Math.Max(a.Bottom, b.Bottom);
        return new RectangleF(x1, y1, x2 - x1, y2 - y1);
    }

    public void Inflate(float dx, float dy) { X -= dx; Y -= dy; Width += 2 * dx; Height += 2 * dy; }
    public void Offset(float dx, float dy) { X += dx; Y += dy; }
    public void Offset(PointF p) => Offset(p.X, p.Y);

    public static RectangleF FromLTRB(float l, float t, float r, float b) => new(l, t, r - l, b - t);

    public static implicit operator System.Drawing.RectangleF(RectangleF r) => new System.Drawing.RectangleF(r.X, r.Y, r.Width, r.Height);
    public static implicit operator RectangleF(System.Drawing.RectangleF r) => new RectangleF(r.X, r.Y, r.Width, r.Height);

    public static bool operator ==(RectangleF a, RectangleF b)
        => a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
    public static bool operator !=(RectangleF a, RectangleF b) => !(a == b);
    public override bool Equals(object? obj) => obj is RectangleF r && this == r;
    public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);
    public override string ToString() => $"{{X={X},Y={Y},Width={Width},Height={Height}}}";
}

