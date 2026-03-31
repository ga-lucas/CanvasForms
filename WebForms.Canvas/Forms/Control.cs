using WebForms.Canvas.Drawing;

namespace WebForms.Canvas.Forms;

public abstract class Control
{
    private Control? _parent;
    private readonly List<Control> _controls = new();
    private string _text = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                Invalidate();
            }
        }
    }

    private int _left;
    private int _top;
    private int _width = 100;
    private int _height = 20;
    private DockStyle _dock = DockStyle.None;
    private AnchorStyles _anchor = AnchorStyles.Top | AnchorStyles.Left;

    // Original bounds before docking/anchoring (for anchor calculations)
    internal int OriginalLeft;
    internal int OriginalTop;
    internal int OriginalWidth;
    internal int OriginalHeight;
    internal int OriginalParentWidth;
    internal int OriginalParentHeight;
    internal bool OriginalBoundsSet = false;

    public int Left 
    { 
        get => _left;
        set
        {
            if (_left != value)
            {
                _left = value;
                Invalidate();
            }
        }
    }

    public int Top 
    { 
        get => _top;
        set
        {
            if (_top != value)
            {
                _top = value;
                Invalidate();
            }
        }
    }

    public int Width 
    { 
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                Invalidate();
            }
        }
    }

    public int Height 
    { 
        get => _height;
        set
        {
            if (_height != value)
            {
                _height = value;
                Invalidate();
            }
        }
    }

    public DockStyle Dock
    {
        get => _dock;
        set
        {
            if (_dock != value)
            {
                _dock = value;
                _parent?.PerformLayout();
                Invalidate();
            }
        }
    }

    public AnchorStyles Anchor
    {
        get => _anchor;
        set
        {
            if (_anchor != value)
            {
                _anchor = value;
                Invalidate();
            }
        }
    }

    public Color BackColor { get; set; } = Color.White;
    public Color ForeColor { get; set; } = Color.Black;
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public object? Tag { get; set; }

    // Location and Size helpers
    public Point Location
    {
        get => new Point(Left, Top);
        set { Left = value.X; Top = value.Y; }
    }

    public Size Size
    {
        get => new Size(Width, Height);
        set { Width = value.Width; Height = value.Height; }
    }

    public Rectangle Bounds
    {
        get => new Rectangle(Left, Top, Width, Height);
        set { Left = value.X; Top = value.Y; Width = value.Width; Height = value.Height; }
    }

    // Parent/child relationships
    public Control? Parent
    {
        get => _parent;
        internal set => _parent = value;
    }

    public ControlCollection Controls => new ControlCollection(this, _controls);

    // Paint events
    public event PaintEventHandler? Paint;

    // Mouse events
    public event MouseEventHandler? MouseDown;
    public event MouseEventHandler? MouseUp;
    public event MouseEventHandler? MouseMove;
    public event MouseEventHandler? MouseClick;
    public event MouseEventHandler? MouseDoubleClick;
    public event MouseEventHandler? MouseEnter;
    public event MouseEventHandler? MouseLeave;

    // Keyboard events
    public event KeyEventHandler? KeyDown;
    public event KeyEventHandler? KeyUp;
    public event KeyPressEventHandler? KeyPress;

    protected internal virtual void OnPaint(PaintEventArgs e)
    {
        Paint?.Invoke(this, e);
    }

    protected internal virtual void OnMouseDown(MouseEventArgs e)
    {
        MouseDown?.Invoke(this, e);
    }

    protected internal virtual void OnMouseUp(MouseEventArgs e)
    {
        MouseUp?.Invoke(this, e);
    }

    protected internal virtual void OnMouseMove(MouseEventArgs e)
    {
        MouseMove?.Invoke(this, e);
    }

    protected internal virtual void OnMouseClick(MouseEventArgs e)
    {
        MouseClick?.Invoke(this, e);
    }

    protected internal virtual void OnMouseDoubleClick(MouseEventArgs e)
    {
        MouseDoubleClick?.Invoke(this, e);
    }

    protected internal virtual void OnMouseEnter(EventArgs e)
    {
        MouseEnter?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, 0, 0));
    }

    protected internal virtual void OnMouseLeave(EventArgs e)
    {
        MouseLeave?.Invoke(this, new MouseEventArgs(MouseButtons.None, 0, 0, 0));
    }

    protected internal virtual void OnKeyDown(KeyEventArgs e)
    {
        KeyDown?.Invoke(this, e);
    }

    protected internal virtual void OnKeyUp(KeyEventArgs e)
    {
        KeyUp?.Invoke(this, e);
    }

    protected internal virtual void OnKeyPress(KeyPressEventArgs e)
    {
        KeyPress?.Invoke(this, e);
    }

    public void Invalidate()
    {
        // Async fire-and-forget - render will happen asynchronously
        var task = RequestRender?.Invoke();
    }

    /// <summary>
    /// Performs layout logic for docked and anchored controls
    /// </summary>
    public virtual void PerformLayout()
    {
        if (_controls.Count == 0) return;

        // First, store original bounds for anchored controls
        foreach (var control in _controls)
        {
            if (!control.OriginalBoundsSet && control.Dock == DockStyle.None)
            {
                control.OriginalLeft = control.Left;
                control.OriginalTop = control.Top;
                control.OriginalWidth = control.Width;
                control.OriginalHeight = control.Height;
                control.OriginalParentWidth = this.Width;
                control.OriginalParentHeight = this.Height;
                control.OriginalBoundsSet = true;
            }
        }

        // Available client area
        var clientRect = new Rectangle(0, 0, Width, Height);

        // Process docked controls in order: Top, Bottom, Left, Right, then Fill
        var dockedControls = _controls.Where(c => c.Visible && c.Dock != DockStyle.None).ToList();
        var anchoredControls = _controls.Where(c => c.Visible && c.Dock == DockStyle.None).ToList();

        // Apply docking in priority order
        foreach (var dockStyle in new[] { DockStyle.Top, DockStyle.Bottom, DockStyle.Left, DockStyle.Right, DockStyle.Fill })
        {
            foreach (var control in dockedControls.Where(c => c.Dock == dockStyle))
            {
                switch (control.Dock)
                {
                    case DockStyle.Top:
                        control.Left = clientRect.X;
                        control.Top = clientRect.Y;
                        control.Width = clientRect.Width;
                        // Height stays as set by user
                        clientRect.Y += control.Height;
                        clientRect.Height -= control.Height;
                        break;

                    case DockStyle.Bottom:
                        control.Left = clientRect.X;
                        control.Width = clientRect.Width;
                        clientRect.Height -= control.Height;
                        control.Top = clientRect.Y + clientRect.Height;
                        break;

                    case DockStyle.Left:
                        control.Left = clientRect.X;
                        control.Top = clientRect.Y;
                        control.Height = clientRect.Height;
                        // Width stays as set by user
                        clientRect.X += control.Width;
                        clientRect.Width -= control.Width;
                        break;

                    case DockStyle.Right:
                        control.Top = clientRect.Y;
                        control.Height = clientRect.Height;
                        clientRect.Width -= control.Width;
                        control.Left = clientRect.X + clientRect.Width;
                        break;

                    case DockStyle.Fill:
                        control.Left = clientRect.X;
                        control.Top = clientRect.Y;
                        control.Width = clientRect.Width;
                        control.Height = clientRect.Height;
                        break;
                }
            }
        }

        // Apply anchoring to non-docked controls
        foreach (var control in anchoredControls)
        {
            if (!control.OriginalBoundsSet) continue;

            var anchor = control.Anchor;
            var deltaWidth = Width - control.OriginalParentWidth;
            var deltaHeight = Height - control.OriginalParentHeight;

            // Calculate new position and size based on anchoring
            var left = control.OriginalLeft;
            var top = control.OriginalTop;
            var width = control.OriginalWidth;
            var height = control.OriginalHeight;

            bool anchoredLeft = (anchor & AnchorStyles.Left) != 0;
            bool anchoredRight = (anchor & AnchorStyles.Right) != 0;
            bool anchoredTop = (anchor & AnchorStyles.Top) != 0;
            bool anchoredBottom = (anchor & AnchorStyles.Bottom) != 0;

            if (anchoredLeft && anchoredRight)
            {
                // Stretch horizontally
                width = control.OriginalWidth + deltaWidth;
            }
            else if (anchoredRight && !anchoredLeft)
            {
                // Move with right edge
                left = control.OriginalLeft + deltaWidth;
            }
            // else if only left is anchored (default), position stays the same

            if (anchoredTop && anchoredBottom)
            {
                // Stretch vertically
                height = control.OriginalHeight + deltaHeight;
            }
            else if (anchoredBottom && !anchoredTop)
            {
                // Move with bottom edge
                top = control.OriginalTop + deltaHeight;
            }
            // else if only top is anchored (default), position stays the same

            control.Left = left;
            control.Top = top;
            control.Width = width;
            control.Height = height;
        }

        Invalidate();
    }

    internal Func<Task>? RequestRender { get; set; }

    // Propagate RequestRender to all children
    internal void PropagateRequestRender(Func<Task>? requestRender)
    {
        RequestRender = requestRender;
        foreach (var child in _controls)
        {
            child.PropagateRequestRender(requestRender);
        }
    }
}

// Control collection for managing child controls
public class ControlCollection
{
    private readonly Control _owner;
    private readonly List<Control> _list;

    internal ControlCollection(Control owner, List<Control> list)
    {
        _owner = owner;
        _list = list;
    }

    public int Count => _list.Count;

    public Control this[int index] => _list[index];

    public void Add(Control control)
    {
        control.Parent = _owner;
        control.RequestRender = _owner.RequestRender;
        _list.Add(control);
        _owner.Invalidate();
    }

    public void Remove(Control control)
    {
        if (_list.Remove(control))
        {
            control.Parent = null;
            _owner.Invalidate();
        }
    }

    public void Clear()
    {
        foreach (var control in _list)
        {
            control.Parent = null;
        }
        _list.Clear();
        _owner.Invalidate();
    }

    public IEnumerator<Control> GetEnumerator() => _list.GetEnumerator();
}
