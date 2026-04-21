
namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms MonthCalendar control
/// </summary>
public class MonthCalendar : Control
{
    private const int HeaderHeight = 24;
    private const int DayHeaderHeight = 18;
    private const int CellW = 28;
    private const int CellH = 20;
    private const int ColCount = 7;
    private const int RowCount = 6;
    private const int PaddingX = 4;

    private DateTime _selectionStart;
    private DateTime _selectionEnd;
    private DateTime _displayMonth;
    private int _hoverDay = -1;
    private bool _prevHover = false;
    private bool _nextHover = false;

    public event DateRangeEventHandler? DateChanged;
    public event DateRangeEventHandler? DateSelected;

    public MonthCalendar()
    {
        _selectionStart = _selectionEnd = DateTime.Today;
        _displayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        Width = PaddingX * 2 + ColCount * CellW;
        Height = HeaderHeight + DayHeaderHeight + RowCount * CellH + 4;
        BackColor = Color.White;
        ForeColor = Color.Black;
        TabStop = true;
    }

    public DateTime SelectionStart
    {
        get => _selectionStart;
        set { _selectionStart = value; if (_selectionEnd < _selectionStart) _selectionEnd = _selectionStart; Invalidate(); }
    }

    public DateTime SelectionEnd
    {
        get => _selectionEnd;
        set { _selectionEnd = value; if (_selectionStart > _selectionEnd) _selectionStart = _selectionEnd; Invalidate(); }
    }

    public DateTime TodayDate { get; set; } = DateTime.Today;

    public int MaxSelectionCount { get; set; } = 7;
    public DateTime MinDate { get; set; } = new DateTime(1753, 1, 1);
    public DateTime MaxDate { get; set; } = new DateTime(9998, 12, 31);
    public bool ShowToday { get; set; } = true;
    public bool ShowTodayCircle { get; set; } = true;
    public bool ShowWeekNumbers { get; set; } = false;

    private int CalendarWidth => PaddingX * 2 + ColCount * CellW;

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        // Background
        using var bgBrush = new SolidBrush(BackColor);
        g.FillRectangle(bgBrush, 0, 0, Width, Height);
        using var borderPen = new Pen(Color.FromArgb(171, 171, 171));
        g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);

        // Header bar
        var headerBg = Color.FromArgb(0, 120, 215);
        using var headerBrush = new SolidBrush(headerBg);
        g.FillRectangle(headerBrush, 0, 0, Width, HeaderHeight);

        // Prev/Next arrows
        var arrowColor = _prevHover ? Color.White : Color.FromArgb(200, 230, 255);
        using var arrowBrush = new SolidBrush(arrowColor);
        // ◄
        g.DrawString("◄", "Arial", 11, arrowBrush, 6, 5);
        // ►
        var nextColor = _nextHover ? Color.White : Color.FromArgb(200, 230, 255);
        using var nextBrush = new SolidBrush(nextColor);
        g.DrawString("►", "Arial", 11, nextBrush, Width - 18, 5);

        // Month/year label
        var monthText = _displayMonth.ToString("MMMM yyyy");
        int labelX = (Width - monthText.Length * 7) / 2;
        using var headerTextBrush = new SolidBrush(Color.White);
        g.DrawString(monthText, "Arial", 12, headerTextBrush, labelX, 5);

        // Day-of-week headers
        string[] dayNames = { "Su", "Mo", "Tu", "We", "Th", "Fr", "Sa" };
        using var dayHeaderBrush = new SolidBrush(Color.FromArgb(0, 90, 158));
        for (int d = 0; d < 7; d++)
        {
            int x = PaddingX + d * CellW + (CellW - dayNames[d].Length * 7) / 2;
            g.DrawString(dayNames[d], "Arial", 10, dayHeaderBrush, x, HeaderHeight + 2);
        }

        // Day cells
        int daysInMonth = DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month);
        int firstDow = (int)_displayMonth.DayOfWeek; // 0 = Sunday

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(_displayMonth.Year, _displayMonth.Month, day);
            int cellIndex = firstDow + day - 1;
            int row = cellIndex / 7;
            int col = cellIndex % 7;
            int cellX = PaddingX + col * CellW;
            int cellY = HeaderHeight + DayHeaderHeight + row * CellH;

            bool isSelected = date >= _selectionStart && date <= _selectionEnd;
            bool isToday = date == TodayDate;
            bool isHovered = _hoverDay == day;

            // Cell background
            if (isSelected)
            {
                using var selBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                g.FillRectangle(selBrush, cellX, cellY, CellW - 1, CellH - 1);
            }
            else if (isHovered)
            {
                using var hoverBrush = new SolidBrush(Color.FromArgb(229, 241, 251));
                g.FillRectangle(hoverBrush, cellX, cellY, CellW - 1, CellH - 1);
            }

            // Today circle
            if (isToday && ShowTodayCircle && !isSelected)
            {
                using var circlePen = new Pen(Color.FromArgb(0, 120, 215));
                g.DrawRectangle(circlePen, cellX, cellY, CellW - 2, CellH - 2);
            }

            // Day number
            var dayColor = isSelected ? Color.White : (date.DayOfWeek == DayOfWeek.Sunday ? Color.FromArgb(180, 0, 0) : ForeColor);
            using var dayBrush = new SolidBrush(dayColor);
            int numX = cellX + (CellW - day.ToString().Length * 7) / 2;
            g.DrawString(day.ToString(), "Arial", 11, dayBrush, numX, cellY + 2);
        }

        base.OnPaint(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        _prevHover = e.Y < HeaderHeight && e.X < 20;
        _nextHover = e.Y < HeaderHeight && e.X > Width - 20;

        int newHover = HitTestDay(e.X, e.Y);
        if (newHover != _hoverDay) { _hoverDay = newHover; Invalidate(); }

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        _hoverDay = -1; _prevHover = false; _nextHover = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) { base.OnMouseDown(e); return; }
        Focus();

        // Prev/Next month navigation
        if (e.Y < HeaderHeight)
        {
            if (e.X < 20) NavigateMonth(-1);
            else if (e.X > Width - 20) NavigateMonth(1);
            return;
        }

        int day = HitTestDay(e.X, e.Y);
        if (day > 0)
        {
            var date = new DateTime(_displayMonth.Year, _displayMonth.Month, day);
            _selectionStart = _selectionEnd = date;
            DateChanged?.Invoke(this, new DateRangeEventArgs(_selectionStart, _selectionEnd));
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected internal override void OnMouseUp(MouseEventArgs e)
    {
        int day = HitTestDay(e.X, e.Y);
        if (day > 0)
            DateSelected?.Invoke(this, new DateRangeEventArgs(_selectionStart, _selectionEnd));
        base.OnMouseUp(e);
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.Left:  MoveSelection(-1); e.Handled = true; return;
            case Keys.Right: MoveSelection(1);  e.Handled = true; return;
            case Keys.Up:    MoveSelection(-7); e.Handled = true; return;
            case Keys.Down:  MoveSelection(7);  e.Handled = true; return;
            case Keys.PageUp:   NavigateMonth(-1); e.Handled = true; return;
            case Keys.PageDown: NavigateMonth(1);  e.Handled = true; return;
        }
        base.OnKeyDown(e);
    }

    private void MoveSelection(int days)
    {
        var newDate = _selectionStart.AddDays(days);
        if (newDate < MinDate || newDate > MaxDate) return;
        _selectionStart = _selectionEnd = newDate;
        if (newDate.Year != _displayMonth.Year || newDate.Month != _displayMonth.Month)
            _displayMonth = new DateTime(newDate.Year, newDate.Month, 1);
        DateChanged?.Invoke(this, new DateRangeEventArgs(_selectionStart, _selectionEnd));
        Invalidate();
    }

    private void NavigateMonth(int delta)
    {
        _displayMonth = _displayMonth.AddMonths(delta);
        Invalidate();
    }

    private int HitTestDay(int x, int y)
    {
        int cellY0 = HeaderHeight + DayHeaderHeight;
        if (y < cellY0) return -1;
        int daysInMonth = DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month);
        int firstDow = (int)_displayMonth.DayOfWeek;
        for (int day = 1; day <= daysInMonth; day++)
        {
            int cellIndex = firstDow + day - 1;
            int row = cellIndex / 7;
            int col = cellIndex % 7;
            int cx = PaddingX + col * CellW;
            int cy = cellY0 + row * CellH;
            if (x >= cx && x < cx + CellW && y >= cy && y < cy + CellH)
                return day;
        }
        return -1;
    }
}

public delegate void DateRangeEventHandler(object? sender, DateRangeEventArgs e);
public class DateRangeEventArgs : EventArgs
{
    public DateTime Start { get; }
    public DateTime End { get; }
    public DateRangeEventArgs(DateTime start, DateTime end) { Start = start; End = end; }
}
