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

    /// <summary>
    /// Draws single-line text with TextBox-specific features (alignment and auto-scroll)
    /// </summary>
    protected override void DrawSingleLineText(Graphics g, string displayText, Rectangle textBounds, System.Drawing.Color textColor, bool hasFocus, TextMeasurementService? measureService)
    {
        // Update scroll to keep caret visible
        if (measureService != null && !string.IsNullOrEmpty(displayText))
        {
            UpdateScrollPosition(displayText, measureService, textBounds.Width);
        }

        // Calculate text X position based on alignment
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

    internal void HideAutoCompletePanel()
    {
        _autoCompletePanel?.Hide();
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

    #endregion

    protected override string GetDisplayText()
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

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        UpdateAutoComplete();
    }

    /// <summary>
    /// Measures the current text asynchronously to populate the cache
    /// This ensures accurate measurements are available for rendering
    /// </summary>
    private async Task MeasureTextForCacheAsync()
    {
        var measureService = FindForm()?.TextMeasurementService;
        if (measureService == null) return;

        var displayText = GetDisplayText();
        if (string.IsNullOrEmpty(displayText)) return;

        try
        {
            // Measure full text
            await measureService.MeasureTextAsync(displayText, Font.Family, (int)Font.Size);

            // Also measure text before caret for accurate caret positioning
            if (_caretPosition > 0 && _caretPosition <= displayText.Length)
            {
                var textBeforeCaret = displayText.Substring(0, _caretPosition);
                await measureService.MeasureTextAsync(textBeforeCaret, Font.Family, (int)Font.Size);
            }
        }
        catch
        {
            // Measurement failed, will use estimation fallback during render
        }
    }

    #region Keyboard Input

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        // Let TextBoxBase handle character insertion.
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

        // Delegate core editing/navigation/shortcuts to TextBoxBase.
        base.OnKeyDown(e);

        if (e.Handled)
        {
            UpdateAutoComplete();
        }
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

    protected internal override void OnMouseWheel(MouseEventArgs e)
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
