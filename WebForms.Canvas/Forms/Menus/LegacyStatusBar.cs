namespace System.Windows.Forms;

// ── StatusBarPanelAutoSize ────────────────────────────────────────────────────
public enum StatusBarPanelAutoSize
{
    None     = 1,
    Spring   = 2,
    Contents = 3,
}

// ── StatusBarPanelBorderStyle ─────────────────────────────────────────────────
public enum StatusBarPanelBorderStyle
{
    None   = 1,
    Raised = 2,
    Sunken = 3,
}

// ── StatusBarPanelStyle ───────────────────────────────────────────────────────
public enum StatusBarPanelStyle
{
    Text   = 1,
    OwnerDraw = 2,
}

// ── StatusBarPanel ────────────────────────────────────────────────────────────

/// <summary>
/// A panel inside a legacy <see cref="StatusBar"/> control.
/// Matches the WinForms <c>StatusBarPanel</c> class.
/// </summary>
public class StatusBarPanel
{
    private string _text = string.Empty;
    private string _toolTipText = string.Empty;
    private int    _width = 100;
    private int    _minWidth = 10;
    private HorizontalAlignment _alignment = HorizontalAlignment.Left;
    private StatusBarPanelAutoSize  _autoSize  = StatusBarPanelAutoSize.None;
    private StatusBarPanelBorderStyle _borderStyle = StatusBarPanelBorderStyle.Sunken;
    private StatusBarPanelStyle     _style     = StatusBarPanelStyle.Text;

    internal StatusBar? Owner { get; set; }

    public string Text
    {
        get => _text;
        set { _text = value ?? string.Empty; Owner?.Invalidate(); }
    }

    public string ToolTipText
    {
        get => _toolTipText;
        set => _toolTipText = value ?? string.Empty;
    }

    public int Width
    {
        get => _width;
        set { _width = Math.Max(_minWidth, value); Owner?.Invalidate(); }
    }

    public int MinWidth
    {
        get => _minWidth;
        set => _minWidth = Math.Max(0, value);
    }

    public HorizontalAlignment Alignment
    {
        get => _alignment;
        set { _alignment = value; Owner?.Invalidate(); }
    }

    public StatusBarPanelAutoSize AutoSize
    {
        get => _autoSize;
        set { _autoSize = value; Owner?.Invalidate(); }
    }

    public StatusBarPanelBorderStyle BorderStyle
    {
        get => _borderStyle;
        set { _borderStyle = value; Owner?.Invalidate(); }
    }

    public StatusBarPanelStyle Style
    {
        get => _style;
        set { _style = value; Owner?.Invalidate(); }
    }

    // Misc (stub, matching WinForms API surface)
    public string? Name  { get; set; }
    public object? Tag   { get; set; }
    public Image?  Icon  { get; set; }
    public Image?  Image { get; set; }
}

// ── StatusBarPanelCollection ──────────────────────────────────────────────────

public sealed class StatusBarPanelCollection : System.Collections.IEnumerable
{
    private readonly StatusBar _owner;
    private readonly List<StatusBarPanel> _list = new();

    public StatusBarPanelCollection(StatusBar owner) => _owner = owner;

    public int Count => _list.Count;

    public StatusBarPanel this[int index] => _list[index];

    public StatusBarPanel? this[string? key]
        => _list.FirstOrDefault(p => p.Name == key);

    public void Add(StatusBarPanel panel)
    {
        panel.Owner = _owner;
        _list.Add(panel);
        _owner.Invalidate();
    }

    public StatusBarPanel Add(string text)
    {
        var p = new StatusBarPanel { Text = text };
        Add(p);
        return p;
    }

    public void AddRange(StatusBarPanel[] panels)
    {
        foreach (var p in panels) Add(p);
    }

    public void Remove(StatusBarPanel panel)
    {
        panel.Owner = null;
        _list.Remove(panel);
        _owner.Invalidate();
    }

    public void RemoveAt(int index)
    {
        _list[index].Owner = null;
        _list.RemoveAt(index);
        _owner.Invalidate();
    }

    public void Clear()
    {
        foreach (var p in _list) p.Owner = null;
        _list.Clear();
        _owner.Invalidate();
    }

    public bool Contains(StatusBarPanel panel) => _list.Contains(panel);
    public int  IndexOf(StatusBarPanel panel)  => _list.IndexOf(panel);

    public IEnumerator<StatusBarPanel> GetEnumerator() => _list.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _list.GetEnumerator();
}

// ── StatusBarDrawItemEventArgs ────────────────────────────────────────────────

public class StatusBarDrawItemEventArgs : DrawItemEventArgs
{
    public StatusBarPanel Panel { get; }

    public StatusBarDrawItemEventArgs(
        Graphics g, Font font, Rectangle r, int index,
        DrawItemState state, StatusBarPanel panel)
        : base(g, font, r, index, state)
    {
        Panel = panel;
    }
}

public delegate void StatusBarDrawItemEventHandler(object sender, StatusBarDrawItemEventArgs e);

// ── StatusBarPanelClickEventArgs ──────────────────────────────────────────────

public class StatusBarPanelClickEventArgs : MouseEventArgs
{
    public StatusBarPanel StatusBarPanel { get; }

    public StatusBarPanelClickEventArgs(StatusBarPanel panel, MouseButtons button, int clicks, int x, int y)
        : base(button, clicks, x, y, 0)
    {
        StatusBarPanel = panel;
    }
}

public delegate void StatusBarPanelClickEventHandler(object sender, StatusBarPanelClickEventArgs e);

// ── StatusBar ─────────────────────────────────────────────────────────────────

/// <summary>
/// Legacy status bar control (pre-<see cref="StatusStrip"/>).
/// Renders a simple single-line bar at the bottom of a form; supports a
/// <c>Panels</c> collection with text, borders, and spring auto-sizing.
/// Matches the WinForms <c>StatusBar : Control</c> hierarchy.
/// </summary>
public class StatusBar : Control
{
    private StatusBarPanelCollection? _panels;
    private bool   _showPanels   = false;
    private bool   _sizingGrip   = true;
    private string _simpleText   = string.Empty;

    private const int BarHeight  = 22;
    private const int GripSize   = 12;

    public StatusBar()
    {
        Dock      = DockStyle.Bottom;
        Height    = BarHeight;
        BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
        TabStop   = false;
    }

    // ── Properties ────────────────────────────────────────────────────────────

    public StatusBarPanelCollection Panels
        => _panels ??= new StatusBarPanelCollection(this);

    public bool ShowPanels
    {
        get => _showPanels;
        set { _showPanels = value; Invalidate(); }
    }

    public bool SizingGrip
    {
        get => _sizingGrip;
        set { _sizingGrip = value; Invalidate(); }
    }

    // When ShowPanels is false, this simple text fills the whole bar.
    public override string Text
    {
        get => _simpleText;
        set { _simpleText = value ?? string.Empty; Invalidate(); }
    }

    // ── Events ─────────────────────────────────────────────────────────────────

    public event StatusBarPanelClickEventHandler? PanelClick;
    public event StatusBarDrawItemEventHandler?   DrawItem;

    protected virtual void OnPanelClick(StatusBarPanelClickEventArgs e) => PanelClick?.Invoke(this, e);
    protected virtual void OnDrawItem(StatusBarDrawItemEventArgs e)      => DrawItem?.Invoke(this, e);

    // ── Mouse (panel click) ────────────────────────────────────────────────────

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (_showPanels && _panels is not null)
        {
            int x = 0;
            int total = TotalFixedWidth();
            foreach (var panel in _panels)
            {
                int w = PanelWidth(panel, total);
                if (e.X >= x && e.X < x + w)
                {
                    OnPanelClick(new StatusBarPanelClickEventArgs(panel, e.Button, e.Clicks, e.X, e.Y));
                    break;
                }
                x += w;
            }
        }
        base.OnMouseDown(e);
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        using var bgBrush = new SolidBrush(Drawing.Color.FromArgb(BackColor.R, BackColor.G, BackColor.B));
        g.FillRectangle(bgBrush, 0, 0, Width, Height);

        // Top border
        using var topPen = new Pen(Drawing.Color.FromArgb(160, 160, 160));
        g.DrawLine(topPen, 0, 0, Width, 0);

        if (_showPanels && _panels is not null && _panels.Count > 0)
            PaintPanels(g);
        else
            PaintSimpleText(g);

        if (_sizingGrip)
            PaintSizingGrip(g);

        base.OnPaint(e);
    }

    private void PaintSimpleText(Graphics g)
    {
        using var fg = new SolidBrush(Drawing.Color.FromArgb(ForeColor.R, ForeColor.G, ForeColor.B));
        g.DrawString(_simpleText, "Segoe UI", 11, fg, 4, (Height - 13) / 2);
    }

    private void PaintPanels(Graphics g)
    {
        if (_panels is null) return;
        int x   = 0;
        int tot = TotalFixedWidth();

        foreach (var panel in _panels)
        {
            int w = PanelWidth(panel, tot);
            var r = new Rectangle(x + 1, 2, w - 2, Height - 4);

            // Panel border
            using var bBrush = new SolidBrush(Drawing.Color.FromArgb(BackColor.R, BackColor.G, BackColor.B));
            g.FillRectangle(bBrush, r);

            if (panel.BorderStyle != StatusBarPanelBorderStyle.None)
            {
                var borderCol = panel.BorderStyle == StatusBarPanelBorderStyle.Sunken
                    ? Drawing.Color.FromArgb(128, 128, 128)
                    : Drawing.Color.FromArgb(220, 220, 220);
                using var bPen = new Pen(borderCol);
                g.DrawRectangle(bPen, r.X, r.Y, r.Width - 1, r.Height - 1);
            }

            if (panel.Style == StatusBarPanelStyle.OwnerDraw && DrawItem is not null)
            {
                var font = new Font("Segoe UI", 11);
                var args = new StatusBarDrawItemEventArgs(g, font, r, 0, DrawItemState.None, panel);
                OnDrawItem(args);
            }
            else
            {
                using var fg = new SolidBrush(Drawing.Color.FromArgb(ForeColor.R, ForeColor.G, ForeColor.B));
                int tx = panel.Alignment switch
                {
                    HorizontalAlignment.Center => r.X + (r.Width - panel.Text.Length * 7) / 2,
                    HorizontalAlignment.Right  => r.Right - panel.Text.Length * 7 - 4,
                    _                          => r.X + 4,
                };
                g.DrawString(panel.Text, "Segoe UI", 11, fg, tx, r.Y + (r.Height - 13) / 2);
            }

            x += w;
        }
    }

    private void PaintSizingGrip(Graphics g)
    {
        int gx = Width - GripSize;
        int gy = Height - GripSize;
        using var pen1 = new Pen(Drawing.Color.White);
        using var pen2 = new Pen(Drawing.Color.FromArgb(128, 128, 128));
        for (int i = 0; i < 3; i++)
        {
            int ox = gx + i * 4;
            int oy = gy + i * 4;
            g.DrawLine(pen2, ox + GripSize, gy,       gx,       gy + GripSize);
            g.DrawLine(pen1, ox + GripSize + 1, gy,   gx + 1,   gy + GripSize);
        }
    }

    // ── Layout helpers ─────────────────────────────────────────────────────────

    private int TotalFixedWidth()
    {
        if (_panels is null) return 0;
        int total = 0;
        foreach (var p in _panels)
            if (p.AutoSize != StatusBarPanelAutoSize.Spring)
                total += p.Width;
        return total;
    }

    private int PanelWidth(StatusBarPanel panel, int totalFixed)
    {
        if (panel.AutoSize == StatusBarPanelAutoSize.Spring)
        {
            // Count spring panels
            int springs = 0;
            if (_panels is not null)
                foreach (var p in _panels)
                    if (p.AutoSize == StatusBarPanelAutoSize.Spring) springs++;
            int remaining = Math.Max(0, Width - totalFixed - (SizingGrip ? GripSize : 0));
            return springs > 0 ? remaining / springs : panel.Width;
        }
        return panel.Width;
    }
}
