namespace System.Windows.Forms;

/// <summary>
/// Specifies the border style for a control.
/// </summary>
public enum BorderStyle
{
    None = 0,
    FixedSingle = 1,
    Fixed3D = 2
}

/// <summary>
/// Specifies the style of a 3D border used by ToolStripStatusLabel and similar controls.
/// </summary>
public enum Border3DStyle
{
    Flat        = 0x000A,
    Adjust      = 0x2000,
    Bump        = 0x0009,
    Etched      = 0x0006,
    Raised      = 0x0005,
    RaisedInner = 0x0004,
    RaisedOuter = 0x0001,
    Recess      = 0x000C,
    Sunken      = 0x000A,
    SunkenInner = 0x0008,
    SunkenOuter = 0x0002
}
