namespace Canvas.Windows.Forms.Drawing;

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

    // Equality
    public bool Equals(Color other)
    {
        return A == other.A && R == other.R && G == other.G && B == other.B;
    }

    public override bool Equals(object? obj)
    {
        return obj is Color other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(A, R, G, B);
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }

    // Common colors (standard .NET System.Drawing.Color named colors)
    public static Color Transparent => FromArgb(0, 0, 0, 0);
    public static Color Black => FromArgb(0, 0, 0);
    public static Color White => FromArgb(255, 255, 255);

    // Primary colors
    public static Color Red => FromArgb(255, 0, 0);
    public static Color Green => FromArgb(0, 128, 0);  // Web color Green
    public static Color Blue => FromArgb(0, 0, 255);
    public static Color Yellow => FromArgb(255, 255, 0);
    public static Color Cyan => FromArgb(0, 255, 255);
    public static Color Magenta => FromArgb(255, 0, 255);

    // Grays
    public static Color Gray => FromArgb(128, 128, 128);
    public static Color DarkGray => FromArgb(169, 169, 169);
    public static Color LightGray => FromArgb(211, 211, 211);
    public static Color DimGray => FromArgb(105, 105, 105);
    public static Color SlateGray => FromArgb(112, 128, 144);
    public static Color DarkSlateGray => FromArgb(47, 79, 79);
    public static Color LightSlateGray => FromArgb(119, 136, 153);

    // Common web/UI colors
    public static Color Orange => FromArgb(255, 165, 0);
    public static Color Purple => FromArgb(128, 0, 128);
    public static Color Brown => FromArgb(165, 42, 42);
    public static Color Pink => FromArgb(255, 192, 203);
    public static Color Lime => FromArgb(0, 255, 0);
    public static Color Navy => FromArgb(0, 0, 128);
    public static Color Teal => FromArgb(0, 128, 128);
    public static Color Olive => FromArgb(128, 128, 0);
    public static Color Maroon => FromArgb(128, 0, 0);
    public static Color Silver => FromArgb(192, 192, 192);
    public static Color Aqua => FromArgb(0, 255, 255);
    public static Color Fuchsia => FromArgb(255, 0, 255);

    public static implicit operator Color(System.Drawing.Color c) => FromArgb(c.A, c.R, c.G, c.B);
    public static implicit operator System.Drawing.Color(Color c) => System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
}
