namespace System.Windows.Forms;

/// <summary>
/// Provides the basic functionality for a control that can contain other controls.
/// Matches System.Windows.Forms.IContainerControl.
/// </summary>
public interface IContainerControl
{
    /// <summary>
    /// Gets or sets the control that is active in the container.
    /// </summary>
    Control? ActiveControl { get; set; }

    /// <summary>
    /// Activates a specified control.
    /// </summary>
    bool ActivateControl(Control active);
}
