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

    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; } = 100;
    public int Height { get; set; } = 20;
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
