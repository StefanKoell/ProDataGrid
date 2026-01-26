using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls.DataGridPivoting;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public sealed class PivotValueSortViewModel : ObservableObject
    {
        private readonly IList<SalesRecord> _filteredSource;
        private bool _showFilteredData;

        public PivotValueSortViewModel()
        {
            Source = new ObservableCollection<SalesRecord>(SalesRecordSampleData.CreateSalesRecords(900));
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
                var salesField = new PivotValueField
                {
                    Header = "Sales",
                    ValueSelector = item => ((SalesRecord)item!).Sales,
                    AggregateType = PivotAggregateType.Sum,
                    StringFormat = "C0"
                };

                var regionField = new PivotAxisField
                {
                    Header = "Region",
                    ValueSelector = item => ((SalesRecord)item!).Region,
                    ValueSort = new PivotValueSort
                    {
                        ValueField = salesField,
                        SortDirection = ListSortDirection.Descending
                    }
                };

                var yearField = new PivotAxisField
                {
                    Header = "Year",
                    ValueSelector = item => ((SalesRecord)item!).OrderDate,
                    GroupSelector = value => value is System.DateTime date ? date.Year : null,
                    SortDirection = ListSortDirection.Ascending
                };

                pivot.RowFields.Add(regionField);
                pivot.ColumnFields.Add(yearField);
                pivot.ValueFields.Add(salesField);

                pivot.Layout.RowLayout = PivotRowLayout.Compact;
                pivot.Layout.ValuesPosition = PivotValuesPosition.Columns;
                pivot.Layout.ShowRowSubtotals = false;
                pivot.Layout.ShowColumnSubtotals = false;
                pivot.Layout.ShowRowGrandTotals = true;
                pivot.Layout.ShowColumnGrandTotals = true;
            }

            return pivot;
        }
    }
}
