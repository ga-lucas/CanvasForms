# Chart Control — Implementation Progress

> **Branch:** `additional_controls`  
> **Status:** ⚠️ Partial — core rendering pipeline complete, refinements pending

---

## Plan

| # | Task | Status |
|---|------|--------|
| 1 | `Chart.cs` — WinForms-compatible class (Series, ChartArea, Legend, Title collections) | ✅ Done |
| 2 | `ChartRenderer.razor` — Blazor component rendering `<canvas>` via JS interop | ✅ Done |
| 3 | `chart-bridge.js` — Converts .NET chart model → Chart.js config, manages instance lifecycle | ✅ Done |
| 4 | ILTranslator retarget — `System.Windows.Forms.DataVisualization.Charting` → `Canvas.Windows.Forms` | ✅ Done |
| 5 | `ChartDemoForm.cs` — TabControl demo with one tab per chart type | ✅ Done |
| 6 | `WelcomeForm.cs` — "Charts Demo" button registered | ✅ Done |
| 7 | `README.md` roadmap updated | ✅ Done |
| 8 | `COMPATIBILITY_REVIEW.md` Chart section | ⏳ Pending |
| 9 | Axis title rendering (AxisX.Title / AxisY.Title) | ⏳ Pending |
| 10 | Mixed chart types (multiple `ChartType` values across Series) | ⏳ Pending |
| 11 | `DataPoint.Color` per-point overrides for non-polar charts | ✅ Done |
| 12 | `DataPoint.Label` callout rendering | ⏳ Pending |
| 13 | Candlestick / Stock chart type (OHLC data) | ⏳ Pending |
| 14 | Chart image export (`SaveImage`) | ⏳ Pending |

---

## Files Created / Modified

### New files

| File | Purpose |
|------|---------|
| `WebForms.Canvas/Forms/Display/Chart.cs` | WinForms-compatible `Chart` control class |
| `WebForms.Canvas/wwwroot/chart-bridge.js` | JS bridge: .NET model JSON → Chart.js 4 config |
| `WebForms.Canvas/Components/ChartRenderer.razor` | Blazor DOM overlay component for chart canvas |
| `WebForms.Canvas/Samples/ChartDemoForm.cs` | 11-tab demo form (one tab per chart type) |

### Modified files

| File | Change |
|------|--------|
| `WebForms.Canvas/Components/FormRenderer.razor` | Added `GetCharts()` / `CollectCharts()` overlay loop + `PropagateChartRenderCallback` |
| `WebForms.Canvas.Host/wwwroot/index.html` | Added Chart.js CDN + `chart-bridge.js` script tags |
| `Canvas.Windows.Forms.ILTranslator/Program.cs` | Added `DataVisualization.Charting` to assembly retarget map |
| `WebForms.Canvas/Samples/WelcomeForm.cs` | Added "Charts Demo" launch button (Row 6, col 3) |
| `README.md` | Moved `Chart` from Not-yet-implemented → Tier 3 roadmap as `⚠️` |

---

## API Coverage

### `Chart` class (`System.Windows.Forms.Chart`)

| Member | Status | Notes |
|--------|--------|-------|
| `Series` collection | ✅ | `SeriesCollection` — add/remove/clear |
| `ChartAreas` collection | ✅ | `ChartAreaCollection`; default `ChartArea1` always present |
| `Legends` collection | ✅ | `LegendCollection`; default `Legend1` always present |
| `Titles` collection | ✅ | `TitleCollection` |
| `BackColor` | ✅ | Via base `Control.BackColor`; propagated to overlay div |
| `Width` / `Height` / `Left` / `Top` | ✅ | Via base `Control` |
| `Visible` | ✅ | Hides overlay div |
| `Invalidate()` | ✅ | Triggers `ChartInvalidated` → `StateHasChanged` → re-render |
| `SerializeModel()` | ✅ | Internal; produces JSON consumed by `chart-bridge.js` |
| `SaveImage()` | ❌ | Not yet implemented |

### `Series` class

| Member | Status | Notes |
|--------|--------|-------|
| `Name` | ✅ | Shown in legend |
| `ChartType` | ✅ | All 19 `SeriesChartType` values mapped |
| `Color` | ✅ | CSS color string; falls back to default palette |
| `BorderWidth` | ✅ | Line/border thickness |
| `ChartArea` | ✅ | Linked by name |
| `Legend` | ✅ | Linked by name |
| `IsVisibleInLegend` | ✅ | |
| `Points` | ✅ | `DataPointCollection` |

### `DataPoint` class

| Member | Status | Notes |
|--------|--------|-------|
| `XValue` | ✅ | |
| `YValues` | ✅ | `double[]`; `YValues[1]` used as bubble radius |
| `AxisLabel` | ✅ | Category label on X axis |
| `IsEmpty` | ✅ | Renders as `null` (gap in line) |
| `Color` | ✅ | Per-point colour for all chart types; series colour used as fallback |
| `Label` | ⏳ | Not yet surfaced in Chart.js config |
| `ToolTip` | ⏳ | Not yet wired |

### `ChartArea` class

| Member | Status | Notes |
|--------|--------|-------|
| `Name` | ✅ | |
| `AxisX.Title` / `AxisY.Title` | ⚠️ | Passed to Chart.js `scales.x/y.title` — display only when non-empty |
| `AxisX.Minimum` / `Maximum` | ✅ | Passed as `min`/`max` on X scale |
| `AxisY.Minimum` / `Maximum` | ✅ | Passed as `min`/`max` on Y scale |
| `AxisX.IsLogarithmic` / `AxisY.IsLogarithmic` | ✅ | Serialised + wired to Chart.js `type: 'logarithmic'` |
| `Area3DStyleEnable3D` | ❌ | No 3D support in Chart.js |

### `Legend` class

| Member | Status | Notes |
|--------|--------|-------|
| `Enabled` | ✅ | |
| `Docking` | ✅ | Maps to Chart.js `position`: top/bottom/left/right |
| `Name` | ✅ | |

### `Title` class

| Member | Status | Notes |
|--------|--------|-------|
| `Text` | ✅ | Shown via Chart.js `plugins.title` |
| `Docking` | ✅ | `top` and `bottom` mapped to Chart.js `position` |
| `Font` | ✅ | CSS font string parsed to Chart.js `font` object (size, family, weight, style) |

---

## Supported `SeriesChartType` values

| WinForms ChartType | Chart.js type | Notes |
|--------------------|---------------|-------|
| `Column` | `bar` | Default vertical bar |
| `Bar` | `bar` + `indexAxis: 'y'` | Horizontal bar |
| `StackedBar` | `bar` + `stacked` + `indexAxis: 'y'` | |
| `StackedBar100` | `bar` + `stacked` + `indexAxis: 'y'` | 100% normalised; Y axis forced 0–100% |
| `StackedColumn` | `bar` + `stacked` | |
| `StackedColumn100` | `bar` + `stacked` | 100% normalised; Y axis forced 0–100% |
| `Line` | `line` | |
| `Spline` | `line` + `tension: 0.4` | |
| `Area` | `line` + `fill: true` | |
| `SplineArea` | `line` + `fill: true` + `tension: 0.4` | |
| `StepLine` | `line` + `stepped: true` | |
| `Pie` | `pie` | Per-point colours auto-assigned |
| `Doughnut` | `doughnut` | Per-point colours auto-assigned |
| `Radar` | `radar` | |
| `Scatter` | `scatter` | |
| `Point` | `scatter` | Alias for Scatter |
| `Bubble` | `bubble` | `YValues[1]` = radius |
| `Candlestick` | `bar` (fallback) | ⏳ Needs Chart.js financial plugin |
| `Stock` | `bar` (fallback) | ⏳ Needs Chart.js financial plugin |

---

## Architecture

```
.NET Chart control (Chart.cs)
  │  SerializeModel() → JSON
  │
  ▼
ChartRenderer.razor (Blazor overlay)
  │  JSRuntime.InvokeVoidAsync("chartBridge.init", id, json)
  │
  ▼
chart-bridge.js
  │  buildConfig(model) → Chart.js config object
  │
  ▼
Chart.js 4 (CDN: cdn.jsdelivr.net/npm/chart.js@4)
  │  new Chart(canvas, config) / chart.update()
  ▼
<canvas> DOM element (absolutely positioned overlay on top of the Blazor form canvas)
```

**Overlay positioning** follows the same pattern as `WebBrowser` iframes:
```
left  = Form.Left + BorderWidth + chart.Left
top   = Form.Top  + TitleBarHeight + BorderWidth + chart.Top
```

**Invalidation flow:**  
`Series/Points mutation` → `Collection.Changed` → `Chart.OnChartChanged()` → `ChartInvalidated event` → `FormRenderer.PropagateChartRenderCallback` → `InvokeAsync(StateHasChanged)` → `ChartRenderer.OnAfterRenderAsync` → `chartBridge.init(id, json)`

---

## ILTranslator Retargeting

Translated apps that reference `System.Windows.Forms.DataVisualization.Charting` are automatically remapped to `Canvas.Windows.Forms` by the assembly retarget pass in `Canvas.Windows.Forms.ILTranslator/Program.cs`:

```csharp
if (reference.Name is "System.Windows.Forms"
					or "System.Windows.Forms.Primitives"
					or "System.Windows.Forms.DataVisualization.Charting"  // ← added
					or "WebForms.Canvas")
{
	reference.Name = "Canvas.Windows.Forms";
}
```

The `Chart`, `Series`, `DataPoint`, `ChartArea`, `Legend`, and `Title` types all live in  
`System.Windows.Forms.DataVisualization.Charting` within the `Canvas.Windows.Forms` assembly, preserving the original WinForms namespace structure.

---

## Demo Form — `ChartDemoForm.cs`

Located: `WebForms.Canvas/Samples/ChartDemoForm.cs`  
Launched from: **WelcomeForm → "Charts Demo"** button (Row 6, col 3)

| Tab | Chart type | Data |
|-----|-----------|------|
| Line | `SeriesChartType.Line` | Monthly revenue, 2 series |
| Bar | `SeriesChartType.Bar` | Sales by region |
| Column | `SeriesChartType.Column` | Quarterly performance, 2 series |
| Area | `SeriesChartType.Area` | CPU usage over time |
| Pie | `SeriesChartType.Pie` | Market share, 4 segments |
| Doughnut | `SeriesChartType.Doughnut` | Budget allocation |
| Radar | `SeriesChartType.Radar` | Skills assessment, 2 people |
| Scatter | `SeriesChartType.Scatter` | Height vs weight, 30 points |
| Bubble | `SeriesChartType.Bubble` | Risk analysis, 5 bubbles |
| Stacked Bar | `SeriesChartType.StackedBar` | Stacked quarterly sales by region |
| Spline | `SeriesChartType.Spline` | Temperature trend (12 months) |

---

## Known Gaps / TODO

- [ ] Mixed `ChartType` across series in a single chart area — partially works via per-dataset `type` override; edge cases with scale sharing untested
- [ ] Candlestick / Stock — require `chartjs-chart-financial` plugin (OHLC data; `YValues[0..3]` = open/high/low/close)
- [ ] `Chart.SaveImage(stream, format)` — can be implemented via `canvas.toDataURL()` → download link
- [ ] `DataPoint.ToolTip` — not wired to Chart.js tooltip callbacks
- [ ] `ChartArea.AxisX2` / `AxisY2` (secondary axes) — not yet mapped to Chart.js `y2` scale
- [ ] Multiple `ChartArea` support — only first area used; multi-area layout not supported by Chart.js natively
