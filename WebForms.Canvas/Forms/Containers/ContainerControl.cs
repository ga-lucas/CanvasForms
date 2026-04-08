namespace System.Windows.Forms;

public class ContainerControl : ScrollableControl
{
    public Control? ActiveControl { get; set; }

    public bool AutoValidate { get; set; } = true;

    public System.Drawing.SizeF AutoScaleDimensions { get; set; } = new System.Drawing.SizeF(6f, 13f);

    public AutoScaleMode AutoScaleMode { get; set; } = AutoScaleMode.Font;

    public System.Drawing.SizeF CurrentAutoScaleDimensions => AutoScaleDimensions;

    protected virtual void PerformAutoScale()
    {
        // Stub: real WinForms computes scaling based on font/DPI.
    }

    public virtual void ValidateChildren()
    {
        // No-op in canvas host.
    }
}

public enum AutoScaleMode
{
    None = 0,
    Font = 1,
    Dpi = 2,
    Inherit = 3,
}
