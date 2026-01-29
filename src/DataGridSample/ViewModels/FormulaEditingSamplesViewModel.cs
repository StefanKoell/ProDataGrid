using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Data.Core;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public sealed class FormulaEditingSamplesViewModel : ObservableObject
    {
        public FormulaEditingSamplesViewModel()
        {
            EditingItems = new ObservableCollection<FormulaEditingRow>(BuildEditingItems());
            EditingColumns = BuildEditingColumns();

            SpillItems = new ObservableCollection<FormulaSpillRow>
            {
                new FormulaSpillRow { Label = "Spill" }
            };
            SpillColumns = BuildSpillColumns();

            RecalcItems = new ObservableCollection<FormulaRecalcRow>(BuildRecalcItems());
            RecalcColumns = BuildRecalcColumns();
        }

        public ObservableCollection<FormulaEditingRow> EditingItems { get; }

        public ObservableCollection<DataGridColumnDefinition> EditingColumns { get; }

        public ObservableCollection<FormulaSpillRow> SpillItems { get; }

        public ObservableCollection<DataGridColumnDefinition> SpillColumns { get; }

        public ObservableCollection<FormulaRecalcRow> RecalcItems { get; }

        public ObservableCollection<DataGridColumnDefinition> RecalcColumns { get; }

        private static ObservableCollection<FormulaEditingRow> BuildEditingItems()
        {
            return new ObservableCollection<FormulaEditingRow>
            {
                new FormulaEditingRow { Item = "Starter Kit", Quantity = 12, UnitPrice = 24.5, Cost = 15 },
                new FormulaEditingRow { Item = "Travel Pack", Quantity = 18, UnitPrice = 29, Cost = 18 },
                new FormulaEditingRow { Item = "Field Bundle", Quantity = 9, UnitPrice = 42, Cost = 28 },
                new FormulaEditingRow { Item = "Studio Set", Quantity = 6, UnitPrice = 65, Cost = 44 }
            };
        }

        private static ObservableCollection<FormulaRecalcRow> BuildRecalcItems()
        {
            return new ObservableCollection<FormulaRecalcRow>
            {
                new FormulaRecalcRow { Input = 10, Factor = 1.1 },
                new FormulaRecalcRow { Input = 12, Factor = 1.2 },
                new FormulaRecalcRow { Input = 14, Factor = 1.3 },
                new FormulaRecalcRow { Input = 16, Factor = 1.4 },
                new FormulaRecalcRow { Input = 18, Factor = 1.5 }
            };
        }

        private static ObservableCollection<DataGridColumnDefinition> BuildEditingColumns()
        {
            var builder = DataGridColumnDefinitionBuilder.For<FormulaEditingRow>();
            var itemProperty = CreateProperty<FormulaEditingRow, string>(
                nameof(FormulaEditingRow.Item),
                row => row.Item,
                (row, value) => row.Item = value);
            var quantityProperty = CreateProperty<FormulaEditingRow, double>(
                nameof(FormulaEditingRow.Quantity),
                row => row.Quantity,
                (row, value) => row.Quantity = value);
            var unitPriceProperty = CreateProperty<FormulaEditingRow, double>(
                nameof(FormulaEditingRow.UnitPrice),
                row => row.UnitPrice,
                (row, value) => row.UnitPrice = value);
            var costProperty = CreateProperty<FormulaEditingRow, double>(
                nameof(FormulaEditingRow.Cost),
                row => row.Cost,
                (row, value) => row.Cost = value);

            return new ObservableCollection<DataGridColumnDefinition>
            {
                builder.Text(
                    header: "Item",
                    property: itemProperty,
                    getter: row => row.Item,
                    setter: (row, value) => row.Item = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaEditingRow.Item);
                        column.Width = new DataGridLength(1.6, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Quantity",
                    property: quantityProperty,
                    getter: row => row.Quantity,
                    setter: (row, value) => row.Quantity = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaEditingRow.Quantity);
                        column.FormatString = "N0";
                        column.Minimum = 0;
                        column.Width = new DataGridLength(0.9, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Unit Price",
                    property: unitPriceProperty,
                    getter: row => row.UnitPrice,
                    setter: (row, value) => row.UnitPrice = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaEditingRow.UnitPrice);
                        column.FormatString = "C2";
                        column.Minimum = 0;
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Cost",
                    property: costProperty,
                    getter: row => row.Cost,
                    setter: (row, value) => row.Cost = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaEditingRow.Cost);
                        column.FormatString = "C2";
                        column.Minimum = 0;
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Revenue",
                    formula: "=[@Quantity]*[@UnitPrice]",
                    formulaName: "Revenue",
                    configure: column =>
                    {
                        column.ColumnKey = "Revenue";
                        column.FormulaValueType = typeof(double);
                        column.AllowCellFormulas = true;
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Profit",
                    formula: "=[@Quantity]*([@UnitPrice]-[@Cost])",
                    formulaName: "Profit",
                    configure: column =>
                    {
                        column.ColumnKey = "Profit";
                        column.FormulaValueType = typeof(double);
                        column.AllowCellFormulas = true;
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    })
            };
        }

        private static ObservableCollection<DataGridColumnDefinition> BuildSpillColumns()
        {
            var builder = DataGridColumnDefinitionBuilder.For<FormulaSpillRow>();
            var labelProperty = CreateProperty<FormulaSpillRow, string>(
                nameof(FormulaSpillRow.Label),
                row => row.Label,
                (row, value) => row.Label = value);

            return new ObservableCollection<DataGridColumnDefinition>
            {
                builder.Text(
                    header: "Row",
                    property: labelProperty,
                    getter: row => row.Label,
                    setter: (row, value) => row.Label = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaSpillRow.Label);
                        column.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Spill 1",
                    formula: string.Empty,
                    formulaName: "Spill1",
                    configure: column =>
                    {
                        column.ColumnKey = "Spill1";
                        column.FormulaValueType = typeof(double);
                        column.AllowCellFormulas = true;
                        column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Spill 2",
                    formula: string.Empty,
                    formulaName: "Spill2",
                    configure: column =>
                    {
                        column.ColumnKey = "Spill2";
                        column.FormulaValueType = typeof(double);
                        column.AllowCellFormulas = true;
                        column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Spill 3",
                    formula: string.Empty,
                    formulaName: "Spill3",
                    configure: column =>
                    {
                        column.ColumnKey = "Spill3";
                        column.FormulaValueType = typeof(double);
                        column.AllowCellFormulas = true;
                        column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Spill 4",
                    formula: string.Empty,
                    formulaName: "Spill4",
                    configure: column =>
                    {
                        column.ColumnKey = "Spill4";
                        column.FormulaValueType = typeof(double);
                        column.AllowCellFormulas = true;
                        column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                    })
            };
        }

        private static ObservableCollection<DataGridColumnDefinition> BuildRecalcColumns()
        {
            var builder = DataGridColumnDefinitionBuilder.For<FormulaRecalcRow>();
            var inputProperty = CreateProperty<FormulaRecalcRow, double>(
                nameof(FormulaRecalcRow.Input),
                row => row.Input,
                (row, value) => row.Input = value);
            var factorProperty = CreateProperty<FormulaRecalcRow, double>(
                nameof(FormulaRecalcRow.Factor),
                row => row.Factor,
                (row, value) => row.Factor = value);

            return new ObservableCollection<DataGridColumnDefinition>
            {
                builder.Numeric(
                    header: "Input",
                    property: inputProperty,
                    getter: row => row.Input,
                    setter: (row, value) => row.Input = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaRecalcRow.Input);
                        column.FormatString = "N2";
                        column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Factor",
                    property: factorProperty,
                    getter: row => row.Factor,
                    setter: (row, value) => row.Factor = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaRecalcRow.Factor);
                        column.FormatString = "N2";
                        column.Width = new DataGridLength(1.0, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Scaled",
                    formula: "=[@Input]*[@Factor]",
                    formulaName: "Scaled",
                    configure: column =>
                    {
                        column.ColumnKey = "Scaled";
                        column.FormulaValueType = typeof(double);
                        column.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Delta",
                    formula: "=[@Scaled]-[@Input]",
                    formulaName: "Delta",
                    configure: column =>
                    {
                        column.ColumnKey = "Delta";
                        column.FormulaValueType = typeof(double);
                        column.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    })
            };
        }

        private static IPropertyInfo CreateProperty<TItem, TValue>(
            string name,
            Func<TItem, TValue> getter,
            Action<TItem, TValue>? setter = null)
        {
            return new ClrPropertyInfo(
                name,
                target => target is TItem item ? getter(item) : default!,
                setter == null
                    ? null
                    : (target, value) =>
                    {
                        if (target is TItem item)
                        {
                            setter(item, value is null ? default! : (TValue)value);
                        }
                    },
                typeof(TValue));
        }
    }
}
