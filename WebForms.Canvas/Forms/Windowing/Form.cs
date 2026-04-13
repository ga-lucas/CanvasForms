using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

public class Form : ContainerControl
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

    // Event fired when form is about to close (can be cancelled)
    public event FormClosingEventHandler? FormClosing;

    protected virtual void OnFormClosing(FormClosingEventArgs e)
    {
        FormClosing?.Invoke(this, e);
    }

    // Event fired when form is closed
    public event FormClosedEventHandler? FormClosed;

    protected virtual void OnFormClosed(FormClosedEventArgs e)
    {
        FormClosed?.Invoke(this, e);
    }

    // Event fired when form is activated (brought to front)
    public event EventHandler? Activated;

    protected virtual void OnActivated(EventArgs e)
    {
        Activated?.Invoke(this, e);
    }

    // Track the close reason for the current close operation
    private CloseReason _closeReason = CloseReason.None;

    /// <summary>
    /// Closes the form. Can be cancelled by handling the FormClosing event.
    /// </summary>
    public void Close()
    {
        Close(CloseReason.UserClosing);
    }

    /// <summary>
    /// Closes the form with a specific reason. Can be cancelled by handling the FormClosing event.
    /// </summary>
    internal void Close(CloseReason reason)
    {
        _closeReason = reason;

        // Raise FormClosing event - allow cancellation
        var closingArgs = new FormClosingEventArgs(reason);
        OnFormClosing(closingArgs);

        if (closingArgs.Cancel)
        {
            _closeReason = CloseReason.None;
            return; // Close was cancelled
        }

        // Hide the form
        Visible = false;

        // Raise FormClosed event
        var closedArgs = new FormClosedEventArgs(reason);
        OnFormClosed(closedArgs);

        _closeReason = CloseReason.None;
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

    public new System.Drawing.Size ClientSize
    {
        get => new System.Drawing.Size(ClientWidth, ClientHeight);
        set
        {
            Width = value.Width;
            Height = value.Height + TitleBarHeight;
        }
    }

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

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Draw form background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        // Let user code handle Paint event first
        base.OnPaint(e);

        // Then render child controls (full tree), excluding overlays.
        PaintControlsRecursive(g, this, offsetX: 0, offsetY: 0);

        // Final pass: paint overlays (ComboBox drop-down, DateTimePicker popup, TextBox autocomplete)
        // on top of everything, including when the owner is nested in containers.
        PaintOverlaysRecursive(g, this, offsetX: 0, offsetY: 0);
    }

    private void PaintControlsRecursive(Graphics g, Control parent, int offsetX, int offsetY)
    {
        foreach (var control in parent.Controls)
        {
            if (!control.Visible) continue;

            var (sx, sy) = GetChildScrollOffset(parent);
            var childOffsetX = offsetX + sx + control.Left;
            var childOffsetY = offsetY + sy + control.Top;

            g.TranslateTransform(childOffsetX, childOffsetY);

            var controlPaintArgs = new PaintEventArgs(
                g,
                new Rectangle(0, 0, control.Width, control.Height)
            );

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

            g.TranslateTransform(-childOffsetX, -childOffsetY);

            if (control.HasChildren)
            {
                PaintControlsRecursive(g, control, childOffsetX, childOffsetY);
            }
        }
    }

    private void PaintOverlaysRecursive(Graphics g, Control parent, int offsetX, int offsetY)
    {
        foreach (var control in parent.Controls)
        {
            if (!control.Visible) continue;

            var (sx, sy) = GetChildScrollOffset(parent);
            var childOffsetX = offsetX + sx + control.Left;
            var childOffsetY = offsetY + sy + control.Top;

            if (control is ComboBox comboBox && comboBox.DroppedDown)
            {
                g.TranslateTransform(childOffsetX, childOffsetY);
                var ddArgs = new PaintEventArgs(g, new Rectangle(0, 0, control.Width, control.Height));
                comboBox.PaintDropDownOnly(ddArgs);
                g.TranslateTransform(-childOffsetX, -childOffsetY);
            }
            else if (control is DateTimePicker dateTimePicker && dateTimePicker.HasVisibleDropDown)
            {
                g.TranslateTransform(childOffsetX, childOffsetY);
                var ddArgs = new PaintEventArgs(g, new Rectangle(0, 0, control.Width, control.Height));
                dateTimePicker.PaintDropDownOnly(ddArgs);
                g.TranslateTransform(-childOffsetX, -childOffsetY);
            }
            else if (control is TextBox textBox && textBox.HasVisibleAutoComplete)
            {
                g.TranslateTransform(childOffsetX, childOffsetY);
                var acArgs = new PaintEventArgs(g, new Rectangle(0, 0, control.Width, control.Height));
                textBox.PaintAutoCompleteOnly(acArgs);
                g.TranslateTransform(-childOffsetX, -childOffsetY);
            }

            if (control.HasChildren)
            {
                PaintOverlaysRecursive(g, control, childOffsetX, childOffsetY);
            }
        }
    }

    private static (int x, int y) GetChildScrollOffset(Control parent)
    {
        if (parent is ScrollableControl scrollable && scrollable.AutoScroll)
        {
            // DisplayRectangle is offset by AutoScrollPosition (negative when scrolled),
            // and painting code translates by that value.
            return (scrollable.DisplayRectangle.X, scrollable.DisplayRectangle.Y);
        }

        return (0, 0);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        var hit = FindDeepestHitControl(this, e.X, e.Y, offsetX: 0, offsetY: 0);
        if (hit.control is null)
        {
            FocusedControl = null;
            CloseAllOverlays(except: null);
            base.OnMouseDown(e);
            return;
        }

        FocusedControl = hit.control;
        CloseAllOverlays(except: hit.control);

        var controlArgs = new MouseEventArgs(e.Button, e.Clicks, hit.x, hit.y);
        hit.control.OnMouseDown(controlArgs);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        var hit = FindDeepestHitControl(this, e.X, e.Y, offsetX: 0, offsetY: 0);
        if (hit.control is not null && hit.control.Enabled)
        {
            var controlArgs = new MouseEventArgs(e.Button, e.Clicks, hit.x, hit.y);
            hit.control.OnMouseUp(controlArgs);
            return;
        }

        base.OnMouseUp(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        var hit = FindDeepestHitControl(this, e.X, e.Y, offsetX: 0, offsetY: 0, includeDisabled: true);
        if (hit.control is not null)
        {
            var controlArgs = new MouseEventArgs(e.Button, e.Clicks, hit.x, hit.y);
            hit.control.OnMouseMove(controlArgs);
            return;
        }

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseWheel(MouseEventArgs e)
    {
        var hit = FindDeepestHitControl(this, e.X, e.Y, offsetX: 0, offsetY: 0);
        if (hit.control is not null && hit.control.Enabled)
        {
            var controlArgs = new MouseEventArgs(e.Button, e.Clicks, hit.x, hit.y, e.Delta);
            hit.control.OnMouseWheel(controlArgs);
            return;
        }

        base.OnMouseWheel(e);
    }

    private static (Control? control, int x, int y) FindDeepestHitControl(Control parent, int formX, int formY, int offsetX, int offsetY, bool includeDisabled = false)
    {
        // Overlays (ComboBox drop-down, DateTimePicker popup, TextBox autocomplete) must be hittable even when
        // the owner control is nested inside containers and the pointer is outside the container bounds.
        // So we must search the entire subtree for overlay hits before doing normal bounds-based hit testing.
        var overlayHit = FindTopMostOverlayHitControl(parent, formX, formY, offsetX, offsetY, includeDisabled);
        if (overlayHit.control is not null)
        {
            return overlayHit;
        }

        // Traverse from top-most to bottom-most.
        for (var i = parent.Controls.Count - 1; i >= 0; i--)
        {
            var child = parent.Controls[i];
            if (!child.Visible) continue;
            if (!includeDisabled && !child.Enabled) continue;

            var (sx, sy) = GetChildScrollOffset(parent);
            var absLeft = offsetX + sx + child.Left;
            var absTop = offsetY + sy + child.Top;

            // Check overlays first so they can be hit even outside parent bounds.
            if (IsPointInOverlay(child, absLeft, absTop, formX, formY, out var localX, out var localY))
            {
                return (child, localX, localY);
            }

            // Normal bounds.
            if (formX >= absLeft && formX < absLeft + child.Width && formY >= absTop && formY < absTop + child.Height)
            {
                // Prefer a deeper child if present.
                if (child.HasChildren)
                {
                    var deep = FindDeepestHitControl(child, formX, formY, absLeft, absTop, includeDisabled);
                    if (deep.control is not null)
                    {
                        return deep;
                    }
                }

                return (child, formX - absLeft, formY - absTop);
            }
        }

        return (null, 0, 0);
    }

    private static (Control? control, int x, int y) FindTopMostOverlayHitControl(Control parent, int formX, int formY, int offsetX, int offsetY, bool includeDisabled)
    {
        for (var i = parent.Controls.Count - 1; i >= 0; i--)
        {
            var child = parent.Controls[i];
            if (!child.Visible) continue;
            if (!includeDisabled && !child.Enabled) continue;

            var (sx, sy) = GetChildScrollOffset(parent);
            var absLeft = offsetX + sx + child.Left;
            var absTop = offsetY + sy + child.Top;

            if (IsPointInOverlay(child, absLeft, absTop, formX, formY, out var localX, out var localY))
            {
                return (child, localX, localY);
            }

            if (child.HasChildren)
            {
                var deep = FindTopMostOverlayHitControl(child, formX, formY, absLeft, absTop, includeDisabled);
                if (deep.control is not null)
                {
                    return deep;
                }
            }
        }

        return (null, 0, 0);
    }

    private static bool IsPointInOverlay(Control control, int absLeft, int absTop, int x, int y, out int localX, out int localY)
    {
        localX = x - absLeft;
        localY = y - absTop;

        if (control is ComboBox comboBox && comboBox.DroppedDown)
        {
            var dd = comboBox.GetDropDownBounds();
            var ddLeft = absLeft + dd.X;
            var ddTop = absTop + dd.Y;
            var ddWidth = dd.Width;
            var ddHeight = dd.Height;

            if (x >= ddLeft && x < ddLeft + ddWidth && y >= ddTop && y < ddTop + ddHeight)
            {
                localX = x - absLeft;
                localY = y - absTop;
                return true;
            }
        }

        if (control is DateTimePicker dateTimePicker && dateTimePicker.DroppedDown)
        {
            var dd = dateTimePicker.GetDropDownBounds();
            var ddLeft = absLeft + dd.X;
            var ddTop = absTop + dd.Y;

            if (x >= ddLeft && x < ddLeft + dd.Width && y >= ddTop && y < ddTop + dd.Height)
            {
                localX = x - absLeft;
                localY = y - absTop;
                return true;
            }
        }

        if (control is TextBox textBox && textBox.HasVisibleAutoComplete)
        {
            var dd = textBox.GetAutoCompletePanelBounds();
            var ddLeft = absLeft + dd.X;
            var ddTop = absTop + dd.Y;

            if (x >= ddLeft && x < ddLeft + dd.Width && y >= ddTop && y < ddTop + dd.Height)
            {
                localX = x - absLeft;
                localY = y - absTop;
                return true;
            }
        }

        return false;
    }

    private void CloseAllOverlays(Control? except)
    {
        CloseAllOverlaysRecursive(this, except);
    }

    private void CloseAllOverlaysRecursive(Control parent, Control? except)
    {
        foreach (var control in parent.Controls)
        {
            if (control is ComboBox comboBox && comboBox != except && comboBox.DroppedDown)
            {
                comboBox.DroppedDown = false;
            }
            else if (control is DateTimePicker dateTimePicker && dateTimePicker != except && dateTimePicker.DroppedDown)
            {
                dateTimePicker.DroppedDown = false;
            }
            else if (control is TextBox textBox && textBox != except)
            {
                textBox.HideAutoCompletePanel();
            }

            if (control.HasChildren)
            {
                CloseAllOverlaysRecursive(control, except);
            }
        }
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

    // ── Public input-dispatch surface ───────────────────────────────────────
    // These thin wrappers let external assemblies (e.g. the server host) route
    // input into the form without needing reflection to reach protected members.

    /// <summary>Dispatches a mouse event into the form's control tree.</summary>
    public void DispatchMouseEvent(string eventType, int x, int y, MouseButtons button)
    {
        var args = new MouseEventArgs(button, 1, x, y);
        switch (eventType)
        {
            case "mousedown":  OnMouseDown(args);       break;
            case "mouseup":    OnMouseUp(args);         break;
            case "mousemove":  OnMouseMove(args);       break;
            case "click":      OnMouseClick(args);      break;
            case "dblclick":   OnMouseDoubleClick(args);break;
        }
    }

    /// <summary>Dispatches a key-down or key-up event into the form.</summary>
    public void DispatchKeyEvent(string eventType, Keys key, bool alt, bool ctrl, bool shift)
    {
        var args = new KeyEventArgs(key, alt, ctrl, shift);
        switch (eventType)
        {
            case "keydown": OnKeyDown(args); break;
            case "keyup":   OnKeyUp(args);   break;
        }
    }

    /// <summary>Dispatches a key-press (character) event into the form.</summary>
    public void DispatchKeyPress(char keyChar)
    {
        OnKeyPress(new KeyPressEventArgs(keyChar));
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
