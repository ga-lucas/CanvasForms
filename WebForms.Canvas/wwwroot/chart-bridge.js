// chart-bridge.js — Converts serialised .NET Chart model → Chart.js config
// and manages chart instance lifecycle.
// Loaded as: _content/Canvas.Windows.Forms/chart-bridge.js

'use strict';

(function () {
    if (window.chartBridge) return; // idempotent

    // Map of chartId → Chart.js instance
    const _instances = {};

    // Default palette used when a series has no explicit colour.
    const DEFAULT_PALETTE = [
        '#4e79a7', '#f28e2c', '#e15759', '#76b7b2',
        '#59a14f', '#edc949', '#af7aa1', '#ff9da7',
        '#9c755f', '#bab0ab',
    ];

    // ── Register datalabels plugin if available ───────────────────────────────
    // chartjs-plugin-datalabels is optional; loaded via CDN before this script.
    if (window.ChartDataLabels) {
        Chart.register(window.ChartDataLabels);
    }

    // ── 100% stacking normalisation ───────────────────────────────────────────

    /**
     * Normalise dataset values to percentages for StackedBar100 / StackedColumn100.
     * Mutates the datasets array in-place.
     */
    function normalise100(datasets, labels) {
        const count = labels.length;
        for (let i = 0; i < count; i++) {
            let total = 0;
            for (const ds of datasets) total += Math.abs(ds.data[i] ?? 0);
            if (total === 0) continue;
            for (const ds of datasets) {
                ds.data[i] = ds.data[i] != null
                    ? Math.round((ds.data[i] / total) * 1000) / 10   // 1 decimal place
                    : null;
            }
        }
    }

    // ── Type mapping ──────────────────────────────────────────────────────────

    /**
     * Map a WinForms SeriesChartType string to Chart.js type + stacking/fill options.
     * Returns { type, stacked, fill, indexAxis }
     */
    function mapType(chartType) {
        switch (chartType) {
            case 'Bar':              return { type: 'bar',    indexAxis: 'y' };
            case 'Column':           return { type: 'bar' };
            case 'StackedBar':       return { type: 'bar',    indexAxis: 'y', stacked: true };
            case 'StackedBar100':    return { type: 'bar',    indexAxis: 'y', stacked: true, pct: true };
            case 'StackedColumn':    return { type: 'bar',    stacked: true };
            case 'StackedColumn100': return { type: 'bar',    stacked: true, pct: true };
            case 'Line':             return { type: 'line' };
            case 'Spline':           return { type: 'line',   tension: 0.4 };
            case 'Area':             return { type: 'line',   fill: true };
            case 'SplineArea':       return { type: 'line',   fill: true, tension: 0.4 };
            case 'StepLine':         return { type: 'line',   stepped: true };
            case 'Pie':              return { type: 'pie' };
            case 'Doughnut':         return { type: 'doughnut' };
            case 'Radar':            return { type: 'radar' };
            case 'Scatter':          return { type: 'scatter' };
            case 'Bubble':           return { type: 'bubble' };
            case 'Point':            return { type: 'scatter' };
            default:                 return { type: 'bar' };
        }
    }

    // ── Legend position mapping ───────────────────────────────────────────────

    function mapLegendPosition(docking) {
        switch (docking) {
            case 'bottom': return 'bottom';
            case 'left':   return 'left';
            case 'right':  return 'right';
            default:       return 'top';
        }
    }

    // ── Title font parser ─────────────────────────────────────────────────────

    /**
     * Convert a WinForms/CSS font string like "11pt Segoe UI" or "bold 14px Arial"
     * into a Chart.js font object { size, family, weight, style }.
     */
    function parseTitleFont(fontStr) {
        if (!fontStr) return undefined;
        const font = {};
        // size — match "12pt", "14px", "1em" etc.
        const sizeMatch = fontStr.match(/(\d+(?:\.\d+)?)(pt|px|em|rem)/i);
        if (sizeMatch) {
            let px = parseFloat(sizeMatch[1]);
            if (sizeMatch[2].toLowerCase() === 'pt') px = Math.round(px * 96 / 72);
            font.size = px;
        }
        if (/bold/i.test(fontStr))   font.weight = 'bold';
        if (/italic/i.test(fontStr)) font.style  = 'italic';
        // family — last token(s) after size
        const familyMatch = fontStr.match(/(?:pt|px|em|rem)\s+(.+)$/i);
        if (familyMatch) font.family = familyMatch[1].trim();
        return Object.keys(font).length ? font : undefined;
    }

    // ── Build Chart.js config from model ─────────────────────────────────────

    function buildConfig(model) {
        const seriesList  = model.series     || [];
        const areaList    = model.chartAreas || [];
        const legendList  = model.legends    || [];
        const titleList   = model.titles     || [];

        if (seriesList.length === 0) {
            // Empty placeholder
            return {
                type: 'bar',
                data: { labels: [], datasets: [] },
                options: {},
            };
        }

        // All series share the same "primary" Chart.js type from the first series.
        const primaryType = mapType(seriesList[0].chartType || 'Column');
        const chartJsType = primaryType.type;

        const isPolar   = chartJsType === 'pie' || chartJsType === 'doughnut';
        const isRadar   = chartJsType === 'radar';
        const isScatter = chartJsType === 'scatter' || chartJsType === 'bubble';

        // Build labels from the first series with axisLabels, falling back to XValues
        let labels = [];
        if (!isScatter) {
            for (const s of seriesList) {
                if (s.points && s.points.length > 0) {
                    const candidate = s.points.map(p => p.axisLabel ?? String(p.xValue ?? ''));
                    if (candidate.length > labels.length) labels = candidate;
                }
            }
        }

        // Build datasets
        const datasets = seriesList.map((s, i) => {
            const typeInfo  = mapType(s.chartType || 'Column');
            const color     = s.color || DEFAULT_PALETTE[i % DEFAULT_PALETTE.length];
            const points    = s.points || [];

            let data;
            if (isScatter) {
                data = points.map(p => {
                    if (s.chartType === 'Bubble') {
                        const r = p.yValues && p.yValues.length > 1 ? p.yValues[1] : 5;
                        return { x: p.xValue, y: p.yValues?.[0] ?? 0, r };
                    }
                    return { x: p.xValue, y: p.yValues?.[0] ?? 0 };
                });
            } else {
                data = points.map(p =>
                    p.isEmpty ? null : (p.yValues?.[0] ?? 0)
                );
            }

            // Per-point colours: polar types always use per-point colours;
            // non-polar types use per-point colour when DataPoint.Color is set,
            // otherwise fall back to the series colour.
            let backgroundColor;
            if (isPolar) {
                backgroundColor = points.map((p, j) =>
                    p.color || DEFAULT_PALETTE[j % DEFAULT_PALETTE.length]);
            } else if (!isScatter && points.some(p => p.color)) {
                // Mix: use per-point colour where set, series colour elsewhere
                backgroundColor = points.map(p =>
                    (p.color || color) + (typeInfo.fill ? '55' : '99'));
            } else {
                backgroundColor = color + (typeInfo.fill ? '55' : '99');
            }

            // DataPoint.Label → Chart.js datalabel (used by chartjs-plugin-datalabels)
            const hasLabels = points.some(p => p.label);

            const dataset = {
                label:           s.name || `Series ${i + 1}`,
                data,
                backgroundColor,
                borderColor:     color,
                borderWidth:     s.borderWidth ?? 2,
                hidden:          !s.isVisibleInLegend,
                tension:         typeInfo.tension   ?? 0,
                fill:            typeInfo.fill      ?? false,
                stepped:         typeInfo.stepped   ?? false,
                spanGaps:        true,
            };

            if (hasLabels && window.ChartDataLabels) {
                dataset.datalabels = {
                    labels: { title: { formatter: (_, ctx) => points[ctx.dataIndex]?.label || null } },
                };
            }

            // Dataset-level type override (mixed charts)
            if (typeInfo.type !== chartJsType) dataset.type = typeInfo.type;

            return dataset;
        });

        // Apply 100% normalisation for stacked-100 types
        if (primaryType.pct && !isScatter) {
            normalise100(datasets, labels);
        }

        // Scale / axes from first chart area
        const area   = areaList[0] || {};
        const scales = {};

        if (!isPolar && !isRadar) {
            const xLogarithmic = area.axisXLogarithmic ?? false;
            const yLogarithmic = area.axisYLogarithmic ?? false;

            scales.x = {
                type:    xLogarithmic ? 'logarithmic' : 'category',
                stacked: primaryType.stacked ?? false,
                title:   area.axisXTitle ? { display: true, text: area.axisXTitle } : undefined,
            };
            // For bar/column/stacked the X axis is category-based; override type for scatter
            if (isScatter) scales.x.type = xLogarithmic ? 'logarithmic' : 'linear';

            scales.y = {
                type:    yLogarithmic ? 'logarithmic' : 'linear',
                stacked: primaryType.stacked ?? false,
                title:   area.axisYTitle ? { display: true, text: area.axisYTitle } : undefined,
                min:     area.axisYMinimum ?? undefined,
                max:     area.axisYMaximum ?? undefined,
            };

            if (primaryType.pct) {
                // 100% stacked: force Y axis 0-100 with % ticks
                scales.y.min = 0;
                scales.y.max = 100;
                scales.y.ticks = { callback: v => v + '%' };
                if (primaryType.indexAxis === 'y') {
                    // Horizontal 100%: the value axis is X
                    scales.x.min = 0;
                    scales.x.max = 100;
                    scales.x.ticks = { callback: v => v + '%' };
                    delete scales.y.min; delete scales.y.max; delete scales.y.ticks;
                }
            }
        }

        // Legend
        const legendCfg = { display: false };
        if (legendList.length > 0 && legendList[0].enabled) {
            legendCfg.display  = true;
            legendCfg.position = mapLegendPosition(legendList[0].docking);
        }

        // Titles — first title drives the Chart.js plugin.title config
        const firstTitle = titleList.find(t => t.text);
        const titleCfg   = firstTitle
            ? {
                display:  true,
                text:     firstTitle.text,
                position: firstTitle.docking === 'bottom' ? 'bottom' : 'top',
                font:     parseTitleFont(firstTitle.font),
              }
            : { display: false };

        const options = {
            responsive:          true,
            maintainAspectRatio: false,
            animation:           { duration: 300 },
            plugins: {
                legend:     legendCfg,
                title:      titleCfg,
                datalabels: window.ChartDataLabels
                    ? { display: ctx => !!(ctx.dataset.datalabels) }
                    : undefined,
            },
        };

        if (!isPolar && !isRadar && Object.keys(scales).length > 0) {
            options.scales = scales;
        }

        if (primaryType.indexAxis) options.indexAxis = primaryType.indexAxis;

        return {
            type:    chartJsType,
            data:    { labels, datasets },
            options,
        };
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /**
     * Initialise or update a Chart.js instance.
     * @param {string}  canvasId  — id of the <canvas> element
     * @param {string}  modelJson — JSON-serialised .NET Chart model
     */
    function init(canvasId, modelJson) {
        let model;
        try { model = JSON.parse(modelJson); } catch (e) {
            console.error('[chart-bridge] Failed to parse model JSON', e);
            return;
        }

        _initWithModel(canvasId, model, 0);
    }

    function _initWithModel(canvasId, model, attempt) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            if (attempt < 10) {
                setTimeout(() => _initWithModel(canvasId, model, attempt + 1), 50);
            } else {
                console.warn('[chart-bridge] Canvas not found after retries:', canvasId);
            }
            return;
        }

        const config = buildConfig(model);

        if (_instances[canvasId]) {
            // Update existing chart
            const chart = _instances[canvasId];
            chart.data    = config.data;
            chart.options = config.options;
            chart.update();
        } else {
            // Destroy any stale Chart.js instance that may be attached to this canvas
            const stale = Chart.getChart(canvas);
            if (stale) stale.destroy();
            _instances[canvasId] = new Chart(canvas, config);
        }
    }

    /**
     * Destroy a Chart.js instance when the control is removed.
     * @param {string} canvasId
     */
    function destroy(canvasId) {
        if (_instances[canvasId]) {
            _instances[canvasId].destroy();
            delete _instances[canvasId];
        }
    }

    window.chartBridge = { init, destroy };
})();
