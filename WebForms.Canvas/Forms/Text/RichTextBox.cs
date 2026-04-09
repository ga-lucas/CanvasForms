using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms RichTextBox control
/// </summary>
public class RichTextBox : TextBoxBase
{
    private bool _detectUrls = true;
    private bool _enableAutoDragDrop = false;
    private RichTextBoxScrollBars _scrollBars = RichTextBoxScrollBars.Both;
    private int _zoomFactor = 1;
    private string _rtf = string.Empty;

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
    /// RTF content — stored for compatibility, rendered as plain text
    /// </summary>
    public string Rtf
    {
        get => _rtf;
        set
        {
            _rtf = value ?? string.Empty;
            // Strip RTF tags for plain-text rendering
            Text = StripRtf(_rtf);
        }
    }

    public int ZoomFactor { get => _zoomFactor; set { _zoomFactor = Math.Max(1, value); Invalidate(); } }

    /// <summary>
    /// Appends text followed by a newline
    /// </summary>
    public void AppendText(string text)
    {
        Text += text;
        SelectionStart = Text.Length;
        Invalidate();
    }

    /// <summary>
    /// Clears the undo buffer
    /// </summary>
    public void ClearUndo() { /* stub */ }

    /// <summary>
    /// Selects a range of text
    /// </summary>
    public void Select(int start, int length)
    {
        SelectionStart = Math.Max(0, Math.Min(start, Text.Length));
        SelectionLength = Math.Max(0, Math.Min(length, Text.Length - SelectionStart));
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    /// <summary>
    /// Selects all text
    /// </summary>
    public void SelectAll()
    {
        Select(0, Text.Length);
    }

    /// <summary>
    /// Scrolls to the caret position
    /// </summary>
    public void ScrollToCaret() { /* stub */ }

    private static string StripRtf(string rtf)
    {
        // Minimal RTF stripper: remove {, }, and \word tokens
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
                if (i < rtf.Length && rtf[i] == '\\') { sb.Append('\\'); i++; }
                else if (i < rtf.Length && rtf[i] == '{') { sb.Append('{'); i++; }
                else if (i < rtf.Length && rtf[i] == '}') { sb.Append('}'); i++; }
                else if (i < rtf.Length && rtf[i] == '\n') { sb.Append('\n'); i++; }
                else if (i < rtf.Length && rtf[i] == '\'')
                {
                    // Hex encoded char \' xx
                    i++;
                    if (i + 1 < rtf.Length)
                    {
                        var hex = rtf.Substring(i, 2);
                        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int code))
                            sb.Append((char)code);
                        i += 2;
                    }
                }
                else { while (i < rtf.Length && rtf[i] != ' ' && rtf[i] != '\\' && rtf[i] != '{' && rtf[i] != '}') i++; if (i < rtf.Length && rtf[i] == ' ') i++; }
            }
            else if (c == '{' || c == '}') i++;
            else { sb.Append(c); i++; }
        }
        return sb.ToString().Trim();
    }
}

public enum RichTextBoxScrollBars { None, Horizontal, Vertical, Both, ForcedHorizontal, ForcedVertical, ForcedBoth }
