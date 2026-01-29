# Formula Engine Reference Syntax

The Excel parser supports A1 and R1C1 syntax, sheet prefixes, external workbooks, structured references, and range operators.

## A1 references

- Relative: `A1`, `B2`
- Absolute: `$A$1`, `$B$2`
- Mixed: `$A1`, `B$2`

## R1C1 references

- Absolute: `R1C1`
- Relative: `R[-1]C[2]`
- Mixed: `R1C[2]`, `R[1]C3`

Reference mode is controlled by `FormulaCalculationSettings.ReferenceMode`.

## Sheet prefixes

- Simple sheet: `Sheet1!A1`
- Quoted sheet: `'Sales Q1'!B2`
- Sheet ranges: `Sheet1:Sheet3!A1` (3D references)

## External workbook references

External references use the workbook name in brackets:

- `'[Book2]Sheet1'!A1`
- `'[Sales 2024]January'!B5`

External references resolve through `IFormulaWorkbookResolver`.

## Range operators

- Range: `A1:B10`
- Union: `A1:A10,B1:B10`
- Intersection: `A1:C3 B2:D4`

The union operator uses the culture list separator. For example, in `de-DE` the union separator is `;`.

## Named ranges

Names can be defined at the workbook or worksheet scope and referenced directly:

- `Revenue + 1`
- `Sheet1!TaxRate`

## Structured references

Structured references target table columns and scopes:

- `Table1[Amount]`
- `[@Amount]` (this row)
- `Table1[[#Headers],[Amount]]`
- `Table1[[#Totals],[Amount]]`
- `Table1[[#Data],[Amount]]`
- `Table1[[#All],[Amount]]`

Structured references are resolved by the workbook or worksheet via `IFormulaStructuredReferenceResolver`.

## Error literals

The parser understands Excel error literals:

- `#DIV/0!`, `#VALUE!`, `#NAME?`, `#REF!`, `#NUM!`, `#N/A`, `#NULL!`, `#SPILL!`

## Leading equals

Formulas can be entered with or without `=`. The parser accepts both:

- `=A1+B1`
- `A1+B1`
