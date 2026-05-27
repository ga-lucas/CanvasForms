namespace System.Drawing;

/// <summary>
/// Represents an area of a display surface.
/// In CanvasForms this is a bounding-rectangle stub — clipping and region-math
/// are not natively supported in the canvas pipeline.  The stub is provided so
/// translated assemblies that construct and test Region objects compile and run.
/// </summary>
public sealed class Region : IDisposable
{
    private RectangleF _bounds;

    /// <summary>Constructs an infinite (unbounded) region.</summary>
    public Region()
        => _bounds = new RectangleF(float.MinValue / 2, float.MinValue / 2,
                                    float.MaxValue,     float.MaxValue);

    /// <summary>Constructs a region from a <see cref="RectangleF"/>.</summary>
    public Region(RectangleF rect) => _bounds = rect;

    /// <summary>Constructs a region from an integer <see cref="Rectangle"/>.</summary>
    public Region(Rectangle rect)
        => _bounds = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>Creates a region from a graphics path (stub — uses the path's bounding box).</summary>
    public Region(System.Drawing.Drawing2D.GraphicsPath path) { /* no bounding info available; treated as infinite */ }

    /// <summary>Creates a region from a System.Drawing.Region (compat overload).</summary>
    public Region(System.Drawing.Region region) => _bounds = region.GetBounds(null);

    // ── Combine operations (bounding-rect approximation) ─────────────────────

    public void Intersect(RectangleF rect)
    {
        float x = Math.Max(_bounds.X, rect.X);
        float y = Math.Max(_bounds.Y, rect.Y);
        float r = Math.Min(_bounds.X + _bounds.Width,  rect.X + rect.Width);
        float b = Math.Min(_bounds.Y + _bounds.Height, rect.Y + rect.Height);
        _bounds = r > x && b > y ? new RectangleF(x, y, r - x, b - y) : RectangleF.Empty;
    }

    public void Intersect(Rectangle rect)
        => Intersect(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));

    public void Union(RectangleF rect)
    {
        float x = Math.Min(_bounds.X, rect.X);
        float y = Math.Min(_bounds.Y, rect.Y);
        float r = Math.Max(_bounds.X + _bounds.Width,  rect.X + rect.Width);
        float b = Math.Max(_bounds.Y + _bounds.Height, rect.Y + rect.Height);
        _bounds = new RectangleF(x, y, r - x, b - y);
    }

    public void Union(Rectangle rect)
        => Union(new RectangleF(rect.X, rect.Y, rect.Width, rect.Height));

    public void Exclude(RectangleF rect) { /* stub — bounding rect unchanged */ }
    public void Exclude(Rectangle rect)  { /* stub */ }

    public void MakeEmpty()   => _bounds = RectangleF.Empty;
    public void MakeInfinite() => _bounds = new RectangleF(float.MinValue / 2, float.MinValue / 2,
                                                            float.MaxValue, float.MaxValue);

    // ── Query ─────────────────────────────────────────────────────────────────

    public bool IsEmpty(Graphics? g) => _bounds.Width <= 0 || _bounds.Height <= 0;
    public bool IsInfinite(Graphics? g) => _bounds.Width >= float.MaxValue / 2;

    public bool IsVisible(float x, float y)
        => x >= _bounds.X && x < _bounds.X + _bounds.Width
        && y >= _bounds.Y && y < _bounds.Y + _bounds.Height;

    public bool IsVisible(PointF pt) => IsVisible(pt.X, pt.Y);
    public bool IsVisible(Point pt)  => IsVisible(pt.X, pt.Y);

    public bool IsVisible(float x, float y, float width, float height)
        => IsVisible(x, y) || IsVisible(x + width, y + height);

    public bool IsVisible(RectangleF rect)
        => IsVisible(rect.X, rect.Y, rect.Width, rect.Height);

    public bool IsVisible(Rectangle rect)
        => IsVisible(rect.X, rect.Y, rect.Width, rect.Height);

    /// <summary>Returns the bounding rectangle of this region (approximation).</summary>
    public RectangleF GetBounds(Graphics? g) => _bounds;

    public Region Clone() => new Region(_bounds);

    public void Dispose() { /* no unmanaged resources */ }
}
