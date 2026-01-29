# Formula Engine Values and Coercion

The engine follows Excel-like typing and coercion rules. Values are represented by `FormulaValue`.

## Value kinds

- `Blank` (empty cell)
- `Number` (double)
- `Text` (string)
- `Boolean` (`TRUE`/`FALSE`)
- `Error` (e.g., `#DIV/0!`)
- `Array` (2D matrix of `FormulaValue`)
- `Reference` (cell or range reference)

## Coercion rules (high level)

- Numbers: keep numeric values, apply optional 15-digit precision.
- Booleans: coerce to 1 or 0 in arithmetic contexts.
- Blank: coerces to 0 in arithmetic and empty text in string contexts.
- Text: parsed using Excel-style number parsing when enabled in settings.
- Errors: propagate through most operations and functions.

The parsing rules respect the workbook culture (decimal and list separators) and use Excel-like parsing when `UseExcelNumberParsing` is enabled.

## Comparisons

- Numeric comparisons happen when both sides are numeric.
- Otherwise, comparisons fall back to text comparison.
- Errors propagate when involved in comparisons.

## Arrays and implicit intersection

When a scalar context receives a range or array, the evaluator applies implicit intersection. In grid scenarios, dynamic array results spill into neighboring cells when enabled.

## Errors

Errors are represented by `FormulaError` and mapped to Excel-style error literals:

- `#DIV/0!`, `#VALUE!`, `#NAME?`, `#REF!`, `#NUM!`, `#N/A`, `#NULL!`, `#SPILL!`
