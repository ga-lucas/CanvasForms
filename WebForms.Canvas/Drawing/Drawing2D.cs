namespace System.Drawing.Drawing2D;

// ── Enumerations ──────────────────────────────────────────────────────────────

/// <summary>Specifies the overall quality of rendering.</summary>
public enum SmoothingMode
{
    Invalid       = -1,
    Default       = 0,
    HighSpeed     = 1,
    HighQuality   = 2,
    None          = 3,
    AntiAlias     = 4,
}

/// <summary>Specifies the algorithm used when scaling images.</summary>
public enum InterpolationMode
{
    Invalid             = -1,
    Default             = 0,
    Low                 = 1,
    High                = 2,
    Bilinear            = 3,
    Bicubic             = 4,
    NearestNeighbor     = 5,
    HighQualityBilinear = 6,
    HighQualityBicubic  = 7,
}

/// <summary>Specifies the quality level of compositing.</summary>
public enum CompositingQuality
{
    Invalid          = -1,
    Default          = 0,
    HighSpeed        = 1,
    HighQuality      = 2,
    GammaCorrected   = 3,
    AssumeLinear     = 4,
}

/// <summary>Specifies how to join two lines or curves.</summary>
public enum LineJoin
{
    Miter        = 0,
    Bevel        = 1,
    Round        = 2,
    MiterClipped = 3,
}

/// <summary>Specifies the cap style at the end of a line.</summary>
public enum LineCap
{
    Flat          = 0,
    Square        = 1,
    Round         = 2,
    Triangle      = 3,
    NoAnchor      = 16,
    SquareAnchor  = 17,
    RoundAnchor   = 18,
    DiamondAnchor = 19,
    ArrowAnchor   = 20,
    Custom        = 255,
    AnchorMask    = 240,
}

/// <summary>Specifies the direction in which a linear gradient progresses.</summary>
public enum LinearGradientMode
{
    Horizontal        = 0,
    Vertical          = 1,
    ForwardDiagonal   = 2,
    BackwardDiagonal  = 3,
}

/// <summary>Specifies a hatch pattern used to fill shapes.</summary>
public enum HatchStyle
{
    Horizontal                   = 0,
    Vertical                     = 1,
    ForwardDiagonal              = 2,
    BackwardDiagonal             = 3,
    Cross                        = 4,
    DiagonalCross                = 5,
    Percent05                    = 6,
    Percent10                    = 7,
    Percent20                    = 8,
    Percent25                    = 9,
    Percent30                    = 10,
    Percent40                    = 11,
    Percent50                    = 12,
    Percent60                    = 13,
    Percent70                    = 14,
    Percent75                    = 15,
    Percent80                    = 16,
    Percent90                    = 17,
    LightDownwardDiagonal        = 18,
    LightUpwardDiagonal          = 19,
    DarkDownwardDiagonal         = 20,
    DarkUpwardDiagonal           = 21,
    WideDownwardDiagonal         = 22,
    WideUpwardDiagonal           = 23,
    LightVertical                = 24,
    LightHorizontal              = 25,
    NarrowVertical               = 26,
    NarrowHorizontal             = 27,
    DarkVertical                 = 28,
    DarkHorizontal               = 29,
    DashedDownwardDiagonal       = 30,
    DashedUpwardDiagonal         = 31,
    DashedHorizontal             = 32,
    DashedVertical               = 33,
    SmallConfetti                = 34,
    LargeConfetti                = 35,
    ZigZag                       = 36,
    Wave                         = 37,
    DiagonalBrick                = 38,
    HorizontalBrick              = 39,
    Weave                        = 40,
    Plaid                        = 41,
    Divot                        = 42,
    DottedGrid                   = 43,
    DottedDiamond                = 44,
    Shingle                      = 45,
    Trellis                      = 46,
    Sphere                       = 47,
    SmallGrid                    = 48,
    SmallCheckerBoard            = 49,
    LargeCheckerBoard            = 50,
    OutlinedDiamond              = 51,
    SolidDiamond                 = 52,
    LargeGrid                    = Cross,
    Min                          = Horizontal,
    Max                          = SolidDiamond,
}

/// <summary>Specifies how the fill of a path gradient behaves at its boundary.</summary>
public enum WrapMode
{
    Tile       = 0,
    TileFlipX  = 1,
    TileFlipY  = 2,
    TileFlipXY = 3,
    Clamp      = 4,
}

// ── Stubs ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Stub for <c>System.Drawing.Drawing2D.GraphicsPath</c>.
/// All geometry building calls are accepted but not stored — canvas rendering
/// does not use GDI+ paths.
/// </summary>
public sealed class GraphicsPath : IDisposable
{
    public void AddLine(float x1, float y1, float x2, float y2) { }
    public void AddLine(System.Drawing.Point pt1, System.Drawing.Point pt2) { }
    public void AddRectangle(System.Drawing.Rectangle rect) { }
    public void AddEllipse(float x, float y, float width, float height) { }
    public void AddEllipse(System.Drawing.Rectangle rect) { }
    public void AddArc(float x, float y, float w, float h, float startAngle, float sweepAngle) { }
    public void AddBezier(float x1, float y1, float x2, float y2,
                          float x3, float y3, float x4, float y4) { }
    public void AddPolygon(System.Drawing.Point[] points) { }
    public void AddString(string s, System.Drawing.FontFamily? family, int style, float emSize,
                          System.Drawing.Point origin, System.Drawing.StringFormat? format) { }
    public void CloseFigure() { }
    public void StartFigure() { }
    public System.Drawing.RectangleF GetBounds() => System.Drawing.RectangleF.Empty;
    public void Dispose() { }
}

/// <summary>
/// Stub for <c>System.Drawing.Drawing2D.Matrix</c> (2D affine transform).
/// Operations are accepted but not applied — canvas transforms are not supported
/// through this stub.
/// </summary>
public sealed class Matrix : IDisposable
{
    public bool IsIdentity => true;
    public float[] Elements => new float[] { 1, 0, 0, 1, 0, 0 };
    public void Reset() { }
    public void Translate(float dx, float dy) { }
    public void Scale(float sx, float sy) { }
    public void Rotate(float angle) { }
    public void Invert() { }
    public Matrix Clone() => new Matrix();
    public void Dispose() { }
}

/// <summary>
/// Stub for <c>System.Drawing.Drawing2D.LinearGradientBrush</c>.
/// </summary>
public sealed class LinearGradientBrush : System.Drawing.Brush
{
    public LinearGradientBrush(
        System.Drawing.Point point1, System.Drawing.Point point2,
        System.Drawing.Color color1, System.Drawing.Color color2) { }

    public LinearGradientBrush(
        System.Drawing.Rectangle rect,
        System.Drawing.Color color1, System.Drawing.Color color2,
        LinearGradientMode mode) { }

    public LinearGradientBrush(
        System.Drawing.Rectangle rect,
        System.Drawing.Color color1, System.Drawing.Color color2,
        float angle) { }

    public WrapMode WrapMode { get; set; } = WrapMode.Tile;
}

/// <summary>
/// Stub for <c>System.Drawing.Drawing2D.HatchBrush</c>.
/// </summary>
public sealed class HatchBrush : System.Drawing.Brush
{
    public HatchStyle HatchStyle { get; }
    public System.Drawing.Color ForegroundColor { get; }
    public System.Drawing.Color BackgroundColor { get; }

    public HatchBrush(HatchStyle style, System.Drawing.Color foreColor)
    {
        HatchStyle       = style;
        ForegroundColor  = foreColor;
        BackgroundColor  = System.Drawing.Color.Transparent;
    }

    public HatchBrush(HatchStyle style, System.Drawing.Color foreColor, System.Drawing.Color backColor)
    {
        HatchStyle       = style;
        ForegroundColor  = foreColor;
        BackgroundColor  = backColor;
    }
}

/// <summary>
/// Stub for <c>System.Drawing.Drawing2D.PathGradientBrush</c>.
/// </summary>
public sealed class PathGradientBrush : System.Drawing.Brush
{
    public System.Drawing.Color CenterColor { get; set; }
    public System.Drawing.Color[] SurroundColors { get; set; } = Array.Empty<System.Drawing.Color>();

    public PathGradientBrush(System.Drawing.Point[] points) { }
    public PathGradientBrush(GraphicsPath path) { }
}
