
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

    // AutoComplete state for DropDown editable mode
    // _acUserText = what the user has actually typed (no appended suffix)
    // _acSuffix   = the suggested suffix appended after the user's text (displayed as "selected")
    private string _acUserText = "";
    private string _acSuffix   = "";

    public ComboBox()
    {
        Width = 121;
        Height = 23;
        BackColor = Color.White;
        ForeColor = Color.Black;
    }

    // ── Owner-draw support ────────────────────────────────────────────────────

    private DrawMode _drawMode = DrawMode.Normal;

    /// <summary>
    /// Gets or sets whether the control elements are drawn by the operating system
    /// or by user code.  Mirrors the real WinForms API; <see cref="DrawMode.OwnerDrawFixed"/>
    /// and <see cref="DrawMode.OwnerDrawVariable"/> both route item rendering through
    /// <see cref="DrawItem"/>.
    /// </summary>
    public DrawMode DrawMode
    {
        get => _drawMode;
        set { _drawMode = value; Invalidate(); }
    }

    /// <summary>Raised before an item is drawn so the caller can supply its height (OwnerDrawVariable only).</summary>
    public event MeasureItemEventHandler? MeasureItem;

    /// <summary>Raised when an item needs to be painted (any owner-draw mode).</summary>
    public event DrawItemEventHandler? DrawItem;

    /// <summary>
    /// The item height used for fixed-height modes. For <see cref="DrawMode.OwnerDrawVariable"/>
    /// the height is queried per-item via <see cref="MeasureItem"/>.
    /// </summary>
    private int _ownerDrawItemHeight = DefaultItemHeight;

    protected override int ItemHeight =>
        _drawMode == DrawMode.Normal ? DefaultItemHeight : _ownerDrawItemHeight;

    /// <summary>Returns the height for a specific item index, firing MeasureItem when needed.</summary>
    private int GetItemHeightAt(int index)
    {
        if (_drawMode != DrawMode.OwnerDrawVariable)
            return ItemHeight;

        using var tmpGraphics = new Graphics(Width, ItemHeight);
        var args = new MeasureItemEventArgs(tmpGraphics, index, DefaultItemHeight);
        MeasureItem?.Invoke(this, args);
        return Math.Max(1, args.ItemHeight);
    }

    /// <summary>
    /// Raises DrawItem for a single item. Returns the bounds actually passed to the handler.
    /// When DrawItem is null the method falls back to the default rendering.
    /// </summary>
    private void RaiseDrawItem(Graphics g, int index, Rectangle bounds, DrawItemState state)
    {
        if (DrawItem == null) return;
        var args = new DrawItemEventArgs(g, Font, bounds, index, state)
        {
            BackColor = BackColor,
            ForeColor = ForeColor
        };
        DrawItem.Invoke(this, args);
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
                _acUserText = _text;
                _acSuffix   = "";

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
    /// Gets or sets the auto-complete mode
    /// </summary>
    public AutoCompleteMode AutoCompleteMode { get; set; } = AutoCompleteMode.None;

    /// <summary>
    /// Gets or sets the source for auto-complete suggestions
    /// </summary>
    public AutoCompleteSource AutoCompleteSource { get; set; } = AutoCompleteSource.None;

    /// <summary>
    /// Gets or sets a custom auto-complete source (ListItems source is handled automatically)
    /// </summary>
    public AutoCompleteStringCollection AutoCompleteCustomSource { get; set; } = new AutoCompleteStringCollection();

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
        var bgColor = Enabled ? BackColor : System.Drawing.Color.FromArgb(240, 240, 240);
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

        // Owner-draw: fire DrawItem for the selected-item face (ComboBoxEdit state)
        if (_drawMode != DrawMode.Normal && DrawItem != null && _selectedIndex >= 0)
        {
            var faceState = DrawItemState.ComboBoxEdit;
            if (Focused) faceState |= DrawItemState.Focus;
            RaiseDrawItem(g, _selectedIndex, textBounds, faceState);
        }
        else
        {
            // Draw selected text or editable text
            var displayText = _dropDownStyle == ComboBoxStyle.DropDownList
                ? (_selectedIndex >= 0 ? GetItemText(Items[_selectedIndex]) : "")
                : _text;

            var textColor = Enabled ? ForeColor : System.Drawing.Color.FromArgb(109, 109, 109);

            if (_dropDownStyle == ComboBoxStyle.DropDown && _acSuffix.Length > 0)
            {
                // Draw user-typed part normally, then the autocomplete suffix with a selection highlight
                g.DrawString(_acUserText, textBounds.X, textBounds.Y + 1, textColor);

                // Estimate pixel width of the user-typed portion using ~7px/char heuristic
                var userWidth = EstimateTextWidth(_acUserText);
                var suffixBounds = new Rectangle(
                    textBounds.X + userWidth,
                    textBounds.Y,
                    textBounds.Width - userWidth,
                    textBounds.Height);

                // Selection highlight behind suffix
                var suffixWidth = Math.Min(suffixBounds.Width, EstimateTextWidth(_acSuffix) + 2);
                using var selBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                g.FillRectangle(selBrush, suffixBounds.X, suffixBounds.Y, suffixWidth, suffixBounds.Height);
                g.DrawString(_acSuffix, suffixBounds.X, textBounds.Y + 1, System.Drawing.Color.White);
            }
            else
            {
                g.DrawString(displayText, textBounds.X, textBounds.Y + 1, textColor);
            }
        }

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
        var contentWidth = dropDownWidth - 2 - (needsScrollbar ? VerticalScrollbarHelper.Width : 0);

        // Drop-down background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, dropDownBounds);

        // Drop-down border
        using var borderPen = new Pen(Color.FromArgb(100, 100, 100));
        g.DrawRectangle(borderPen, dropDownBounds);

        // Draw items — supports variable height for OwnerDrawVariable
        int yOffset = 1;
        int maxY    = dropDownHeight - 2;
        for (int i = _topIndex; i < Items.Count; i++)
        {
            var h = _drawMode == DrawMode.OwnerDrawVariable ? GetItemHeightAt(i) : ItemHeight;
            if (yOffset + h > maxY) break;

            var itemBounds = new Rectangle(1, Height + yOffset, contentWidth, h);
            DrawDropDownItem(g, i, itemBounds);
            yOffset += h;
        }

        // Draw scrollbar if needed
        if (needsScrollbar)
        {
            DrawDropDownScrollbar(g, dropDownBounds);
        }
    }

    private void DrawDropDownItem(Graphics g, int index, Rectangle bounds)
    {
        var isSelected = index == _selectedIndex;
        var isHovered  = index == _dropDownHoveredIndex;
        var state      = DrawItemState.Default;
        if (isSelected || isHovered) state |= DrawItemState.Selected;
        if (index == _selectedIndex)  state |= DrawItemState.Focus;

        if (_drawMode != DrawMode.Normal && DrawItem != null)
        {
            RaiseDrawItem(g, index, bounds, state);
            return;
        }

        var item = Items[index];

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
        var textColor = (isSelected || isHovered) ? System.Drawing.Color.White : ForeColor;
        g.DrawString(text, bounds.X + ItemPadding, bounds.Y + ItemPadding, textColor);
    }

    private void DrawDropDownScrollbar(Graphics g, Rectangle dropDownBounds)
    {
        var track = new Rectangle(
            dropDownBounds.Right - VerticalScrollbarHelper.Width - 1,
            dropDownBounds.Y + 1,
            VerticalScrollbarHelper.Width,
            dropDownBounds.Height - 2);

        var itemsPerPage = (dropDownBounds.Height - 2) / ItemHeight;
        var sb = new VerticalScrollbarHelper(track, Items.Count, itemsPerPage, _topIndex);
        sb.Draw(g);
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
            var scrollbarX = Width - VerticalScrollbarHelper.Width - 1;

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
                _acUserText = GetItemText(Items[itemIndex]);
                _acSuffix   = "";
                DroppedDown = false;
            }
        }

        base.OnMouseDown(e);
    }

    private void HandleDropDownScrollbarClick(MouseEventArgs e)
    {
        var sb = MakeDropDownScrollbarHelper();
        var hit = sb.HitTest(e.X, e.Y);
        if (hit == ScrollbarHit.None) return;

        if (hit == ScrollbarHit.Thumb)
        {
            _isDraggingScrollbar = true;
            _scrollbarDragStartY = e.Y;
            _scrollbarDragStartTopIndex = _topIndex;
        }
        else if (hit == ScrollbarHit.ArrowUp)
        {
            _topIndex = sb.ClampTopIndex(_topIndex - 1);
        }
        else if (hit == ScrollbarHit.ArrowDown)
        {
            _topIndex = sb.ClampTopIndex(_topIndex + 1);
        }
        else
        {
            _topIndex = sb.ComputePageTopIndex(e.Y, _topIndex);
        }

        Invalidate();
    }

    private VerticalScrollbarHelper MakeDropDownScrollbarHelper()
    {
        var dropDownHeight = GetActualDropDownHeight() - 2;
        var track = new Rectangle(
            Width - VerticalScrollbarHelper.Width - 1,
            Height + 1,
            VerticalScrollbarHelper.Width,
            dropDownHeight);
        var itemsPerPage = dropDownHeight / ItemHeight;
        return new VerticalScrollbarHelper(track, Items.Count, itemsPerPage, _topIndex);
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
            var newTop = MakeDropDownScrollbarHelper()
                .ComputeDragTopIndex(e.Y, _scrollbarDragStartY, _scrollbarDragStartTopIndex);

            if (newTop != _topIndex)
            {
                _topIndex = newTop;
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
            var scrollbarX = Width - VerticalScrollbarHelper.Width - 1;

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
                    // If a suggestion is highlighted, select it
                    if (_dropDownHoveredIndex >= 0 && _dropDownHoveredIndex < Items.Count)
                    {
                        SelectedIndex = _dropDownHoveredIndex;
                        OnSelectionChangeCommitted(EventArgs.Empty);
                    }
                    DroppedDown = false;
                }
                CommitAutoComplete();
                handled = true;
                break;

            case Keys.Escape:
                if (_isDroppedDown)
                    DroppedDown = false;
                RevertAutoComplete();
                handled = true;
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
        // Close drop-down when losing focus — commit any pending autocomplete suffix
        if (_isDroppedDown)
            DroppedDown = false;
        CommitAutoComplete();
        base.OnLostFocus(e);
    }

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        if (!Enabled)
        {
            base.OnKeyPress(e);
            return;
        }

        // DropDownList: type-ahead — select first item whose text starts with the typed char
        if (_dropDownStyle == ComboBoxStyle.DropDownList)
        {
            if (!char.IsControl(e.KeyChar))
            {
                var match = FindString(e.KeyChar.ToString(), _selectedIndex);
                if (match == -1)
                    match = FindString(e.KeyChar.ToString(), -1); // wrap around
                if (match >= 0)
                {
                    SelectedIndex = match;
                    OnSelectionChangeCommitted(EventArgs.Empty);
                }
                e.Handled = true;
            }
            base.OnKeyPress(e);
            return;
        }

        // DropDown (editable): build _acUserText char by char
        if (_dropDownStyle == ComboBoxStyle.DropDown)
        {
            if (e.KeyChar == '\b') // Backspace
            {
                if (_acSuffix.Length > 0)
                {
                    // Discard suffix, keep user text as-is
                    _acSuffix = "";
                    _text = _acUserText;
                }
                else if (_acUserText.Length > 0)
                {
                    _acUserText = _acUserText[..^1];
                    _text = _acUserText;
                }
                ApplyAutoComplete();
                Invalidate();
                e.Handled = true;
            }
            else if (!char.IsControl(e.KeyChar))
            {
                // If a suffix was selected and user types a char that matches its start, advance
                if (_acSuffix.Length > 0 && char.ToLowerInvariant(e.KeyChar) == char.ToLowerInvariant(_acSuffix[0]))
                {
                    _acUserText += _acSuffix[0];
                    _acSuffix = _acSuffix[1..];
                    _text = _acUserText + _acSuffix;
                }
                else
                {
                    _acSuffix = "";
                    _acUserText += e.KeyChar;
                    _text = _acUserText;
                    ApplyAutoComplete();
                }
                Invalidate();
                e.Handled = true;
            }
        }

        base.OnKeyPress(e);
    }

    // ── AutoComplete helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Simple character-width estimate (~7 px per char at default font size).
    /// Matches the heuristic used by <c>TextMeasurementService.EstimateTextWidth</c>.
    /// </summary>
    private static int EstimateTextWidth(string text)
        => string.IsNullOrEmpty(text) ? 0 : (int)(text.Length * 7.0);

    /// <summary>
    /// Applies the current AutoCompleteMode to <see cref="_acUserText"/> and
    /// updates <see cref="_acSuffix"/> / dropdown state accordingly.
    /// </summary>
    private void ApplyAutoComplete()
    {
        if (AutoCompleteMode == AutoCompleteMode.None || string.IsNullOrEmpty(_acUserText))
        {
            _acSuffix = "";
            return;
        }

        var match = FindBestMatch(_acUserText);

        bool append  = AutoCompleteMode is AutoCompleteMode.Append  or AutoCompleteMode.SuggestAppend;
        bool suggest = AutoCompleteMode is AutoCompleteMode.Suggest or AutoCompleteMode.SuggestAppend;

        if (match != null)
        {
            if (append)
            {
                // Suffix = the part of the matched text beyond what the user typed
                _acSuffix = match.Length > _acUserText.Length
                    ? match[_acUserText.Length..]
                    : "";
                _text = _acUserText + _acSuffix;
            }

            if (suggest)
            {
                // Highlight the matching item in the dropdown and open it
                var idx = FindStringExact(match);
                if (idx < 0) idx = FindString(match);
                _dropDownHoveredIndex = idx;
                if (!_isDroppedDown)
                    DroppedDown = true;
            }
        }
        else
        {
            _acSuffix = "";
        }
    }

    /// <summary>
    /// Returns the best matching item text for <paramref name="prefix"/>,
    /// consulting <see cref="AutoCompleteSource"/> and
    /// <see cref="AutoCompleteCustomSource"/>.
    /// </summary>
    private string? FindBestMatch(string prefix)
    {
        IEnumerable<string> candidates;

        if (AutoCompleteSource == AutoCompleteSource.CustomSource)
        {
            candidates = AutoCompleteCustomSource.Cast<string>();
        }
        else
        {
            // ListItems (and the default when source is not Custom)
            candidates = Items.Cast<object>().Select(GetItemText);
        }

        return candidates.FirstOrDefault(s =>
            s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Commits the pending autocomplete suffix into the user text (called on
    /// Enter, Tab, LostFocus, and explicit item selection).
    /// </summary>
    private void CommitAutoComplete()
    {
        if (_acSuffix.Length > 0)
        {
            _acUserText = _acUserText + _acSuffix;
            _acSuffix   = "";
            _text = _acUserText;
        }
    }

    /// <summary>
    /// Discards the pending autocomplete suffix (called on Escape).
    /// </summary>
    private void RevertAutoComplete()
    {
        _acSuffix = "";
        _text = _acUserText;
        Invalidate();
    }

    /// <summary>
    /// Selects all text in the editable portion
    /// </summary>
    public void SelectAll()
    {
        // Commits any pending suffix so the whole text is "selected"
        CommitAutoComplete();
        Invalidate();
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
