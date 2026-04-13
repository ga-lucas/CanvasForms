using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms combo box control
/// </summary>
public class ComboBox : ListControl
{
    private const int ItemPadding = 2;
    private const int DropDownButtonWidth = 20;
    private const int DefaultDropDownHeight = 106; // ~6 items

    private ComboBoxStyle _dropDownStyle = ComboBoxStyle.DropDown;
    private int _dropDownWidth = 0; // 0 means use control width
    private int _dropDownHeight = DefaultDropDownHeight;
    private int _maxDropDownItems = 8;
    private bool _isDroppedDown = false;
    private string _text = "";
    private int _dropDownHoveredIndex = -1;

    public ComboBox()
    {
        Width = 121;
        Height = 23;
        BackColor = Color.White;
        ForeColor = Color.Black;
    }

    internal Rectangle GetDropDownBounds()
    {
        return new Rectangle(0, Height, DropDownWidth, GetActualDropDownHeight());
    }

    /// <summary>
    /// Gets or sets the style of the combo box
    /// </summary>
    public ComboBoxStyle DropDownStyle
    {
        get => _dropDownStyle;
        set
        {
            if (_dropDownStyle != value)
            {
                _dropDownStyle = value;
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the width of the drop-down portion
    /// </summary>
    public int DropDownWidth
    {
        get => _dropDownWidth > 0 ? _dropDownWidth : Width;
        set
        {
            _dropDownWidth = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the height of the drop-down portion
    /// </summary>
    public int DropDownHeight
    {
        get => _dropDownHeight;
        set
        {
            _dropDownHeight = Math.Max(1, value);
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of items in the drop-down
    /// </summary>
    public int MaxDropDownItems
    {
        get => _maxDropDownItems;
        set
        {
            _maxDropDownItems = Math.Max(1, Math.Min(100, value));
        }
    }

    /// <summary>
    /// Gets or sets whether the drop-down is currently open
    /// </summary>
    public bool DroppedDown
    {
        get => _isDroppedDown;
        set
        {
            if (_isDroppedDown != value)
            {
                _isDroppedDown = value;
                if (_isDroppedDown)
                {
                    OnDropDown(EventArgs.Empty);
                    // Reset scroll position and ensure selected item is visible
                    _topIndex = 0;
                    if (_selectedIndex >= 0)
                    {
                        EnsureVisible(_selectedIndex);
                    }
                }
                else
                {
                    OnDropDownClosed(EventArgs.Empty);
                    _dropDownHoveredIndex = -1;
                }
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text in the editable portion of the combo box
    /// </summary>
    public new string Text
    {
        get
        {
            if (_dropDownStyle == ComboBoxStyle.DropDownList)
            {
                return _selectedIndex >= 0 ? GetItemText(Items[_selectedIndex]) : "";
            }
            return _text;
        }
        set
        {
            if (_text != value)
            {
                _text = value ?? "";

                // Try to find matching item
                if (_dropDownStyle == ComboBoxStyle.DropDownList)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        if (GetItemText(Items[i]) == _text)
                        {
                            SelectedIndex = i;
                            break;
                        }
                    }
                }

                OnTextChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the selected index
    /// </summary>
    public override int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex != value && value >= -1 && value < Items.Count)
            {
                _selectedIndex = value;

                // Update text for DropDownList style
                if (_dropDownStyle == ComboBoxStyle.DropDownList && _selectedIndex >= 0)
                {
                    _text = GetItemText(Items[_selectedIndex]);
                }

                OnSelectedIndexChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether items are sorted
    /// </summary>
    public bool Sorted { get; set; } = false;

    /// <summary>
    /// Occurs when the drop-down is opened
    /// </summary>
    public event EventHandler? DropDown;

    /// <summary>
    /// Occurs when the drop-down is closed
    /// </summary>
    public event EventHandler? DropDownClosed;

    /// <summary>
    /// Occurs when the selected item changes via selection
    /// </summary>
    public event EventHandler? SelectionChangeCommitted;

    protected virtual void OnDropDown(EventArgs e)
    {
        DropDown?.Invoke(this, e);
    }

    protected virtual void OnDropDownClosed(EventArgs e)
    {
        DropDownClosed?.Invoke(this, e);
    }

    protected virtual void OnSelectionChangeCommitted(EventArgs e)
    {
        SelectionChangeCommitted?.Invoke(this, e);
    }

    /// <summary>
    /// Override border width - ComboBox has a 1px border
    /// </summary>
    protected override int BorderWidth => 1;

    /// <summary>
    /// Override to not use scrollbar in the main control area
    /// </summary>
    protected override bool NeedsScrollbar() => false;

    /// <summary>
    /// Gets the actual drop-down height based on items
    /// </summary>
    private int GetActualDropDownHeight()
    {
        var itemCount = Math.Min(Items.Count, _maxDropDownItems);
        var calculatedHeight = itemCount * ItemHeight + 2; // +2 for border
        return Math.Min(_dropDownHeight, Math.Max(calculatedHeight, ItemHeight + 2));
    }

    /// <summary>
    /// Gets whether the drop-down needs a scrollbar
    /// </summary>
    private bool DropDownNeedsScrollbar()
    {
        var dropDownHeight = GetActualDropDownHeight() - 2; // Exclude border
        var itemsPerPage = dropDownHeight / ItemHeight;
        return Items.Count > itemsPerPage;
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);

        // Draw the main combo box area
        DrawComboBoxArea(g, bounds);

        // Draw drop-down if open (but this will be overridden by Form's two-pass rendering)
        if (_isDroppedDown && Items.Count > 0)
        {
            DrawDropDown(g);
        }

        base.OnPaint(e);
    }

    /// <summary>
    /// Paints the ComboBox without the drop-down (called by Form for first pass)
    /// </summary>
    internal void PaintWithoutDropDown(PaintEventArgs e)
    {
        var g = e.Graphics;
        var bounds = new Rectangle(0, 0, Width, Height);

        // Draw only the main combo box area
        DrawComboBoxArea(g, bounds);

        // Don't call base.OnPaint here to avoid double-painting
    }

    /// <summary>
    /// Paints only the drop-down portion (called by Form for second pass, on top of everything)
    /// </summary>
    internal void PaintDropDownOnly(PaintEventArgs e)
    {
        if (_isDroppedDown && Items.Count > 0)
        {
            DrawDropDown(e.Graphics);
        }
    }

    private void DrawComboBoxArea(Graphics g, Rectangle bounds)
    {
        // Background
        var bgColor = Enabled ? BackColor : Color.FromArgb(240, 240, 240);
        using var bgBrush = new SolidBrush(bgColor);
        g.FillRectangle(bgBrush, bounds);

        // Border
        using var borderPen = new Pen(Focused ? Color.FromArgb(0, 120, 215) : Color.FromArgb(122, 122, 122));
        g.DrawRectangle(borderPen, bounds);

        // Text area
        var textBounds = new Rectangle(
            BorderWidth + 2,
            BorderWidth + 2,
            Width - DropDownButtonWidth - BorderWidth - 4,
            Height - (BorderWidth * 2) - 4
        );

        // Draw selected text or editable text
        var displayText = _dropDownStyle == ComboBoxStyle.DropDownList
            ? (_selectedIndex >= 0 ? GetItemText(Items[_selectedIndex]) : "")
            : _text;

        var textColor = Enabled ? ForeColor : Color.FromArgb(109, 109, 109);
        g.DrawString(displayText, textBounds.X, textBounds.Y + 1, textColor);

        // Drop-down button
        DrawDropDownButton(g, bounds);
    }

    private void DrawDropDownButton(Graphics g, Rectangle bounds)
    {
        var buttonBounds = new Rectangle(
            Width - DropDownButtonWidth - BorderWidth,
            BorderWidth,
            DropDownButtonWidth,
            Height - (BorderWidth * 2)
        );

        // Button background
        var buttonColor = _isDroppedDown 
            ? Color.FromArgb(204, 228, 247)
            : Color.FromArgb(240, 240, 240);
        using var buttonBrush = new SolidBrush(buttonColor);
        g.FillRectangle(buttonBrush, buttonBounds);

        // Button border (left edge)
        using var separatorPen = new Pen(Color.FromArgb(122, 122, 122));
        g.DrawLine(separatorPen, buttonBounds.X, buttonBounds.Y, buttonBounds.X, buttonBounds.Bottom);

        // Draw arrow
        var arrowX = buttonBounds.X + (buttonBounds.Width / 2);
        var arrowY = buttonBounds.Y + (buttonBounds.Height / 2);

        using var arrowPen = new Pen(Enabled ? Color.FromArgb(96, 96, 96) : Color.FromArgb(160, 160, 160));
        // Simple down arrow using lines
        g.DrawLine(arrowPen, arrowX - 4, arrowY - 2, arrowX, arrowY + 2);
        g.DrawLine(arrowPen, arrowX, arrowY + 2, arrowX + 4, arrowY - 2);
    }

    private void DrawDropDown(Graphics g)
    {
        var dropDownWidth = DropDownWidth;
        var dropDownHeight = GetActualDropDownHeight();
        var dropDownBounds = new Rectangle(0, Height, dropDownWidth, dropDownHeight);
        var needsScrollbar = DropDownNeedsScrollbar();
        var contentWidth = dropDownWidth - 2 - (needsScrollbar ? ScrollbarWidth : 0);

        // Drop-down background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, dropDownBounds);

        // Drop-down border
        using var borderPen = new Pen(Color.FromArgb(100, 100, 100));
        g.DrawRectangle(borderPen, dropDownBounds);

        // Calculate visible items
        var itemsPerPage = (dropDownHeight - 2) / ItemHeight;
        var visibleCount = Math.Min(itemsPerPage, Items.Count - _topIndex);

        // Draw items
        for (int i = 0; i < visibleCount; i++)
        {
            var itemIndex = _topIndex + i;
            if (itemIndex >= Items.Count) break;

            var itemBounds = new Rectangle(
                1,
                Height + 1 + (i * ItemHeight),
                contentWidth,
                ItemHeight
            );

            DrawDropDownItem(g, itemIndex, itemBounds);
        }

        // Draw scrollbar if needed
        if (needsScrollbar)
        {
            DrawDropDownScrollbar(g, dropDownBounds);
        }
    }

    private void DrawDropDownItem(Graphics g, int index, Rectangle bounds)
    {
        var item = Items[index];
        var isSelected = index == _selectedIndex;
        var isHovered = index == _dropDownHoveredIndex;

        // Background
        Color itemBgColor;
        if (isSelected || isHovered)
        {
            itemBgColor = Color.FromArgb(0, 120, 215);
        }
        else
        {
            itemBgColor = BackColor;
        }

        using var itemBgBrush = new SolidBrush(itemBgColor);
        g.FillRectangle(itemBgBrush, bounds);

        // Text
        var text = GetItemText(item);
        var textColor = (isSelected || isHovered) ? Color.White : ForeColor;
        g.DrawString(text, bounds.X + ItemPadding, bounds.Y + ItemPadding, textColor);
    }

    private void DrawDropDownScrollbar(Graphics g, Rectangle dropDownBounds)
    {
        var scrollbarBounds = new Rectangle(
            dropDownBounds.Right - ScrollbarWidth - 1,
            dropDownBounds.Y + 1,
            ScrollbarWidth,
            dropDownBounds.Height - 2
        );

        // Scrollbar background
        using var scrollBgBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
        g.FillRectangle(scrollBgBrush, scrollbarBounds);

        // Calculate thumb
        var dropDownHeight = dropDownBounds.Height - 2;
        var itemsPerPage = dropDownHeight / ItemHeight;
        var thumbHeight = Math.Max(20, (itemsPerPage * scrollbarBounds.Height) / Math.Max(1, Items.Count));
        var maxTopIndex = Math.Max(1, Items.Count - itemsPerPage);
        var thumbTop = (_topIndex * (scrollbarBounds.Height - thumbHeight)) / maxTopIndex;

        var thumbBounds = new Rectangle(
            scrollbarBounds.X + 2,
            scrollbarBounds.Y + thumbTop,
            ScrollbarWidth - 4,
            thumbHeight
        );

        // Thumb
        using var thumbBrush = new SolidBrush(Color.FromArgb(205, 205, 205));
        g.FillRectangle(thumbBrush, thumbBounds);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseDown(e);
            return;
        }

        Focus();

        // Check if clicking on drop-down button
        var buttonBounds = new Rectangle(
            Width - DropDownButtonWidth - BorderWidth,
            BorderWidth,
            DropDownButtonWidth,
            Height - (BorderWidth * 2)
        );

        if (e.X >= buttonBounds.X && e.X < buttonBounds.Right &&
            e.Y >= buttonBounds.Y && e.Y < buttonBounds.Bottom)
        {
            // Toggle drop-down
            DroppedDown = !DroppedDown;
            base.OnMouseDown(e);
            return;
        }

        // Check if clicking in the text area (for DropDownList, also toggle)
        if (e.Y < Height)
        {
            // WinForms: DropDownList opens when clicking the non-button area.
            // In this canvas implementation, the editable DropDown style is not truly editable,
            // so we also open when clicking the text area to match expected user interaction.
            if (_dropDownStyle is ComboBoxStyle.DropDownList or ComboBoxStyle.DropDown)
            {
                DroppedDown = !DroppedDown;
            }
            base.OnMouseDown(e);
            return;
        }

        // Check if clicking in drop-down area
        if (_isDroppedDown && e.Y >= Height)
        {
            var dropDownHeight = GetActualDropDownHeight();
            var needsScrollbar = DropDownNeedsScrollbar();
            var scrollbarX = Width - ScrollbarWidth - 1;

            // Check scrollbar click
            if (needsScrollbar && e.X >= scrollbarX)
            {
                HandleDropDownScrollbarClick(e);
                base.OnMouseDown(e);
                return;
            }

            // Check item click
            var itemIndex = _topIndex + ((e.Y - Height - 1) / ItemHeight);
            if (itemIndex >= 0 && itemIndex < Items.Count)
            {
                SelectedIndex = itemIndex;
                OnSelectionChangeCommitted(EventArgs.Empty);
                DroppedDown = false;
            }
        }

        base.OnMouseDown(e);
    }

    private void HandleDropDownScrollbarClick(MouseEventArgs e)
    {
        var dropDownHeight = GetActualDropDownHeight() - 2;
        var itemsPerPage = dropDownHeight / ItemHeight;
        var thumbHeight = Math.Max(20, (itemsPerPage * dropDownHeight) / Math.Max(1, Items.Count));
        var maxTopIndex = Math.Max(0, Items.Count - itemsPerPage);
        var thumbTop = Height + 1 + (_topIndex * (dropDownHeight - thumbHeight)) / Math.Max(1, maxTopIndex);

        if (e.Y < thumbTop)
        {
            // Page up
            _topIndex = Math.Max(0, _topIndex - itemsPerPage);
        }
        else if (e.Y > thumbTop + thumbHeight)
        {
            // Page down
            _topIndex = Math.Min(maxTopIndex, _topIndex + itemsPerPage);
        }
        else
        {
            // Start thumb drag
            _isDraggingScrollbar = true;
            _scrollbarDragStartY = e.Y;
            _scrollbarDragStartTopIndex = _topIndex;
        }

        Invalidate();
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        _isDraggingScrollbar = false;
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
        if (_isDraggingScrollbar && _isDroppedDown)
        {
            var dropDownHeight = GetActualDropDownHeight() - 2;
            var itemsPerPage = dropDownHeight / ItemHeight;
            var maxTopIndex = Math.Max(0, Items.Count - itemsPerPage);
            var thumbHeight = Math.Max(20, (itemsPerPage * dropDownHeight) / Math.Max(1, Items.Count));
            var trackHeight = dropDownHeight - thumbHeight;

            var deltaY = e.Y - _scrollbarDragStartY;
            var indexDelta = (int)((deltaY * maxTopIndex) / Math.Max(1, trackHeight));
            var newTopIndex = Math.Clamp(_scrollbarDragStartTopIndex + indexDelta, 0, maxTopIndex);

            if (newTopIndex != _topIndex)
            {
                _topIndex = newTopIndex;
                Invalidate();
            }

            base.OnMouseMove(e);
            return;
        }

        // Update hover state in drop-down
        if (_isDroppedDown && e.Y >= Height)
        {
            var dropDownHeight = GetActualDropDownHeight();
            var needsScrollbar = DropDownNeedsScrollbar();
            var scrollbarX = Width - ScrollbarWidth - 1;

            // Don't hover if over scrollbar
            if (needsScrollbar && e.X >= scrollbarX)
            {
                if (_dropDownHoveredIndex != -1)
                {
                    _dropDownHoveredIndex = -1;
                    Invalidate();
                }
            }
            else
            {
                var hoveredIndex = _topIndex + ((e.Y - Height - 1) / ItemHeight);
                if (hoveredIndex >= 0 && hoveredIndex < Items.Count)
                {
                    if (_dropDownHoveredIndex != hoveredIndex)
                    {
                        _dropDownHoveredIndex = hoveredIndex;
                        Invalidate();
                    }
                }
                else if (_dropDownHoveredIndex != -1)
                {
                    _dropDownHoveredIndex = -1;
                    Invalidate();
                }
            }
        }
        else if (_dropDownHoveredIndex != -1)
        {
            _dropDownHoveredIndex = -1;
            Invalidate();
        }

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_dropDownHoveredIndex != -1)
        {
            _dropDownHoveredIndex = -1;
            Invalidate();
        }
        base.OnMouseLeave(e);
    }

    protected internal override void OnMouseWheel(MouseEventArgs e)
    {
        if (_isDroppedDown)
        {
            // Scroll the drop-down
            var dropDownHeight = GetActualDropDownHeight() - 2;
            var itemsPerPage = dropDownHeight / ItemHeight;
            var maxTopIndex = Math.Max(0, Items.Count - itemsPerPage);

            if (e.Delta > 0)
            {
                _topIndex = Math.Max(0, _topIndex - 3);
            }
            else
            {
                _topIndex = Math.Min(maxTopIndex, _topIndex + 3);
            }
            Invalidate();
        }
        else
        {
            // Change selection
            if (e.Delta > 0 && _selectedIndex > 0)
            {
                SelectedIndex--;
                OnSelectionChangeCommitted(EventArgs.Empty);
            }
            else if (e.Delta < 0 && _selectedIndex < Items.Count - 1)
            {
                SelectedIndex++;
                OnSelectionChangeCommitted(EventArgs.Empty);
            }
        }

        base.OnMouseWheel(e);
    }

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
            case Keys.Up:
                if (_selectedIndex > 0)
                {
                    SelectedIndex--;
                    OnSelectionChangeCommitted(EventArgs.Empty);
                    handled = true;
                }
                break;

            case Keys.Down:
                if (e.Alt && !_isDroppedDown)
                {
                    // Alt+Down opens the drop-down
                    DroppedDown = true;
                    handled = true;
                }
                else if (_selectedIndex < Items.Count - 1)
                {
                    SelectedIndex++;
                    OnSelectionChangeCommitted(EventArgs.Empty);
                    handled = true;
                }
                break;

            case Keys.Enter:
                if (_isDroppedDown)
                {
                    DroppedDown = false;
                    handled = true;
                }
                break;

            case Keys.Escape:
                if (_isDroppedDown)
                {
                    DroppedDown = false;
                    handled = true;
                }
                break;

            case Keys.Home:
                if (Items.Count > 0)
                {
                    SelectedIndex = 0;
                    OnSelectionChangeCommitted(EventArgs.Empty);
                    handled = true;
                }
                break;

            case Keys.End:
                if (Items.Count > 0)
                {
                    SelectedIndex = Items.Count - 1;
                    OnSelectionChangeCommitted(EventArgs.Empty);
                    handled = true;
                }
                break;

            case Keys.PageUp:
                if (Items.Count > 0 && _selectedIndex > 0)
                {
                    var dropDownHeight = GetActualDropDownHeight() - 2;
                    var itemsPerPage = Math.Max(1, dropDownHeight / ItemHeight);
                    SelectedIndex = Math.Max(0, _selectedIndex - itemsPerPage);
                    OnSelectionChangeCommitted(EventArgs.Empty);
                    handled = true;
                }
                break;

            case Keys.PageDown:
                if (Items.Count > 0 && _selectedIndex < Items.Count - 1)
                {
                    var dropDownHeight = GetActualDropDownHeight() - 2;
                    var itemsPerPage = Math.Max(1, dropDownHeight / ItemHeight);
                    SelectedIndex = Math.Min(Items.Count - 1, _selectedIndex + itemsPerPage);
                    OnSelectionChangeCommitted(EventArgs.Empty);
                    handled = true;
                }
                break;

            case Keys.F4:
                // F4 toggles drop-down
                DroppedDown = !DroppedDown;
                handled = true;
                break;
        }

        if (handled)
        {
            e.Handled = true;
        }

        base.OnKeyDown(e);
    }

    protected internal override void OnLostFocus(EventArgs e)
    {
        // Close drop-down when losing focus
        if (_isDroppedDown)
        {
            DroppedDown = false;
        }
        base.OnLostFocus(e);
    }

    /// <summary>
    /// Selects all text in the editable portion
    /// </summary>
    public void SelectAll()
    {
        // For now, just a stub - would need text selection support
    }

    /// <summary>
    /// Finds the first item that starts with the specified string
    /// </summary>
    public int FindString(string s)
    {
        return FindString(s, -1);
    }

    /// <summary>
    /// Finds the first item after the specified index that starts with the specified string
    /// </summary>
    public int FindString(string s, int startIndex)
    {
        if (string.IsNullOrEmpty(s)) return -1;

        for (int i = startIndex + 1; i < Items.Count; i++)
        {
            var text = GetItemText(Items[i]);
            if (text.StartsWith(s, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds the first item that exactly matches the specified string
    /// </summary>
    public int FindStringExact(string s)
    {
        return FindStringExact(s, -1);
    }

    /// <summary>
    /// Finds the first item after the specified index that exactly matches the specified string
    /// </summary>
    public int FindStringExact(string s, int startIndex)
    {
        if (string.IsNullOrEmpty(s)) return -1;

        for (int i = startIndex + 1; i < Items.Count; i++)
        {
            var text = GetItemText(Items[i]);
            if (text.Equals(s, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}

/// <summary>
/// Specifies the style of a combo box
/// </summary>
public enum ComboBoxStyle
{
    /// <summary>
    /// The text portion is editable. The user can click the arrow button to display the list.
    /// </summary>
    DropDown = 1,

    /// <summary>
    /// The user cannot edit the text portion. The user must click the arrow button to display the list.
    /// </summary>
    DropDownList = 2,

    /// <summary>
    /// The text portion is editable. The list portion is always visible.
    /// </summary>
    Simple = 0
}
