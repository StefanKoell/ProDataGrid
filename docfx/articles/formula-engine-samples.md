# Formula Engine Samples

The sample app includes multiple pages that showcase the formula engine in action.

Run the sample app:

```bash
dotnet run --project src/DataGridSample/DataGridSample.csproj
```

## Recommended pages

- Formula Engine Samples
  - A1 and R1C1 references.
  - Sheet prefixes and error literals.
  - Operator precedence and arrays.
- Formula Columns (A1)
  - Formula columns bound to row data using A1 references.
- Formula Columns (Structured)
  - Structured references like `[@Price]` and `[@Quantity]`.
- Formula Editing Samples
  - Inline formula editing with validation.
  - Dynamic arrays and spill rendering.
- Formula Engine Integration
  - DataGrid edits triggering incremental recalculation.
  - Formula-driven summaries and derived values.

## Charting and formulas

The chart samples include series formulas that evaluate per item. This demonstrates how formulas can drive chart values without extra model code.
