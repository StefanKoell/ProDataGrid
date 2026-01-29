using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls.DataGridPivoting;
using ProCharts;
using ProCharts.Skia;
using ProDataGrid.Charting;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public sealed class PivotChartViewModel : ObservableObject
    {
        private readonly IList<SalesRecord> _filteredSource;
        private bool _showFilteredData;
        private PivotChartSeriesSource _seriesSource;
        private bool _includeSubtotals;
        private bool _includeGrandTotals;

        public PivotChartViewModel()
        {
            Source = new ObservableCollection<SalesRecord>(SalesRecordSampleData.CreateSalesRecords(600));
            _filteredSource = Source;

            Pivot = new PivotTableModel
            {
                ItemsSource = Source,
                Culture = CultureInfo.CurrentCulture
            };

            PivotValueField salesField;
            using (Pivot.DeferRefresh())
            {
                Pivot.RowFields.Add(new PivotAxisField
                {
                    Header = "Region",
                    ValueSelector = item => ((SalesRecord)item!).Region,
                    SortDirection = ListSortDirection.Ascending
                });

                Pivot.ColumnFields.Add(new PivotAxisField
                {
                    Header = "Year",
                    ValueSelector = item => ((SalesRecord)item!).OrderDate,
                    GroupSelector = value => value is DateTime date ? date.Year : null,
                    SortDirection = ListSortDirection.Ascending
                });

                salesField = new PivotValueField
                {
                    Header = "Sales",
                    ValueSelector = item => ((SalesRecord)item!).Sales,
                    AggregateType = PivotAggregateType.Sum,
                    StringFormat = "C0"
                };

                Pivot.ValueFields.Add(salesField);

                Pivot.Layout.RowLayout = PivotRowLayout.Tabular;
                Pivot.Layout.ValuesPosition = PivotValuesPosition.Columns;
                Pivot.Layout.ShowRowSubtotals = false;
                Pivot.Layout.ShowColumnSubtotals = false;
                Pivot.Layout.ShowRowGrandTotals = true;
                Pivot.Layout.ShowColumnGrandTotals = true;
            }

            Chart = new PivotChartModel
            {
                Pivot = Pivot,
                SeriesSource = PivotChartSeriesSource.Rows,
                ValueField = salesField
            };

            ChartData = new PivotChartDataSource
            {
                PivotChart = Chart,
                SeriesKind = ChartSeriesKind.Column
            };

            ChartModel = new ChartModel
            {
                DataSource = ChartData
            };

            ChartStyle = new SkiaChartStyle
            {
                ShowGridlines = true,
                ShowCategoryGridlines = true,
                LegendFlow = SkiaLegendFlow.Column,
                LegendWrap = true
            };

            SeriesSources = Enum.GetValues<PivotChartSeriesSource>();
            _seriesSource = Chart.SeriesSource;
            _includeSubtotals = Chart.IncludeSubtotals;
            _includeGrandTotals = Chart.IncludeGrandTotals;
        }

        public ObservableCollection<SalesRecord> Source { get; }

        public PivotTableModel Pivot { get; }

        public PivotChartModel Chart { get; }

        public PivotChartDataSource ChartData { get; }

        public ChartModel ChartModel { get; }

        public SkiaChartStyle ChartStyle { get; }

        public PivotChartSeriesSource[] SeriesSources { get; }

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

        public PivotChartSeriesSource SeriesSource
        {
            get => _seriesSource;
            set
            {
                if (SetProperty(ref _seriesSource, value))
                {
                    Chart.SeriesSource = value;
                }
            }
        }

        public bool IncludeSubtotals
        {
            get => _includeSubtotals;
            set
            {
                if (SetProperty(ref _includeSubtotals, value))
                {
                    Chart.IncludeSubtotals = value;
                }
            }
        }

        public bool IncludeGrandTotals
        {
            get => _includeGrandTotals;
            set
            {
                if (SetProperty(ref _includeGrandTotals, value))
                {
                    Chart.IncludeGrandTotals = value;
                }
            }
        }
    }
}
