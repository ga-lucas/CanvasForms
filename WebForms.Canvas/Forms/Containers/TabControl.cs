
namespace System.Windows.Forms;

public class TabControl : Control
{
    private const int DefaultHeaderHeight = 24;
    private const int DefaultTabPaddingX = 16;
    private const int DefaultTabMinWidth = 60;

    private readonly TabPageCollection _tabPages;
    private readonly List<Rectangle> _tabHeaderRects = new();

    private int _selectedIndex = -1;
    private int _hoveredIndex = -1;
    private int _firstVisibleTab = 0;
    private int _lastVisibleTab = -1;

    private ControlCollection? _tabControlControls;

    public TabControl()
    {
        Width = 200;
        Height = 150;
        BackColor = CanvasColor.White;
        ForeColor = CanvasColor.Black;
        TabStop = true;

        SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint, true);

        _tabPages = new TabPageCollection(this);
    }

    // WinForms designer commonly uses tabControl.Controls.Add(tabPage).
    // Hide Controls so we can keep TabPages in sync for translated apps.
    public new ControlCollection Controls => _tabControlControls ??= new TabControlControlCollection(this);

    public TabAlignment Alignment { get; set; } = TabAlignment.Top;
    public TabAppearance Appearance { get; set; } = TabAppearance.Normal;
    public TabDrawMode DrawMode { get; set; } = TabDrawMode.Normal;
    public TabSizeMode SizeMode { get; set; } = TabSizeMode.Normal;

    public ImageList? ImageList { get; set; }

    public bool Multiline { get; set; }
    public bool HotTrack { get; set; }

    public int SelectedImageIndex { get; set; } = -1;

    public int ItemSizeWidth { get; set; } = 0;
    public int ItemSizeHeight { get; set; } = 0;

    public TabPageCollection TabPages => _tabPages;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetSelectedIndex(value, userInitiated: false);
    }

    public TabPage? SelectedTab
    {
        get => _selectedIndex >= 0 && _selectedIndex < _tabPages.Count ? _tabPages[_selectedIndex] : null;
        set
        {
            if (value == null)
            {
                SelectedIndex = -1;
                return;
            }

            var idx = _tabPages.IndexOf(value);
            if (idx >= 0)
            {
                SelectedIndex = idx;
            }
        }
    }

    public event EventHandler? SelectedIndexChanged;
    public event TabControlCancelEventHandler? Selecting;
    public event TabControlEventHandler? Selected;
    public event TabControlCancelEventHandler? Deselecting;
    public event TabControlEventHandler? Deselected;

    protected virtual void OnSelectedIndexChanged(EventArgs e) => SelectedIndexChanged?.Invoke(this, e);

    protected virtual void OnSelecting(TabControlCancelEventArgs e) => Selecting?.Invoke(this, e);
    protected virtual void OnSelected(TabControlEventArgs e) => Selected?.Invoke(this, e);
    protected virtual void OnDeselecting(TabControlCancelEventArgs e) => Deselecting?.Invoke(this, e);
    protected virtual void OnDeselected(TabControlEventArgs e) => Deselected?.Invoke(this, e);

    public override Rectangle DisplayRectangle
    {
        get
        {
            return GetPageBounds();
        }
    }

    public override void PerformLayout()
    {
        base.PerformLayout();

        var client = GetPageBounds();

        for (var i = 0; i < _tabPages.Count; i++)
        {
            var page = _tabPages[i];

            // Ensure pages are positioned and sized like WinForms (fill the page area).
            page.Left = client.X;
            page.Top = client.Y;
            page.Width = client.Width;
            page.Height = client.Height;

            page.Visible = i == _selectedIndex;

            if (page.Visible)
            {
                page.PerformLayout();
            }
        }

        Invalidate();
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        DrawControlBackground(g);

        // Border
        using (var pen = new Pen(CanvasColor.FromArgb(200, 200, 200)))
        {
            g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        PaintHeaderArea(g);

        base.OnPaint(e);
    }

    private void PaintHeaderArea(Graphics g)
    {
        var headerBounds = GetHeaderBounds();

        // Header strip background
        using (var headerBrush = new SolidBrush(CanvasColor.FromArgb(240, 240, 240)))
        {
            g.FillRectangle(headerBrush, headerBounds);
        }

        // Divider line between headers and page area
        using (var dividerPen = new Pen(CanvasColor.FromArgb(200, 200, 200)))
        {
            if (Alignment == TabAlignment.Top)
            {
                g.DrawLine(dividerPen, 0, headerBounds.Bottom - 1, Width, headerBounds.Bottom - 1);
            }
            else if (Alignment == TabAlignment.Bottom)
            {
                g.DrawLine(dividerPen, 0, headerBounds.Y, Width, headerBounds.Y);
            }
            else if (Alignment == TabAlignment.Left)
            {
                g.DrawLine(dividerPen, headerBounds.Right - 1, 0, headerBounds.Right - 1, Height);
            }
            else
            {
                g.DrawLine(dividerPen, headerBounds.X, 0, headerBounds.X, Height);
            }
        }

        BuildHeaderRects();
        PaintHeaders(g);

        // Simple scroll affordances for single-row top/bottom alignment.
        if (!Multiline && (Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom) && NeedsHeaderScroll())
        {
            PaintHeaderScrollButtons(g);
        }
    }

    private void PaintHeaders(Graphics g)
    {
        if (_tabPages.Count == 0) return;

        var headerBounds = GetHeaderBounds();

        // Clip headers to header area.
        g.Save();
        g.SetClip(headerBounds);

        for (var i = _firstVisibleTab; i < _tabPages.Count; i++)
        {
            var rect = _tabHeaderRects[i];
            if (Multiline)
            {
                // Multiline: draw all.
            }
            else
            {
                // Single row: stop once we leave visible header area.
                if (Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom)
                {
                    if (rect.X >= headerBounds.Right)
                    {
                        _lastVisibleTab = i - 1;
                        break;
                    }
                }
                else
                {
                    if (rect.Y >= headerBounds.Bottom)
                    {
                        _lastVisibleTab = i - 1;
                        break;
                    }
                }
            }

            var selected = i == _selectedIndex;
            var hovered = i == _hoveredIndex;

            var bg = selected ? CanvasColor.White : (hovered && HotTrack ? CanvasColor.FromArgb(229, 241, 251) : CanvasColor.FromArgb(240, 240, 240));

            using (var b = new SolidBrush(bg))
            {
                g.FillRectangle(b, rect);
            }

            using (var border = new Pen(CanvasColor.FromArgb(200, 200, 200)))
            {
                g.DrawRectangle(border, rect);
            }

            var textColor = Enabled ? ForeColor : DisabledForeColor;
            using var tb = new SolidBrush(textColor);

            var text = _tabPages[i].Text ?? string.Empty;

            if (DrawMode == TabDrawMode.OwnerDrawFixed)
            {
                var args = new DrawItemEventArgs(g, Font, rect, i, selected ? DrawItemState.Selected : DrawItemState.None);
                OnDrawItem(args);
            }
            else
            {
                g.DrawString(text, Font, tb, rect.X + 8, rect.Y + 5);
            }
        }

        g.Restore();

        // Make selected tab visually connect with the page area.
        if (_selectedIndex >= 0 && _selectedIndex < _tabHeaderRects.Count)
        {
            var rect = _tabHeaderRects[_selectedIndex];
            using var erase = new Pen(CanvasColor.White);

            var selectedHeaderBounds = GetHeaderBounds();

            if (Alignment == TabAlignment.Top)
            {
                var y = selectedHeaderBounds.Bottom - 1;
                g.DrawLine(erase, rect.X + 1, y, rect.Right - 1, y);
            }
            else if (Alignment == TabAlignment.Bottom)
            {
                var y = selectedHeaderBounds.Y;
                g.DrawLine(erase, rect.X + 1, y, rect.Right - 1, y);
            }
            else if (Alignment == TabAlignment.Left)
            {
                var x = selectedHeaderBounds.Right - 1;
                g.DrawLine(erase, x, rect.Y + 1, x, rect.Bottom - 1);
            }
            else
            {
                var x = selectedHeaderBounds.X;
                g.DrawLine(erase, x, rect.Y + 1, x, rect.Bottom - 1);
            }
        }
    }

    private DrawItemEventHandler? _drawItem;

    public event DrawItemEventHandler? DrawItem
    {
        add => _drawItem += value;
        remove => _drawItem -= value;
    }

    protected virtual void OnDrawItem(DrawItemEventArgs e)
    {
        _drawItem?.Invoke(this, e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        var hovered = HitTestTabHeader(e.X, e.Y);
        if (hovered != _hoveredIndex)
        {
            _hoveredIndex = hovered;
            Invalidate();
        }

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_hoveredIndex != -1)
        {
            _hoveredIndex = -1;
            Invalidate();
        }

        base.OnMouseLeave(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseDown(e);
            return;
        }

        Focus();

        var idx = HitTestTabHeader(e.X, e.Y);
        if (idx >= 0)
        {
            SetSelectedIndex(idx, userInitiated: true);
            return;
        }

        if (!Multiline && (Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom) && NeedsHeaderScroll())
        {
            if (HitTestScrollLeft(e.X, e.Y))
            {
                ScrollTabs(-1);
                return;
            }

            if (HitTestScrollRight(e.X, e.Y))
            {
                ScrollTabs(1);
                return;
            }
        }

        base.OnMouseDown(e);
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (!Enabled)
        {
            base.OnKeyDown(e);
            return;
        }

        if (!e.Handled)
        {
            if (e.KeyCode == Keys.Left)
            {
                if (_tabPages.Count > 0)
                {
                    var next = _selectedIndex <= 0 ? _tabPages.Count - 1 : _selectedIndex - 1;
                    SetSelectedIndex(next, userInitiated: true);
                    e.Handled = true;
                    return;
                }
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (_tabPages.Count > 0)
                {
                    var next = _selectedIndex < 0 || _selectedIndex >= _tabPages.Count - 1 ? 0 : _selectedIndex + 1;
                    SetSelectedIndex(next, userInitiated: true);
                    e.Handled = true;
                    return;
                }
            }
        }

        base.OnKeyDown(e);
    }

    internal void AddTabPage(TabPage page)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));

        // Match WinForms: TabPages are child controls.
        if (!base.Controls.Contains(page))
        {
            base.Controls.Add(page);
        }

        page.Visible = false;

        if (_selectedIndex == -1)
        {
            SetSelectedIndex(0, userInitiated: false);
        }

        PerformLayout();
    }

    internal void RemoveTabPageAt(int index)
    {
        if (index < 0 || index >= _tabPages.Count) throw new ArgumentOutOfRangeException(nameof(index));

        var page = _tabPages._list[index];

        if (index == _selectedIndex)
        {
            // Remove first so new selection lookup uses the correct tab indices, but keep old page reference
            // for deselect/selected event args.
            _tabPages._list.RemoveAt(index);

            var newIndex = _tabPages.Count == 0 ? -1 : Math.Min(index, _tabPages.Count - 1);
            SetSelectedIndex(newIndex, userInitiated: false, oldPageOverride: page, oldIndexOverride: index);
        }
        else
        {
            _tabPages._list.RemoveAt(index);

            if (index < _selectedIndex)
            {
                _selectedIndex--;
            }

            PerformLayout();
        }

        if (base.Controls.Contains(page))
        {
            base.Controls.Remove(page);
        }

        Invalidate();
    }

    internal void RemoveTabPage(TabPage page)
    {
        if (page == null) throw new ArgumentNullException(nameof(page));

        var index = _tabPages.IndexOf(page);
        if (index >= 0)
        {
            RemoveTabPageAt(index);
            return;
        }

        if (base.Controls.Contains(page))
        {
            base.Controls.Remove(page);
        }

        PerformLayout();
    }

    private void SetSelectedIndex(int index, bool userInitiated)
        => SetSelectedIndex(index, userInitiated, oldPageOverride: null, oldIndexOverride: -2);

    private void SetSelectedIndex(int index, bool userInitiated, TabPage? oldPageOverride, int oldIndexOverride)
    {
        if (index < -1 || index >= _tabPages.Count) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == _selectedIndex) return;

        var oldIndex = oldIndexOverride == -2 ? _selectedIndex : oldIndexOverride;
        var oldPage = oldPageOverride ?? (oldIndex >= 0 && oldIndex < _tabPages.Count ? _tabPages[oldIndex] : null);
        var newPage = index >= 0 && index < _tabPages.Count ? _tabPages[index] : null;

        if (oldPage != null)
        {
            var args = new TabControlCancelEventArgs(oldPage, oldIndex, TabControlAction.Deselecting);
            OnDeselecting(args);
            if (args.Cancel) return;
        }

        if (newPage != null)
        {
            var args = new TabControlCancelEventArgs(newPage, index, TabControlAction.Selecting);
            OnSelecting(args);
            if (args.Cancel) return;
        }

        if (oldPage != null) oldPage.Visible = false;

        _selectedIndex = index;

        if (newPage != null)
        {
            // Ensure it is placed correctly.
            newPage.Visible = true;
        }

        PerformLayout();

        if (oldPage != null)
        {
            OnDeselected(new TabControlEventArgs(oldPage, oldIndex, TabControlAction.Deselected));
        }

        if (newPage != null)
        {
            OnSelected(new TabControlEventArgs(newPage, index, TabControlAction.Selected));
        }

        OnSelectedIndexChanged(EventArgs.Empty);

        // When switching tabs while TabControl has focus, try to focus first focusable control in the page.
        if (newPage != null && (Focused || ContainsFocus))
        {
            if (!FocusFirstControl(newPage))
            {
                Focus();
            }
        }
    }

    private bool FocusFirstControl(Control container)
    {
        var ordered = container.Controls
            .Where(c => c.Visible)
            .OrderBy(c => c.TabIndex)
            .ThenBy(c => container.Controls.IndexOf(c))
            .ToList();

        foreach (var c in ordered)
        {
            if (c.CanFocus)
            {
                return c.Focus();
            }

            if (c.HasChildren)
            {
                if (FocusFirstControl(c))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private Rectangle GetPageBounds()
    {
        var header = GetHeaderBounds();

        const int inset = 1;

        return Alignment switch
        {
            TabAlignment.Top => new Rectangle(inset, header.Bottom, Math.Max(0, Width - (inset * 2)), Math.Max(0, Height - header.Bottom - inset)),
            TabAlignment.Bottom => new Rectangle(inset, inset, Math.Max(0, Width - (inset * 2)), Math.Max(0, header.Y - inset)),
            TabAlignment.Left => new Rectangle(header.Right, inset, Math.Max(0, Width - header.Right - inset), Math.Max(0, Height - (inset * 2))),
            TabAlignment.Right => new Rectangle(inset, inset, Math.Max(0, header.X - inset), Math.Max(0, Height - (inset * 2))),
            _ => new Rectangle(inset, header.Bottom, Math.Max(0, Width - (inset * 2)), Math.Max(0, Height - header.Bottom - inset))
        };
    }

    private int GetHeaderHeight()
    {
        if (ItemSizeHeight > 0) return ItemSizeHeight;
        return DefaultHeaderHeight;
    }

    private int GetHeaderThickness()
    {
        // For left/right alignment, thickness is analogous to height on top/bottom.
        return GetHeaderHeight();
    }

    private Rectangle GetHeaderBounds()
    {
        var headerHeight = GetHeaderHeight();
        var headerThickness = GetHeaderThickness();

        if (Multiline && _tabPages.Count > 0)
        {
            if (Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom)
            {
                headerHeight *= GetMultilineRowCount();
            }
            else
            {
                // Left/Right: multiline wraps into multiple columns.
                headerThickness *= GetMultilineColumnCount();
            }
        }

        return Alignment switch
        {
            TabAlignment.Top => new Rectangle(0, 0, Width, headerHeight),
            TabAlignment.Bottom => new Rectangle(0, Math.Max(0, Height - headerHeight), Width, headerHeight),
            TabAlignment.Left => new Rectangle(0, 0, headerThickness, Height),
            TabAlignment.Right => new Rectangle(Math.Max(0, Width - headerThickness), 0, headerThickness, Height),
            _ => new Rectangle(0, 0, Width, headerHeight)
        };
    }

    private void BuildHeaderRects()
    {
        _tabHeaderRects.Clear();

        var header = GetHeaderBounds();
        var headerHeight = GetHeaderHeight();
        var headerThickness = GetHeaderThickness();

        // Multiline wrap computations (simple row/col packing).
        var x = header.X + 2;
        var y = header.Y + 1;
        var row = 0;
        var col = 0;

        for (var i = 0; i < _tabPages.Count; i++)
        {
            var w = GetTabHeaderWidth(i);

            if (Alignment == TabAlignment.Top || Alignment == TabAlignment.Bottom)
            {
                if (!Multiline)
                {
                    // Single row, apply scroll offset.
                    if (i < _firstVisibleTab)
                    {
                        _tabHeaderRects.Add(new Rectangle(-10000, -10000, w, headerHeight - 1));
                        continue;
                    }
                }
                else
                {
                    // Wrap to next row.
                    if (x + w > header.Right - 2 && x > header.X + 2)
                    {
                        row++;
                        x = header.X + 2;
                        y = header.Y + 1 + (row * headerHeight);
                    }
                }

                // Bottom aligns header at bottom area.
                if (Alignment == TabAlignment.Bottom)
                {
                    // header.Y already set to bottom.
                }

                _tabHeaderRects.Add(new Rectangle(x, y, w, headerHeight - 1));
                x += w;
            }
            else
            {
                // Left/Right: stack vertically, wrap into columns when Multiline.
                if (Multiline)
                {
                    if (y + headerHeight > header.Bottom - 1 && y > header.Y + 1)
                    {
                        col++;
                        y = header.Y + 1;
                        x = header.X + 1 + (col * headerThickness);
                    }
                }

                var tabRect = new Rectangle(x, y, Math.Max(0, headerThickness - 2), headerHeight - 1);
                _tabHeaderRects.Add(tabRect);
                y += headerHeight;
            }
        }

        if (_tabHeaderRects.Count > 0)
        {
            _lastVisibleTab = _tabHeaderRects.Count - 1;
        }
    }

    private int HitTestTabHeader(int x, int y)
    {
        var header = GetHeaderBounds();
        if (x < header.X || x >= header.Right || y < header.Y || y >= header.Bottom) return -1;

        if (_tabHeaderRects.Count != _tabPages.Count)
        {
            BuildHeaderRects();
        }

        for (var i = 0; i < _tabHeaderRects.Count; i++)
        {
            var r = _tabHeaderRects[i];
            if (x >= r.X && x < r.Right && y >= r.Y && y < r.Bottom)
            {
                return i;
            }
        }

        return -1;
    }

    private bool NeedsHeaderScroll()
    {
        if (_tabPages.Count == 0) return false;
        if (Multiline) return false;

        // Only implemented for Top/Bottom.
        if (Alignment != TabAlignment.Top && Alignment != TabAlignment.Bottom) return false;

        var header = GetHeaderBounds();
        var total = 2;
        for (var i = 0; i < _tabPages.Count; i++)
        {
            total += GetTabHeaderWidth(i);
        }

        // reserve some space for scroll buttons
        const int buttonSpace = 40;
        return total > header.Width - buttonSpace;
    }

    private int GetTabHeaderWidth(int index)
    {
        if (index < 0 || index >= _tabPages.Count) return DefaultTabMinWidth;

        if (ItemSizeWidth > 0) return ItemSizeWidth;

        var text = _tabPages[index].Text ?? string.Empty;
        var textWidth = MeasureTextWidth(text);
        return Math.Max(DefaultTabMinWidth, textWidth + DefaultTabPaddingX);
    }

    private int GetMultilineRowCount()
    {
        // Multiline for Top/Bottom: rows are based on wrapping by available width.
        var available = Math.Max(0, Width - 4);
        if (available == 0) return 1;

        var rows = 1;
        var x = 2;

        for (var i = 0; i < _tabPages.Count; i++)
        {
            var w = GetTabHeaderWidth(i);

            if (x > 2 && x + w > available)
            {
                rows++;
                x = 2;
            }

            x += w;
        }

        return Math.Max(1, rows);
    }

    private int GetMultilineColumnCount()
    {
        // Multiline for Left/Right: columns are based on wrapping by available height.
        var tabHeight = GetHeaderHeight();
        var available = Math.Max(0, Height - 2);
        if (available == 0 || tabHeight <= 0) return 1;

        var cols = 1;
        var y = 1;

        for (var i = 0; i < _tabPages.Count; i++)
        {
            if (y > 1 && y + tabHeight > available)
            {
                cols++;
                y = 1;
            }

            y += tabHeight;
        }

        return Math.Max(1, cols);
    }

    private sealed class TabControlControlCollection : ControlCollection
    {
        private readonly TabControl _owner;

        public TabControlControlCollection(TabControl owner) : base(owner, owner._controls)
        {
            _owner = owner;
        }

        public override void Add(Control control)
        {
            if (control is not TabPage page)
            {
                throw new ArgumentException("Only TabPage controls can be added to a TabControl.", nameof(control));
            }

            if (_owner._tabPages.Contains(page))
            {
                // Ensure it exists in the visual tree.
                if (!_owner._controls.Contains(page))
                {
                    base.Add(page);
                    page.Visible = false;
                    _owner.PerformLayout();
                }
                return;
            }

            page.TabStop = false;

            _owner._tabPages._list.Add(page);
            _owner.AddTabPage(page);
        }

        public override void Remove(Control control)
        {
            if (control is not TabPage page)
            {
                base.Remove(control);
                return;
            }

            var idx = _owner._tabPages.IndexOf(page);
            if (idx >= 0)
            {
                _owner.RemoveTabPageAt(idx);
                return;
            }

            base.Remove(control);
        }

        public override void Clear()
        {
            while (_owner._tabPages.Count > 0)
            {
                _owner.RemoveTabPageAt(0);
            }
        }
    }

    private Rectangle GetScrollLeftRect()
    {
        var header = GetHeaderBounds();
        return new Rectangle(header.Right - 38, header.Y + 4, 16, Math.Max(0, header.Height - 8));
    }

    private Rectangle GetScrollRightRect()
    {
        var header = GetHeaderBounds();
        return new Rectangle(header.Right - 20, header.Y + 4, 16, Math.Max(0, header.Height - 8));
    }

    private bool HitTestScrollLeft(int x, int y)
    {
        var r = GetScrollLeftRect();
        return x >= r.X && x < r.Right && y >= r.Y && y < r.Bottom;
    }

    private bool HitTestScrollRight(int x, int y)
    {
        var r = GetScrollRightRect();
        return x >= r.X && x < r.Right && y >= r.Y && y < r.Bottom;
    }

    private void PaintHeaderScrollButtons(Graphics g)
    {
        var left = GetScrollLeftRect();
        var right = GetScrollRightRect();

        using var bg = new SolidBrush(CanvasColor.FromArgb(240, 240, 240));
        g.FillRectangle(bg, left);
        g.FillRectangle(bg, right);

        using var pen = new Pen(CanvasColor.FromArgb(200, 200, 200));
        g.DrawRectangle(pen, left);
        g.DrawRectangle(pen, right);

        // simple chevrons
        using var arrow = new Pen(CanvasColor.FromArgb(80, 80, 80));
        g.DrawLine(arrow, left.X + 10, left.Y + 6, left.X + 6, left.Y + left.Height / 2);
        g.DrawLine(arrow, left.X + 6, left.Y + left.Height / 2, left.X + 10, left.Y + left.Height - 6);

        g.DrawLine(arrow, right.X + 6, right.Y + 6, right.X + 10, right.Y + right.Height / 2);
        g.DrawLine(arrow, right.X + 10, right.Y + right.Height / 2, right.X + 6, right.Y + right.Height - 6);
    }

    private void ScrollTabs(int delta)
    {
        if (_tabPages.Count == 0) return;

        _firstVisibleTab = Math.Max(0, Math.Min(_tabPages.Count - 1, _firstVisibleTab + delta));

        // Keep selected tab reachable.
        if (_selectedIndex >= 0 && _selectedIndex < _firstVisibleTab)
        {
            _firstVisibleTab = _selectedIndex;
        }

        BuildHeaderRects();
        Invalidate();
    }

    private int MeasureTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var measureService = FindForm()?.TextMeasurementService;
        if (measureService != null)
        {
            return measureService.MeasureTextEstimate(text, Font.Family, (int)Font.Size);
        }

        return (int)Math.Ceiling(text.Length * Font.Size * 0.55f);
    }

    public class TabPageCollection : IEnumerable<TabPage>
    {
        private readonly TabControl _owner;
        internal readonly List<TabPage> _list = new();

        internal TabPageCollection(TabControl owner) => _owner = owner;

        public int Count => _list.Count;

        public TabPage this[int index] => _list[index];

        public TabPage Add(string text)
        {
            var page = new TabPage { Text = text };
            Add(page);
            return page;
        }

        public void Add(TabPage page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            if (_list.Contains(page)) return;

            page.TabStop = false;

            _list.Add(page);
            _owner.AddTabPage(page);
        }

        public void Insert(int index, TabPage page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            if (index < 0 || index > _list.Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (_list.Contains(page)) return;

            page.TabStop = false;

            _list.Insert(index, page);

            // Keep the same selected tab when inserting before it.
            if (_owner._selectedIndex >= 0 && index <= _owner._selectedIndex)
            {
                _owner._selectedIndex++;
            }

            _owner.AddTabPage(page);
        }

        public void AddRange(TabPage[] pages)
        {
            foreach (var p in pages)
            {
                Add(p);
            }
        }

        public void Remove(TabPage page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));
            var idx = _list.IndexOf(page);
            if (idx < 0) return;

            _owner.RemoveTabPageAt(idx);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= _list.Count) throw new ArgumentOutOfRangeException(nameof(index));
            _owner.RemoveTabPageAt(index);
        }

        public void Clear()
        {
            while (_list.Count > 0)
            {
                _owner.RemoveTabPageAt(0);
            }
        }

        public bool Contains(TabPage page) => _list.Contains(page);

        public int IndexOf(TabPage page) => _list.IndexOf(page);

        public TabPage? this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key)) return null;
                return _list.FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
            }
        }

        public int IndexOfKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return -1;

            for (var i = 0; i < _list.Count; i++)
            {
                if (string.Equals(_list[i].Name, key, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool ContainsKey(string key) => IndexOfKey(key) >= 0;

        public void RemoveByKey(string key)
        {
            var idx = IndexOfKey(key);
            if (idx >= 0)
            {
                _owner.RemoveTabPageAt(idx);
            }
        }

        public IEnumerator<TabPage> GetEnumerator() => _list.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

public class TabPage : Panel
{
    public TabPage()
    {
        TabStop = false;
        BackColor = CanvasColor.White;
    }

    public TabPage(string text) : this()
    {
        Text = text;
    }

    public bool UseVisualStyleBackColor { get; set; } = true;
}

public delegate void TabControlEventHandler(object? sender, TabControlEventArgs e);
public delegate void TabControlCancelEventHandler(object? sender, TabControlCancelEventArgs e);

public class TabControlEventArgs : EventArgs
{
    public TabControlEventArgs(TabPage? tabPage, int tabPageIndex, TabControlAction action)
    {
        TabPage = tabPage;
        TabPageIndex = tabPageIndex;
        Action = action;
    }

    public TabPage? TabPage { get; }
    public int TabPageIndex { get; }
    public TabControlAction Action { get; }
}

public class TabControlCancelEventArgs : CancelEventArgs
{
    public TabControlCancelEventArgs(TabPage? tabPage, int tabPageIndex, TabControlAction action)
    {
        TabPage = tabPage;
        TabPageIndex = tabPageIndex;
        Action = action;
    }

    public TabPage? TabPage { get; }
    public int TabPageIndex { get; }
    public TabControlAction Action { get; }
}

public enum TabControlAction
{
    Selecting = 0,
    Selected = 1,
    Deselecting = 2,
    Deselected = 3
}

public enum TabAlignment
{
    Top = 0,
    Bottom = 1,
    Left = 2,
    Right = 3
}

public enum TabAppearance
{
    Normal = 0,
    Buttons = 1,
    FlatButtons = 2
}

public enum TabDrawMode
{
    Normal = 0,
    OwnerDrawFixed = 1
}

public enum TabSizeMode
{
    Normal = 0,
    FillToRight = 1,
    Fixed = 2
}
