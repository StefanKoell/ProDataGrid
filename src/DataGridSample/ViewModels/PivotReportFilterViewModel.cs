using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls.DataGridPivoting;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public sealed class PivotReportFilterViewModel : ObservableObject
    {
        private const string ReportFilterRegion = "West";
        private readonly IList<SalesRecord> _filteredSource;
        private bool _showFilteredData;

        public PivotReportFilterViewModel()
        {
            Source = new ObservableCollection<SalesRecord>(SalesRecordSampleData.CreateSalesRecords(700));
            _filteredSource = Source
                .Where(record => string.Equals(record.Region, ReportFilterRegion, StringComparison.OrdinalIgnoreCase))
                .ToList();
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
                var categoryField = new PivotAxisField
                {
                    Header = "Category",
                    ValueSelector = item => ((SalesRecord)item!).Category,
                    SubtotalPosition = PivotTotalPosition.Start
                };

                var productField = new PivotAxisField
                {
                    Header = "Product",
                    ValueSelector = item => ((SalesRecord)item!).Product
                };

                var yearField = new PivotAxisField
                {
                    Header = "Year",
                    ValueSelector = item => ((SalesRecord)item!).OrderDate,
                    GroupSelector = value => value is DateTime date ? date.Year : null,
                    SortDirection = ListSortDirection.Ascending
                };

                var regionFilter = new PivotAxisField
                {
                    Header = "Region",
                    ValueSelector = item => ((SalesRecord)item!).Region,
                    Filter = new PivotFieldFilter(included: new object?[] { ReportFilterRegion })
                };

                pivot.RowFields.Add(categoryField);
                pivot.RowFields.Add(productField);
                pivot.ColumnFields.Add(yearField);
                pivot.FilterFields.Add(regionFilter);

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

                pivot.Layout.RowLayout = PivotRowLayout.Compact;
                pivot.Layout.ValuesPosition = PivotValuesPosition.Columns;
                pivot.Layout.ShowRowSubtotals = true;
                pivot.Layout.ShowColumnSubtotals = false;
                pivot.Layout.ShowRowGrandTotals = true;
                pivot.Layout.ShowColumnGrandTotals = true;
            }

            return pivot;
        }
    }
}
