namespace System.Windows.Forms;

// ── DataFormats ───────────────────────────────────────────────────────────────

/// <summary>
/// WinForms-compatible <c>DataFormats</c> — predefined clipboard/drag-drop format names.
/// </summary>
public static class DataFormats
{
    public const string Text            = "Text";
    public const string UnicodeText     = "UnicodeText";
    public const string Html            = "HTML Format";
    public const string Rtf             = "Rich Text Format";
    public const string Bitmap          = "Bitmap";
    public const string FileDrop        = "FileDrop";
    public const string StringFormat    = "System.String";
    public const string Serializable    = "WindowsForms10PersistentObject";
    public const string CommaSeparatedValue = "CSV";

    /// <summary>Returns a <see cref="Format"/> descriptor for a named format.</summary>
    public static Format GetFormat(string format) => new Format(format, format.GetHashCode());

    /// <summary>Minimal format descriptor (name + id).</summary>
    public record Format(string Name, int Id);
}

// ── IDataObject ───────────────────────────────────────────────────────────────

/// <summary>
/// WinForms <c>IDataObject</c> interface — in-process data container.
/// </summary>
public interface IDataObject
{
    object?  GetData(string format);
    object?  GetData(Type format);
    object?  GetData(string format, bool autoConvert);
    bool     GetDataPresent(string format);
    bool     GetDataPresent(Type format);
    bool     GetDataPresent(string format, bool autoConvert);
    string[] GetFormats();
    string[] GetFormats(bool autoConvert);
    void     SetData(object data);
    void     SetData(string format, object data);
    void     SetData(Type format, object data);
    void     SetData(string format, bool autoConvert, object data);
}

// ── DataObject ────────────────────────────────────────────────────────────────

/// <summary>
/// WinForms-compatible in-process <c>DataObject</c>.
/// Stores arbitrary typed payloads keyed by format name.
/// No OS clipboard interaction — use <see cref="Clipboard"/> for that.
/// </summary>
public class DataObject : IDataObject
{
    private readonly Dictionary<string, object> _store = new(StringComparer.OrdinalIgnoreCase);

    public DataObject() { }

    /// <summary>Initialise with a single text payload.</summary>
    public DataObject(string text)
    {
        SetData(DataFormats.UnicodeText, text);
        SetData(DataFormats.Text, text);
    }

    /// <summary>Initialise with a named payload.</summary>
    public DataObject(string format, object data) => SetData(format, data);

    // ── IDataObject ───────────────────────────────────────────────────────────

    public object? GetData(string format)
        => _store.TryGetValue(format, out var v) ? v : null;

    public object? GetData(Type format)
        => GetData(format.FullName ?? format.Name);

    public object? GetData(string format, bool autoConvert)
    {
        if (_store.TryGetValue(format, out var v)) return v;
        if (!autoConvert) return null;
        // Auto-convert: Text ↔ UnicodeText
        if (string.Equals(format, DataFormats.Text, StringComparison.OrdinalIgnoreCase)
            && _store.TryGetValue(DataFormats.UnicodeText, out var u)) return u;
        if (string.Equals(format, DataFormats.UnicodeText, StringComparison.OrdinalIgnoreCase)
            && _store.TryGetValue(DataFormats.Text, out var t)) return t;
        return null;
    }

    public bool GetDataPresent(string format)
        => _store.ContainsKey(format);

    public bool GetDataPresent(Type format)
        => GetDataPresent(format.FullName ?? format.Name);

    public bool GetDataPresent(string format, bool autoConvert)
        => GetData(format, autoConvert) != null;

    public string[] GetFormats() => _store.Keys.ToArray();

    public string[] GetFormats(bool autoConvert)
    {
        if (!autoConvert) return GetFormats();
        var formats = new HashSet<string>(_store.Keys, StringComparer.OrdinalIgnoreCase);
        if (formats.Contains(DataFormats.Text))      formats.Add(DataFormats.UnicodeText);
        if (formats.Contains(DataFormats.UnicodeText)) formats.Add(DataFormats.Text);
        return formats.ToArray();
    }

    public void SetData(object data)
    {
        var key = data?.GetType().FullName ?? DataFormats.StringFormat;
        _store[key] = data!;
        // Convenience: also store strings under both text format names
        if (data is string s)
        {
            _store[DataFormats.Text]        = s;
            _store[DataFormats.UnicodeText] = s;
        }
    }

    public void SetData(string format, object data)             => _store[format] = data;
    public void SetData(Type format, object data)               => SetData(format.FullName ?? format.Name, data);
    public void SetData(string format, bool autoConvert, object data) => SetData(format, data);

    // ── Convenience accessors ─────────────────────────────────────────────────

    /// <summary>Returns the stored text (Unicode or plain) or null.</summary>
    public string? GetText()
        => GetData(DataFormats.UnicodeText) as string
        ?? GetData(DataFormats.Text) as string;

    /// <summary>Returns the stored HTML fragment or null.</summary>
    public string? GetHtml() => GetData(DataFormats.Html) as string;

    /// <summary>Returns the stored RTF or null.</summary>
    public string? GetRtf() => GetData(DataFormats.Rtf) as string;
}
