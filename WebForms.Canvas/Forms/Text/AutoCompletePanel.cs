using WebForms.Canvas.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents an auto-complete dropdown panel for TextBox controls
/// </summary>
internal class AutoCompletePanel
{
    private readonly TextBox _owner;
    private readonly List<string> _suggestions = new();
    private int _selectedIndex = -1;
    private int _hoveredIndex = -1;
    private int _topIndex = 0;
    private const int MaxVisibleItems = 8;
    private const int ItemHeight = 18;
    private const int ItemPadding = 3;
    private const int ScrollbarWidth = 16;
    private bool _isDraggingScrollbar = false;
    private int _scrollbarDragStartY = 0;
    private int _scrollbarDragStartTopIndex = 0;

    public AutoCompletePanel(TextBox owner)
    {
        _owner = owner;
    }

    public bool IsVisible { get; private set; }
    public int SuggestionCount => _suggestions.Count;

    /// <summary>
    /// Shows the autocomplete panel with the given suggestions
    /// </summary>
    public void Show(IEnumerable<string> suggestions, string currentText)
    {
        _suggestions.Clear();
        _suggestions.AddRange(suggestions);
        _selectedIndex = -1;
        _hoveredIndex = -1;
        _topIndex = 0;
        IsVisible = _suggestions.Count > 0;

        if (IsVisible)
        {
            _owner.Invalidate();
        }
    }

    /// <summary>
    /// Hides the autocomplete panel
    /// </summary>
    public void Hide()
    {
        if (IsVisible)
        {
            IsVisible = false;
            _suggestions.Clear();
            _selectedIndex = -1;
            _hoveredIndex = -1;
            _owner.Invalidate();
        }
    }

    /// <summary>
    /// Gets the bounds of the autocomplete panel
    /// </summary>
    public Rectangle GetBounds()
    {
        var visibleItems = Math.Min(_suggestions.Count, MaxVisibleItems);
        var height = (visibleItems * ItemHeight) + 4; // +4 for border
        var width = _owner.Width;

        return new Rectangle(0, _owner.Height, width, height);
    }

    /// <summary>
    /// Paints the autocomplete panel
    /// </summary>
    public void Paint(Graphics g)
    {
        if (!IsVisible || _suggestions.Count == 0)
            return;

        var bounds = GetBounds();
        var needsScrollbar = _suggestions.Count > MaxVisibleItems;
        var contentWidth = bounds.Width - 4 - (needsScrollbar ? ScrollbarWidth : 0);

        // Background
        using var bgBrush = new SolidBrush(Color.White);
        g.FillRectangle(bgBrush, bounds);

        // Border
        using var borderPen = new Pen(Color.FromArgb(100, 100, 100));
        g.DrawRectangle(borderPen, bounds);

        // Draw items
        var visibleCount = Math.Min(MaxVisibleItems, _suggestions.Count - _topIndex);
        for (int i = 0; i < visibleCount; i++)
        {
            var itemIndex = _topIndex + i;
            if (itemIndex >= _suggestions.Count) break;

            var itemBounds = new Rectangle(
                bounds.X + 2,
                bounds.Y + 2 + (i * ItemHeight),
                contentWidth,
                ItemHeight
            );

            DrawItem(g, itemIndex, itemBounds);
        }

        // Draw scrollbar if needed
        if (needsScrollbar)
        {
            DrawScrollbar(g, bounds);
        }
    }

    private void DrawItem(Graphics g, int index, Rectangle bounds)
    {
        var item = _suggestions[index];
        var isSelected = index == _selectedIndex;
        var isHovered = index == _hoveredIndex;

        // Background
        Color bgColor;
        if (isSelected || isHovered)
        {
            bgColor = Color.FromArgb(0, 120, 215);
        }
        else
        {
            bgColor = Color.White;
        }

        using var bgBrush = new SolidBrush(bgColor);
        g.FillRectangle(bgBrush, bounds);

        // Text
        var textColor = (isSelected || isHovered) ? Color.White : Color.Black;
        g.DrawString(item, bounds.X + ItemPadding, bounds.Y + ItemPadding, textColor);
    }

    private void DrawScrollbar(Graphics g, Rectangle panelBounds)
    {
        var scrollbarBounds = new Rectangle(
            panelBounds.Right - ScrollbarWidth - 2,
            panelBounds.Y + 2,
            ScrollbarWidth,
            panelBounds.Height - 4
        );

        // Background
        using var scrollBgBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
        g.FillRectangle(scrollBgBrush, scrollbarBounds);

        // Calculate thumb
        var thumbHeight = Math.Max(20, (MaxVisibleItems * scrollbarBounds.Height) / Math.Max(1, _suggestions.Count));
        var maxTopIndex = Math.Max(1, _suggestions.Count - MaxVisibleItems);
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

    /// <summary>
    /// Handles mouse down on the autocomplete panel
    /// </summary>
    public bool HandleMouseDown(MouseEventArgs e)
    {
        if (!IsVisible)
            return false;

        var bounds = GetBounds();
        var needsScrollbar = _suggestions.Count > MaxVisibleItems;

        // Check if click is within panel bounds (relative to TextBox)
        if (e.Y >= bounds.Y && e.Y < bounds.Bottom &&
            e.X >= bounds.X && e.X < bounds.Right)
        {
            // Check scrollbar click
            if (needsScrollbar && e.X >= bounds.Right - ScrollbarWidth - 2)
            {
                HandleScrollbarClick(e, bounds);
                return true;
            }

            // Check item click
            var relativeY = e.Y - bounds.Y - 2;
            var itemIndex = _topIndex + (relativeY / ItemHeight);

            if (itemIndex >= 0 && itemIndex < _suggestions.Count)
            {
                AcceptSuggestion(itemIndex);
                return true;
            }

            return true; // Consumed the click
        }

        return false;
    }

    private void HandleScrollbarClick(MouseEventArgs e, Rectangle panelBounds)
    {
        var scrollbarBounds = new Rectangle(
            panelBounds.Right - ScrollbarWidth - 2,
            panelBounds.Y + 2,
            ScrollbarWidth,
            panelBounds.Height - 4
        );

        var thumbHeight = Math.Max(20, (MaxVisibleItems * scrollbarBounds.Height) / Math.Max(1, _suggestions.Count));
        var maxTopIndex = Math.Max(0, _suggestions.Count - MaxVisibleItems);
        var thumbTop = scrollbarBounds.Y + (_topIndex * (scrollbarBounds.Height - thumbHeight)) / Math.Max(1, maxTopIndex);

        if (e.Y < thumbTop)
        {
            // Page up
            _topIndex = Math.Max(0, _topIndex - MaxVisibleItems);
        }
        else if (e.Y > thumbTop + thumbHeight)
        {
            // Page down
            _topIndex = Math.Min(maxTopIndex, _topIndex + MaxVisibleItems);
        }
        else
        {
            // Start thumb drag
            _isDraggingScrollbar = true;
            _scrollbarDragStartY = e.Y;
            _scrollbarDragStartTopIndex = _topIndex;
        }

        _owner.Invalidate();
    }

    /// <summary>
    /// Handles mouse move on the autocomplete panel
    /// </summary>
    public bool HandleMouseMove(MouseEventArgs e)
    {
        if (!IsVisible)
            return false;

        if (_isDraggingScrollbar)
        {
            HandleScrollbarDrag(e);
            return true;
        }

        var bounds = GetBounds();
        var needsScrollbar = _suggestions.Count > MaxVisibleItems;

        // Update hover state
        if (e.Y >= bounds.Y && e.Y < bounds.Bottom &&
            e.X >= bounds.X && e.X < bounds.Right - (needsScrollbar ? ScrollbarWidth : 0))
        {
            var relativeY = e.Y - bounds.Y - 2;
            var itemIndex = _topIndex + (relativeY / ItemHeight);

            if (itemIndex >= 0 && itemIndex < _suggestions.Count)
            {
                if (_hoveredIndex != itemIndex)
                {
                    _hoveredIndex = itemIndex;
                    _owner.Invalidate();
                }
                return true;
            }
        }

        if (_hoveredIndex != -1)
        {
            _hoveredIndex = -1;
            _owner.Invalidate();
        }

        return e.Y >= bounds.Y && e.Y < bounds.Bottom;
    }

    private void HandleScrollbarDrag(MouseEventArgs e)
    {
        var bounds = GetBounds();
        var scrollbarHeight = bounds.Height - 4;
        var thumbHeight = Math.Max(20, (MaxVisibleItems * scrollbarHeight) / Math.Max(1, _suggestions.Count));
        var trackHeight = scrollbarHeight - thumbHeight;
        var maxTopIndex = Math.Max(0, _suggestions.Count - MaxVisibleItems);

        var deltaY = e.Y - _scrollbarDragStartY;
        var indexDelta = (int)((deltaY * maxTopIndex) / Math.Max(1, trackHeight));
        var newTopIndex = Math.Clamp(_scrollbarDragStartTopIndex + indexDelta, 0, maxTopIndex);

        if (newTopIndex != _topIndex)
        {
            _topIndex = newTopIndex;
            _owner.Invalidate();
        }
    }

    /// <summary>
    /// Handles mouse up
    /// </summary>
    public void HandleMouseUp()
    {
        _isDraggingScrollbar = false;
    }

    /// <summary>
    /// Handles mouse wheel
    /// </summary>
    public bool HandleMouseWheel(MouseEventArgs e)
    {
        if (!IsVisible)
            return false;

        var bounds = GetBounds();
        if (e.Y >= bounds.Y && e.Y < bounds.Bottom)
        {
            var maxTopIndex = Math.Max(0, _suggestions.Count - MaxVisibleItems);

            if (e.Delta > 0)
            {
                _topIndex = Math.Max(0, _topIndex - 3);
            }
            else
            {
                _topIndex = Math.Min(maxTopIndex, _topIndex + 3);
            }

            _owner.Invalidate();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handles keyboard navigation
    /// </summary>
    public bool HandleKeyDown(KeyEventArgs e)
    {
        if (!IsVisible || _suggestions.Count == 0)
            return false;

        switch (e.KeyCode)
        {
            case Keys.Down:
                if (_selectedIndex < _suggestions.Count - 1)
                {
                    _selectedIndex++;
                    EnsureVisible(_selectedIndex);
                    _owner.Invalidate();
                }
                else if (_selectedIndex == -1 && _suggestions.Count > 0)
                {
                    _selectedIndex = 0;
                    _owner.Invalidate();
                }
                return true;

            case Keys.Up:
                if (_selectedIndex > 0)
                {
                    _selectedIndex--;
                    EnsureVisible(_selectedIndex);
                    _owner.Invalidate();
                }
                return true;

            case Keys.PageDown:
                if (_selectedIndex < _suggestions.Count - 1)
                {
                    _selectedIndex = Math.Min(_suggestions.Count - 1, _selectedIndex + MaxVisibleItems);
                    EnsureVisible(_selectedIndex);
                    _owner.Invalidate();
                }
                return true;

            case Keys.PageUp:
                if (_selectedIndex > 0)
                {
                    _selectedIndex = Math.Max(0, _selectedIndex - MaxVisibleItems);
                    EnsureVisible(_selectedIndex);
                    _owner.Invalidate();
                }
                return true;

            case Keys.Home:
                if (_suggestions.Count > 0)
                {
                    _selectedIndex = 0;
                    EnsureVisible(0);
                    _owner.Invalidate();
                }
                return true;

            case Keys.End:
                if (_suggestions.Count > 0)
                {
                    _selectedIndex = _suggestions.Count - 1;
                    EnsureVisible(_selectedIndex);
                    _owner.Invalidate();
                }
                return true;

            case Keys.Enter:
            case Keys.Tab:
                if (_selectedIndex >= 0 && _selectedIndex < _suggestions.Count)
                {
                    AcceptSuggestion(_selectedIndex);
                    return true;
                }
                break;

            case Keys.Escape:
                Hide();
                return true;
        }

        return false;
    }

    private void EnsureVisible(int index)
    {
        if (index < _topIndex)
        {
            _topIndex = index;
        }
        else if (index >= _topIndex + MaxVisibleItems)
        {
            _topIndex = index - MaxVisibleItems + 1;
        }
    }

    private void AcceptSuggestion(int index)
    {
        if (index >= 0 && index < _suggestions.Count)
        {
            var suggestion = _suggestions[index];
            _owner.AcceptAutoCompleteSuggestion(suggestion);
            Hide();
        }
    }

    /// <summary>
    /// Gets the currently selected suggestion
    /// </summary>
    public string? GetSelectedSuggestion()
    {
        if (_selectedIndex >= 0 && _selectedIndex < _suggestions.Count)
            return _suggestions[_selectedIndex];
        return null;
    }
}
