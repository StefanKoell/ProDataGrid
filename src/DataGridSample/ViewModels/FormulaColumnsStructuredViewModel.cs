using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Data.Core;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public sealed class FormulaColumnsStructuredViewModel : ObservableObject
    {
        public FormulaColumnsStructuredViewModel()
        {
            Items = new ObservableCollection<FormulaEngineSalesRecord>(CreateItems());

            var builder = DataGridColumnDefinitionBuilder.For<FormulaEngineSalesRecord>();

            var regionProperty = CreateProperty(nameof(FormulaEngineSalesRecord.Region), row => row.Region, (row, value) => row.Region = value);
            var categoryProperty = CreateProperty(nameof(FormulaEngineSalesRecord.Category), row => row.Category, (row, value) => row.Category = value);
            var salesProperty = CreateProperty(nameof(FormulaEngineSalesRecord.Sales), row => row.Sales, (row, value) => row.Sales = value);
            var profitProperty = CreateProperty(nameof(FormulaEngineSalesRecord.Profit), row => row.Profit, (row, value) => row.Profit = value);
            var quantityProperty = CreateProperty(nameof(FormulaEngineSalesRecord.Quantity), row => row.Quantity, (row, value) => row.Quantity = value);

            ColumnDefinitions = new ObservableCollection<DataGridColumnDefinition>
            {
                builder.Text(
                    header: "Region",
                    property: regionProperty,
                    getter: row => row.Region,
                    setter: (row, value) => row.Region = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaEngineSalesRecord.Region);
                        column.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Text(
                    header: "Category",
                    property: categoryProperty,
                    getter: row => row.Category,
                    setter: (row, value) => row.Category = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaEngineSalesRecord.Category);
                        column.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Sales",
                    property: salesProperty,
                    getter: row => row.Sales,
                    setter: (row, value) => row.Sales = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaEngineSalesRecord.Sales);
                        column.FormatString = "C0";
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Profit",
                    property: profitProperty,
                    getter: row => row.Profit,
                    setter: (row, value) => row.Profit = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaEngineSalesRecord.Profit);
                        column.FormatString = "C0";
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Units",
                    property: quantityProperty,
                    getter: row => row.Quantity,
                    setter: (row, value) => row.Quantity = value,
                    configure: column =>
                    {
                        column.ColumnKey = nameof(FormulaEngineSalesRecord.Quantity);
                        column.FormatString = "N0";
                        column.Minimum = 0;
                        column.Width = new DataGridLength(0.9, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Margin",
                    formula: "=IF([@Sales]=0,0,[@Profit]/[@Sales])",
                    formulaName: "Margin",
                    configure: column =>
                    {
                        column.ColumnKey = "Margin";
                        column.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Sales / Unit",
                    formula: "=IF([@Quantity]=0,0,[@Sales]/[@Quantity])",
                    formulaName: "SalesPerUnit",
                    configure: column =>
                    {
                        column.ColumnKey = "SalesPerUnit";
                        column.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Sales Share",
                    formula: "=IF(SUM(SalesTable[Sales])=0,0,[@Sales]/SUM(SalesTable[Sales]))",
                    formulaName: "SalesShare",
                    configure: column =>
                    {
                        column.ColumnKey = "SalesShare";
                        column.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Formula(
                    header: "Sales Rank",
                    formula: "=RANK.EQ([@Sales], SalesTable[Sales])",
                    formulaName: "SalesRank",
                    configure: column =>
                    {
                        column.ColumnKey = "SalesRank";
                        column.Width = new DataGridLength(0.9, DataGridLengthUnitType.Star);
                    })
            };
        }

        public ObservableCollection<FormulaEngineSalesRecord> Items { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        private static IPropertyInfo CreateProperty<TValue>(
            string name,
            Func<FormulaEngineSalesRecord, TValue> getter,
            Action<FormulaEngineSalesRecord, TValue>? setter = null)
        {
            return new ClrPropertyInfo(
                name,
                target => target is FormulaEngineSalesRecord row ? getter(row) : default!,
                setter == null
                    ? null
                    : (target, value) =>
                    {
                        if (target is FormulaEngineSalesRecord row)
                        {
                            setter(row, value is null ? default! : (TValue)value);
                        }
                    },
                typeof(TValue));
        }

        private static ObservableCollection<FormulaEngineSalesRecord> CreateItems()
        {
            var items = new ObservableCollection<FormulaEngineSalesRecord>();
            foreach (var record in SalesRecordSampleData.CreateSalesRecords(60))
            {
                items.Add(new FormulaEngineSalesRecord
                {
                    OrderDate = record.OrderDate,
                    Region = record.Region,
                    Category = record.Category,
                    Sales = record.Sales,
                    Profit = record.Profit,
                    Quantity = record.Quantity
                });
            }

            return items;
        }
    }
}
