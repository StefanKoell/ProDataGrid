# Formula Engine Settings and Culture

`FormulaCalculationSettings` controls parsing and evaluation behavior. It is exposed on the workbook and is used by the calculation engine and evaluator.

## Key settings

- `ReferenceMode`: `A1` or `R1C1`.
- `DateSystem`: `Windows1900` or `Mac1904`.
- `Culture`: drives decimal and list separators.
- `CalculationMode`: `Automatic` or `Manual`.
- `UseExcelNumberParsing`: Excel-style number parsing for text values.
- `ApplyNumberPrecision`: enable 15-digit precision rules.
- `EnableDynamicArrays`: allow spill results for array formulas.
- `EnableIterativeCalculation`: allow circular references with iteration.
- `EnableCompiledExpressions`: enable bytecode compilation and caching.
- `EnableParallelCalculation`: allow parallel evaluation of independent levels.
- `MaxDegreeOfParallelism`: cap worker count for parallel evaluation.
- `IterativeMaxIterations` and `IterativeTolerance`: control iterative recalculation.
- `CalculationObserver`: optional telemetry hooks.

## Culture-aware parsing

Settings create parse options automatically:

```csharp
var settings = workbook.Settings;
settings.Culture = new CultureInfo("de-DE");

var options = settings.CreateParseOptions();
// Argument separator and decimal separator reflect the culture.
```

The parser respects:

- Argument separators (comma or semicolon).
- Decimal separators (dot or comma).
- Leading equals (enabled by default).

## Calculation modes

- `RecalculateIfAutomatic` honors `CalculationMode`.
- `Recalculate` always recalculates the provided dirty range.

```csharp
engine.RecalculateIfAutomatic(workbook, dirtyCells);
```
