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
    public sealed class PivotValuesInRowsViewModel : ObservableObject
    {
        private readonly IList<SalesRecord> _filteredSource;
        private bool _showFilteredData;

        public PivotValuesInRowsViewModel()
        {
            Source = new ObservableCollection<SalesRecord>(SalesRecordSampleData.CreateSalesRecords(700));
            _filteredSource = Source;
            Pivot = BuildPivot(Source);
        }

        public ObservableCollection<SalesRecord> Source { get; }

        public PivotTableModel Pivot { get; }

        public IEnumerable<SalesRecord> DataRows => _showFilteredData ? _filteredSource : Source;

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
                var regionField = new PivotAxisField
                {
                    Header = "Region",
                    ValueSelector = item => ((SalesRecord)item!).Region
                };

                var segmentField = new PivotAxisField
                {
                    Header = "Segment",
                    ValueSelector = item => ((SalesRecord)item!).Segment
                };

                var yearField = new PivotAxisField
                {
                    Header = "Year",
                    ValueSelector = item => ((SalesRecord)item!).OrderDate,
                    GroupSelector = value => value is DateTime date ? date.Year : null,
                    SortDirection = ListSortDirection.Ascending
                };

                pivot.RowFields.Add(regionField);
                pivot.RowFields.Add(segmentField);
                pivot.ColumnFields.Add(yearField);

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
                    Header = "Units",
                    ValueSelector = item => ((SalesRecord)item!).Quantity,
                    AggregateType = PivotAggregateType.Sum,
                    StringFormat = "N0"
                });

                pivot.Layout.RowLayout = PivotRowLayout.Tabular;
                pivot.Layout.ValuesPosition = PivotValuesPosition.Rows;
                pivot.Layout.RepeatRowLabels = true;
                pivot.Layout.ShowRowSubtotals = true;
                pivot.Layout.ShowColumnSubtotals = false;
                pivot.Layout.ShowRowGrandTotals = true;
                pivot.Layout.ShowColumnGrandTotals = true;
            }

            return pivot;
        }
    }
}
