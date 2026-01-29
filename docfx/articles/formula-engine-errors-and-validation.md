# Formula Engine Errors and Validation

The engine uses Excel-style error values. Errors propagate through expressions and are surfaced in the DataGrid formula UI.

## Error types

`FormulaErrorType` maps to Excel error literals:

- `Div0` -> `#DIV/0!`
- `NA` -> `#N/A`
- `Name` -> `#NAME?`
- `Null` -> `#NULL!`
- `Num` -> `#NUM!`
- `Ref` -> `#REF!`
- `Value` -> `#VALUE!`
- `Spill` -> `#SPILL!`
- `Calc` -> `#CALC!`
- `Circ` -> `#CIRC!`

## Parse errors

Parsing failures raise `FormulaParseException`, which includes the error position. In DataGrid editing, parse errors are captured and stored on the cell so the UI can highlight invalid formulas.

## DataGrid validation

`DataGridFormulaModel.TrySetCellFormula` returns an error message when parsing fails. The sample app displays these errors inline so users can fix formulas quickly.

## Error propagation

Most operators and functions propagate errors from their arguments. Some functions (like `IFERROR`) explicitly handle errors and provide fallbacks.
