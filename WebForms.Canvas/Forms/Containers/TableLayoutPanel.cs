using System.Runtime.CompilerServices;

namespace System.Windows.Forms;

public class TableLayoutPanel : Panel
{
   private int _columnCount = 1;
    private int _rowCount = 1;
    private TableLayoutPanelGrowStyle _growStyle = TableLayoutPanelGrowStyle.AddRows;
    private TableLayoutPanelCellBorderStyle _cellBorderStyle = TableLayoutPanelCellBorderStyle.None;

    private readonly TableLayoutStyleCollection _columnStyles = new();
    private readonly TableLayoutStyleCollection _rowStyles = new();

    private static readonly ConditionalWeakTable<Control, TableLayoutInfo> s_layoutInfo = new();

    public TableLayoutPanel()
    {
        TabStop = false;
        EnsureStyles(_columnStyles, _columnCount, isColumn: true);
        EnsureStyles(_rowStyles, _rowCount, isColumn: false);
    }

    public int ColumnCount
    {
        get => _columnCount;
        set
        {
            value = System.Math.Max(0, value);
            if (_columnCount != value)
            {
                _columnCount = value;
              EnsureStyles(_columnStyles, _columnCount, isColumn: true);
                PerformLayout();
                Invalidate();
            }
        }
    }

    public int RowCount
    {
        get => _rowCount;
        set
        {
            value = System.Math.Max(0, value);
            if (_rowCount != value)
            {
                _rowCount = value;
                EnsureStyles(_rowStyles, _rowCount, isColumn: false);
                PerformLayout();
                Invalidate();
            }
        }
    }

    public TableLayoutPanelGrowStyle GrowStyle
    {
        get => _growStyle;
        set
        {
            if (_growStyle != value)
            {
                _growStyle = value;
                PerformLayout();
                Invalidate();
            }
        }
    }

    public TableLayoutPanelCellBorderStyle CellBorderStyle
    {
        get => _cellBorderStyle;
        set
        {
            if (_cellBorderStyle != value)
            {
                _cellBorderStyle = value;
                Invalidate();
            }
        }
    }

    public TableLayoutStyleCollection ColumnStyles => _columnStyles;
    public TableLayoutStyleCollection RowStyles => _rowStyles;

    public void SetCellPosition(Control control, TableLayoutPanelCellPosition position)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));
        var info = s_layoutInfo.GetOrCreateValue(control);
        info.Column = position.Column;
        info.Row = position.Row;
        PerformLayout();
        Invalidate();
    }

    public TableLayoutPanelCellPosition GetCellPosition(Control control)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));
        return GetCellPositionStatic(control);
    }

    public static void SetColumn(Control control, int column)
    {
        if (control == null) return;
        var info = s_layoutInfo.GetOrCreateValue(control);
        info.Column = column;
      if (info.Row < 0) info.Row = 0;
        (control.Parent as TableLayoutPanel)?.PerformLayout();
        control.Parent?.Invalidate();
    }

    public static int GetColumn(Control control)
    {
        if (control == null) return -1;
        return s_layoutInfo.TryGetValue(control, out var info) ? info.Column : -1;
    }

    public static void SetRow(Control control, int row)
    {
        if (control == null) return;
        var info = s_layoutInfo.GetOrCreateValue(control);
        info.Row = row;
      if (info.Column < 0) info.Column = 0;
        (control.Parent as TableLayoutPanel)?.PerformLayout();
        control.Parent?.Invalidate();
    }

    public static int GetRow(Control control)
    {
        if (control == null) return -1;
        return s_layoutInfo.TryGetValue(control, out var info) ? info.Row : -1;
    }

    public static void SetColumnSpan(Control control, int value)
    {
        if (control == null) return;
        var info = s_layoutInfo.GetOrCreateValue(control);
        info.ColumnSpan = System.Math.Max(1, value);
        (control.Parent as TableLayoutPanel)?.PerformLayout();
        control.Parent?.Invalidate();
    }

    public static int GetColumnSpan(Control control)
    {
     if (control == null) return 1;
        return s_layoutInfo.TryGetValue(control, out var info) ? System.Math.Max(1, info.ColumnSpan) : 1;
    }

    public static void SetRowSpan(Control control, int value)
    {
        if (control == null) return;
        var info = s_layoutInfo.GetOrCreateValue(control);
        info.RowSpan = System.Math.Max(1, value);
        (control.Parent as TableLayoutPanel)?.PerformLayout();
        control.Parent?.Invalidate();
    }

    public static int GetRowSpan(Control control)
    {
        if (control == null) return 1;
        return s_layoutInfo.TryGetValue(control, out var info) ? System.Math.Max(1, info.RowSpan) : 1;
    }

    public override void PerformLayout()
    {
        if (IsLayoutSuspended) return;

        NormalizeTable();

        if (Controls.Count == 0)
        {
            return;
        }

        var borderWidth = GetBorderWidth();
        var paddingX = Padding.Width;
        var paddingY = Padding.Height;

        var innerLeft = borderWidth + paddingX;
        var innerTop = borderWidth + paddingY;
        var innerWidth = System.Math.Max(0, Width - (borderWidth * 2) - (paddingX * 2));
        var innerHeight = System.Math.Max(0, Height - (borderWidth * 2) - (paddingY * 2));

      var colWidths = ComputeTrackSizes(isColumn: true, _columnStyles, ColumnCount, innerWidth);
        var rowHeights = ComputeTrackSizes(isColumn: false, _rowStyles, RowCount, innerHeight);

        // Position children based on their cell.
        foreach (var child in Controls)
        {
            if (!child.Visible) continue;

             var pos = GetCellPositionStatic(child);
            var col = System.Math.Clamp(pos.Column, 0, System.Math.Max(0, ColumnCount - 1));
            var row = System.Math.Clamp(pos.Row, 0, System.Math.Max(0, RowCount - 1));

            var colSpan = System.Math.Clamp(GetColumnSpan(child), 1, System.Math.Max(1, ColumnCount - col));
            var rowSpan = System.Math.Clamp(GetRowSpan(child), 1, System.Math.Max(1, RowCount - row));

         var cellX = innerLeft + Sum(colWidths, 0, col);
            var cellY = innerTop + Sum(rowHeights, 0, row);
            var cellW = Sum(colWidths, col, colSpan);
            var cellH = Sum(rowHeights, row, rowSpan);

            LayoutChildInCell(child, cellX, cellY, cellW, cellH);
        }

        // After we size/position children, let each child run its own layout pass.
        // This is required for nested containers (e.g., a Panel with anchored children,
        // or a FlowLayoutPanel inside a table cell) to update their internal layouts.
        foreach (var child in Controls)
        {
            if (!child.Visible) continue;
            child.PerformLayout();
        }

        Invalidate();
    }

    private void LayoutChildInCell(Control child, int cellX, int cellY, int cellW, int cellH)
    {
        var marginX = child.Margin.Width;
        var marginY = child.Margin.Height;

        var innerX = cellX + marginX;
        var innerY = cellY + marginY;
        var innerW = System.Math.Max(0, cellW - (marginX * 2));
        var innerH = System.Math.Max(0, cellH - (marginY * 2));

        // 1) Dock is handled first (WinForms-like precedence)
        if (child.Dock != DockStyle.None)
        {
            switch (child.Dock)
            {
                case DockStyle.Fill:
                    child.Left = innerX;
                    child.Top = innerY;
                    child.Width = innerW;
                    child.Height = innerH;
                    return;
                case DockStyle.Top:
                    child.Left = innerX;
                    child.Top = innerY;
                    child.Width = innerW;
                    return;
                case DockStyle.Bottom:
                    child.Left = innerX;
                    child.Top = innerY + System.Math.Max(0, innerH - child.Height);
                    child.Width = innerW;
                    return;
                case DockStyle.Left:
                    child.Left = innerX;
                    child.Top = innerY;
                    child.Height = innerH;
                    return;
                case DockStyle.Right:
                    child.Left = innerX + System.Math.Max(0, innerW - child.Width);
                    child.Top = innerY;
                    child.Height = innerH;
                    return;
            }
        }

        // 2) Anchor within the cell
        var anchor = child.Anchor;
        var left = innerX;
        var top = innerY;
        var width = child.Width;
        var height = child.Height;

        var anchoredLeft = (anchor & AnchorStyles.Left) != 0;
        var anchoredRight = (anchor & AnchorStyles.Right) != 0;
        var anchoredTop = (anchor & AnchorStyles.Top) != 0;
        var anchoredBottom = (anchor & AnchorStyles.Bottom) != 0;

        if (anchoredLeft && anchoredRight)
        {
            width = innerW;
            left = innerX;
        }
        else if (anchoredRight && !anchoredLeft)
        {
            left = innerX + System.Math.Max(0, innerW - width);
        }
        else
        {
            left = innerX;
        }

        if (anchoredTop && anchoredBottom)
        {
            height = innerH;
            top = innerY;
        }
        else if (anchoredBottom && !anchoredTop)
        {
            top = innerY + System.Math.Max(0, innerH - height);
        }
        else
        {
            top = innerY;
        }

        child.Left = left;
        child.Top = top;
        child.Width = System.Math.Max(0, width);
        child.Height = System.Math.Max(0, height);

        // TableLayoutPanel is a layout container; after it has established a child's bounds,
        // treat those bounds as the anchor baseline for subsequent resizing.
        // This prevents the global Control.PerformLayout anchoring logic from using stale
        // baselines set before the table assigned cell bounds.
        if (child.Dock == DockStyle.None)
        {
            child.OriginalLeft = child.Left;
            child.OriginalTop = child.Top;
            child.OriginalWidth = child.Width;
            child.OriginalHeight = child.Height;
            child.OriginalParentWidth = Width;
            child.OriginalParentHeight = Height;
            child.OriginalBoundsSet = true;
        }
    }

    private void NormalizeTable()
    {
      if (ColumnCount <= 0) ColumnCount = 1;
        if (RowCount <= 0) RowCount = 1;

        EnsureStyles(_columnStyles, ColumnCount, isColumn: true);
        EnsureStyles(_rowStyles, RowCount, isColumn: false);

        EnsureImplicitAssignments();
    }

    private void EnsureImplicitAssignments()
    {
       var occupied = new HashSet<(int col, int row)>();
        foreach (var child in Controls)
        {
            var info = s_layoutInfo.GetOrCreateValue(child);
            if (info.Column >= 0 && info.Row >= 0)
            {
                EnsureCapacityFor(info.Column, info.Row);
                occupied.Add((info.Column, info.Row));
            }
        }

        var scanCol = 0;
        var scanRow = 0;

        foreach (var child in Controls)
        {
            var info = s_layoutInfo.GetOrCreateValue(child);
            if (info.Column >= 0 && info.Row >= 0) continue;

            // If one dimension is set, preserve it and find the next free cell along the other.
            if (info.Column >= 0 && info.Row < 0)
            {
                EnsureCapacityFor(info.Column, 0);
                info.Row = FindFirstFreeRowInColumn(info.Column, occupied);
                occupied.Add((info.Column, info.Row));
                continue;
            }

            if (info.Row >= 0 && info.Column < 0)
            {
                EnsureCapacityFor(0, info.Row);
                info.Column = FindFirstFreeColumnInRow(info.Row, occupied);
                occupied.Add((info.Column, info.Row));
                continue;
            }

            // Neither set.
            FindNextFreeCell(occupied, ref scanCol, ref scanRow);
            info.Column = scanCol;
            info.Row = scanRow;
            occupied.Add((scanCol, scanRow));
        }
    }

    private void EnsureCapacityFor(int column, int row)
    {
        if (column >= ColumnCount)
        {
            ColumnCount = column + 1;
        }
        if (row >= RowCount)
        {
            RowCount = row + 1;
        }
    }

    private int FindFirstFreeRowInColumn(int column, HashSet<(int col, int row)> occupied)
    {
        var row = 0;
        for (;;)
        {
            if (row >= RowCount)
            {
                if (GrowStyle == TableLayoutPanelGrowStyle.FixedSize)
                    return System.Math.Max(0, RowCount - 1);
                RowCount++;
            }

            if (!occupied.Contains((column, row)))
                return row;
            row++;
        }
    }

    private int FindFirstFreeColumnInRow(int row, HashSet<(int col, int row)> occupied)
    {
        var col = 0;
        for (;;)
        {
            if (col >= ColumnCount)
            {
                if (GrowStyle == TableLayoutPanelGrowStyle.FixedSize)
                    return System.Math.Max(0, ColumnCount - 1);
                ColumnCount++;
            }

            if (!occupied.Contains((col, row)))
                return col;
            col++;
        }
    }

    private void FindNextFreeCell(HashSet<(int col, int row)> occupied, ref int col, ref int row)
    {
        for (;;)
        {
            if (col >= ColumnCount)
            {
                col = 0;
                row++;
            }

            if (row >= RowCount)
            {
                if (GrowStyle == TableLayoutPanelGrowStyle.FixedSize)
                {
                    row = RowCount - 1;
                    col = System.Math.Min(col, ColumnCount - 1);
                    return;
                }

                if (GrowStyle == TableLayoutPanelGrowStyle.AddColumns)
                {
                    ColumnCount++;
                    col = ColumnCount - 1;
                    row = 0;
                }
                else
                {
                    RowCount++;
                    row = RowCount - 1;
                    col = 0;
                }
            }

            if (!occupied.Contains((col, row)))
            {
                return;
            }

            col++;
        }
    }

    public TableLayoutPanelCellPosition GetPositionFromControl(Control control)
    {
        if (control == null) throw new ArgumentNullException(nameof(control));
        return GetCellPositionStatic(control);
    }

    public Control? GetControlFromPosition(int column, int row)
    {
        foreach (var child in Controls)
        {
            var pos = GetCellPositionStatic(child);
            if (pos.Column == column && pos.Row == row)
                return child;
        }
        return null;
    }

     private static TableLayoutPanelCellPosition GetCellPositionStatic(Control control)
    {
        if (control == null) return new TableLayoutPanelCellPosition(0, 0);
        if (!s_layoutInfo.TryGetValue(control, out var info)) return new TableLayoutPanelCellPosition(-1, -1);
        return new TableLayoutPanelCellPosition(info.Column, info.Row);
    }

    private static void EnsureStyles(TableLayoutStyleCollection styles, int count, bool isColumn)
    {
        while (styles.Count < count)
        {
            styles.Add(isColumn ? new ColumnStyle(SizeType.Percent, 100f) : new RowStyle(SizeType.Percent, 100f));
        }
        while (styles.Count > count)
        {
            styles.RemoveAt(styles.Count - 1);
        }
    }

    private int[] ComputeTrackSizes(bool isColumn, TableLayoutStyleCollection styles, int count, int total)
    {
        var sizes = new int[count];
        if (count == 0) return sizes;

        var absTotal = 0;
        var percentTotal = 0f;
        var autoIndices = new List<int>();

        for (var i = 0; i < count; i++)
        {
            var style = styles[i];
            switch (style.SizeType)
            {
                case SizeType.Absolute:
                    sizes[i] = (int)System.Math.Round(style.Size);
                    absTotal += sizes[i];
                    break;
              case SizeType.AutoSize:
                    autoIndices.Add(i);
                    break;
                case SizeType.Percent:
                    percentTotal += style.Size;
                    break;
                default:
                    percentTotal += style.Size;
                    break;
            }
        }

      // Resolve AutoSize tracks based on preferred sizes of contained controls.
        if (autoIndices.Count > 0)
        {
            foreach (var idx in autoIndices)
            {
                var max = 0;
                foreach (var child in Controls)
                {
                    if (!child.Visible) continue;
                    var pos = GetCellPosition(child);
                    if (pos.Column < 0 || pos.Row < 0) continue;

                 var span = isColumn ? GetColumnSpan(child) : GetRowSpan(child);
                    var startTrack = isColumn ? pos.Column : pos.Row;
                    if (span < 1) span = 1;
                    if (idx < startTrack || idx >= startTrack + span) continue;

                    var margin = isColumn ? child.Margin.Width * 2 : child.Margin.Height * 2;
                    var pref = child.PreferredSize;
                  var need = (isColumn ? pref.Width : pref.Height) + margin;

                    // For spanning controls, attribute a proportional share of the need to each
                    // autosize track it touches.
                    if (span > 1)
                    {
                        need = (int)System.Math.Ceiling(need / (double)span);
                    }
                    max = System.Math.Max(max, need);
                }

                sizes[idx] = max;
                absTotal += max;
            }
        }

        var remaining = System.Math.Max(0, total - absTotal);
     if (percentTotal <= 0f)
        {
           // Even split among non-absolute/non-autosize
            var flexibleCount = 0;
            for (var i = 0; i < count; i++)
            {
                if (styles[i].SizeType != SizeType.Absolute && styles[i].SizeType != SizeType.AutoSize) flexibleCount++;
            }

            var each = flexibleCount > 0 ? remaining / flexibleCount : 0;
            for (var i = 0; i < count; i++)
            {
                if (styles[i].SizeType != SizeType.Absolute && styles[i].SizeType != SizeType.AutoSize)
                {
                    sizes[i] = each;
                }
            }
            return sizes;
        }

        // Percent split
        var used = 0;
        for (var i = 0; i < count; i++)
        {
          if (styles[i].SizeType == SizeType.Absolute || styles[i].SizeType == SizeType.AutoSize) continue;
            var part = (int)System.Math.Floor(remaining * (styles[i].Size / percentTotal));
            sizes[i] = part;
            used += part;
        }

        // Distribute leftover pixels
        var leftover = remaining - used;
        for (var i = 0; i < count && leftover > 0; i++)
        {
          if (styles[i].SizeType == SizeType.Absolute || styles[i].SizeType == SizeType.AutoSize) continue;
            sizes[i]++;
            leftover--;
        }

        return sizes;
    }

    private static int Sum(int[] arr, int start, int count)
    {
        var s = 0;
        for (var i = 0; i < count; i++) s += arr[start + i];
        return s;
    }

 private int GetBorderWidth()
 {
    return BorderStyle switch
     {
         BorderStyle.Fixed3D => 2,
         BorderStyle.FixedSingle => 1,
         _ => 0
     };
 }

 // ---- CellBorderStyle rendering ----

 protected internal override void OnPaint(PaintEventArgs e)
 {
     // Let Panel paint background + children first.
     base.OnPaint(e);

     if (_cellBorderStyle == TableLayoutPanelCellBorderStyle.None) return;
     if (_columnCount <= 0 || _rowCount <= 0) return;

     var borderWidth = GetBorderWidth();
     var paddingX = Padding.Width;
     var paddingY = Padding.Height;

     var innerLeft = borderWidth + paddingX;
     var innerTop = borderWidth + paddingY;
     var innerWidth = System.Math.Max(0, Width - (borderWidth * 2) - (paddingX * 2));
     var innerHeight = System.Math.Max(0, Height - (borderWidth * 2) - (paddingY * 2));

     var colWidths = ComputeTrackSizes(isColumn: true, _columnStyles, _columnCount, innerWidth);
     var rowHeights = ComputeTrackSizes(isColumn: false, _rowStyles, _rowCount, innerHeight);

     var g = e.Graphics;

     switch (_cellBorderStyle)
     {
         case TableLayoutPanelCellBorderStyle.Single:
             DrawCellGrid(g, innerLeft, innerTop, colWidths, rowHeights, 1,
                 CanvasColor.FromArgb(172, 172, 172));
             break;

         case TableLayoutPanelCellBorderStyle.Inset:
             DrawCellGrid3D(g, innerLeft, innerTop, colWidths, rowHeights,
                 CanvasColor.FromArgb(128, 128, 128), CanvasColor.FromArgb(255, 255, 255));
             break;

         case TableLayoutPanelCellBorderStyle.InsetDouble:
             DrawCellGrid3D(g, innerLeft, innerTop, colWidths, rowHeights,
                 CanvasColor.FromArgb(128, 128, 128), CanvasColor.FromArgb(255, 255, 255));
             DrawCellGrid3D(g, innerLeft + 1, innerTop + 1,
                 ShiftSizes(colWidths, -2), ShiftSizes(rowHeights, -2),
                 CanvasColor.FromArgb(160, 160, 160), CanvasColor.FromArgb(235, 235, 235));
             break;

         case TableLayoutPanelCellBorderStyle.Outset:
             DrawCellGrid3D(g, innerLeft, innerTop, colWidths, rowHeights,
                 CanvasColor.FromArgb(255, 255, 255), CanvasColor.FromArgb(128, 128, 128));
             break;

         case TableLayoutPanelCellBorderStyle.OutsetDouble:
         case TableLayoutPanelCellBorderStyle.OutsetPartial:
             DrawCellGrid3D(g, innerLeft, innerTop, colWidths, rowHeights,
                 CanvasColor.FromArgb(255, 255, 255), CanvasColor.FromArgb(128, 128, 128));
             DrawCellGrid3D(g, innerLeft + 1, innerTop + 1,
                 ShiftSizes(colWidths, -2), ShiftSizes(rowHeights, -2),
                 CanvasColor.FromArgb(235, 235, 235), CanvasColor.FromArgb(160, 160, 160));
             break;
     }
 }

 private static void DrawCellGrid(Graphics g, int left, int top, int[] colWidths, int[] rowHeights,
     int penWidth, CanvasColor color)
 {
     using var pen = new Pen(color);

     // Vertical lines (including right edge)
     var x = left;
     for (var c = 0; c <= colWidths.Length; c++)
     {
         var totalH = 0;
         foreach (var h in rowHeights) totalH += h;
         g.DrawLine(pen, x, top, x, top + totalH);
         if (c < colWidths.Length) x += colWidths[c];
     }

     // Horizontal lines (including bottom edge)
     var y = top;
     for (var r = 0; r <= rowHeights.Length; r++)
     {
         var totalW = 0;
         foreach (var w in colWidths) totalW += w;
         g.DrawLine(pen, left, y, left + totalW, y);
         if (r < rowHeights.Length) y += rowHeights[r];
     }
 }

 private static void DrawCellGrid3D(Graphics g, int left, int top, int[] colWidths, int[] rowHeights,
     CanvasColor topLeftColor, CanvasColor bottomRightColor)
 {
     using var tlPen = new Pen(topLeftColor);
     using var brPen = new Pen(bottomRightColor);

     var x = left;
     for (var c = 0; c <= colWidths.Length; c++)
     {
         var totalH = 0;
         foreach (var h in rowHeights) totalH += h;
         g.DrawLine(tlPen, x, top, x, top + totalH);
         if (c < colWidths.Length && c + 1 <= colWidths.Length)
         {
             g.DrawLine(brPen, x + 1, top, x + 1, top + totalH);
         }
         if (c < colWidths.Length) x += colWidths[c];
     }

     var y = top;
     for (var r = 0; r <= rowHeights.Length; r++)
     {
         var totalW = 0;
         foreach (var w in colWidths) totalW += w;
         g.DrawLine(tlPen, left, y, left + totalW, y);
         if (r < rowHeights.Length)
         {
             g.DrawLine(brPen, left, y + 1, left + totalW, y + 1);
         }
         if (r < rowHeights.Length) y += rowHeights[r];
     }
 }

 private static int[] ShiftSizes(int[] sizes, int delta)
 {
     var result = new int[sizes.Length];
     for (var i = 0; i < sizes.Length; i++)
         result[i] = System.Math.Max(0, sizes[i] + delta);
     return result;
 }

 private sealed class TableLayoutInfo
 {
        public int Column { get; set; } = -1;
        public int Row { get; set; } = -1;
        public int ColumnSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
    }
}

public readonly struct TableLayoutPanelCellPosition
{
    public int Column { get; }
    public int Row { get; }

    public TableLayoutPanelCellPosition(int column, int row)
    {
        Column = column;
        Row = row;
    }
}

public enum TableLayoutPanelGrowStyle
{
    FixedSize = 0,
    AddRows = 1,
    AddColumns = 2
}

public enum TableLayoutPanelCellBorderStyle
{
    None = 0,
    Single = 1,
    Inset = 2,
    InsetDouble = 3,
    Outset = 4,
    OutsetDouble = 5,
    OutsetPartial = 6
}

public enum SizeType
{
    Absolute = 0,
    Percent = 1,
    AutoSize = 2
}

public abstract class TableLayoutStyle
{
    public SizeType SizeType { get; set; }
    public float Size { get; set; }

    protected TableLayoutStyle(SizeType sizeType, float size)
    {
        SizeType = sizeType;
        Size = size;
    }
}

public sealed class ColumnStyle : TableLayoutStyle
{
    public ColumnStyle() : base(SizeType.Percent, 100f) { }
    public ColumnStyle(SizeType sizeType) : base(sizeType, 0f) { }
    public ColumnStyle(SizeType sizeType, float width) : base(sizeType, width) { }
}

public sealed class RowStyle : TableLayoutStyle
{
    public RowStyle() : base(SizeType.Percent, 100f) { }
    public RowStyle(SizeType sizeType) : base(sizeType, 0f) { }
    public RowStyle(SizeType sizeType, float height) : base(sizeType, height) { }
}

public sealed class TableLayoutStyleCollection : System.Collections.ObjectModel.Collection<TableLayoutStyle>
{
}
