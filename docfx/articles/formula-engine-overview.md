# Formula Engine Overview

The formula engine provides Excel-compatible parsing, evaluation, and recalculation for ProDataGrid. It supports A1 and R1C1 references, sheet prefixes (including external workbooks), structured references, arrays, dynamic spills, and Excel-style errors.

This section is split into focused articles:

- Architecture and core types: `formula-engine-architecture.md`
- Reference syntax: `formula-engine-reference-syntax.md`
- Values and coercion rules: `formula-engine-values-and-coercion.md`
- Arrays and spills: `formula-engine-arrays-and-spills.md`
- Calculation engine and dependency graph: `formula-engine-calculation-engine.md`
- Function library and custom functions: `formula-engine-functions.md`
- Settings and culture: `formula-engine-settings.md`
- Names and structured references: `formula-engine-names-and-structured-references.md`
- Errors and validation: `formula-engine-errors-and-validation.md`
- Formatting and normalization: `formula-engine-formatting.md`
- Diagnostics and telemetry: `formula-engine-diagnostics.md`
- Integration in DataGrid, pivots, and charts: `formula-engine-integration.md`
- Testing, parity, and benchmarks: `formula-engine-testing-and-parity.md`

## What the engine does

- Parses Excel-like formulas into an AST.
- Evaluates formulas against a workbook abstraction.
- Tracks dependencies and recalculates incrementally.
- Applies Excel-style coercion, error propagation, and implicit intersection.
- Supports dynamic array functions with spill ranges in grids.

## Core components

- `ExcelFormulaParser` and `ExcelFormulaTokenizer` (syntax and tokens).
- `FormulaEvaluator` (expression execution).
- `FormulaCalculationEngine` and `FormulaDependencyGraph` (recalc).
- `FormulaValue` and `FormulaArray` (value model).
- `ExcelFunctionRegistry` (function library and metadata).

## Where it runs

The engine uses `IFormulaWorkbook` and `IFormulaWorksheet` so it can evaluate:

- DataGrid cell formulas and formula columns.
- Pivot calculated measures.
- Chart series built from formula-driven data.
