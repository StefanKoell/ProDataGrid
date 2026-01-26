// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;

namespace Avalonia.Controls.DataGridPivoting
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotLayoutOptions : INotifyPropertyChanged
    {
        private PivotRowLayout _rowLayout = PivotRowLayout.Compact;
        private PivotValuesPosition _valuesPosition = PivotValuesPosition.Columns;
        private bool _showRowSubtotals = true;
        private bool _showColumnSubtotals = true;
        private bool _showRowGrandTotals = true;
        private bool _showColumnGrandTotals = true;
        private PivotTotalPosition _rowGrandTotalPosition = PivotTotalPosition.End;
        private PivotTotalPosition _columnGrandTotalPosition = PivotTotalPosition.End;
        private bool _repeatRowLabels;
        private string _rowHeaderLabel = "Row Labels";
        private string _grandTotalLabel = "Grand Total";
        private string _subtotalLabelFormat = "{0} Total";
        private string _valuesHeaderLabel = "Values";
        private double _compactIndentSize = 12d;
        private string _emptyValueLabel = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PivotRowLayout RowLayout
        {
            get => _rowLayout;
            set => SetProperty(ref _rowLayout, value, nameof(RowLayout));
        }

        public PivotValuesPosition ValuesPosition
        {
            get => _valuesPosition;
            set => SetProperty(ref _valuesPosition, value, nameof(ValuesPosition));
        }

        public bool ShowRowSubtotals
        {
            get => _showRowSubtotals;
            set => SetProperty(ref _showRowSubtotals, value, nameof(ShowRowSubtotals));
        }

        public bool ShowColumnSubtotals
        {
            get => _showColumnSubtotals;
            set => SetProperty(ref _showColumnSubtotals, value, nameof(ShowColumnSubtotals));
        }

        public bool ShowRowGrandTotals
        {
            get => _showRowGrandTotals;
            set => SetProperty(ref _showRowGrandTotals, value, nameof(ShowRowGrandTotals));
        }

        public bool ShowColumnGrandTotals
        {
            get => _showColumnGrandTotals;
            set => SetProperty(ref _showColumnGrandTotals, value, nameof(ShowColumnGrandTotals));
        }

        public PivotTotalPosition RowGrandTotalPosition
        {
            get => _rowGrandTotalPosition;
            set => SetProperty(ref _rowGrandTotalPosition, value, nameof(RowGrandTotalPosition));
        }

        public PivotTotalPosition ColumnGrandTotalPosition
        {
            get => _columnGrandTotalPosition;
            set => SetProperty(ref _columnGrandTotalPosition, value, nameof(ColumnGrandTotalPosition));
        }

        public bool RepeatRowLabels
        {
            get => _repeatRowLabels;
            set => SetProperty(ref _repeatRowLabels, value, nameof(RepeatRowLabels));
        }

        public string RowHeaderLabel
        {
            get => _rowHeaderLabel;
            set => SetProperty(ref _rowHeaderLabel, value, nameof(RowHeaderLabel));
        }

        public string GrandTotalLabel
        {
            get => _grandTotalLabel;
            set => SetProperty(ref _grandTotalLabel, value, nameof(GrandTotalLabel));
        }

        public string SubtotalLabelFormat
        {
            get => _subtotalLabelFormat;
            set => SetProperty(ref _subtotalLabelFormat, value, nameof(SubtotalLabelFormat));
        }

        public string ValuesHeaderLabel
        {
            get => _valuesHeaderLabel;
            set => SetProperty(ref _valuesHeaderLabel, value, nameof(ValuesHeaderLabel));
        }

        public double CompactIndentSize
        {
            get => _compactIndentSize;
            set => SetProperty(ref _compactIndentSize, value, nameof(CompactIndentSize));
        }

        public string EmptyValueLabel
        {
            get => _emptyValueLabel;
            set => SetProperty(ref _emptyValueLabel, value, nameof(EmptyValueLabel));
        }

        private void SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
