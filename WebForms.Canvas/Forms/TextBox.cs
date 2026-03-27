using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class TextBox : Control
{
    private int _caretPosition = 0;
    private int _selectionStart = 0;
    private int _selectionLength = 0;

    public TextBox()
    {
        Width = 100;
        Height = 20;
        BackColor = Color.White;
        ForeColor = Color.Black;
        Text = string.Empty;
    }

    // Better text width estimation (closer to Arial 12px)
    private int EstimateTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        int width = 0;
        foreach (char c in text)
        {
            if (c == ' ')
                width += 3; // Space is narrow
            else if (char.IsUpper(c))
                width += 9; // Uppercase is wider
            else if (c == 'i' || c == 'l' || c == 'I' || c == 't' || c == 'j')
                width += 4; // Narrow characters
            else if (c == 'm' || c == 'w' || c == 'M' || c == 'W')
                width += 10; // Wide characters
            else
                width += 7; // Average character
        }
        return width;
    }

    public int MaxLength { get; set; } = 32767;
    public bool ReadOnly { get; set; } = false;
    public char PasswordChar { get; set; } = '\0';

    // Selection properties
    public int SelectionStart
    {
        get => _selectionStart;
        set
        {
            _selectionStart = Math.Max(0, Math.Min(Text.Length, value));
            Invalidate();
        }
    }

    public int SelectionLength
    {
        get => _selectionLength;
        set
        {
            _selectionLength = Math.Max(0, Math.Min(Text.Length - _selectionStart, value));
            Invalidate();
        }
    }

    public string SelectedText
    {
        get
        {
            if (_selectionLength > 0)
                return Text.Substring(_selectionStart, _selectionLength);
            return string.Empty;
        }
        set
        {
            if (_selectionLength > 0)
            {
                Text = Text.Remove(_selectionStart, _selectionLength);
                if (!string.IsNullOrEmpty(value))
                {
                    Text = Text.Insert(_selectionStart, value);
                    _caretPosition = _selectionStart + value.Length;
                }
                else
                {
                    _caretPosition = _selectionStart;
                }
                _selectionLength = 0;
                OnTextChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public event EventHandler? TextChanged;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);

        // Check if we have focus (parent form's focused control is us)
        var hasFocus = Parent is Form form && form.FocusedControl == this;

        // Draw background
        var bgColor = Enabled ? BackColor : Color.FromArgb(240, 240, 240);
        g.FillRectangle(new SolidBrush(bgColor), bounds);

        // Draw border
        var borderColor = hasFocus ? Color.FromArgb(0, 120, 215) : Color.FromArgb(122, 122, 122);
        g.DrawRectangle(new Pen(borderColor), bounds);

        var displayText = GetDisplayText();

        // Draw selection highlight if there's a selection
        if (hasFocus && _selectionLength > 0 && !string.IsNullOrEmpty(displayText))
        {
            var textBeforeSelection = displayText.Substring(0, _selectionStart);
            var selectedText = displayText.Substring(_selectionStart, 
                Math.Min(_selectionLength, displayText.Length - _selectionStart));

            var selStartX = 3 + EstimateTextWidth(textBeforeSelection);
            var selWidth = EstimateTextWidth(selectedText);

            g.FillRectangle(new SolidBrush(Color.FromArgb(0, 120, 215)), 
                new Rectangle(selStartX, 3, selWidth, Height - 6));

            // Draw selected text in white
            g.DrawString(selectedText, selStartX, 3, Color.White);
        }

        // Draw text
        if (!string.IsNullOrEmpty(displayText))
        {
            var textColor = Enabled ? ForeColor : Color.FromArgb(109, 109, 109);

            // Draw text before selection
            if (_selectionStart > 0)
            {
                var beforeSelection = displayText.Substring(0, Math.Min(_selectionStart, displayText.Length));
                g.DrawString(beforeSelection, 3, 3, textColor);
            }

            // Draw text after selection
            if (_selectionStart + _selectionLength < displayText.Length)
            {
                var afterStart = _selectionStart + _selectionLength;
                var afterSelection = displayText.Substring(afterStart);
                var textBeforeAfter = displayText.Substring(0, afterStart);
                var afterX = 3 + EstimateTextWidth(textBeforeAfter);
                g.DrawString(afterSelection, afterX, 3, textColor);
            }

            // If no selection, draw all text
            if (_selectionLength == 0)
            {
                g.DrawString(displayText, 3, 3, textColor);
            }
        }

        // Draw static caret if focused and no selection
        if (hasFocus && Enabled && _selectionLength == 0)
        {
            var textBeforeCaret = displayText.Substring(0, Math.Min(_caretPosition, displayText.Length));
            var caretX = 3 + EstimateTextWidth(textBeforeCaret);
            g.DrawLine(new Pen(Color.Black), caretX, 3, caretX, Height - 6);
        }

        base.OnPaint(e);
    }

    private string GetDisplayText()
    {
        if (PasswordChar != '\0' && !string.IsNullOrEmpty(Text))
        {
            return new string(PasswordChar, Text.Length);
        }
        return Text;
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled)
        {
            // Calculate caret position from click
            var clickX = e.X - 3;
            var newPosition = Math.Max(0, Math.Min(Text.Length, clickX / 7));

            _caretPosition = newPosition;

            // Clear selection on single click
            _selectionStart = newPosition;
            _selectionLength = 0;

            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        if (!Enabled || ReadOnly)
        {
            base.OnKeyPress(e);
            return;
        }

        var ch = e.KeyChar;

        // Handle printable characters
        if (char.IsControl(ch))
        {
            base.OnKeyPress(e);
            return;
        }

        // Delete selection first if exists
        if (_selectionLength > 0)
        {
            Text = Text.Remove(_selectionStart, _selectionLength);
            _caretPosition = _selectionStart;
            _selectionLength = 0;
        }

        if (Text.Length < MaxLength)
        {
            Text = Text.Insert(_caretPosition, ch.ToString());
            _caretPosition++;
            OnTextChanged(EventArgs.Empty);
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

        // Handle Ctrl+C (Copy)
        if (e.Control && e.KeyCode == Keys.C)
        {
            if (_selectionLength > 0)
            {
                // Copy to clipboard would require JSInterop
                // For now, just store in a static field (simplified clipboard)
                ClipboardText = SelectedText;
            }
            base.OnKeyDown(e);
            return;
        }

        // Handle Ctrl+X (Cut)
        if (e.Control && e.KeyCode == Keys.X)
        {
            if (!ReadOnly && _selectionLength > 0)
            {
                ClipboardText = SelectedText;
                SelectedText = string.Empty; // Delete selection
                _caretPosition = _selectionStart;
            }
            base.OnKeyDown(e);
            return;
        }

        // Handle Ctrl+V (Paste)
        if (e.Control && e.KeyCode == Keys.V)
        {
            if (!ReadOnly && !string.IsNullOrEmpty(ClipboardText))
            {
                // Delete selection if exists
                if (_selectionLength > 0)
                {
                    Text = Text.Remove(_selectionStart, _selectionLength);
                    _caretPosition = _selectionStart;
                    _selectionLength = 0;
                }

                // Insert clipboard text
                Text = Text.Insert(_caretPosition, ClipboardText);
                _caretPosition += ClipboardText.Length;
                OnTextChanged(EventArgs.Empty);
                Invalidate();
            }
            base.OnKeyDown(e);
            return;
        }

        // Handle Ctrl+A (Select All)
        if (e.Control && e.KeyCode == Keys.A)
        {
            _selectionStart = 0;
            _selectionLength = Text.Length;
            _caretPosition = Text.Length;
            Invalidate();
            base.OnKeyDown(e);
            return;
        }

        switch (e.KeyCode)
        {
            case Keys.Back:
                if (!ReadOnly)
                {
                    if (_selectionLength > 0)
                    {
                        // Delete selection
                        Text = Text.Remove(_selectionStart, _selectionLength);
                        _caretPosition = _selectionStart;
                        _selectionLength = 0;
                        OnTextChanged(EventArgs.Empty);
                        Invalidate();
                    }
                    else if (_caretPosition > 0)
                    {
                        Text = Text.Remove(_caretPosition - 1, 1);
                        _caretPosition--;
                        OnTextChanged(EventArgs.Empty);
                        Invalidate();
                    }
                }
                break;

            case Keys.Delete:
                if (!ReadOnly)
                {
                    if (_selectionLength > 0)
                    {
                        // Delete selection
                        Text = Text.Remove(_selectionStart, _selectionLength);
                        _caretPosition = _selectionStart;
                        _selectionLength = 0;
                        OnTextChanged(EventArgs.Empty);
                        Invalidate();
                    }
                    else if (_caretPosition < Text.Length)
                    {
                        Text = Text.Remove(_caretPosition, 1);
                        OnTextChanged(EventArgs.Empty);
                        Invalidate();
                    }
                }
                break;

            case Keys.Left:
                if (e.Shift)
                {
                    // Extend selection left
                    if (_selectionLength == 0)
                    {
                        _selectionStart = _caretPosition;
                    }

                    if (_caretPosition > 0)
                    {
                        _caretPosition--;

                        if (_caretPosition < _selectionStart)
                        {
                            _selectionLength = _selectionStart - _caretPosition;
                            _selectionStart = _caretPosition;
                        }
                        else
                        {
                            _selectionLength--;
                        }
                    }
                }
                else
                {
                    // Clear selection and move left
                    if (_selectionLength > 0)
                    {
                        _caretPosition = _selectionStart;
                        _selectionLength = 0;
                    }
                    else if (_caretPosition > 0)
                    {
                        _caretPosition--;
                    }
                }
                Invalidate();
                break;

            case Keys.Right:
                if (e.Shift)
                {
                    // Extend selection right
                    if (_selectionLength == 0)
                    {
                        _selectionStart = _caretPosition;
                    }

                    if (_caretPosition < Text.Length)
                    {
                        _caretPosition++;
                        _selectionLength = _caretPosition - _selectionStart;
                    }
                }
                else
                {
                    // Clear selection and move right
                    if (_selectionLength > 0)
                    {
                        _caretPosition = _selectionStart + _selectionLength;
                        _selectionLength = 0;
                    }
                    else if (_caretPosition < Text.Length)
                    {
                        _caretPosition++;
                    }
                }
                Invalidate();
                break;

            case Keys.Home:
                if (e.Shift)
                {
                    // Select from current position to start
                    if (_selectionLength == 0)
                    {
                        _selectionStart = 0;
                        _selectionLength = _caretPosition;
                    }
                    else
                    {
                        _selectionLength = _selectionStart + _selectionLength;
                        _selectionStart = 0;
                    }
                    _caretPosition = 0;
                }
                else
                {
                    _caretPosition = 0;
                    _selectionLength = 0;
                }
                Invalidate();
                break;

            case Keys.End:
                if (e.Shift)
                {
                    // Select from current position to end
                    if (_selectionLength == 0)
                    {
                        _selectionStart = _caretPosition;
                    }
                    _selectionLength = Text.Length - _selectionStart;
                    _caretPosition = Text.Length;
                }
                else
                {
                    _caretPosition = Text.Length;
                    _selectionLength = 0;
                }
                Invalidate();
                break;
        }

        base.OnKeyDown(e);
    }

    // Simple static clipboard (shared across all TextBox instances)
    // In a real implementation, this would use JavaScript clipboard API
    private static string ClipboardText = string.Empty;

    protected virtual void OnTextChanged(EventArgs e)
    {
        TextChanged?.Invoke(this, e);
    }
}
