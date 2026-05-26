namespace System.Drawing.Imaging;

/// <summary>Pixel format constants (stub subset).</summary>
public enum PixelFormat
{
    Undefined         = 0,
    Format1bppIndexed = 196865,
    Format4bppIndexed = 197634,
    Format8bppIndexed = 198659,
    Format16bppGrayScale = 1052676,
    Format16bppRgb555 = 135173,
    Format16bppRgb565 = 135174,
    Format16bppArgb1555 = 397319,
    Format24bppRgb    = 137224,
    Format32bppRgb    = 139273,
    Format32bppArgb   = 2498570,
    Format32bppPArgb  = 925707,
    Format48bppRgb    = 1060876,
    Format64bppArgb   = 3424269,
    Format64bppPArgb  = 29622286,
}

/// <summary>Lock mode flags for <see cref="System.Drawing.Bitmap.LockBits"/>.</summary>
[Flags]
public enum ImageLockMode
{
    ReadOnly  = 1,
    WriteOnly = 2,
    ReadWrite = ReadOnly | WriteOnly,
    UserInputBuffer = 4,
}

/// <summary>Stub for bitmap data returned by LockBits.</summary>
public sealed class BitmapData
{
    public int Width      { get; set; }
    public int Height     { get; set; }
    public int Stride     { get; set; }
    public IntPtr Scan0   { get; set; }
    public PixelFormat PixelFormat { get; set; }
    public int Reserved  { get; set; }
}

/// <summary>Image format identifiers (stub).</summary>
public sealed class ImageFormat
{
    public static readonly ImageFormat Bmp  = new("Bmp");
    public static readonly ImageFormat Png  = new("Png");
    public static readonly ImageFormat Jpeg = new("Jpeg");
    public static readonly ImageFormat Gif  = new("Gif");
    public static readonly ImageFormat Tiff = new("Tiff");
    public static readonly ImageFormat Icon = new("Icon");
    public static readonly ImageFormat Emf  = new("Emf");
    public static readonly ImageFormat Wmf  = new("Wmf");

    public string Name { get; }
    private ImageFormat(string name) => Name = name;
    public override string ToString() => Name;
}
