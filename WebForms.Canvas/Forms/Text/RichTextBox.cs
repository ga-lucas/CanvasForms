
namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms RichTextBox control.
/// Rendering: when RTF content is present the control parses it into styled runs
/// and draws each run with its own font/colour so bold, italic, underline, colour
/// changes and font-size changes are visible on the canvas.  Plain-text input still
/// falls through to the normal <see cref="TextBoxBase"/> paint path.
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

    // Parsed run cache — rebuilt whenever RTF changes.
    private List<RtfRun>? _runs;
    private string _lastParsedRtf = string.Empty;

    // Selection-level formatting applied by code (not reflected back into _rtf).
    private Font?  _selectionFont;
    private Color  _selectionColor  = Color.Empty;
    private Color  _selectionBackColor = Color.Empty;

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

    // ── Properties ────────────────────────────────────────────────────────────

    public bool DetectUrls { get => _detectUrls; set => _detectUrls = value; }
    public bool EnableAutoDragDrop { get => _enableAutoDragDrop; set => _enableAutoDragDrop = value; }
    public RichTextBoxScrollBars RichTextBoxScrollBars { get => _scrollBars; set { _scrollBars = value; Invalidate(); } }

    /// <summary>
    /// RTF content — stored and parsed into runs for canvas rendering.
    /// </summary>
    public string Rtf
    {
        get => _rtf;
        set
        {
            _rtf  = value ?? string.Empty;
            _html = string.Empty;
            _runs = null;
            Text  = StripRtf(_rtf);
        }
    }

    /// <summary>
    /// HTML content of the control.  Tags are stripped to produce plain text for
    /// the text buffer; the original HTML is kept for clipboard round-tripping.
    /// </summary>
    public string HtmlContent
    {
        get => _html;
        set
        {
            _html = value ?? string.Empty;
            _rtf  = string.Empty;
            _runs = null;
            Text  = Clipboard.StripHtmlTags(_html);
            Invalidate();
        }
    }

    public int ZoomFactor { get => _zoomFactor; set { _zoomFactor = Math.Max(1, value); Invalidate(); } }

    // ── Selection-level formatting ────────────────────────────────────────────
    // These match the WinForms API used by designer-generated code to apply
    // formatting to a selection programmatically.  They are applied when text is
    // inserted and encoded back into _rtf on future GetRtf() calls (basic support).

    public Font? SelectionFont
    {
        get => _selectionFont;
        set { _selectionFont = value; Invalidate(); }
    }

    public Color SelectionColor
    {
        get => _selectionColor;
        set { _selectionColor = value; Invalidate(); }
    }

    public Color SelectionBackColor
    {
        get => _selectionBackColor;
        set { _selectionBackColor = value; Invalidate(); }
    }

    public bool SelectionBold
    {
        get => (_selectionFont?.Style & FontStyle.Bold) != 0;
        set
        {
            var cur  = _selectionFont ?? Font ?? new Font("Segoe UI", 12);
            var style = value
                ? cur.Style | FontStyle.Bold
                : cur.Style & ~FontStyle.Bold;
            _selectionFont = new Font(cur.Family, cur.Size, style);
            Invalidate();
        }
    }

    public bool SelectionItalic
    {
        get => (_selectionFont?.Style & FontStyle.Italic) != 0;
        set
        {
            var cur  = _selectionFont ?? Font ?? new Font("Segoe UI", 12);
            var style = value
                ? cur.Style | FontStyle.Italic
                : cur.Style & ~FontStyle.Italic;
            _selectionFont = new Font(cur.Family, cur.Size, style);
            Invalidate();
        }
    }

    public bool SelectionUnderline
    {
        get => (_selectionFont?.Style & FontStyle.Underline) != 0;
        set
        {
            var cur  = _selectionFont ?? Font ?? new Font("Segoe UI", 12);
            var style = value
                ? cur.Style | FontStyle.Underline
                : cur.Style & ~FontStyle.Underline;
            _selectionFont = new Font(cur.Family, cur.Size, style);
            Invalidate();
        }
    }

    public HorizontalAlignment SelectionAlignment { get; set; } = HorizontalAlignment.Left;
    public RichTextBoxSelectionTypes SelectionType   { get; } = RichTextBoxSelectionTypes.Empty;
    public bool SelectionProtected { get; set; }
    public int  SelectionIndent    { get; set; }
    public int  SelectionHangingIndent { get; set; }
    public int  SelectionRightIndent  { get; set; }

    // ── Search ────────────────────────────────────────────────────────────────

    public int Find(string str) => Find(str, RichTextBoxFinds.None);

    public int Find(string str, RichTextBoxFinds options)
    {
        if (string.IsNullOrEmpty(str)) return -1;
        var comparison = (options & RichTextBoxFinds.MatchCase) != 0
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        var idx = Text.IndexOf(str, comparison);
        if (idx >= 0)
        {
            SelectionStart  = idx;
            SelectionLength = str.Length;
        }
        return idx;
    }

    public int Find(string str, int start, int end, RichTextBoxFinds options)
    {
        if (string.IsNullOrEmpty(str) || start >= Text.Length) return -1;
        var sub = Text[start..Math.Min(end, Text.Length)];
        var comparison = (options & RichTextBoxFinds.MatchCase) != 0
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
        var rel = sub.IndexOf(str, comparison);
        if (rel >= 0)
        {
            SelectionStart  = start + rel;
            SelectionLength = str.Length;
            return start + rel;
        }
        return -1;
    }

    // ── RTF load / save ───────────────────────────────────────────────────────

    public void LoadFile(string path, RichTextBoxStreamType fileType = RichTextBoxStreamType.RichText)
    {
        if (!global::System.IO.File.Exists(path)) return;
        var content = global::System.IO.File.ReadAllText(path);
        if (fileType == RichTextBoxStreamType.PlainText)
            Text = content;
        else
            Rtf = content;
    }

    public void SaveFile(string path, RichTextBoxStreamType fileType = RichTextBoxStreamType.RichText)
    {
        var content = fileType == RichTextBoxStreamType.PlainText ? Text : _rtf;
        global::System.IO.File.WriteAllText(path, content);
    }

    // ── Copy / Paste (HTML-aware) ─────────────────────────────────────────────

    public new void Copy()
    {
        if (SelectionLength == 0) return;
        var plain = SelectedText;
        if (!string.IsNullOrEmpty(_html))
        {
            var fragment = BuildHtmlFragment(plain);
            Clipboard.SetText(fragment, TextDataFormat.Html);
        }
        else
        {
            Clipboard.SetText(plain);
        }
    }

    public new void Cut()
    {
        if (ReadOnly || SelectionLength == 0) return;
        Copy();
        SelectedText = string.Empty;
    }

    public new void Paste() => _ = PasteRichAsync();

    private async Task PasteRichAsync()
    {
        if (ReadOnly) return;
        var html = await Clipboard.GetHtmlAsync();
        if (!string.IsNullOrEmpty(html))
        {
            var plain = Clipboard.StripHtmlTags(html);
            if (string.IsNullOrEmpty(_html))
            {
                _html = html;
                var before = Text[..SelectionStart];
                var after  = Text[(SelectionStart + SelectionLength)..];
                Text  = before + plain + after;
            }
            else
            {
                _html += html;
                Text  += plain;
            }
            SelectionStart  = Text.Length;
            SelectionLength = 0;
            Invalidate();
            return;
        }
        await Clipboard.RefreshFromJsAsync();
        var text = Clipboard.GetText();
        if (!string.IsNullOrEmpty(text))
        {
            SelectedText = text;
            Invalidate();
        }
    }

    // ── Painting ──────────────────────────────────────────────────────────────

    /// <summary>
    /// When RTF content is present the control renders styled runs directly.
    /// Plain-text content falls through to the base TextBoxBase paint path.
    /// </summary>
    protected internal override void OnPaint(PaintEventArgs e)
    {
        if (string.IsNullOrEmpty(_rtf))
        {
            // No RTF — use the standard TextBoxBase renderer.
            base.OnPaint(e);
            return;
        }

        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);
        var borderWidth = GetBorderWidth();
        var hasFocus = FindForm() is Form form && form.FocusedControl == this;

        DrawBackground(g, bounds);
        DrawBorder(g, bounds, hasFocus);

        const int textPadding = 3;
        var textBounds = new Rectangle(
            borderWidth + textPadding,
            borderWidth + textPadding,
            Width  - borderWidth * 2 - textPadding * 2,
            Height - borderWidth * 2 - textPadding * 2);

        g.Save();
        g.SetClip(textBounds);

        DrawRtfRuns(g, textBounds);

        if (hasFocus && Enabled && _selectionLength == 0)
            DrawCaret(g, Text, textBounds, FindForm()?.TextMeasurementService);

        g.Restore();

        // Scrollbar chrome (reuse base helper via plain-text path — scrollbar state is in TextBoxBase).
        if (Multiline && _scrollBars != RichTextBoxScrollBars.None)
        {
            // Call base only for the scrollbar chrome by temporarily blanking _rtf.
            // Simpler: just draw using the same approach as base.
            DrawRtfScrollbar(g, bounds, borderWidth);
        }

        // Skip base.OnPaint — we drew everything ourselves.
    }

    private void DrawRtfRuns(Graphics g, Rectangle textBounds)
    {
        var runs = GetParsedRuns();
        if (runs.Count == 0) return;

        var baseFont   = Font ?? new Font("Segoe UI", 12);
        var lineHeight = (int)(baseFont.Size * 1.4f) + 2;
        int x = textBounds.X;
        int y = textBounds.Y - _scrollOffsetY;

        foreach (var run in runs)
        {
            if (run.IsLineBreak)
            {
                x  = textBounds.X;
                y += lineHeight;
                continue;
            }

            if (string.IsNullOrEmpty(run.Text)) continue;
            if (y + lineHeight < textBounds.Y) { /* above visible area */ continue; }
            if (y > textBounds.Bottom) break;

            // Build font for this run.
            var fontFamily = string.IsNullOrEmpty(run.FontFamily) ? baseFont.Family : run.FontFamily;
            var fontSize   = run.FontSize > 0 ? run.FontSize : (int)baseFont.Size;
            var style      = FontStyle.Regular;
            if (run.Bold)      style |= FontStyle.Bold;
            if (run.Italic)    style |= FontStyle.Italic;
            if (run.Underline) style |= FontStyle.Underline;
            if (run.Strikeout) style |= FontStyle.Strikeout;

            var runFont = new Font(fontFamily, fontSize, style);
            Color color = run.Color.IsEmpty ? ForeColor : run.Color;
            using var brush = new SolidBrush(color);
            g.DrawString(run.Text, runFont, brush, x, y);

            // Advance x by approximate run width (no TextMeasurementService needed for layout here;
            // a future pass can use it for precise glyph-width tracking).
            x += run.Text.Length * (int)(fontSize * 0.6f);

            // Simple word-wrap: if x exceeds right edge push to next line.
            if (x >= textBounds.Right)
            {
                x  = textBounds.X;
                y += lineHeight;
            }
        }
    }

    private void DrawRtfScrollbar(Graphics g, Rectangle bounds, int borderWidth)
    {
        var baseFont     = Font ?? new Font("Segoe UI", 12);
        var lineHeight   = (int)(baseFont.Size * 1.4f) + 2;
        var runs         = GetParsedRuns();
        var totalLines   = Math.Max(1, runs.Count(r => r.IsLineBreak) + 1);
        var visibleLines = Math.Max(1, (bounds.Height - borderWidth * 2 - 6) / lineHeight);

        if (totalLines <= visibleLines) return;

        var track = new Rectangle(
            bounds.Right - VerticalScrollbarHelper.Width - borderWidth,
            borderWidth,
            VerticalScrollbarHelper.Width,
            bounds.Height - borderWidth * 2);
        var topLine = _scrollOffsetY / Math.Max(1, lineHeight);
        new VerticalScrollbarHelper(track, totalLines, visibleLines, topLine).Draw(g);
    }

    // ── RTF parser ────────────────────────────────────────────────────────────

    private List<RtfRun> GetParsedRuns()
    {
        if (_runs != null && _lastParsedRtf == _rtf) return _runs;
        _runs = ParseRtfRuns(_rtf);
        _lastParsedRtf = _rtf;
        return _runs;
    }

    /// <summary>
    /// Parses a subset of RTF sufficient to extract styled text runs.
    /// Handles: \colortbl, \fonttbl, \b, \i, \ul, \ulnone, \strike, \plain,
    /// \pard, \par, \cf, \fs, \f, \'xx hex escapes.
    /// </summary>
    private static List<RtfRun> ParseRtfRuns(string rtf)
    {
        var result = new List<RtfRun>();
        if (string.IsNullOrEmpty(rtf)) return result;

        // ── Extract colour table ──────────────────────────────────────────────
        var colorTable = new List<Color> { Color.Black }; // index 0 = auto
        var ctMatch = System.Text.RegularExpressions.Regex.Match(
            rtf, @"\\colortbl\s*;(.*?)}", System.Text.RegularExpressions.RegexOptions.Singleline);
        if (ctMatch.Success)
        {
            foreach (System.Text.RegularExpressions.Match m in
                System.Text.RegularExpressions.Regex.Matches(ctMatch.Groups[1].Value,
                    @"\\red(\d+)\\green(\d+)\\blue(\d+)\s*;"))
            {
                colorTable.Add(Color.FromArgb(
                    int.Parse(m.Groups[1].Value),
                    int.Parse(m.Groups[2].Value),
                    int.Parse(m.Groups[3].Value)));
            }
        }

        // ── Extract font table ────────────────────────────────────────────────
        var fontTable = new Dictionary<int, string>();
        foreach (System.Text.RegularExpressions.Match m in
            System.Text.RegularExpressions.Regex.Matches(rtf,
                @"\{\\f(\d+)[^}]*\\fnil[^}]*\\fname\s+([^;]+);"))
        {
            fontTable[int.Parse(m.Groups[1].Value)] = m.Groups[2].Value.Trim();
        }
        // Also try simpler font name pattern: {\f0\froman Times New Roman;}
        foreach (System.Text.RegularExpressions.Match m in
            System.Text.RegularExpressions.Regex.Matches(rtf,
                @"\{\\f(\d+)[^}]+ ([A-Za-z][^;\\{}]+);\}"))
        {
            if (!fontTable.ContainsKey(int.Parse(m.Groups[1].Value)))
                fontTable[int.Parse(m.Groups[1].Value)] = m.Groups[2].Value.Trim();
        }

        // ── Walk the RTF token stream ─────────────────────────────────────────
        // State
        bool bold = false, italic = false, underline = false, strikeout = false;
        int  colorIdx = 0, fontIdx = 0, halfPts = 0; // \fs is in half-points
        var  textBuf  = new System.Text.StringBuilder();
        int depth = 0;
        int i = 0;

        // Skip outer header up to first \pard or until depth returns to 1.
        // Simple approach: skip the {\rtf1 header group content until we hit \pard.
        int pard = rtf.IndexOf("\\pard", StringComparison.Ordinal);
        if (pard > 0) i = pard;

        void FlushRun()
        {
            if (textBuf.Length == 0) return;
            var color = (colorIdx > 0 && colorIdx < colorTable.Count)
                ? colorTable[colorIdx] : Color.Empty;
            var fontSize = halfPts > 0 ? halfPts / 2 : 0;
            fontTable.TryGetValue(fontIdx, out var fontFamily);
            result.Add(new RtfRun(textBuf.ToString(), bold, italic, underline, strikeout, color, fontFamily ?? string.Empty, fontSize));
            textBuf.Clear();
        }

        while (i < rtf.Length)
        {
            char c = rtf[i];

            if (c == '{')  { depth++; i++; continue; }
            if (c == '}')  { depth--; i++; if (depth < 0) break; continue; }

            if (c == '\\')
            {
                i++;
                if (i >= rtf.Length) break;
                char next = rtf[i];

                // Escaped chars
                if (next == '\\') { textBuf.Append('\\'); i++; continue; }
                if (next == '{')  { textBuf.Append('{');  i++; continue; }
                if (next == '}')  { textBuf.Append('}');  i++; continue; }
                if (next == '\n' || next == '\r') { i++; continue; }

                // Hex escape \'xx
                if (next == '\'')
                {
                    i++;
                    if (i + 1 < rtf.Length && int.TryParse(rtf.Substring(i, 2),
                        System.Globalization.NumberStyles.HexNumber, null, out int code))
                    {
                        textBuf.Append((char)code);
                        i += 2;
                    }
                    continue;
                }

                // Read control word
                var wordStart = i;
                while (i < rtf.Length && char.IsLetter(rtf[i])) i++;
                var word = rtf[wordStart..i];

                // Read optional numeric parameter
                bool neg = i < rtf.Length && rtf[i] == '-';
                if (neg) i++;
                var numStart = i;
                while (i < rtf.Length && char.IsDigit(rtf[i])) i++;
                int? param = null;
                if (i > numStart && int.TryParse(rtf[numStart..i], out int pv))
                    param = neg ? -pv : pv;
                // Consume trailing space delimiter
                if (i < rtf.Length && rtf[i] == ' ') i++;

                switch (word)
                {
                    case "par":
                        FlushRun();
                        result.Add(RtfRun.LineBreak);
                        break;
                    case "pard":
                    case "plain":
                        FlushRun();
                        bold = italic = underline = strikeout = false;
                        colorIdx = 0; fontIdx = 0; halfPts = 0;
                        break;
                    case "b":
                        FlushRun();
                        bold = param != 0;   // \b1 or \b (toggle on); \b0 off
                        if (param == null) bold = true;
                        break;
                    case "i":
                        FlushRun();
                        italic = param != 0;
                        if (param == null) italic = true;
                        break;
                    case "ul":
                        FlushRun();
                        underline = param != 0;
                        if (param == null) underline = true;
                        break;
                    case "ulnone":
                        FlushRun();
                        underline = false;
                        break;
                    case "strike":
                        FlushRun();
                        strikeout = param != 0;
                        if (param == null) strikeout = true;
                        break;
                    case "cf":
                        FlushRun();
                        colorIdx = param ?? 0;
                        break;
                    case "fs":
                        FlushRun();
                        halfPts = param ?? 0;
                        break;
                    case "f":
                        FlushRun();
                        fontIdx = param ?? 0;
                        break;
                    case "line":
                        FlushRun();
                        result.Add(RtfRun.LineBreak);
                        break;
                    case "tab":
                        textBuf.Append('\t');
                        break;
                    // Ignore all other control words.
                }
                continue;
            }

            // Plain character — only add if we are inside the body (depth >= 1).
            if (c != '\r' && c != '\n')
                textBuf.Append(c);
            i++;
        }

        FlushRun();
        return result;
    }

    // ── Inherited helpers ─────────────────────────────────────────────────────

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

    // ── Private run record ────────────────────────────────────────────────────

    private sealed record RtfRun(
        string Text,
        bool   Bold,
        bool   Italic,
        bool   Underline,
        bool   Strikeout,
        Color  Color,
        string FontFamily,
        int    FontSize,
        bool   IsLineBreak = false)
    {
        public static readonly RtfRun LineBreak =
            new(string.Empty, false, false, false, false, Color.Empty, string.Empty, 0, true);
    }
}

// ── Supporting enums ──────────────────────────────────────────────────────────

public enum RichTextBoxScrollBars
{
    None, Horizontal, Vertical, Both,
    ForcedHorizontal, ForcedVertical, ForcedBoth
}

[Flags]
public enum RichTextBoxFinds
{
    None       = 0,
    WholeWord  = 2,
    MatchCase  = 4,
    Reverse    = 8,
    NoHighlight = 16,
}

public enum RichTextBoxSelectionTypes { Empty = 0, Text = 1, Object = 2, MultiChar = 4, MultiObject = 8 }

public enum RichTextBoxStreamType { RichText, PlainText, RichNoOleObjs, TextTextOleObjs, UnicodePlainText }
