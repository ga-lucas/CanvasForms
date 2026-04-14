using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

public class SplitContainer : ContainerControl
{
    private readonly SplitterPanel _panel1;
    private readonly SplitterPanel _panel2;

    private Orientation _orientation = Orientation.Vertical;
    private int _splitterDistance = 50;
    private int _splitterWidth = 4;

    private FixedPanel _fixedPanel = FixedPanel.None;
    private bool _isSplitterFixed;

    private int _splitterIncrement = 1;

    private int _panel1MinSize = 25;
    private int _panel2MinSize = 25;

    private bool _dragging;
    private int _dragOffset;

    public SplitContainer()
    {
        TabStop = false;

        _panel1 = new SplitterPanel(this, panelIndex: 1)
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.None,
        };

        _panel2 = new SplitterPanel(this, panelIndex: 2)
        {
            BackColor = Color.Transparent,
            Dock = DockStyle.None,
        };

        base.Controls.Add(_panel1);
        base.Controls.Add(_panel2);

        BackColor = Color.Transparent;
        BorderStyle = BorderStyle.None;

        UpdateSplitterBounds();
    }

    public BorderStyle BorderStyle { get; set; } = BorderStyle.None;

    public SplitterPanel Panel1 => _panel1;

    public SplitterPanel Panel2 => _panel2;

    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            if (_orientation != value)
            {
                _orientation = value;
                CoerceSplitterDistance();
                PerformLayout();
                Invalidate();
            }
        }
    }

    public int SplitterDistance
    {
        get => _splitterDistance;
        set
        {
            var newValue = value;
            if (newValue < 0) newValue = 0;

            if (_splitterIncrement > 1)
            {
                newValue = (int)Math.Round(newValue / (double)_splitterIncrement) * _splitterIncrement;
            }

            if (_splitterDistance != newValue)
            {
                _splitterDistance = newValue;
                CoerceSplitterDistance();
                PerformLayout();
                Invalidate();
                OnSplitterMoved(new SplitterEventArgs(0, 0,
                    _orientation == Orientation.Vertical ? _splitterDistance : 0,
                    _orientation == Orientation.Horizontal ? _splitterDistance : 0));
            }
        }
    }

    public int SplitterWidth
    {
        get => _splitterWidth;
        set
        {
            var newValue = Math.Max(1, value);
            if (_splitterWidth != newValue)
            {
                _splitterWidth = newValue;
                CoerceSplitterDistance();
                PerformLayout();
                Invalidate();
            }
        }
    }

    public FixedPanel FixedPanel
    {
        get => _fixedPanel;
        set
        {
            if (_fixedPanel != value)
            {
                _fixedPanel = value;
                PerformLayout();
                Invalidate();
            }
        }
    }

    public bool IsSplitterFixed
    {
        get => _isSplitterFixed;
        set
        {
            if (_isSplitterFixed != value)
            {
                _isSplitterFixed = value;
                if (_isSplitterFixed)
                {
                    _dragging = false;
                }
                Invalidate();
            }
        }
    }

    public int Panel1MinSize
    {
        get => _panel1MinSize;
        set
        {
            var newValue = Math.Max(0, value);
            if (_panel1MinSize != newValue)
            {
                _panel1MinSize = newValue;
                CoerceSplitterDistance();
                PerformLayout();
                Invalidate();
            }
        }
    }

    public int Panel2MinSize
    {
        get => _panel2MinSize;
        set
        {
            var newValue = Math.Max(0, value);
            if (_panel2MinSize != newValue)
            {
                _panel2MinSize = newValue;
                CoerceSplitterDistance();
                PerformLayout();
                Invalidate();
            }
        }
    }

    public int SplitterIncrement
    {
        get => _splitterIncrement;
        set
        {
            var newValue = Math.Max(1, value);
            if (_splitterIncrement != newValue)
            {
                _splitterIncrement = newValue;
            }
        }
    }

    public Rectangle SplitterRectangle => GetSplitterRect();

    public event SplitterEventHandler? SplitterMoved;
    public event SplitterEventHandler? SplitterMoving;

    protected virtual void OnSplitterMoved(SplitterEventArgs e) => SplitterMoved?.Invoke(this, e);
    protected virtual void OnSplitterMoving(SplitterEventArgs e) => SplitterMoving?.Invoke(this, e);

    public override void PerformLayout()
    {
        // SplitContainer does not rely on base docking/anchoring for Panel1/Panel2.
        // It owns their bounds.
        UpdateSplitterBounds();

        _panel1.PerformLayout();
        _panel2.PerformLayout();

        Invalidate();
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        var bounds = new Rectangle(0, 0, Width, Height);
        if (BackColor != System.Drawing.Color.Transparent)
        {
            using var bgBrush = new SolidBrush(BackColor);
            g.FillRectangle(bgBrush, bounds);
        }

        DrawBorder(g, bounds);

        // Let user paint
        base.OnPaint(e);

        // Child painting is handled by Form recursive painter, but Panel/GroupBox do explicit painting.
        // We match Panel behavior here so SplitterPanel children are clipped to their panels.
        PaintPanel(g, _panel1);
        PaintPanel(g, _panel2);

        DrawSplitter(g);
    }

    private void PaintPanel(Graphics g, SplitterPanel panel)
    {
        if (!panel.Visible) return;

        g.TranslateTransform(panel.Left, panel.Top);

        // Clip to panel bounds (important for split resizing).
        g.Save();
        g.SetClip(new Rectangle(0, 0, panel.Width, panel.Height));

        var panelArgs = new PaintEventArgs(g, new Rectangle(0, 0, panel.Width, panel.Height));
        panel.OnPaint(panelArgs);

        // Panel.OnPaint does its own children; SplitterPanel is a Panel-derived implementation.

        g.Restore();
        g.TranslateTransform(-panel.Left, -panel.Top);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseDown(e);
            return;
        }

        if (!_isSplitterFixed && e.Button == MouseButtons.Left && IsInSplitter(e.X, e.Y))
        {
            _dragging = true;
            _dragOffset = _orientation == Orientation.Vertical ? e.X - _splitterDistance : e.Y - _splitterDistance;
            Capture = true;

            // Focus behavior: SplitContainer itself is not selectable.
            Invalidate();
            base.OnMouseDown(e);
            return;
        }

        // Route to appropriate panel
        var hitPanel = GetPanelAt(e.X, e.Y);
        if (hitPanel != null)
        {
            // Focus routed like Panel.
            if (FindForm() is Form form)
            {
                var deepest = FindDeepestEnabledChild(hitPanel, e.X - hitPanel.Left, e.Y - hitPanel.Top);
                if (deepest != null)
                {
                    form.FocusedControl = deepest;
                    deepest.Focus();
                }
                else
                {
                    form.FocusedControl = hitPanel;
                }
            }

            var args = new MouseEventArgs(e.Button, e.Clicks, e.X - hitPanel.Left, e.Y - hitPanel.Top);
            hitPanel.OnMouseDown(args);
            return;
        }

        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseMove(e);
            return;
        }

        // Cursor support (FormRenderer queries the hovered control's Cursor)
        if (!_dragging && !_isSplitterFixed)
        {
            var wantsSplit = IsInSplitter(e.X, e.Y);
            var desired = wantsSplit
                ? (_orientation == Orientation.Vertical ? Cursor.VSplit : Cursor.HSplit)
                : DefaultCursor;

            if (Cursor != desired)
            {
                Cursor = desired;
                Invalidate();
            }
        }

        if (_dragging && !_isSplitterFixed)
        {
            var raw = _orientation == Orientation.Vertical ? e.X - _dragOffset : e.Y - _dragOffset;

            if (_splitterIncrement > 1)
            {
                raw = (int)Math.Round(raw / (double)_splitterIncrement) * _splitterIncrement;
            }

            var coerced = CoerceSplitterDistance(raw);
            if (coerced != _splitterDistance)
            {
                _splitterDistance = coerced;
                UpdateSplitterBounds();
                OnSplitterMoving(CreateSplitterEventArgs(e));
                Invalidate();
            }
            return;
        }

        var hitPanel = GetPanelAt(e.X, e.Y);
        if (hitPanel != null)
        {
            var args = new MouseEventArgs(e.Button, e.Clicks, e.X - hitPanel.Left, e.Y - hitPanel.Top);
            hitPanel.OnMouseMove(args);
            return;
        }

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseUp(e);
            return;
        }

        if (_dragging)
        {
            _dragging = false;
            Capture = false;
            OnSplitterMoved(CreateSplitterEventArgs(e));
            Invalidate();
            base.OnMouseUp(e);
            return;
        }

        var hitPanel = GetPanelAt(e.X, e.Y);
        if (hitPanel != null)
        {
            var args = new MouseEventArgs(e.Button, e.Clicks, e.X - hitPanel.Left, e.Y - hitPanel.Top);
            hitPanel.OnMouseUp(args);
            return;
        }

        base.OnMouseUp(e);
    }

    private SplitterEventArgs CreateSplitterEventArgs(MouseEventArgs e)
    {
        // X/Y are mouse coords relative to SplitContainer.
        // SplitX/SplitY are the splitter location.
        var splitX = _orientation == Orientation.Vertical ? _splitterDistance : 0;
        var splitY = _orientation == Orientation.Horizontal ? _splitterDistance : 0;
        return new SplitterEventArgs(e.X, e.Y, splitX, splitY);
    }

    private SplitterPanel? GetPanelAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) return null;

        if (_panel1.Visible && x >= _panel1.Left && x < _panel1.Left + _panel1.Width && y >= _panel1.Top && y < _panel1.Top + _panel1.Height)
            return _panel1;

        if (_panel2.Visible && x >= _panel2.Left && x < _panel2.Left + _panel2.Width && y >= _panel2.Top && y < _panel2.Top + _panel2.Height)
            return _panel2;

        return null;
    }

    private bool IsInSplitter(int x, int y)
    {
        var splitter = GetSplitterRect();
        return x >= splitter.X && x < splitter.Right && y >= splitter.Y && y < splitter.Bottom;
    }

    private Rectangle GetSplitterRect()
    {
        if (_orientation == Orientation.Vertical)
        {
            return new Rectangle(_splitterDistance, 0, _splitterWidth, Height);
        }

        return new Rectangle(0, _splitterDistance, Width, _splitterWidth);
    }

    private void DrawSplitter(Graphics g)
    {
        var rect = GetSplitterRect();

        // WinForms default: subtle 3D splitter.
        using var bgBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
        g.FillRectangle(bgBrush, rect);

        using var dark = new Pen(Color.FromArgb(160, 160, 160));
        using var light = new Pen(Color.FromArgb(255, 255, 255));

        if (_orientation == Orientation.Vertical)
        {
            g.DrawLine(dark, rect.X, rect.Y, rect.X, rect.Bottom);
            g.DrawLine(light, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom);
        }
        else
        {
            g.DrawLine(dark, rect.X, rect.Y, rect.Right, rect.Y);
            g.DrawLine(light, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
        }
    }

    private void DrawBorder(Graphics g, Rectangle bounds)
    {
        if (BorderStyle == BorderStyle.None) return;

        switch (BorderStyle)
        {
            case BorderStyle.FixedSingle:
                using (var pen = new Pen(Color.FromArgb(122, 122, 122)))
                {
                    g.DrawRectangle(pen, bounds);
                }
                break;

            case BorderStyle.Fixed3D:
                using (var darkPen = new Pen(Color.FromArgb(122, 122, 122)))
                {
                    g.DrawRectangle(darkPen, bounds);
                }
                using (var lightPen = new Pen(Color.FromArgb(240, 240, 240)))
                {
                    g.DrawRectangle(lightPen, new Rectangle(1, 1, Width - 3, Height - 3));
                }
                break;
        }
    }

    private void UpdateSplitterBounds()
    {
        CoerceSplitterDistance();

        if (_orientation == Orientation.Vertical)
        {
            _panel1.Left = 0;
            _panel1.Top = 0;
            _panel1.Width = Math.Max(0, _splitterDistance);
            _panel1.Height = Height;

            _panel2.Left = _splitterDistance + _splitterWidth;
            _panel2.Top = 0;
            _panel2.Width = Math.Max(0, Width - _panel2.Left);
            _panel2.Height = Height;
        }
        else
        {
            _panel1.Left = 0;
            _panel1.Top = 0;
            _panel1.Width = Width;
            _panel1.Height = Math.Max(0, _splitterDistance);

            _panel2.Left = 0;
            _panel2.Top = _splitterDistance + _splitterWidth;
            _panel2.Width = Width;
            _panel2.Height = Math.Max(0, Height - _panel2.Top);
        }

        // Ensure children layout inside panels gets updated.
        _panel1.PerformLayout();
        _panel2.PerformLayout();
    }

    private void CoerceSplitterDistance()
    {
        _splitterDistance = CoerceSplitterDistance(_splitterDistance);
    }

    private int CoerceSplitterDistance(int desired)
    {
        var max = GetMaxSplitterDistance();
        if (desired < _panel1MinSize) desired = _panel1MinSize;
        if (desired > max) desired = max;
        return desired;
    }

    private int GetMaxSplitterDistance()
    {
        if (_orientation == Orientation.Vertical)
        {
            // Panel2 must have at least Panel2MinSize.
            return Math.Max(_panel1MinSize, Width - _splitterWidth - _panel2MinSize);
        }

        return Math.Max(_panel1MinSize, Height - _splitterWidth - _panel2MinSize);
    }

    private Control? FindDeepestEnabledChild(Control root, int x, int y)
    {
        // Match Form.FindDeepestHitControl behavior in the simplest form for focus selection.
        for (var i = root.Controls.Count - 1; i >= 0; i--)
        {
            var child = root.Controls[i];
            if (!child.Visible || !child.Enabled) continue;

            if (x >= child.Left && x < child.Left + child.Width && y >= child.Top && y < child.Top + child.Height)
            {
                if (child.HasChildren)
                {
                    var deep = FindDeepestEnabledChild(child, x - child.Left, y - child.Top);
                    if (deep != null) return deep;
                }

                return child;
            }
        }

        return null;
    }
}

public class SplitterPanel : Panel
{
    internal SplitterPanel(SplitContainer owner, int panelIndex)
    {
        Owner = owner;
        PanelIndex = panelIndex;

        // WinForms: panels are not selectable.
        TabStop = false;
        BorderStyle = BorderStyle.None;
        BackColor = Color.Transparent;
    }

    public SplitContainer Owner { get; }

    public int PanelIndex { get; }

    public override string ToString() => $"SplitterPanel{PanelIndex}";
}

public enum FixedPanel
{
    None = 0,
    Panel1 = 1,
    Panel2 = 2
}

// Orientation is a WinForms type; it is defined elsewhere in the repo.
