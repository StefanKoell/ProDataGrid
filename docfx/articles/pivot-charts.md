# Pivot Chart Model

`PivotChartModel` converts pivot output into chart-friendly series and category lists. Bind it to your chart control or use it to build custom visuals.

## Basic setup

```csharp
var pivot = new PivotTableModel
{
    ItemsSource = sales
};

var salesField = new PivotValueField
{
    Header = "Sales",
    ValueSelector = item => ((Sale)item!).Sales,
    AggregateType = PivotAggregateType.Sum
};

pivot.RowFields.Add(new PivotAxisField
{
    Header = "Region",
    ValueSelector = item => ((Sale)item!).Region
});

pivot.ColumnFields.Add(new PivotAxisField
{
    Header = "Year",
    ValueSelector = item => ((Sale)item!).OrderDate,
    GroupSelector = value => value is DateTime date ? date.Year : null
});

pivot.ValueFields.Add(salesField);

var chart = new PivotChartModel
{
    Pivot = pivot,
    SeriesSource = PivotChartSeriesSource.Rows,
    ValueField = salesField
};
```

`PivotChartModel` listens to `PivotTableModel.PivotChanged` and refreshes automatically.

## Options

- `SeriesSource`: `Rows` or `Columns` to flip series/category axes.
- `IncludeSubtotals` and `IncludeGrandTotals` to include total rows/columns.
- `ValueField` to chart a specific value field when multiple measures exist.
- `IncludeEmptySeries` to keep series that contain only null values.

## Binding

`PivotChartModel` exposes:

- `Categories`: the category labels in axis order.
- `Series`: a list of `PivotChartSeries` with `Name` and numeric `Values`.

## Sample

Run the sample app and open the "Pivot Chart Model" tab to see the generated series and category lists.
