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

    /// <summary>Protected default constructor for subclasses (e.g. System.Drawing.Graphics shim).</summary>
    protected Graphics() { }

    /// <summary>
    /// Creates a <see cref="Graphics"/> for drawing onto an <see cref="Image"/> (stub).
    /// Returns a default-sized Graphics instance; actual image-backed rendering is
    /// not supported in the browser canvas host.
    /// </summary>
    public static Graphics FromImage(System.Drawing.Image image)
        => new Graphics(image?.Width ?? 1, image?.Height ?? 1);

    /// <summary>Static stub — returns a no-op Graphics; HWND-based access is not meaningful in the canvas host.</summary>
    public static Graphics FromHwnd(IntPtr hwnd) => new Graphics();

    /// <summary>Static stub — returns a no-op Graphics.</summary>
    public static Graphics FromHdc(IntPtr hdc) => new Graphics();

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

    public void DrawRectangle(Pen pen, float x, float y, float width, float height)
        => DrawRectangle(pen, (int)x, (int)y, (int)width, (int)height);

    public void DrawRectangles(Pen pen, Rectangle[] rects)
    { foreach (var r in rects) DrawRectangle(pen, r); }

    public void DrawRectangles(Pen pen, RectangleF[] rects)
    { foreach (var r in rects) DrawRectangle(pen, (int)r.X, (int)r.Y, (int)r.Width, (int)r.Height); }

    public void FillRectangle(Brush brush, int x, int y, int width, int height)
    {
        _commands.Add(new FillRectangleCommand(brush, x + _translateX, y + _translateY, width, height));
    }

    public void FillRectangle(Brush brush, float x, float y, float width, float height)
        => FillRectangle(brush, (int)x, (int)y, (int)width, (int)height);

    public void FillRectangle(Brush brush, RectangleF rect)
        => FillRectangle(brush, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

    public void FillRectangle(Brush brush, Rectangle rect)
    {
        FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void FillRectangles(Brush brush, Rectangle[] rects)
    { foreach (var r in rects) FillRectangle(brush, r); }

    public void FillRectangles(Brush brush, RectangleF[] rects)
    { foreach (var r in rects) FillRectangle(brush, (int)r.X, (int)r.Y, (int)r.Width, (int)r.Height); }

    public void DrawEllipse(Pen pen, int x, int y, int width, int height)
    {
        _commands.Add(new DrawEllipseCommand(pen, x + _translateX, y + _translateY, width, height));
    }

    public void DrawEllipse(Pen pen, float x, float y, float width, float height)
        => DrawEllipse(pen, (int)x, (int)y, (int)width, (int)height);

    public void DrawEllipse(Pen pen, Rectangle rect)
    {
        DrawEllipse(pen, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void DrawEllipse(Pen pen, RectangleF rect)
        => DrawEllipse(pen, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

    public void FillEllipse(Brush brush, int x, int y, int width, int height)
    {
        _commands.Add(new FillEllipseCommand(brush, x + _translateX, y + _translateY, width, height));
    }

    public void FillEllipse(Brush brush, float x, float y, float width, float height)
        => FillEllipse(brush, (int)x, (int)y, (int)width, (int)height);

    public void FillEllipse(Brush brush, Rectangle rect)
    {
        FillEllipse(brush, rect.X, rect.Y, rect.Width, rect.Height);
    }

    public void FillEllipse(Brush brush, RectangleF rect)
        => FillEllipse(brush, (int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);

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

    // ── Images ────────────────────────────────────────────────────────────────

    /// <summary>Draws an image at (x, y) using its natural size (or 1×1 if unknown).</summary>
    public void DrawImage(Image image, int x, int y)
    {
        if (image?.Source == null) return;
        int w = image.Width > 0 ? image.Width : 1;
        int h = image.Height > 0 ? image.Height : 1;
        _commands.Add(new DrawImageCommand(image.Source, x + _translateX, y + _translateY, w, h));
    }

    /// <summary>Draws an image scaled into the destination rectangle.</summary>
    public void DrawImage(Image image, int x, int y, int width, int height)
    {
        if (image?.Source == null) return;
        _commands.Add(new DrawImageCommand(image.Source, x + _translateX, y + _translateY, width, height));
    }

    /// <summary>Draws a portion of an image (srcRect) scaled into the destination rectangle.</summary>
    public void DrawImage(Image image, Rectangle dstRect, Rectangle srcRect)
    {
        if (image?.Source == null) return;
        _commands.Add(new DrawImageCommand(
            image.Source,
            dstRect.X + _translateX, dstRect.Y + _translateY, dstRect.Width, dstRect.Height,
            srcRect));
    }

    /// <summary>Draws an image into the destination rectangle.</summary>
    public void DrawImage(Image image, Rectangle dstRect)
        => DrawImage(image, dstRect.X, dstRect.Y, dstRect.Width, dstRect.Height);

    /// <summary>Draws an image at the specified floating-point location.</summary>
    public void DrawImage(Image image, float x, float y)
        => DrawImage(image, (int)x, (int)y);

    /// <summary>Draws an image at the specified floating-point point.</summary>
    public void DrawImage(Image image, PointF point)
        => DrawImage(image, (int)point.X, (int)point.Y);

    /// <summary>Draws an image into the specified floating-point destination rectangle.</summary>
    public void DrawImage(Image image, RectangleF dstRect)
        => DrawImage(image, (int)dstRect.X, (int)dstRect.Y, (int)dstRect.Width, (int)dstRect.Height);

    /// <summary>Draws an image at the specified floating-point location with the given dimensions.</summary>
    public void DrawImage(Image image, float x, float y, float width, float height)
        => DrawImage(image, (int)x, (int)y, (int)width, (int)height);

    /// <summary>Draws a portion of an image at the specified floating-point location.</summary>
    public void DrawImage(Image image, RectangleF dstRect, RectangleF srcRect, System.Drawing.GraphicsUnit srcUnit)
        => DrawImage(image, (int)dstRect.X, (int)dstRect.Y, (int)dstRect.Width, (int)dstRect.Height);

    /// <summary>Draws a portion of an image (srcUnit compat overload).</summary>
    public void DrawImage(Image image, Rectangle dstRect, Rectangle srcRect, System.Drawing.GraphicsUnit srcUnit)
        => DrawImage(image, dstRect, srcRect);

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

    // ── WinForms API compatibility properties ─────────────────────────────────

    /// <summary>
    /// Gets or sets the rendering quality of this Graphics.
    /// Canvas rendering ignores smoothing mode — provided for API compatibility.
    /// </summary>
    public System.Drawing.Drawing2D.SmoothingMode SmoothingMode { get; set; }
        = System.Drawing.Drawing2D.SmoothingMode.Default;

    /// <summary>
    /// Gets or sets the rendering hint for text.
    /// Canvas text is always rendered by the browser — provided for API compatibility.
    /// </summary>
    public global::System.Drawing.Text.TextRenderingHint TextRenderingHint { get; set; }
        = global::System.Drawing.Text.TextRenderingHint.SystemDefault;

    /// <summary>
    /// Gets or sets the compositing quality.
    /// Provided for API compatibility — ignored in canvas rendering.
    /// </summary>
    public System.Drawing.Drawing2D.CompositingQuality CompositingQuality { get; set; }
        = System.Drawing.Drawing2D.CompositingQuality.Default;

    /// <summary>
    /// Gets or sets the interpolation mode.
    /// Provided for API compatibility — ignored in canvas rendering.
    /// </summary>
    public System.Drawing.Drawing2D.InterpolationMode InterpolationMode { get; set; }
        = System.Drawing.Drawing2D.InterpolationMode.Default;

    /// <summary>
    /// Gets or sets the pixel offset mode.
    /// Provided for API compatibility — ignored in canvas rendering.
    /// </summary>
    public System.Drawing.Drawing2D.PixelOffsetMode PixelOffsetMode { get; set; }
        = System.Drawing.Drawing2D.PixelOffsetMode.Default;

    /// <summary>Gets the current clipping region as a Rectangle (canvas approximation).</summary>
    public Rectangle Clip
    {
        get => _clipRect ?? new Rectangle(0, 0, _width, _height);
        set => _clipRect = value;
    }

    /// <summary>Gets the visible clipping bounds as a RectangleF.</summary>
    public System.Drawing.RectangleF ClipBounds
        => _clipRect.HasValue
            ? new System.Drawing.RectangleF(_clipRect.Value.X, _clipRect.Value.Y, _clipRect.Value.Width, _clipRect.Value.Height)
            : new System.Drawing.RectangleF(0, 0, _width, _height);

    // ── Transform helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Applies an additional rotation transform.
    /// Canvas-side stub: rotations are not yet propagated to drawing commands.
    /// </summary>
    public void RotateTransform(float angle) { /* stub — browser canvas does not support server-side rotation accumulation */ }

    /// <summary>
    /// Applies a scale transform.
    /// Canvas-side stub: scale is not yet propagated to drawing commands.
    /// </summary>
    public void ScaleTransform(float sx, float sy) { /* stub */ }

    /// <summary>Resets the current transform to the identity matrix.</summary>
    public void ResetTransform()
    {
        _translateX = 0;
        _translateY = 0;
    }

    // ── Additional drawing helpers ────────────────────────────────────────────

    /// <summary>Draws a string at the specified float coordinates (rounds to int).</summary>
    public void DrawString(string text, Font font, Brush brush, float x, float y)
        => DrawString(text, font, brush, (int)x, (int)y);

    /// <summary>Draws a string at the specified PointF position.</summary>
    public void DrawString(string text, Font font, Brush brush, System.Drawing.PointF point)
        => DrawString(text, font, brush, (int)point.X, (int)point.Y);

    /// <summary>Draws a string at the specified PointF with a format specifier.</summary>
    public void DrawString(string text, Font font, Brush brush, System.Drawing.PointF point, System.Drawing.StringFormat? format)
        => DrawString(text, font, brush, (int)point.X, (int)point.Y);

    /// <summary>Draws a string inside the specified Rectangle (top-left positioned).</summary>
    public void DrawString(string text, Font font, Brush brush, Rectangle layoutRectangle)
        => DrawString(text, font, brush, layoutRectangle.X, layoutRectangle.Y);

    /// <summary>Draws a string inside the specified Rectangle with a format specifier.</summary>
    public void DrawString(string text, Font font, Brush brush, Rectangle layoutRectangle, System.Drawing.StringFormat? format)
        => DrawString(text, font, brush, layoutRectangle.X, layoutRectangle.Y);

    /// <summary>Draws a string inside the specified layout rectangle (top-left positioned).</summary>
    public void DrawString(string text, Font font, Brush brush, System.Drawing.RectangleF layoutRectangle)
        => DrawString(text, font, brush, (int)layoutRectangle.X, (int)layoutRectangle.Y);

    /// <summary>Draws a string with a format specifier (format ignored in canvas rendering).</summary>
    public void DrawString(string text, Font font, Brush brush, float x, float y, System.Drawing.StringFormat? format)
        => DrawString(text, font, brush, (int)x, (int)y);

    /// <summary>Draws a string inside the specified layout rectangle with a format specifier.</summary>
    public void DrawString(string text, Font font, Brush brush, System.Drawing.RectangleF layoutRectangle, System.Drawing.StringFormat? format)
        => DrawString(text, font, brush, (int)layoutRectangle.X, (int)layoutRectangle.Y);

    /// <summary>
    /// Draws an image without scaling it (1:1 pixel mapping).
    /// </summary>
    public void DrawImageUnscaled(Image image, int x, int y)
        => DrawImage(image, x, y);

    public void DrawImageUnscaled(Image image, Point pt)
        => DrawImage(image, pt.X, pt.Y);

    public void DrawImageUnscaled(Image image, Rectangle rect)
        => DrawImage(image, rect.X, rect.Y);

    /// <summary>Draws an icon at the specified location (uses icon's internal image).</summary>
    public void DrawIcon(System.Drawing.Icon icon, int x, int y)
    {
        if (icon?.Image is Image img)
            DrawImage(img, x, y, icon.Width, icon.Height);
    }

    public void DrawIcon(System.Drawing.Icon icon, Rectangle targetRect)
        => DrawIcon(icon, targetRect.X, targetRect.Y);

    public void DrawIconUnstretched(System.Drawing.Icon icon, Rectangle targetRect)
        => DrawIcon(icon, targetRect.X, targetRect.Y);

    public void DrawImageUnscaledAndClipped(Image image, Rectangle rect)
        => DrawImageUnscaled(image, rect);

    /// <summary>Stub — CopyFromScreen is not available in a browser context.</summary>
    public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize) { }
    public void CopyFromScreen(Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize) { }
    public void CopyFromScreen(int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize, System.Drawing.CopyPixelOperation copyPixelOperation) { }

    /// <summary>
    /// Fills the interior of a Region.  Canvas approximation: fills the bounding rectangle.
    /// </summary>
    public void FillRegion(Brush brush, System.Drawing.Region region)
    {
        if (region is null) return;
        var bounds = region.GetBounds(null);
        FillRectangle(brush, (int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
    }

    /// <summary>
    /// Intersects the current clip region with the specified rectangle.
    /// </summary>
    public void IntersectClip(Rectangle rect)
    {
        if (_clipRect.HasValue)
        {
            int x = Math.Max(_clipRect.Value.X, rect.X);
            int y = Math.Max(_clipRect.Value.Y, rect.Y);
            int r = Math.Min(_clipRect.Value.Right, rect.Right);
            int b = Math.Min(_clipRect.Value.Bottom, rect.Bottom);
            _clipRect = new Rectangle(x, y, Math.Max(0, r - x), Math.Max(0, b - y));
        }
        else
        {
            _clipRect = rect;
        }
    }

    /// <summary>Returns whether the specified point is within the visible clip region.</summary>
    public bool IsVisible(int x, int y)
    {
        var clip = _clipRect ?? new Rectangle(0, 0, _width, _height);
        return clip.Contains(x, y);
    }

    public bool IsVisible(Point pt) => IsVisible(pt.X, pt.Y);

    public bool IsVisible(Rectangle rect)
    {
        var clip = _clipRect ?? new Rectangle(0, 0, _width, _height);
        return rect.X < clip.Right && rect.Right > clip.X &&
               rect.Y < clip.Bottom && rect.Bottom > clip.Y;
    }

    // ── MeasureString ─────────────────────────────────────────────────────────

    /// <summary>
    /// Measures the bounding box of a string.
    /// Returns a heuristic approximation based on font size since server-side
    /// text measurement is not possible without a browser layout engine.
    /// </summary>
    public System.Drawing.SizeF MeasureString(string text, Font font)
    {
        if (string.IsNullOrEmpty(text)) return System.Drawing.SizeF.Empty;
        float charW = font.Size * 0.6f;
        float lineH = font.Size * 1.4f;
        return new System.Drawing.SizeF(text.Length * charW, lineH);
    }

    public System.Drawing.SizeF MeasureString(string text, Font font, int width)
        => MeasureString(text, font);

    public System.Drawing.SizeF MeasureString(string text, Font font, System.Drawing.SizeF layoutArea)
        => MeasureString(text, font);

    public System.Drawing.SizeF MeasureString(string text, Font font, int width, System.Drawing.StringFormat? format)
        => MeasureString(text, font);

    public System.Drawing.SizeF MeasureString(string text, Font font, System.Drawing.PointF origin, System.Drawing.StringFormat? format)
        => MeasureString(text, font);

    // ── Page/DPI properties ───────────────────────────────────────────────────

    /// <summary>Gets or sets the unit of measure used for page coordinates (API compat stub).</summary>
    public System.Drawing.GraphicsUnit PageUnit { get; set; } = System.Drawing.GraphicsUnit.Pixel;

    /// <summary>Gets or sets the scaling between world and page coordinates (API compat stub).</summary>
    public float PageScale { get; set; } = 1.0f;

    /// <summary>Gets or sets how composited images are drawn (browser always composites SourceOver).</summary>
    public System.Drawing.Drawing2D.CompositingMode CompositingMode { get; set; }
        = System.Drawing.Drawing2D.CompositingMode.SourceOver;

    /// <summary>Horizontal DPI of the display (fixed at 96 for browser canvas).</summary>
    public float DpiX => 96.0f;

    /// <summary>Vertical DPI of the display (fixed at 96 for browser canvas).</summary>
    public float DpiY => 96.0f;

    /// <summary>Gets the visible clip bounds as a RectangleF.</summary>
    public System.Drawing.RectangleF VisibleClipBounds
    {
        get
        {
            var r = _clipRect ?? new Rectangle(0, 0, _width, _height);
            return new System.Drawing.RectangleF(r.X, r.Y, r.Width, r.Height);
        }
    }

    // ── Clip helpers ──────────────────────────────────────────────────────────

    /// <summary>Sets the clip region to the intersection of the current clip and a Region (stub — uses region bounds).</summary>
    public void SetClip(System.Drawing.Region region)
    {
        var b = region.GetBounds(null);
        SetClip(new Rectangle((int)b.X, (int)b.Y, (int)b.Width, (int)b.Height));
    }

    public void SetClip(System.Drawing.RectangleF rect)
        => SetClip(new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height));

    public void ResetClip() { _clipRect = null; }

    // ── Pie / arc drawing ─────────────────────────────────────────────────────

    public void DrawPie(Pen pen, int x, int y, int width, int height, float startAngle, float sweepAngle)
        => _commands.Add(new DrawPieCommand(pen, x + _translateX, y + _translateY, width, height, startAngle, sweepAngle));

    public void DrawPie(Pen pen, Rectangle rect, float startAngle, float sweepAngle)
        => DrawPie(pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

    public void FillPie(Brush brush, int x, int y, int width, int height, float startAngle, float sweepAngle)
    {
        string color = brush is SolidBrush sb ? sb.Color.ToRgbaString() : "rgba(0,0,0,1)";
        _commands.Add(new FillPieCommand(color, x + _translateX, y + _translateY, width, height, startAngle, sweepAngle));
    }

    public void FillPie(Brush brush, Rectangle rect, float startAngle, float sweepAngle)
        => FillPie(brush, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);

    // ── Curve drawing ─────────────────────────────────────────────────────────

    public void DrawCurve(Pen pen, Point[] points) { for (int i = 0; i < points.Length - 1; i++) DrawLine(pen, points[i], points[i + 1]); }
    public void DrawCurve(Pen pen, Point[] points, float tension) => DrawCurve(pen, points);
    public void DrawCurve(Pen pen, Point[] points, int offset, int numberOfSegments) => DrawCurve(pen, points);
    public void DrawCurve(Pen pen, PointF[] points)
    {
        for (int i = 0; i < points.Length - 1; i++)
            DrawLine(pen, (int)points[i].X, (int)points[i].Y, (int)points[i + 1].X, (int)points[i + 1].Y);
    }

    public void DrawClosedCurve(Pen pen, Point[] points) => DrawPolygon(pen, points);
    public void DrawClosedCurve(Pen pen, Point[] points, float tension, System.Drawing.Drawing2D.FillMode fillMode)
        => DrawPolygon(pen, points);

    public void FillClosedCurve(Brush brush, Point[] points) => FillPolygon(brush, points);
    public void FillClosedCurve(Brush brush, Point[] points, System.Drawing.Drawing2D.FillMode fillMode)
        => FillPolygon(brush, points);

    // ── GDI handle stubs ──────────────────────────────────────────────────────

    /// <summary>Returns a GDI device-context handle stub (always zero — no GDI in browser).</summary>
    public IntPtr GetHdc() => IntPtr.Zero;

    /// <summary>Releases the GDI device context obtained via <see cref="GetHdc"/> (no-op).</summary>
    public void ReleaseHdc() { }
    public void ReleaseHdc(IntPtr dc) { }
    public void ReleaseHdcInternal(IntPtr dc) { }

    // ── Character range measurement ───────────────────────────────────────────

    /// <summary>
    /// Measures the specified character ranges (stub — returns heuristic regions).
    /// </summary>
    public System.Drawing.Region[] MeasureCharacterRanges(string text, Font font, System.Drawing.RectangleF layoutRect, System.Drawing.StringFormat? format)
    {
        float charW = font.Size * 0.6f;
        // Return one region per range
        var sf = format;
        var ranges = sf?.GetMeasurableCharacterRanges() ?? Array.Empty<System.Drawing.CharacterRange>();
        if (ranges.Length == 0)
            return new[] { new System.Drawing.Region(layoutRect) };
        return ranges.Select(r =>
        {
            float x = layoutRect.X + r.First * charW;
            float w = r.Length * charW;
            return new System.Drawing.Region(new System.Drawing.RectangleF(x, layoutRect.Y, w, layoutRect.Height));
        }).ToArray();
    }

    // ── Transform stubs (extended) ────────────────────────────────────────────

    public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix) { /* stub */ }
    public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.Drawing2D.MatrixOrder order) { /* stub */ }
    public System.Drawing.Drawing2D.Matrix Transform
    {
        get => new System.Drawing.Drawing2D.Matrix();
        set { /* stub */ }
    }
    public System.Drawing.Drawing2D.GraphicsContainer BeginContainer() => new();
    public void EndContainer(System.Drawing.Drawing2D.GraphicsContainer container) { }

    // ── Flush ─────────────────────────────────────────────────────────────────

    public void Flush() { }
    public void Flush(System.Drawing.Drawing2D.FlushIntention intention) { }
}

// Graphics state for save/restore
internal record GraphicsState(int TranslateX, int TranslateY, Rectangle? ClipRect);
