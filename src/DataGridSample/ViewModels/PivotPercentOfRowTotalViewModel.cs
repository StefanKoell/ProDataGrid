using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls.DataGridPivoting;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public sealed class PivotPercentOfRowTotalViewModel : ObservableObject
    {
        private readonly IList<SalesRecord> _filteredSource;
        private bool _showFilteredData;

        public PivotPercentOfRowTotalViewModel()
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

                var categoryField = new PivotAxisField
                {
                    Header = "Category",
                    ValueSelector = item => ((SalesRecord)item!).Category
                };

                var segmentField = new PivotAxisField
                {
                    Header = "Segment",
                    ValueSelector = item => ((SalesRecord)item!).Segment
                };

                pivot.RowFields.Add(regionField);
                pivot.RowFields.Add(categoryField);
                pivot.ColumnFields.Add(segmentField);

                pivot.ValueFields.Add(new PivotValueField
                {
                    Header = "Sales Share",
                    ValueSelector = item => ((SalesRecord)item!).Sales,
                    AggregateType = PivotAggregateType.Sum,
                    DisplayMode = PivotValueDisplayMode.PercentOfRowTotal,
                    StringFormat = "P1"
                });

                pivot.Layout.RowLayout = PivotRowLayout.Tabular;
                pivot.Layout.ValuesPosition = PivotValuesPosition.Columns;
                pivot.Layout.RepeatRowLabels = true;
                pivot.Layout.ShowRowSubtotals = false;
                pivot.Layout.ShowColumnSubtotals = false;
                pivot.Layout.ShowRowGrandTotals = false;
                pivot.Layout.ShowColumnGrandTotals = false;
            }

            return pivot;
        }
    }
}
