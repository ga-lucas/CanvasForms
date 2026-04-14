namespace System.Windows.Forms;

/// <summary>
/// Base class for modal dialog boxes.
/// Minimal WinForms-compatible surface for Canvas.Windows.Forms.
/// </summary>
public abstract class CommonDialog
{
    public virtual void Reset()
    {
    }

    public DialogResult ShowDialog() => ShowDialog(owner: null);

    public virtual DialogResult ShowDialog(IWin32Window? owner)
    {
        Reset();
        return RunDialog(owner);
    }

    protected abstract DialogResult RunDialog(IWin32Window? owner);
}
