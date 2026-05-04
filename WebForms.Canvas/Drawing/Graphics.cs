namespace Canvas.Windows.Forms.Drawing;

public class Graphics : IDisposable
{
    private readonly List<DrawingCommand> _commands = new();
    private readonly int _width;
    private readonly int _height;
    private int _translateX = 0;
    private int _translateY = 0;
    private readonly Stack<GraphicsState> _stateStack = new();
    private Rectangle? _clipRect = null;

    public Graphics(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public void Save()
    {
        _stateStack.Push(new GraphicsState(_translateX, _translateY, _clipRect));
        _commands.Add(new SaveStateCommand());
    }

    public void Restore()
    {
        if (_stateStack.Count > 0)
        {
            var state = _stateStack.Pop();
            _translateX = state.TranslateX;
            _translateY = state.TranslateY;
            _clipRect = state.ClipRect;
            _commands.Add(new RestoreStateCommand());
        }
    }

    public void SetClip(Rectangle rect)
    {
        // Clip needs to respect the current translation transform so callers can
        // specify clip bounds in the same coordinate space as other drawing APIs.
        var translatedRect = new Rectangle(
            rect.X + _translateX,
            rect.Y + _translateY,
            rect.Width,
            rect.Height);

        _clipRect = translatedRect;
        _commands.Add(new SetClipCommand(translatedRect));
    }

    public void TranslateTransform(int dx, int dy)
    {
        _translateX += dx;
        _translateY += dy;
    }

    public void Clear(Color color)
    {
        _commands.Clear();
        _commands.Add(new ClearCommand(color, _width, _height));
    }

    public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
    {
        _commands.Add(new DrawLineCommand(pen, x1 + _translateX, y1 + _translateY, x2 + _translateX, y2 + _translateY));
    }

    public void DrawLine(Pen pen, Point pt1, Point pt2)
    {
        DrawLine(pen, pt1.X, pt1.Y, pt2.X, pt2.Y);
    }

    public void DrawRectangle(Pen pen, int x, int y, int width, int height)
    {
        _commands.Add(new DrawRectangleCommand(pen, x + _translateX, y + _translateY, width, height));
    }

    public void DrawRectangle(Pen pen, Rectangle rect)
    {
        DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void FillRectangle(Brush brush, int x, int y, int width, int height)
    {
        _commands.Add(new FillRectangleCommand(brush, x + _translateX, y + _translateY, width, height));
    }

    public void FillRectangle(Brush brush, Rectangle rect)
    {
        FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void DrawEllipse(Pen pen, int x, int y, int width, int height)
    {
        _commands.Add(new DrawEllipseCommand(pen, x + _translateX, y + _translateY, width, height));
    }

    public void DrawEllipse(Pen pen, Rectangle rect)
    {
        DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void FillEllipse(Brush brush, int x, int y, int width, int height)
    {
        _commands.Add(new FillEllipseCommand(brush, x + _translateX, y + _translateY, width, height));
    }

    public void FillEllipse(Brush brush, Rectangle rect)
    {
        FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void DrawString(string text, string fontFamily, int fontSize, Brush brush, int x, int y)
    {
        _commands.Add(new DrawStringCommand(text, fontFamily, fontSize, brush, x + _translateX, y + _translateY));
    }

    public void DrawString(string text, string fontFamily, int fontSize, Brush brush, Point point)
    {
        DrawString(text, fontFamily, fontSize, brush, point.X, point.Y);
    }

    // Convenience overload for Color
    public void DrawString(string text, int x, int y, Color color)
    {
        DrawString(text, "Arial", 12, new SolidBrush(color), x, y);
    }

    // Overload with Font
    public void DrawString(string text, Font font, Brush brush, int x, int y)
    {
        _commands.Add(new DrawStringCommand(text, font.Family, (int)font.Size, font.Style, brush, x + _translateX, y + _translateY));
    }

    // Overload with Font and Color
    public void DrawString(string text, Font font, Color color, int x, int y)
    {
        _commands.Add(new DrawStringCommand(text, font.Family, (int)font.Size, font.Style, new SolidBrush(color), x + _translateX, y + _translateY));
    }

    // Draw image
    public void DrawImage(string imageUrl, int x, int y, int width, int height)
    {
        _commands.Add(new DrawImageCommand(imageUrl, x + _translateX, y + _translateY, width, height));
    }

    public void DrawImage(string imageUrl, Rectangle rect)
    {
        DrawImage(imageUrl, rect.X, rect.Y, rect.Width, rect.Height);
    }

    // ── RoundRect ─────────────────────────────────────────────────────────────

    public void DrawRoundRect(Pen pen, int x, int y, int width, int height, int radius)
        => _commands.Add(new DrawRoundRectCommand(pen, x + _translateX, y + _translateY, width, height, radius));

    public void DrawRoundRect(Pen pen, Rectangle rect, int radius)
        => DrawRoundRect(pen, rect.X, rect.Y, rect.Width, rect.Height, radius);

    public void FillRoundRect(Brush brush, int x, int y, int width, int height, int radius)
        => _commands.Add(new FillRoundRectCommand(brush, x + _translateX, y + _translateY, width, height, radius));

    public void FillRoundRect(Brush brush, Rectangle rect, int radius)
        => FillRoundRect(brush, rect.X, rect.Y, rect.Width, rect.Height, radius);

    // ── Arc ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Draws an arc (portion of an ellipse outline).
    /// <paramref name="startAngle"/> and <paramref name="sweepAngle"/> are in degrees, clockwise.
    /// </summary>
    public void DrawArc(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
        => _commands.Add(new DrawArcCommand(pen, x + _translateX, y + _translateY, width, height, startAngle, sweepAngle));

    public void DrawArc(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        => DrawArc(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

    // ── Bezier ────────────────────────────────────────────────────────────────

    public void DrawBezier(Pen pen, int x1, int y1, int cx1, int cy1, int cx2, int cy2, int x2, int y2)
        => _commands.Add(new DrawBezierCommand(pen,
            x1 + _translateX, y1 + _translateY,
            cx1 + _translateX, cy1 + _translateY,
            cx2 + _translateX, cy2 + _translateY,
            x2 + _translateX, y2 + _translateY));

    public void DrawBezier(Pen pen, Point p1, Point c1, Point c2, Point p2)
        => DrawBezier(pen, p1.X, p1.Y, c1.X, c1.Y, c2.X, c2.Y, p2.X, p2.Y);

    // ── Polygon ───────────────────────────────────────────────────────────────

    public void DrawPolygon(Pen pen, Point[] points)
    {
        var translated = Translate(points);
        _commands.Add(new DrawPolygonCommand(pen, translated));
    }

    public void FillPolygon(Brush brush, Point[] points)
    {
        var translated = Translate(points);
        _commands.Add(new DrawPolygonCommand(brush, translated));
    }

    // ── GraphicsPath ──────────────────────────────────────────────────────────

    public void DrawPath(Pen pen, GraphicsPath path)
        => _commands.Add(new DrawPathCommand(pen, path));

    public void FillPath(Brush brush, GraphicsPath path)
        => _commands.Add(new FillPathCommand(brush, path));

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Point[] Translate(Point[] points)
    {
        if (_translateX == 0 && _translateY == 0) return points;
        var result = new Point[points.Length];
        for (int i = 0; i < points.Length; i++)
            result[i] = new Point(points[i].X + _translateX, points[i].Y + _translateY);
        return result;
    }

    public IEnumerable<DrawingCommand> GetCommands() => _commands;

    public void Dispose()
    {
        _commands.Clear();
    }
}

// Graphics state for save/restore
internal record GraphicsState(int TranslateX, int TranslateY, Rectangle? ClipRect);
