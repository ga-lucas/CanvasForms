// Stubs for the System.Drawing.Text namespace.
// These types live in a separate file because SystemDrawingStubs.cs uses a
// file-scoped namespace declaration, which cannot coexist with block namespaces.

namespace System.Drawing.Text
{
    /// <summary>
    /// Specifies the quality of text rendering. Provided for API compatibility;
    /// the browser controls text anti-aliasing in canvas rendering.
    /// </summary>
    public enum TextRenderingHint
    {
        SystemDefault            = 0,
        SingleBitPerPixelGridFit = 1,
        SingleBitPerPixel        = 2,
        AntiAliasGridFit         = 3,
        AntiAlias                = 4,
        ClearTypeGridFit         = 5,
    }

    /// <summary>Abstract base for font collections.</summary>
    public abstract class FontCollection
    {
        /// <summary>
        /// Returns the array of font families in this collection.
        /// Always empty in the canvas host — font enumeration is unavailable server-side.
        /// </summary>
        public global::System.Drawing.FontFamily[] Families
            => global::System.Array.Empty<global::System.Drawing.FontFamily>();
    }

    /// <summary>
    /// Provides access to installed font families.
    /// In the canvas host, font enumeration is not available — returns empty collection.
    /// </summary>
    public sealed class InstalledFontCollection : FontCollection
    {
        public InstalledFontCollection() { }
    }

    /// <summary>
    /// Provides access to a private collection of font families loaded from files or memory.
    /// In the canvas host, font loading from files is not supported — stub no-ops.
    /// </summary>
    public sealed class PrivateFontCollection : FontCollection, global::System.IDisposable
    {
        /// <summary>Adds a font from the specified file path (no-op in canvas host).</summary>
        public void AddFontFile(string filename) { }

        /// <summary>Adds a font from a memory block (no-op in canvas host).</summary>
        public void AddMemoryFont(global::System.IntPtr memory, int length) { }

        public void Dispose() { }
    }
}
