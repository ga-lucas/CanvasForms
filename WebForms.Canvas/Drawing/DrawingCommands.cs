using System.Text;

namespace Canvas.Windows.Forms.Drawing;

// Represents drawing commands that can be serialized to JavaScript
public abstract class DrawingCommand
{
    public abstract string ToJavaScript();

    // Structured command representation to avoid building/executing JS source strings.
    // Format: object[] where [0] is an int opcode and remaining entries are primitive args.
    public abstract object[] ToCommand();
}

internal static class CanvasCommandOp
{
    public const int StrokeLine    = 1;
    public const int StrokeRect    = 2;
    public const int FillRect      = 3;
    public const int StrokeEllipse = 4;
    public const int FillEllipse   = 5;
    public const int DrawText      = 6;
    public const int Clear         = 7;
    public const int Save          = 8;
    public const int Restore       = 9;
    public const int ClipRect      = 10;
    public const int DrawImage     = 11;
    // ── New ops ────────────────────────────────────────────────
    public const int StrokeRoundRect       = 12;
    public const int FillRoundRect         = 13;
    public const int FillLinearGradient    = 14;  // rect or any shape via path
    public const int FillRadialGradient    = 15;
    public const int DrawPath              = 16;
    public const int FillPath              = 17;
    public const int DrawArc               = 18;
    public const int DrawBezier            = 19;
    public const int DrawPolygon           = 20;
    public const int FillPolygon           = 21;
    public const int TranslateTransform    = 22;
}

/// <summary>
/// Serialises a <see cref="Pen"/>'s <see cref="DashStyle"/> into a compact string
/// that the canvas renderer can pass to <c>ctx.setLineDash()</c>.
/// Format: "d:&lt;int&gt;" or for Custom "d:C:&lt;f1&gt;,&lt;f2&gt;,..."
/// </summary>
internal static class PenHelper
{
    /// <summary>Returns a compact dash descriptor appended to stroke command arrays.</summary>
    public static string ToDashToken(Pen pen)
    {
        if (pen.DashStyle == DashStyle.Custom && pen.DashPattern is { Length: > 0 })
            return "d:C:" + string.Join(",", pen.DashPattern);
        return $"d:{(int)pen.DashStyle}";
    }
}

public class DrawLineCommand : DrawingCommand
{
    public Pen Pen { get; }
    public int X1 { get; }
    public int Y1 { get; }
    public int X2 { get; }
    public int Y2 { get; }

    public DrawLineCommand(Pen pen, int x1, int y1, int x2, int y2)
    {
        Pen = pen;
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ctx.strokeStyle = '{Pen.Color.ToRgbaString()}';");
        sb.AppendLine($"ctx.lineWidth = {Pen.Width};");
        sb.AppendLine("ctx.beginPath();");
        sb.AppendLine($"ctx.moveTo({X1}, {Y1});");
        sb.AppendLine($"ctx.lineTo({X2}, {Y2});");
        sb.AppendLine("ctx.stroke();");
        return sb.ToString();
    }

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.StrokeLine, X1, Y1, X2, Y2, Pen.Width, Pen.Color.ToRgbaString(), PenHelper.ToDashToken(Pen) };
}

public class DrawRectangleCommand : DrawingCommand
{
    public Pen Pen { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public DrawRectangleCommand(Pen pen, int x, int y, int width, int height)
    {
        Pen = pen;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ctx.strokeStyle = '{Pen.Color.ToRgbaString()}';");
        sb.AppendLine($"ctx.lineWidth = {Pen.Width};");
        sb.AppendLine($"ctx.strokeRect({X}, {Y}, {Width}, {Height});");
        return sb.ToString();
    }

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.StrokeRect, X, Y, Width, Height, Pen.Width, Pen.Color.ToRgbaString(), PenHelper.ToDashToken(Pen) };
}

public class FillRectangleCommand : DrawingCommand
{
    public Brush Brush { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public FillRectangleCommand(Brush brush, int x, int y, int width, int height)
    {
        Brush = brush;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        if (Brush is SolidBrush solidBrush)
        {
            sb.AppendLine($"ctx.fillStyle = '{solidBrush.Color.ToRgbaString()}';");
        }
        sb.AppendLine($"ctx.fillRect({X}, {Y}, {Width}, {Height});");
        return sb.ToString();
    }

    public override object[] ToCommand()
    {
        var fill = BrushHelper.ToFillStyle(Brush, X, Y, Width, Height);
        return new object[] { CanvasCommandOp.FillRect, X, Y, Width, Height, fill };
    }
}

// ── FillRectangleWithGradient (FillRect with gradient brush) ──────────────────
// FillRectangleCommand.ToCommand() is overridden to emit FillLinearGradient/FillRadialGradient
// when the brush is not solid; handled in the same JS case via the colour-string prefix.

public class DrawEllipseCommand : DrawingCommand
{
    public Pen Pen { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public DrawEllipseCommand(Pen pen, int x, int y, int width, int height)
    {
        Pen = pen;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        var centerX = X + Width / 2.0;
        var centerY = Y + Height / 2.0;
        var radiusX = Width / 2.0;
        var radiusY = Height / 2.0;

        sb.AppendLine($"ctx.strokeStyle = '{Pen.Color.ToRgbaString()}';");
        sb.AppendLine($"ctx.lineWidth = {Pen.Width};");
        sb.AppendLine("ctx.beginPath();");
        sb.AppendLine($"ctx.ellipse({centerX}, {centerY}, {radiusX}, {radiusY}, 0, 0, 2 * Math.PI);");
        sb.AppendLine("ctx.stroke();");
        return sb.ToString();
    }

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.StrokeEllipse, X, Y, Width, Height, Pen.Width, Pen.Color.ToRgbaString(), PenHelper.ToDashToken(Pen) };
}

public class FillEllipseCommand : DrawingCommand
{
    public Brush Brush { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public FillEllipseCommand(Brush brush, int x, int y, int width, int height)
    {
        Brush = brush;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        var centerX = X + Width / 2.0;
        var centerY = Y + Height / 2.0;
        var radiusX = Width / 2.0;
        var radiusY = Height / 2.0;

        if (Brush is SolidBrush solidBrush)
        {
            sb.AppendLine($"ctx.fillStyle = '{solidBrush.Color.ToRgbaString()}';");
        }
        sb.AppendLine("ctx.beginPath();");
        sb.AppendLine($"ctx.ellipse({centerX}, {centerY}, {radiusX}, {radiusY}, 0, 0, 2 * Math.PI);");
        sb.AppendLine("ctx.fill();");
        return sb.ToString();
    }

    public override object[] ToCommand()
    {
        var fill = BrushHelper.ToFillStyle(Brush, X, Y, Width, Height);
        return new object[] { CanvasCommandOp.FillEllipse, X, Y, Width, Height, fill };
    }
}

public class DrawStringCommand : DrawingCommand
{
    public string Text { get; }
    public string FontFamily { get; }
    public int FontSize { get; }
    public FontStyle Style { get; }
    public Brush Brush { get; }
    public int X { get; }
    public int Y { get; }

    public DrawStringCommand(string text, string fontFamily, int fontSize, Brush brush, int x, int y)
        : this(text, fontFamily, fontSize, FontStyle.Regular, brush, x, y) { }

    public DrawStringCommand(string text, string fontFamily, int fontSize, FontStyle style, Brush brush, int x, int y)
    {
        Text = text;
        FontFamily = fontFamily;
        FontSize = fontSize;
        Style = style;
        Brush = brush;
        X = x;
        Y = y;
    }

    private string CssFontString()
    {
        var parts = new System.Text.StringBuilder();
        if ((Style & FontStyle.Bold)   != 0) parts.Append("bold ");
        if ((Style & FontStyle.Italic) != 0) parts.Append("italic ");
        parts.Append($"{FontSize}px ");
        parts.Append(FontFamily);
        return parts.ToString();
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ctx.font = '{CssFontString()}';");
        sb.AppendLine("ctx.textBaseline = 'top';");
        var color = Brush is SolidBrush solidBrush ? solidBrush.Color.ToRgbaString() : "rgba(0,0,0,1)";
        sb.AppendLine($"ctx.fillStyle = '{color}';");
        sb.AppendLine($"ctx.fillText('{Text.Replace("'", "\\'")}', {X}, {Y});");

        // Strikeout: horizontal line through the visual midpoint of the text
        if ((Style & FontStyle.Strikeout) != 0)
        {
            int midY = Y + FontSize / 2;
            sb.AppendLine($"var __sw = ctx.measureText('{Text.Replace("'", "\\'")}').width;");
            sb.AppendLine($"ctx.strokeStyle = '{color}';");
            sb.AppendLine($"ctx.lineWidth = Math.max(1, {FontSize} / 12);");
            sb.AppendLine($"ctx.beginPath(); ctx.moveTo({X}, {midY}); ctx.lineTo({X} + __sw, {midY}); ctx.stroke();");
        }

        // Underline: line just below the text baseline
        if ((Style & FontStyle.Underline) != 0)
        {
            int underY = Y + FontSize + 1;
            sb.AppendLine($"var __uw = ctx.measureText('{Text.Replace("'", "\\'")}').width;");
            sb.AppendLine($"ctx.strokeStyle = '{color}';");
            sb.AppendLine($"ctx.lineWidth = Math.max(1, {FontSize} / 14);");
            sb.AppendLine($"ctx.beginPath(); ctx.moveTo({X}, {underY}); ctx.lineTo({X} + __uw, {underY}); ctx.stroke();");
        }

        return sb.ToString();
    }

    public override object[] ToCommand()
    {
        var color = Brush is SolidBrush solidBrush ? solidBrush.Color.ToRgbaString() : "rgba(0,0,0,1)";
        // [op, text, fontFamily, fontSize, x, y, color, fontStyle]
        // fontStyle is a bitmask: 1=Bold 2=Italic 4=Underline 8=Strikeout  (matches FontStyle enum)
        return new object[] { CanvasCommandOp.DrawText, Text, FontFamily, FontSize, X, Y, color, (int)Style };
    }
}

public class ClearCommand : DrawingCommand
{
    public Color BackColor { get; }
    public int Width { get; }
    public int Height { get; }

    public ClearCommand(Color backColor, int width, int height)
    {
        BackColor = backColor;
        Width = width;
        Height = height;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ctx.fillStyle = '{BackColor.ToRgbaString()}';");
        sb.AppendLine($"ctx.fillRect(0, 0, {Width}, {Height});");
        return sb.ToString();
    }

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.Clear, Width, Height, BackColor.ToRgbaString() };
}

public class SaveStateCommand : DrawingCommand
{
    public override string ToJavaScript()
    {
        return "ctx.save();";
    }

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.Save };
}

public class RestoreStateCommand : DrawingCommand
{
    public override string ToJavaScript()
    {
        return "ctx.restore();";
    }

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.Restore };
}

public class SetClipCommand : DrawingCommand
{
    public Rectangle ClipRect { get; }

    public SetClipCommand(Rectangle clipRect)
    {
        ClipRect = clipRect;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine("ctx.beginPath();");
        sb.AppendLine($"ctx.rect({ClipRect.X}, {ClipRect.Y}, {ClipRect.Width}, {ClipRect.Height});");
        sb.AppendLine("ctx.clip();");
        return sb.ToString();
    }

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.ClipRect, ClipRect.X, ClipRect.Y, ClipRect.Width, ClipRect.Height };
}

public class DrawImageCommand : DrawingCommand
{
    public string ImageUrl { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }

    public DrawImageCommand(string imageUrl, int x, int y, int width, int height)
    {
        ImageUrl = imageUrl;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        // Use async image loading with a cache
        sb.AppendLine($"await drawImageAsync(ctx, '{ImageUrl.Replace("'", "\\'")}', {X}, {Y}, {Width}, {Height});");
        return sb.ToString();
    }

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.DrawImage, ImageUrl, X, Y, Width, Height };
}

// ── RoundRect ─────────────────────────────────────────────────────────────────

public class DrawRoundRectCommand : DrawingCommand
{
    public Pen Pen { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public int Radius { get; }

    public DrawRoundRectCommand(Pen pen, int x, int y, int width, int height, int radius)
    {
        Pen = pen; X = x; Y = y; Width = width; Height = height;
        Radius = Math.Clamp(radius, 0, Math.Min(width, height) / 2);
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ctx.strokeStyle = '{Pen.Color.ToRgbaString()}';");
        sb.AppendLine($"ctx.lineWidth = {Pen.Width};");
        sb.AppendLine("ctx.beginPath();");
        sb.AppendLine($"ctx.roundRect({X}, {Y}, {Width}, {Height}, {Radius});");
        sb.AppendLine("ctx.stroke();");
        return sb.ToString();
    }

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.StrokeRoundRect, X, Y, Width, Height, Pen.Width, Pen.Color.ToRgbaString(), Radius, PenHelper.ToDashToken(Pen) };
}

public class FillRoundRectCommand : DrawingCommand
{
    public Brush Brush { get; }
    public int X { get; }
    public int Y { get; }
    public int Width { get; }
    public int Height { get; }
    public int Radius { get; }

    public FillRoundRectCommand(Brush brush, int x, int y, int width, int height, int radius)
    {
        Brush = brush; X = x; Y = y; Width = width; Height = height;
        Radius = Math.Clamp(radius, 0, Math.Min(width, height) / 2);
    }

    public override string ToJavaScript() => string.Empty; // handled by ToCommand

    public override object[] ToCommand()
    {
        return BrushHelper.BuildGradientOrSolid(Brush, X, Y, Width, Height,
            solid => new object[] { CanvasCommandOp.FillRoundRect, X, Y, Width, Height, solid, Radius },
            linear => new object[] { CanvasCommandOp.FillRoundRect, X, Y, Width, Height,
                BrushHelper.LinearGradientToken(linear, X, Y, Width, Height), Radius },
            radial => new object[] { CanvasCommandOp.FillRoundRect, X, Y, Width, Height,
                BrushHelper.RadialGradientToken(radial), Radius });
    }
}

// ── Gradient fills ────────────────────────────────────────────────────────────

/// <summary>
/// Serialises LinearGradientBrush and RadialGradientBrush to gradient-descriptor tokens
/// that the JS renderer understands.  A gradient token is a JSON-like string that starts
/// with "LG:" or "RG:" so the JS side can distinguish it from a plain rgba() colour.
/// Field separator is '|' (not ',') to avoid clashing with rgba() commas.
/// </summary>
internal static class BrushHelper
{
    public static string ToFillStyle(Brush brush, int x, int y, int w, int h)
    {
        if (brush is SolidBrush sb) return sb.Color.ToRgbaString();
        if (brush is LinearGradientBrush lg) return LinearGradientToken(lg, x, y, w, h);
        if (brush is RadialGradientBrush rg) return RadialGradientToken(rg);
        return "rgba(0,0,0,1)";
    }

    // Format: "LG:x1|y1|x2|y2|color1|color2[|offset:color|...]"
    public static string LinearGradientToken(LinearGradientBrush lg, int x, int y, int w, int h)
    {
        var p1 = lg.Point1;
        var p2 = lg.Point2;
        var sb = new StringBuilder();
        sb.Append($"LG:{p1.X}|{p1.Y}|{p2.X}|{p2.Y}|{lg.Color1.ToRgbaString()}|{lg.Color2.ToRgbaString()}");
        if (lg.InterpolationColors != null)
            foreach (var (off, col) in lg.InterpolationColors)
                sb.Append($"|{off.ToString(System.Globalization.CultureInfo.InvariantCulture)}:{col.ToRgbaString()}");
        return sb.ToString();
    }

    // Format: "RG:cx|cy|r|centerColor|surroundColor"
    public static string RadialGradientToken(RadialGradientBrush rg)
        => $"RG:{rg.Center.X}|{rg.Center.Y}|{rg.Radius.ToString(System.Globalization.CultureInfo.InvariantCulture)}|{rg.CenterColor.ToRgbaString()}|{rg.SurroundColor.ToRgbaString()}";

    public static object[] BuildGradientOrSolid(Brush brush, int x, int y, int w, int h,
        Func<string, object[]> onSolid,
        Func<LinearGradientBrush, object[]> onLinear,
        Func<RadialGradientBrush, object[]> onRadial)
    {
        if (brush is LinearGradientBrush lg) return onLinear(lg);
        if (brush is RadialGradientBrush rg) return onRadial(rg);
        var color = brush is SolidBrush sb ? sb.Color.ToRgbaString() : "rgba(0,0,0,1)";
        return onSolid(color);
    }
}

// ── Arc / Bezier / Polygon ────────────────────────────────────────────────────

public class DrawArcCommand : DrawingCommand
{
    public Pen Pen { get; }
    public int X { get; } public int Y { get; }
    public int Width { get; } public int Height { get; }
    public float StartAngle { get; } public float SweepAngle { get; }

    public DrawArcCommand(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
    { Pen = pen; X = x; Y = y; Width = width; Height = height; StartAngle = startAngle; SweepAngle = sweepAngle; }

    public override string ToJavaScript() => string.Empty;

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.DrawArc, X, Y, Width, Height, StartAngle, SweepAngle, Pen.Width, Pen.Color.ToRgbaString(), PenHelper.ToDashToken(Pen) };
}

public class DrawBezierCommand : DrawingCommand
{
    public Pen Pen { get; }
    public int X1 { get; } public int Y1 { get; }
    public int Cx1 { get; } public int Cy1 { get; }
    public int Cx2 { get; } public int Cy2 { get; }
    public int X2 { get; } public int Y2 { get; }

    public DrawBezierCommand(Pen pen, int x1, int y1, int cx1, int cy1, int cx2, int cy2, int x2, int y2)
    { Pen = pen; X1=x1; Y1=y1; Cx1=cx1; Cy1=cy1; Cx2=cx2; Cy2=cy2; X2=x2; Y2=y2; }

    public override string ToJavaScript() => string.Empty;

    public override object[] ToCommand()
        => new object[] { CanvasCommandOp.DrawBezier, X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Pen.Width, Pen.Color.ToRgbaString(), PenHelper.ToDashToken(Pen) };
}

public class DrawPolygonCommand : DrawingCommand
{
    public Pen Pen { get; }
    public Point[] Points { get; }
    public bool Fill { get; }
    public Brush? FillBrush { get; }

    public DrawPolygonCommand(Pen pen, Point[] points) { Pen = pen; Points = points; Fill = false; }
    public DrawPolygonCommand(Brush brush, Point[] points) { Pen = new Pen(Color.Transparent); Points = points; Fill = true; FillBrush = brush; }

    public override string ToJavaScript() => string.Empty;

    public override object[] ToCommand()
    {
        // Flat array: [op, penWidth, penColor, dashToken, fillStyle, p0x, p0y, p1x, p1y, ...]
        var flat = new List<object>
        {
            Fill ? CanvasCommandOp.FillPolygon : CanvasCommandOp.DrawPolygon,
            Pen.Width, Pen.Color.ToRgbaString(),
            PenHelper.ToDashToken(Pen),
            Fill && FillBrush != null ? BrushHelper.ToFillStyle(FillBrush, 0, 0, 0, 0) : "rgba(0,0,0,0)"
        };
        foreach (var p in Points) { flat.Add(p.X); flat.Add(p.Y); }
        return flat.ToArray();
    }
}

// ── GraphicsPath ──────────────────────────────────────────────────────────────

/// <summary>
/// Accumulates path segments and serialises them for the canvas renderer.
/// Mirrors the essential WinForms <c>System.Drawing.Drawing2D.GraphicsPath</c> API.
/// </summary>
public class GraphicsPath : IDisposable
{
    // Path segment opcodes (embedded in the serialised array, different from CanvasCommandOp)
    internal static class Seg
    {
        public const int MoveTo   = 1;
        public const int LineTo   = 2;
        public const int BezierTo = 3;   // cubic: cx1,cy1,cx2,cy2,x,y
        public const int ArcTo    = 4;   // x,y,w,h,startDeg,sweepDeg
        public const int Close    = 5;
        public const int RectTo   = 6;
        public const int EllipseTo= 7;
    }

    private readonly List<object> _segments = new();

    public void StartFigure() { } // no-op: next MoveTo starts new subpath
    public void CloseFigure() => _segments.Add(Seg.Close);

    public void AddLine(int x1, int y1, int x2, int y2)
    {
        _segments.Add(Seg.MoveTo); _segments.Add(x1); _segments.Add(y1);
        _segments.Add(Seg.LineTo); _segments.Add(x2); _segments.Add(y2);
    }
    public void AddLine(Point p1, Point p2) => AddLine(p1.X, p1.Y, p2.X, p2.Y);

    public void AddLines(Point[] points)
    {
        if (points.Length == 0) return;
        _segments.Add(Seg.MoveTo); _segments.Add(points[0].X); _segments.Add(points[0].Y);
        for (int i = 1; i < points.Length; i++)
        { _segments.Add(Seg.LineTo); _segments.Add(points[i].X); _segments.Add(points[i].Y); }
    }

    public void AddBezier(int x1, int y1, int cx1, int cy1, int cx2, int cy2, int x2, int y2)
    {
        _segments.Add(Seg.MoveTo); _segments.Add(x1); _segments.Add(y1);
        _segments.Add(Seg.BezierTo); _segments.Add(cx1); _segments.Add(cy1);
        _segments.Add(cx2); _segments.Add(cy2); _segments.Add(x2); _segments.Add(y2);
    }
    public void AddBezier(Point p1, Point c1, Point c2, Point p2)
        => AddBezier(p1.X, p1.Y, c1.X, c1.Y, c2.X, c2.Y, p2.X, p2.Y);

    public void AddArc(int x, int y, int width, int height, float startAngle, float sweepAngle)
    {
        _segments.Add(Seg.ArcTo);
        _segments.Add(x); _segments.Add(y); _segments.Add(width); _segments.Add(height);
        _segments.Add(startAngle); _segments.Add(sweepAngle);
    }
    public void AddArc(Rectangle rect, float startAngle, float sweepAngle)
        => AddArc(rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

    public void AddRectangle(Rectangle rect)
    {
        _segments.Add(Seg.RectTo);
        _segments.Add(rect.X); _segments.Add(rect.Y); _segments.Add(rect.Width); _segments.Add(rect.Height);
    }

    public void AddEllipse(int x, int y, int width, int height)
    {
        _segments.Add(Seg.EllipseTo);
        _segments.Add(x); _segments.Add(y); _segments.Add(width); _segments.Add(height);
    }
    public void AddEllipse(Rectangle rect) => AddEllipse(rect.X, rect.Y, rect.Width, rect.Height);

    public void AddPolygon(Point[] points)
    {
        AddLines(points);
        CloseFigure();
    }

    internal object[] SerialiseSegments() => _segments.ToArray();

    public void Dispose() { }
}

public class DrawPathCommand : DrawingCommand
{
    public Pen Pen { get; }
    public GraphicsPath Path { get; }

    public DrawPathCommand(Pen pen, GraphicsPath path) { Pen = pen; Path = path; }

    public override string ToJavaScript() => string.Empty;

    public override object[] ToCommand()
    {
        var segments = Path.SerialiseSegments();
        var header = new object[] { CanvasCommandOp.DrawPath, Pen.Width, Pen.Color.ToRgbaString(), PenHelper.ToDashToken(Pen), segments.Length };
        return header.Concat(segments).ToArray();
    }
}

public class FillPathCommand : DrawingCommand
{
    public Brush Brush { get; }
    public GraphicsPath Path { get; }

    public FillPathCommand(Brush brush, GraphicsPath path) { Brush = brush; Path = path; }

    public override string ToJavaScript() => string.Empty;

    public override object[] ToCommand()
    {
        var segments = Path.SerialiseSegments();
        var fillStyle = Brush is SolidBrush sb ? sb.Color.ToRgbaString()
                      : Brush is LinearGradientBrush lg ? BrushHelper.LinearGradientToken(lg, 0, 0, 0, 0)
                      : Brush is RadialGradientBrush rg ? BrushHelper.RadialGradientToken(rg)
                      : "rgba(0,0,0,1)";
        var header = new object[] { CanvasCommandOp.FillPath, fillStyle, segments.Length };
        return header.Concat(segments).ToArray();
    }
}
