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
    public const int StrokeLine = 1;
    public const int StrokeRect = 2;
    public const int FillRect = 3;
    public const int StrokeEllipse = 4;
    public const int FillEllipse = 5;
    public const int DrawText = 6;
    public const int Clear = 7;
    public const int Save = 8;
    public const int Restore = 9;
    public const int ClipRect = 10;
    public const int DrawImage = 11;
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
        => new object[] { CanvasCommandOp.StrokeLine, X1, Y1, X2, Y2, Pen.Width, Pen.Color.ToRgbaString() };
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
        => new object[] { CanvasCommandOp.StrokeRect, X, Y, Width, Height, Pen.Width, Pen.Color.ToRgbaString() };
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
        var color = Brush is SolidBrush solidBrush ? solidBrush.Color.ToRgbaString() : "rgba(0,0,0,1)";
        return new object[] { CanvasCommandOp.FillRect, X, Y, Width, Height, color };
    }
}

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
        => new object[] { CanvasCommandOp.StrokeEllipse, X, Y, Width, Height, Pen.Width, Pen.Color.ToRgbaString() };
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
        var color = Brush is SolidBrush solidBrush ? solidBrush.Color.ToRgbaString() : "rgba(0,0,0,1)";
        return new object[] { CanvasCommandOp.FillEllipse, X, Y, Width, Height, color };
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
