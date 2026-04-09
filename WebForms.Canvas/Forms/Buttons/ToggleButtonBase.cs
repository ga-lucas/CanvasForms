using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Intermediate base class for toggle-style buttons (CheckBox, RadioButton).
/// Provides the shared Checked / CheckedChanged / AutoCheck pattern so
/// neither CheckBox nor RadioButton need to duplicate it.
/// </summary>
public abstract class ToggleButtonBase : ButtonBase
{
    private bool _checked = false;

    public bool AutoCheck { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the control is in the checked state.
    /// </summary>
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked != value)
            {
                _checked = value;
                OnCheckedChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public event EventHandler? CheckedChanged;

    protected virtual void OnCheckedChanged(EventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Returns the appropriate indicator border color based on hover/pressed state.
    /// </summary>
    protected Color GetIndicatorBorderColor()
    {
        var state = GetButtonState();
        return (state == ButtonState.Hot || state == ButtonState.Pushed) && Enabled
            ? Color.FromArgb(0, 120, 215)
            : Color.FromArgb(122, 122, 122);
    }

    /// <summary>
    /// Returns the appropriate indicator fill color (for the inner mark/dot).
    /// </summary>
    protected Color GetIndicatorMarkColor()
        => Enabled ? Color.FromArgb(0, 120, 215) : DisabledForeColor;

    /// <summary>
    /// Returns the background fill for the indicator box/circle.
    /// </summary>
    protected Color GetIndicatorBackColor()
        => Enabled ? Color.White : Color.FromArgb(240, 240, 240);
}
