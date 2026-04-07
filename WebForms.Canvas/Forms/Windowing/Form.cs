using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

public class Form : Control
{
    private static int _nextZIndex = 1;
    private const int TitleBarHeight = 32; // Height of the title bar
    private Control? _focusedControl;
    private FormWindowState _windowState = FormWindowState.Normal;
    private Rectangle _normalBounds; // Store bounds before minimize/maximize

    // Creation timestamp for maintaining order
    public DateTime CreatedAt { get; } = DateTime.Now;

    public bool AllowResize { get; set; } = true;
    public bool AllowMove { get; set; } = true;
    public int MinimumWidth { get; set; } = 100;
    public int MinimumHeight { get; set; } = 50;
    public int MaximumWidth { get; set; } = 0; // 0 = no limit
    public int MaximumHeight { get; set; } = 0; // 0 = no limit

    // Z-order for stacking
    public int ZIndex { get; set; } = 0;

    // Window state
    public FormWindowState WindowState
    {
        get => _windowState;
        set
        {
            if (_windowState != value)
            {
                _windowState = value;
                OnWindowStateChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    // Event fired when window state changes
    public event EventHandler? WindowStateChanged;

    protected virtual void OnWindowStateChanged(EventArgs e)
    {
        WindowStateChanged?.Invoke(this, e);
    }

    // Event fired when form is closed
    public event EventHandler? FormClosed;

    protected virtual void OnFormClosed(EventArgs e)
    {
        FormClosed?.Invoke(this, e);
    }

    // Event fired when form is activated (brought to front)
    public event EventHandler? Activated;

    protected virtual void OnActivated(EventArgs e)
    {
        Activated?.Invoke(this, e);
    }

    // Callback for notifying parent container of changes (e.g., new forms created)
    // This is needed for Blazor to know when to re-render
    public Action? OnContainerChanged { get; set; }

    // Focused control for keyboard input
    public Control? FocusedControl
    {
        get => _focusedControl;
        set
        {
            if (_focusedControl != value)
            {
                _focusedControl = value;
                Invalidate();
            }
        }
    }

    // Text measurement service for accurate text rendering
    public TextMeasurementService? TextMeasurementService { get; set; }

    // Client area dimensions (excluding title bar)
    public int ClientWidth => Width;
    public int ClientHeight => Math.Max(0, Height - TitleBarHeight);

    // Override layout dimensions to use client area (excludes title bar)
    protected override int LayoutWidth => ClientWidth;
    protected override int LayoutHeight => ClientHeight;

    public Form()
    {
        Text = "Form";
        Width = 800;
        Height = 600;
        Left = 50;
        Top = 50;
        BackColor = Color.FromArgb(240, 240, 240);
        ZIndex = _nextZIndex++;
    }

    public new void BringToFront()
    {
        ZIndex = _nextZIndex++;
        OnActivated(EventArgs.Empty);
        Invalidate();
    }

    /// <summary>
    /// Brings the form to front only if it's not already the topmost form.
    /// </summary>
    /// <param name="currentMaxZIndex">The current maximum z-index of all visible forms</param>
    /// <returns>True if the z-index was changed, false otherwise</returns>
    public bool BringToFrontIfNeeded(int currentMaxZIndex)
    {
        if (ZIndex < currentMaxZIndex)
        {
            ZIndex = _nextZIndex++;
            OnActivated(EventArgs.Empty);
            Invalidate();
            return true;
        }

        // Already at front, just fire activated event
        OnActivated(EventArgs.Empty);
        return false;
    }

    public new Graphics CreateGraphics()
    {
        return new Graphics(ClientWidth, ClientHeight);
    }

    public new void Show()
    {
        Visible = true;
        PerformLayout(); // Layout controls when form is shown
        Invalidate();

        // Notify container that state changed (for Blazor re-rendering)
        OnContainerChanged?.Invoke();
    }

    public void Close()
    {
        Visible = false;
        OnFormClosed(EventArgs.Empty);

        // Notify container that state changed
        OnContainerChanged?.Invoke();
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw form background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        // Let user code handle Paint event first
        base.OnPaint(e);

        // Then render child controls
        foreach (var control in Controls)
        {
            if (!control.Visible) continue;

            // Translate graphics to control position
            g.TranslateTransform(control.Left, control.Top);

            // Create paint args for the control
            var controlPaintArgs = new PaintEventArgs(
                g,
                new Rectangle(0, 0, control.Width, control.Height)
            );

            // Let control paint itself (but skip drop-downs for ComboBox/DateTimePicker and autocomplete for TextBox)
            if (control is ComboBox comboBox)
            {
                comboBox.PaintWithoutDropDown(controlPaintArgs);
            }
            else if (control is DateTimePicker dateTimePicker)
            {
                dateTimePicker.PaintWithoutDropDown(controlPaintArgs);
            }
            else if (control is TextBox textBox)
            {
                textBox.PaintWithoutAutoComplete(controlPaintArgs);
            }
            else
            {
                control.OnPaint(controlPaintArgs);
            }

            // Restore graphics state
            g.TranslateTransform(-control.Left, -control.Top);
        }

        // Paint all ComboBox drop-downs on top of everything
        foreach (var control in Controls)
        {
            if (control is ComboBox comboBox && comboBox.DroppedDown && control.Visible)
            {
                // Translate to control position
                g.TranslateTransform(control.Left, control.Top);

                // Paint just the drop-down
                var dropDownPaintArgs = new PaintEventArgs(
                    g,
                    new Rectangle(0, 0, control.Width, control.Height)
                );
                comboBox.PaintDropDownOnly(dropDownPaintArgs);

                // Restore
                g.TranslateTransform(-control.Left, -control.Top);
            }
        }

        // Paint all DateTimePicker drop-downs on top of everything
        foreach (var control in Controls)
        {
            if (control is DateTimePicker dateTimePicker && dateTimePicker.HasVisibleDropDown && control.Visible)
            {
                // Translate to control position
                g.TranslateTransform(control.Left, control.Top);

                // Paint just the drop-down
                var dropDownPaintArgs = new PaintEventArgs(
                    g,
                    new Rectangle(0, 0, control.Width, control.Height)
                );
                dateTimePicker.PaintDropDownOnly(dropDownPaintArgs);

                // Restore
                g.TranslateTransform(-control.Left, -control.Top);
            }
        }

        // Paint all TextBox autocomplete panels on top of everything
        foreach (var control in Controls)
        {
            if (control is TextBox textBox && textBox.HasVisibleAutoComplete && control.Visible)
            {
                // Translate to control position
                g.TranslateTransform(control.Left, control.Top);

                // Paint just the autocomplete
                var autoCompletePaintArgs = new PaintEventArgs(
                    g,
                    new Rectangle(0, 0, control.Width, control.Height)
                );
                textBox.PaintAutoCompleteOnly(autoCompletePaintArgs);

                // Restore
                g.TranslateTransform(-control.Left, -control.Top);
            }
        }
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        // Check if any child control was clicked
        bool hitAnyControl = false;

        foreach (var control in Controls)
        {
            if (!control.Visible || !control.Enabled) continue;

            if (HitTest(control, e.X, e.Y))
            {
                hitAnyControl = true;

                // Set focus to this control
                FocusedControl = control;

                var controlArgs = new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - control.Left,
                    e.Y - control.Top
                );
                control.OnMouseDown(controlArgs);
                break; // Don't check other controls
            }
        }

        if (!hitAnyControl)
        {
            // Clicked on form background - clear focus and close any open ComboBox drop-downs
            FocusedControl = null;

            // Close all ComboBox drop-downs
            foreach (var control in Controls)
            {
                if (control is ComboBox comboBox && comboBox.DroppedDown)
                {
                    comboBox.DroppedDown = false;
                }
                if (control is DateTimePicker dateTimePicker && dateTimePicker.DroppedDown)
                {
                    dateTimePicker.DroppedDown = false;
                }
            }
        }
        else
        {
            // Close other ComboBox drop-downs (keep only the clicked one open if it's a ComboBox)
            foreach (var control in Controls)
            {
                if (control is ComboBox comboBox && comboBox != FocusedControl && comboBox.DroppedDown)
                {
                    comboBox.DroppedDown = false;
                }
                if (control is DateTimePicker dateTimePicker && dateTimePicker != FocusedControl && dateTimePicker.DroppedDown)
                {
                    dateTimePicker.DroppedDown = false;
                }
            }
        }

        if (!hitAnyControl)
        {
            base.OnMouseDown(e);
        }
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        // Check if any child control should receive mouse up
        foreach (var control in Controls)
        {
            if (!control.Visible || !control.Enabled) continue;

            if (HitTest(control, e.X, e.Y))
            {
                var controlArgs = new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - control.Left,
                    e.Y - control.Top
                );
                control.OnMouseUp(controlArgs);
                return;
            }
        }

        base.OnMouseUp(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        // Check if any child control should receive mouse move
        foreach (var control in Controls)
        {
            if (!control.Visible) continue;

            if (HitTest(control, e.X, e.Y))
            {
                var controlArgs = new MouseEventArgs(
                    e.Button,
                    e.Clicks,
                    e.X - control.Left,
                    e.Y - control.Top
                );
                control.OnMouseMove(controlArgs);
                return;
            }
        }

        base.OnMouseMove(e);
    }

    private bool HitTest(Control control, int x, int y)
    {
        // Normal bounds check
        var inNormalBounds = x >= control.Left && x < control.Left + control.Width &&
                             y >= control.Top && y < control.Top + control.Height;

        if (inNormalBounds)
            return true;

        // Special case: ComboBox with drop-down open
        if (control is ComboBox comboBox && comboBox.DroppedDown)
        {
            // Check if clicking in drop-down area (below the control)
            var dropDownHeight = comboBox.DropDownHeight;
            var dropDownWidth = comboBox.DropDownWidth;

            return x >= control.Left && x < control.Left + dropDownWidth &&
                   y >= control.Top + control.Height && y < control.Top + control.Height + dropDownHeight;
        }

        // Special case: DateTimePicker with drop-down open
        if (control is DateTimePicker dateTimePicker && dateTimePicker.DroppedDown)
        {
            var dd = dateTimePicker.GetDropDownBounds();

            return x >= control.Left + dd.X && x < control.Left + dd.Right &&
                   y >= control.Top + dd.Y && y < control.Top + dd.Bottom;
        }

        // Special case: TextBox with autocomplete panel open
        if (control is TextBox textBox && textBox.HasVisibleAutoComplete)
        {
            // Check if clicking in autocomplete panel area (below the control)
            var panelBounds = textBox.GetAutoCompletePanelBounds();

            return x >= control.Left + panelBounds.X && x < control.Left + panelBounds.Right &&
                   y >= control.Top + panelBounds.Y && y < control.Top + panelBounds.Bottom;
        }

        return false;
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        // Route keyboard events to focused control
        if (FocusedControl != null && FocusedControl.Enabled)
        {
            FocusedControl.OnKeyDown(e);
        }
        else
        {
            base.OnKeyDown(e);
        }
    }

    protected internal override void OnKeyUp(KeyEventArgs e)
    {
        // Route keyboard events to focused control
        if (FocusedControl != null && FocusedControl.Enabled)
        {
            FocusedControl.OnKeyUp(e);
        }
        else
        {
            base.OnKeyUp(e);
        }
    }

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        // Route keyboard events to focused control
        if (FocusedControl != null && FocusedControl.Enabled)
        {
            FocusedControl.OnKeyPress(e);
        }
        else
        {
            base.OnKeyPress(e);
        }
    }

    // Window state management methods
    public void Minimize()
    {
        if (_windowState != FormWindowState.Minimized)
        {
            // Save current bounds
            _normalBounds = new Rectangle(Left, Top, Width, Height);
            WindowState = FormWindowState.Minimized;
        }
    }

    public void Maximize(int desktopWidth, int desktopHeight, int taskbarHeight, bool preserveNormalBounds = false)
    {
        if (_windowState != FormWindowState.Maximized)
        {
            // Save current bounds if not already minimized (unless preserveNormalBounds is true)
            if (_windowState == FormWindowState.Normal && !preserveNormalBounds)
            {
                _normalBounds = new Rectangle(Left, Top, Width, Height);
            }

            // Set to maximized state (fill desktop except taskbar)
            // Note: Left and Top are relative to desktop area (which is below taskbar)
            Left = 0;
            Top = 0; // Desktop area starts at 0 (already accounting for taskbar)
            Width = desktopWidth;
            Height = desktopHeight - taskbarHeight;
            WindowState = FormWindowState.Maximized;
        }
    }

    /// <summary>
    /// Sets the normal bounds that will be used when restoring from maximized/minimized state.
    /// This is useful for snap-to-maximize where we want to restore to the pre-drag position.
    /// </summary>
    public void SetNormalBounds(int left, int top, int width, int height)
    {
        _normalBounds = new Rectangle(left, top, width, height);
    }

    public void Restore()
    {
        if (_windowState != FormWindowState.Normal)
        {
            // Restore to normal bounds
            if (_normalBounds.Width > 0 && _normalBounds.Height > 0)
            {
                Left = _normalBounds.X;
                Top = _normalBounds.Y;
                Width = _normalBounds.Width;
                Height = _normalBounds.Height;
            }
            WindowState = FormWindowState.Normal;
        }
    }

    /// <summary>
    /// Ensures the form's title bar is visible within the specified viewport bounds.
    /// If the title bar is not visible, the form is repositioned to make it visible.
    /// </summary>
    /// <param name="viewportWidth">Width of the available viewport</param>
    /// <param name="viewportHeight">Height of the available viewport</param>
    /// <param name="taskbarHeight">Height of the taskbar at the top</param>
    public void EnsureTitleBarVisible(int viewportWidth, int viewportHeight, int taskbarHeight)
    {
        // Only apply to normal windows (not minimized or maximized)
        if (_windowState != FormWindowState.Normal) return;

        // Title bar is at the top of the form, so we need to ensure:
        // 1. The top of the form is not above the desktop area (minimum is 0, which is just below taskbar)
        // 2. The title bar doesn't extend below the bottom of the viewport
        // 3. If the form is too wide, position it as far left as possible

        // Ensure form is not above the desktop area (Top is relative to desktop, so minimum is 0)
        if (Top < 0)
        {
            Top = 0;
        }

        // Ensure the title bar is visible at the bottom
        // The form's Top position is relative to the desktop area (after taskbar)
        // So if Top + taskbarHeight + TitleBarHeight > viewportHeight, the title bar is cut off
        var maxTop = viewportHeight - taskbarHeight - TitleBarHeight;
        if (Top > maxTop)
        {
            Top = maxTop;
        }

        // Ensure some of the left side is visible (at least 50 pixels to grab)
        var minLeft = -(Width - 50);
        if (Left < minLeft)
        {
            Left = minLeft;
        }

        // If form is too wide to fit, position as far left as possible
        if (Width > viewportWidth)
        {
            Left = 0;
        }
        else
        {
            // Ensure form doesn't extend too far right (keep at least 50 pixels visible on the left)
            var maxLeft = viewportWidth - 50;
            if (Left > maxLeft)
            {
                Left = maxLeft;
            }
        }
    }
}

/// <summary>
/// Specifies how a form window is displayed
/// </summary>
public enum FormWindowState
{
    /// <summary>
    /// A normal sized window
    /// </summary>
    Normal,

    /// <summary>
    /// A minimized window (hidden, shown only in taskbar)
    /// </summary>
    Minimized,

    /// <summary>
    /// A maximized window (fills the desktop)
    /// </summary>
    Maximized
}
