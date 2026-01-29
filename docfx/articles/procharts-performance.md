# ProCharts Performance

ProCharts is designed for high-volume data while keeping interaction responsive.

## Windowed rendering

Use `ChartDataRequest.WindowStart` and `WindowCount` to render a slice of the data. This is ideal for streaming or timeline charts.

```csharp
model.Request.WindowStart = 5000;
model.Request.WindowCount = 1000;
```

## Downsampling

`ChartDataRequest.MaxPoints` and `DownsampleMode` control downsampling:

- `Bucket`: simple aggregation per bucket.
- `MinMax`: preserves extrema within buckets.
- `Lttb`: Largest Triangle Three Buckets.
- `Adaptive`: selects a mode based on data density.

```csharp
model.Request.MaxPoints = 2000;
model.Request.DownsampleMode = ChartDownsampleMode.Lttb;
```

## Incremental updates

If your data source implements `IChartIncrementalDataSource`, it can return `ChartDataUpdate` with a `ChartDataDelta` so the renderer avoids full rebuilds.

## DataGrid-specific tuning

`DataGridChartModel` supports aggregation and group-aware series. For large datasets:

- Use aggregation (`Sum`, `Average`, `Min`, `Max`, `Count`).
- Configure `DownsampleMode` and `DownsampleAggregation` on the model.
- Combine grouping with windowing for interactive pivots.
