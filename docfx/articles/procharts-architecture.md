# ProCharts Architecture

ProCharts is split into a renderer-agnostic core and renderer-specific implementations.

## Projects

- `ProCharts` defines the model, data snapshots, and styling types.
- `ProCharts.Skia` renders snapshots with SkiaSharp and provides export helpers.
- `ProCharts.Avalonia` hosts `ProChartView` for interactive charts in Avalonia.
- `ProDataGrid.Charting` bridges ProDataGrid data and pivot results into chart models.

## Core pipeline

1. A `ChartModel` owns axes, legend settings, and a data source.
2. The data source produces a `ChartDataSnapshot`.
3. A renderer draws the snapshot using a style configuration.
4. `ChartDataDelta` enables incremental updates when possible.

## Renderer independence

The core model uses `ChartTheme` and `ChartSeriesStyle` so multiple renderers can share the same styling metadata. Renderer-specific styles (like `SkiaChartStyle`) adapt the core values to drawing primitives.

## Data sources

A chart can build its data from:

- A custom `IChartDataSource`.
- An incremental source that implements `IChartIncrementalDataSource`.
- `DataGridChartModel` or `PivotChartDataSource` for grid-driven charts.

## Key types

- `ChartModel`, `ChartDataRequest`, `ChartDataSnapshot`
- `ChartAxisDefinition`, `ChartLegendDefinition`
- `ChartSeriesSnapshot`, `ChartDataDelta`, `ChartDataUpdate`
