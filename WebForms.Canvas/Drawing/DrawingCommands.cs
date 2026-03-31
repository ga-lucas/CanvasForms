using System.Text;

namespace WebForms.Canvas.Drawing;

// Represents drawing commands that can be serialized to JavaScript
public abstract class DrawingCommand
{
    public abstract string ToJavaScript();
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
}

public class DrawStringCommand : DrawingCommand
{
    public string Text { get; }
    public string FontFamily { get; }
    public int FontSize { get; }
    public Brush Brush { get; }
    public int X { get; }
    public int Y { get; }

    public DrawStringCommand(string text, string fontFamily, int fontSize, Brush brush, int x, int y)
    {
        Text = text;
        FontFamily = fontFamily;
        FontSize = fontSize;
        Brush = brush;
        X = x;
        Y = y;
    }

    public override string ToJavaScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ctx.font = '{FontSize}px {FontFamily}';");
        sb.AppendLine("ctx.textBaseline = 'top';"); // Use top baseline for consistent positioning
        if (Brush is SolidBrush solidBrush)
        {
            sb.AppendLine($"ctx.fillStyle = '{solidBrush.Color.ToRgbaString()}';");
        }
        sb.AppendLine($"ctx.fillText('{Text.Replace("'", "\\'")}', {X}, {Y});");
        return sb.ToString();
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
}

public class SaveStateCommand : DrawingCommand
{
    public override string ToJavaScript()
    {
        return "ctx.save();";
    }
}

public class RestoreStateCommand : DrawingCommand
{
    public override string ToJavaScript()
    {
        return "ctx.restore();";
    }
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
}
