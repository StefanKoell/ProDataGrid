# ProCharts Rendering Pipeline

Rendering is based on immutable snapshots so renderers can stay stateless and fast.

## Snapshot flow

1. `ChartModel` asks the data source for a `ChartDataSnapshot`.
2. The renderer lays out axes, plot area, labels, and legend.
3. Series geometry is drawn into cached layers.

## Incremental updates

`ChartDataUpdate` includes a `ChartDataDelta`. Renderers can use it to update only the changed geometry instead of rebuilding everything.

## Skia renderer

`SkiaChartRenderer` is the default renderer. It supports:

- Series drawing for all `ChartSeriesKind` values.
- Axes, gridlines, and labels.
- Legend layout and data labels.
- Hit testing for tooltips and selection.

`SkiaChartRenderCache` stores rendered layers (axes, legend, data, labels) to avoid repeated work when only parts of the chart change.

## Avalonia integration

`ProChartView` hosts a renderer and maintains a bitmap buffer. It listens to `ChartModel.SnapshotUpdated` to invalidate only when needed.
