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
    public sealed class PivotValueFilterViewModel : ObservableObject
    {
        private const int TopCategoryCount = 2;
        private readonly IList<SalesRecord> _filteredSource;
        private bool _showFilteredData;

        public PivotValueFilterViewModel()
        {
            Source = new ObservableCollection<SalesRecord>(SalesRecordSampleData.CreateSalesRecords(900));
            _filteredSource = FilterTopCategories(Source, TopCategoryCount);
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

                var categoryField = new PivotAxisField
                {
                    Header = "Category",
                    ValueSelector = item => ((SalesRecord)item!).Category,
                    ValueFilter = new PivotValueFilter
                    {
                        FilterType = PivotValueFilterType.Top,
                        Count = TopCategoryCount,
                        ValueField = salesField
                    }
                };

                var yearField = new PivotAxisField
                {
                    Header = "Year",
                    ValueSelector = item => ((SalesRecord)item!).OrderDate,
                    GroupSelector = value => value is DateTime date ? date.Year : null,
                    SortDirection = ListSortDirection.Ascending
                };

                pivot.RowFields.Add(categoryField);
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

        private static IList<SalesRecord> FilterTopCategories(IEnumerable<SalesRecord> source, int count)
        {
            var records = source as IReadOnlyList<SalesRecord> ?? source.ToList();
            var totals = new Dictionary<string?, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var record in records)
            {
                if (!totals.TryGetValue(record.Category, out var total))
                {
                    total = 0d;
                }

                totals[record.Category] = total + record.Sales;
            }

            var topCategories = totals
                .OrderByDescending(entry => entry.Value)
                .Take(Math.Max(1, count))
                .Select(entry => entry.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var filtered = new List<SalesRecord>(records.Count);
            foreach (var record in records)
            {
                if (topCategories.Contains(record.Category))
                {
                    filtered.Add(record);
                }
            }

            return filtered;
        }
    }
}
