# Formula Engine Names and Structured References

The engine supports workbook and worksheet names plus structured references used by table-like data sources.

## Named ranges

Names are resolved through `IFormulaNameProvider` and can be defined at two scopes:

- Workbook scope.
- Worksheet scope.

The dependency graph tracks name dependencies by scope so updates only recalc affected formulas.

## Structured references

Structured references provide table-style access to columns and scopes:

- `Table1[Amount]`
- `[@Amount]` (this row)
- `Table1[[#Headers],[Amount]]`
- `Table1[[#Totals],[Amount]]`
- `Table1[[#Data],[Amount]]`
- `Table1[[#All],[Amount]]`

The parser recognizes the standard Excel scopes:

- `#Headers`, `#Totals`, `#Data`, `#All`, `#ThisRow`

## DataGrid mapping

`DataGridFormulaModel` resolves structured references to DataGrid columns.

- `[@Column]` resolves to the current row.
- Column ranges resolve to arrays for the full column range.
- `#Headers` returns header values.
- `#Totals` returns totals row values when present.

The resolver matches the table name against the DataGrid name or workbook name, so you can use either `Table1[...]` or `[@Column]` depending on your context.

## Dependencies

Structured references are expanded into dependencies when they point at full columns or ranges. Row-only references (`#Headers`, `#Totals`, `#ThisRow`) do not add per-row dependencies to keep recalculation efficient.
