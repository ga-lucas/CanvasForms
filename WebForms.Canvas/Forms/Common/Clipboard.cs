using Microsoft.JSInterop;

namespace System.Windows.Forms;

/// <summary>
/// WinForms-compatible <c>System.Windows.Forms.Clipboard</c> shim.
///
/// Maintains a local in-process cache for synchronous callers and bridges to the
/// real browser clipboard via <c>navigator.clipboard</c> for text and HTML.
/// HTML round-trips use <c>ClipboardItem</c> (<c>text/html</c> + <c>text/plain</c>).
/// An in-process <see cref="IDataObject"/> store supports <see cref="SetDataObject"/>
/// / <see cref="GetDataObject"/> for arbitrary typed payloads.
/// </summary>
public static class Clipboard
{
    // ── Local caches ──────────────────────────────────────────────────────────
    private static string      _localText = string.Empty;
    private static string      _localHtml = string.Empty;
    private static IDataObject? _dataObject;

    // Injected by FormRenderer on first render – null until then.
    internal static IJSRuntime? _jsRuntime;

    // ── Plain-text API (WinForms compatible) ──────────────────────────────────

    public static bool   ContainsText()                     => !string.IsNullOrEmpty(_localText);
    public static bool   ContainsText(TextDataFormat format) =>
        format == TextDataFormat.Html ? !string.IsNullOrEmpty(_localHtml) : ContainsText();

    public static string GetText()                          => _localText;
    public static string GetText(TextDataFormat format)     =>
        format == TextDataFormat.Html ? _localHtml : _localText;

    public static void SetText(string text)
    {
        _localText = text ?? string.Empty;
        _localHtml = string.Empty; // plain write clears HTML cache
        _ = WriteTextToJsAsync(_localText);
    }

    public static void SetText(string text, TextDataFormat format)
    {
        if (format == TextDataFormat.Html)
        {
            _localHtml = text ?? string.Empty;
            // Derive plain-text fallback by stripping tags
            _localText = StripHtmlTags(_localHtml);
            _ = WriteHtmlToJsAsync(_localHtml, _localText);
        }
        else
        {
            SetText(text);
        }
    }

    public static void Clear()
    {
        _localText  = string.Empty;
        _localHtml  = string.Empty;
        _dataObject = null;
        _ = WriteTextToJsAsync(string.Empty);
    }

    // ── Async helpers ─────────────────────────────────────────────────────────

    /// <summary>Awaitable plain-text write to local cache + real clipboard.</summary>
    public static async Task SetTextAsync(string text)
    {
        _localText = text ?? string.Empty;
        _localHtml = string.Empty;
        await WriteTextToJsAsync(_localText);
    }

    /// <summary>
    /// Awaitable HTML write.  Stores HTML + derived plain text; writes both to the
    /// real clipboard via <c>ClipboardItem</c> when available.
    /// </summary>
    public static async Task SetHtmlAsync(string html, string? plainTextFallback = null)
    {
        _localHtml = html ?? string.Empty;
        _localText = plainTextFallback ?? StripHtmlTags(_localHtml);
        await WriteHtmlToJsAsync(_localHtml, _localText);
    }

    /// <summary>
    /// Reads plain text from the real browser clipboard; falls back to local cache.
    /// </summary>
    public static async Task<string> GetTextAsync()
    {
        if (_jsRuntime != null)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<string?>("clipboardReadText");
                if (result != null) { _localText = result; return result; }
            }
            catch { /* fall through */ }
        }
        return _localText;
    }

    /// <summary>
    /// Reads HTML from the real browser clipboard (<c>text/html</c> MIME).
    /// Returns <c>null</c> if no HTML is present, permission is denied, or the
    /// browser (Firefox) does not support <c>clipboard.read()</c>.
    /// Falls back to the local HTML cache.
    /// </summary>
    public static async Task<string?> GetHtmlAsync()
    {
        if (_jsRuntime != null)
        {
            try
            {
                var result = await _jsRuntime.InvokeAsync<string?>("clipboardReadHtml");
                if (result != null) { _localHtml = result; return result; }
            }
            catch { /* fall through */ }
        }
        return string.IsNullOrEmpty(_localHtml) ? null : _localHtml;
    }

    // ── DataObject API (in-process, WinForms compatible) ─────────────────────

    /// <summary>
    /// Returns <c>true</c> if the clipboard contains data for <paramref name="format"/>.
    /// </summary>
    public static bool ContainsData(string format)
        => _dataObject?.GetDataPresent(format) ?? false;

    /// <summary>
    /// Sets arbitrary data on the in-process clipboard store.
    /// </summary>
    /// <param name="copy">Ignored (no OS-level clipboard ownership in WASM).</param>
    public static void SetDataObject(object data, bool copy = false)
    {
        if (data is IDataObject ido)
        {
            _dataObject = ido;
            // Sync text caches if the object carries text
            if (ido.GetData(DataFormats.UnicodeText) is string ut) _localText = ut;
            else if (ido.GetData(DataFormats.Text) is string t)    _localText = t;
            if (ido.GetData(DataFormats.Html) is string h)         _localHtml = h;
            _ = SyncDataObjectToJsAsync(_dataObject);
        }
        else
        {
            // Wrap plain object
            var obj = new DataObject();
            obj.SetData(data);
            SetDataObject(obj, copy);
        }
    }

    /// <summary>
    /// Returns the current in-process <see cref="IDataObject"/>, or a new one
    /// constructed from the text/HTML caches if none has been set.
    /// </summary>
    public static IDataObject GetDataObject()
    {
        if (_dataObject != null) return _dataObject;
        var obj = new DataObject();
        if (!string.IsNullOrEmpty(_localText)) obj.SetData(DataFormats.Text,        _localText);
        if (!string.IsNullOrEmpty(_localText)) obj.SetData(DataFormats.UnicodeText, _localText);
        if (!string.IsNullOrEmpty(_localHtml)) obj.SetData(DataFormats.Html,        _localHtml);
        return obj;
    }

    // ── Internal helpers ──────────────────────────────────────────────────────

    internal static async Task RefreshFromJsAsync()
    {
        if (_jsRuntime == null) return;
        try
        {
            // Try HTML first (richer), fall back to plain text
            var html = await _jsRuntime.InvokeAsync<string?>("clipboardReadHtml");
            if (html != null)
            {
                _localHtml = html;
                _localText = StripHtmlTags(html);
                return;
            }
            var text = await _jsRuntime.InvokeAsync<string?>("clipboardReadText");
            if (text != null) _localText = text;
        }
        catch { /* permission denied */ }
    }

    private static async Task WriteTextToJsAsync(string text)
    {
        if (_jsRuntime == null) return;
        try { await _jsRuntime.InvokeAsync<bool>("clipboardWriteText", text); }
        catch { /* swallow */ }
    }

    private static async Task WriteHtmlToJsAsync(string html, string plainText)
    {
        if (_jsRuntime == null) return;
        try { await _jsRuntime.InvokeAsync<bool>("clipboardWriteHtml", html, plainText); }
        catch { /* swallow */ }
    }

    private static async Task SyncDataObjectToJsAsync(IDataObject data)
    {
        // Write whatever text format is available to the real clipboard
        if (data.GetData(DataFormats.Html) is string html)
        {
            var plain = data.GetData(DataFormats.UnicodeText) as string
                     ?? data.GetData(DataFormats.Text) as string
                     ?? StripHtmlTags(html);
            await WriteHtmlToJsAsync(html, plain);
        }
        else if (data.GetData(DataFormats.UnicodeText) is string ut)
            await WriteTextToJsAsync(ut);
        else if (data.GetData(DataFormats.Text) is string t)
            await WriteTextToJsAsync(t);
    }

    /// <summary>
    /// Minimal HTML tag stripper — removes all tags and decodes common entities.
    /// Used to derive plain-text fallback from an HTML clipboard payload.
    /// </summary>
    internal static string StripHtmlTags(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;

        // Remove <style> and <script> blocks entirely
        html = System.Text.RegularExpressions.Regex.Replace(html,
            @"<(script|style)[^>]*>.*?</(script|style)>",
            string.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase |
            System.Text.RegularExpressions.RegexOptions.Singleline);

        // Block-level tags → newline
        html = System.Text.RegularExpressions.Regex.Replace(html,
            @"<(br|/p|/div|/h[1-6]|/li|/tr)[^>]*>",
            "\n",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Remove remaining tags
        html = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>", string.Empty);

        // Decode common HTML entities
        html = html.Replace("&amp;",  "&")
                   .Replace("&lt;",   "<")
                   .Replace("&gt;",   ">")
                   .Replace("&quot;", "\"")
                   .Replace("&apos;", "'")
                   .Replace("&nbsp;", " ");

        // Collapse excessive blank lines
        html = System.Text.RegularExpressions.Regex.Replace(html, @"\n{3,}", "\n\n");

        return html.Trim();
    }
}

/// <summary>
/// Specifies the data format used with the <see cref="Clipboard"/> methods.
/// </summary>
public enum TextDataFormat
{
    Text,
    UnicodeText,
    Rtf,
    Html,
    CommaSeparatedValue
}
