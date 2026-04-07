using WebForms.Canvas.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Base class for button-like controls (Button, CheckBox, RadioButton)
/// Provides common functionality for all button controls
/// </summary>
public abstract class ButtonBase : Control
{
    protected bool _isPressed = false;
    protected bool _isHovered = false;

    // Common button properties
    public FlatStyle FlatStyle { get; set; } = FlatStyle.Standard;
    public ContentAlignment TextAlign { get; set; } = ContentAlignment.MiddleCenter;
    public ContentAlignment ImageAlign { get; set; } = ContentAlignment.MiddleCenter;
    public Image? Image { get; set; }
    public TextImageRelation TextImageRelation { get; set; } = TextImageRelation.Overlay;
    public bool UseMnemonic { get; set; } = true;
    public bool UseVisualStyleBackColor { get; set; } = true;
    public bool AutoEllipsis { get; set; } = false;

    protected ButtonBase()
    {
        this.SetStyle(ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.UserPaint, true);
        TabStop = true;
    }

    /// <summary>
    /// Gets the current visual state of the button
    /// </summary>
    protected ButtonState GetButtonState()
    {
        if (!Enabled)
            return ButtonState.Disabled;
        if (_isPressed)
            return ButtonState.Pushed;
        if (_isHovered)
            return ButtonState.Hot;
        if (Focused)
            return ButtonState.Focused;
        return ButtonState.Normal;
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled && e.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Focus();
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        bool wasPressed = _isPressed;
        _isPressed = false;

        if (Enabled && wasPressed && e.Button == MouseButtons.Left)
        {
            // Only fire click if mouse is still over button
            if (ContainsPoint(e.X, e.Y))
            {
                OnClick(EventArgs.Empty);
            }

            // Single invalidate for mouse-up (pressed -> normal) and any state changes from click.
            Invalidate();
        }

        base.OnMouseUp(e);
    }

    protected internal override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        _isPressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            _isPressed = true;
            Invalidate();
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected internal override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            _isPressed = false;
            OnClick(EventArgs.Empty);
            // Single invalidate for key-up (pressed -> normal) and any state changes from click.
            Invalidate();
            e.Handled = true;
        }
        base.OnKeyUp(e);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        Invalidate();
        base.OnTextChanged(e);
    }

    /// <summary>
    /// Simulates a button click
    /// </summary>
    public void PerformClick()
    {
        OnClick(EventArgs.Empty);
    }

    /// <summary>
    /// Helper method to lighten a color
    /// </summary>
    protected static Color LightenColor(Color color, float amount)
    {
        int r = Math.Min(255, (int)(color.R + (255 - color.R) * amount));
        int g = Math.Min(255, (int)(color.G + (255 - color.G) * amount));
        int b = Math.Min(255, (int)(color.B + (255 - color.B) * amount));
        return Color.FromArgb(color.A, r, g, b);
    }

    /// <summary>
    /// Helper method to darken a color
    /// </summary>
    protected static Color DarkenColor(Color color, float amount)
    {
        int r = Math.Max(0, (int)(color.R * (1 - amount)));
        int g = Math.Max(0, (int)(color.G * (1 - amount)));
        int b = Math.Max(0, (int)(color.B * (1 - amount)));
        return Color.FromArgb(color.A, r, g, b);
    }
}

/// <summary>
/// Specifies the flat style appearance of a button control
/// </summary>
public enum FlatStyle
{
    Flat,
    Popup,
    Standard,
    System
}

/// <summary>
/// Specifies the relationship between text and image on a button
/// </summary>
public enum TextImageRelation
{
    Overlay,
    ImageAboveText,
    TextAboveImage,
    ImageBeforeText,
    TextBeforeImage
}

/// <summary>
/// Specifies the visual state of a button
/// </summary>
public enum ButtonState
{
    Normal,
    Hot,
    Pushed,
    Disabled,
    Focused
}

/// <summary>
/// Control style flags
/// </summary>
[Flags]
public enum ControlStyles
{
    None = 0,
    Selectable = 1,
    UserPaint = 2,
    StandardClick = 4,
    StandardDoubleClick = 8,
    AllPaintingInWmPaint = 16,
    Opaque = 32,
    ResizeRedraw = 64,
    FixedWidth = 128,
    FixedHeight = 256,
    SupportsTransparentBackColor = 512,
    UserMouse = 1024,
    ContainerControl = 2048,
    EnableNotifyMessage = 4096
}
