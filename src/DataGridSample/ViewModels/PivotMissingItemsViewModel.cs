using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls.DataGridPivoting;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public sealed class PivotMissingItemsViewModel : ObservableObject
    {
        private const string MissingCategory = "Office Supplies";
        private const string MissingSegment = "Home Office";
        private readonly IList<SalesRecord> _filteredSource;
        private bool _showFilteredData;

        public PivotMissingItemsViewModel()
        {
            var records = SalesRecordSampleData.CreateSalesRecords(700).ToList();
            _filteredSource = FilterMissingItems(records).ToList();
            Source = new ObservableCollection<SalesRecord>(records);
            Pivot = BuildPivot(_filteredSource);
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
                    ShowItemsWithNoData = true,
                    ItemsSource = SalesRecordSampleData.Categories
                };

                var segmentField = new PivotAxisField
                {
                    Header = "Segment",
                    ValueSelector = item => ((SalesRecord)item!).Segment,
                    ShowItemsWithNoData = true,
                    ItemsSource = SalesRecordSampleData.Segments
                };

                pivot.RowFields.Add(categoryField);
                pivot.ColumnFields.Add(segmentField);

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Sales",
                    ValueSelector = item => ((SalesRecord)item!).Sales,
                    AggregateType = PivotAggregateType.Sum,
                    StringFormat = "C0"
                });

                pivot.Layout.RowLayout = PivotRowLayout.Compact;
                pivot.Layout.ValuesPosition = PivotValuesPosition.Columns;
                pivot.Layout.ShowRowSubtotals = false;
                pivot.Layout.ShowColumnSubtotals = false;
                pivot.Layout.ShowRowGrandTotals = true;
                pivot.Layout.ShowColumnGrandTotals = true;
            }

            return pivot;
        }

        private static IEnumerable<SalesRecord> FilterMissingItems(IEnumerable<SalesRecord> source)
        {
            return source.Where(record =>
                !string.Equals(record.Category, MissingCategory, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(record.Segment, MissingSegment, StringComparison.OrdinalIgnoreCase));
        }
    }
}
