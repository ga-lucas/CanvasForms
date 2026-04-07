namespace Canvas.Windows.Forms.Drawing;

public struct Size
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Size(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static Size Empty => new(0, 0);

    public bool IsEmpty => Width == 0 && Height == 0;

    public static bool operator ==(Size left, Size right) => left.Width == right.Width && left.Height == right.Height;
    public static bool operator !=(Size left, Size right) => !(left == right);

    public override bool Equals(object? obj) => obj is Size size && this == size;
    public override int GetHashCode() => HashCode.Combine(Width, Height);
}

public struct SizeF
{
    public float Width { get; set; }
    public float Height { get; set; }

    public SizeF(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public static SizeF Empty => new(0, 0);
}
