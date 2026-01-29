# Formula Engine Integration

The formula engine integrates with ProDataGrid and charting so formulas can drive grid values and series data. The engine is UI-agnostic and can be wired to custom data sources through the workbook abstraction.

## DataGrid integration

`DataGrid` exposes a `FormulaModel` property. The default implementation is `DataGridFormulaModel`, which:

- Tracks formulas and dependencies per cell.
- Applies recalculation to affected rows only.
- Supports dynamic arrays and spill ranges.
- Exposes validation errors for inline editing.

### Formula columns

Use `DataGridFormulaColumnDefinition` to add computed columns:

```csharp
var definition = new DataGridFormulaColumnDefinition
{
    Header = "Total",
    Formula = "=[Price] * [Quantity]",
    AllowCellFormulas = true
};
```

Formula columns use structured references (e.g., `[@Price]`) when a row context is available.

### Per-cell formulas

When `AllowCellFormulas` is enabled, users can edit individual cell formulas. `DataGridFormulaTextColumn` ensures the edit box shows the raw formula and commits it to the formula model.

### Recalculation and invalidation

The formula model listens for:

- Item changes (INotifyPropertyChanged).
- Collection changes (INotifyCollectionChanged).
- Column definition updates.

It translates these into dirty ranges and triggers recalculation through `FormulaCalculationEngine`.

## Charting integration

`DataGridChartModel` supports formulas per series using `DataGridChartSeriesDefinition.Formula`. Series formulas are evaluated by the formula engine for each item in the series.

This allows charts to consume:

- Calculated measures.
- Derived values from multiple fields.
- Culture-aware parsing and numeric coercion.

## Custom integrations

If you need a different data source, implement:

- `IFormulaWorkbook`
- `IFormulaWorksheet`
- `IFormulaCell`

Then use `FormulaEvaluator` or `FormulaCalculationEngine` to parse and evaluate formulas in your own model.

## Samples

The sample app includes formula engine pages:

```bash
dotnet run --project src/DataGridSample/DataGridSample.csproj
```

Recommended pages:

- Formula Engine samples (A1/R1C1, errors, arrays)
- Formula columns (A1 and structured refs)
- Formula editing and spills
- Calculated measures (chart formulas)
