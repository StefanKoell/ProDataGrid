using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Controls.DataGridPivoting;
using Avalonia.Threading;
using DataGridSample.Models;
using DataGridSample.Mvvm;
using ProCharts;
using ProCharts.Skia;
using ProDataGrid.Charting;
using ProDataGrid.FormulaEngine;
using ProDataGrid.FormulaEngine.Excel;

namespace DataGridSample.ViewModels
{
    public sealed class FormulaEngineIntegrationViewModel : ObservableObject
    {
        private const string MarginFormulaName = "Margin";
        private const string SalesPerUnitFormulaName = "Sales per Unit";
        private const string ProfitPerUnitFormulaName = "Profit per Unit";

        private readonly RowFormulaEvaluator _rowEvaluator;
        private bool _refreshPending;

        public FormulaEngineIntegrationViewModel()
        {
            Formulas = new ObservableCollection<FormulaDefinition>
            {
                new FormulaDefinition(MarginFormulaName, "IF(Sales=0,0,Profit/Sales)"),
                new FormulaDefinition(SalesPerUnitFormulaName, "IF(Quantity=0,0,Sales/Quantity)"),
                new FormulaDefinition(ProfitPerUnitFormulaName, "IF(Quantity=0,0,Profit/Quantity)")
            };
            Formulas.CollectionChanged += OnFormulasCollectionChanged;
            foreach (var formula in Formulas)
            {
                formula.PropertyChanged += OnFormulaPropertyChanged;
            }

            _rowEvaluator = new RowFormulaEvaluator(CultureInfo.CurrentCulture);

            Items = new ObservableCollection<FormulaEngineSalesRecord>(BuildItems());
            foreach (var item in Items)
            {
                item.PropertyChanged += OnItemPropertyChanged;
                EvaluateItem(item);
            }

            ItemsView = new DataGridCollectionView(Items);

            Pivot = BuildPivot(Items);

            ChartData = BuildChartData(ItemsView);
            UpdateChartSeriesFormulas();
            Chart = new ChartModel { DataSource = ChartData };
            Chart.Legend.IsVisible = true;
            Chart.CategoryAxis.Title = "Region";
            Chart.ValueAxis.Title = "Per-unit values";
            Chart.SecondaryValueAxis.IsVisible = true;
            Chart.SecondaryValueAxis.Title = "Margin";

            ChartStyle = new SkiaChartStyle
            {
                ShowGridlines = true,
                ShowDataLabels = true,
                LegendPosition = ChartLegendPosition.Bottom,
                LegendFlow = SkiaLegendFlow.Row,
                SeriesColors = SkiaChartStyle.DefaultSeriesColors
            };
        }

        public ObservableCollection<FormulaDefinition> Formulas { get; }

        public ObservableCollection<FormulaEngineSalesRecord> Items { get; }

        public DataGridCollectionView ItemsView { get; }

        public PivotTableModel Pivot { get; }

        public DataGridChartModel ChartData { get; }

        public ChartModel Chart { get; }

        public SkiaChartStyle ChartStyle { get; }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not FormulaEngineSalesRecord record)
            {
                return;
            }

            if (e.PropertyName is nameof(FormulaEngineSalesRecord.Margin)
                or nameof(FormulaEngineSalesRecord.SalesPerUnit)
                or nameof(FormulaEngineSalesRecord.ProfitPerUnit))
            {
                return;
            }

            EvaluateItem(record);
            QueueRefresh();
        }

        private void EvaluateItem(FormulaEngineSalesRecord item)
        {
            item.Margin = EvaluateFormula(MarginFormulaName, item);
            item.SalesPerUnit = EvaluateFormula(SalesPerUnitFormulaName, item);
            item.ProfitPerUnit = EvaluateFormula(ProfitPerUnitFormulaName, item);
        }

        private double? EvaluateFormula(string name, FormulaEngineSalesRecord item)
        {
            var formula = NormalizeFormulaText(GetFormulaText(name));
            return _rowEvaluator.EvaluateNumber(formula, item);
        }

        private string GetFormulaText(string name)
        {
            foreach (var formula in Formulas)
            {
                if (string.Equals(formula.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return formula.Formula;
                }
            }

            return string.Empty;
        }

        private static string NormalizeFormulaText(string formula)
        {
            if (string.IsNullOrWhiteSpace(formula))
            {
                return string.Empty;
            }

            formula = formula.Trim();
            return formula.StartsWith("=", StringComparison.Ordinal) ? formula.Substring(1) : formula;
        }

        private void RecalculateAll()
        {
            foreach (var item in Items)
            {
                EvaluateItem(item);
            }

            QueueRefresh();
        }

        private void OnFormulaPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(FormulaDefinition.Formula))
            {
                return;
            }

            UpdateChartSeriesFormulas();
            RecalculateAll();
        }

        private void OnFormulasCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (FormulaDefinition formula in e.OldItems)
                {
                    formula.PropertyChanged -= OnFormulaPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (FormulaDefinition formula in e.NewItems)
                {
                    formula.PropertyChanged += OnFormulaPropertyChanged;
                }
            }

            UpdateChartSeriesFormulas();
            RecalculateAll();
        }

        private void UpdateChartSeriesFormulas()
        {
            foreach (var series in ChartData.Series)
            {
                if (string.IsNullOrWhiteSpace(series.Name))
                {
                    continue;
                }

                var formula = NormalizeFormulaText(GetFormulaText(series.Name));
                series.Formula = string.IsNullOrWhiteSpace(formula) ? null : formula;
            }
        }

        private void QueueRefresh()
        {
            if (_refreshPending)
            {
                return;
            }

            _refreshPending = true;
            Dispatcher.UIThread.Post(() =>
            {
                _refreshPending = false;
                Pivot.Refresh();
                Chart.Refresh();
            }, DispatcherPriority.Background);
        }

        private static IEnumerable<FormulaEngineSalesRecord> BuildItems()
        {
            foreach (var record in SalesRecordSampleData.CreateSalesRecords(200))
            {
                yield return new FormulaEngineSalesRecord
                {
                    OrderDate = record.OrderDate,
                    Region = record.Region,
                    Category = record.Category,
                    Sales = record.Sales,
                    Profit = record.Profit,
                    Quantity = record.Quantity
                };
            }
        }

        private static PivotTableModel BuildPivot(IEnumerable<FormulaEngineSalesRecord> source)
        {
            var pivot = new PivotTableModel
            {
                ItemsSource = source,
                Culture = CultureInfo.CurrentCulture
            };

            using (pivot.DeferRefresh())
            {
                pivot.RowFields.Add(new PivotAxisField
                {
                    Header = "Region",
                    ValueSelector = item => ((FormulaEngineSalesRecord)item!).Region
                });

                pivot.ColumnFields.Add(new PivotAxisField
                {
                    Header = "Category",
                    ValueSelector = item => ((FormulaEngineSalesRecord)item!).Category
                });

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Sales",
                    ValueSelector = item => ((FormulaEngineSalesRecord)item!).Sales,
                    AggregateType = PivotAggregateType.Sum,
                    StringFormat = "C0"
                });

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Profit",
                    ValueSelector = item => ((FormulaEngineSalesRecord)item!).Profit,
                    AggregateType = PivotAggregateType.Sum,
                    StringFormat = "C0"
                });

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Margin",
                    ValueSelector = item => ((FormulaEngineSalesRecord)item!).Margin,
                    AggregateType = PivotAggregateType.Average,
                    StringFormat = "P1"
                });

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Profit / Unit",
                    ValueSelector = item => ((FormulaEngineSalesRecord)item!).ProfitPerUnit,
                    AggregateType = PivotAggregateType.Average,
                    StringFormat = "C2"
                });

                pivot.Layout.RowLayout = PivotRowLayout.Tabular;
                pivot.Layout.ValuesPosition = PivotValuesPosition.Columns;
                pivot.Layout.ShowRowSubtotals = false;
                pivot.Layout.ShowColumnSubtotals = false;
                pivot.Layout.ShowRowGrandTotals = true;
                pivot.Layout.ShowColumnGrandTotals = true;
            }

            return pivot;
        }

        private static DataGridChartModel BuildChartData(DataGridCollectionView view)
        {
            var model = new DataGridChartModel
            {
                View = view,
                CategoryPath = nameof(FormulaEngineSalesRecord.Region),
                GroupMode = DataGridChartGroupMode.TopLevel,
                DownsampleAggregation = DataGridChartAggregation.Average,
                Culture = CultureInfo.CurrentCulture,
                UseIncrementalUpdates = false
            };

            model.Series.Add(new DataGridChartSeriesDefinition
            {
                Name = "Sales per Unit",
                Formula = "IF(Quantity=0,0,Sales/Quantity)",
                Kind = ChartSeriesKind.Column,
                Aggregation = DataGridChartAggregation.Average
            });

            model.Series.Add(new DataGridChartSeriesDefinition
            {
                Name = "Profit per Unit",
                Formula = "IF(Quantity=0,0,Profit/Quantity)",
                Kind = ChartSeriesKind.Line,
                Aggregation = DataGridChartAggregation.Average
            });

            model.Series.Add(new DataGridChartSeriesDefinition
            {
                Name = "Margin",
                Formula = "IF(Sales=0,0,Profit/Sales)",
                Kind = ChartSeriesKind.Line,
                Aggregation = DataGridChartAggregation.Average,
                ValueAxisAssignment = ChartValueAxisAssignment.Secondary
            });

            return model;
        }

        private sealed class RowFormulaEvaluator
        {
            private readonly ExcelFormulaParser _parser = new();
            private readonly ExcelFunctionRegistry _registry = new();
            private readonly FormulaEvaluator _evaluator = new();
            private readonly FormulaCalculationSettings _settings;
            private readonly FormulaRowWorkbook _workbook;
            private readonly FormulaRowWorksheet _worksheet;
            private readonly FormulaCellAddress _address;
            private readonly RowFormulaResolver _resolver;

            public RowFormulaEvaluator(CultureInfo culture)
            {
                _settings = new FormulaCalculationSettings
                {
                    ReferenceMode = FormulaReferenceMode.A1,
                    Culture = culture,
                    DateSystem = FormulaDateSystem.Windows1900
                };
                _workbook = new FormulaRowWorkbook("Row", _settings);
                _worksheet = new FormulaRowWorksheet("RowData", _workbook);
                _workbook.AddWorksheet(_worksheet);
                _address = new FormulaCellAddress(_worksheet.Name, 1, 1);
                _resolver = new RowFormulaResolver(this);
            }

            public double? EvaluateNumber(string formula, FormulaEngineSalesRecord record)
            {
                if (string.IsNullOrWhiteSpace(formula))
                {
                    return null;
                }

                formula = NormalizeFormula(formula);
                FormulaExpression expression;
                try
                {
                    expression = _parser.Parse(formula, new FormulaParseOptions
                    {
                        ReferenceMode = _settings.ReferenceMode
                    });
                }
                catch (FormulaParseException)
                {
                    return null;
                }

                _resolver.SetCurrent(record);
                var context = new FormulaEvaluationContext(_workbook, _worksheet, _address, _registry);
                var value = _evaluator.Evaluate(expression, context, _resolver);
                if (value.Kind == FormulaValueKind.Array)
                {
                    value = FormulaCoercion.ApplyImplicitIntersection(value, _address);
                }

                if (value.Kind == FormulaValueKind.Error || value.Kind == FormulaValueKind.Blank)
                {
                    return null;
                }

                return FormulaCoercion.TryCoerceToNumber(value, out var number, out _)
                    ? number
                    : null;
            }

            private static string NormalizeFormula(string formula)
            {
                formula = formula.Trim();
                return formula.StartsWith("=", StringComparison.Ordinal)
                    ? formula.Substring(1)
                    : formula;
            }

            private sealed class RowFormulaResolver : IFormulaValueResolver
            {
                private readonly RowFormulaEvaluator _owner;
                private FormulaEngineSalesRecord? _current;
                private readonly Dictionary<string, Func<FormulaEngineSalesRecord, object?>> _selectors;

                public RowFormulaResolver(RowFormulaEvaluator owner)
                {
                    _owner = owner;
                    _selectors = new Dictionary<string, Func<FormulaEngineSalesRecord, object?>>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["OrderDate"] = item => item.OrderDate,
                        ["Region"] = item => item.Region,
                        ["Category"] = item => item.Category,
                        ["Sales"] = item => item.Sales,
                        ["Profit"] = item => item.Profit,
                        ["Quantity"] = item => item.Quantity,
                        ["Margin"] = item => item.Margin,
                        ["SalesPerUnit"] = item => item.SalesPerUnit,
                        ["ProfitPerUnit"] = item => item.ProfitPerUnit
                    };
                }

                public void SetCurrent(FormulaEngineSalesRecord record)
                {
                    _current = record;
                }

                public bool TryResolveName(FormulaEvaluationContext context, string name, out FormulaValue value)
                {
                    if (_current != null && _selectors.TryGetValue(name, out var selector))
                    {
                        value = ConvertToFormulaValue(selector(_current), _owner._settings.Culture);
                        return true;
                    }

                    value = FormulaValue.FromError(new FormulaError(FormulaErrorType.Name));
                    return true;
                }

                public bool TryResolveReference(FormulaEvaluationContext context, FormulaReference reference, out FormulaValue value)
                {
                    value = FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
                    return true;
                }
            }

            private static FormulaValue ConvertToFormulaValue(object? value, CultureInfo culture)
            {
                if (value == null)
                {
                    return FormulaValue.Blank;
                }

                if (value is FormulaValue formulaValue)
                {
                    return formulaValue;
                }

                if (value is FormulaError error)
                {
                    return FormulaValue.FromError(error);
                }

                if (value is string text)
                {
                    return FormulaValue.FromText(text);
                }

                if (value is bool boolValue)
                {
                    return FormulaValue.FromBoolean(boolValue);
                }

                if (value is double doubleValue)
                {
                    return FormulaValue.FromNumber(doubleValue);
                }

                if (value is float floatValue)
                {
                    return FormulaValue.FromNumber(floatValue);
                }

                if (value is decimal decimalValue)
                {
                    return FormulaValue.FromNumber((double)decimalValue);
                }

                if (value is int intValue)
                {
                    return FormulaValue.FromNumber(intValue);
                }

                if (value is long longValue)
                {
                    return FormulaValue.FromNumber(longValue);
                }

                if (value is short shortValue)
                {
                    return FormulaValue.FromNumber(shortValue);
                }

                if (value is byte byteValue)
                {
                    return FormulaValue.FromNumber(byteValue);
                }

                if (value is DateTime dateValue)
                {
                    return FormulaValue.FromNumber(dateValue.ToOADate());
                }

                if (value is TimeSpan timeValue)
                {
                    return FormulaValue.FromNumber(timeValue.TotalDays);
                }

                try
                {
                    return FormulaValue.FromNumber(Convert.ToDouble(value, culture));
                }
                catch
                {
                    return FormulaValue.FromText(Convert.ToString(value, culture) ?? string.Empty);
                }
            }
        }

        private sealed class FormulaRowWorkbook : IFormulaWorkbook
        {
            private readonly List<IFormulaWorksheet> _worksheets = new();

            public FormulaRowWorkbook(string name, FormulaCalculationSettings settings)
            {
                Name = name;
                Settings = settings;
            }

            public string Name { get; }

            public IReadOnlyList<IFormulaWorksheet> Worksheets => _worksheets;

            public FormulaCalculationSettings Settings { get; }

            public void AddWorksheet(IFormulaWorksheet worksheet)
            {
                _worksheets.Add(worksheet);
            }

            public IFormulaWorksheet GetWorksheet(string name)
            {
                foreach (var sheet in _worksheets)
                {
                    if (string.Equals(sheet.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return sheet;
                    }
                }

                throw new InvalidOperationException($"Worksheet '{name}' not found.");
            }
        }

        private sealed class FormulaRowWorksheet : IFormulaWorksheet
        {
            public FormulaRowWorksheet(string name, IFormulaWorkbook workbook)
            {
                Name = name;
                Workbook = workbook;
            }

            public string Name { get; }

            public IFormulaWorkbook Workbook { get; }

            public IFormulaCell GetCell(int row, int column)
            {
                return new FormulaRowCell(new FormulaCellAddress(Name, row, column));
            }

            public bool TryGetCell(int row, int column, out IFormulaCell cell)
            {
                cell = new FormulaRowCell(new FormulaCellAddress(Name, row, column));
                return true;
            }
        }

        private sealed class FormulaRowCell : IFormulaCell
        {
            public FormulaRowCell(FormulaCellAddress address)
            {
                Address = address;
                Value = FormulaValue.Blank;
            }

            public FormulaCellAddress Address { get; }

            public string? Formula { get; set; }

            public FormulaExpression? Expression { get; set; }

            public FormulaValue Value { get; set; }
        }
    }
}
