
namespace System.Windows.Forms;

public class Form : ContainerControl
{
    private static int _nextZIndex = 1;
    private const int TitleBarHeight = 32; // Height of the title bar
    private Control? _focusedControl;
    private Control? _capturedControl;
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

    internal string GetCursorNameAtClientPoint(int x, int y)
    {
        // Use the same hit-testing logic as input dispatch so cursor matches what will receive the event.
        var hit = FindDeepestHitControl(this, x, y, offsetX: 0, offsetY: 0, includeDisabled: true);
        return hit.control?.Cursor?.Name ?? "default";
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

    // ── Load event ────────────────────────────────────────────────────────────
    // Fired once after the form is fully initialised and shown for the first time.
    // WinForms: Control defines Load + OnLoad; Form inherits it and fires it
    // from WM_LOAD. Translated designer-generated code subscribes via:
    //   this.Load += new System.EventHandler(this.MyForm_Load);

    /// <summary>
    /// Occurs before the form is displayed for the first time.
    /// Matches WinForms <c>Form.Load</c>.
    /// </summary>
    public event EventHandler? Load;

    /// <summary>
    /// Raises the <see cref="Load"/> event.
    /// Override in subclasses to run initialisation code after the form is ready.
    /// Matches WinForms <c>Form.OnLoad(EventArgs)</c>.
    /// </summary>
    protected virtual void OnLoad(EventArgs e) => Load?.Invoke(this, e);

    /// <summary>
    /// Called by the hosting infrastructure after the form tree is ready.
    /// Equivalent to WinForms firing WM_LOAD on the first Show.
    /// </summary>
    public void RaiseLoad() => OnLoad(EventArgs.Empty);

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

            // Clip each control to its own bounds so it can't paint outside its rectangle.
            // This is important for SplitContainer/ListView: columns should not overflow the panel.
            g.Save();
            g.TranslateTransform(childOffsetX, childOffsetY);
            g.SetClip(new Rectangle(0, 0, control.Width, control.Height));

            var controlPaintArgs = new PaintEventArgs(g, new Rectangle(0, 0, control.Width, control.Height));

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

            g.Restore();

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
                g.Save();
                g.TranslateTransform(childOffsetX, childOffsetY);
                var ddArgs = new PaintEventArgs(g, new Rectangle(0, 0, control.Width, control.Height));
                comboBox.PaintDropDownOnly(ddArgs);
                g.Restore();
            }
            else if (control is DateTimePicker dateTimePicker && dateTimePicker.HasVisibleDropDown)
            {
                g.Save();
                g.TranslateTransform(childOffsetX, childOffsetY);
                var ddArgs = new PaintEventArgs(g, new Rectangle(0, 0, control.Width, control.Height));
                dateTimePicker.PaintDropDownOnly(ddArgs);
                g.Restore();
            }
            else if (control is TextBox textBox && textBox.HasVisibleAutoComplete)
            {
                g.Save();
                g.TranslateTransform(childOffsetX, childOffsetY);
                var acArgs = new PaintEventArgs(g, new Rectangle(0, 0, control.Width, control.Height));
                textBox.PaintAutoCompleteOnly(acArgs);
                g.Restore();
            }

            // Paint any open ToolStripMenuItem dropdowns owned by MenuStrip items
            if (control is MenuStrip menuStrip)
            {
                PaintMenuDropDownsRecursive(g, menuStrip.Items, childOffsetX, childOffsetY);
            }

            // Paint ContextMenuStrip if visible
            if (control.ContextMenuStrip is { IsVisible: true } cms)
            {
                PaintDropDownOverlay(g, cms, 0, 0);
            }

            if (control.HasChildren)
            {
                PaintOverlaysRecursive(g, control, childOffsetX, childOffsetY);
            }
        }

        // Also paint any ContextMenuStrip attached to the form itself
        if (parent == this && ContextMenuStrip is { IsVisible: true } formCms)
        {
            PaintDropDownOverlay(g, formCms, 0, 0);
        }
    }

    private static void PaintMenuDropDownsRecursive(Graphics g, ToolStripItemCollection items, int offsetX, int offsetY)
    {
        foreach (var item in items)
        {
            if (item is not ToolStripMenuItem mi) continue;
            if (!mi.HasDropDownItems) continue;

            var dd = mi.DropDown;
            if (dd.IsVisible)
            {
                PaintDropDownOverlay(g, dd, 0, 0);
                // Recurse into open sub-menus
                PaintMenuDropDownsRecursive(g, dd.Items, 0, 0);
            }
        }
    }

    private static void PaintDropDownOverlay(Graphics g, ToolStripDropDown dd, int offsetX, int offsetY)
    {
        var loc = dd.PopupLocation;
        g.Save();
        g.TranslateTransform(loc.X + offsetX, loc.Y + offsetY);
        dd.PaintDropDown(g);
        g.Restore();
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
        UpdateCapturedControl();

        // If a control has mouse capture, it receives all mouse messages.
        // This is required for drag operations (e.g., SplitContainer splitter).
        if (_capturedControl is not null)
        {
            var (capturedX, capturedY) = TranslateToCapturedControl(_capturedControl, e.X, e.Y);
            var capturedArgs = new MouseEventArgs(e.Button, e.Clicks, capturedX, capturedY);
            _capturedControl.OnMouseDown(capturedArgs);
            return;
        }

        var hit = FindDeepestHitControl(this, e.X, e.Y, offsetX: 0, offsetY: 0);
        if (hit.control is null)
        {
            FocusedControl = null;
            CloseAllOverlays(except: null);
            base.OnMouseDown(e);
            return;
        }

        // Right-click: try to show ContextMenuStrip on the hit control (or its ancestors)
        if (e.Button == MouseButtons.Right)
        {
            var cms = FindContextMenuStrip(hit.control);
            if (cms != null)
            {
                CloseAllOverlays(except: null);
                cms.Show(e.X, e.Y);
                return;
            }
        }

        // Check if the click landed inside an open menu dropdown
        if (TryRouteMouseToMenuDropDown(e.X, e.Y, e))
            return;

        FocusedControl = hit.control;
        CloseAllOverlays(except: hit.control);

        var controlArgs = new MouseEventArgs(e.Button, e.Clicks, hit.x, hit.y);
        hit.control.OnMouseDown(controlArgs);
    }

    /// <summary>
    /// Routes a mouse-down to the deepest open ToolStripDropDown at the given
    /// form coordinates. Returns true if the event was consumed.
    /// </summary>
    private bool TryRouteMouseToMenuDropDown(int formX, int formY, MouseEventArgs e)
    {
        if (TryRouteToDropDowns(GetAllMenuStrips(this), formX, formY)) return true;
        if (TryRouteToContextMenuStrip(this, formX, formY)) return true;
        return false;
    }

    private static bool TryRouteToDropDowns(IEnumerable<MenuStrip> strips, int formX, int formY)
    {
        foreach (var ms in strips)
        {
            foreach (var item in ms.Items)
            {
                if (item is ToolStripMenuItem mi && mi.DropDownIsOpen)
                {
                    if (RouteToDropDown(mi.DropDown, formX, formY)) return true;
                }
            }
        }
        return false;
    }

    private static bool TryRouteToContextMenuStrip(Control root, int formX, int formY)
    {
        if (root.ContextMenuStrip is { IsVisible: true } cms)
        {
            if (RouteToDropDown(cms, formX, formY)) return true;
        }
        foreach (var child in root.Controls)
        {
            if (TryRouteToContextMenuStrip(child, formX, formY)) return true;
        }
        return false;
    }

    private static bool RouteToDropDown(ToolStripDropDown dd, int formX, int formY)
    {
        if (!dd.IsVisible) return false;
        var loc = dd.PopupLocation;
        var w   = dd.ComputeDropWidth();
        var h   = dd.ComputeDropHeight();
        if (formX >= loc.X && formX < loc.X + w && formY >= loc.Y && formY < loc.Y + h)
        {
            int lx = formX - loc.X;
            int ly = formY - loc.Y;
            // First check sub-menus
            var idx = dd.GetItemIndexAt(ly);
            if (idx >= 0 && idx < dd.Items.Count && dd.Items[idx] is ToolStripMenuItem mi && mi.DropDownIsOpen)
            {
                if (RouteToDropDown(mi.DropDown, formX, formY)) return true;
            }
            dd.HandleMouseDown(lx, ly);
            return true;
        }
        return false;
    }

    private static IEnumerable<MenuStrip> GetAllMenuStrips(Control root)
    {
        foreach (var child in root.Controls)
        {
            if (child is MenuStrip ms) yield return ms;
            foreach (var sub in GetAllMenuStrips(child)) yield return sub;
        }
    }

    private static ContextMenuStrip? FindContextMenuStrip(Control? control)
    {
        while (control != null)
        {
            if (control.ContextMenuStrip != null) return control.ContextMenuStrip;
            control = control.Parent;
        }
        return null;
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        UpdateCapturedControl();

        // If a control has mouse capture, it receives all mouse messages.
        if (_capturedControl is not null)
        {
            var (capturedX, capturedY) = TranslateToCapturedControl(_capturedControl, e.X, e.Y);
            var capturedArgs = new MouseEventArgs(e.Button, e.Clicks, capturedX, capturedY);
            _capturedControl.OnMouseUp(capturedArgs);

            // Capture may have been released during OnMouseUp.
            UpdateCapturedControl();
            return;
        }

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
        UpdateCapturedControl();

        // If a control has mouse capture, it receives all mouse messages.
        if (_capturedControl is not null)
        {
            var (capturedX, capturedY) = TranslateToCapturedControl(_capturedControl, e.X, e.Y);
            var capturedArgs = new MouseEventArgs(e.Button, e.Clicks, capturedX, capturedY);
            _capturedControl.OnMouseMove(capturedArgs);
            return;
        }

        // Route hover into open menu dropdowns so item highlight updates.
        if (TryRouteMouseMoveToMenuDropDown(e.X, e.Y)) return;

        var hit = FindDeepestHitControl(this, e.X, e.Y, offsetX: 0, offsetY: 0, includeDisabled: true);
        if (hit.control is not null)
        {
            var controlArgs = new MouseEventArgs(e.Button, e.Clicks, hit.x, hit.y);
            hit.control.OnMouseMove(controlArgs);
            return;
        }

        base.OnMouseMove(e);
    }

    // ── Menu overlay mouse-move routing ───────────────────────────────────────

    private bool TryRouteMouseMoveToMenuDropDown(int formX, int formY)
    {
        foreach (var ms in GetAllMenuStrips(this))
        {
            foreach (var item in ms.Items)
            {
                if (item is ToolStripMenuItem mi && mi.DropDownIsOpen)
                    if (RouteMoveToDdChain(mi.DropDown, formX, formY)) return true;
            }
        }
        return TryRouteMoveToContextMenuStrip(this, formX, formY);
    }

    private static bool TryRouteMoveToContextMenuStrip(Control root, int formX, int formY)
    {
        if (root.ContextMenuStrip is { IsVisible: true } cms)
            if (RouteMoveToDdChain(cms, formX, formY)) return true;
        foreach (var child in root.Controls)
            if (TryRouteMoveToContextMenuStrip(child, formX, formY)) return true;
        return false;
    }

    private static bool RouteMoveToDdChain(ToolStripDropDown dd, int formX, int formY)
    {
        if (!dd.IsVisible) return false;
        var loc = dd.PopupLocation;
        var w   = dd.ComputeDropWidth();
        var h   = dd.ComputeDropHeight();
        if (formX >= loc.X && formX < loc.X + w && formY >= loc.Y && formY < loc.Y + h)
        {
            dd.HandleMouseMove(formX - loc.X, formY - loc.Y);
            return true;
        }
        // Also check open sub-menus even when pointer is outside this level
        foreach (var item in dd.Items)
            if (item is ToolStripMenuItem mi && mi.DropDownIsOpen)
                if (RouteMoveToDdChain(mi.DropDown, formX, formY)) return true;
        return false;
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

        // ToolStripMenuItem dropdowns (MenuStrip)
        if (control is MenuStrip menuStrip)
        {
            if (IsPointInMenuDropDowns(menuStrip.Items, x, y, out localX, out localY))
                return true;
        }

        // ContextMenuStrip
        if (control.ContextMenuStrip is { IsVisible: true } cms)
        {
            var loc = cms.PopupLocation;
            var w   = cms.ComputeDropWidth();
            var h   = cms.ComputeDropHeight();
            if (x >= loc.X && x < loc.X + w && y >= loc.Y && y < loc.Y + h)
            {
                localX = x - loc.X;
                localY = y - loc.Y;
                return true;
            }
        }

        return false;
    }

    private static bool IsPointInMenuDropDowns(ToolStripItemCollection items, int x, int y, out int localX, out int localY)
    {
        localX = 0; localY = 0;
        foreach (var item in items)
        {
            if (item is not ToolStripMenuItem mi || !mi.HasDropDownItems) continue;
            var dd = mi.DropDown;
            if (!dd.IsVisible) continue;

            var loc = dd.PopupLocation;
            var w   = dd.ComputeDropWidth();
            var h   = dd.ComputeDropHeight();
            if (x >= loc.X && x < loc.X + w && y >= loc.Y && y < loc.Y + h)
            {
                localX = x - loc.X;
                localY = y - loc.Y;
                return true;
            }
            // Recurse into open sub-menus
            if (IsPointInMenuDropDowns(dd.Items, x, y, out localX, out localY))
                return true;
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

            // Close any open MenuStrip dropdowns
            if (control is MenuStrip menuStrip && control != except)
            {
                CloseMenuStripDropDowns(menuStrip.Items);
            }

            // Close any open ContextMenuStrip
            if (control.ContextMenuStrip is { IsVisible: true } cms && control != except)
            {
                cms.Close();
            }

            if (control.HasChildren)
            {
                CloseAllOverlaysRecursive(control, except);
            }
        }

        // Close form-level ContextMenuStrip
        if (ContextMenuStrip is { IsVisible: true } formCms)
            formCms.Close();
    }

    private static void CloseMenuStripDropDowns(ToolStripItemCollection items)
    {
        foreach (var item in items)
        {
            if (item is ToolStripMenuItem mi && mi.DropDownIsOpen)
                mi.CloseDropDown();
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

    private void UpdateCapturedControl()
    {
        var found = FindCapturedControl(this);
        if (!ReferenceEquals(_capturedControl, found))
        {
            _capturedControl = found;
        }
    }

    private static Control? FindCapturedControl(Control parent)
    {
        if (parent.Capture) return parent;

        foreach (var child in parent.Controls)
        {
            if (!child.Visible) continue;

            var deep = FindCapturedControl(child);
            if (deep is not null) return deep;
        }

        return null;
    }

    private static (int x, int y) TranslateToCapturedControl(Control captured, int formX, int formY)
    {
        var (left, top) = GetAbsoluteClientPosition(captured);
        return (formX - left, formY - top);
    }

    private static (int left, int top) GetAbsoluteClientPosition(Control control)
    {
        var x = 0;
        var y = 0;
        var current = control;

        while (current.Parent is not null)
        {
            var parent = current.Parent;
            var (sx, sy) = GetChildScrollOffset(parent);
            x += sx + current.Left;
            y += sy + current.Top;
            current = parent;
        }

        return (x, y);
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
