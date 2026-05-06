
namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms RichTextBox control.
/// Canvas rendering is plain-text; rich content is preserved in
/// <see cref="HtmlContent"/> (HTML) and <see cref="Rtf"/> (RTF).
/// Copy writes <c>text/html</c> + <c>text/plain</c> to the real browser clipboard;
/// Paste reads <c>text/html</c> first and falls back to plain text.
/// </summary>
public class RichTextBox : TextBoxBase
{
    private bool _detectUrls = true;
    private bool _enableAutoDragDrop = false;
    private RichTextBoxScrollBars _scrollBars = RichTextBoxScrollBars.Both;
    private int _zoomFactor = 1;
    private string _rtf  = string.Empty;
    private string _html = string.Empty;

    public event EventHandler? SelectionChanged;

    public RichTextBox()
    {
        Width = 100;
        Height = 96;
        Multiline = true;
        WordWrap = true;
        ScrollBars = ScrollBars.Vertical;
        AcceptsReturn = true;
    }

    public bool DetectUrls { get => _detectUrls; set => _detectUrls = value; }
    public bool EnableAutoDragDrop { get => _enableAutoDragDrop; set => _enableAutoDragDrop = value; }
    public RichTextBoxScrollBars RichTextBoxScrollBars { get => _scrollBars; set { _scrollBars = value; Invalidate(); } }

    /// <summary>
    /// RTF content — stored for compatibility, rendered as plain text.
    /// Setting this property also clears any stored HTML content.
    /// </summary>
    public string Rtf
    {
        get => _rtf;
        set
        {
            _rtf  = value ?? string.Empty;
            _html = string.Empty;
            Text  = StripRtf(_rtf);
        }
    }

    /// <summary>
    /// HTML content of the control.  Setting this property updates
    /// <see cref="TextBoxBase.Text"/> with a plain-text representation
    /// (tags stripped) so the canvas renderer can display it.
    /// </summary>
    public string HtmlContent
    {
        get => _html;
        set
        {
            _html = value ?? string.Empty;
            _rtf  = string.Empty;
            Text  = Clipboard.StripHtmlTags(_html);
            Invalidate();
        }
    }

    public int ZoomFactor { get => _zoomFactor; set { _zoomFactor = Math.Max(1, value); Invalidate(); } }

    // ── Copy / Paste (HTML-aware) ─────────────────────────────────────────────

    /// <summary>
    /// Copies selected text to the clipboard.  When the control has HTML content
    /// the selection is wrapped in a minimal HTML fragment and written as
    /// <c>text/html</c> + <c>text/plain</c> via the browser Clipboard API.
    /// </summary>
    public new void Copy()
    {
        if (SelectionLength == 0) return;

        var plain = SelectedText;

        if (!string.IsNullOrEmpty(_html))
        {
            // Build a minimal HTML fragment for the selection.
            // A full implementation would map char offsets into the DOM; here we
            // wrap the plain-text selection in a <span> so other apps receive HTML.
            var fragment = BuildHtmlFragment(plain);
            Clipboard.SetText(fragment, TextDataFormat.Html);
        }
        else
        {
            Clipboard.SetText(plain);
        }
    }

    /// <summary>
    /// Cuts selected text.
    /// </summary>
    public new void Cut()
    {
        if (ReadOnly || SelectionLength == 0) return;
        Copy();
        // Remove selected text through base
        SelectedText = string.Empty;
    }

    /// <summary>
    /// Pastes from the clipboard.  Tries <c>text/html</c> first so that HTML
    /// pasted from a browser page is preserved in <see cref="HtmlContent"/>;
    /// falls back to plain text.
    /// </summary>
    public new void Paste() => _ = PasteRichAsync();

    private async Task PasteRichAsync()
    {
        if (ReadOnly) return;

        // Try HTML first
        var html = await Clipboard.GetHtmlAsync();
        if (!string.IsNullOrEmpty(html))
        {
            var plain = Clipboard.StripHtmlTags(html);
            // Merge into existing HTML (append at caret) or replace
            if (string.IsNullOrEmpty(_html))
            {
                _html = html;
                var before = Text[..SelectionStart];
                var after  = Text[(SelectionStart + SelectionLength)..];
                Text  = before + plain + after;
            }
            else
            {
                // Simple strategy: append pasted HTML to stored HTML
                _html += html;
                Text  += plain;
            }
            SelectionStart  = Text.Length;
            SelectionLength = 0;
            Invalidate();
            return;
        }

        // Fall back to plain text via base behaviour
        await Clipboard.RefreshFromJsAsync();
        var text = Clipboard.GetText();
        if (!string.IsNullOrEmpty(text))
        {
            SelectedText = text;
            Invalidate();
        }
    }

    // ── Inherited / overridden helpers ────────────────────────────────────────

    public new void AppendText(string text)
    {
        Text += text;
        if (!string.IsNullOrEmpty(_html))
            _html += System.Net.WebUtility.HtmlEncode(text);
        SelectionStart = Text.Length;
        Invalidate();
    }

    public new void ClearUndo() { /* stub */ }

    public new void Select(int start, int length)
    {
        SelectionStart  = Math.Max(0, Math.Min(start, Text.Length));
        SelectionLength = Math.Max(0, Math.Min(length, Text.Length - SelectionStart));
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    public new void SelectAll() => Select(0, Text.Length);
    public new void ScrollToCaret() { /* stub */ }

    // ── Static helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Wraps a plain-text snippet in an HTML fragment compatible with the
    /// "CF_HTML" clipboard format convention used by browsers.
    /// </summary>
    private static string BuildHtmlFragment(string plainText)
    {
        var encoded = System.Net.WebUtility.HtmlEncode(plainText)
                            .Replace("\r\n", "<br>")
                            .Replace("\n",   "<br>")
                            .Replace("\r",   "<br>");
        return $"<html><body><span>{encoded}</span></body></html>";
    }

    private static string StripRtf(string rtf)
    {
        if (string.IsNullOrEmpty(rtf) || !rtf.TrimStart().StartsWith("{\\rtf"))
            return rtf;

        var sb = new System.Text.StringBuilder();
        int i = 0;
        while (i < rtf.Length)
        {
            char c = rtf[i];
            if (c == '\\')
            {
                i++;
                if (i < rtf.Length && rtf[i] == '\\')      { sb.Append('\\'); i++; }
                else if (i < rtf.Length && rtf[i] == '{')  { sb.Append('{');  i++; }
                else if (i < rtf.Length && rtf[i] == '}')  { sb.Append('}');  i++; }
                else if (i < rtf.Length && rtf[i] == '\n') { sb.Append('\n'); i++; }
                else if (i < rtf.Length && rtf[i] == '\'')
                {
                    i++;
                    if (i + 1 < rtf.Length)
                    {
                        var hex = rtf.Substring(i, 2);
                        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                            sb.Append((char)code);
                        i += 2;
                    }
                }
                else
                {
                    while (i < rtf.Length && rtf[i] != ' ' && rtf[i] != '\\' && rtf[i] != '{' && rtf[i] != '}') i++;
                    if (i < rtf.Length && rtf[i] == ' ') i++;
                }
            }
            else if (c == '{' || c == '}') i++;
            else { sb.Append(c); i++; }
        }
        return sb.ToString().Trim();
    }
}

public enum RichTextBoxScrollBars { None, Horizontal, Vertical, Both, ForcedHorizontal, ForcedVertical, ForcedBoth }
