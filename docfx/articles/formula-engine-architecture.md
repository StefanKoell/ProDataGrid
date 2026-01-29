# Formula Engine Architecture

The formula engine is split into a small core and an Excel compatibility layer so it can be reused in multiple contexts.

## Projects and responsibilities

- `ProDataGrid.FormulaEngine` (core):
  - Value model (`FormulaValue`, `FormulaArray`).
  - Expression tree (`FormulaExpression`).
  - Dependency graph and recalculation (`FormulaDependencyGraph`, `FormulaCalculationEngine`).
  - Workbook abstractions (`IFormulaWorkbook`, `IFormulaWorksheet`, `IFormulaCell`).
- `ProDataGrid.FormulaEngine.Excel` (Excel compatibility):
  - Tokenizer and parser (`ExcelFormulaTokenizer`, `ExcelFormulaParser`).
  - Excel function registry (`ExcelFunctionRegistry`).
  - Excel-specific formatting and parsing helpers.
- `Avalonia.Controls.DataGrid` (integration):
  - DataGrid formula model and column definitions.
  - Formula editing and validation in the UI.

## Evaluation pipeline

1. Parse formula text into a `FormulaExpression`.
2. Register dependencies in `FormulaDependencyGraph`.
3. Recalculate affected cells in dependency order.
4. Evaluate expressions with `FormulaEvaluator` and a value resolver.
5. Return a `FormulaValue` and cache results on cells.

A simplified flow looks like this:

```text
Formula text -> Parser -> AST -> Dependency graph -> Recalc order -> Evaluator -> FormulaValue
```

## Workbook and resolver model

The engine does not depend on a specific grid implementation. It uses:

- `IFormulaWorkbook` to resolve worksheets and settings.
- `IFormulaWorksheet` to resolve sparse cells.
- `IFormulaCell` to hold formula text, parsed expression, and cached value.

The default resolver (`WorkbookValueResolver`) evaluates cell references, ranges, external workbook references, names, and structured references using these interfaces.

## Calculation settings

`FormulaCalculationSettings` controls parsing and evaluation behavior:

- Reference mode (A1 or R1C1).
- Culture-aware list and decimal separators.
- Date system (1900 or 1904).
- Automatic vs manual calculation.
- Dynamic array support and spill behavior.
- Iterative calculation options.

## Extensibility

- Custom function registries can be plugged in by implementing `IFormulaFunctionRegistry`.
- Custom resolvers can be used to read from alternative data sources.
- Integration layers (like DataGrid) can map rows and columns into the workbook model.
