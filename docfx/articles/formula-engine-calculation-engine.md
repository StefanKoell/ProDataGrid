# Formula Engine Calculation Engine

`FormulaCalculationEngine` manages dependency tracking, recalculation, and spill handling.

## Dependency graph

- Dependencies are derived from parsed expressions.
- The engine tracks direct dependencies and reverse dependents.
- Named range dependencies are tracked by scope (workbook vs worksheet).

External workbook references are evaluated at runtime but do not contribute to the dependency graph inside the current workbook.

## Recalculation flow

1. Mark dirty cells.
2. Expand dirty cells to include spill ranges.
3. Compute a recalculation order from the dependency graph.
4. Evaluate each formula and update cached values.

The engine supports topological ordering and optional parallel execution.

## Calculation modes

- `Automatic`: `RecalculateIfAutomatic` recalculates when cells are invalidated.
- `Manual`: callers trigger `Recalculate` explicitly.

You can query the effective mode with `GetCalculationMode` and set it via `FormulaCalculationSettings`.

## Volatile functions

Functions marked as `IsVolatile` are recalculated every cycle. The engine tracks volatile cells in a separate set and includes them in dirty expansion.

## Name changes

If you use workbook or sheet names, call `TrackNameChanges` so the dependency graph updates when names change:

```csharp
using var subscription = engine.TrackNameChanges(workbook);
```

## Calculation settings

Key settings that affect recalculation:

- `EnableDynamicArrays`
- `EnableIterativeCalculation`
- `EnableParallelCalculation`
- `MaxDegreeOfParallelism`
- `IterativeMaxIterations` and `IterativeTolerance`
