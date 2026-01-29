# ProCharts and ProDataGrid Integration

`ProDataGrid.Charting` provides adapters that build chart snapshots directly from DataGrid data and pivot results.

## DataGridChartModel

`DataGridChartModel` implements `IChartIncrementalDataSource` and exposes a series definition API:

```csharp
var chartModel = new ChartModel
{
    DataSource = new DataGridChartModel
    {
        ItemsSource = view, // DataGridCollectionView or any IEnumerable
        Series =
        {
            new DataGridChartSeriesDefinition
            {
                Name = "Revenue",
                ValuePath = "Revenue",
                Kind = ChartSeriesKind.Column
            }
        }
    }
};
```

Key features:

- Aggregation (`Sum`, `Average`, `Min`, `Max`, `Count`).
- Group-aware series via `GroupMode`.
- Incremental updates from `INotifyCollectionChanged` and `INotifyPropertyChanged`.
- Formula-driven series using `DataGridChartSeriesDefinition.Formula`.

## Pivot charts

`PivotChartDataSource` turns pivot table results into chart snapshots. This keeps charts in sync with:

- Pivot grouping.
- Calculated measures.
- Filtering and slicers.

## Formula-driven measures

Series can use formulas instead of raw values:

```csharp
new DataGridChartSeriesDefinition
{
    Name = "Gross Margin",
    Formula = "=[Revenue]-[Cost]",
    Kind = ChartSeriesKind.Line
}
```

This uses the formula engine under the hood and respects culture-aware parsing.
