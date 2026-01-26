using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls.DataGridPivoting;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public sealed class PivotCalculatedMeasuresViewModel : ObservableObject
    {
        private readonly IList<SalesRecord> _filteredSource;
        private bool _showFilteredData;

        public PivotCalculatedMeasuresViewModel()
        {
            Source = new ObservableCollection<SalesRecord>(SalesRecordSampleData.CreateSalesRecords(800));
            _filteredSource = Source;
            Pivot = BuildPivot(Source);

            Formulas = new[]
            {
                "Margin = Profit / Sales",
                "Sales Share = Sales / GrandTotal(Sales)"
            };
        }

        public ObservableCollection<SalesRecord> Source { get; }

        public PivotTableModel Pivot { get; }

        public IEnumerable<SalesRecord> DataRows => _showFilteredData ? _filteredSource : Source;

        public IReadOnlyList<string> Formulas { get; }

        public bool ShowFilteredData
        {
            get => _showFilteredData;
            set
            {
                if (SetProperty(ref _showFilteredData, value))
                {
                    OnPropertyChanged(nameof(DataRows));
                }
            }
        }

        private static PivotTableModel BuildPivot(IEnumerable<SalesRecord> source)
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
                    ValueSelector = item => ((SalesRecord)item!).Region
                });

                pivot.RowFields.Add(new PivotAxisField
                {
                    Header = "Category",
                    ValueSelector = item => ((SalesRecord)item!).Category
                });

                pivot.ColumnFields.Add(new PivotAxisField
                {
                    Header = "Year",
                    ValueSelector = item => ((SalesRecord)item!).OrderDate,
                    GroupSelector = value => value is DateTime date ? date.Year : null,
                    SortDirection = ListSortDirection.Ascending
                });

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Sales",
                    ValueSelector = item => ((SalesRecord)item!).Sales,
                    AggregateType = PivotAggregateType.Sum,
                    StringFormat = "C0"
                });

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Profit",
                    ValueSelector = item => ((SalesRecord)item!).Profit,
                    AggregateType = PivotAggregateType.Sum,
                    StringFormat = "C0"
                });

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Margin",
                    AggregateType = PivotAggregateType.None,
                    Formula = "Profit / Sales",
                    StringFormat = "P1"
                });

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Sales Share",
                    AggregateType = PivotAggregateType.None,
                    Formula = "Sales / GrandTotal(Sales)",
                    StringFormat = "P1"
                });

                pivot.Layout.RowLayout = PivotRowLayout.Tabular;
                pivot.Layout.ValuesPosition = PivotValuesPosition.Columns;
                pivot.Layout.ShowRowSubtotals = false;
                pivot.Layout.ShowColumnSubtotals = false;
                pivot.Layout.ShowRowGrandTotals = true;
                pivot.Layout.ShowColumnGrandTotals = true;
                pivot.Layout.RepeatRowLabels = true;
            }

            return pivot;
        }
    }
}
