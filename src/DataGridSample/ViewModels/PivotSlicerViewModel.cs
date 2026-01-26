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
    public sealed class PivotSlicerViewModel : ObservableObject
    {
        private IList<SalesRecord> _filteredSource;
        private bool _showFilteredData;
        private int _topCount = 4;
        private bool _useValueFilter = true;

        public PivotSlicerViewModel()
        {
            Source = new ObservableCollection<SalesRecord>(SalesRecordSampleData.CreateSalesRecords(800));
            _filteredSource = Source;

            Pivot = new PivotTableModel
            {
                ItemsSource = Source,
                Culture = CultureInfo.CurrentCulture
            };

            var regionField = new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((SalesRecord)item!).Region
            };

            var categoryField = new PivotAxisField
            {
                Header = "Category",
                ValueSelector = item => ((SalesRecord)item!).Category,
                SortDirection = ListSortDirection.Ascending
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

            var salesField = new PivotValueField
            {
                Header = "Sales",
                ValueSelector = item => ((SalesRecord)item!).Sales,
                AggregateType = PivotAggregateType.Sum,
                StringFormat = "C0"
            };

            var profitField = new PivotValueField
            {
                Header = "Profit",
                ValueSelector = item => ((SalesRecord)item!).Profit,
                AggregateType = PivotAggregateType.Sum,
                StringFormat = "C0"
            };

            using (Pivot.DeferRefresh())
            {
                Pivot.RowFields.Add(categoryField);
                Pivot.RowFields.Add(productField);
                Pivot.ColumnFields.Add(yearField);
                Pivot.FilterFields.Add(regionField);

                Pivot.ValueFields.Add(salesField);
                Pivot.ValueFields.Add(profitField);

                Pivot.Layout.RowLayout = PivotRowLayout.Tabular;
                Pivot.Layout.ValuesPosition = PivotValuesPosition.Columns;
                Pivot.Layout.ShowRowSubtotals = false;
                Pivot.Layout.ShowColumnSubtotals = false;
                Pivot.Layout.ShowRowGrandTotals = true;
                Pivot.Layout.ShowColumnGrandTotals = true;
            }

            RegionSlicer = new PivotSlicerModel
            {
                ItemsSource = Source,
                Field = regionField,
                FilterMode = PivotSlicerFilterMode.Include
            };

            ValueFilter = new PivotValueFilterModel
            {
                Field = categoryField,
                ValueField = salesField,
                FilterType = PivotValueFilterType.Top,
                Count = _topCount
            };

            SelectAllRegionsCommand = new RelayCommand(_ => RegionSlicer.SelectAll());
            ClearRegionsCommand = new RelayCommand(_ => RegionSlicer.ClearSelection());

            RegionSlicer.PropertyChanged += RegionSlicerOnPropertyChanged;
            ValueFilter.PropertyChanged += ValueFilterOnPropertyChanged;
            UpdateFilteredSource();
        }

        public ObservableCollection<SalesRecord> Source { get; }

        public PivotTableModel Pivot { get; }

        public PivotSlicerModel RegionSlicer { get; }

        public PivotValueFilterModel ValueFilter { get; }

        public RelayCommand SelectAllRegionsCommand { get; }

        public RelayCommand ClearRegionsCommand { get; }

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

        public int TopCount
        {
            get => _topCount;
            set
            {
                if (SetProperty(ref _topCount, value))
                {
                    ValueFilter.Count = value;
                }
            }
        }

        public bool UseValueFilter
        {
            get => _useValueFilter;
            set
            {
                if (SetProperty(ref _useValueFilter, value))
                {
                    ValueFilter.FilterType = value ? PivotValueFilterType.Top : PivotValueFilterType.None;
                }
            }
        }

        private void RegionSlicerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(PivotSlicerModel.Filter))
            {
                UpdateFilteredSource();
            }
        }

        private void ValueFilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateFilteredSource();
        }

        private void UpdateFilteredSource()
        {
            var records = Source.ToList();
            var regionFilter = RegionSlicer.Filter;

            if (regionFilter != null &&
                (regionFilter.Included.Count > 0 || regionFilter.Excluded.Count > 0 || regionFilter.Predicate != null))
            {
                records = records
                    .Where(record => regionFilter.IsMatch(record.Region))
                    .ToList();
            }

            if (ValueFilter.FilterType == PivotValueFilterType.Top && ValueFilter.Count is > 0)
            {
                records = ApplyTopCategoryFilter(records, ValueFilter.Count.Value);
            }

            _filteredSource = records;
            if (_showFilteredData)
            {
                OnPropertyChanged(nameof(DataRows));
            }
        }

        private static List<SalesRecord> ApplyTopCategoryFilter(IReadOnlyList<SalesRecord> records, int count)
        {
            if (records.Count == 0 || count <= 0)
            {
                return new List<SalesRecord>();
            }

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
                .ThenBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
                .Take(Math.Max(1, count))
                .Select(entry => entry.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (topCategories.Count == 0)
            {
                return new List<SalesRecord>();
            }

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
