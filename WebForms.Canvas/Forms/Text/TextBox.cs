using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms text box control
/// </summary>
public class TextBox : TextBoxBase
{
    private char _passwordChar = '\0';
    private HorizontalAlignment _textAlign = HorizontalAlignment.Left;
    private CharacterCasing _characterCasing = CharacterCasing.Normal;
    private bool _useSystemPasswordChar = false;
    private string[] _autoCompleteCustomSource = Array.Empty<string>();
    private AutoCompleteMode _autoCompleteMode = AutoCompleteMode.None;
    private AutoCompleteSource _autoCompleteSource = AutoCompleteSource.None;
    private bool _shortcutsEnabled = true;
    private AutoCompletePanel? _autoCompletePanel;

    public TextBox()
    {
        Width = 100;
        Height = 23;
        Font = new Font("Arial", 12);
        _autoCompletePanel = new AutoCompletePanel(this);
    }

    #region Properties

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
                OnTextAlignChanged(EventArgs.Empty);
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

    /// <summary>
    /// Gets or sets the auto-complete custom source (stub)
    /// </summary>
    public string[] AutoCompleteCustomSource
    {
        get => _autoCompleteCustomSource;
        set => _autoCompleteCustomSource = value ?? Array.Empty<string>();
    }

    /// <summary>
    /// Gets or sets the auto-complete mode (stub)
    /// </summary>
    public AutoCompleteMode AutoCompleteMode
    {
        get => _autoCompleteMode;
        set => _autoCompleteMode = value;
    }

    /// <summary>
    /// Gets or sets the auto-complete source (stub)
    /// </summary>
    public AutoCompleteSource AutoCompleteSource
    {
        get => _autoCompleteSource;
        set => _autoCompleteSource = value;
    }

    /// <summary>
    /// Gets or sets whether shortcuts are enabled
    /// </summary>
    public bool ShortcutsEnabled
    {
        get => _shortcutsEnabled;
        set => _shortcutsEnabled = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether text in the text box is read-only
    /// Overridden to ensure single-line default behavior
    /// </summary>
    public new bool Multiline
    {
        get => base.Multiline;
        set
        {
            if (base.Multiline != value)
            {
                base.Multiline = value;
                // Adjust height for single-line
                if (!value && Height != 23)
                {
                    Height = 23;
                }
            }
        }
    }

    /// <summary>
    /// Gets the preferred height for a text box (stub)
    /// </summary>
    public int PreferredHeight => 23;

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the TextAlign property changes
    /// </summary>
    public event EventHandler? TextAlignChanged;

    #endregion

    #region Methods

    /// <summary>
    /// Raises the TextAlignChanged event
    /// </summary>
    protected virtual void OnTextAlignChanged(EventArgs e)
    {
        TextAlignChanged?.Invoke(this, e);
    }

    #endregion

    #region Painting

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
            var textX = CalculateTextX(textBounds, displayText, measureService);
            var textY = textBounds.Y;

            // Draw selection if any
            if (_selectionLength > 0 && hasFocus && measureService != null)
            {
                DrawTextWithSelection(g, displayText, textX, textY, textColor, measureService);
            }
            else
            {
                g.DrawString(displayText, Font, textColor, textX, textY);
            }
        }

        // Draw caret if focused
        if (hasFocus && Enabled && _selectionLength == 0)
        {
            DrawCaret(g, displayText, textBounds, measureService);
        }

        base.OnPaint(e);
    }

    /// <summary>
    /// Paint without autocomplete (called by Form to paint control layer)
    /// </summary>
    internal void PaintWithoutAutoComplete(PaintEventArgs e)
    {
        OnPaint(e);
    }

    /// <summary>
    /// Paint only the autocomplete panel (called by Form to paint on top)
    /// </summary>
    internal void PaintAutoCompleteOnly(PaintEventArgs e)
    {
        if (_autoCompletePanel?.IsVisible == true)
        {
            _autoCompletePanel.Paint(e.Graphics);
        }
    }

    /// <summary>
    /// Gets whether autocomplete is visible
    /// </summary>
    internal bool HasVisibleAutoComplete => _autoCompletePanel?.IsVisible == true;

    /// <summary>
    /// Gets the bounds of the autocomplete panel relative to this TextBox
    /// </summary>
    internal Rectangle GetAutoCompletePanelBounds()
    {
        return _autoCompletePanel?.GetBounds() ?? Rectangle.Empty;
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

    private int CalculateTextX(Rectangle textBounds, string displayText, TextMeasurementService? measureService)
    {
        if (_textAlign == HorizontalAlignment.Left || measureService == null)
        {
            return textBounds.X - _scrollOffsetX;
        }

        var textWidth = measureService.MeasureTextEstimate(displayText, Font.Family, (int)Font.Size);

        return _textAlign switch
        {
            HorizontalAlignment.Center => textBounds.X + (textBounds.Width - textWidth) / 2,
            HorizontalAlignment.Right => textBounds.Right - textWidth,
            _ => textBounds.X - _scrollOffsetX
        };
    }

    private void DrawTextWithSelection(Graphics g, string displayText, int textX, int textY, Color textColor, TextMeasurementService measureService)
    {
        if (_selectionStart >= displayText.Length)
        {
            g.DrawString(displayText, Font, textColor, textX, textY);
            return;
        }

        var selEnd = Math.Min(_selectionStart + _selectionLength, displayText.Length);
        var textBeforeSelection = _selectionStart > 0 ? displayText.Substring(0, _selectionStart) : "";
        var selectedText = displayText.Substring(_selectionStart, selEnd - _selectionStart);
        var textAfterSelection = selEnd < displayText.Length ? displayText.Substring(selEnd) : "";

        var widthBefore = measureService.MeasureTextEstimate(textBeforeSelection, Font.Family, (int)Font.Size);
        var widthSelected = measureService.MeasureTextEstimate(selectedText, Font.Family, (int)Font.Size);

        // Draw selection background
        using var selBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
        g.FillRectangle(selBrush, textX + widthBefore, textY, widthSelected, Height - 6);

        // Draw text parts
        if (!string.IsNullOrEmpty(textBeforeSelection))
            g.DrawString(textBeforeSelection, Font, textColor, textX, textY);

        if (!string.IsNullOrEmpty(selectedText))
            g.DrawString(selectedText, Font, Color.White, textX + widthBefore, textY);

        if (!string.IsNullOrEmpty(textAfterSelection))
            g.DrawString(textAfterSelection, Font, textColor, textX + widthBefore + widthSelected, textY);
    }

    private void DrawCaret(Graphics g, string displayText, Rectangle textBounds, TextMeasurementService? measureService)
    {
        if (measureService == null) return;

        var textBeforeCaret = _caretPosition > 0 && _caretPosition <= displayText.Length
            ? displayText.Substring(0, _caretPosition)
            : "";

        var width = measureService.MeasureTextEstimate(textBeforeCaret, Font.Family, (int)Font.Size);
        var caretX = CalculateTextX(textBounds, displayText, measureService) + width;

        // Only draw if visible
        if (caretX >= textBounds.X && caretX <= textBounds.Right)
        {
            using var pen = new Pen(Color.Black, 1);
            g.DrawLine(pen, caretX, textBounds.Y, caretX, textBounds.Bottom);
        }
    }

    private void UpdateScrollPosition(string displayText, TextMeasurementService measureService, int visibleWidth)
    {
        if (string.IsNullOrEmpty(displayText) || _textAlign != HorizontalAlignment.Left)
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

    private void UpdateAutoComplete()
    {
        if (_autoCompleteMode == AutoCompleteMode.None || 
            _autoCompleteSource == AutoCompleteSource.None ||
            string.IsNullOrWhiteSpace(Text))
        {
            _autoCompletePanel?.Hide();
            return;
        }

        // Get suggestions based on source
        var suggestions = GetAutoCompleteSuggestions(Text);

        if (suggestions.Any())
        {
            _autoCompletePanel?.Show(suggestions, Text);
        }
        else
        {
            _autoCompletePanel?.Hide();
        }
    }

    private IEnumerable<string> GetAutoCompleteSuggestions(string text)
    {
        if (_autoCompleteSource == AutoCompleteSource.CustomSource)
        {
            return _autoCompleteCustomSource
                .Where(s => s.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s)
                .Take(20);
        }

        // Other sources would be implemented here (FileSystem, HistoryList, etc.)
        return Enumerable.Empty<string>();
    }

    internal void AcceptAutoCompleteSuggestion(string suggestion)
    {
        if (_autoCompleteMode == AutoCompleteMode.Suggest || 
            _autoCompleteMode == AutoCompleteMode.SuggestAppend)
        {
            Text = suggestion;
            _caretPosition = suggestion.Length;
            _selectionStart = _caretPosition;
            _selectionLength = 0;
            Invalidate();
        }
    }

    #endregion

    #region Keyboard Input

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

            var charToInsert = _characterCasing switch
            {
                CharacterCasing.Upper => char.ToUpper(c),
                CharacterCasing.Lower => char.ToLower(c),
                _ => c
            };

            Text = Text.Insert(_caretPosition, charToInsert.ToString());
            _caretPosition++;
            Modified = true;
            OnTextChanged(EventArgs.Empty);
            UpdateAutoComplete();
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

        // Let autocomplete panel handle keys first if it's visible
        if (_autoCompletePanel?.IsVisible == true)
        {
            if (_autoCompletePanel.HandleKeyDown(e))
            {
                e.Handled = true;
                return;
            }
        }

        var handled = false;

        // Check if shortcuts are enabled for Ctrl combinations
        if (!_shortcutsEnabled && e.Control && 
            (e.KeyCode == Keys.A || e.KeyCode == Keys.C || e.KeyCode == Keys.X || 
             e.KeyCode == Keys.V || e.KeyCode == Keys.Z))
        {
            e.Handled = true;
            base.OnKeyDown(e);
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Left:
                if (e.Control)
                {
                    // Ctrl+Left: Move to previous word
                    _caretPosition = GetPreviousWordPosition();
                }
                else if (_caretPosition > 0)
                {
                    _caretPosition--;
                }

                if (!e.Shift)
                {
                    _selectionStart = _caretPosition;
                    _selectionLength = 0;
                }
                handled = true;
                Invalidate();
                break;

            case Keys.Right:
                if (e.Control)
                {
                    // Ctrl+Right: Move to next word
                    _caretPosition = GetNextWordPosition();
                }
                else if (_caretPosition < Text.Length)
                {
                    _caretPosition++;
                }

                if (!e.Shift)
                {
                    _selectionStart = _caretPosition;
                    _selectionLength = 0;
                }
                handled = true;
                Invalidate();
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

    private int GetPreviousWordPosition()
    {
        if (_caretPosition == 0) return 0;

        var pos = _caretPosition - 1;

        // Skip whitespace
        while (pos > 0 && char.IsWhiteSpace(Text[pos]))
            pos--;

        // Skip word characters
        while (pos > 0 && !char.IsWhiteSpace(Text[pos - 1]))
            pos--;

        return pos;
    }

    private int GetNextWordPosition()
    {
        if (_caretPosition >= Text.Length) return Text.Length;

        var pos = _caretPosition;

        // Skip current word
        while (pos < Text.Length && !char.IsWhiteSpace(Text[pos]))
            pos++;

        // Skip whitespace
        while (pos < Text.Length && char.IsWhiteSpace(Text[pos]))
            pos++;

        return pos;
    }

    #endregion

    #region Mouse Input

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseDown(e);
            return;
        }

        // Check if clicking on autocomplete panel
        if (_autoCompletePanel?.HandleMouseDown(e) == true)
        {
            return;
        }

        // Close autocomplete if clicking outside
        if (_autoCompletePanel?.IsVisible == true)
        {
            var panelBounds = _autoCompletePanel.GetBounds();
            if (e.Y < panelBounds.Y || e.Y >= panelBounds.Bottom)
            {
                _autoCompletePanel.Hide();
            }
        }

        var borderWidth = GetBorderWidth();
        const int textPadding = 3;
        var textBounds = new Rectangle(
            borderWidth + textPadding,
            borderWidth + textPadding,
            Width - (borderWidth * 2) - (textPadding * 2),
            Height - (borderWidth * 2) - (textPadding * 2)
        );

        // Check if click is within text area
        if (e.X >= textBounds.X && e.X < textBounds.Right &&
            e.Y >= textBounds.Y && e.Y < textBounds.Bottom)
        {
            var displayText = GetDisplayText();
            var measureService = (Parent as Form)?.TextMeasurementService;

            if (measureService != null && !string.IsNullOrEmpty(displayText))
            {
                // Calculate click position relative to text start (accounting for scroll and alignment)
                var textX = CalculateTextX(textBounds, displayText, measureService);
                var clickX = e.X - textX;

                // Find the character position closest to the click
                var closestPosition = 0;
                var minDistance = int.MaxValue;

                for (int i = 0; i <= displayText.Length; i++)
                {
                    var textUpToPosition = i > 0 ? displayText.Substring(0, i) : "";
                    var width = measureService.MeasureTextEstimate(textUpToPosition, Font.Family, (int)Font.Size);

                    var distance = Math.Abs(width - clickX);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPosition = i;
                    }
                    else
                    {
                        // Distance is increasing, we've passed the closest point
                        break;
                    }
                }

                _caretPosition = closestPosition;
                _selectionStart = closestPosition;
                _selectionLength = 0;
                Invalidate();
            }
        }

        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        // Let autocomplete panel handle mouse move
        _autoCompletePanel?.HandleMouseMove(e);

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        // Notify autocomplete panel
        _autoCompletePanel?.HandleMouseUp();

        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        // Let autocomplete panel handle mouse wheel if visible
        if (_autoCompletePanel?.HandleMouseWheel(e) == true)
        {
            return;
        }

        base.OnMouseWheel(e);
    }

    #endregion
}

#region Enums

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

/// <summary>
/// Specifies the auto-complete mode (stub)
/// </summary>
public enum AutoCompleteMode
{
    None = 0,
    Suggest = 1,
    Append = 2,
    SuggestAppend = 3
}

/// <summary>
/// Specifies the auto-complete source (stub)
/// </summary>
public enum AutoCompleteSource
{
    None = 0,
    FileSystem = 1,
    HistoryList = 2,
    RecentlyUsedList = 3,
    AllUrl = 4,
    AllSystemSources = 5,
    FileSystemDirectories = 6,
    CustomSource = 7,
    ListItems = 8
}

#endregion
