namespace System.Drawing;

/// <summary>
/// Cross-platform stub for <c>System.Drawing.Icon</c>.
/// GDI+ icon loading is Windows-only; this stub accepts file paths and embedded
/// resource handles and exposes the API surface designer-generated code calls.
/// Rendering uses the <see cref="ResourcePath"/> URL via the browser image pipeline.
/// </summary>
public sealed class Icon : IDisposable
{
    /// <summary>File path or URL used when rendering the icon as an image.</summary>
    public string? ResourcePath { get; }

    /// <summary>Width hint (px). Default 32.</summary>
    public int Width { get; } = 32;

    /// <summary>Height hint (px). Default 32.</summary>
    public int Height { get; } = 32;

    /// <summary>Creates an icon from a file path or URL.</summary>
    public Icon(string path)
    {
        ResourcePath = path;
    }

    /// <summary>Creates a sized icon from a file path or URL.</summary>
    public Icon(string path, int width, int height)
    {
        ResourcePath = path;
        Width  = width;
        Height = height;
    }

    /// <summary>
    /// Creates an icon from a native handle (Windows HICON).
    /// In the canvas host this is a no-op — the icon will render as blank.
    /// </summary>
    public static Icon FromHandle(IntPtr handle) => new Icon(string.Empty);

    /// <summary>
    /// Creates a new <see cref="Icon"/> of the specified size from this instance.
    /// Returns a same-path icon at the requested dimensions.
    /// </summary>
    public Icon(Icon original, int width, int height)
    {
        ResourcePath = original?.ResourcePath;
        Width  = width;
        Height = height;
    }

    /// <summary>
    /// Creates an icon from a <see cref="Type"/> and a resource name.
    /// Used by <c>ComponentResourceManager</c> / designer-generated code to load
    /// assembly-embedded icons. Stub — renders blank in the canvas host.
    /// </summary>
    public Icon(Type type, string resource)
    {
        ResourcePath = string.Empty; // embedded resource not accessible in browser host
    }

    /// <summary>
    /// Creates an icon from an embedded resource stream.
    /// Stub — the stream is consumed but not decoded. Icon renders blank.
    /// </summary>
    public Icon(global::System.IO.Stream stream)
    {
        _ = stream; // accepted for API compat; no pixel decode in the canvas host
    }

    /// <summary>
    /// Creates a sized icon from an embedded resource stream.
    /// </summary>
    public Icon(global::System.IO.Stream stream, int width, int height)
    {
        _ = stream;
        Width  = width;
        Height = height;
    }

    /// <summary>
    /// Returns a canvas <see cref="Canvas.Windows.Forms.Drawing.Image"/> that wraps the same resource path,
    /// allowing callers that treat an Icon as an Image to render it via the browser image pipeline.
    /// </summary>
    public Canvas.Windows.Forms.Drawing.Image? Image
        => ResourcePath is { Length: > 0 }
            ? new Canvas.Windows.Forms.Drawing.Image { Source = ResourcePath, Width = Width, Height = Height }
            : null;

    public System.Drawing.Size Size => new System.Drawing.Size(Width, Height);

    public System.Drawing.Bitmap ToBitmap() => new System.Drawing.Bitmap(Width, Height);

    public IntPtr Handle => IntPtr.Zero;

    public void Dispose() { }
}

/// <summary>
/// Stub for <c>System.Drawing.SystemIcons</c>.
/// All icons are blank stubs in the canvas host.
/// </summary>
public static class SystemIcons
{
    private static readonly Icon _blank = new Icon(string.Empty);

    public static Icon Application  => _blank;
    public static Icon Asterisk     => _blank;
    public static Icon Error        => _blank;
    public static Icon Exclamation  => _blank;
    public static Icon Hand         => _blank;
    public static Icon Information  => _blank;
    public static Icon Question     => _blank;
    public static Icon Shield       => _blank;
    public static Icon Warning      => _blank;
    public static Icon WinLogo      => _blank;
}
