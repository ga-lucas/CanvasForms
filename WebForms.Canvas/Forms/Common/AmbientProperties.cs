namespace System.Windows.Forms;

/// <summary>
/// Provides ambient property values for a <see cref="ContainerControl"/>.
/// Ambient properties are properties that, if not set, are obtained from the parent control.
/// This stub stores the values and returns them on request.
/// </summary>
public sealed class AmbientProperties
{
    /// <summary>Gets or sets the ambient background color.</summary>
    public System.Drawing.Color BackColor { get; set; } = System.Drawing.Color.Empty;

    /// <summary>Gets or sets the ambient cursor.</summary>
    public Cursor? Cursor { get; set; }

    /// <summary>Gets or sets the ambient font.</summary>
    public Font? Font { get; set; }

    /// <summary>Gets or sets the ambient foreground color.</summary>
    public System.Drawing.Color ForeColor { get; set; } = System.Drawing.Color.Empty;
}
