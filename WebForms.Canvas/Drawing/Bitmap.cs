namespace System.Drawing;

/// <summary>
/// Cross-platform stub for <c>System.Drawing.Bitmap</c>.
/// Real pixel manipulation is Windows-only (GDI+/System.Drawing.Common).
/// This stub exposes the constructor and property surface that translated
/// WinForms apps reference, preventing <see cref="TypeLoadException"/> at startup.
/// Pixel access methods are no-ops or return safe defaults.
/// </summary>
public sealed class Bitmap : IDisposable
{
    /// <summary>Width of the bitmap in pixels.</summary>
    public int Width { get; }

    /// <summary>Height of the bitmap in pixels.</summary>
    public int Height { get; }

    /// <summary>Pixel format (stub — always <see cref="Imaging.PixelFormat.Format32bppArgb"/>).</summary>
    public Imaging.PixelFormat PixelFormat => Imaging.PixelFormat.Format32bppArgb;

    /// <summary>Creates a bitmap of the given size.</summary>
    public Bitmap(int width, int height)
    {
        Width  = width;
        Height = height;
    }

    /// <summary>Creates a bitmap with a specific pixel format (format ignored in stub).</summary>
    public Bitmap(int width, int height, Imaging.PixelFormat format)
    {
        Width  = width;
        Height = height;
    }

    /// <summary>Loads a bitmap from a file path (stub — size 0×0).</summary>
    public Bitmap(string filename)
    {
        Width  = 0;
        Height = 0;
    }

    /// <summary>Loads a bitmap from a stream (stub — size 0×0).</summary>
    public Bitmap(System.IO.Stream stream)
    {
        Width  = 0;
        Height = 0;
    }

    /// <summary>Creates a bitmap copy of another <see cref="Bitmap"/> (stub).</summary>
    public Bitmap(Bitmap original)
    {
        Width  = original?.Width ?? 0;
        Height = original?.Height ?? 0;
    }

    /// <summary>Creates a bitmap from an <see cref="System.Drawing.Image"/> (stub).</summary>
    public Bitmap(Image image)
    {
        Width  = image?.Width ?? 0;
        Height = image?.Height ?? 0;
    }

    // ── Pixel access (stubs) ──────────────────────────────────────────────────

    /// <summary>Returns <see cref="Color.Transparent"/> — pixel read is not supported in the canvas host.</summary>
    public Color GetPixel(int x, int y) => Color.Transparent;

    /// <summary>No-op — pixel write is not supported in the canvas host.</summary>
    public void SetPixel(int x, int y, Color color) { }

    /// <summary>
    /// Locks a region of the bitmap for direct pixel access (stub).
    /// Returns a zeroed <see cref="Imaging.BitmapData"/>.
    /// </summary>
    public Imaging.BitmapData LockBits(Rectangle rect, Imaging.ImageLockMode flags, Imaging.PixelFormat format)
        => new Imaging.BitmapData();

    /// <summary>No-op unlock (stub).</summary>
    public void UnlockBits(Imaging.BitmapData data) { }

    // ── Serialisation stubs ───────────────────────────────────────────────────

    /// <summary>No-op save stub.</summary>
    public void Save(string filename) { }

    /// <summary>No-op save stub.</summary>
    public void Save(System.IO.Stream stream, Imaging.ImageFormat format) { }

    // ── Graphics factory ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns a canvas-layer <see cref="Canvas.Windows.Forms.Drawing.Graphics"/> for
    /// drawing onto this bitmap (stub — actual pixel writes are not supported).
    /// </summary>
    public Canvas.Windows.Forms.Drawing.Graphics GetGraphics()
        => new Canvas.Windows.Forms.Drawing.Graphics(Width, Height);

    // ── Clone ─────────────────────────────────────────────────────────────────

    /// <summary>Returns a shallow copy (stub — no pixel data is copied).</summary>
    public Bitmap Clone() => new Bitmap(this);

    /// <summary>Returns a cropped copy (stub — no pixel data is copied).</summary>
    public Bitmap Clone(Rectangle rect, Imaging.PixelFormat format) => new Bitmap(rect.Width, rect.Height, format);

    // ── IDisposable ───────────────────────────────────────────────────────────

    public void Dispose() { }
}

/// <summary>Stub for <c>System.Drawing.Image</c> — base class referenced by Bitmap and PictureBox.</summary>
public abstract class Image : IDisposable
{
    public virtual int Width  { get; protected set; }
    public virtual int Height { get; protected set; }

    public virtual void Dispose() { }

    public static Image FromFile(string filename) => new _FileImage(filename);
    public static Image FromFile(string filename, bool useEmbeddedColorManagement) => new _FileImage(filename);
    public static Image FromStream(System.IO.Stream stream) => new _StreamImage();
    public static Image FromStream(System.IO.Stream stream, bool useEmbeddedColorManagement) => new _StreamImage();
    public static Bitmap FromHbitmap(IntPtr hbitmap) => new Bitmap(0, 0);

    public Bitmap GetThumbnailImage(int thumbWidth, int thumbHeight,
        System.Drawing.Image.GetThumbnailImageAbort? callback, IntPtr callbackData)
        => new Bitmap(thumbWidth, thumbHeight);

    public delegate bool GetThumbnailImageAbort();

    public void Save(string filename) { }
    public void Save(System.IO.Stream stream, Imaging.ImageFormat format) { }
    public void Save(string filename, Imaging.ImageFormat format) { }

    public Imaging.PixelFormat PixelFormat => Imaging.PixelFormat.Format32bppArgb;
    public SizeF PhysicalDimension => new SizeF(Width, Height);
    public System.Drawing.Size Size => new System.Drawing.Size(Width, Height);
    public float HorizontalResolution => 96f;
    public float VerticalResolution => 96f;
    public Imaging.ImageFormat RawFormat => Imaging.ImageFormat.Png;

    private sealed class _FileImage : Image
    {
        public _FileImage(string _) { }
    }
    private sealed class _StreamImage : Image { }
}
