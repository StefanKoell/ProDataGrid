# Pivot Tables

Pivot tables are available through the model-driven `PivotTableModel` in `Avalonia.Controls.DataGridPivoting`. The model builds a result set with rows, columns, and column definitions that you bind to a `DataGrid`, allowing Excel-style layouts, totals, and display modes while keeping the grid virtualized.

## Basic setup

Create a model, define fields, then bind the grid to the model outputs.

```csharp
using Avalonia.Controls.DataGridPivoting;

var model = new PivotTableModel
{
    ItemsSource = sales
};

model.RowFields.Add(new PivotAxisField
{
    Header = "Region",
    ValueSelector = item => ((Sale)item!).Region
});

model.ColumnFields.Add(new PivotAxisField
{
    Header = "Year",
    ValueSelector = item => ((Sale)item!).OrderDate,
    GroupSelector = value => value is DateTime date ? date.Year : null
});

model.ValueFields.Add(new PivotValueField
{
    Header = "Sales",
    ValueSelector = item => ((Sale)item!).Sales,
    AggregateType = PivotAggregateType.Sum
});

model.Layout.RowLayout = PivotRowLayout.Compact;
model.Layout.ValuesPosition = PivotValuesPosition.Columns;
model.Layout.ShowRowGrandTotals = true;
model.Layout.ShowColumnGrandTotals = true;

pivotGrid.AutoGenerateColumns = false;
pivotGrid.ItemsSource = model.Rows;
pivotGrid.ColumnDefinitionsSource = model.ColumnDefinitions;
```

XAML binding is just as simple:

```xml
<DataGrid ItemsSource="{Binding Pivot.Rows}"
          ColumnDefinitionsSource="{Binding Pivot.ColumnDefinitions}"
          AutoGenerateColumns="False" />
```

## Fields and grouping

- `PivotAxisField` is used for row and column axes. Use `ValueSelector` or `PropertyPath` to pick a value, and `GroupSelector` to group by calculated keys (such as year, quarter, or buckets).
- `SortDirection` and `Comparer` control ordering per field.
- `Filter` accepts a `PivotFieldFilter` (include/exclude sets or a predicate) to trim the source items.
- `ValueFilter` filters row/column items by aggregated values (Top/Bottom N, thresholds, or percent).
- `ValueSort` orders row/column items by aggregated values.
- `ShowItemsWithNoData` plus `ItemsSource` lets you include members that have no data (provide the full list of group values for that field). Set `ApplyGroupSelectorToItemsSource` if you want the field's `GroupSelector` applied to the supplied values.
- `SubtotalPosition` controls whether a field's subtotal appears before or after its children.

## Value fields and display modes

`PivotValueField` defines the aggregation and display behavior for each value column.

- `AggregateType` selects built-in aggregation (Sum, Average, Count, Min, Max, etc).
- `CustomAggregator` lets you plug in your own `IPivotAggregator`.
- `DisplayMode` switches to percent of row total, column total, or grand total.
- `StringFormat` and `Converter` control formatting.

## Calculated measures

Use `PivotValueField.Formula` to define Excel-style calculated measures evaluated after aggregation. See [Pivot Calculated Measures](pivot-calculated-measures.md) for syntax, total functions, and examples.

To register a custom aggregator globally, use the registry:

```csharp
model.Aggregators.Register(new MyCustomAggregator());
```

### Value filters and sorting

Value filters and value sorting align with Excel's "Value Filters" and "Sort by Values" features.

```csharp
var salesField = new PivotValueField
{
    Header = "Sales",
    ValueSelector = item => ((Sale)item!).Sales,
    AggregateType = PivotAggregateType.Sum
};

var categoryField = new PivotAxisField
{
    Header = "Category",
    ValueSelector = item => ((Sale)item!).Category,
    ValueFilter = new PivotValueFilter
    {
        FilterType = PivotValueFilterType.Top,
        Count = 3,
        ValueField = salesField
    }
};

var regionField = new PivotAxisField
{
    Header = "Region",
    ValueSelector = item => ((Sale)item!).Region,
    ValueSort = new PivotValueSort
    {
        ValueField = salesField,
        SortDirection = ListSortDirection.Descending
    }
};
```

Value filters apply to row/column items based on their grand total across the opposite axis (row totals for row fields, column totals for column fields).

## Layout and totals

`PivotLayoutOptions` controls Excel-style layout choices:

- `RowLayout`: Compact or Tabular.
- `ValuesPosition`: Values in columns or rows.
- `ShowRowSubtotals`, `ShowColumnSubtotals`, `ShowRowGrandTotals`, `ShowColumnGrandTotals`.
- `RowGrandTotalPosition` and `ColumnGrandTotalPosition`.
- `RepeatRowLabels` for tabular layouts.
- Label customization via `RowHeaderLabel`, `ValuesHeaderLabel`, `GrandTotalLabel`, `SubtotalLabelFormat`, and `EmptyValueLabel`.

## Advanced show values as

Excel-style "Show Values As" modes are available in addition to percent-of totals:

- `PivotValueDisplayMode.RunningTotal`
- `PivotValueDisplayMode.DifferenceFromPrevious`
- `PivotValueDisplayMode.PercentDifferenceFromPrevious`
- `PivotValueDisplayMode.PercentOfParentRowTotal`
- `PivotValueDisplayMode.PercentOfParentColumnTotal`
- `PivotValueDisplayMode.Index`

Sequence-based modes operate along the column axis and apply to detail columns; totals continue to show the raw aggregate value. If you need row-axis sequence calculations, use a computed field or custom aggregator.

## Theming and templates

Pivot output uses template keys and style classes so it can be themed without custom columns:

- `DataGridPivotHeaderTemplate` renders column header stacks.
- `DataGridPivotRowHeaderTemplate` renders compact row headers.
- Style classes applied to `DataGridCell` and `DataGridColumnHeader`:
  - `pivot-row-header`
  - `pivot-values-header`
  - `pivot-value`
  - `pivot-subtotal`
  - `pivot-grandtotal`

Override templates or styles in your theme or page resources as needed.

## Virtualization

The pivot table model produces a flat set of rows and column definitions, and the `DataGrid` continues to use its built-in virtualization for display. For very large datasets, build the pivot output on a background thread and swap `ItemsSource` when ready.

## Sample

Run the sample app and open the "Pivot Tables", "Pivot Report Filter", "Pivot Values in Rows", "Pivot Percent of Row Total", "Pivot Show Items With No Data", "Pivot Value Filter", "Pivot Value Sort", "Pivot Running Total", "Pivot Difference From Previous", "Pivot Percent of Parent Total", or "Pivot Index" tabs for live pivot configuration examples.
