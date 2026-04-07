using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms list box control with check boxes next to each item
/// </summary>
public class CheckedListBox : ListControl
{
    private const int ItemPadding = 2;
    private const int CheckBoxSize = 13;
    private const int CheckBoxMargin = 4;

    private readonly HashSet<int> _checkedIndices = new();
    private readonly Dictionary<int, CheckState> _itemCheckStates = new();
    private bool _checkOnClick = false;
    private bool _threeState = false;
    private int _mouseDownIndex = -1;

    public CheckedListBox()
    {
        Width = 120;
        Height = 96;
        BackColor = Color.White;
        ForeColor = Color.Black;
        BorderStyle = BorderStyle.Fixed3D;
    }

    #region Properties

    /// <summary>
    /// Gets or sets the border style
    /// </summary>
    public BorderStyle BorderStyle { get; set; }

    /// <summary>
    /// Override border width based on BorderStyle
    /// </summary>
    protected override int BorderWidth => BorderStyle == BorderStyle.None ? 0 : 2;

    /// <summary>
    /// Gets or sets whether clicking an item checks/unchecks it
    /// </summary>
    public bool CheckOnClick
    {
        get => _checkOnClick;
        set => _checkOnClick = value;
    }

    /// <summary>
    /// Gets the collection of checked indices
    /// </summary>
    public CheckedIndexCollection CheckedIndices => new CheckedIndexCollection(_checkedIndices);

    /// <summary>
    /// Gets the collection of checked items
    /// </summary>
    public CheckedItemCollection CheckedItems => new CheckedItemCollection(this, _checkedIndices);

    /// <summary>
    /// Gets or sets whether items can have three states (unchecked, checked, indeterminate)
    /// </summary>
    public bool ThreeState
    {
        get => _threeState;
        set => _threeState = value;
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when the checked state of an item changes
    /// </summary>
    public event ItemCheckEventHandler? ItemCheck;

    #endregion

    #region Methods

    /// <summary>
    /// Gets the checked state of an item
    /// </summary>
    public CheckState GetItemCheckState(int index)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _itemCheckStates.TryGetValue(index, out var state) ? state : CheckState.Unchecked;
    }

    /// <summary>
    /// Sets the checked state of an item
    /// </summary>
    public void SetItemCheckState(int index, CheckState value)
    {
        if (index < 0 || index >= Items.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (!_threeState && value == CheckState.Indeterminate)
            value = CheckState.Checked;

        var oldState = GetItemCheckState(index);
        if (oldState != value)
        {
            // Raise ItemCheck event
            var args = new ItemCheckEventArgs(index, value, oldState);
            OnItemCheck(args);

            // If not cancelled, apply the change
            if (args.NewValue != oldState)
            {
                _itemCheckStates[index] = args.NewValue;

                if (args.NewValue == CheckState.Checked)
                    _checkedIndices.Add(index);
                else
                    _checkedIndices.Remove(index);

                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets whether an item is checked
    /// </summary>
    public bool GetItemChecked(int index)
    {
        return GetItemCheckState(index) == CheckState.Checked;
    }

    /// <summary>
    /// Sets whether an item is checked
    /// </summary>
    public void SetItemChecked(int index, bool value)
    {
        SetItemCheckState(index, value ? CheckState.Checked : CheckState.Unchecked);
    }

    /// <summary>
    /// Raises the ItemCheck event
    /// </summary>
    protected virtual void OnItemCheck(ItemCheckEventArgs e)
    {
        ItemCheck?.Invoke(this, e);
    }

    #endregion

    #region Painting

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);

        // Draw background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, bounds);

        // Draw border
        if (BorderStyle != BorderStyle.None)
        {
            var borderColor = Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(122, 122, 122);
            using var borderPen = new Pen(borderColor, BorderWidth);
            g.DrawRectangle(borderPen, bounds);
        }

        // Calculate visible area
        var contentBounds = new Rectangle(
            BorderWidth,
            BorderWidth,
            Width - BorderWidth * 2,
            Height - BorderWidth * 2
        );

        var needsScrollbar = NeedsScrollbar();
        var contentWidth = contentBounds.Width - (needsScrollbar ? ScrollbarWidth : 0);

        // Calculate visible items
        var itemsPerPage = (contentBounds.Height - 4) / ItemHeight;
        var visibleCount = Math.Min(itemsPerPage, Items.Count - _topIndex);

        // Draw items
        for (int i = 0; i < visibleCount; i++)
        {
            var itemIndex = _topIndex + i;
            if (itemIndex >= Items.Count) break;

            var itemBounds = new Rectangle(
                contentBounds.X + 2,
                contentBounds.Y + 2 + (i * ItemHeight),
                contentWidth - 4,
                ItemHeight
            );

            DrawItem(g, itemIndex, itemBounds);
        }

        // Draw scrollbar if needed
        if (needsScrollbar)
        {
            DrawScrollbar(g);
        }

        base.OnPaint(e);
    }

    private void DrawItem(Graphics g, int index, Rectangle bounds)
    {
        var item = Items[index];
        var isSelected = index == _selectedIndex;
        var isHovered = index == _hoveredIndex;

        // Background
        Color bgColor;
        if (isSelected)
        {
            bgColor = Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(217, 217, 217);
        }
        else if (isHovered && Enabled)
        {
            bgColor = Color.FromArgb(229, 243, 255);
        }
        else
        {
            bgColor = BackColor;
        }

        using var itemBgBrush = new SolidBrush(bgColor);
        g.FillRectangle(itemBgBrush, bounds);

        // Draw checkbox
        var checkBoxBounds = new Rectangle(
            bounds.X + CheckBoxMargin,
            bounds.Y + (bounds.Height - CheckBoxSize) / 2,
            CheckBoxSize,
            CheckBoxSize
        );

        DrawCheckBox(g, checkBoxBounds, GetItemCheckState(index), Enabled);

        // Draw item text
        var textX = checkBoxBounds.Right + CheckBoxMargin;
        var textBounds = new Rectangle(
            textX,
            bounds.Y,
            bounds.Right - textX,
            bounds.Height
        );

        var text = GetItemText(item);
        var textColor = Enabled 
            ? (isSelected && Focused ? Color.White : ForeColor)
            : Color.FromArgb(109, 109, 109);

        g.DrawString(text, textBounds.X + ItemPadding, textBounds.Y + ItemPadding, textColor);
    }

    private void DrawCheckBox(Graphics g, Rectangle bounds, CheckState state, bool enabled)
    {
        // Background
        var bgColor = enabled ? Color.White : Color.FromArgb(240, 240, 240);
        using var bgBrush = new SolidBrush(bgColor);
        g.FillRectangle(bgBrush, bounds);

        // Border
        var borderColor = enabled ? Color.FromArgb(122, 122, 122) : Color.FromArgb(200, 200, 200);
        using var borderPen = new Pen(borderColor);
        g.DrawRectangle(borderPen, bounds);

        // Check mark or indeterminate state
        if (state == CheckState.Checked)
        {
            // Draw check mark
            var checkColor = enabled ? Color.FromArgb(0, 120, 215) : Color.FromArgb(150, 150, 150);
            using var checkPen = new Pen(checkColor, 2);

            var x1 = bounds.X + 3;
            var y1 = bounds.Y + bounds.Height / 2;
            var x2 = bounds.X + bounds.Width / 2 - 1;
            var y2 = bounds.Bottom - 3;
            var x3 = bounds.Right - 3;
            var y3 = bounds.Y + 3;

            g.DrawLine(checkPen, x1, y1, x2, y2);
            g.DrawLine(checkPen, x2, y2, x3, y3);
        }
        else if (state == CheckState.Indeterminate)
        {
            // Draw filled square for indeterminate
            var indeterminateColor = enabled ? Color.FromArgb(0, 120, 215) : Color.FromArgb(150, 150, 150);
            using var indeterminateBrush = new SolidBrush(indeterminateColor);
            var innerBounds = new Rectangle(
                bounds.X + 3,
                bounds.Y + 3,
                bounds.Width - 6,
                bounds.Height - 6
            );
            g.FillRectangle(indeterminateBrush, innerBounds);
        }
    }

    #endregion

    #region Mouse Handling

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseDown(e);
            return;
        }

        Focus();

        var contentBounds = new Rectangle(BorderWidth, BorderWidth, Width - BorderWidth * 2, Height - BorderWidth * 2);
        var needsScrollbar = NeedsScrollbar();

        // Check if clicking on scrollbar
        if (needsScrollbar && e.X >= Width - ScrollbarWidth - BorderWidth)
        {
            HandleScrollbarMouseDown(e);
            base.OnMouseDown(e);
            return;
        }

        // Check if clicking on an item
        if (e.X >= contentBounds.X && e.X < contentBounds.Right - (needsScrollbar ? ScrollbarWidth : 0) &&
            e.Y >= contentBounds.Y && e.Y < contentBounds.Bottom)
        {
            var itemIndex = _topIndex + ((e.Y - contentBounds.Y - 2) / ItemHeight);

            if (itemIndex >= 0 && itemIndex < Items.Count)
            {
                _mouseDownIndex = itemIndex;

                // Check if clicking on checkbox area
                var checkBoxRight = contentBounds.X + 2 + CheckBoxMargin + CheckBoxSize + CheckBoxMargin;
                var isCheckBoxClick = e.X < checkBoxRight;

                if (_checkOnClick || isCheckBoxClick)
                {
                    // Toggle check state
                    var currentState = GetItemCheckState(itemIndex);
                    var newState = currentState switch
                    {
                        CheckState.Unchecked => CheckState.Checked,
                        CheckState.Checked => _threeState ? CheckState.Indeterminate : CheckState.Unchecked,
                        CheckState.Indeterminate => CheckState.Unchecked,
                        _ => CheckState.Unchecked
                    };
                    SetItemCheckState(itemIndex, newState);
                }

                // Update selection
                if (SelectedIndex != itemIndex)
                {
                    SelectedIndex = itemIndex;
                }

                Invalidate();
            }
        }

        base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        _isDraggingScrollbar = false;
        _mouseDownIndex = -1;
        base.OnMouseUp(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseMove(e);
            return;
        }

        // Handle scrollbar dragging
        if (_isDraggingScrollbar)
        {
            HandleScrollbarMouseMove(e);
            base.OnMouseMove(e);
            return;
        }

        // Update hover state
        var contentBounds = new Rectangle(BorderWidth, BorderWidth, Width - BorderWidth * 2, Height - BorderWidth * 2);
        var needsScrollbar = NeedsScrollbar();

        if (e.X >= contentBounds.X && e.X < contentBounds.Right - (needsScrollbar ? ScrollbarWidth : 0) &&
            e.Y >= contentBounds.Y && e.Y < contentBounds.Bottom)
        {
            var itemIndex = _topIndex + ((e.Y - contentBounds.Y - 2) / ItemHeight);

            if (itemIndex >= 0 && itemIndex < Items.Count)
            {
                if (_hoveredIndex != itemIndex)
                {
                    _hoveredIndex = itemIndex;
                    Invalidate();
                }
            }
            else if (_hoveredIndex != -1)
            {
                _hoveredIndex = -1;
                Invalidate();
            }
        }
        else if (_hoveredIndex != -1)
        {
            _hoveredIndex = -1;
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

    #endregion

    #region Keyboard Handling

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (!Enabled)
        {
            base.OnKeyDown(e);
            return;
        }

        var handled = false;

        switch (e.KeyCode)
        {
            case Keys.Space:
                // Toggle check state of selected item
                if (_selectedIndex >= 0 && _selectedIndex < Items.Count)
                {
                    var currentState = GetItemCheckState(_selectedIndex);
                    var newState = currentState switch
                    {
                        CheckState.Unchecked => CheckState.Checked,
                        CheckState.Checked => _threeState ? CheckState.Indeterminate : CheckState.Unchecked,
                        CheckState.Indeterminate => CheckState.Unchecked,
                        _ => CheckState.Unchecked
                    };
                    SetItemCheckState(_selectedIndex, newState);
                    handled = true;
                }
                break;

            case Keys.Up:
                if (_selectedIndex > 0)
                {
                    SelectedIndex--;
                    EnsureVisible(_selectedIndex);
                    handled = true;
                }
                break;

            case Keys.Down:
                if (_selectedIndex < Items.Count - 1)
                {
                    SelectedIndex++;
                    EnsureVisible(_selectedIndex);
                    handled = true;
                }
                break;

            case Keys.Home:
                if (Items.Count > 0)
                {
                    SelectedIndex = 0;
                    EnsureVisible(0);
                    handled = true;
                }
                break;

            case Keys.End:
                if (Items.Count > 0)
                {
                    SelectedIndex = Items.Count - 1;
                    EnsureVisible(Items.Count - 1);
                    handled = true;
                }
                break;

            case Keys.PageUp:
                if (Items.Count > 0)
                {
                    var itemsPerPage = Math.Max(1, (Height - BorderWidth * 2 - 4) / ItemHeight);
                    SelectedIndex = Math.Max(0, _selectedIndex - itemsPerPage);
                    EnsureVisible(_selectedIndex);
                    handled = true;
                }
                break;

            case Keys.PageDown:
                if (Items.Count > 0)
                {
                    var itemsPerPage = Math.Max(1, (Height - BorderWidth * 2 - 4) / ItemHeight);
                    SelectedIndex = Math.Min(Items.Count - 1, _selectedIndex + itemsPerPage);
                    EnsureVisible(_selectedIndex);
                    handled = true;
                }
                break;
        }

        if (handled)
        {
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    #endregion
}

#region Collections

/// <summary>
/// Collection of checked indices in a CheckedListBox
/// </summary>
public class CheckedIndexCollection
{
    private readonly HashSet<int> _indices;

    internal CheckedIndexCollection(HashSet<int> indices)
    {
        _indices = indices;
    }

    public int Count => _indices.Count;

    public int this[int index]
    {
        get
        {
            if (index < 0 || index >= _indices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _indices.OrderBy(i => i).ElementAt(index);
        }
    }

    public bool Contains(int index) => _indices.Contains(index);

    public int IndexOf(int index)
    {
        if (!_indices.Contains(index))
            return -1;
        return _indices.OrderBy(i => i).ToList().IndexOf(index);
    }
}

/// <summary>
/// Collection of checked items in a CheckedListBox
/// </summary>
public class CheckedItemCollection
{
    private readonly CheckedListBox _owner;
    private readonly HashSet<int> _indices;

    internal CheckedItemCollection(CheckedListBox owner, HashSet<int> indices)
    {
        _owner = owner;
        _indices = indices;
    }

    public int Count => _indices.Count;

    public object this[int index]
    {
        get
        {
            if (index < 0 || index >= _indices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            var itemIndex = _indices.OrderBy(i => i).ElementAt(index);
            return _owner.Items[itemIndex];
        }
    }

    public bool Contains(object item)
    {
        var itemIndex = _owner.Items.IndexOf(item);
        return itemIndex >= 0 && _indices.Contains(itemIndex);
    }

    public int IndexOf(object item)
    {
        var itemIndex = _owner.Items.IndexOf(item);
        if (itemIndex < 0 || !_indices.Contains(itemIndex))
            return -1;
        return _indices.OrderBy(i => i).ToList().IndexOf(itemIndex);
    }
}

#endregion

#region EventArgs and Delegates

/// <summary>
/// Provides data for the ItemCheck event
/// </summary>
public class ItemCheckEventArgs : EventArgs
{
    public ItemCheckEventArgs(int index, CheckState newCheckValue, CheckState currentValue)
    {
        Index = index;
        NewValue = newCheckValue;
        CurrentValue = currentValue;
    }

    public int Index { get; }
    public CheckState NewValue { get; set; }
    public CheckState CurrentValue { get; }
}

/// <summary>
/// Represents the method that will handle the ItemCheck event
/// </summary>
public delegate void ItemCheckEventHandler(object? sender, ItemCheckEventArgs e);

#endregion
