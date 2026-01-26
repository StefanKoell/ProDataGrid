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
    public class PivotTableViewModel : ObservableObject
    {
        private const string AllRegionsLabel = "All Regions";

        private readonly PivotAxisField _regionField;
        private readonly List<PivotValueField> _valueFields = new();
        private readonly Dictionary<PivotValueField, string?> _valueFormats = new();

        private IList<SalesRecord> _filteredSource = Array.Empty<SalesRecord>();
        private PivotValuesPosition _valuesPosition;
        private PivotRowLayout _rowLayout;
        private PivotValueDisplayMode _displayMode;
        private bool _showRowSubtotals;
        private bool _showColumnSubtotals;
        private bool _showRowGrandTotals;
        private bool _showColumnGrandTotals;
        private bool _repeatRowLabels;
        private bool _showFilteredData;
        private string _selectedRegion = AllRegionsLabel;

        public PivotTableViewModel()
        {
            Source = new ObservableCollection<SalesRecord>(SalesRecordSampleData.CreateSalesRecords(900));
            Pivot = new PivotTableModel
            {
                ItemsSource = Source,
                Culture = CultureInfo.CurrentCulture
            };

            using (Pivot.DeferRefresh())
            {
                _regionField = new PivotAxisField
                {
                    Header = "Region",
                    ValueSelector = item => ((SalesRecord)item!).Region
                };

                var categoryField = new PivotAxisField
                {
                    Header = "Category",
                    ValueSelector = item => ((SalesRecord)item!).Category
                };

                var yearField = new PivotAxisField
                {
                    Header = "Year",
                    ValueSelector = item => ((SalesRecord)item!).OrderDate,
                    GroupSelector = value => value is DateTime date ? date.Year : null,
                    SortDirection = ListSortDirection.Ascending
                };

                var segmentField = new PivotAxisField
                {
                    Header = "Segment",
                    ValueSelector = item => ((SalesRecord)item!).Segment
                };

                Pivot.RowFields.Add(_regionField);
                Pivot.RowFields.Add(categoryField);
                Pivot.ColumnFields.Add(yearField);
                Pivot.ColumnFields.Add(segmentField);

                AddValueField("Sales", record => record.Sales, PivotAggregateType.Sum, "C0");
                AddValueField("Profit", record => record.Profit, PivotAggregateType.Sum, "C0");
                AddValueField("Units", record => record.Quantity, PivotAggregateType.Sum, "N0");

                Pivot.Layout.RowLayout = PivotRowLayout.Compact;
                Pivot.Layout.ValuesPosition = PivotValuesPosition.Columns;
                Pivot.Layout.ShowRowSubtotals = true;
                Pivot.Layout.ShowColumnSubtotals = true;
                Pivot.Layout.ShowRowGrandTotals = true;
                Pivot.Layout.ShowColumnGrandTotals = true;
                Pivot.Layout.RepeatRowLabels = false;
            }

            ValuesPositions = Enum.GetValues<PivotValuesPosition>();
            RowLayouts = Enum.GetValues<PivotRowLayout>();
            DisplayModes = Enum.GetValues<PivotValueDisplayMode>();
            RegionOptions = BuildRegionOptions(Source);

            _valuesPosition = Pivot.Layout.ValuesPosition;
            _rowLayout = Pivot.Layout.RowLayout;
            _showRowSubtotals = Pivot.Layout.ShowRowSubtotals;
            _showColumnSubtotals = Pivot.Layout.ShowColumnSubtotals;
            _showRowGrandTotals = Pivot.Layout.ShowRowGrandTotals;
            _showColumnGrandTotals = Pivot.Layout.ShowColumnGrandTotals;
            _repeatRowLabels = Pivot.Layout.RepeatRowLabels;

            ApplyDisplayMode(PivotValueDisplayMode.Value);
            UpdateFilteredSource(_selectedRegion);
        }

        public PivotTableModel Pivot { get; }

        public ObservableCollection<SalesRecord> Source { get; }

        public IEnumerable<SalesRecord> DataRows => _showFilteredData ? _filteredSource : Source;

        public PivotValuesPosition[] ValuesPositions { get; }

        public PivotRowLayout[] RowLayouts { get; }

        public PivotValueDisplayMode[] DisplayModes { get; }

        public string[] RegionOptions { get; }

        public PivotValuesPosition ValuesPosition
        {
            get => _valuesPosition;
            set
            {
                if (SetProperty(ref _valuesPosition, value))
                {
                    Pivot.Layout.ValuesPosition = value;
                }
            }
        }

        public PivotRowLayout RowLayout
        {
            get => _rowLayout;
            set
            {
                if (SetProperty(ref _rowLayout, value))
                {
                    Pivot.Layout.RowLayout = value;
                }
            }
        }

        public PivotValueDisplayMode DisplayMode
        {
            get => _displayMode;
            set
            {
                if (SetProperty(ref _displayMode, value))
                {
                    ApplyDisplayMode(value);
                }
            }
        }

        public bool ShowRowSubtotals
        {
            get => _showRowSubtotals;
            set
            {
                if (SetProperty(ref _showRowSubtotals, value))
                {
                    Pivot.Layout.ShowRowSubtotals = value;
                }
            }
        }

        public bool ShowColumnSubtotals
        {
            get => _showColumnSubtotals;
            set
            {
                if (SetProperty(ref _showColumnSubtotals, value))
                {
                    Pivot.Layout.ShowColumnSubtotals = value;
                }
            }
        }

        public bool ShowRowGrandTotals
        {
            get => _showRowGrandTotals;
            set
            {
                if (SetProperty(ref _showRowGrandTotals, value))
                {
                    Pivot.Layout.ShowRowGrandTotals = value;
                }
            }
        }

        public bool ShowColumnGrandTotals
        {
            get => _showColumnGrandTotals;
            set
            {
                if (SetProperty(ref _showColumnGrandTotals, value))
                {
                    Pivot.Layout.ShowColumnGrandTotals = value;
                }
            }
        }

        public bool RepeatRowLabels
        {
            get => _repeatRowLabels;
            set
            {
                if (SetProperty(ref _repeatRowLabels, value))
                {
                    Pivot.Layout.RepeatRowLabels = value;
                }
            }
        }

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

        public string SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (SetProperty(ref _selectedRegion, value))
                {
                    ApplyRegionFilter(value);
                }
            }
        }

        private void AddValueField(
            string header,
            Func<SalesRecord, object?> selector,
            PivotAggregateType aggregateType,
            string? format)
        {
            var field = new PivotValueField
            {
                Header = header,
                AggregateType = aggregateType,
                ValueSelector = item => selector((SalesRecord)item!)
            };

            _valueFields.Add(field);
            _valueFormats[field] = format;
            Pivot.ValueFields.Add(field);
        }

        private void ApplyDisplayMode(PivotValueDisplayMode mode)
        {
            var usePercentFormat = IsPercentDisplayMode(mode);
            foreach (var field in _valueFields)
            {
                field.DisplayMode = mode;
                if (mode == PivotValueDisplayMode.Value)
                {
                    _valueFormats.TryGetValue(field, out var format);
                    field.StringFormat = format;
                }
                else if (usePercentFormat)
                {
                    field.StringFormat = "P2";
                }
                else
                {
                    _valueFormats.TryGetValue(field, out var format);
                    field.StringFormat = format;
                }
            }
        }

        private void ApplyRegionFilter(string region)
        {
            if (string.IsNullOrWhiteSpace(region) || region == AllRegionsLabel)
            {
                _regionField.Filter = null;
            }
            else
            {
                _regionField.Filter = new PivotFieldFilter(included: new[] { region });
            }

            UpdateFilteredSource(region);
        }

        private void UpdateFilteredSource(string region)
        {
            if (string.IsNullOrWhiteSpace(region) || region == AllRegionsLabel)
            {
                _filteredSource = Source;
            }
            else
            {
                _filteredSource = Source
                    .Where(record => string.Equals(record.Region, region, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (_showFilteredData)
            {
                OnPropertyChanged(nameof(DataRows));
            }
        }

        private static string[] BuildRegionOptions(IEnumerable<SalesRecord> source)
        {
            var regions = source
                .Select(record => record.Region)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            regions.Insert(0, AllRegionsLabel);
            return regions.ToArray();
        }

        private static bool IsPercentDisplayMode(PivotValueDisplayMode mode)
        {
            return mode == PivotValueDisplayMode.PercentOfRowTotal ||
                   mode == PivotValueDisplayMode.PercentOfColumnTotal ||
                   mode == PivotValueDisplayMode.PercentOfGrandTotal ||
                   mode == PivotValueDisplayMode.PercentOfParentRowTotal ||
                   mode == PivotValueDisplayMode.PercentOfParentColumnTotal ||
                   mode == PivotValueDisplayMode.PercentDifferenceFromPrevious;
        }

    }
}
