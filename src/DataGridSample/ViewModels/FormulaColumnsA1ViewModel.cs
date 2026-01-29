using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Data.Core;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public sealed class FormulaColumnsA1ViewModel : ObservableObject
    {
        public FormulaColumnsA1ViewModel()
        {
            Items = new ObservableCollection<FormulaA1Row>(CreateItems());

            var builder = DataGridColumnDefinitionBuilder.For<FormulaA1Row>();

            var itemProperty = CreateProperty(nameof(FormulaA1Row.Item), row => row.Item, (row, value) => row.Item = value);
            var unitsProperty = CreateProperty(nameof(FormulaA1Row.Units), row => row.Units, (row, value) => row.Units = value);
            var unitPriceProperty = CreateProperty(nameof(FormulaA1Row.UnitPrice), row => row.UnitPrice, (row, value) => row.UnitPrice = value);
            var costProperty = CreateProperty(nameof(FormulaA1Row.Cost), row => row.Cost, (row, value) => row.Cost = value);
            var isSummaryProperty = CreateProperty(nameof(FormulaA1Row.IsSummary), row => row.IsSummary, (row, value) => row.IsSummary = value);

            ColumnDefinitions = new ObservableCollection<DataGridColumnDefinition>
            {
                builder.Text(
                    header: "Item",
                    property: itemProperty,
                    getter: row => row.Item,
                    setter: (row, value) => row.Item = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaA1Row.Item);
                        column.Width = new DataGridLength(1.6, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Units",
                    property: unitsProperty,
                    getter: row => row.Units,
                    setter: (row, value) => row.Units = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaA1Row.Units);
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
                        column.ColumnKey = nameof(FormulaA1Row.UnitPrice);
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
                        column.ColumnKey = nameof(FormulaA1Row.Cost);
                        column.FormatString = "C2";
                        column.Minimum = 0;
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Revenue",
                    formula: "=[@Units]*[@UnitPrice]",
                    formulaName: "Revenue",
                    configure: column =>
                    {
                        column.ColumnKey = "Revenue";
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Profit",
                    formula: "=[@Units]*([@UnitPrice]-[@Cost])",
                    formulaName: "Profit",
                    configure: column =>
                    {
                        column.ColumnKey = "Profit";
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.CheckBox(
                    header: "Summary",
                    property: isSummaryProperty,
                    getter: row => row.IsSummary,
                    setter: (row, value) => row.IsSummary = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaA1Row.IsSummary);
                        column.Width = new DataGridLength(0.8, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Total Units",
                    formula: "=IF([@IsSummary], SUM(B1:B5), \"\")",
                    formulaName: "TotalUnits",
                    configure: column =>
                    {
                        column.ColumnKey = "TotalUnits";
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Total Revenue",
                    formula: "=IF([@IsSummary], SUM(E1:E5), \"\")",
                    formulaName: "TotalRevenue",
                    configure: column =>
                    {
                        column.ColumnKey = "TotalRevenue";
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Total Profit",
                    formula: "=IF([@IsSummary], SUM(F1:F5), \"\")",
                    formulaName: "TotalProfit",
                    configure: column =>
                    {
                        column.ColumnKey = "TotalProfit";
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    })
            };
        }

        public ObservableCollection<FormulaA1Row> Items { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        private static IPropertyInfo CreateProperty<TValue>(
            string name,
            Func<FormulaA1Row, TValue> getter,
            Action<FormulaA1Row, TValue>? setter = null)
        {
            return new ClrPropertyInfo(
                name,
                target => target is FormulaA1Row row ? getter(row) : default!,
                setter == null
                    ? null
                    : (target, value) =>
                    {
                        if (target is FormulaA1Row row)
                        {
                            setter(row, value is null ? default! : (TValue)value);
                        }
                    },
                typeof(TValue));
        }

        private static ObservableCollection<FormulaA1Row> CreateItems()
        {
            return new ObservableCollection<FormulaA1Row>
            {
                new FormulaA1Row { Item = "Starter Kit", Units = 12, UnitPrice = 24.5, Cost = 15.0 },
                new FormulaA1Row { Item = "Travel Pack", Units = 18, UnitPrice = 29.0, Cost = 18.0 },
                new FormulaA1Row { Item = "Field Bundle", Units = 9, UnitPrice = 42.0, Cost = 28.0 },
                new FormulaA1Row { Item = "Studio Set", Units = 6, UnitPrice = 65.0, Cost = 44.0 },
                new FormulaA1Row { Item = "Custom Order", Units = 4, UnitPrice = 120.0, Cost = 84.0 },
                new FormulaA1Row { Item = "Totals", Units = 0, UnitPrice = 0, Cost = 0, IsSummary = true }
            };
        }
    }
}
