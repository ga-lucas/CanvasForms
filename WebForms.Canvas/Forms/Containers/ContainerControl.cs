namespace System.Windows.Forms;

/// <summary>Specifies which controls are validated when <see cref="ContainerControl.ValidateChildren()"/> is called.</summary>
[Flags]
public enum ValidationConstraints
{
    None         = 0x00,
    Selectable   = 0x01,
    Enabled      = 0x02,
    Visible      = 0x04,
    TabStop      = 0x08,
    ImmediateChildren = 0x10,
}

public class ContainerControl : ScrollableControl
{
    public Control? ActiveControl { get; set; }

    public AutoValidate AutoValidate { get; set; } = AutoValidate.EnablePreventFocusChange;

    public System.Drawing.SizeF AutoScaleDimensions { get; set; } = new System.Drawing.SizeF(6f, 13f);

    public AutoScaleMode AutoScaleMode { get; set; } = AutoScaleMode.Font;

    public System.Drawing.SizeF CurrentAutoScaleDimensions => AutoScaleDimensions;

    protected virtual void PerformAutoScale()
    {
        // Stub: real WinForms computes scaling based on font/DPI.
    }

    /// <summary>
    /// Validates all child controls in the container, firing <see cref="Control.Validating"/>
    /// and <see cref="Control.Validated"/> on each control where
    /// <see cref="Control.CausesValidation"/> is <c>true</c>.
    /// </summary>
    /// <returns><c>true</c> if all validations pass; <c>false</c> if any handler cancels.</returns>
    public virtual bool ValidateChildren()
        => ValidateChildren(ValidationConstraints.Selectable | ValidationConstraints.Enabled | ValidationConstraints.Visible);

    /// <summary>
    /// Validates child controls matching the given <paramref name="validationConstraints"/>.
    /// </summary>
    public virtual bool ValidateChildren(ValidationConstraints validationConstraints)
    {
        bool allValid = true;
        ValidateDescendants(this, validationConstraints, ref allValid);
        return allValid;
    }

    private static void ValidateDescendants(Control parent, ValidationConstraints constraints, ref bool allValid)
    {
        bool immediateOnly = (constraints & ValidationConstraints.ImmediateChildren) != 0;

        foreach (Control child in parent.Controls)
        {
            // Apply constraints
            if ((constraints & ValidationConstraints.Enabled)    != 0 && !child.Enabled)    continue;
            if ((constraints & ValidationConstraints.Visible)    != 0 && !child.Visible)    continue;
            if ((constraints & ValidationConstraints.TabStop)    != 0 && !child.TabStop)    continue;
            if ((constraints & ValidationConstraints.Selectable) != 0 && !child.CanSelect)  continue;

            if (child.CausesValidation)
            {
                if (!child.Validate())
                    allValid = false;
            }

            if (!immediateOnly)
                ValidateDescendants(child, constraints, ref allValid);
        }
    }
}

public enum AutoScaleMode
{
    None = 0,
    Font = 1,
    Dpi = 2,
    Inherit = 3,
}

/// <summary>Specifies the automatic validation behavior when focus changes.</summary>
public enum AutoValidate
{
    Disable = 0,
    EnablePreventFocusChange = 1,
    EnableAllowFocusChange = 2,
    Inherit = 3,
}
