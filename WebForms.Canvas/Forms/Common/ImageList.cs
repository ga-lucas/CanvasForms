namespace System.Windows.Forms;

/// <summary>
/// Manages a collection of images referenced by URL/path for use in canvas-rendered controls.
/// Images are stored as URL strings (relative or absolute) so they can be drawn via
/// Graphics.DrawImage, which resolves them through the browser's image cache.
/// </summary>
public class ImageList : IDisposable
{
    private readonly ImageCollection _images;

    public ImageList()
    {
        _images = new ImageCollection(this);
    }

    /// <summary>
    /// Initialises a new <see cref="ImageList"/> owned by the specified
    /// <see cref="System.ComponentModel.IContainer"/> (for designer compatibility).
    /// </summary>
    public ImageList(System.ComponentModel.IContainer container) : this() { }

    // ── Properties ────────────────────────────────────────────────────────────

    /// <summary>The size to use when drawing images from this list (default 16×16).</summary>
    public Size ImageSize { get; set; } = new Size(16, 16);

    /// <summary>Not used for rendering; accepted for API compatibility.</summary>
    public ColorDepth ColorDepth { get; set; } = ColorDepth.Depth32Bit;

    /// <summary>Not used for rendering; accepted for API compatibility.</summary>
    public Color TransparentColor { get; set; } = Color.Transparent;

    public object? Tag { get; set; }

    public ImageCollection Images => _images;

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the URL stored at <paramref name="index"/>, or null if out of range.
    /// Controls call this to resolve what to pass to Graphics.DrawImage.
    /// </summary>
    public string? GetUrl(int index) => _images.GetUrl(index);

    /// <summary>
    /// Returns the URL for the image matching <paramref name="key"/>, or null if not found.
    /// </summary>
    public string? GetUrl(string key) => _images.GetUrl(key);

    public void Dispose() { }

    // ── ImageCollection ───────────────────────────────────────────────────────

    public sealed class ImageCollection : IEnumerable<string>
    {
        private readonly ImageList _owner;
        private readonly List<string> _urls = new();
        private readonly List<string> _keys = new();

        internal ImageCollection(ImageList owner) => _owner = owner;

        public int Count => _urls.Count;

        /// <summary>Returns the URL at the given index.</summary>
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _urls.Count) return string.Empty;
                return _urls[index];
            }
        }

        /// <summary>Returns the URL for the image with the given key.</summary>
        public string this[string key]
        {
            get
            {
                var idx = IndexOfKey(key);
                return idx >= 0 ? _urls[idx] : string.Empty;
            }
        }

        /// <summary>
        /// Adds an image by its URL or relative path.
        /// This is the primary method for CanvasForms apps; the URL is passed directly to DrawImage.
        /// </summary>
        public int Add(string imageUrl, string key = "")
        {
            _urls.Add(imageUrl);
            _keys.Add(key);
            return _urls.Count - 1;
        }

        /// <summary>
        /// WinForms API compat: accepts any object; only stores the result of ToString()
        /// so that URL strings round-trip correctly through translated apps.
        /// </summary>
        public int Add(object image)
        {
            var url = image?.ToString() ?? string.Empty;
            return Add(url);
        }

        /// <summary>
        /// Associates a key with an already-added image index.
        /// </summary>
        public void SetKeyName(int index, string key)
        {
            if (index >= 0 && index < _keys.Count)
                _keys[index] = key;
        }

        public bool ContainsKey(string key) => IndexOfKey(key) >= 0;

        public int IndexOfKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return -1;
            for (int i = 0; i < _keys.Count; i++)
                if (string.Equals(_keys[i], key, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _urls.Count) return;
            _urls.RemoveAt(index);
            _keys.RemoveAt(index);
        }

        public void Clear()
        {
            _urls.Clear();
            _keys.Clear();
        }

        internal string? GetUrl(int index) =>
            (index >= 0 && index < _urls.Count) ? _urls[index] : null;

        internal string? GetUrl(string key)
        {
            var idx = IndexOfKey(key);
            return idx >= 0 ? _urls[idx] : null;
        }

        public IEnumerator<string> GetEnumerator() => _urls.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

public enum ColorDepth
{
    Depth4Bit = 4,
    Depth8Bit = 8,
    Depth16Bit = 16,
    Depth24Bit = 24,
    Depth32Bit = 32,
}
