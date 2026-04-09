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

    public override void UpButton() => Value = Math.Min(_maximum, _value + _increment);
    public override void DownButton() => Value = Math.Max(_minimum, _value - _increment);

    protected override string GetValueText()
    {
        if (_hexadecimal)
            return ((long)_value).ToString("X");

        string format = _decimalPlaces > 0 ? "F" + _decimalPlaces : "G";
        string text = _value.ToString(format);

        if (_thousandsSeparator && !_hexadecimal)
            text = _value.ToString("N" + _decimalPlaces);

        return text;
    }
}
