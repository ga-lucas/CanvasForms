namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms NumericUpDown control
/// </summary>
public class NumericUpDown : UpDownBase
{
    private decimal _value = 0;
    private decimal _minimum = 0;
    private decimal _maximum = 100;
    private decimal _increment = 1;
    private int _decimalPlaces = 0;
    private bool _thousandsSeparator = false;
    private bool _hexadecimal = false;

    public event EventHandler? ValueChanged;

    public NumericUpDown()
    {
        Width = 100;
        Height = 23;
    }

    public decimal Value
    {
        get => _value;
        set
        {
            var clamped = Math.Max(_minimum, Math.Min(_maximum, value));
            if (_value != clamped)
            {
                _value = clamped;
                ValueChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public decimal Minimum
    {
        get => _minimum;
        set { _minimum = value; if (_value < _minimum) Value = _minimum; }
    }

    public decimal Maximum
    {
        get => _maximum;
        set { _maximum = value; if (_value > _maximum) Value = _maximum; }
    }

    public decimal Increment { get => _increment; set => _increment = value; }

    public HorizontalAlignment TextAlign { get; set; } = HorizontalAlignment.Left;

    public int DecimalPlaces
    {
        get => _decimalPlaces;
        set { _decimalPlaces = Math.Max(0, value); Invalidate(); }
    }

    public bool ThousandsSeparator
    {
        get => _thousandsSeparator;
        set { _thousandsSeparator = value; Invalidate(); }
    }

    public bool Hexadecimal
    {
        get => _hexadecimal;
        set { _hexadecimal = value; Invalidate(); }
    }

    public override void UpButton()
    {
        Value = Math.Min(_maximum, _value + _increment);
        _typingBuffer = string.Empty; // commit any partial entry
    }

    public override void DownButton()
    {
        Value = Math.Max(_minimum, _value - _increment);
        _typingBuffer = string.Empty;
    }

    // ── Text property ─────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the current value as a string.
    /// Setting parses the string and clamps to [Minimum, Maximum].
    /// </summary>
    public new string Text
    {
        get => GetValueText();
        set
        {
            if (decimal.TryParse(value, out var parsed))
                Value = parsed;
        }
    }

    // ── Typing buffer ─────────────────────────────────────────────────────────

    private string _typingBuffer = string.Empty;
    private bool _allSelected = false; // true after GotFocus until first keystroke

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        if (ReadOnly) { base.OnKeyPress(e); return; }

        char c = e.KeyChar;
        if (c == (char)Keys.Back)
        {
            if (_allSelected)
            {
                _typingBuffer = string.Empty;
                _allSelected = false;
            }
            else if (_typingBuffer.Length > 0)
            {
                _typingBuffer = _typingBuffer[..^1];
            }
            Invalidate();
        }
        else if (c == '\r' || c == '\n')
        {
            CommitTypingBuffer();
            _typingBuffer = string.Empty;
            _allSelected = false;
        }
        else if (char.IsDigit(c) || (c == '-' && (_typingBuffer.Length == 0 || _allSelected) && _minimum < 0)
                 || (c == '.' && _decimalPlaces > 0 && !_typingBuffer.Contains('.')))
        {
            if (_allSelected)
            {
                _typingBuffer = string.Empty;
                _allSelected = false;
            }
            _typingBuffer += c;
            Invalidate();
        }
        else if (c == 27) // Escape — revert to committed value
        {
            _typingBuffer = string.Empty;
            _allSelected = false;
            Invalidate();
        }
        e.Handled = true;
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (InterceptArrowKeys && Enabled)
        {
            if (e.KeyCode == Keys.Up)   { CommitTypingBuffer(); UpButton();   e.Handled = true; return; }
            if (e.KeyCode == Keys.Down) { CommitTypingBuffer(); DownButton(); e.Handled = true; return; }
        }

        if (e.KeyCode == Keys.PageUp)
        {
            CommitTypingBuffer();
            Value = Math.Min(_maximum, _value + _increment * 10);
            e.Handled = true;
            return;
        }
        if (e.KeyCode == Keys.PageDown)
        {
            CommitTypingBuffer();
            Value = Math.Max(_minimum, _value - _increment * 10);
            e.Handled = true;
            return;
        }

        // Ctrl+A — select all (mirror WinForms)
        if (e.Control && e.KeyCode == Keys.A)
        {
            _typingBuffer = GetValueText();
            _allSelected = true;
            Invalidate();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    protected internal override void OnMouseWheel(MouseEventArgs e)
    {
        if (!Enabled) { base.OnMouseWheel(e); return; }
        if (e.Delta > 0) UpButton();
        else if (e.Delta < 0) DownButton();
        base.OnMouseWheel(e);
    }

    protected internal override void OnGotFocus(EventArgs e)
    {
        // Select-all on focus — matches WinForms default
        _typingBuffer = GetValueText();
        _allSelected = true;
        Invalidate();
        base.OnGotFocus(e);
    }

    protected internal override void OnLostFocus(EventArgs e)
    {
        CommitTypingBuffer();
        _typingBuffer = string.Empty;
        _allSelected = false;
        base.OnLostFocus(e);
    }

    private void CommitTypingBuffer()
    {
        if (!string.IsNullOrEmpty(_typingBuffer) && decimal.TryParse(_typingBuffer, out var parsed))
            Value = parsed;
    }

    // ── Display ───────────────────────────────────────────────────────────────

    protected override string GetValueText()
    {
        // Show the live typing buffer when the user is actively editing
        if (!string.IsNullOrEmpty(_typingBuffer) && !_allSelected)
            return _typingBuffer;

        if (_hexadecimal)
            return ((long)_value).ToString("X");

        if (_thousandsSeparator)
            return _value.ToString("N" + _decimalPlaces);

        return _decimalPlaces > 0 ? _value.ToString("F" + _decimalPlaces) : _value.ToString("G");
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Draw selection highlight over text when all-selected
        if (_allSelected && Focused && Enabled)
        {
            var g = e.Graphics;
            int btnX = UpDownAlign == LeftRightAlignment.Right ? Width - ButtonWidth : 0;
            int textX = UpDownAlign == LeftRightAlignment.Right ? 3 : ButtonWidth + 3;
            int textW = (UpDownAlign == LeftRightAlignment.Right ? btnX : Width) - textX - 3;
            using var selBrush = new SolidBrush(Color.FromArgb(80, 0, 120, 215));
            g.FillRectangle(selBrush, textX, 3, textW, Height - 6);
        }
    }
}
