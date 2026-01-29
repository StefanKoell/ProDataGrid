# ProCharts Series Types

Series types are defined by `ChartSeriesKind`. Each series snapshot also includes optional X values, size values, and styles.

## Cartesian series

- `Line`, `Area`, `Column`, `Bar`
- `Scatter`, `Bubble`
- `StackedColumn`, `StackedBar`, `StackedArea`
- `StackedColumn100`, `StackedBar100`, `StackedArea100`
- `Waterfall`
- `Histogram`, `Pareto`
- `Radar`
- `BoxWhisker`
- `Funnel`

## Pie and donut

- `Pie`
- `Donut`

Pie and donut series ignore category axes and render labels around the arc.

## X/Y series

`Scatter` and `Bubble` series can use explicit `XValues`. Bubble series can also use `SizeValues`.

```csharp
new ChartSeriesSnapshot(
    name: "Speed",
    kind: ChartSeriesKind.Scatter,
    values: new double?[] { 4, 6, 7, 3 },
    xValues: new double[] { 1, 2, 3, 4 })
```

## Trendlines and error bars

`ChartSeriesSnapshot` supports trendlines and error bars:

- `TrendlineType`: `Linear`, `Exponential`, `Logarithmic`, `Polynomial`, `Power`, `MovingAverage`.
- `ErrorBarType`: `Fixed`, `Percentage`, `StandardDeviation`, `StandardError`.

Use `ChartSeriesSnapshot` parameters to enable them per series.
