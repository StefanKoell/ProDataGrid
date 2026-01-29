# Formula Engine Diagnostics and Telemetry

The calculation engine exposes hooks to observe parsing, compilation, and evaluation timing.

## Calculation observer

Implement `IFormulaCalculationObserver` and assign it to `FormulaCalculationSettings.CalculationObserver`:

```csharp
var telemetry = new FormulaCalculationTelemetry();
workbook.Settings.CalculationObserver = telemetry;
```

Observer callbacks include:

- `OnExpressionParsed`
- `OnExpressionCompiled`
- `OnCellEvaluated`
- `OnRecalculationStarted`
- `OnRecalculationCompleted`

## Built-in telemetry helper

`FormulaCalculationTelemetry` aggregates counters and timings:

- Parsed expressions
- Compiled expressions and cache hits
- Cells evaluated
- Total recalculation time

Call `Reset()` to clear counters between runs.

## Compiled expression caching

When `EnableCompiledExpressions` is enabled, the evaluator compiles expressions into bytecode and caches them in a weak table. This improves throughput for repeated evaluations of the same formula AST.

Compiled expressions are skipped for formulas that use union or intersection operators so array semantics remain correct.
