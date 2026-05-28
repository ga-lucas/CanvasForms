using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Canvas.Windows.Forms.Services;
using Microsoft.JSInterop;

namespace System.Windows.Forms
{

namespace DataVisualization.Charting
{

// ── Enumerations ──────────────────────────────────────────────────────────────

/// <summary>WinForms-compatible chart type enumeration.</summary>
public enum SeriesChartType
{
    Bar         = 0,
    Line        = 1,
    Column      = 2,
    Area        = 3,
    StackedBar  = 4,
    StackedBar100 = 5,
    StackedColumn = 6,
    StackedColumn100 = 7,
    Pie         = 8,
    Doughnut    = 9,
    Radar       = 10,
    Scatter     = 11,
    Bubble      = 12,
    Point       = 13,
    Spline      = 14,
    SplineArea  = 15,
    StepLine    = 16,
    Candlestick = 17,
    Stock       = 18,
}

/// <summary>Legend docking position.</summary>
public enum Docking { Top, Bottom, Left, Right }

/// <summary>Legend/title alignment.</summary>
public enum StringAlignment { Near, Center, Far }

// ── DataPoint ─────────────────────────────────────────────────────────────────

/// <summary>A single data point in a <see cref="Series"/>.</summary>
public class DataPoint
{
    public string? AxisLabel { get; set; }
    public double  XValue    { get; set; }
    public double[] YValues  { get; set; } = [0d];
    public bool    IsEmpty   { get; set; }
    public string? Label     { get; set; }
    public string? Color     { get; set; }
    public string? ToolTip   { get; set; }

    public DataPoint() { }
    public DataPoint(double xValue, double yValue)
    {
        XValue  = xValue;
        YValues = [yValue];
    }

    internal void OnChanged() => Changed?.Invoke();
    internal event Action? Changed;
}

/// <summary>Strongly-typed collection of <see cref="DataPoint"/> objects.</summary>
public class DataPointCollection : Collection<DataPoint>
{
    internal event Action? Changed;

    public void AddXY(double x, double y)
    {
        var dp = new DataPoint(x, y);
        Add(dp);
    }

    public void AddXY(string label, double y)
    {
        var dp = new DataPoint { AxisLabel = label, YValues = [y] };
        Add(dp);
    }

    public void AddY(double y)
    {
        var dp = new DataPoint { YValues = [y] };
        Add(dp);
    }

    protected override void InsertItem(int index, DataPoint item)
    {
        base.InsertItem(index, item);
        item.Changed += () => Changed?.Invoke();
        Changed?.Invoke();
    }

    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        Changed?.Invoke();
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        Changed?.Invoke();
    }
}

// ── Series ────────────────────────────────────────────────────────────────────

/// <summary>Represents a data series in a <see cref="Chart"/>.</summary>
public class Series
{
    private string _name = string.Empty;
    private SeriesChartType _chartType = SeriesChartType.Column;
    private string? _color;
    private int _borderWidth = 2;
    private string _chartArea = "ChartArea1";
    private string _legend    = "Legend1";
    private bool   _isVisibleInLegend = true;

    public string Name
    {
        get => _name;
        set { _name = value; OnChanged(); }
    }

    public SeriesChartType ChartType
    {
        get => _chartType;
        set { _chartType = value; OnChanged(); }
    }

    /// <summary>Series color as an HTML/CSS color string (e.g. "#FF0000" or "red").</summary>
    public string? Color
    {
        get => _color;
        set { _color = value; OnChanged(); }
    }

    public int BorderWidth
    {
        get => _borderWidth;
        set { _borderWidth = value; OnChanged(); }
    }

    public string ChartArea
    {
        get => _chartArea;
        set { _chartArea = value; OnChanged(); }
    }

    public string Legend
    {
        get => _legend;
        set { _legend = value; OnChanged(); }
    }

    public bool IsVisibleInLegend
    {
        get => _isVisibleInLegend;
        set { _isVisibleInLegend = value; OnChanged(); }
    }

    public DataPointCollection Points { get; } = new();

    public Series() { Points.Changed += OnChanged; }
    public Series(string name) : this() { _name = name; }

    internal event Action? Changed;
    private void OnChanged() => Changed?.Invoke();
}

/// <summary>Strongly-typed collection of <see cref="Series"/> objects.</summary>
public class SeriesCollection : Collection<Series>
{
    internal event Action? Changed;

    protected override void InsertItem(int index, Series item)
    {
        base.InsertItem(index, item);
        item.Changed += () => Changed?.Invoke();
        Changed?.Invoke();
    }

    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        Changed?.Invoke();
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        Changed?.Invoke();
    }
}

// ── Axes ──────────────────────────────────────────────────────────────────────

/// <summary>Represents a chart axis.</summary>
public class Axis
{
    public string Title        { get; set; } = string.Empty;
    public bool   Enabled      { get; set; } = true;
    public double Minimum      { get; set; } = double.NaN;
    public double Maximum      { get; set; } = double.NaN;
    public double Interval     { get; set; } = double.NaN;
    public bool   IsLogarithmic { get; set; }
    public string LabelStyle   { get; set; } = string.Empty;
    public bool   Crossing     { get; set; }
}

// ── ChartArea ─────────────────────────────────────────────────────────────────

/// <summary>Represents a chart area within a <see cref="Chart"/>.</summary>
public class ChartArea
{
    public string Name    { get; set; } = "ChartArea1";
    public Axis   AxisX   { get; } = new();
    public Axis   AxisY   { get; } = new();
    public Axis   AxisX2  { get; } = new();
    public Axis   AxisY2  { get; } = new();
    public bool   Area3DStyleEnable3D { get; set; }

    public ChartArea() { }
    public ChartArea(string name) { Name = name; }
}

/// <summary>Strongly-typed collection of <see cref="ChartArea"/> objects.</summary>
public class ChartAreaCollection : Collection<ChartArea>
{
    internal event Action? Changed;

    protected override void InsertItem(int index, ChartArea item)
    {
        base.InsertItem(index, item);
        Changed?.Invoke();
    }

    protected override void RemoveItem(int index)
    {
        base.RemoveItem(index);
        Changed?.Invoke();
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        Changed?.Invoke();
    }
}

// ── Legend ────────────────────────────────────────────────────────────────────

/// <summary>Represents a legend in a <see cref="Chart"/>.</summary>
public class Legend
{
    public string         Name      { get; set; } = "Legend1";
    public Docking        Docking   { get; set; } = Docking.Top;
    public bool           Enabled   { get; set; } = true;
    public StringAlignment Alignment { get; set; } = StringAlignment.Center;

    public Legend() { }
    public Legend(string name) { Name = name; }
}

/// <summary>Strongly-typed collection of <see cref="Legend"/> objects.</summary>
public class LegendCollection : Collection<Legend>
{
    internal event Action? Changed;

    protected override void InsertItem(int index, Legend item)
    {
        base.InsertItem(index, item);
        Changed?.Invoke();
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        Changed?.Invoke();
    }
}

// ── Title ─────────────────────────────────────────────────────────────────────

/// <summary>Represents a title in a <see cref="Chart"/>.</summary>
public class Title
{
    public string         Text      { get; set; } = string.Empty;
    public string         Name      { get; set; } = string.Empty;
    public Docking        Docking   { get; set; } = Docking.Top;
    public StringAlignment Alignment { get; set; } = StringAlignment.Center;
    public string         Font      { get; set; } = "11pt Segoe UI";

    public Title() { }
    public Title(string text) { Text = text; }
}

/// <summary>Strongly-typed collection of <see cref="Title"/> objects.</summary>
public class TitleCollection : Collection<Title>
{
    internal event Action? Changed;

    protected override void InsertItem(int index, Title item)
    {
        base.InsertItem(index, item);
        Changed?.Invoke();
    }

    protected override void ClearItems()
    {
        base.ClearItems();
        Changed?.Invoke();
    }
}

// ── Chart ─────────────────────────────────────────────────────────────────────

} // namespace DataVisualization.Charting

/// <summary>
/// WinForms-compatible Chart control rendered via Chart.js bridge.
/// Follows the WebBrowser overlay pattern: positioned as an absolutely-placed
/// &lt;div&gt;/&lt;canvas&gt; on top of the form canvas by <c>FormRenderer.razor</c>.
/// </summary>
public class Chart : Control
{
    private static int _instanceCounter;
    private readonly string _chartId = $"chart-{System.Threading.Interlocked.Increment(ref _instanceCounter)}";

    // ── Internal JS handle ────────────────────────────────────────────────────
    internal IJSRuntime? JSRuntime { get; set; }

    // ── Chart model collections ───────────────────────────────────────────────
    public DataVisualization.Charting.SeriesCollection   Series     { get; } = new();
    public DataVisualization.Charting.ChartAreaCollection ChartAreas { get; } = new();
    public DataVisualization.Charting.LegendCollection   Legends    { get; } = new();
    public DataVisualization.Charting.TitleCollection    Titles     { get; } = new();

    // ── Internal state ────────────────────────────────────────────────────────
    internal string ChartId => _chartId;
    internal bool   NeedsRender { get; private set; } = true;
    internal event Action? ChartInvalidated;

    public Chart()
    {
        // Default chart area and legend always present (WinForms default)
        ChartAreas.Add(new DataVisualization.Charting.ChartArea("ChartArea1"));
        Legends.Add(new DataVisualization.Charting.Legend("Legend1"));

        Series.Changed     += OnChartChanged;
        ChartAreas.Changed += OnChartChanged;
        Legends.Changed    += OnChartChanged;
        Titles.Changed     += OnChartChanged;
    }

    private void OnChartChanged()
    {
        NeedsRender = true;
        ChartInvalidated?.Invoke();
    }

    internal void ClearNeedsRender() => NeedsRender = false;

    // ── Serialise model for JS ────────────────────────────────────────────────

    /// <summary>Serializes the chart model to a JSON string consumed by <c>chart-bridge.js</c>.</summary>
    public string SerializeModel()
    {
        var model = new ChartModel
        {
            ChartId    = _chartId,
            Series     = [.. Series.Select(s => new SeriesModel
            {
                Name      = s.Name,
                ChartType = s.ChartType.ToString(),
                Color     = s.Color,
                BorderWidth = s.BorderWidth,
                IsVisibleInLegend = s.IsVisibleInLegend,
                Points    = [.. s.Points.Select(p => new DataPointModel
                {
                    AxisLabel = p.AxisLabel,
                    XValue    = p.XValue,
                    YValues   = p.YValues,
                    Label     = p.Label,
                    Color     = p.Color,
                    IsEmpty   = p.IsEmpty,
                })]
            })],
            ChartAreas = [.. ChartAreas.Select(ca => new ChartAreaModel
            {
                Name               = ca.Name,
                AxisXTitle         = ca.AxisX.Title,
                AxisYTitle         = ca.AxisY.Title,
                AxisXMinimum       = double.IsNaN(ca.AxisX.Minimum) ? null : ca.AxisX.Minimum,
                AxisXMaximum       = double.IsNaN(ca.AxisX.Maximum) ? null : ca.AxisX.Maximum,
                AxisYMinimum       = double.IsNaN(ca.AxisY.Minimum) ? null : ca.AxisY.Minimum,
                AxisYMaximum       = double.IsNaN(ca.AxisY.Maximum) ? null : ca.AxisY.Maximum,
                AxisXLogarithmic   = ca.AxisX.IsLogarithmic,
                AxisYLogarithmic   = ca.AxisY.IsLogarithmic,
            })],
            Legends = [.. Legends.Select(l => new LegendModel
            {
                Name    = l.Name,
                Enabled = l.Enabled,
                Docking = l.Docking.ToString().ToLowerInvariant(),
            })],
            Titles = [.. Titles.Select(t => new TitleModel
            {
                Text    = t.Text,
                Docking = t.Docking.ToString().ToLowerInvariant(),
                Font    = t.Font,
            })],
        };

        return JsonSerializer.Serialize(model, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        });
    }

    // ── Serialisation POCOs (internal) ────────────────────────────────────────

    internal sealed class ChartModel
    {
        public string ChartId    { get; set; } = string.Empty;
        public SeriesModel[]    Series     { get; set; } = [];
        public ChartAreaModel[] ChartAreas { get; set; } = [];
        public LegendModel[]    Legends    { get; set; } = [];
        public TitleModel[]     Titles     { get; set; } = [];
    }

    internal sealed class SeriesModel
    {
        public string Name      { get; set; } = string.Empty;
        public string ChartType { get; set; } = "Column";
        public string? Color    { get; set; }
        public int BorderWidth  { get; set; }
        public bool IsVisibleInLegend { get; set; }
        public DataPointModel[] Points { get; set; } = [];
    }

    internal sealed class DataPointModel
    {
        public string? AxisLabel { get; set; }
        public double  XValue    { get; set; }
        public double[] YValues  { get; set; } = [];
        public string? Label     { get; set; }
        public string? Color     { get; set; }
        public bool    IsEmpty   { get; set; }
    }

    internal sealed class ChartAreaModel
    {
        public string  Name               { get; set; } = string.Empty;
        public string  AxisXTitle         { get; set; } = string.Empty;
        public string  AxisYTitle         { get; set; } = string.Empty;
        public double? AxisXMinimum       { get; set; }
        public double? AxisXMaximum       { get; set; }
        public double? AxisYMinimum       { get; set; }
        public double? AxisYMaximum       { get; set; }
        public bool    AxisXLogarithmic   { get; set; }
        public bool    AxisYLogarithmic   { get; set; }
    }

    internal sealed class LegendModel
    {
        public string Name    { get; set; } = string.Empty;
        public bool   Enabled { get; set; }
        public string Docking { get; set; } = "top";
    }

    internal sealed class TitleModel
    {
        public string Text    { get; set; } = string.Empty;
        public string Docking { get; set; } = "top";
        public string Font    { get; set; } = string.Empty;
    }
}

} // namespace System.Windows.Forms
