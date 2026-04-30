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

    public IEnumerable<DrawingCommand> GetCommands() => _commands;

    public void Dispose()
    {
        _commands.Clear();
    }
}

// Graphics state for save/restore
internal record GraphicsState(int TranslateX, int TranslateY, Rectangle? ClipRect);
