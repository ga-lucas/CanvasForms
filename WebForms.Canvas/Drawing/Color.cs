namespace Canvas.Windows.Forms.Drawing;

public struct Color
{
    public byte A { get; }
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    // True only for the Empty sentinel — matches System.Drawing.Color.IsEmpty semantics.
    private readonly bool _isEmpty;

    private Color(byte a, byte r, byte g, byte b, bool isEmpty = false)
    {
        A = a;
        R = r;
        G = g;
        B = b;
        _isEmpty = isEmpty;
    }

    /// <summary>
    /// Gets a value indicating whether this Color structure is uninitialized / unset.
    /// Matches System.Drawing.Color.IsEmpty — distinct from Color.Transparent.
    /// </summary>
    public bool IsEmpty => _isEmpty;

    /// <summary>
    /// Represents an unset color. Equivalent to System.Drawing.Color.Empty.
    /// </summary>
    public static Color Empty => new Color(0, 0, 0, 0, isEmpty: true);

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

    // Equality — _isEmpty is part of identity (Empty != Transparent)
    public bool Equals(Color other)
    {
        return _isEmpty == other._isEmpty && A == other.A && R == other.R && G == other.G && B == other.B;
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
    public static Color Orange     => FromArgb(255, 165,   0);
    public static Color Purple     => FromArgb(128,   0, 128);
    public static Color Brown      => FromArgb(165,  42,  42);
    public static Color Pink       => FromArgb(255, 192, 203);
    public static Color Lime       => FromArgb(  0, 255,   0);
    public static Color Navy       => FromArgb(  0,   0, 128);
    public static Color Teal       => FromArgb(  0, 128, 128);
    public static Color Olive      => FromArgb(128, 128,   0);
    public static Color Maroon     => FromArgb(128,   0,   0);
    public static Color Silver     => FromArgb(192, 192, 192);
    public static Color Aqua       => FromArgb(  0, 255, 255);
    public static Color Fuchsia    => FromArgb(255,   0, 255);
    // Extended named colors
    public static Color DarkBlue   => FromArgb(  0,   0, 139);
    public static Color DarkRed    => FromArgb(139,   0,   0);
    public static Color DarkGreen  => FromArgb(  0, 100,   0);
    public static Color DarkOrange => FromArgb(255, 140,   0);
    public static Color DeepPink   => FromArgb(255,  20, 147);
    public static Color OrangeRed  => FromArgb(255,  69,   0);
    public static Color Coral      => FromArgb(255, 127,  80);
    public static Color Gold       => FromArgb(255, 215,   0);
    public static Color Indigo     => FromArgb( 75,   0, 130);
    public static Color Violet     => FromArgb(238, 130, 238);
    public static Color SkyBlue    => FromArgb(135, 206, 235);
    public static Color Crimson    => FromArgb(220,  20,  60);
    public static Color LightBlue  => FromArgb(173, 216, 230);
    public static Color LightGreen => FromArgb(144, 238, 144);
    public static Color LightYellow=> FromArgb(255, 255, 224);

    /// <summary>Returns the ARGB value as a 32-bit integer (AARRGGBB).</summary>
    public int ToArgb() => (A << 24) | (R << 16) | (G << 8) | B;

    /// <summary>Returns a human-readable name for this color, or the hex string if unnamed.</summary>
    public string Name => _namedColorLookup.TryGetValue(ToArgb() & 0xFFFFFF, out var name) ? name : ToHexString();

    /// <summary>Returns true if this color has a well-known name in the WinForms color table.</summary>
    public bool IsNamedColor => _namedColorLookup.ContainsKey(ToArgb() & 0xFFFFFF);

    /// <summary>Returns true for all fully-opaque named colors (matches WinForms KnownColor semantics).</summary>
    public bool IsKnownColor => IsNamedColor && A == 255;

    /// <summary>Returns true if the alpha channel is fully transparent (A == 0).</summary>
    public bool IsTransparent => A == 0;

    public override string ToString() => IsEmpty ? "Color [Empty]" : $"Color [{Name}]";

    /// <summary>
    /// Creates a <see cref="Color"/> from a named color string (e.g. "Red", "AliceBlue") or
    /// an HTML hex string (e.g. "#FF0000"). Returns <see cref="Empty"/> for unrecognised names.
    /// </summary>
    public static Color FromName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Empty;
        // Try named lookup first (case-insensitive)
        foreach (var kv in _namedColorLookup)
        {
            if (string.Equals(kv.Value, name, StringComparison.OrdinalIgnoreCase))
                return FromArgb(255, (kv.Key >> 16) & 0xFF, (kv.Key >> 8) & 0xFF, kv.Key & 0xFF);
        }
        // Try HTML hex (#RRGGBB / #RGB)
        if (name.StartsWith('#'))
        {
            var hex = name.TrimStart('#');
            if (hex.Length == 6 && int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int rgb))
                return FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
            if (hex.Length == 3)
            {
                var r = Convert.ToByte(new string(hex[0], 2), 16);
                var g = Convert.ToByte(new string(hex[1], 2), 16);
                var b = Convert.ToByte(new string(hex[2], 2), 16);
                return FromArgb(255, r, g, b);
            }
        }
        return Empty;
    }

    // Lookup table: 0xRRGGBB -> name
    private static readonly Dictionary<int, string> _namedColorLookup = new()
    {
        { 0xF0F8FF, "AliceBlue" },       { 0xFAEBD7, "AntiqueWhite" },
        { 0x00FFFF, "Aqua" },            { 0x7FFFD4, "Aquamarine" },
        { 0xF0FFFF, "Azure" },           { 0xF5F5DC, "Beige" },
        { 0xFFE4C4, "Bisque" },          { 0x000000, "Black" },
        { 0xFFEBCD, "BlanchedAlmond" },  { 0x0000FF, "Blue" },
        { 0x8A2BE2, "BlueViolet" },      { 0xA52A2A, "Brown" },
        { 0xDEB887, "BurlyWood" },       { 0x5F9EA0, "CadetBlue" },
        { 0x7FFF00, "Chartreuse" },      { 0xD2691E, "Chocolate" },
        { 0xFF7F50, "Coral" },           { 0x6495ED, "CornflowerBlue" },
        { 0xFFF8DC, "Cornsilk" },        { 0xDC143C, "Crimson" },
        { 0x00008B, "DarkBlue" },        { 0x008B8B, "DarkCyan" },
        { 0xB8860B, "DarkGoldenrod" },   { 0xA9A9A9, "DarkGray" },
        { 0x006400, "DarkGreen" },       { 0xBDB76B, "DarkKhaki" },
        { 0x8B008B, "DarkMagenta" },     { 0x556B2F, "DarkOliveGreen" },
        { 0xFF8C00, "DarkOrange" },      { 0x9932CC, "DarkOrchid" },
        { 0x8B0000, "DarkRed" },         { 0xE9967A, "DarkSalmon" },
        { 0x8FBC8F, "DarkSeaGreen" },    { 0x483D8B, "DarkSlateBlue" },
        { 0x2F4F4F, "DarkSlateGray" },   { 0x00CED1, "DarkTurquoise" },
        { 0x9400D3, "DarkViolet" },      { 0xFF1493, "DeepPink" },
        { 0x00BFFF, "DeepSkyBlue" },     { 0x696969, "DimGray" },
        { 0x1E90FF, "DodgerBlue" },      { 0xB22222, "Firebrick" },
        { 0xFFFAF0, "FloralWhite" },     { 0x228B22, "ForestGreen" },
        { 0xFF00FF, "Fuchsia" },         { 0xDCDCDC, "Gainsboro" },
        { 0xF8F8FF, "GhostWhite" },      { 0xFFD700, "Gold" },
        { 0xDAA520, "Goldenrod" },       { 0x808080, "Gray" },
        { 0x008000, "Green" },           { 0xADFF2F, "GreenYellow" },
        { 0xF0FFF0, "Honeydew" },        { 0xFF69B4, "HotPink" },
        { 0xCD5C5C, "IndianRed" },       { 0x4B0082, "Indigo" },
        { 0xFFFFF0, "Ivory" },           { 0xF0E68C, "Khaki" },
        { 0xE6E6FA, "Lavender" },        { 0xFFF0F5, "LavenderBlush" },
        { 0x7CFC00, "LawnGreen" },       { 0xFFFACD, "LemonChiffon" },
        { 0xADD8E6, "LightBlue" },       { 0xF08080, "LightCoral" },
        { 0xE0FFFF, "LightCyan" },       { 0xFAFAD2, "LightGoldenrodYellow" },
        { 0xD3D3D3, "LightGray" },       { 0x90EE90, "LightGreen" },
        { 0xFFB6C1, "LightPink" },       { 0xFFA07A, "LightSalmon" },
        { 0x20B2AA, "LightSeaGreen" },   { 0x87CEFA, "LightSkyBlue" },
        { 0x778899, "LightSlateGray" },  { 0xB0C4DE, "LightSteelBlue" },
        { 0xFFFFE0, "LightYellow" },     { 0x00FF00, "Lime" },
        { 0x32CD32, "LimeGreen" },       { 0xFAF0E6, "Linen" },
        { 0xFF00FF, "Magenta" },         { 0x800000, "Maroon" },
        { 0x66CDAA, "MediumAquamarine" },{ 0x0000CD, "MediumBlue" },
        { 0xBA55D3, "MediumOrchid" },    { 0x9370DB, "MediumPurple" },
        { 0x3CB371, "MediumSeaGreen" },  { 0x7B68EE, "MediumSlateBlue" },
        { 0x00FA9A, "MediumSpringGreen" },{ 0x48D1CC, "MediumTurquoise" },
        { 0xC71585, "MediumVioletRed" }, { 0x191970, "MidnightBlue" },
        { 0xF5FFFA, "MintCream" },       { 0xFFE4E1, "MistyRose" },
        { 0xFFE4B5, "Moccasin" },        { 0xFFDEAD, "NavajoWhite" },
        { 0x000080, "Navy" },            { 0xFDF5E6, "OldLace" },
        { 0x808000, "Olive" },           { 0x6B8E23, "OliveDrab" },
        { 0xFFA500, "Orange" },          { 0xFF4500, "OrangeRed" },
        { 0xDA70D6, "Orchid" },          { 0xEEE8AA, "PaleGoldenrod" },
        { 0x98FB98, "PaleGreen" },       { 0xAFEEEE, "PaleTurquoise" },
        { 0xDB7093, "PaleVioletRed" },   { 0xFFEFD5, "PapayaWhip" },
        { 0xFFDAB9, "PeachPuff" },       { 0xCD853F, "Peru" },
        { 0xFFC0CB, "Pink" },            { 0xDDA0DD, "Plum" },
        { 0xB0E0E6, "PowderBlue" },      { 0x800080, "Purple" },
        { 0xFF0000, "Red" },             { 0xBC8F8F, "RosyBrown" },
        { 0x4169E1, "RoyalBlue" },       { 0x8B4513, "SaddleBrown" },
        { 0xFA8072, "Salmon" },          { 0xF4A460, "SandyBrown" },
        { 0x2E8B57, "SeaGreen" },        { 0xFFF5EE, "SeaShell" },
        { 0xA0522D, "Sienna" },          { 0xC0C0C0, "Silver" },
        { 0x87CEEB, "SkyBlue" },         { 0x6A5ACD, "SlateBlue" },
        { 0x708090, "SlateGray" },       { 0xFFFAFA, "Snow" },
        { 0x00FF7F, "SpringGreen" },     { 0x4682B4, "SteelBlue" },
        { 0xD2B48C, "Tan" },             { 0x008080, "Teal" },
        { 0xD8BFD8, "Thistle" },         { 0xFF6347, "Tomato" },
        { 0x40E0D0, "Turquoise" },       { 0xEE82EE, "Violet" },
        { 0xF5DEB3, "Wheat" },           { 0xFFFFFF, "White" },
        { 0xF5F5F5, "WhiteSmoke" },      { 0xFFFF00, "Yellow" },
        { 0x9ACD32, "YellowGreen" },
    };

    public static implicit operator Color(System.Drawing.Color c)
    {
        if (c.IsEmpty) return Empty;
        return FromArgb(c.A, c.R, c.G, c.B);
    }
    public static implicit operator System.Drawing.Color(Color c)
    {
        if (c._isEmpty) return System.Drawing.Color.Empty;
        return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
    }
}
