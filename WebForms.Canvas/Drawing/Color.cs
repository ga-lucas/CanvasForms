namespace WebForms.Canvas.Drawing;

public struct Color
{
    public byte A { get; }
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    private Color(byte a, byte r, byte g, byte b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public static Color FromArgb(int argb)
    {
        byte a = (byte)((argb >> 24) & 0xFF);
        byte r = (byte)((argb >> 16) & 0xFF);
        byte g = (byte)((argb >> 8) & 0xFF);
        byte b = (byte)(argb & 0xFF);
        return new Color(a, r, g, b);
    }

    public static Color FromArgb(int alpha, int red, int green, int blue)
    {
        return new Color((byte)alpha, (byte)red, (byte)green, (byte)blue);
    }

    public static Color FromArgb(int red, int green, int blue)
    {
        return new Color(255, (byte)red, (byte)green, (byte)blue);
    }

    public string ToRgbaString()
    {
        return $"rgba({R},{G},{B},{A / 255.0})";
    }

    public string ToHexString()
    {
        return $"#{R:X2}{G:X2}{B:X2}";
    }

    // Common colors
    public static Color Black => FromArgb(0, 0, 0);
    public static Color White => FromArgb(255, 255, 255);
    public static Color Red => FromArgb(255, 0, 0);
    public static Color Green => FromArgb(0, 255, 0);
    public static Color Blue => FromArgb(0, 0, 255);
    public static Color Yellow => FromArgb(255, 255, 0);
    public static Color Transparent => FromArgb(0, 0, 0, 0);
}
