
namespace System.Windows.Forms;

/// <summary>
/// Represents a node in a TreeView control
/// </summary>
public class TreeNode
{
    private string _text = string.Empty;
    private bool _isExpanded = false;
    private TreeNodeCollection? _nodes;
    private TreeView? _treeView;

    public TreeNode() { }
    public TreeNode(string text) { _text = text; }
    public TreeNode(string text, TreeNode[] children) { _text = text; foreach (var c in children) Nodes.Add(c); }

    public string Text { get => _text; set { _text = value; _treeView?.Invalidate(); } }
    public string Name { get; set; } = string.Empty;
    public object? Tag { get; set; }
    public int ImageIndex { get; set; } = -1;
    public int SelectedImageIndex { get; set; } = -1;
    public Color ForeColor { get; set; } = Color.Transparent;
    public Color BackColor { get; set; } = Color.Transparent;
    public bool Checked { get; set; } = false;

    public TreeNode? Parent { get; internal set; }
    public TreeView? TreeView { get => _treeView; internal set { _treeView = value; foreach (var n in Nodes) n.TreeView = value; } }

    public bool IsExpanded => _isExpanded;
    public bool IsSelected => _treeView?.SelectedNode == this;
    public int Level => Parent == null ? 0 : Parent.Level + 1;

    public TreeNodeCollection Nodes => _nodes ??= new TreeNodeCollection(this);

    public bool HasChildren => _nodes != null && _nodes.Count > 0;

    public string FullPath
    {
        get
        {
            var parts = new List<string> { Text };
            var node = Parent;
            while (node != null) { parts.Insert(0, node.Text); node = node.Parent; }
            return string.Join("\\", parts);
        }
    }

    public void Expand() { _isExpanded = true; _treeView?.Invalidate(); }
    public void Collapse() { _isExpanded = false; _treeView?.Invalidate(); }
    public void Toggle() { _isExpanded = !_isExpanded; _treeView?.Invalidate(); }
    public void ExpandAll() { Expand(); foreach (var n in Nodes) n.ExpandAll(); }
    public void CollapseAll() { Collapse(); foreach (var n in Nodes) n.CollapseAll(); }
    public void Remove() { Parent?.Nodes.Remove(this); _treeView?.Nodes.Remove(this); }
}

/// <summary>
/// Collection of TreeNode objects
/// </summary>
public class TreeNodeCollection : IEnumerable<TreeNode>
{
    private readonly List<TreeNode> _list = new();
    private readonly TreeNode? _owner;
    private readonly TreeView? _treeOwner;

    internal TreeNodeCollection(TreeNode owner) => _owner = owner;
    internal TreeNodeCollection(TreeView owner) => _treeOwner = owner;

    public int Count => _list.Count;
    public TreeNode this[int index] => _list[index];
    public TreeNode? this[string key] => _list.FirstOrDefault(n => n.Name == key);

    public TreeNode Add(string text)
    {
        var node = new TreeNode(text);
        Add(node);
        return node;
    }

    public void Add(TreeNode node)
    {
        node.Parent = _owner;
        node.TreeView = _treeOwner ?? _owner?.TreeView;
        _list.Add(node);
        node.TreeView?.Invalidate();
    }

    public void Remove(TreeNode node)
    {
        if (_list.Remove(node))
        {
            node.Parent = null;
            node.TreeView?.Invalidate();
        }
    }

    public void Clear()
    {
        foreach (var n in _list) { n.Parent = null; }
        _list.Clear();
        (_treeOwner ?? _owner?.TreeView)?.Invalidate();
    }

    public bool Contains(TreeNode node) => _list.Contains(node);
    public IEnumerator<TreeNode> GetEnumerator() => _list.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Represents a Windows Forms TreeView control
/// </summary>
public class TreeView : Control
{
    private const int Indent = 19;
    private const int ItemHeight = 20;
    private const int ExpandBoxSize = 9;
    private const int ExpandBoxOffset = 4;
    private const int LeftMargin = 2;

    private TreeNodeCollection? _nodes;
    private TreeNode? _selectedNode;
    private int _scrollOffset = 0;

    public event TreeViewEventHandler? AfterSelect;
    public event TreeViewEventHandler? BeforeSelect;
    public event TreeViewEventHandler? AfterExpand;
    public event TreeViewEventHandler? AfterCollapse;
    public event TreeViewCancelEventHandler? BeforeExpand;
    public event TreeViewCancelEventHandler? BeforeCollapse;
    public event TreeNodeMouseClickEventHandler? NodeMouseClick;
    public event TreeNodeMouseClickEventHandler? NodeMouseDoubleClick;

    public TreeView()
    {
        Width = 121;
        Height = 97;
        BackColor = Color.White;
        ForeColor = Color.Black;
        TabStop = true;
        SetStyle(ControlStyles.Selectable | ControlStyles.UserPaint, true);
    }

    public TreeNodeCollection Nodes => _nodes ??= new TreeNodeCollection(this);
    public TreeNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (_selectedNode == value) return;
            var old = _selectedNode;
            _selectedNode = value;
            Invalidate();
            AfterSelect?.Invoke(this, new TreeViewEventArgs(_selectedNode, TreeViewAction.Unknown));
        }
    }

    public bool ShowLines { get; set; } = true;
    public bool ShowPlusMinus { get; set; } = true;
    public bool ShowRootLines { get; set; } = true;
    public bool FullRowSelect { get; set; } = false;
    public bool HideSelection { get; set; } = true;
    public bool CheckBoxes { get; set; } = false;
    public int Indent_ { get; set; } = Indent;
    public BorderStyle BorderStyle { get; set; } = BorderStyle.Fixed3D;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, 0, 0, Width, Height);
        using var borderPen = new Pen(Color.FromArgb(122, 122, 122));
        if (BorderStyle != BorderStyle.None)
            g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        int y = 2 - _scrollOffset;
        foreach (var node in Nodes)
            y = DrawNode(g, node, 0, y);

        base.OnPaint(e);
    }

    private int DrawNode(Graphics g, TreeNode node, int depth, int y)
    {
        bool belowView = y > Height;
        bool inView = !belowView && y + ItemHeight >= 0;

        if (inView)
        {
            int x = LeftMargin + 2 + depth * Indent_;
            bool isSelected = node == _selectedNode;

            // Calculate where content starts (expand box or text)
            int contentStartX = node.HasChildren && ShowPlusMinus 
                ? Math.Max(LeftMargin, x - ExpandBoxSize - ExpandBoxOffset) 
                : x;

            // Selection background - start from content area, not just text
            if (isSelected && (!HideSelection || Focused))
            {
                int selX = FullRowSelect ? 0 : contentStartX;
                int selW = FullRowSelect ? Width : Width - selX - 2;
                using var selBrush = new SolidBrush(Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(204, 228, 247));
                g.FillRectangle(selBrush, selX, y, selW, ItemHeight);
            }

            // Expand/collapse box
            int bx = -1;
            if (node.HasChildren && ShowPlusMinus)
            {
                bx = Math.Max(LeftMargin, x - ExpandBoxSize - ExpandBoxOffset);
                int by = y + (ItemHeight - ExpandBoxSize) / 2;
                using var boxPen = new Pen(Color.FromArgb(128, 128, 128));
                g.DrawRectangle(boxPen, bx, by, ExpandBoxSize, ExpandBoxSize);
                int mx = bx + ExpandBoxSize / 2;
                int my = by + ExpandBoxSize / 2;
                using var signPen = new Pen(Color.FromArgb(80, 80, 80));
                g.DrawLine(signPen, bx + 2, my, bx + ExpandBoxSize - 2, my);
                if (!node.IsExpanded)
                    g.DrawLine(signPen, mx, by + 2, mx, by + ExpandBoxSize - 2);
            }

            // Checkbox
            int textX;
            if (node.HasChildren && ShowPlusMinus)
                textX = bx + ExpandBoxSize + 2;
            else
                textX = x;

            if (CheckBoxes)
            {
                int cbx = textX;
                int cby = y + (ItemHeight - 13) / 2;
                using var cbPen = new Pen(Color.FromArgb(122, 122, 122));
                g.DrawRectangle(cbPen, cbx, cby, 13, 13);
                if (node.Checked)
                {
                    using var checkPen = new Pen(Color.FromArgb(0, 120, 215), 2);
                    g.DrawLine(checkPen, cbx + 2, cby + 7, cbx + 5, cby + 10);
                    g.DrawLine(checkPen, cbx + 5, cby + 10, cbx + 11, cby + 2);
                }
                textX += 17;
            }

            // Node text
            var textColor = !node.ForeColor.Equals(Color.Transparent) ? node.ForeColor :
                            (isSelected && Focused ? Color.White : ForeColor);
            using var textBrush = new SolidBrush(textColor);
            g.DrawString(node.Text, "Arial", 12, textBrush, textX, y + 3);
        }

        y += ItemHeight;

        if (node.IsExpanded)
            foreach (var child in node.Nodes)
                y = DrawNode(g, child, depth + 1, y);

        return y;
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        var (node, onExpander) = HitTest(e.X, e.Y);
        if (node != null)
        {
            if (onExpander && node.HasChildren)
            {
                node.Toggle();
                var args = new TreeViewEventArgs(node, TreeViewAction.Collapse);
                if (node.IsExpanded) AfterExpand?.Invoke(this, args);
                else AfterCollapse?.Invoke(this, args);
            }
            else
            {
                SelectedNode = node;
                NodeMouseClick?.Invoke(this, new TreeNodeMouseClickEventArgs(node, e.Button, e.Clicks, e.X, e.Y));
            }
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (_selectedNode == null) { base.OnKeyDown(e); return; }
        switch (e.KeyCode)
        {
            case Keys.Up:
            {
                var prev = GetPreviousVisible(_selectedNode);
                if (prev != null) SelectedNode = prev;
                e.Handled = true; return;
            }
            case Keys.Down:
            {
                var next = GetNextVisible(_selectedNode);
                if (next != null) SelectedNode = next;
                e.Handled = true; return;
            }
            case Keys.Left:
                if (_selectedNode.IsExpanded) _selectedNode.Collapse();
                else if (_selectedNode.Parent != null) SelectedNode = _selectedNode.Parent;
                e.Handled = true; return;
            case Keys.Right:
                if (_selectedNode.HasChildren && !_selectedNode.IsExpanded) _selectedNode.Expand();
                e.Handled = true; return;
        }
        base.OnKeyDown(e);
    }

    private (TreeNode? node, bool onExpander) HitTest(int x, int y)
    {
        var allVisible = new List<(TreeNode node, int depth, int y)>();
        CollectVisible(Nodes, 0, 2 - _scrollOffset, allVisible);
        foreach (var (node, depth, ny) in allVisible)
        {
            if (y >= ny && y < ny + ItemHeight)
            {
                int nx = LeftMargin + 2 + depth * Indent_;
                int bx = Math.Max(LeftMargin, nx - ExpandBoxSize - ExpandBoxOffset);
                bool onExp = x >= bx && x <= bx + ExpandBoxSize && node.HasChildren;
                return (node, onExp);
            }
        }
        return (null, false);
    }

    private int CollectVisible(IEnumerable<TreeNode> nodes, int depth, int y, List<(TreeNode, int, int)> list)
    {
        foreach (var node in nodes)
        {
            list.Add((node, depth, y));
            y += ItemHeight;
            if (node.IsExpanded)
                y = CollectVisible(node.Nodes, depth + 1, y, list);
        }
        return y;
    }

    private TreeNode? GetPreviousVisible(TreeNode node)
    {
        var all = new List<TreeNode>();
        CollectAllVisible(Nodes, all);
        int i = all.IndexOf(node);
        return i > 0 ? all[i - 1] : null;
    }

    private TreeNode? GetNextVisible(TreeNode node)
    {
        var all = new List<TreeNode>();
        CollectAllVisible(Nodes, all);
        int i = all.IndexOf(node);
        return i >= 0 && i < all.Count - 1 ? all[i + 1] : null;
    }

    private void CollectAllVisible(IEnumerable<TreeNode> nodes, List<TreeNode> result)
    {
        foreach (var node in nodes)
        {
            result.Add(node);
            if (node.IsExpanded) CollectAllVisible(node.Nodes, result);
        }
    }
}

public delegate void TreeViewEventHandler(object? sender, TreeViewEventArgs e);
public delegate void TreeViewCancelEventHandler(object? sender, TreeViewCancelEventArgs e);
public delegate void TreeNodeMouseClickEventHandler(object? sender, TreeNodeMouseClickEventArgs e);

public class TreeViewEventArgs : EventArgs
{
    public TreeNode? Node { get; }
    public TreeViewAction Action { get; }
    public TreeViewEventArgs(TreeNode? node, TreeViewAction action) { Node = node; Action = action; }
}

public class TreeViewCancelEventArgs : TreeViewEventArgs
{
    public bool Cancel { get; set; }
    public TreeViewCancelEventArgs(TreeNode? node, bool cancel, TreeViewAction action) : base(node, action) { Cancel = cancel; }
}

public class TreeNodeMouseClickEventArgs : MouseEventArgs
{
    public TreeNode Node { get; }
    public TreeNodeMouseClickEventArgs(TreeNode node, MouseButtons button, int clicks, int x, int y)
        : base(button, clicks, x, y) { Node = node; }
}

public enum TreeViewAction { Unknown, ByKeyboard, ByMouse, Collapse, Expand }
