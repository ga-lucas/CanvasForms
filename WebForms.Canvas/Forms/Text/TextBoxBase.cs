using WebForms.Canvas.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Base class for text editing controls (TextBox, RichTextBox)
/// </summary>
public abstract class TextBoxBase : Control
{
    private int _maxLength = 32767;
    private bool _readOnly = false;
    private bool _multiline = false;
    private bool _wordWrap = true;
    private bool _hideSelection = true;
    private bool _modified = false;
    private BorderStyle _borderStyle = BorderStyle.Fixed3D;
    private ScrollBars _scrollBars = ScrollBars.None;
    private bool _autoSize = true;
    private Font _font;

    // Selection state
    protected int _selectionStart = 0;
    protected int _selectionLength = 0;
    protected int _caretPosition = 0;

    // Scrolling
    protected int _scrollOffsetX = 0;
    protected int _scrollOffsetY = 0;
    private bool _acceptsReturn = false;
    private bool _acceptsTab = false;

    // Undo/Redo
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();
    private const int MaxUndoLevels = 100;

    protected TextBoxBase()
    {
        SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint, true);
        TabStop = true;
        BackColor = Color.White;
        ForeColor = Color.Black;
        _font = new Font("Arial", 12);
    }

    #region Properties

    /// <summary>
    /// Gets or sets the font of the text
    /// </summary>
    public new Font Font
    {
        get => _font;
        set
        {
            if (_font != value)
            {
                _font = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the control automatically sizes (stub)
    /// </summary>
    public new bool AutoSize
    {
        get => _autoSize;
        set
        {
            if (_autoSize != value)
            {
                _autoSize = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets which scroll bars should appear (stub)
    /// </summary>
    public ScrollBars ScrollBars
    {
        get => _scrollBars;
        set
        {
            if (_scrollBars != value)
            {
                _scrollBars = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether pressing ENTER creates a new line
    /// </summary>
    public bool AcceptsReturn
    {
        get => _acceptsReturn;
        set
        {
            if (_acceptsReturn != value)
            {
                _acceptsReturn = value;
                OnAcceptsTabChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether pressing TAB moves focus or inserts a tab
    /// </summary>
    public bool AcceptsTab
    {
        get => _acceptsTab;
        set
        {
            if (_acceptsTab != value)
            {
                _acceptsTab = value;
                OnAcceptsTabChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets the border style
    /// </summary>
    public BorderStyle BorderStyle
    {
        get => _borderStyle;
        set
        {
            if (_borderStyle != value)
            {
                _borderStyle = value;
                OnBorderStyleChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the user can undo the previous operation
    /// </summary>
    public virtual bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// Gets or sets whether the selection is hidden when the control loses focus
    /// </summary>
    public bool HideSelection
    {
        get => _hideSelection;
        set
        {
            if (_hideSelection != value)
            {
                _hideSelection = value;
                OnHideSelectionChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets the lines of text in the control as a string array
    /// </summary>
    public virtual string[] Lines
    {
        get
        {
            if (string.IsNullOrEmpty(Text))
                return Array.Empty<string>();

            // Split by line breaks, handling different line ending styles
            return Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }
        set
        {
            if (value == null || value.Length == 0)
            {
                Text = string.Empty;
            }
            else
            {
                Text = string.Join(Environment.NewLine, value);
            }
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of characters
    /// </summary>
    public virtual int MaxLength
    {
        get => _maxLength;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            _maxLength = value;
        }
    }

    /// <summary>
    /// Gets or sets whether the text is modified since last set
    /// </summary>
    public bool Modified
    {
        get => _modified;
        set
        {
            if (_modified != value)
            {
                _modified = value;
                OnModifiedChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether this is a multiline text box
    /// </summary>
    public virtual bool Multiline
    {
        get => _multiline;
        set
        {
            if (_multiline != value)
            {
                _multiline = value;
                OnMultilineChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the text is read-only
    /// </summary>
    public bool ReadOnly
    {
        get => _readOnly;
        set
        {
            if (_readOnly != value)
            {
                _readOnly = value;
                OnReadOnlyChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the currently selected text
    /// </summary>
    public virtual string SelectedText
    {
        get
        {
            if (_selectionLength > 0 && _selectionStart < Text.Length)
            {
                var length = Math.Min(_selectionLength, Text.Length - _selectionStart);
                return Text.Substring(_selectionStart, length);
            }
            return string.Empty;
        }
        set
        {
            if (_readOnly) return;

            SaveUndoState();

            if (_selectionLength > 0)
            {
                Text = Text.Remove(_selectionStart, _selectionLength);
            }

            if (!string.IsNullOrEmpty(value))
            {
                if (_maxLength > 0 && Text.Length + value.Length > _maxLength)
                {
                    value = value.Substring(0, _maxLength - Text.Length);
                }

                Text = Text.Insert(_selectionStart, value);
                _caretPosition = _selectionStart + value.Length;
            }
            else
            {
                _caretPosition = _selectionStart;
            }

            _selectionLength = 0;
            _modified = true;
            OnTextChanged(EventArgs.Empty);
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the number of characters selected
    /// </summary>
    public virtual int SelectionLength
    {
        get => _selectionLength;
        set
        {
            _selectionLength = Math.Max(0, Math.Min(Text.Length - _selectionStart, value));
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the starting point of selected text
    /// </summary>
    public virtual int SelectionStart
    {
        get => _selectionStart;
        set
        {
            _selectionStart = Math.Max(0, Math.Min(Text.Length, value));
            Invalidate();
        }
    }

    /// <summary>
    /// Gets the length of the text
    /// </summary>
    public virtual int TextLength => Text.Length;

    /// <summary>
    /// Gets or sets whether text wraps in multiline mode
    /// </summary>
    public bool WordWrap
    {
        get => _wordWrap;
        set
        {
            if (_wordWrap != value)
            {
                _wordWrap = value;
                Invalidate();
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Appends text to the current text
    /// </summary>
    public void AppendText(string text)
    {
        if (_readOnly) return;

        SaveUndoState();
        Text += text;
        _caretPosition = Text.Length;
        _modified = true;
        OnTextChanged(EventArgs.Empty);
        Invalidate();
    }

    /// <summary>
    /// Clears all text
    /// </summary>
    public void Clear()
    {
        if (_readOnly) return;

        SaveUndoState();
        Text = string.Empty;
        _selectionStart = 0;
        _selectionLength = 0;
        _caretPosition = 0;
        _modified = true;
        OnTextChanged(EventArgs.Empty);
        Invalidate();
    }

    /// <summary>
    /// Clears undo buffer
    /// </summary>
    public void ClearUndo()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    /// <summary>
    /// Copies selection to clipboard
    /// </summary>
    public void Copy()
    {
        if (_selectionLength > 0)
        {
            // In a real implementation, this would use the system clipboard
            // For now, we'll just store it in a static field
            ClipboardText = SelectedText;
        }
    }

    /// <summary>
    /// Cuts selection to clipboard
    /// </summary>
    public void Cut()
    {
        if (_readOnly || _selectionLength == 0) return;

        Copy();
        SaveUndoState();
        SelectedText = string.Empty;
    }

    /// <summary>
    /// Pastes from clipboard
    /// </summary>
    public void Paste()
    {
        if (_readOnly) return;

        if (!string.IsNullOrEmpty(ClipboardText))
        {
            SelectedText = ClipboardText;
        }
    }

    /// <summary>
    /// Selects all text
    /// </summary>
    public void SelectAll()
    {
        _selectionStart = 0;
        _selectionLength = Text.Length;
        Invalidate();
    }

    /// <summary>
    /// Selects a range of text
    /// </summary>
    public void Select(int start, int length)
    {
        _selectionStart = Math.Max(0, Math.Min(Text.Length, start));
        _selectionLength = Math.Max(0, Math.Min(Text.Length - _selectionStart, length));
        _caretPosition = _selectionStart + _selectionLength;
        Invalidate();
    }

    /// <summary>
    /// Undoes the last operation
    /// </summary>
    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            _redoStack.Push(Text);
            Text = _undoStack.Pop();
            _modified = true;
            OnTextChanged(EventArgs.Empty);
            Invalidate();
        }
    }

    /// <summary>
    /// Scrolls contents to the caret position
    /// </summary>
    public void ScrollToCaret()
    {
        // Implemented by derived classes
        Invalidate();
    }

    /// <summary>
    /// Deselects all text
    /// </summary>
    public void DeselectAll()
    {
        _selectionStart = 0;
        _selectionLength = 0;
        Invalidate();
    }

    /// <summary>
    /// Gets the character index of the first character of a given line
    /// </summary>
    public int GetFirstCharIndexFromLine(int lineNumber)
    {
        var lines = Lines;
        if (lineNumber < 0 || lineNumber >= lines.Length)
            return -1;

        int charIndex = 0;
        for (int i = 0; i < lineNumber; i++)
        {
            charIndex += lines[i].Length + Environment.NewLine.Length;
        }
        return charIndex;
    }

    /// <summary>
    /// Gets the line number from a character position
    /// </summary>
    public int GetLineFromCharIndex(int index)
    {
        if (index < 0 || index > Text.Length)
            return 0;

        var textUpToIndex = Text.Substring(0, index);
        return textUpToIndex.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length - 1;
    }

    /// <summary>
    /// Gets the character index of the first character of the current line
    /// </summary>
    public int GetFirstCharIndexOfCurrentLine()
    {
        var lineNumber = GetLineFromCharIndex(_caretPosition);
        return GetFirstCharIndexFromLine(lineNumber);
    }

    /// <summary>
    /// Gets the pixel position of the character at the specified index
    /// </summary>
    public Point GetPositionFromCharIndex(int index)
    {
        if (index < 0 || index > Text.Length)
            return new Point(0, 0);

        // Get the form's text measurement service
        var measureService = (Parent as Form)?.TextMeasurementService;
        if (measureService == null || _font == null)
            return new Point(0, 0);

        var borderWidth = GetBorderWidth();
        const int textPadding = 3;

        if (!_multiline)
        {
            // Single-line: calculate horizontal position only
            var textBeforeIndex = index > 0 ? Text.Substring(0, index) : "";
            var width = measureService.MeasureTextEstimate(textBeforeIndex, _font.Family, (int)_font.Size);

            return new Point(
                borderWidth + textPadding + width - _scrollOffsetX,
                borderWidth + textPadding
            );
        }
        else
        {
            // Multi-line: calculate both line number and position within line
            var lineNumber = GetLineFromCharIndex(index);
            var lines = Lines;

            if (lineNumber >= lines.Length)
                return new Point(borderWidth + textPadding, borderWidth + textPadding);

            // Get character position within the line
            var firstCharOfLine = GetFirstCharIndexFromLine(lineNumber);
            var positionInLine = index - firstCharOfLine;

            // Calculate horizontal position
            var textBeforeInLine = positionInLine > 0 && positionInLine <= lines[lineNumber].Length
                ? lines[lineNumber].Substring(0, positionInLine)
                : "";
            var width = measureService.MeasureTextEstimate(textBeforeInLine, _font.Family, (int)_font.Size);

            // Calculate vertical position (line number * line height)
            var lineHeight = (int)_font.Size + 4; // Font size + padding
            var y = borderWidth + textPadding + (lineNumber * lineHeight) - _scrollOffsetY;

            return new Point(
                borderWidth + textPadding + width - _scrollOffsetX,
                y
            );
        }
    }

    /// <summary>
    /// Gets the character index from a pixel position
    /// </summary>
    public int GetCharIndexFromPosition(Point pt)
    {
        // Get the form's text measurement service
        var measureService = (Parent as Form)?.TextMeasurementService;
        if (measureService == null || _font == null)
            return 0;

        var borderWidth = GetBorderWidth();
        const int textPadding = 3;

        // Adjust for border and scroll offset
        var relativeX = pt.X - borderWidth - textPadding + _scrollOffsetX;
        var relativeY = pt.Y - borderWidth - textPadding + _scrollOffsetY;

        if (!_multiline)
        {
            // Single-line: find character at X position
            if (string.IsNullOrEmpty(Text))
                return 0;

            // Binary search for closest character position
            int closestIndex = 0;
            int minDistance = int.MaxValue;

            for (int i = 0; i <= Text.Length; i++)
            {
                var textUpToIndex = i > 0 ? Text.Substring(0, i) : "";
                var width = measureService.MeasureTextEstimate(textUpToIndex, _font.Family, (int)_font.Size);
                var distance = Math.Abs(width - relativeX);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
                else
                {
                    // Distance is increasing, we've passed the closest point
                    break;
                }
            }

            return closestIndex;
        }
        else
        {
            // Multi-line: determine line number, then position within line
            var lineHeight = (int)_font.Size + 4;
            var lineNumber = Math.Max(0, relativeY / lineHeight);

            var lines = Lines;
            if (lineNumber >= lines.Length)
                lineNumber = lines.Length - 1;

            if (lineNumber < 0 || lines.Length == 0)
                return 0;

            // Get the line text
            var lineText = lines[lineNumber];
            if (string.IsNullOrEmpty(lineText))
            {
                // Empty line - return start of line
                return GetFirstCharIndexFromLine(lineNumber);
            }

            // Find character position within the line
            int closestPosInLine = 0;
            int minDistance = int.MaxValue;

            for (int i = 0; i <= lineText.Length; i++)
            {
                var textUpToPos = i > 0 ? lineText.Substring(0, i) : "";
                var width = measureService.MeasureTextEstimate(textUpToPos, _font.Family, (int)_font.Size);
                var distance = Math.Abs(width - relativeX);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPosInLine = i;
                }
                else
                {
                    break;
                }
            }

            // Convert line position to absolute character index
            return GetFirstCharIndexFromLine(lineNumber) + closestPosInLine;
        }
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Saves current state for undo
    /// </summary>
    protected void SaveUndoState()
    {
        if (_undoStack.Count == 0 || _undoStack.Peek() != Text)
        {
            _undoStack.Push(Text);
            _redoStack.Clear();

            // Limit undo stack size
            while (_undoStack.Count > MaxUndoLevels)
            {
                var temp = _undoStack.Reverse().ToList();
                temp.RemoveAt(temp.Count - 1);
                _undoStack.Clear();
                foreach (var item in temp.AsEnumerable().Reverse())
                {
                    _undoStack.Push(item);
                }
            }
        }
    }

    /// <summary>
    /// Gets the border width based on border style
    /// </summary>
    protected int GetBorderWidth()
    {
        return _borderStyle switch
        {
            BorderStyle.None => 0,
            BorderStyle.FixedSingle => 1,
            BorderStyle.Fixed3D => 2,
            _ => 0
        };
    }

    /// <summary>
    /// Checks if a character should be accepted as input
    /// </summary>
    protected virtual bool IsInputChar(char charCode)
    {
        // Override in derived classes for special character handling
        return !char.IsControl(charCode) || charCode == '\t' || charCode == '\r' || charCode == '\n';
    }

    /// <summary>
    /// Checks if a key is an input key (stub)
    /// </summary>
    protected virtual bool IsInputKey(Keys keyData)
    {
        // Stub: Determines if a key is an input key
        var key = keyData & ~(Keys.Shift | Keys.Control | Keys.Alt);

        switch (key)
        {
            case Keys.Tab:
                return _acceptsTab;
            case Keys.Enter:
                return _acceptsReturn && _multiline;
            case Keys.Left:
            case Keys.Right:
            case Keys.Up:
            case Keys.Down:
            case Keys.Home:
            case Keys.End:
            case Keys.PageUp:
            case Keys.PageDown:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Raises the AcceptsTabChanged event
    /// </summary>
    protected virtual void OnAcceptsTabChanged(EventArgs e)
    {
        AcceptsTabChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the BorderStyleChanged event
    /// </summary>
    protected virtual void OnBorderStyleChanged(EventArgs e)
    {
        BorderStyleChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the HideSelectionChanged event
    /// </summary>
    protected virtual void OnHideSelectionChanged(EventArgs e)
    {
        HideSelectionChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the ModifiedChanged event
    /// </summary>
    protected virtual void OnModifiedChanged(EventArgs e)
    {
        ModifiedChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the MultilineChanged event
    /// </summary>
    protected virtual void OnMultilineChanged(EventArgs e)
    {
        MultilineChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the ReadOnlyChanged event
    /// </summary>
    protected virtual void OnReadOnlyChanged(EventArgs e)
    {
        ReadOnlyChanged?.Invoke(this, e);
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when AcceptsTab property changes
    /// </summary>
    public event EventHandler? AcceptsTabChanged;

    /// <summary>
    /// Occurs when BorderStyle property changes
    /// </summary>
    public event EventHandler? BorderStyleChanged;

    /// <summary>
    /// Occurs when HideSelection property changes
    /// </summary>
    public event EventHandler? HideSelectionChanged;

    /// <summary>
    /// Occurs when Modified property changes
    /// </summary>
    public event EventHandler? ModifiedChanged;

    /// <summary>
    /// Occurs when Multiline property changes
    /// </summary>
    public event EventHandler? MultilineChanged;

    /// <summary>
    /// Occurs when ReadOnly property changes
    /// </summary>
    public event EventHandler? ReadOnlyChanged;

    #endregion

    #region Static Clipboard (Simple Implementation)

    // Simple static clipboard - in a real implementation this would use the system clipboard
    private static string ClipboardText { get; set; } = string.Empty;

    #endregion
}

/// <summary>
/// Specifies the border styles for a text box
/// </summary>
public enum BorderStyle
{
    None = 0,
    FixedSingle = 1,
    Fixed3D = 2
}

/// <summary>
/// Specifies which scroll bars will be visible on a control
/// </summary>
public enum ScrollBars
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    Both = 3
}
