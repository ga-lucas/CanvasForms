namespace System.Drawing;

/// <summary>
/// Specifies the unit of measure for the given data.
/// </summary>
public enum GraphicsUnit
{
    /// <summary>Specifies the world coordinate system unit as the unit of measure.</summary>
    World      = 0,
    /// <summary>Specifies the unit of measure of the display device.</summary>
    Display    = 1,
    /// <summary>Specifies a device pixel as the unit of measure.</summary>
    Pixel      = 2,
    /// <summary>Specifies a printer's point (1/72 inch) as the unit of measure.</summary>
    Point      = 3,
    /// <summary>Specifies an inch as the unit of measure.</summary>
    Inch       = 4,
    /// <summary>Specifies 1/300 of an inch as the unit of measure.</summary>
    Document   = 5,
    /// <summary>Specifies a millimeter as the unit of measure.</summary>
    Millimeter = 6,
}
