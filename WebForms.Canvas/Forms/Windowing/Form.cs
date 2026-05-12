
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

    // ── Chrome / appearance ──────────────────────────────────────────────────

    private FormBorderStyle _formBorderStyle = FormBorderStyle.Sizable;

    /// <summary>
    /// Gets or sets the border style of the form.
    /// Affects whether the chrome shows resize handles. Fixed styles disable resizing.
    /// </summary>
    public FormBorderStyle FormBorderStyle
    {
        get => _formBorderStyle;
        set
        {
            _formBorderStyle = value;
            // Fixed border styles disable user resizing
            AllowResize = value is FormBorderStyle.Sizable or FormBorderStyle.SizableToolWindow;
            Invalidate();
        }
    }

    /// <summary>Gets or sets whether the Minimize button is shown in the title bar.</summary>
    public bool MinimizeBox { get; set; } = true;

    /// <summary>Gets or sets whether the Maximize button is shown in the title bar.</summary>
    public bool MaximizeBox { get; set; } = true;

    /// <summary>Gets or sets whether the control box (icon + sys-menu + close button) is shown.</summary>
    public bool ControlBox { get; set; } = true;

    private bool _topMost;
    /// <summary>
    /// Gets or sets whether the form is always on top of other forms.
    /// In canvas mode this sets an elevated base z-order.
    /// </summary>
    public bool TopMost
    {
        get => _topMost;
        set
        {
            _topMost = value;
            if (value) BringToFront();
        }
    }

    private double _opacity = 1.0;
    /// <summary>
    /// Gets or sets the opacity of the form (0.0 = transparent, 1.0 = opaque).
    /// Communicated to the canvas host via <see cref="FormOpacity"/>.
    /// </summary>
    public double Opacity
    {
        get => _opacity;
        set => _opacity = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>Opacity value exposed to the JS/Blazor renderer (same as <see cref="Opacity"/>).</summary>
    public double FormOpacity => _opacity;

    /// <summary>
    /// Gets or sets the icon for the form. When set, the browser favicon is updated for the active form.
    /// </summary>
    private Icon? _icon;
    public Icon? Icon
    {
        get => _icon;
        set { _icon = value; Invalidate(); }
    }

    /// <summary>
    /// Gets or sets the <see cref="MenuStrip"/> that is the main menu container for the form.
    /// Setting this provides a hint to the canvas chrome; it does not change the control tree.
    /// </summary>
    public MenuStrip? MainMenuStrip { get; set; }
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

    /// <summary>Occurs when the form loses focus and is no longer the active form.</summary>
    public event EventHandler? Deactivate;

    protected virtual void OnDeactivate(EventArgs e) => Deactivate?.Invoke(this, e);

    /// <summary>
    /// Occurs after the form is first shown (after <see cref="Load"/>).
    /// Matches WinForms <c>Form.Shown</c>.
    /// </summary>
    public event EventHandler? Shown;

    protected virtual void OnShown(EventArgs e) => Shown?.Invoke(this, e);

    /// <summary>
    /// Called by the hosting infrastructure after the form is rendered for the first time.
    /// Fires <see cref="Shown"/>.
    /// </summary>
    public void RaiseShown() => OnShown(EventArgs.Empty);

    /// <summary>Occurs when the user begins resizing the form.</summary>
    public event EventHandler? ResizeBegin;

    protected virtual void OnResizeBegin(EventArgs e) => ResizeBegin?.Invoke(this, e);

    /// <summary>Occurs when the user finishes resizing the form.</summary>
    public event EventHandler? ResizeEnd;

    protected virtual void OnResizeEnd(EventArgs e) => ResizeEnd?.Invoke(this, e);

    /// <summary>Notifies the form that a resize operation has started (called by the chrome host).</summary>
    public void RaiseResizeBegin() => OnResizeBegin(EventArgs.Empty);

    /// <summary>Notifies the form that a resize operation has ended (called by the chrome host).</summary>
    public void RaiseResizeEnd() => OnResizeEnd(EventArgs.Empty);

    /// <summary>Occurs when the form is moved.</summary>
    public new event EventHandler? Move;

    protected override void OnMove(EventArgs e) => Move?.Invoke(this, e);

    /// <summary>Notifies the form that it was moved (called by the chrome host).</summary>
    public void RaiseMove() => OnMove(EventArgs.Empty);

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
    /// Activates the form and gives it focus, bringing it to the front of the z-order.
    /// Matches WinForms <c>Form.Activate()</c>.
    /// </summary>
    public void Activate()
    {
        if (!Visible) Visible = true;
        BringToFront();
        // Focus the form's first focusable child, or the form itself
        var first = GetNextControl(null, true);
        if (first != null)
            FocusedControl = first;
    }

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
                var previous = _focusedControl;
                _focusedControl = value;

                if (previous != null)
                {
                    previous.Focused = false;
                    previous.OnLostFocus(EventArgs.Empty);
                }

                if (value != null)
                {
                    value.Focused = true;
                    value.OnGotFocus(EventArgs.Empty);
                }

                Invalidate();
            }
        }
    }

    // Text measurement service for accurate text rendering
    public TextMeasurementService? TextMeasurementService { get; set; }

    // ── Ownership ────────────────────────────────────────────────────────────

    private Form? _owner;
    private readonly List<Form> _ownedForms = [];

    /// <summary>Gets or sets the form that owns this form.</summary>
    public Form? Owner
    {
        get => _owner;
        set
        {
            if (_owner == value) return;
            _owner?.RemoveOwnedForm(this);
            _owner = value;
            _owner?.AddOwnedForm(this);
        }
    }

    /// <summary>Returns an array of forms that are owned by this form.</summary>
    public Form[] OwnedForms => [.. _ownedForms];

    internal void AddOwnedForm(Form form)
    {
        if (!_ownedForms.Contains(form))
            _ownedForms.Add(form);
    }

    internal void RemoveOwnedForm(Form form) => _ownedForms.Remove(form);

    // ── Dialog support ───────────────────────────────────────────────────────

    private DialogResult _dialogResult = DialogResult.None;

    /// <summary>
    /// Gets or sets the dialog result for the form.
    /// Setting this to anything other than <see cref="DialogResult.None"/> closes a modal dialog.
    /// </summary>
    public DialogResult DialogResult
    {
        get => _dialogResult;
        set
        {
            _dialogResult = value;
            if (value != DialogResult.None && _modalTcs is not null)
            {
                _modalTcs.TrySetResult(value);
                Close(CloseReason.UserClosing);
            }
        }
    }

    private TaskCompletionSource<DialogResult>? _modalTcs;

    /// <summary>
    /// Shows the form as a modal dialog and returns the <see cref="DialogResult"/>.
    /// Awaitable — does not block the WASM thread.
    /// </summary>
    public Task<DialogResult> ShowDialogAsync() => ShowDialogAsync(owner: null);

    /// <summary>
    /// Shows the form as a modal dialog owned by <paramref name="owner"/> and returns the <see cref="DialogResult"/>.
    /// </summary>
    public Task<DialogResult> ShowDialogAsync(Form? owner)
    {
        if (owner != null) Owner = owner;
        _modalTcs = new TaskCompletionSource<DialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _dialogResult = DialogResult.None;
        Show();
        BringToFront();
        return _modalTcs.Task;
    }

    /// <summary>
    /// Synchronous ShowDialog stub — returns <see cref="DialogResult.None"/>.
    /// Use <see cref="ShowDialogAsync()"/> for proper async modal behaviour.
    /// </summary>
    public DialogResult ShowDialog() => ShowDialog(owner: null);

    /// <summary>
    /// Synchronous ShowDialog stub with owner — returns <see cref="DialogResult.None"/>.
    /// Use <see cref="ShowDialogAsync(Form?)"/> for proper async modal behaviour.
    /// </summary>
    public DialogResult ShowDialog(Form? owner)
    {
        if (owner != null) Owner = owner;
        Show();
        BringToFront();
        return _dialogResult;
    }

    // ── ActiveControl (alias for FocusedControl) ─────────────────────────────

    /// <summary>
    /// Gets or sets the active (focused) control on the form.
    /// This is an alias for <see cref="FocusedControl"/>.
    /// </summary>
    public new Control? ActiveControl
    {
        get => FocusedControl;
        set => FocusedControl = value;
    }

    // ── Auto-scaling ─────────────────────────────────────────────────────────

    /// <summary>Stub for designer compat — no-op in canvas mode.</summary>
    public new System.Drawing.SizeF AutoScaleDimensions { get; set; } = new System.Drawing.SizeF(6f, 13f);

    /// <summary>Stub for designer compat — auto-scaling is not needed in canvas mode.</summary>
    public new AutoScaleMode AutoScaleMode { get; set; } = AutoScaleMode.None;

    // ── MDI stubs (Tier 3 — not yet implemented) ─────────────────────────────

    /// <summary>Stub. MDI container support is not yet implemented.</summary>
    // ── MDI ───────────────────────────────────────────────────────────────────

    private bool _isMdiContainer;
    private Form? _mdiParent;
    private readonly List<Form> _mdiChildren = [];
    private Form? _activeMdiChild;

    /// <summary>
    /// Gets or sets whether this form is an MDI parent container.
    /// When true the form hosts MDI child windows in its client area.
    /// </summary>
    public bool IsMdiContainer
    {
        get => _isMdiContainer;
        set { _isMdiContainer = value; Invalidate(); }
    }

    /// <summary>
    /// Gets or sets the MDI parent form for this child form.
    /// Setting this property registers the child with the parent's MDI child list.
    /// </summary>
    public Form? MdiParent
    {
        get => _mdiParent;
        set
        {
            if (_mdiParent == value) return;
            _mdiParent?.RemoveMdiChild(this);
            _mdiParent = value;
            _mdiParent?.AddMdiChild(this);
            OnMdiParentChanged();
        }
    }

    /// <summary>Returns the MDI child forms hosted by this MDI parent.</summary>
    public Form[] MdiChildren => [.. _mdiChildren];

    /// <summary>Gets the currently active MDI child form, or null if none.</summary>
    public Form? ActiveMdiChild => _activeMdiChild;

    /// <summary>Fires when the active MDI child changes.</summary>
    public event EventHandler? MdiChildActivate;

    protected virtual void OnMdiChildActivate(EventArgs e) => MdiChildActivate?.Invoke(this, e);

    /// <summary>Activates the specified MDI child form.</summary>
    public void ActivateMdiChild(Form? child)
    {
        if (child != null && !_mdiChildren.Contains(child)) return;
        if (_activeMdiChild == child) return;
        _activeMdiChild = child;
        child?.BringToFront();
        OnMdiChildActivate(EventArgs.Empty);
        OnMdiChanged();
    }

    /// <summary>
    /// Arranges the MDI child windows according to <paramref name="value"/>.
    /// </summary>
    public void LayoutMdi(MdiLayout value)
    {
        var children = _mdiChildren.Where(c => c.Visible && c.WindowState != FormWindowState.Minimized).ToList();
        if (children.Count == 0) return;

        // Client area available for layout (title bar already excluded by the renderer)
        int cw = ClientWidth;
        int ch = ClientHeight;

        switch (value)
        {
            case MdiLayout.Cascade:
                const int cascadeStep = 24;
                for (int i = 0; i < children.Count; i++)
                {
                    var c = children[i];
                    c.Left = i * cascadeStep;
                    c.Top  = i * cascadeStep;
                    c.Width  = Math.Max(200, cw - i * cascadeStep - cascadeStep);
                    c.Height = Math.Max(150, ch - i * cascadeStep - cascadeStep);
                }
                break;

            case MdiLayout.TileHorizontal:
                int rowH = ch / children.Count;
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Left = 0;
                    children[i].Top  = i * rowH;
                    children[i].Width  = cw;
                    children[i].Height = rowH;
                }
                break;

            case MdiLayout.TileVertical:
                int colW = cw / children.Count;
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].Left = i * colW;
                    children[i].Top  = 0;
                    children[i].Width  = colW;
                    children[i].Height = ch;
                }
                break;

            case MdiLayout.ArrangeIcons:
                // Arrange minimized icons along bottom — stub
                break;
        }

        OnMdiChanged();
    }

    internal void AddMdiChild(Form child)
    {
        if (_mdiChildren.Contains(child)) return;
        _mdiChildren.Add(child);
        // Give child a default position / size if not yet set
        if (child.Width == 0 || child.Height == 0)
        {
            child.Width  = Math.Max(300, ClientWidth  / 2);
            child.Height = Math.Max(200, ClientHeight / 2);
        }
        if (_activeMdiChild == null) ActivateMdiChild(child);
        child.Visible = true;
        OnMdiChanged();
    }

    internal void RemoveMdiChild(Form child)
    {
        _mdiChildren.Remove(child);
        if (_activeMdiChild == child)
            ActivateMdiChild(_mdiChildren.LastOrDefault());
        OnMdiChanged();
    }

    private void OnMdiParentChanged()
    {
        // When assigned to a parent, hide from the top-level desktop (managed inside parent)
        Invalidate();
    }

    /// <summary>Callback to notify the MDI rendering host to refresh.</summary>
    internal Action? OnMdiChanged { get; set; } = () => { };

    // ── StartPosition ─────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the starting position of the form at run time.
    /// <see cref="FormStartPosition.CenterScreen"/> and <see cref="FormStartPosition.CenterParent"/>
    /// are applied when <see cref="ApplyStartPosition"/> is called by the host after the desktop
    /// dimensions are known.
    /// </summary>
    public FormStartPosition StartPosition { get; set; } = FormStartPosition.WindowsDefaultLocation;

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

    // ── Position helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Sets the desktop location of the form (equivalent to setting <see cref="Control.Left"/> and <see cref="Control.Top"/>).
    /// </summary>
    public void SetDesktopLocation(int x, int y) { Left = x; Top = y; }

    /// <summary>
    /// Sets the position and size of the form on the desktop.
    /// </summary>
    public void SetDesktopBounds(int x, int y, int width, int height)
    {
        Left = x; Top = y; Width = width; Height = height;
    }

    /// <summary>
    /// Centers the form on the screen using the supplied desktop dimensions.
    /// </summary>
    public void CenterToScreen(int desktopWidth, int desktopHeight)
    {
        Left = Math.Max(0, (desktopWidth  - Width)  / 2);
        Top  = Math.Max(0, (desktopHeight - Height) / 2);
    }

    /// <summary>
    /// Centers the form over its <see cref="Owner"/>.
    /// Falls back to <see cref="CenterToScreen"/> when there is no owner.
    /// </summary>
    public void CenterToParent(int desktopWidth, int desktopHeight)
    {
        if (Owner != null)
        {
            Left = Owner.Left + (Owner.Width  - Width)  / 2;
            Top  = Owner.Top  + (Owner.Height - Height) / 2;
        }
        else
        {
            CenterToScreen(desktopWidth, desktopHeight);
        }
    }

    /// <summary>
    /// Called by the host after desktop dimensions are known.
    /// Applies <see cref="StartPosition"/> by repositioning the form.
    /// </summary>
    public void ApplyStartPosition(int desktopWidth, int desktopHeight)
    {
        switch (StartPosition)
        {
            case FormStartPosition.CenterScreen:
                CenterToScreen(desktopWidth, desktopHeight);
                break;
            case FormStartPosition.CenterParent:
                CenterToParent(desktopWidth, desktopHeight);
                break;
            // Manual / WindowsDefaultLocation / WindowsDefaultBounds — leave position as-is
        }
    }

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

        // Draw form background — use the clip rectangle so we fill only the
        // client-area buffer passed by FormRenderer (which excludes chrome).
        using var bgBrush = new SolidBrush(BackColor);
        var clip = e.ClipRectangle;
        g.FillRectangle(bgBrush, clip.X, clip.Y, clip.Width, clip.Height);

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

    // ── Tab-order navigation ──────────────────────────────────────────────────

    /// <summary>
    /// Collects all tab-stop controls in tab-index order (depth-first), matching WinForms tab traversal.
    /// </summary>
    private List<Control> GetTabStops()
    {
        var stops = new List<Control>();
        CollectTabStops(this, stops);
        return stops;
    }

    private static void CollectTabStops(Control parent, List<Control> stops)
    {
        // Sort children by TabIndex, then by Controls insertion order for ties
        var sorted = parent.Controls
            .OrderBy(c => c.TabIndex)
            .ToList();

        foreach (var child in sorted)
        {
            if (!child.Visible || !child.Enabled) continue;

            if (child.HasChildren)
            {
                // Container — recurse, but also add the container itself if it's a tab stop
                if (child.TabStop)
                    stops.Add(child);
                CollectTabStops(child, stops);
            }
            else if (child.TabStop)
            {
                stops.Add(child);
            }
        }
    }

    private Control? GetNextTabStop(Control? current)
    {
        var stops = GetTabStops();
        if (stops.Count == 0) return null;
        if (current == null) return stops[0];
        var idx = stops.IndexOf(current);
        if (idx < 0) return stops[0];
        return stops[(idx + 1) % stops.Count];
    }

    private Control? GetPreviousTabStop(Control? current)
    {
        var stops = GetTabStops();
        if (stops.Count == 0) return null;
        if (current == null) return stops[^1];
        var idx = stops.IndexOf(current);
        if (idx < 0) return stops[^1];
        return stops[(idx - 1 + stops.Count) % stops.Count];
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

    // ── Keyboard policy ───────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets whether the form receives key events before they are dispatched to the focused control.
    /// When true, the form's <c>KeyDown</c>, <c>KeyPress</c>, and <c>KeyUp</c> events fire first;
    /// setting <see cref="KeyEventArgs.Handled"/> (or <see cref="KeyPressEventArgs.Handled"/>) suppresses delivery to the control.
    /// </summary>
    public bool KeyPreview { get; set; } = false;

    /// <summary>
    /// Gets or sets the button that is clicked when the user presses Enter,
    /// unless the focused control handles Enter itself (e.g. a multiline TextBox).
    /// </summary>
    public IButtonControl? AcceptButton { get; set; }

    /// <summary>
    /// Gets or sets the button that is clicked when the user presses Escape.
    /// </summary>
    public IButtonControl? CancelButton { get; set; }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        // KeyPreview: let the form see the event first
        if (KeyPreview)
        {
            base.OnKeyDown(e);
            if (e.Handled) return;
        }

        // AcceptButton (Enter) — only when focused control is not a multiline TextBox or button
        if (!e.Handled && e.KeyCode == Keys.Enter && AcceptButton is Control acceptCtrl && acceptCtrl.Enabled)
        {
            var isSelf = FocusedControl is TextBox { Multiline: true } or ButtonBase;
            if (!isSelf)
            {
                e.Handled = true;
                AcceptButton.PerformClick();
                return;
            }
        }

        // CancelButton (Escape)
        if (!e.Handled && e.KeyCode == Keys.Escape && CancelButton is Control cancelCtrl && cancelCtrl.Enabled)
        {
            e.Handled = true;
            CancelButton.PerformClick();
            return;
        }

        // Tab / Shift+Tab — move focus between tab-stop controls
        if (!e.Handled && e.KeyCode == Keys.Tab)
        {
            var next = e.Shift
                ? GetPreviousTabStop(FocusedControl)
                : GetNextTabStop(FocusedControl);
            if (next != null)
            {
                FocusedControl = next;
                e.Handled = true;
                return;
            }
        }

        if (!e.Handled && FocusedControl != null && FocusedControl.Enabled)
        {
            FocusedControl.OnKeyDown(e);
        }
        else if (!e.Handled)
        {
            base.OnKeyDown(e);
        }
    }

    protected internal override void OnKeyUp(KeyEventArgs e)
    {
        if (KeyPreview)
        {
            base.OnKeyUp(e);
            if (e.Handled) return;
        }

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
        if (KeyPreview)
        {
            base.OnKeyPress(e);
            if (e.Handled) return;
        }

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

/// <summary>
/// Specifies the initial position of a form.
/// </summary>
public enum FormStartPosition
{
    /// <summary>The position is determined by the <c>Left</c> and <c>Top</c> properties.</summary>
    Manual = 0,
    /// <summary>Centered on the current screen.</summary>
    CenterScreen = 1,
    /// <summary>Default OS-determined position (treated as Manual in canvas).</summary>
    WindowsDefaultLocation = 2,
    /// <summary>Default OS-determined position and size.</summary>
    WindowsDefaultBounds = 3,
    /// <summary>Centered over the owner form (falls back to CenterScreen when owner is null).</summary>
    CenterParent = 4,
}

/// <summary>
/// Specifies the border style of a form.
/// </summary>
public enum FormBorderStyle
{
    None           = 0,
    FixedSingle    = 1,
    Fixed3D        = 2,
    FixedDialog    = 3,
    Sizable        = 4,
    FixedToolWindow   = 5,
    SizableToolWindow = 6,
}

/// <summary>
/// Specifies the layout of MDI child windows in an MDI parent form.
/// Matches <c>System.Windows.Forms.MdiLayout</c>.
/// </summary>
public enum MdiLayout
{
    /// <summary>All MDI child windows are cascaded within the MDI parent form's client area.</summary>
    Cascade         = 0,
    /// <summary>All MDI child windows are tiled horizontally within the MDI parent form's client area.</summary>
    TileHorizontal  = 1,
    /// <summary>All MDI child windows are tiled vertically within the MDI parent form's client area.</summary>
    TileVertical    = 2,
    /// <summary>All MDI child icons are arranged within the MDI parent form's client area.</summary>
    ArrangeIcons    = 3,
}

/// <summary>
/// Defines the interface for a control that acts as a button (can be clicked via AcceptButton/CancelButton).
/// Matches <c>System.Windows.Forms.IButtonControl</c>.
/// </summary>
public interface IButtonControl
{
    /// <summary>Gets or sets the value returned to the parent form when the button is clicked.</summary>
    DialogResult DialogResult { get; set; }

    /// <summary>Notifies the button that it is the default button and alters appearance accordingly.</summary>
    void NotifyDefault(bool value);

    /// <summary>Programmatically performs a click.</summary>
    void PerformClick();
}
