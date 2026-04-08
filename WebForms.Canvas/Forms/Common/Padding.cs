namespace System.Windows.Forms;

/// <summary>
/// Represents padding or margin information associated with a control.
/// </summary>
public struct Padding : IEquatable<Padding>
{
    private int _left;
    private int _top;
    private int _right;
    private int _bottom;

    /// <summary>
    /// Provides a Padding object with no padding.
    /// </summary>
    public static readonly Padding Empty = new Padding(0);

    /// <summary>
    /// Initializes a new instance of the Padding structure using the same padding size for all edges.
    /// </summary>
    public Padding(int all)
    {
        _left = _top = _right = _bottom = all;
    }

    /// <summary>
    /// Initializes a new instance of the Padding structure using separate padding sizes for each edge.
    /// </summary>
    public Padding(int left, int top, int right, int bottom)
    {
        _left = left;
        _top = top;
        _right = right;
        _bottom = bottom;
    }

    /// <summary>
    /// Gets or sets the padding value for the left edge.
    /// </summary>
    public int Left
    {
        get => _left;
        set => _left = value;
    }

    /// <summary>
    /// Gets or sets the padding value for the top edge.
    /// </summary>
    public int Top
    {
        get => _top;
        set => _top = value;
    }

    /// <summary>
    /// Gets or sets the padding value for the right edge.
    /// </summary>
    public int Right
    {
        get => _right;
        set => _right = value;
    }

    /// <summary>
    /// Gets or sets the padding value for the bottom edge.
    /// </summary>
    public int Bottom
    {
        get => _bottom;
        set => _bottom = value;
    }

    /// <summary>
    /// Gets or sets the padding value for all edges.
    /// </summary>
    public int All
    {
        get => _left == _top && _top == _right && _right == _bottom ? _left : -1;
        set => _left = _top = _right = _bottom = value;
    }

    /// <summary>
    /// Gets the combined padding for the left and right edges.
    /// </summary>
    public int Horizontal => _left + _right;

    /// <summary>
    /// Gets the combined padding for the top and bottom edges.
    /// </summary>
    public int Vertical => _top + _bottom;

    /// <summary>
    /// Gets the padding information in the form of a Size.
    /// </summary>
    public System.Drawing.Size Size => new System.Drawing.Size(Horizontal, Vertical);

    /// <summary>
    /// Computes the sum of two Padding values.
    /// </summary>
    public static Padding Add(Padding p1, Padding p2)
    {
        return new Padding(p1.Left + p2.Left, p1.Top + p2.Top, p1.Right + p2.Right, p1.Bottom + p2.Bottom);
    }

    /// <summary>
    /// Subtracts one Padding value from another.
    /// </summary>
    public static Padding Subtract(Padding p1, Padding p2)
    {
        return new Padding(p1.Left - p2.Left, p1.Top - p2.Top, p1.Right - p2.Right, p1.Bottom - p2.Bottom);
    }

    public static Padding operator +(Padding p1, Padding p2) => Add(p1, p2);
    public static Padding operator -(Padding p1, Padding p2) => Subtract(p1, p2);

    public static bool operator ==(Padding p1, Padding p2) => p1.Equals(p2);
    public static bool operator !=(Padding p1, Padding p2) => !p1.Equals(p2);

    public bool Equals(Padding other)
    {
        return _left == other._left && _top == other._top && _right == other._right && _bottom == other._bottom;
    }

    public override bool Equals(object? obj)
    {
        return obj is Padding padding && Equals(padding);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_left, _top, _right, _bottom);
    }

    public override string ToString()
    {
        return $"{{Left={_left}, Top={_top}, Right={_right}, Bottom={_bottom}}}";
    }
}
