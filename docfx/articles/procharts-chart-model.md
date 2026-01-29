# ProCharts Chart Model

`ChartModel` is the central configuration object. It owns axes, legend settings, and a data source.

## Basic usage

```csharp
var dataSource = new SimpleDataSource(
    new[] { "Q1", "Q2", "Q3", "Q4" },
    new[]
    {
        new ChartSeriesSnapshot("Revenue", ChartSeriesKind.Column, new double?[] { 10, 12, 8, 15 })
    });

var model = new ChartModel { DataSource = dataSource };
```

`SimpleDataSource` is a minimal `IChartDataSource` you can implement yourself. Any `IChartDataSource` works.

```csharp
internal sealed class SimpleDataSource : IChartDataSource
{
    private readonly ChartDataSnapshot _snapshot;

    public SimpleDataSource(IReadOnlyList<string?> categories, IReadOnlyList<ChartSeriesSnapshot> series)
    {
        _snapshot = new ChartDataSnapshot(categories, series);
    }

    public event EventHandler? DataInvalidated;

    public ChartDataSnapshot BuildSnapshot(ChartDataRequest request) => _snapshot;
}
```

## Data requests

`ChartDataRequest` controls windowing and downsampling:

- `MaxPoints`: limit points per series.
- `DownsampleMode`: `Bucket`, `MinMax`, `Lttb`, or `Adaptive`.
- `WindowStart` / `WindowCount`: slice the category range.

```csharp
model.Request.MaxPoints = 2000;
model.Request.WindowStart = 5000;
model.Request.WindowCount = 1000;
```

## Axes and legend

`ChartModel` exposes four axis definitions:

- `CategoryAxis` / `SecondaryCategoryAxis`
- `ValueAxis` / `SecondaryValueAxis`

Legend settings live in `ChartLegendDefinition`:

```csharp
model.Legend.Position = ChartLegendPosition.Bottom;
```

## Snapshots and updates

- `ChartDataSnapshot` contains resolved series and categories.
- `ChartDataUpdate` includes a `ChartDataDelta` so renderers can update incrementally.
- `ChartModel.SnapshotUpdated` fires for all updates.

```csharp
model.SnapshotUpdated += (_, e) =>
{
    var delta = e.Update.Delta;
    // Use delta.Kind to decide if a full refresh is needed.
};
```
