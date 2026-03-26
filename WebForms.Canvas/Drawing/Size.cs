namespace WebForms.Canvas.Drawing;

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
