using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demo form showcasing all implemented Chart types.
/// Each chart type gets its own tab page.
/// </summary>
public class ChartDemoForm : Form
{
    public ChartDemoForm()
    {
        Text   = "Chart Demo";
        Width  = 820;
        Height = 560;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
    }

    private void InitializeControls()
    {
        var title = new Label
        {
            Text      = "Chart Control — WinForms-compatible, rendered via Chart.js",
            Left      = 12,
            Top       = 10,
            Width     = 780,
            Height    = 22,
            ForeColor = Color.FromArgb(0, 51, 153),
        };
        Controls.Add(title);

        var tabs = new TabControl
        {
            Left   = 12,
            Top    = 38,
            Width  = 782,
            Height = 468,
        };
        Controls.Add(tabs);

        tabs.TabPages.Add(BuildTab("Line",        BuildLineChart()));
        tabs.TabPages.Add(BuildTab("Bar",         BuildBarChart()));
        tabs.TabPages.Add(BuildTab("Column",      BuildColumnChart()));
        tabs.TabPages.Add(BuildTab("Area",        BuildAreaChart()));
        tabs.TabPages.Add(BuildTab("Pie",         BuildPieChart()));
        tabs.TabPages.Add(BuildTab("Doughnut",    BuildDoughnutChart()));
        tabs.TabPages.Add(BuildTab("Radar",       BuildRadarChart()));
        tabs.TabPages.Add(BuildTab("Scatter",     BuildScatterChart()));
        tabs.TabPages.Add(BuildTab("Bubble",      BuildBubbleChart()));
        tabs.TabPages.Add(BuildTab("Stacked Bar", BuildStackedBarChart()));
        tabs.TabPages.Add(BuildTab("Spline",      BuildSplineChart()));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static TabPage BuildTab(string name, Chart chart)
    {
        var page = new TabPage(name);
        chart.Left   = 8;
        chart.Top    = 8;
        chart.Width  = 740;
        chart.Height = 400;
        page.Controls.Add(chart);
        return page;
    }

    private static Chart MakeChart(string title)
    {
        var chart = new Chart();
        if (!string.IsNullOrEmpty(title))
            chart.Titles.Add(new Title(title));
        return chart;
    }

    // ── Chart builders ────────────────────────────────────────────────────────

    private static Chart BuildLineChart()
    {
        var chart = MakeChart("Monthly Revenue — Line");
        chart.Legends.Add(new Legend("Legend1") { Enabled = true });

        var s1 = new Series("Product A") { ChartType = SeriesChartType.Line, BorderWidth = 3 };
        s1.Points.AddXY("Jan", 120); s1.Points.AddXY("Feb", 145); s1.Points.AddXY("Mar", 132);
        s1.Points.AddXY("Apr", 168); s1.Points.AddXY("May", 175); s1.Points.AddXY("Jun", 190);

        var s2 = new Series("Product B") { ChartType = SeriesChartType.Line, BorderWidth = 3 };
        s2.Points.AddXY("Jan", 80); s2.Points.AddXY("Feb", 95); s2.Points.AddXY("Mar", 110);
        s2.Points.AddXY("Apr", 102); s2.Points.AddXY("May", 130); s2.Points.AddXY("Jun", 148);

        chart.Series.Add(s1);
        chart.Series.Add(s2);
        return chart;
    }

    private static Chart BuildBarChart()
    {
        var chart = MakeChart("Sales by Region — Horizontal Bar");
        chart.Legends.Add(new Legend("Legend1") { Enabled = true, Docking = Docking.Bottom });

        var s = new Series("Sales") { ChartType = SeriesChartType.Bar };
        s.Points.AddXY("North", 430); s.Points.AddXY("South", 380);
        s.Points.AddXY("East", 510);  s.Points.AddXY("West", 290);
        chart.Series.Add(s);
        return chart;
    }

    private static Chart BuildColumnChart()
    {
        var chart = MakeChart("Quarterly Performance — Column");
        chart.Legends.Add(new Legend("Legend1") { Enabled = true });

        var teams = new[] { "Alpha", "Beta", "Gamma" };
        var q1    = new[] { 82.0, 67.0, 74.0 };
        var q2    = new[] { 88.0, 72.0, 79.0 };

        var s1 = new Series("Q1") { ChartType = SeriesChartType.Column };
        var s2 = new Series("Q2") { ChartType = SeriesChartType.Column };
        for (int i = 0; i < teams.Length; i++)
        {
            s1.Points.AddXY(teams[i], q1[i]);
            s2.Points.AddXY(teams[i], q2[i]);
        }
        chart.Series.Add(s1);
        chart.Series.Add(s2);
        return chart;
    }

    private static Chart BuildAreaChart()
    {
        var chart = MakeChart("CPU Usage — Area");
        chart.Legends.Add(new Legend("Legend1") { Enabled = false });

        var s = new Series("Usage %") { ChartType = SeriesChartType.Area, Color = "#3498db" };
        foreach (var (label, val) in new[] {
            ("00:00", 15.0), ("04:00", 10.0), ("08:00", 45.0),
            ("12:00", 72.0), ("16:00", 68.0), ("20:00", 38.0) })
        {
            s.Points.AddXY(label, val);
        }
        chart.Series.Add(s);
        return chart;
    }

    private static Chart BuildPieChart()
    {
        var chart = MakeChart("Market Share — Pie");
        chart.Legends.Add(new Legend("Legend1") { Enabled = true, Docking = Docking.Right });

        var s = new Series("Share") { ChartType = SeriesChartType.Pie };
        s.Points.AddXY("CanvasForms", 38);
        s.Points.AddXY("WinForms",    25);
        s.Points.AddXY("WPF",         20);
        s.Points.AddXY("MAUI",        17);
        chart.Series.Add(s);
        return chart;
    }

    private static Chart BuildDoughnutChart()
    {
        var chart = MakeChart("Budget Allocation — Doughnut");
        chart.Legends.Add(new Legend("Legend1") { Enabled = true, Docking = Docking.Bottom });

        var s = new Series("Budget") { ChartType = SeriesChartType.Doughnut };
        s.Points.AddXY("Engineering", 40);
        s.Points.AddXY("Marketing",   20);
        s.Points.AddXY("Support",     15);
        s.Points.AddXY("Operations",  25);
        chart.Series.Add(s);
        return chart;
    }

    private static Chart BuildRadarChart()
    {
        var chart = MakeChart("Skills Assessment — Radar");
        chart.Legends.Add(new Legend("Legend1") { Enabled = true });

        var alice = new Series("Alice") { ChartType = SeriesChartType.Radar, Color = "#e15759" };
        alice.Points.AddXY("C#",        90);
        alice.Points.AddXY("JS",        75);
        alice.Points.AddXY("DevOps",    60);
        alice.Points.AddXY("SQL",       85);
        alice.Points.AddXY("Design",    50);

        var bob = new Series("Bob") { ChartType = SeriesChartType.Radar, Color = "#4e79a7" };
        bob.Points.AddXY("C#",      70);
        bob.Points.AddXY("JS",      92);
        bob.Points.AddXY("DevOps",  80);
        bob.Points.AddXY("SQL",     60);
        bob.Points.AddXY("Design",  88);

        chart.Series.Add(alice);
        chart.Series.Add(bob);
        return chart;
    }

    private static Chart BuildScatterChart()
    {
        var chart = MakeChart("Height vs Weight — Scatter");
        chart.Legends.Add(new Legend("Legend1") { Enabled = false });

        var s = new Series("People") { ChartType = SeriesChartType.Scatter, Color = "#59a14f" };
        var rng = new Random(42);
        for (int i = 0; i < 30; i++)
            s.Points.AddXY(150 + rng.NextDouble() * 40, 50 + rng.NextDouble() * 50);
        chart.Series.Add(s);
        return chart;
    }

    private static Chart BuildBubbleChart()
    {
        var chart = MakeChart("Risk Analysis — Bubble");
        chart.Legends.Add(new Legend("Legend1") { Enabled = false });

        var s = new Series("Risks") { ChartType = SeriesChartType.Bubble, Color = "#af7aa1" };
        // DataPoint: XValue = probability, YValues[0] = impact, YValues[1] = size
        s.Points.Add(new DataPoint { XValue = 10, YValues = [20, 5] });
        s.Points.Add(new DataPoint { XValue = 30, YValues = [60, 12] });
        s.Points.Add(new DataPoint { XValue = 50, YValues = [40, 8] });
        s.Points.Add(new DataPoint { XValue = 70, YValues = [80, 18] });
        s.Points.Add(new DataPoint { XValue = 90, YValues = [30, 6] });
        chart.Series.Add(s);
        return chart;
    }

    private static Chart BuildStackedBarChart()
    {
        var chart = MakeChart("Stacked Sales — Stacked Bar");
        chart.Legends.Add(new Legend("Legend1") { Enabled = true, Docking = Docking.Bottom });

        var q1 = new Series("Q1") { ChartType = SeriesChartType.StackedBar };
        var q2 = new Series("Q2") { ChartType = SeriesChartType.StackedBar };
        var q3 = new Series("Q3") { ChartType = SeriesChartType.StackedBar };

        foreach (var region in new[] { "North", "South", "East", "West" })
        {
            q1.Points.AddXY(region, 100 + new Random(region.Length).Next(50));
            q2.Points.AddXY(region, 80  + new Random(region.Length + 1).Next(60));
            q3.Points.AddXY(region, 90  + new Random(region.Length + 2).Next(55));
        }

        chart.Series.Add(q1);
        chart.Series.Add(q2);
        chart.Series.Add(q3);
        return chart;
    }

    private static Chart BuildSplineChart()
    {
        var chart = MakeChart("Temperature Trend — Spline");
        chart.Legends.Add(new Legend("Legend1") { Enabled = false });

        var s = new Series("Temp °C") { ChartType = SeriesChartType.Spline, Color = "#f28e2c", BorderWidth = 3 };
        s.Points.AddXY("Jan", 2);  s.Points.AddXY("Feb", 4);  s.Points.AddXY("Mar", 9);
        s.Points.AddXY("Apr", 15); s.Points.AddXY("May", 21); s.Points.AddXY("Jun", 26);
        s.Points.AddXY("Jul", 29); s.Points.AddXY("Aug", 28); s.Points.AddXY("Sep", 23);
        s.Points.AddXY("Oct", 16); s.Points.AddXY("Nov", 9);  s.Points.AddXY("Dec", 4);
        chart.Series.Add(s);
        return chart;
    }
}
