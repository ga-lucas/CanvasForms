using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

/// <summary>
/// Represents a Windows Forms text box control
/// </summary>
public class TextBox : TextBoxBase
{
    private char _passwordChar = '\0';
    private HorizontalAlignment _textAlign = HorizontalAlignment.Left;
    private CharacterCasing _characterCasing = CharacterCasing.Normal;
    private bool _useSystemPasswordChar = false;

    public TextBox()
    {
        Width = 100;
        Height = 23;
        Font = new Font("Arial", 12);
    }

    public Font Font { get; set; }

    /// <summary>
    /// Gets or sets the character used for password masking
    /// </summary>
    public char PasswordChar
    {
        get => _passwordChar;
        set
        {
            if (_passwordChar != value)
            {
                _passwordChar = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to use the system password character
    /// </summary>
    public bool UseSystemPasswordChar
    {
        get => _useSystemPasswordChar;
        set
        {
            if (_useSystemPasswordChar != value)
            {
                _useSystemPasswordChar = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the alignment of text
    /// </summary>
    public HorizontalAlignment TextAlign
    {
        get => _textAlign;
        set
        {
            if (_textAlign != value)
            {
                _textAlign = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the character casing
    /// </summary>
    public CharacterCasing CharacterCasing
    {
        get => _characterCasing;
        set
        {
            if (_characterCasing != value)
            {
                _characterCasing = value;
                Invalidate();
            }
        }
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);
        var borderWidth = GetBorderWidth();

        // Check if we have focus
        var hasFocus = Parent is Form form && form.FocusedControl == this;

        // Draw background
        var bgColor = Enabled ? BackColor : Color.FromArgb(240, 240, 240);
        using var bgBrush = new SolidBrush(bgColor);
        g.FillRectangle(bgBrush, bounds);

        // Draw border
        DrawBorder(g, bounds, hasFocus);

        // Get display text (with password masking if applicable)
        var displayText = GetDisplayText();

        const int textPadding = 3;
        var textBounds = new Rectangle(
            borderWidth + textPadding,
            borderWidth + textPadding,
            Width - (borderWidth * 2) - (textPadding * 2),
            Height - (borderWidth * 2) - (textPadding * 2)
        );

        // Get text measurement service
        var measureService = (Parent as Form)?.TextMeasurementService;

        if (measureService != null && !string.IsNullOrEmpty(displayText))
        {
            // Update scroll to keep caret visible
            UpdateScrollPosition(displayText, measureService, textBounds.Width);
        }

        // Draw text
        if (!string.IsNullOrEmpty(displayText))
        {
            var textColor = Enabled ? ForeColor : Color.FromArgb(109, 109, 109);
            var textX = textBounds.X - _scrollOffsetX;
            var textY = textBounds.Y;

            g.DrawString(displayText, Font, textColor, textX, textY);
        }

        // Draw caret if focused
        if (hasFocus && Enabled && _selectionLength == 0)
        {
            DrawCaret(g, displayText, textBounds, measureService);
        }

        base.OnPaint(e);
    }

    private void DrawBorder(Graphics g, Rectangle bounds, bool hasFocus)
    {
        switch (BorderStyle)
        {
            case BorderStyle.FixedSingle:
                {
                    var borderColor = hasFocus ? Color.FromArgb(0, 120, 215) : Color.FromArgb(122, 122, 122);
                    using var pen = new Pen(borderColor);
                    g.DrawRectangle(pen, bounds);
                }
                break;

            case BorderStyle.Fixed3D:
                {
                    var borderColor = hasFocus ? Color.FromArgb(0, 120, 215) : Color.FromArgb(122, 122, 122);
                    using var pen = new Pen(borderColor);
                    g.DrawRectangle(pen, bounds);
                }
                break;
        }
    }

    private void DrawCaret(Graphics g, string displayText, Rectangle textBounds, TextMeasurementService? measureService)
    {
        if (measureService == null) return;

        var textBeforeCaret = _caretPosition > 0 && _caretPosition <= displayText.Length
            ? displayText.Substring(0, _caretPosition)
            : "";

        var width = measureService.MeasureTextEstimate(textBeforeCaret, Font.Family, (int)Font.Size);
        var caretX = textBounds.X - _scrollOffsetX + width;

        // Only draw if visible
        if (caretX >= textBounds.X && caretX <= textBounds.Right)
        {
            using var pen = new Pen(Color.Black, 1);
            g.DrawLine(pen, caretX, textBounds.Y, caretX, textBounds.Bottom);
        }
    }

    private void UpdateScrollPosition(string displayText, TextMeasurementService measureService, int visibleWidth)
    {
        if (string.IsNullOrEmpty(displayText))
        {
            _scrollOffsetX = 0;
            return;
        }

        var textBeforeCaret = _caretPosition > 0 && _caretPosition <= displayText.Length
            ? displayText.Substring(0, _caretPosition)
            : "";

        var caretPixelPos = measureService.MeasureTextEstimate(textBeforeCaret, Font.Family, (int)Font.Size);
        var visibleCaretPos = caretPixelPos - _scrollOffsetX;

        const int margin = 5;

        // Scroll right if caret is beyond right edge
        if (visibleCaretPos > visibleWidth - margin)
        {
            _scrollOffsetX = caretPixelPos - visibleWidth + margin;
        }
        // Scroll left if caret is beyond left edge
        else if (visibleCaretPos < margin)
        {
            _scrollOffsetX = Math.Max(0, caretPixelPos - margin);
        }

        _scrollOffsetX = Math.Max(0, _scrollOffsetX);
    }

    private string GetDisplayText()
    {
        var text = Text ?? string.Empty;

        // Apply character casing
        text = _characterCasing switch
        {
            CharacterCasing.Upper => text.ToUpper(),
            CharacterCasing.Lower => text.ToLower(),
            _ => text
        };

        // Apply password masking
        if (_useSystemPasswordChar || _passwordChar != '\0')
        {
            var maskChar = _useSystemPasswordChar ? '●' : _passwordChar;
            return new string(maskChar, text.Length);
        }

        return text;
    }

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        if (ReadOnly || !Enabled)
        {
            base.OnKeyPress(e);
            return;
        }

        var c = e.KeyChar;

        // Handle backspace
        if (c == '\b')
        {
            if (_selectionLength > 0)
            {
                SaveUndoState();
                SelectedText = string.Empty;
            }
            else if (_caretPosition > 0)
            {
                SaveUndoState();
                Text = Text.Remove(_caretPosition - 1, 1);
                _caretPosition--;
                Modified = true;
                OnTextChanged(EventArgs.Empty);
            }
            e.Handled = true;
            Invalidate();
            return;
        }

        // Handle normal characters
        if (!char.IsControl(c))
        {
            if (MaxLength > 0 && Text.Length >= MaxLength && _selectionLength == 0)
            {
                e.Handled = true;
                return;
            }

            SaveUndoState();

            if (_selectionLength > 0)
            {
                Text = Text.Remove(_selectionStart, _selectionLength);
                _caretPosition = _selectionStart;
                _selectionLength = 0;
            }

            Text = Text.Insert(_caretPosition, c.ToString());
            _caretPosition++;
            Modified = true;
            OnTextChanged(EventArgs.Empty);
            e.Handled = true;
            Invalidate();
        }

        base.OnKeyPress(e);
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (!Enabled)
        {
            base.OnKeyDown(e);
            return;
        }

        var handled = false;

        switch (e.KeyCode)
        {
            case Keys.Left:
                if (_caretPosition > 0)
                {
                    _caretPosition--;
                    if (!e.Shift)
                    {
                        _selectionStart = _caretPosition;
                        _selectionLength = 0;
                    }
                    handled = true;
                    Invalidate();
                }
                break;

            case Keys.Right:
                if (_caretPosition < Text.Length)
                {
                    _caretPosition++;
                    if (!e.Shift)
                    {
                        _selectionStart = _caretPosition;
                        _selectionLength = 0;
                    }
                    handled = true;
                    Invalidate();
                }
                break;

            case Keys.Home:
                _caretPosition = 0;
                if (!e.Shift)
                {
                    _selectionStart = 0;
                    _selectionLength = 0;
                }
                handled = true;
                Invalidate();
                break;

            case Keys.End:
                _caretPosition = Text.Length;
                if (!e.Shift)
                {
                    _selectionStart = Text.Length;
                    _selectionLength = 0;
                }
                handled = true;
                Invalidate();
                break;

            case Keys.Delete:
                if (!ReadOnly)
                {
                    if (_selectionLength > 0)
                    {
                        SaveUndoState();
                        SelectedText = string.Empty;
                    }
                    else if (_caretPosition < Text.Length)
                    {
                        SaveUndoState();
                        Text = Text.Remove(_caretPosition, 1);
                        Modified = true;
                        OnTextChanged(EventArgs.Empty);
                    }
                    handled = true;
                    Invalidate();
                }
                break;

            case Keys.A:
                if (e.Control)
                {
                    SelectAll();
                    handled = true;
                }
                break;

            case Keys.C:
                if (e.Control)
                {
                    Copy();
                    handled = true;
                }
                break;

            case Keys.X:
                if (e.Control && !ReadOnly)
                {
                    Cut();
                    handled = true;
                }
                break;

            case Keys.V:
                if (e.Control && !ReadOnly)
                {
                    Paste();
                    handled = true;
                }
                break;

            case Keys.Z:
                if (e.Control && !ReadOnly)
                {
                    Undo();
                    handled = true;
                }
                break;
        }

        if (handled)
        {
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }
}

/// <summary>
/// Specifies the horizontal alignment of text
/// </summary>
public enum HorizontalAlignment
{
    Left = 0,
    Right = 1,
    Center = 2
}

/// <summary>
/// Specifies the case of characters in a TextBox
/// </summary>
public enum CharacterCasing
{
    Normal = 0,
    Upper = 1,
    Lower = 2
}
