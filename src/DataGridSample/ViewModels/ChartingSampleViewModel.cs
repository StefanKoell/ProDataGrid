using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using ProCharts;
using ProCharts.Skia;
using ProDataGrid.Charting;
using DataGridSample.Models;

namespace DataGridSample.ViewModels
{
    public sealed class ChartingSampleViewModel : INotifyPropertyChanged
    {
        private readonly DataGridChartSeriesDefinition _salesSeries;
        private readonly DataGridChartSeriesDefinition _quantitySeries;
        private readonly DataGridChartSeriesDefinition _profitSeries;
        private bool _useStackedArea = true;
        private bool _useFormattedLabels = true;
        private bool _showDataLabels = true;
        private SkiaChartStyle _chartStyle = new();

        public ChartingSampleViewModel()
        {
            Items = new ObservableCollection<SalesRecord>(SalesRecordSampleData.CreateSalesRecords(400));

            ChartData = new DataGridChartModel
            {
                ItemsSource = Items,
                CategoryPath = nameof(SalesRecord.OrderDate),
                GroupMode = DataGridChartGroupMode.LeafItems,
                DownsampleAggregation = DataGridChartAggregation.Average
            };

            _salesSeries = new DataGridChartSeriesDefinition
            {
                Name = "Sales",
                ValuePath = nameof(SalesRecord.Sales),
                Kind = ChartSeriesKind.StackedArea
            };

            _quantitySeries = new DataGridChartSeriesDefinition
            {
                Name = "Quantity",
                ValuePath = nameof(SalesRecord.Quantity),
                Kind = ChartSeriesKind.StackedArea
            };

            _profitSeries = new DataGridChartSeriesDefinition
            {
                Name = "Profit",
                ValuePath = nameof(SalesRecord.Profit),
                Kind = ChartSeriesKind.Line,
                Aggregation = DataGridChartAggregation.Sum
            };

            ChartData.Series.Add(_salesSeries);
            ChartData.Series.Add(_quantitySeries);
            ChartData.Series.Add(_profitSeries);

            Chart = new ChartModel
            {
                DataSource = ChartData
            };

            Chart.Request.MaxPoints = 200;

            ApplySeriesKinds();
            ApplyFormatting();
            UpdateChartStyle();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<SalesRecord> Items { get; }

        public DataGridChartModel ChartData { get; }

        public ChartModel Chart { get; }

        public SkiaChartStyle ChartStyle
        {
            get => _chartStyle;
            private set
            {
                if (ReferenceEquals(_chartStyle, value))
                {
                    return;
                }

                _chartStyle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChartStyle)));
            }
        }

        public bool UseStackedArea
        {
            get => _useStackedArea;
            set
            {
                if (_useStackedArea == value)
                {
                    return;
                }

                _useStackedArea = value;
                ApplySeriesKinds();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseStackedArea)));
            }
        }

        public bool UseFormattedLabels
        {
            get => _useFormattedLabels;
            set
            {
                if (_useFormattedLabels == value)
                {
                    return;
                }

                _useFormattedLabels = value;
                ApplyFormatting();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseFormattedLabels)));
            }
        }

        public bool ShowDataLabels
        {
            get => _showDataLabels;
            set
            {
                if (_showDataLabels == value)
                {
                    return;
                }

                _showDataLabels = value;
                UpdateChartStyle();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowDataLabels)));
            }
        }

        private void ApplySeriesKinds()
        {
            if (_useStackedArea)
            {
                _salesSeries.Kind = ChartSeriesKind.StackedArea;
                _quantitySeries.Kind = ChartSeriesKind.StackedArea;
            }
            else
            {
                _salesSeries.Kind = ChartSeriesKind.Column;
                _quantitySeries.Kind = ChartSeriesKind.Area;
            }

            _profitSeries.Kind = ChartSeriesKind.Line;
        }

        private void ApplyFormatting()
        {
            if (_useFormattedLabels)
            {
                var culture = CultureInfo.CurrentCulture;
                _salesSeries.DataLabelFormatter = value => value.ToString("C0", culture);
                _profitSeries.DataLabelFormatter = value => value.ToString("C0", culture);
                _quantitySeries.DataLabelFormatter = value => value.ToString("N0", culture);
                Chart.ValueAxis.LabelFormatter = value => value.ToString("C0", culture);
            }
            else
            {
                _salesSeries.DataLabelFormatter = null;
                _profitSeries.DataLabelFormatter = null;
                _quantitySeries.DataLabelFormatter = null;
                Chart.ValueAxis.LabelFormatter = null;
            }

            Chart.Refresh();
        }

        private void UpdateChartStyle()
        {
            var style = new SkiaChartStyle(ChartStyle)
            {
                ShowDataLabels = _showDataLabels,
                ShowCategoryGridlines = true,
                ShowGridlines = true,
                LegendGroupStackedSeries = true,
                LegendFlow = SkiaLegendFlow.Column,
                LegendWrap = true
            };

            ChartStyle = style;
        }
    }
}
