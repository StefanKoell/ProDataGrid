# Formula Engine Functions

Functions are provided through `IFormulaFunctionRegistry`. The Excel compatibility layer registers a large set of Excel functions in `ExcelFunctionRegistry`.

## Built-in registry

```csharp
var registry = new ExcelFunctionRegistry();
```

The registry exposes lookup and enumeration:

- `TryGetFunction(name, out function)`
- `GetAll()`

## Function metadata

Each function exposes a `FormulaFunctionInfo`:

- `MinArgs` / `MaxArgs`
- `IsVolatile`

Variadic functions set `MaxArgs` to `-1`.

## Custom function example

```csharp
internal sealed class AddTaxFunction : IFormulaFunction
{
    public string Name => "ADDTAX";
    public FormulaFunctionInfo Info => new FormulaFunctionInfo(2, 2);

    public FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
    {
        var settings = context.EvaluationContext.Workbook.Settings;
        if (!FormulaCoercion.TryCoerceToNumber(args[0], settings, out var amount, out var error))
        {
            return FormulaValue.FromError(error);
        }
        if (!FormulaCoercion.TryCoerceToNumber(args[1], settings, out var rate, out error))
        {
            return FormulaValue.FromError(error);
        }
        return FormulaValue.FromNumber(amount * (1 + rate));
    }
}

var registry = new ExcelFunctionRegistry();
registry.Register(new AddTaxFunction());
```

## Lazy functions

Functions that need short-circuit evaluation can implement `ILazyFormulaFunction`. The evaluator will pass the argument expressions and a resolver so the function can decide which arguments to evaluate.
