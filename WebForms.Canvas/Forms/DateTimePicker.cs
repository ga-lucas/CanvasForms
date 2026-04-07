using System.Globalization;
using Canvas.Windows.Forms.Drawing;

namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms DateTimePicker control (simplified)
/// </summary>
public class DateTimePicker : Control
{
    private const int DropDownButtonWidth = 20;
    private const int BorderWidth = 1;
    private const int PaddingX = 4;
    private const int PaddingY = 3;

    private const int CalendarHeaderHeight = 20;
    private const int CalendarDayHeaderHeight = 16;
    private const int CalendarCellSize = 20;
    private const int CalendarPadding = 2;

    private DateTime _value = DateTime.Now;
    private DateTime _displayMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private bool _droppedDown;
    private int _hoverDay = -1;

    private bool _showCheckBox;
    private bool _checked = true;

    public DateTimePicker()
    {
        Width = 140;
        Height = 23;
        BackColor = Color.White;
        ForeColor = Color.Black;

        // WinForms default
        Format = DateTimePickerFormat.Long;
    }

    public event EventHandler? ValueChanged;
    public event EventHandler? DropDown;
    public event EventHandler? CloseUp;
    public event EventHandler? FormatChanged;

    public event EventHandler? CheckedChanged;

    public override string Text
    {
        get => Checked ? GetDisplayText() : string.Empty;
        set
        {
            // Best-effort parse like WinForms.
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dt))
            {
                Value = dt;
            }
        }
    }

    public DateTime Value
    {
        get => _value;
        set
        {
            var clamped = Clamp(value);
            if (_value != clamped)
            {
                _value = clamped;
                _displayMonth = new DateTime(_value.Year, _value.Month, 1);
                OnValueChanged(EventArgs.Empty);
                OnTextChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    private DateTimePickerFormat _format = DateTimePickerFormat.Long;
    public DateTimePickerFormat Format
    {
        get => _format;
        set
        {
            if (_format != value)
            {
                _format = value;
                OnFormatChanged(EventArgs.Empty);
                OnTextChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public string CustomFormat
    {
        get => _customFormat;
        set
        {
            if (_customFormat != value)
            {
                _customFormat = value ?? string.Empty;
                if (Format == DateTimePickerFormat.Custom)
                    OnFormatChanged(EventArgs.Empty);
                OnTextChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    private string _customFormat = string.Empty;

    public bool ShowCheckBox
    {
        get => _showCheckBox;
        set
        {
            if (_showCheckBox != value)
            {
                _showCheckBox = value;
                OnTextChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public bool Checked
    {
        get => !ShowCheckBox || _checked;
        set
        {
            var newValue = value;
            if (_checked != newValue)
            {
                _checked = newValue;
                OnCheckedChanged(EventArgs.Empty);
                OnTextChanged(EventArgs.Empty);
                Invalidate();
            }
        }
    }

    protected virtual void OnValueChanged(EventArgs e)
    {
        ValueChanged?.Invoke(this, e);
    }

    protected virtual void OnDropDown(EventArgs e)
    {
        DropDown?.Invoke(this, e);
    }

    protected virtual void OnCloseUp(EventArgs e)
    {
        CloseUp?.Invoke(this, e);
    }

    protected virtual void OnFormatChanged(EventArgs e)
    {
        FormatChanged?.Invoke(this, e);
    }

    protected virtual void OnCheckedChanged(EventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
    }

    public LeftRightAlignment DropDownAlign { get; set; } = LeftRightAlignment.Left;

    public bool ShowUpDown { get; set; } = false;

    public new Font Font { get; set; } = new("Segoe UI", 9);

    private DateTime _minDate = new(1753, 1, 1);
    public DateTime MinDate
    {
        get => _minDate;
        set
        {
            _minDate = value;
            if (_maxDate < _minDate)
                _maxDate = _minDate;

            Value = Clamp(Value);
            Invalidate();
        }
    }

    private DateTime _maxDate = new(9998, 12, 31);
    public DateTime MaxDate
    {
        get => _maxDate;
        set
        {
            _maxDate = value;
            if (_minDate > _maxDate)
                _minDate = _maxDate;

            Value = Clamp(Value);
            Invalidate();
        }
    }

    public bool DroppedDown
    {
        get => _droppedDown;
        set
        {
            if (ShowUpDown)
                value = false;

            if (!Checked)
                value = false;

            if (_droppedDown == value) return;
            _droppedDown = value;
            _hoverDay = -1;

            if (_droppedDown)
            {
                _displayMonth = new DateTime(_value.Year, _value.Month, 1);
                OnDropDown(EventArgs.Empty);
            }
            else
            {
                OnCloseUp(EventArgs.Empty);
            }

            Invalidate();
        }
    }

    protected internal override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;

        var checkBoxWidth = ShowCheckBox ? 18 : 0;

        // Background
        using (var bg = new SolidBrush(Enabled ? BackColor : Color.FromArgb(240, 240, 240)))
        {
            g.FillRectangle(bg, 0, 0, Width, Height);
        }

        // Border
        using (var borderPen = new Pen(Color.FromArgb(122, 122, 122)))
        {
            g.DrawRectangle(borderPen, 0, 0, Width, Height);
        }

        // Button area
        var btnRect = new Rectangle(Width - DropDownButtonWidth, 0, DropDownButtonWidth, Height);
        using (var btnBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
        {
            g.FillRectangle(btnBrush, btnRect);
        }

        using (var sepPen = new Pen(Color.FromArgb(200, 200, 200)))
        {
            g.DrawLine(sepPen, btnRect.X, 0, btnRect.X, Height);
        }

        if (ShowUpDown)
        {
            DrawUpDownGlyph(g, btnRect);
        }
        else
        {
            DrawDropDownGlyph(g, btnRect);
        }

        if (ShowCheckBox)
        {
            DrawCheckBox(g, 2, (Height - 14) / 2, Checked);
        }

        // Text
        var text = Checked ? GetDisplayText() : string.Empty;
        var textBounds = new Rectangle(PaddingX + checkBoxWidth, PaddingY, Width - DropDownButtonWidth - (PaddingX * 2) - checkBoxWidth, Height - (PaddingY * 2));
        var textColor = Enabled ? ForeColor : Color.FromArgb(109, 109, 109);
        g.DrawString(text, Font.Family, (int)Font.Size, new SolidBrush(textColor), textBounds.X, textBounds.Y + 2);

        base.OnPaint(e);
    }

    internal void PaintWithoutDropDown(PaintEventArgs e)
    {
        OnPaint(e);
    }

    internal void PaintDropDownOnly(PaintEventArgs e)
    {
        if (!_droppedDown)
            return;

        PaintCalendar(e.Graphics);
    }

    internal bool HasVisibleDropDown => _droppedDown;

    internal Rectangle GetDropDownBounds()
    {
        var contentWidth = (CalendarPadding * 2) + (CalendarCellSize * 7);
        var width = Math.Max(contentWidth + (BorderWidth * 2), Width);
        var rows = GetRequiredRowCount(_displayMonth);
        var height = (CalendarPadding * 2) + CalendarHeaderHeight + CalendarDayHeaderHeight + (CalendarCellSize * rows) + (BorderWidth * 2);

        var x = DropDownAlign == LeftRightAlignment.Right ? Width - width : 0;
        return new Rectangle(x, Height, width, height);
    }

    protected internal override void OnMouseDown(MouseEventArgs e)
    {
        if (!Enabled)
        {
            base.OnMouseDown(e);
            return;
        }

        // WinForms behavior: clicking the text area sets focus and opens the drop-down
        Focus();

        var checkBoxWidth = ShowCheckBox ? 18 : 0;

        // Click on checkbox toggles checked
        if (ShowCheckBox && e.X >= 2 && e.X < 2 + 14 && e.Y >= (Height - 14) / 2 && e.Y < (Height - 14) / 2 + 14)
        {
            Checked = !Checked;
            return;
        }

        var btnRect = new Rectangle(Width - DropDownButtonWidth, 0, DropDownButtonWidth, Height);

        // Click in dropdown calendar area
        if (_droppedDown)
        {
            var dd = GetDropDownBounds();
            if (e.Y >= dd.Y && e.Y < dd.Bottom && e.X >= dd.X && e.X < dd.Right)
            {
                if (HandleCalendarMouseDown(e.X - dd.X, e.Y - dd.Y))
                    return;
            }
        }

        // Click on button toggles dropdown / spins value
        if (e.X >= btnRect.X)
        {
            if (ShowUpDown)
            {
                // Split the button into two halves (up/down)
                var isUp = e.Y < Height / 2;
                ApplySpin(isUp ? 1 : -1);
            }
            else
            {
                DroppedDown = !DroppedDown;
            }
            return;
        }

        // Click on text area opens/closes drop-down (like WinForms)
        if (e.Y >= 0 && e.Y < Height && e.X < btnRect.X)
        {
            if (!Checked)
                return;

            DroppedDown = !DroppedDown;
            return;
        }

        base.OnMouseDown(e);
    }

    protected internal override void OnMouseMove(MouseEventArgs e)
    {
        if (_droppedDown)
        {
            var dd = GetDropDownBounds();
            if (e.Y >= dd.Y && e.Y < dd.Bottom && e.X >= dd.X && e.X < dd.Right)
            {
                var hover = HitTestDay(e.X - dd.X, e.Y - dd.Y);
                if (_hoverDay != hover)
                {
                    _hoverDay = hover;
                    Invalidate();
                }
                return;
            }
            if (_hoverDay != -1)
            {
                _hoverDay = -1;
                Invalidate();
            }
        }

        base.OnMouseMove(e);
    }

    protected internal override void OnMouseLeave(EventArgs e)
    {
        if (_hoverDay != -1)
        {
            _hoverDay = -1;
            Invalidate();
        }

        base.OnMouseLeave(e);
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (!Enabled)
        {
            base.OnKeyDown(e);
            return;
        }

        if (_droppedDown)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    DroppedDown = false;
                    e.Handled = true;
                    return;
                case Keys.Left:
                    Value = Value.AddDays(-1);
                    e.Handled = true;
                    return;
                case Keys.Right:
                    Value = Value.AddDays(1);
                    e.Handled = true;
                    return;
                case Keys.Up:
                    Value = Value.AddDays(-7);
                    e.Handled = true;
                    return;
                case Keys.Down:
                    Value = Value.AddDays(7);
                    e.Handled = true;
                    return;
                case Keys.PageUp:
                    Value = Value.AddMonths(-1);
                    e.Handled = true;
                    return;
                case Keys.PageDown:
                    Value = Value.AddMonths(1);
                    e.Handled = true;
                    return;
                case Keys.Enter:
                    DroppedDown = false;
                    e.Handled = true;
                    return;
            }
        }
        else
        {
            if (ShowUpDown)
            {
                if (e.KeyCode == Keys.Up)
                {
                    ApplySpin(1);
                    e.Handled = true;
                    return;
                }
                if (e.KeyCode == Keys.Down)
                {
                    ApplySpin(-1);
                    e.Handled = true;
                    return;
                }
            }

            if (e.KeyCode == Keys.Down && e.Alt)
            {
                if (Checked)
                    DroppedDown = true;
                e.Handled = true;
                return;
            }
        }

        base.OnKeyDown(e);
    }

    protected internal override void OnLostFocus(EventArgs e)
    {
        if (DroppedDown)
        {
            DroppedDown = false;
        }

        base.OnLostFocus(e);
    }

    private static void DrawCheckBox(Graphics g, int x, int y, bool isChecked)
    {
        using var borderPen = new Pen(Color.FromArgb(122, 122, 122));
        using var bgBrush = new SolidBrush(Color.White);
        var rect = new Rectangle(x, y, 14, 14);
        g.FillRectangle(bgBrush, rect);
        g.DrawRectangle(borderPen, rect);

        if (isChecked)
        {
            using var pen = new Pen(Color.FromArgb(0, 0, 0), 2);
            g.DrawLine(pen, x + 3, y + 7, x + 6, y + 10);
            g.DrawLine(pen, x + 6, y + 10, x + 11, y + 3);
        }
    }

    private static void DrawUpDownGlyph(Graphics g, Rectangle btnRect)
    {
        var centerX = btnRect.X + (btnRect.Width / 2);

        using var pen = new Pen(Color.FromArgb(64, 64, 64), 2);

        // Up chevron
        var upCenterY = btnRect.Y + (btnRect.Height / 4);
        g.DrawLine(pen, centerX - 4, upCenterY + 2, centerX, upCenterY - 2);
        g.DrawLine(pen, centerX, upCenterY - 2, centerX + 4, upCenterY + 2);

        // Down chevron
        var downCenterY = btnRect.Y + (btnRect.Height * 3 / 4);
        g.DrawLine(pen, centerX - 4, downCenterY - 2, centerX, downCenterY + 2);
        g.DrawLine(pen, centerX, downCenterY + 2, centerX + 4, downCenterY - 2);
    }

    private void ApplySpin(int direction)
    {
        if (!Enabled || !Checked)
            return;

        var delta = GetSpinDelta();
        if (delta == TimeSpan.Zero)
            return;

        var candidate = Value.AddTicks(delta.Ticks * direction);
        Value = candidate;
    }

    private TimeSpan GetSpinDelta()
    {
        // Minimal, WinForms-compatible default behavior:
        // - Time format spins by minutes
        // - Otherwise spins by days
        return Format == DateTimePickerFormat.Time
            ? TimeSpan.FromMinutes(1)
            : TimeSpan.FromDays(1);
    }

    private void PaintCalendar(Graphics g)
    {
        var bounds = GetDropDownBounds();

        var gridWidth = CalendarCellSize * 7;
        var gridOriginX = bounds.X + Math.Max(CalendarPadding, (bounds.Width - gridWidth) / 2);

        // Panel background
        using (var bg = new SolidBrush(Color.White))
        {
            g.FillRectangle(bg, bounds);
        }

        using (var borderPen = new Pen(Color.FromArgb(122, 122, 122)))
        {
            g.DrawRectangle(borderPen, bounds);
        }

        // Header
        var headerRect = new Rectangle(bounds.X + CalendarPadding, bounds.Y + CalendarPadding, bounds.Width - (CalendarPadding * 2), CalendarHeaderHeight);
        using (var headerBrush = new SolidBrush(Color.FromArgb(240, 240, 240)))
        {
            g.FillRectangle(headerBrush, headerRect);
        }

        // Prev/Next buttons
        var prevRect = new Rectangle(headerRect.X + 4, headerRect.Y + 2, 24, headerRect.Height - 4);
        var nextRect = new Rectangle(headerRect.Right - 28, headerRect.Y + 2, 24, headerRect.Height - 4);
        DrawNavButton(g, prevRect, left: true);
        DrawNavButton(g, nextRect, left: false);

        // Month text (centered between navigation buttons)
        var monthText = _displayMonth.ToString("Y", CultureInfo.CurrentCulture);
        var monthTextWidth = monthText.Length * 7; // Approximate
        var titleAreaX = prevRect.Right + 4;
        var titleAreaWidth = nextRect.X - 4 - titleAreaX;
        var monthX = titleAreaX + Math.Max(0, (titleAreaWidth - monthTextWidth) / 2);
        g.DrawString(monthText, Font.Family, Math.Max(1, (int)Font.Size), new SolidBrush(Color.Black), monthX, headerRect.Y + 3);

        // Day-of-week header
        var dowRect = new Rectangle(gridOriginX, headerRect.Bottom, gridWidth, CalendarDayHeaderHeight);
        using (var dowBrush = new SolidBrush(Color.FromArgb(250, 250, 250)))
        {
            g.FillRectangle(dowBrush, dowRect);
        }

        var dayNames = CultureInfo.CurrentCulture.DateTimeFormat.ShortestDayNames;
        var firstDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        for (int col = 0; col < 7; col++)
        {
            var idx = (firstDayOfWeek + col) % 7;
            var name = dayNames[idx];
            var x = dowRect.X + (col * CalendarCellSize) + 5;
            g.DrawString(name, Font.Family, Math.Max(1, (int)(Font.Size - 1)), new SolidBrush(Color.FromArgb(64, 64, 64)), x, dowRect.Y + 1);
        }

        // Grid
        var gridOriginY = dowRect.Bottom;

        var daysInMonth = DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month);
        var firstOfMonth = _displayMonth;
        var dayOffset = GetDayOffset(firstOfMonth);
        var today = DateTime.Today;

        var requiredRows = GetRequiredRowCount(_displayMonth);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var cellIndex = dayOffset + (day - 1);
            var row = cellIndex / 7;
            var col = cellIndex % 7;

            if (row >= requiredRows)
                continue;

            var cellRect = new Rectangle(
                gridOriginX + (col * CalendarCellSize),
                gridOriginY + (row * CalendarCellSize),
                CalendarCellSize,
                CalendarCellSize);

            var dayDate = new DateTime(_displayMonth.Year, _displayMonth.Month, day);
            var isEnabledDay = IsInRange(dayDate);
            var isSelected = _value.Year == _displayMonth.Year && _value.Month == _displayMonth.Month && _value.Day == day;
            var isHover = _hoverDay == day && isEnabledDay;
            var isToday = dayDate.Date == today;

            if (isSelected)
            {
                using var selBrush = new SolidBrush(Color.FromArgb(0, 120, 215));
                g.FillRectangle(selBrush, cellRect);
            }
            else if (isHover)
            {
                using var hoverBrush = new SolidBrush(Color.FromArgb(229, 241, 251));
                g.FillRectangle(hoverBrush, cellRect);
            }

            // Today indicator (only if not selected)
            if (isToday && !isSelected)
            {
                using var todayPen = new Pen(Color.FromArgb(0, 120, 215));
                var r = new Rectangle(cellRect.X + 1, cellRect.Y + 1, cellRect.Width - 2, cellRect.Height - 2);
                g.DrawRectangle(todayPen, r);
            }

            var textColor = isSelected
                ? Color.White
                : (isEnabledDay ? Color.Black : Color.FromArgb(160, 160, 160));

            g.DrawString(day.ToString(CultureInfo.InvariantCulture), Font.Family, Math.Max(1, (int)Font.Size), new SolidBrush(textColor), cellRect.X + 6, cellRect.Y + 4);

            // Keyboard focus rectangle around the selected day when the control has focus.
            if (Focused && DroppedDown && isSelected)
            {
                var focusColor = isSelected ? Color.White : Color.Black;
                using var focusPen = new Pen(focusColor);
                var focusRect = new Rectangle(cellRect.X + 2, cellRect.Y + 2, cellRect.Width - 4, cellRect.Height - 4);
                g.DrawRectangle(focusPen, focusRect);
            }
        }
    }

    private static void DrawDropDownGlyph(Graphics g, Rectangle btnRect)
    {
        var centerX = btnRect.X + (btnRect.Width / 2);
        var centerY = btnRect.Y + (btnRect.Height / 2);

        using var pen = new Pen(Color.FromArgb(64, 64, 64), 2);
        g.DrawLine(pen, centerX - 4, centerY - 1, centerX, centerY + 3);
        g.DrawLine(pen, centerX, centerY + 3, centerX + 4, centerY - 1);
    }

    private static void DrawNavButton(Graphics g, Rectangle rect, bool left)
    {
        using var pen = new Pen(Color.FromArgb(64, 64, 64), 2);
        var midY = rect.Y + rect.Height / 2;
        if (left)
        {
            g.DrawLine(pen, rect.Right - 8, midY - 5, rect.X + 8, midY);
            g.DrawLine(pen, rect.X + 8, midY, rect.Right - 8, midY + 5);
        }
        else
        {
            g.DrawLine(pen, rect.X + 8, midY - 5, rect.Right - 8, midY);
            g.DrawLine(pen, rect.Right - 8, midY, rect.X + 8, midY + 5);
        }
    }

    private bool HandleCalendarMouseDown(int x, int y)
    {
        // coordinates relative to dropdown bounds (y starts at 0)
        var bounds = GetDropDownBounds();
        var headerRect = new Rectangle(CalendarPadding, CalendarPadding, bounds.Width - (CalendarPadding * 2), CalendarHeaderHeight);
        var prevRect = new Rectangle(headerRect.X + 4, headerRect.Y + 2, 24, headerRect.Height - 4);
        var nextRect = new Rectangle(headerRect.Right - 28, headerRect.Y + 2, 24, headerRect.Height - 4);

        if (x >= prevRect.X && x < prevRect.Right && y >= prevRect.Y && y < prevRect.Bottom)
        {
            _displayMonth = _displayMonth.AddMonths(-1);
            Invalidate();
            return true;
        }

        if (x >= nextRect.X && x < nextRect.Right && y >= nextRect.Y && y < nextRect.Bottom)
        {
            _displayMonth = _displayMonth.AddMonths(1);
            Invalidate();
            return true;
        }

        var day = HitTestDay(x, y);
        if (day >= 1)
        {
            var dateOnly = new DateTime(_displayMonth.Year, _displayMonth.Month, day);
            if (IsInRange(dateOnly))
            {
                var candidate = new DateTime(_displayMonth.Year, _displayMonth.Month, day, _value.Hour, _value.Minute, _value.Second, _value.Kind);
                Value = candidate;
                DroppedDown = false;
                return true;
            }
        }

        return false;
    }

    private int HitTestDay(int x, int y)
    {
        var bounds = GetDropDownBounds();
        var headerTop = CalendarPadding;
        var dowTop = headerTop + CalendarHeaderHeight;
        var gridTop = dowTop + CalendarDayHeaderHeight;

        if (y < gridTop) return -1;

        var gridWidth = CalendarCellSize * 7;
        var gridX = Math.Max(CalendarPadding, (bounds.Width - gridWidth) / 2);
        var gridY = gridTop;

        if (x < gridX || x >= gridX + gridWidth) return -1;

        var relX = x - gridX;
        var relY = y - gridY;

        var col = relX / CalendarCellSize;
        var row = relY / CalendarCellSize;
        var requiredRows = GetRequiredRowCount(_displayMonth);
        if (row < 0 || row >= requiredRows) return -1;

        var firstOfMonth = _displayMonth;
        var offset = GetDayOffset(firstOfMonth);
        var index = (row * 7) + col;
        var day = index - offset + 1;

        var daysInMonth = DateTime.DaysInMonth(_displayMonth.Year, _displayMonth.Month);
        if (day < 1 || day > daysInMonth) return -1;

        var dateOnly = new DateTime(_displayMonth.Year, _displayMonth.Month, day);
        if (!IsInRange(dateOnly)) return -1;

        return day;
    }

    private int GetDayOffset(DateTime firstOfMonth)
    {
        var firstDayOfWeek = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
        var dow = (int)firstOfMonth.DayOfWeek;
        var offset = dow - firstDayOfWeek;
        if (offset < 0) offset += 7;
        return offset;
    }

    private int GetRequiredRowCount(DateTime month)
    {
        var firstOfMonth = new DateTime(month.Year, month.Month, 1);
        var offset = GetDayOffset(firstOfMonth);
        var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
        return (int)Math.Ceiling((offset + daysInMonth) / 7.0);
    }

    private string GetDisplayText()
    {
        return Format switch
        {
            DateTimePickerFormat.Long => _value.ToLongDateString(),
            DateTimePickerFormat.Time => _value.ToLongTimeString(),
            DateTimePickerFormat.Custom when !string.IsNullOrWhiteSpace(CustomFormat) => _value.ToString(CustomFormat, CultureInfo.CurrentCulture),
            _ => _value.ToShortDateString()
        };
    }

    private DateTime Clamp(DateTime value)
    {
        if (value < MinDate) return MinDate;
        if (value > MaxDate) return MaxDate;
        return value;
    }

    private bool IsInRange(DateTime date)
    {
        var d = date.Date;
        return d >= MinDate.Date && d <= MaxDate.Date;
    }
}

public enum DateTimePickerFormat
{
    Long,
    Short,
    Time,
    Custom
}
