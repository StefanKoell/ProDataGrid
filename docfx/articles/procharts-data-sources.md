# ProCharts Data Sources and Updates

Charts read data through `IChartDataSource`. Incremental sources can return deltas to update only the changed range.

## IChartDataSource

Implement `IChartDataSource` to supply a snapshot:

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

    public void Invalidate() => DataInvalidated?.Invoke(this, EventArgs.Empty);
}
```

`ChartModel` listens to `DataInvalidated` and refreshes when `AutoRefresh` is enabled.

## Incremental data sources

Implement `IChartIncrementalDataSource` to return a delta alongside the new snapshot:

```csharp
internal sealed class IncrementalSource : IChartIncrementalDataSource
{
    private readonly List<string?> _categories;
    private readonly List<double?> _values;
    private ChartDataDelta? _pending;

    public IncrementalSource(List<string?> categories, List<double?> values)
    {
        _categories = categories;
        _values = values;
    }

    public event EventHandler? DataInvalidated;

    public ChartDataSnapshot BuildSnapshot(ChartDataRequest request)
    {
        var series = new[] { new ChartSeriesSnapshot("Series", ChartSeriesKind.Line, _values) };
        return new ChartDataSnapshot(_categories, series);
    }

    public bool TryBuildUpdate(ChartDataRequest request, ChartDataSnapshot previousSnapshot, out ChartDataUpdate update)
    {
        if (_pending == null)
        {
            update = new ChartDataUpdate(BuildSnapshot(request), ChartDataDelta.Full);
            return false;
        }

        update = new ChartDataUpdate(BuildSnapshot(request), _pending);
        _pending = null;
        return true;
    }

    public void UpdatePoint(int index, double? value)
    {
        _values[index] = value;
        _pending = new ChartDataDelta(ChartDataDeltaKind.Update, index, 1, 1);
        DataInvalidated?.Invoke(this, EventArgs.Empty);
    }
}
```

## ChartDataDelta

`ChartDataDelta` describes what changed:

- `Insert`, `Remove`, `Replace`, `Move`, `Update`
- `Index`, `OldCount`, `NewCount`
- `SeriesIndices` for series-specific updates

Renderers can use deltas to refresh only the affected segments instead of rebuilding the entire chart.
