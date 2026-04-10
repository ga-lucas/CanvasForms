using Canvas.Windows.Forms.Drawing;

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

    private int _measureRequestId;

    // Selection state
    protected int _selectionStart = 0;
    protected int _selectionLength = 0;
    protected int _caretPosition = 0;

    // Selection helpers
    private bool _isSelecting;
    private int _selectionAnchor;

    // Scrolling
    protected int _scrollOffsetX = 0;
    protected int _scrollOffsetY = 0;
    private bool _acceptsReturn = false;
    private bool _acceptsTab = false;
    private bool _shortcutsEnabled = true;

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

    /// <summary>
    /// Gets or sets a value indicating whether the standard shortcut commands are enabled.
    /// (Ctrl+A/C/X/V/Z)
    /// </summary>
    public bool ShortcutsEnabled
    {
        get => _shortcutsEnabled;
        set => _shortcutsEnabled = value;
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
            _caretPosition = Math.Max(0, Math.Min(Text.Length, _selectionStart + _selectionLength));
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
            _caretPosition = Math.Max(0, Math.Min(Text.Length, _selectionStart + _selectionLength));
            Invalidate();
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        // Ensure caret/selection are always within range after programmatic Text changes.
        var len = Text?.Length ?? 0;
        _selectionStart = Math.Max(0, Math.Min(len, _selectionStart));
        _selectionLength = Math.Max(0, Math.Min(len - _selectionStart, _selectionLength));
        _caretPosition = Math.Max(0, Math.Min(len, _caretPosition));

        // Prime JS text measurement cache so caret placement and scroll calculations
        // are accurate even when Text is set programmatically (initial values).
        var requestId = System.Threading.Interlocked.Increment(ref _measureRequestId);
        _ = MeasureTextForCacheAsync(requestId);

        base.OnTextChanged(e);
    }

    protected internal override void OnGotFocus(EventArgs e)
    {
        var requestId = System.Threading.Interlocked.Increment(ref _measureRequestId);
        _ = MeasureTextForCacheAsync(requestId);
        base.OnGotFocus(e);
    }

    private async Task MeasureTextForCacheAsync(int requestId)
    {
        var measureService = (Parent as Form)?.TextMeasurementService;
        if (measureService == null) return;

        var displayText = GetDisplayText();
        if (string.IsNullOrEmpty(displayText)) return;

        try
        {
            await measureService.MeasureTextAsync(displayText, Font.Family, (int)Font.Size);

            var caret = Math.Max(0, Math.Min(_caretPosition, displayText.Length));
            if (caret > 0)
            {
                await measureService.MeasureTextAsync(displayText.Substring(0, caret), Font.Family, (int)Font.Size);
            }
        }
        catch
        {
            // Best-effort cache priming; rendering will fall back to estimation.
        }
        finally
        {
            // Only refresh if this is the latest measurement request and we're focused.
            if (requestId == System.Threading.Volatile.Read(ref _measureRequestId)
                && Parent is Form form
                && form.FocusedControl == this)
            {
                Invalidate();
            }
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
    /// Returns the text to render, accounting for password masking or any other
    /// display transformation. Override in derived classes (e.g. TextBox applies
    /// CharacterCasing and PasswordChar; MaskedTextBox applies the mask pattern).
    /// </summary>
    protected virtual string GetDisplayText() => Text ?? string.Empty;

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
    protected override bool IsInputChar(char charCode)
    {
        // Override in derived classes for special character handling
        return !char.IsControl(charCode) || charCode == '\t' || charCode == '\r' || charCode == '\n';
    }

    /// <summary>
    /// Checks if a key is an input key
    /// </summary>
    protected override bool IsInputKey(Keys keyData)
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

    #region Painting

    /// <summary>
    /// Paints the text box control
    /// </summary>
    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);
        var borderWidth = GetBorderWidth();

        // Check if we have focus
        var hasFocus = Parent is Form form && form.FocusedControl == this;

        // Draw background
        DrawBackground(g, bounds);

        // Draw border
        DrawBorder(g, bounds, hasFocus);

        // Get display text
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

        // Clip text/caret to the text area so scrolled content doesn't draw outside the control.
        // Note: Graphics.SetClip must respect the current TranslateTransform applied by Form.
        g.Save();
        g.SetClip(textBounds);

        // Draw text
        if (!string.IsNullOrEmpty(displayText))
        {
            DrawTextContent(g, displayText, textBounds, hasFocus, measureService);
        }

        // Draw caret if focused
        if (hasFocus && Enabled && _selectionLength == 0)
        {
            DrawCaret(g, displayText, textBounds, measureService);
        }

        g.Restore();

        base.OnPaint(e);
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

        if (e.Button == MouseButtons.Left)
        {
            var displayText = GetDisplayText();
            var borderWidth = GetBorderWidth();
            const int textPadding = 3;
            var textBounds = new Rectangle(
                borderWidth + textPadding,
                borderWidth + textPadding,
                Width - (borderWidth * 2) - (textPadding * 2),
                Height - (borderWidth * 2) - (textPadding * 2)
            );

            var measureService = (Parent as Form)?.TextMeasurementService;

            if (TryGetCharIndexFromMouse(new Point(e.X, e.Y), textBounds, displayText, measureService, out var index))
            {
                _caretPosition = index;
                _selectionAnchor = index;
                _selectionStart = index;
                _selectionLength = 0;
                _isSelecting = true;

                // Prime measurement cache for the new caret position so the caret lands correctly
                // even when this click is the first interaction after focus/text changes.
                var requestId = System.Threading.Interlocked.Increment(ref _measureRequestId);
                _ = MeasureTextForCacheAsync(requestId);

                Invalidate();
            }
        }

        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseMove(e);
            return;
        }

        if (_isSelecting)
        {
            var displayText = GetDisplayText();
            var borderWidth = GetBorderWidth();
            const int textPadding = 3;
            var textBounds = new Rectangle(
                borderWidth + textPadding,
                borderWidth + textPadding,
                Width - (borderWidth * 2) - (textPadding * 2),
                Height - (borderWidth * 2) - (textPadding * 2)
            );

            var measureService = (Parent as Form)?.TextMeasurementService;

            if (TryGetCharIndexFromMouse(new Point(e.X, e.Y), textBounds, displayText, measureService, out var index))
            {
                _caretPosition = index;

                var start = Math.Min(_selectionAnchor, index);
                var end = Math.Max(_selectionAnchor, index);
                _selectionStart = start;
                _selectionLength = end - start;
                Invalidate();
            }
        }

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        _isSelecting = false;
        base.OnMouseUp(e);
    }

    protected virtual bool TryGetCharIndexFromMouse(Point pt, Rectangle textBounds, string displayText, TextMeasurementService? measureService, out int index)
    {
        index = 0;

        // WinForms behavior: clicking within the control (including empty padding areas)
        // moves the caret to the nearest position. Only reject points completely outside.
        if (pt.X < 0 || pt.X >= Width || pt.Y < 0 || pt.Y >= Height)
        {
            return false;
        }

        // Clamp to the text layout bounds so we can still compute a sensible caret position
        // when clicking in the padding area.
        var clamped = new Point(
            Math.Clamp(pt.X, textBounds.X, Math.Max(textBounds.X, textBounds.Right - 1)),
            Math.Clamp(pt.Y, textBounds.Y, Math.Max(textBounds.Y, textBounds.Bottom - 1))
        );

        // Prefer the measurement service + displayText, since caret placement needs to match
        // exactly how we draw text on the canvas.
        if (measureService != null)
        {
            var relativeX = clamped.X - textBounds.X + _scrollOffsetX;
            var relativeY = clamped.Y - textBounds.Y + _scrollOffsetY;

            if (!_multiline)
            {
                index = GetInsertionIndexFromX(displayText, relativeX, measureService);
                index = Math.Max(0, Math.Min(Text.Length, index));
                return true;
            }

            var lines = displayText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0)
            {
                index = 0;
                return true;
            }

            var lineHeight = (int)_font.Size + 4;
            var lineNumber = lineHeight > 0 ? Math.Clamp(relativeY / lineHeight, 0, lines.Length - 1) : 0;
            var lineText = lines[lineNumber];

            var posInLine = GetInsertionIndexFromX(lineText, relativeX, measureService);
            index = GetFirstCharIndexFromLine(lineNumber) + posInLine;
            index = Math.Max(0, Math.Min(Text.Length, index));
            return true;
        }

        index = GetCharIndexFromPosition(clamped);
        index = Math.Max(0, Math.Min(Text.Length, index));
        return true;
    }

    private int GetInsertionIndexFromX(string text, int x, TextMeasurementService measureService)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        if (x <= 0) return 0;

        var fullWidth = measureService.MeasureTextEstimate(text, _font.Family, (int)_font.Size);
        if (x >= fullWidth) return text.Length;

        // Find the first prefix whose width is >= x.
        int low = 0;
        int high = text.Length;
        while (low < high)
        {
            int mid = (low + high) / 2;
            int w = mid == 0 ? 0 : measureService.MeasureTextEstimate(text.Substring(0, mid), _font.Family, (int)_font.Size);
            if (w < x) low = mid + 1;
            else high = mid;
        }

        var idx = Math.Clamp(low, 0, text.Length);

        // Apply a half-character threshold so clicks to the right half of a glyph place the caret after it.
        if (idx > 0)
        {
            int wPrev = idx - 1 == 0 ? 0 : measureService.MeasureTextEstimate(text.Substring(0, idx - 1), _font.Family, (int)_font.Size);
            int wIdx = measureService.MeasureTextEstimate(text.Substring(0, idx), _font.Family, (int)_font.Size);
            double midpoint = (wPrev + wIdx) / 2.0;
            if (x < midpoint)
            {
                idx--;
            }
        }

        return idx;
    }

    #endregion

    #region Keyboard Input

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (!Enabled)
        {
            base.OnKeyDown(e);
            return;
        }

        // Handle editing/navigation keys here (not KeyPress) because browsers often suppress
        // keypress for non-printable keys (Backspace, Delete, Enter, Tab).
        var handled = false;

        // Shortcuts (WinForms-like)
        if (!_shortcutsEnabled && e.Control &&
            (e.KeyCode == Keys.A || e.KeyCode == Keys.C || e.KeyCode == Keys.X ||
             e.KeyCode == Keys.V || e.KeyCode == Keys.Z))
        {
            e.Handled = true;
            base.OnKeyDown(e);
            return;
        }

        if (e.Control)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    SelectAll();
                    e.Handled = true;
                    return;
                case Keys.C:
                    Copy();
                    e.Handled = true;
                    return;
                case Keys.X:
                    Cut();
                    e.Handled = true;
                    return;
                case Keys.V:
                    Paste();
                    e.Handled = true;
                    return;
                case Keys.Z:
                    Undo();
                    e.Handled = true;
                    return;
            }
        }

        switch (e.KeyCode)
        {
            case Keys.Tab:
                if (_acceptsTab && !_readOnly)
                {
                    InsertText("\t");
                    handled = true;
                }
                break;

            case Keys.Enter:
                if (_acceptsReturn && _multiline && !_readOnly)
                {
                    InsertText(Environment.NewLine);
                    handled = true;
                }
                break;

            case Keys.Left:
                if (e.Control)
                {
                    MoveCaretTo(GetPreviousWordPosition(), e.Shift);
                }
                else
                {
                    MoveCaretBy(-1, e.Shift);
                }
                handled = true;
                Invalidate();
                break;

            case Keys.Right:
                if (e.Control)
                {
                    MoveCaretTo(GetNextWordPosition(), e.Shift);
                }
                else
                {
                    MoveCaretBy(1, e.Shift);
                }
                handled = true;
                Invalidate();
                break;

            case Keys.Home:
                MoveCaretTo(0, e.Shift);
                handled = true;
                Invalidate();
                break;

            case Keys.End:
                MoveCaretTo(Text.Length, e.Shift);
                handled = true;
                Invalidate();
                break;

            case Keys.Back:
                if (!_readOnly)
                {
                    if (_selectionLength > 0)
                    {
                        SaveUndoState();
                        SelectedText = string.Empty;
                    }
                    else if (_caretPosition > 0)
                    {
                        SaveUndoState();

                        var newCaret = _caretPosition - 1;
                        var newText = Text.Remove(newCaret, 1);

                        // Keep caret/selection in sync *before* setting Text.
                        // Setting Text triggers OnTextChanged which clamps indices,
                        // so updating after can cause a double-step at end-of-text.
                        _caretPosition = newCaret;
                        _selectionAnchor = newCaret;
                        _selectionStart = newCaret;
                        _selectionLength = 0;

                        Text = newText;
                        _modified = true;
                    }
                    handled = true;
                    Invalidate();
                }
                break;

            case Keys.Delete:
                if (!_readOnly)
                {
                    if (_selectionLength > 0)
                    {
                        SaveUndoState();
                        SelectedText = string.Empty;
                    }
                    else if (_caretPosition < Text.Length)
                    {
                        SaveUndoState();

                        var newText = Text.Remove(_caretPosition, 1);
                        _selectionAnchor = _caretPosition;
                        _selectionStart = _caretPosition;
                        _selectionLength = 0;

                        Text = newText;
                        _modified = true;
                    }
                    handled = true;
                    Invalidate();
                }
                break;
        }

        if (handled)
        {
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void MoveCaretBy(int delta, bool extendSelection)
    {
        var newCaret = Math.Max(0, Math.Min(Text.Length, _caretPosition + delta));
        MoveCaretTo(newCaret, extendSelection);
    }

    private void MoveCaretTo(int newCaret, bool extendSelection)
    {
        newCaret = Math.Max(0, Math.Min(Text.Length, newCaret));

        if (extendSelection)
        {
            if (_selectionLength == 0)
            {
                _selectionAnchor = _caretPosition;
            }

            _caretPosition = newCaret;
            var start = Math.Min(_selectionAnchor, _caretPosition);
            var end = Math.Max(_selectionAnchor, _caretPosition);
            _selectionStart = start;
            _selectionLength = end - start;
        }
        else
        {
            _caretPosition = newCaret;
            _selectionAnchor = newCaret;
            _selectionStart = newCaret;
            _selectionLength = 0;
        }
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

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        if (_readOnly || !Enabled)
        {
            base.OnKeyPress(e);
            return;
        }

        var c = e.KeyChar;

        if (!char.IsControl(c))
        {
            InsertText(c.ToString());
            e.Handled = true;
            return;
        }

        base.OnKeyPress(e);
    }

    private void InsertText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (_maxLength > 0)
        {
            var currentLength = Text.Length - _selectionLength;
            var remaining = _maxLength - currentLength;
            if (remaining <= 0) return;
            if (text.Length > remaining)
            {
                text = text.Substring(0, remaining);
            }
        }

        SaveUndoState();

        if (_selectionLength > 0)
        {
            Text = Text.Remove(_selectionStart, _selectionLength);
            _caretPosition = _selectionStart;
            _selectionLength = 0;
        }

        Text = Text.Insert(_caretPosition, text);
        _caretPosition += text.Length;
        _selectionStart = _caretPosition;
        _selectionLength = 0;
        _modified = true;
        OnTextChanged(EventArgs.Empty);
        Invalidate();
    }

    /// <summary>
    /// Draws the background of the control
    /// </summary>
    protected virtual void DrawBackground(Graphics g, Rectangle bounds)
    {
        var bgColor = Enabled ? BackColor : Color.FromArgb(240, 240, 240);
        using var bgBrush = new SolidBrush(bgColor);
        g.FillRectangle(bgBrush, bounds);
    }

    /// <summary>
    /// Draws the border of the control
    /// </summary>
    protected virtual void DrawBorder(Graphics g, Rectangle bounds, bool hasFocus)
    {
        switch (_borderStyle)
        {
            case BorderStyle.FixedSingle:
            case BorderStyle.Fixed3D:
                {
                    var borderColor = hasFocus ? Color.FromArgb(0, 120, 215) : Color.FromArgb(122, 122, 122);
                    using var pen = new Pen(borderColor);
                    g.DrawRectangle(pen, bounds);
                }
                break;
        }
    }

    /// <summary>
    /// Draws the text content - override in derived classes for custom rendering
    /// </summary>
    protected virtual void DrawTextContent(Graphics g, string displayText, Rectangle textBounds, bool hasFocus, TextMeasurementService? measureService)
    {
        var textColor = Enabled ? ForeColor : Color.FromArgb(109, 109, 109);

        if (_multiline)
        {
            DrawMultilineText(g, displayText, textBounds, textColor, hasFocus, measureService);
        }
        else
        {
            DrawSingleLineText(g, displayText, textBounds, textColor, hasFocus, measureService);
        }
    }

    /// <summary>
    /// Draws single-line text with selection
    /// </summary>
    protected virtual void DrawSingleLineText(Graphics g, string displayText, Rectangle textBounds, Color textColor, bool hasFocus, TextMeasurementService? measureService)
    {
        var textX = textBounds.X - _scrollOffsetX;
        var textY = textBounds.Y;

        // Draw selection if any
        if (_selectionLength > 0 && hasFocus && measureService != null)
        {
            DrawTextWithSelection(g, displayText, textX, textY, textColor, measureService);
        }
        else
        {
            g.DrawString(displayText, _font, textColor, textX, textY);
        }
    }

    /// <summary>
    /// Draws multiline text with selection
    /// </summary>
    protected virtual void DrawMultilineText(Graphics g, string displayText, Rectangle textBounds, Color textColor, bool hasFocus, TextMeasurementService? measureService)
    {
        var lines = displayText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var lineHeight = (int)_font.Size + 4;
        var y = textBounds.Y - _scrollOffsetY;

        for (int i = 0; i < lines.Length; i++)
        {
            if (y + lineHeight < textBounds.Y) // Above visible area
            {
                y += lineHeight;
                continue;
            }

            if (y > textBounds.Bottom) // Below visible area
                break;

            var line = lines[i];
            var x = textBounds.X - _scrollOffsetX;

            // Check if this line contains selection
            if (hasFocus && _selectionLength > 0 && measureService != null)
            {
                var lineStart = GetFirstCharIndexFromLine(i);
                var lineEnd = lineStart + line.Length;
                var selStart = _selectionStart;
                var selEnd = _selectionStart + _selectionLength;

                if (selEnd > lineStart && selStart < lineEnd)
                {
                    // This line has selection
                    var selLineStart = Math.Max(0, selStart - lineStart);
                    var selLineEnd = Math.Min(line.Length, selEnd - lineStart);

                    var textBefore = selLineStart > 0 ? line.Substring(0, selLineStart) : "";
                    var textSelected = line.Substring(selLineStart, selLineEnd - selLineStart);
                    var textAfter = selLineEnd < line.Length ? line.Substring(selLineEnd) : "";

                    var widthBefore = measureService.MeasureTextEstimate(textBefore, _font.Family, (int)_font.Size);
                    var widthSelected = measureService.MeasureTextEstimate(textSelected, _font.Family, (int)_font.Size);

                    // Draw selection background
                    using var selBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                    g.FillRectangle(selBrush, x + widthBefore, y, widthSelected, lineHeight);

                    // Draw text parts
                    if (!string.IsNullOrEmpty(textBefore))
                        g.DrawString(textBefore, _font, textColor, x, y);

                    if (!string.IsNullOrEmpty(textSelected))
                        g.DrawString(textSelected, _font, Color.White, x + widthBefore, y);

                    if (!string.IsNullOrEmpty(textAfter))
                        g.DrawString(textAfter, _font, textColor, x + widthBefore + widthSelected, y);

                    y += lineHeight;
                    continue;
                }
            }

            // No selection on this line
            g.DrawString(line, _font, textColor, x, y);
            y += lineHeight;
        }
    }

    /// <summary>
    /// Draws text with selection highlighting
    /// </summary>
    protected virtual void DrawTextWithSelection(Graphics g, string displayText, int textX, int textY, Color textColor, TextMeasurementService measureService)
    {
        if (_selectionStart >= displayText.Length)
        {
            g.DrawString(displayText, _font, textColor, textX, textY);
            return;
        }

        var selEnd = Math.Min(_selectionStart + _selectionLength, displayText.Length);
        var textBeforeSelection = _selectionStart > 0 ? displayText.Substring(0, _selectionStart) : "";
        var selectedText = displayText.Substring(_selectionStart, selEnd - _selectionStart);
        var textAfterSelection = selEnd < displayText.Length ? displayText.Substring(selEnd) : "";

        var widthBefore = measureService.MeasureTextEstimate(textBeforeSelection, _font.Family, (int)_font.Size);
        var widthSelected = measureService.MeasureTextEstimate(selectedText, _font.Family, (int)_font.Size);

        var borderWidth = GetBorderWidth();
        const int textPadding = 3;
        var selectionHeight = Math.Max(0, Height - (borderWidth * 2) - (textPadding * 2));

        // Draw background first, then draw the full line once (avoids per-segment rounding drift),
        // and finally draw the selected substring on top in white.
        using var selBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
        g.FillRectangle(selBrush, textX + widthBefore, textY, widthSelected, selectionHeight);

        g.DrawString(displayText, _font, textColor, textX, textY);

        if (!string.IsNullOrEmpty(selectedText))
        {
            g.DrawString(selectedText, _font, Color.White, textX + widthBefore, textY);
        }
    }

    /// <summary>
    /// Draws the caret (cursor) at the current position
    /// </summary>
    protected virtual void DrawCaret(Graphics g, string displayText, Rectangle textBounds, TextMeasurementService? measureService)
    {
        if (measureService == null) return;

        if (_multiline)
        {
            // Multiline caret
            var lineNumber = GetLineFromCharIndex(_caretPosition);
            var lineStart = GetFirstCharIndexFromLine(lineNumber);
            var posInLine = _caretPosition - lineStart;
            var lines = Lines;

            if (lineNumber < lines.Length)
            {
                var line = lines[lineNumber];
                var textBeforeCaret = posInLine > 0 && posInLine <= line.Length ? line.Substring(0, posInLine) : "";
                var width = measureService.MeasureTextEstimate(textBeforeCaret, _font.Family, (int)_font.Size);
                var lineHeight = (int)_font.Size + 4;

                var caretX = textBounds.X + width - _scrollOffsetX;
                var caretY = textBounds.Y + (lineNumber * lineHeight) - _scrollOffsetY;

                // Only draw if visible
                if (caretX >= textBounds.X && caretX <= textBounds.Right &&
                    caretY >= textBounds.Y && caretY + lineHeight <= textBounds.Bottom)
                {
                    using var pen = new Pen(Color.Black, 1);
                    g.DrawLine(pen, caretX, caretY, caretX, caretY + lineHeight);
                }
            }
        }
        else
        {
            // Single line caret
            var textBeforeCaret = _caretPosition > 0 && _caretPosition <= displayText.Length
                ? displayText.Substring(0, _caretPosition)
                : "";

            var width = measureService.MeasureTextEstimate(textBeforeCaret, _font.Family, (int)_font.Size);
            var caretX = textBounds.X + width - _scrollOffsetX;

            // Only draw if visible
            if (caretX >= textBounds.X && caretX <= textBounds.Right)
            {
                using var pen = new Pen(Color.Black, 1);
                g.DrawLine(pen, caretX, textBounds.Y, caretX, textBounds.Bottom);
            }
        }
    }

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
