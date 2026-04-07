using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

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
        Font = new Font("Arial", 12);
    }

    #region Properties

    /// <summary>
    /// Gets or sets whether pressing ENTER creates a new line
    /// </summary>
    public bool AcceptsReturn
    {
        get => _acceptsReturn;
        set => _acceptsReturn = value;
    }

    /// <summary>
    /// Gets or sets whether pressing TAB moves focus or inserts a tab
    /// </summary>
    public bool AcceptsTab
    {
        get => _acceptsTab;
        set => _acceptsTab = value;
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
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets the number of lines in the text
    /// </summary>
    public virtual int Lines => Text.Split('\n').Length;

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
        set => _modified = value;
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
            if (ReadOnly) return;

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
        if (ReadOnly) return;

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
        if (ReadOnly) return;

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
        if (ReadOnly || _selectionLength == 0) return;

        Copy();
        SaveUndoState();
        SelectedText = string.Empty;
    }

    /// <summary>
    /// Pastes from clipboard
    /// </summary>
    public void Paste()
    {
        if (ReadOnly) return;

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
        return BorderStyle switch
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
