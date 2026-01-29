# Formula Engine Testing and Parity

The formula engine ships with a compatibility corpus and focused unit tests to validate Excel parity.

## Compatibility corpus

The compatibility corpus is a JSON dataset of formulas and expected results:

- `src/ProDataGrid.FormulaEngine.UnitTests/Compatibility/ExcelCompatibilityCorpus.json`

The corpus is exercised by `ExcelCompatibilityCorpusTests`.

## Parity report

A human-readable summary is available in:

- `docs/formula-engine-parity.md`

This report documents gaps and known differences from Excel.

## Unit tests

Run the formula engine tests:

```bash
dotnet test src/ProDataGrid.FormulaEngine.UnitTests/ProDataGrid.FormulaEngine.UnitTests.csproj -c Debug
```

## Benchmarks

Benchmarks live in:

- `tests/ProDataGrid.FormulaEngine.Benchmarks`

Run them with:

```bash
dotnet run --project tests/ProDataGrid.FormulaEngine.Benchmarks/ProDataGrid.FormulaEngine.Benchmarks.csproj -c Release
```
