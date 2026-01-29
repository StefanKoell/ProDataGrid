using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using DataGridSample.Models;
using DataGridSample.Mvvm;
using ProDataGrid.FormulaEngine;
using ProDataGrid.FormulaEngine.Excel;

namespace DataGridSample.ViewModels
{
    public sealed class FormulaEngineSamplesViewModel : ObservableObject
    {
        private readonly FormulaSampleWorkbook _workbook;
        private readonly ExcelFormulaParser _parser = new();
        private readonly ExcelFunctionRegistry _registry = new();
        private readonly FormulaEvaluator _evaluator = new();
        private readonly WorkbookValueResolver _resolver;

        public FormulaEngineSamplesViewModel()
        {
            _workbook = BuildWorkbook();
            _resolver = new WorkbookValueResolver(_parser);

            WorkbookCells = new ObservableCollection<FormulaCellSnapshot>(BuildWorkbookSnapshot());

            Basics = BuildSamples(BuildBasics());
            References = BuildSamples(BuildReferences());
            Arrays = BuildSamples(BuildArrays());
            Functions = BuildSamples(BuildFunctions());
            DateTimeSamples = BuildSamples(BuildDateTime());
            Names = BuildSamples(BuildNames());
            Structured = BuildSamples(BuildStructured());
            External = BuildSamples(BuildExternal());
        }

        public ObservableCollection<FormulaCellSnapshot> WorkbookCells { get; }

        public IReadOnlyList<FormulaSampleItem> Basics { get; }

        public IReadOnlyList<FormulaSampleItem> References { get; }

        public IReadOnlyList<FormulaSampleItem> Arrays { get; }

        public IReadOnlyList<FormulaSampleItem> Functions { get; }

        public IReadOnlyList<FormulaSampleItem> DateTimeSamples { get; }

        public IReadOnlyList<FormulaSampleItem> Names { get; }

        public IReadOnlyList<FormulaSampleItem> Structured { get; }

        public IReadOnlyList<FormulaSampleItem> External { get; }

        private FormulaSampleWorkbook BuildWorkbook()
        {
            var settings = new FormulaCalculationSettings
            {
                ReferenceMode = FormulaReferenceMode.A1,
                Culture = CultureInfo.InvariantCulture,
                DateSystem = FormulaDateSystem.Windows1900
            };

            var workbook = new FormulaSampleWorkbook("Samples", settings);
            var sheet1 = workbook.AddWorksheet("Sheet1");
            var sheet2 = workbook.AddWorksheet("Sheet2");
            var sheet3 = workbook.AddWorksheet("Sheet3");

            sheet1.SetValue(1, 1, FormulaValue.FromNumber(10));
            sheet1.SetValue(2, 1, FormulaValue.FromNumber(20));
            sheet1.SetValue(3, 1, FormulaValue.FromNumber(30));
            sheet1.SetValue(1, 2, FormulaValue.FromNumber(2));
            sheet1.SetValue(2, 2, FormulaValue.FromNumber(5));
            sheet1.SetValue(3, 2, FormulaValue.FromNumber(0));
            sheet1.SetValue(1, 3, FormulaValue.FromText("North"));
            sheet1.SetValue(2, 3, FormulaValue.FromText("South"));
            sheet1.SetValue(3, 3, FormulaValue.FromText("East"));

            sheet2.SetValue(1, 1, FormulaValue.FromNumber(100));
            sheet3.SetValue(1, 1, FormulaValue.FromNumber(50));

            sheet1.SetValue(10, 1, FormulaValue.FromText("Region"));
            sheet1.SetValue(10, 2, FormulaValue.FromText("Sales"));
            sheet1.SetValue(10, 3, FormulaValue.FromText("Profit"));
            sheet1.SetValue(10, 4, FormulaValue.FromText("Units"));

            sheet1.SetValue(11, 1, FormulaValue.FromText("North"));
            sheet1.SetValue(11, 2, FormulaValue.FromNumber(1200));
            sheet1.SetValue(11, 3, FormulaValue.FromNumber(200));
            sheet1.SetValue(11, 4, FormulaValue.FromNumber(30));

            sheet1.SetValue(12, 1, FormulaValue.FromText("South"));
            sheet1.SetValue(12, 2, FormulaValue.FromNumber(950));
            sheet1.SetValue(12, 3, FormulaValue.FromNumber(120));
            sheet1.SetValue(12, 4, FormulaValue.FromNumber(25));

            sheet1.SetValue(13, 1, FormulaValue.FromText("East"));
            sheet1.SetValue(13, 2, FormulaValue.FromNumber(1500));
            sheet1.SetValue(13, 3, FormulaValue.FromNumber(300));
            sheet1.SetValue(13, 4, FormulaValue.FromNumber(40));

            sheet1.SetValue(14, 1, FormulaValue.FromText("Total"));
            sheet1.SetValue(14, 2, FormulaValue.FromNumber(3650));
            sheet1.SetValue(14, 3, FormulaValue.FromNumber(620));
            sheet1.SetValue(14, 4, FormulaValue.FromNumber(95));

            var tableColumns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Region"] = 1,
                ["Sales"] = 2,
                ["Profit"] = 3,
                ["Units"] = 4
            };
            workbook.AddTable(new FormulaSampleTable("SalesTable", sheet1, 10, 11, 13, tableColumns, totalsRow: 14));

            workbook.NameTable.SetValue("TaxRate", FormulaValue.FromNumber(0.2));
            workbook.NameTable.SetExpression("NorthSales",
                _parser.Parse("Sheet1!B11", new FormulaParseOptions()));

            sheet1.NameTable.SetValue("LocalRate", FormulaValue.FromNumber(0.05));

            var external = new FormulaSampleWorkbook("Budget.xlsx", settings);
            var externalSheet = external.AddWorksheet("Sheet1");
            externalSheet.SetValue(1, 1, FormulaValue.FromNumber(77));
            workbook.AddExternalWorkbook(external);

            var engine = new FormulaCalculationEngine(_parser, _registry);
            engine.SetCellFormula(sheet1, 1, 4, "A1+B1");
            engine.SetCellFormula(sheet1, 2, 4, "SUM(A1:A3)");
            engine.SetCellFormula(sheet1, 3, 4, "IF(B3=0,0,A3/B3)");
            engine.SetCellFormula(sheet1, 4, 4, "TaxRate*A1");
            engine.SetCellFormula(sheet1, 5, 4, "SUM(SalesTable[Sales])");
            engine.SetCellFormula(sheet1, 6, 4, "Sheet2!A1");
            engine.SetCellFormula(sheet1, 7, 4, "SUM(Sheet1:Sheet3!A1)");
            engine.Recalculate(workbook, engine.DependencyGraph.GetFormulaCells());

            return workbook;
        }

        private IReadOnlyList<FormulaCellSnapshot> BuildWorkbookSnapshot()
        {
            var snapshot = new List<FormulaCellSnapshot>();
            foreach (var worksheet in _workbook.Worksheets)
            {
                if (worksheet is not FormulaSampleWorksheet sampleWorksheet)
                {
                    continue;
                }

                foreach (var cell in sampleWorksheet.Cells)
                {
                    var addressText = FormatAddress(cell.Address);
                    var result = FormatValue(cell.Value, cell.Address);
                    snapshot.Add(new FormulaCellSnapshot(addressText, cell.Formula, result, cell.Value.Kind.ToString()));
                }
            }

            snapshot.Sort((left, right) => string.Compare(left.AddressText, right.AddressText, StringComparison.Ordinal));
            return snapshot;
        }

        private IReadOnlyList<FormulaSampleItem> BuildSamples(IReadOnlyList<FormulaSampleDefinition> definitions)
        {
            var results = new List<FormulaSampleItem>(definitions.Count);
            foreach (var definition in definitions)
            {
                var evaluation = EvaluateFormula(definition);
                results.Add(new FormulaSampleItem(
                    definition.Description,
                    definition.Formula,
                    evaluation.Result,
                    evaluation.Kind,
                    definition.Notes));
            }

            return results;
        }

        private FormulaSampleEvaluation EvaluateFormula(FormulaSampleDefinition definition)
        {
            var worksheet = _workbook.GetWorksheet(definition.SheetName);
            var address = new FormulaCellAddress(definition.SheetName, definition.Row, definition.Column);
            var options = new FormulaParseOptions
            {
                ReferenceMode = definition.ReferenceMode ?? _workbook.Settings.ReferenceMode
            };

            var previousCulture = _workbook.Settings.Culture;
            if (definition.Culture != null)
            {
                _workbook.Settings.Culture = definition.Culture;
            }

            try
            {
                var expression = _parser.Parse(definition.Formula, options);
                var context = new FormulaEvaluationContext(_workbook, worksheet, address, _registry);
                var value = _evaluator.Evaluate(expression, context, _resolver);
                if (definition.ApplyImplicitIntersection)
                {
                    value = FormulaCoercion.ApplyImplicitIntersection(value, address);
                }

                return new FormulaSampleEvaluation(FormatValue(value, address), value.Kind.ToString());
            }
            finally
            {
                _workbook.Settings.Culture = previousCulture;
            }
        }

        private static string FormatValue(FormulaValue value, FormulaCellAddress address)
        {
            if (value.Kind == FormulaValueKind.Array)
            {
                return FormatArray(value.AsArray(), address);
            }

            return FormatScalar(value, address);
        }

        private static string FormatScalar(FormulaValue value, FormulaCellAddress address)
        {
            if (value.Kind == FormulaValueKind.Array)
            {
                value = FormulaCoercion.ApplyImplicitIntersection(value, address);
            }

            return value.Kind switch
            {
                FormulaValueKind.Number => value.AsNumber().ToString("G", CultureInfo.InvariantCulture),
                FormulaValueKind.Text => value.AsText(),
                FormulaValueKind.Boolean => value.AsBoolean() ? "TRUE" : "FALSE",
                FormulaValueKind.Error => value.AsError().ToString(),
                FormulaValueKind.Blank => string.Empty,
                FormulaValueKind.Reference => value.AsReference().ToString(),
                _ => string.Empty
            };
        }

        private static string FormatArray(FormulaArray array, FormulaCellAddress address)
        {
            var rows = new List<string>();
            for (var row = 0; row < array.RowCount; row++)
            {
                var values = new List<string>();
                for (var column = 0; column < array.ColumnCount; column++)
                {
                    if (!array.IsPresent(row, column))
                    {
                        values.Add(string.Empty);
                        continue;
                    }

                    values.Add(FormatScalar(array[row, column], address));
                }
                rows.Add(string.Join(", ", values));
            }

            return $"Array({array.RowCount}x{array.ColumnCount}) {{{string.Join("; ", rows)}}}";
        }

        private static string FormatAddress(FormulaCellAddress address)
        {
            var columnText = ColumnToLetters(address.Column);
            if (string.IsNullOrWhiteSpace(address.SheetName))
            {
                return $"{columnText}{address.Row}";
            }

            return $"{address.SheetName}!{columnText}{address.Row}";
        }

        private static string ColumnToLetters(int column)
        {
            var value = column;
            var letters = string.Empty;
            while (value > 0)
            {
                value--;
                var remainder = value % 26;
                letters = (char)('A' + remainder) + letters;
                value /= 26;
            }

            return letters;
        }

        private static IReadOnlyList<FormulaSampleDefinition> BuildBasics()
        {
            return new[]
            {
                new FormulaSampleDefinition("Operator precedence", "1+2*3", "Basic arithmetic precedence."),
                new FormulaSampleDefinition("Parentheses override precedence", "(1+2)*3"),
                new FormulaSampleDefinition("Text concatenation", "\"Hello\" & \" World\""),
                new FormulaSampleDefinition("Percent operator", "10%"),
                new FormulaSampleDefinition("Coercion (TRUE + 1)", "TRUE+1"),
                new FormulaSampleDefinition("Error literal", "#DIV/0!")
            };
        }

        private static IReadOnlyList<FormulaSampleDefinition> BuildReferences()
        {
            return new[]
            {
                new FormulaSampleDefinition("A1 reference", "A1+A2"),
                new FormulaSampleDefinition("Absolute reference", "$A$1+B2"),
                new FormulaSampleDefinition("Range sum", "SUM(A1:B2)"),
                new FormulaSampleDefinition("Sheet prefix", "Sheet2!A1"),
                new FormulaSampleDefinition("3-D reference", "SUM(Sheet1:Sheet3!A1)"),
                new FormulaSampleDefinition("R1C1 reference mode", "R1C1+R2C1", ReferenceMode: FormulaReferenceMode.R1C1)
            };
        }

        private static IReadOnlyList<FormulaSampleDefinition> BuildArrays()
        {
            return new[]
            {
                new FormulaSampleDefinition("Array literal", "{1,2;3,4}"),
                new FormulaSampleDefinition("Array arithmetic", "A1:B2+1"),
                new FormulaSampleDefinition("Implicit intersection", "IF(A1:B2,1,0)", Row: 2, Column: 2, ApplyImplicitIntersection: true),
                new FormulaSampleDefinition("Union operator", "SUM((A1:A2,B1:B2))"),
                new FormulaSampleDefinition("Intersection operator", "SUM((A1:B2 B2:C3))")
            };
        }

        private static IReadOnlyList<FormulaSampleDefinition> BuildFunctions()
        {
            return new[]
            {
                new FormulaSampleDefinition("ABS + ROUND", "ROUND(ABS(-1.235),2)"),
                new FormulaSampleDefinition("Logical IF", "IF(TRUE,1,1/0)"),
                new FormulaSampleDefinition("IFERROR fallback", "IFERROR(1/0,5)"),
                new FormulaSampleDefinition("LEFT / LEN", "LEFT(\"Hello\",2)&\" (\"&LEN(\"Hello\")&\")\""),
                new FormulaSampleDefinition("VLOOKUP exact", "VLOOKUP(2,{1,\"a\";2,\"b\";3,\"c\"},2,FALSE)"),
                new FormulaSampleDefinition("COUNT / COUNTA", "COUNT(A1:B3) & \"/\" & COUNTA(A1:C3)")
            };
        }

        private static IReadOnlyList<FormulaSampleDefinition> BuildDateTime()
        {
            return new[]
            {
                new FormulaSampleDefinition("DATE serial", "DATE(2024,1,2)"),
                new FormulaSampleDefinition("EOMONTH", "EOMONTH(DATE(2024,1,15),1)"),
                new FormulaSampleDefinition("NETWORKDAYS", "NETWORKDAYS(DATE(2024,1,1),DATE(2024,1,5))"),
                new FormulaSampleDefinition("DATEVALUE (en-GB)", "DATEVALUE(\"31/01/2024\")", Culture: new CultureInfo("en-GB")),
                new FormulaSampleDefinition("TIME / HOUR", "HOUR(TIME(13,45,30))")
            };
        }

        private static IReadOnlyList<FormulaSampleDefinition> BuildNames()
        {
            return new[]
            {
                new FormulaSampleDefinition("Workbook name", "TaxRate"),
                new FormulaSampleDefinition("Worksheet name", "LocalRate"),
                new FormulaSampleDefinition("Name to reference", "NorthSales")
            };
        }

        private static IReadOnlyList<FormulaSampleDefinition> BuildStructured()
        {
            return new[]
            {
                new FormulaSampleDefinition("Structured column", "SUM(SalesTable[Sales])"),
                new FormulaSampleDefinition("Structured this row", "SalesTable[@Profit]", Row: 12, Column: 4, ApplyImplicitIntersection: true)
            };
        }

        private static IReadOnlyList<FormulaSampleDefinition> BuildExternal()
        {
            return new[]
            {
                new FormulaSampleDefinition("External workbook", "[Budget.xlsx]Sheet1!A1")
            };
        }

        private readonly record struct FormulaSampleDefinition(
            string Description,
            string Formula,
            string? Notes = null,
            string SheetName = "Sheet1",
            int Row = 1,
            int Column = 1,
            FormulaReferenceMode? ReferenceMode = null,
            CultureInfo? Culture = null,
            bool ApplyImplicitIntersection = false);

        private readonly record struct FormulaSampleEvaluation(string Result, string Kind);
    }
}
