namespace System.Windows.Forms;

// ── ContextMenuStrip ──────────────────────────────────────────────────────────
/// <summary>
/// A floating context menu that appears at a specific position.
/// Attach to a control via <c>control.ContextMenuStrip = this</c> and
/// it will be shown automatically on right-click, or call Show() manually.
/// Matches the WinForms ContextMenuStrip API.
/// </summary>
public class ContextMenuStrip : ToolStripDropDownMenu
{
    private Control? _sourceControl;

    // ── Events ─────────────────────────────────────────────────────────────────

    // ContextMenuStrip re-surfaces the inherited ToolStripDropDown events with the
    // WinForms signatures expected by designer-generated translated apps.

    /// <summary>Raised before the menu is displayed.  Set Cancel=true to suppress.</summary>
    public new event CancelEventHandler? Opening;

    /// <summary>Raised after the menu is displayed.</summary>
    public new event EventHandler? Opened;

    /// <summary>Raised when the menu is about to close.</summary>
    public new event ToolStripDropDownClosingEventHandler? Closing;

    /// <summary>Raised after the menu has closed.</summary>
    public new event EventHandler? Closed;

    // ── Constructors ───────────────────────────────────────────────────────────

    public ContextMenuStrip() { }

    public ContextMenuStrip(System.ComponentModel.IContainer container)
    {
        container?.Add(new System.ComponentModel.Component());
    }

    // ── Source control ────────────────────────────────────────────────────────

    /// <summary>The control that triggered the context menu.</summary>
    public Control? SourceControl => _sourceControl;

    // ── Show overloads (mirrors WinForms API) ──────────────────────────────────

    /// <summary>Shows the menu at the given point (screen/form coordinates).</summary>
    public void Show(Point location)
    {
        _sourceControl = null;
        PopupLocation  = location;
        OpenInternal();
    }

    /// <summary>Shows the menu at screen/form coordinates.</summary>
    public void Show(int x, int y) => Show(new Point(x, y));

    /// <summary>Shows the menu relative to a control.</summary>
    public void Show(Control control, Point position)
    {
        _sourceControl = control;
        var formPt     = GetControlFormPosition(control);
        PopupLocation  = new Point(formPt.X + position.X, formPt.Y + position.Y);
        OpenInternal();
    }

    public void Show(Control control, int x, int y) => Show(control, new Point(x, y));

    /// <summary>Shows the menu relative to a control in the given direction (stub — direction ignored).</summary>
    public void Show(Control control, Point position, ToolStripDropDownDirection direction)
        => Show(control, position);

    // ── Open / Close ───────────────────────────────────────────────────────────

    /// <summary>Opens the menu, raising Opening (cancellable) then Opened.</summary>
    public void Open()
    {
        OpenInternal();
    }

    private void OpenInternal()
    {
        var cancelArgs = new CancelEventArgs();
        Opening?.Invoke(this, cancelArgs);
        if (cancelArgs.Cancel) return;

        IsVisible = true;
        Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Closes the menu, raising Closing (cancellable with reason) then Closed.</summary>
    public new void Close()
    {
        if (!IsVisible) return;
        var args = new ToolStripDropDownClosingEventArgs(ToolStripDropDownCloseReason.CloseCalled);
        Closing?.Invoke(this, args);
        if (args.Cancel) return;

        CloseChain();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Closes the menu with the specified reason.</summary>
    public void Close(ToolStripDropDownCloseReason reason)
    {
        if (!IsVisible) return;
        var args = new ToolStripDropDownClosingEventArgs(reason);
        Closing?.Invoke(this, args);
        if (args.Cancel) return;

        CloseChain();
        Closed?.Invoke(this, EventArgs.Empty);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Point GetControlFormPosition(Control control)
    {
        int x = control.Left, y = control.Top;
        var p = control.Parent;
        while (p != null && p is not Form)
        {
            x += p.Left;
            y += p.Top;
            p  = p.Parent;
        }
        return new Point(x, y);
    }
}
