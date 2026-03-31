using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public class TextBox : Control
{
    private int _caretPosition = 0;
    private int _selectionStart = 0;
    private int _selectionLength = 0;
    private int _scrollOffset = 0; // Horizontal scroll offset in pixels

    public TextBox()
    {
        Width = 100;
        Height = 20;
        BackColor = Color.White;
        ForeColor = Color.Black;
        Text = string.Empty;
        Font = new Font("Arial", 12);
    }

    public Font Font { get; set; }

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

        // Get text measurement service from parent form
        var measureService = (Parent as Form)?.TextMeasurementService;

        // Pre-measure text asynchronously to populate cache for next render
        if (measureService != null && !string.IsNullOrEmpty(displayText))
        {
            _ = PreMeasureTextSegments(displayText, measureService);
        }

        // Update scroll position to keep caret visible
        if (measureService != null && !string.IsNullOrEmpty(displayText))
        {
            UpdateScrollPosition(displayText, measureService);
        }
        else
        {
            // No measurement service or no text - reset scroll
            _scrollOffset = 0;
        }

        const int textPadding = 3;

        // Draw text using cached or estimated measurements with scroll offset
        if (!string.IsNullOrEmpty(displayText) && measureService != null)
        {
            var textColor = Enabled ? ForeColor : Color.FromArgb(109, 109, 109);

            // Calculate visible portion of text based on scroll offset
            var visibleStartIndex = 0;
            var visibleEndIndex = displayText.Length;

            // Find first visible character
            for (int i = 0; i < displayText.Length; i++)
            {
                var textUpToIndex = displayText.Substring(0, i);
                var width = GetCachedOrEstimatedWidth(textUpToIndex, measureService);

                if (width >= _scrollOffset)
                {
                    visibleStartIndex = i > 0 ? i - 1 : 0; // Include one character before for smoothness
                    break;
                }
            }

            // Find last visible character
            var maxVisibleWidth = _scrollOffset + (Width - textPadding * 2);
            for (int i = visibleStartIndex; i <= displayText.Length; i++)
            {
                var textUpToIndex = displayText.Substring(0, i);
                var width = GetCachedOrEstimatedWidth(textUpToIndex, measureService);

                if (width > maxVisibleWidth)
                {
                    visibleEndIndex = i;
                    break;
                }
            }

            // Only draw visible portion
            if (visibleStartIndex < visibleEndIndex)
            {
                if (_selectionLength > 0 && hasFocus)
                {
                    // Handle selection within visible range
                    var visibleText = displayText.Substring(visibleStartIndex, visibleEndIndex - visibleStartIndex);
                    var textBeforeVisible = displayText.Substring(0, visibleStartIndex);
                    var widthBeforeVisible = GetCachedOrEstimatedWidth(textBeforeVisible, measureService);
                    var visibleTextX = textPadding - _scrollOffset + widthBeforeVisible;

                    // For now, draw the visible portion and handle selection
                    // TODO: Properly handle selection highlighting in visible portion
                    g.DrawString(visibleText, Font, textColor, visibleTextX, textPadding);
                }
                else
                {
                    // No selection - draw visible text portion
                    var visibleText = displayText.Substring(visibleStartIndex, visibleEndIndex - visibleStartIndex);
                    var textBeforeVisible = displayText.Substring(0, visibleStartIndex);
                    var widthBeforeVisible = GetCachedOrEstimatedWidth(textBeforeVisible, measureService);
                    var visibleTextX = textPadding - _scrollOffset + widthBeforeVisible;

                    g.DrawString(visibleText, Font, textColor, visibleTextX, textPadding);
                }
            }
        }

        // Draw static caret if focused and no selection
        if (hasFocus && Enabled && _selectionLength == 0 && measureService != null)
        {
            var textBeforeCaret = string.IsNullOrEmpty(displayText) ? "" : 
                displayText.Substring(0, Math.Min(_caretPosition, displayText.Length));
            var caretX = textPadding - _scrollOffset + GetCachedOrEstimatedWidth(textBeforeCaret, measureService);

            // Only draw caret if it's within visible bounds
            if (caretX >= textPadding && caretX <= Width - textPadding)
            {
                g.DrawLine(new Pen(Color.Black, 1), caretX, textPadding, caretX, Height - (textPadding * 2));
            }
        }

        base.OnPaint(e);
    }

    // Update scroll position to keep caret visible within text area
    private void UpdateScrollPosition(string displayText, TextMeasurementService measureService)
    {
        const int textPadding = 3;
        var textAreaWidth = Width - (textPadding * 2);

        // If no text, reset scroll to 0
        if (string.IsNullOrEmpty(displayText))
        {
            _scrollOffset = 0;
            return;
        }

        // Calculate caret position in pixels
        var textBeforeCaret = _caretPosition > 0 && _caretPosition <= displayText.Length
            ? displayText.Substring(0, _caretPosition)
            : "";
        var caretPixelPosition = GetCachedOrEstimatedWidth(textBeforeCaret, measureService);

        // Calculate visible caret position (accounting for scroll)
        var visibleCaretPosition = caretPixelPosition - _scrollOffset;

        // Adjust scroll if caret is beyond right edge
        if (visibleCaretPosition > textAreaWidth - 5) // 5px margin
        {
            _scrollOffset = caretPixelPosition - textAreaWidth + 5;
        }
        // Adjust scroll if caret is beyond left edge
        else if (visibleCaretPosition < 5) // 5px margin
        {
            _scrollOffset = Math.Max(0, caretPixelPosition - 5);
        }

        // Ensure scroll doesn't go negative
        _scrollOffset = Math.Max(0, _scrollOffset);

        // Optional: If text is shorter than visible area, reset scroll to 0
        var totalTextWidth = GetCachedOrEstimatedWidth(displayText, measureService);
        if (totalTextWidth <= textAreaWidth)
        {
            _scrollOffset = 0;
        }
    }

    // Get width from cache or estimate if not available
    private int GetCachedOrEstimatedWidth(string text, TextMeasurementService measureService)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Try to get from cache first, otherwise use estimation
        return measureService.MeasureTextEstimate(text, Font.Family, (int)Font.Size);
    }

    // Pre-measure all text segments asynchronously to populate cache
    private async Task PreMeasureTextSegments(string text, TextMeasurementService measureService)
    {
        try
        {
            // Build array of all substrings to measure (including full text)
            var textsToMeasure = new List<string>(text.Length + 1);

            // Add full text
            textsToMeasure.Add(text);

            // Add all substrings for caret positioning
            for (int i = 1; i < text.Length; i++)
            {
                textsToMeasure.Add(text.Substring(0, i));
            }

            // Batch measure all texts in a single JS interop call
            await measureService.MeasureTextBatchAsync(textsToMeasure.ToArray(), Font.Family, (int)Font.Size);

            // Small delay to ensure previous render has completed
            await Task.Delay(10);

            // Trigger re-render now that accurate measurements are cached
            // This will update cursor position from estimated to pixel-perfect
            Invalidate();
        }
        catch
        {
            // Ignore errors in background measurement
        }
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
        if (!Enabled)
        {
            base.OnMouseDown(e);
            return;
        }

        var displayText = GetDisplayText();
        var measureService = (Parent as Form)?.TextMeasurementService;

        // Calculate caret position asynchronously using real JavaScript measurement
        _ = Task.Run(async () =>
        {
            try
            {
                if (measureService != null && !string.IsNullOrEmpty(displayText))
                {
                    const int textPadding = 3;
                    var clickX = e.X - textPadding + _scrollOffset; // Account for scroll offset
                    var newPosition = 0;

                    // Use actual JavaScript measurement for pixel-perfect positioning
                    for (int i = 0; i <= displayText.Length; i++)
                    {
                        var textUpToPosition = displayText.Substring(0, i);
                        var widthUpToPosition = await measureService.MeasureTextAsync(textUpToPosition, Font.Family, (int)Font.Size);

                        if (i < displayText.Length)
                        {
                            var textUpToNext = displayText.Substring(0, i + 1);
                            var widthUpToNext = await measureService.MeasureTextAsync(textUpToNext, Font.Family, (int)Font.Size);
                            var midpoint = (widthUpToPosition + widthUpToNext) / 2;

                            if (clickX < midpoint)
                            {
                                newPosition = i;
                                break;
                            }
                        }
                        else
                        {
                            newPosition = displayText.Length;
                        }
                    }

                    _caretPosition = newPosition;
                    _selectionStart = newPosition;
                    _selectionLength = 0;
                    Invalidate();
                }
                else
                {
                    // Fallback when service unavailable
                    const int textPadding = 3;
                    var clickX = e.X - textPadding + _scrollOffset;
                    _caretPosition = Math.Max(0, Math.Min(displayText.Length, clickX / 7));
                    _selectionStart = _caretPosition;
                    _selectionLength = 0;
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TextBox measurement error: {ex.Message}");
                // Fallback on error
                const int textPadding = 3;
                var clickX = e.X - textPadding + _scrollOffset;
                _caretPosition = Math.Max(0, Math.Min(displayText.Length, clickX / 7));
                _selectionStart = _caretPosition;
                _selectionLength = 0;
                Invalidate();
            }
        });

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
        PreMeasureText(); // Pre-measure for better cursor positioning
    }

    // Pre-measure text asynchronously when text changes to populate cache for cursor positioning
    private void PreMeasureText()
    {
        var displayText = GetDisplayText();
        var measureService = (Parent as Form)?.TextMeasurementService;

        if (measureService != null && !string.IsNullOrEmpty(displayText))
        {
            // Start async measurement - don't use Task.Run, keep on UI context
            _ = PreMeasureTextSegments(displayText, measureService);
        }
    }
}
