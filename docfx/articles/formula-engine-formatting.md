# Formula Engine Formatting

The Excel layer includes a formatter that turns parsed expressions back into a formula string.

## Formatter API

- `ExcelFormulaFormatter` implements `IFormulaFormatter`.
- `FormulaFormatOptions` controls output format.

```csharp
var formatter = new ExcelFormulaFormatter();
var options = new FormulaFormatOptions
{
    ReferenceMode = FormulaReferenceMode.A1,
    ArgumentSeparator = ',',
    DecimalSeparator = '.',
    IncludeLeadingEquals = true
};

var text = formatter.Format(expression, options);
```

## Formatting options

`FormulaFormatOptions` controls:

- A1 vs R1C1 reference mode.
- Argument separator (comma or semicolon).
- Decimal separator.
- Optional leading equals.

Formatting is useful for:

- Normalizing formulas for display.
- Serializing formulas with consistent separators.
- Converting between A1 and R1C1 reference modes.
