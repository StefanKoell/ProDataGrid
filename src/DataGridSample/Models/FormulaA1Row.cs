using DataGridSample.Mvvm;

namespace DataGridSample.Models
{
    public sealed class FormulaA1Row : ObservableObject
    {
        private string _item = string.Empty;
        private double _units;
        private double _unitPrice;
        private double _cost;
        private bool _isSummary;

        public string Item
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        public double Units
        {
            get => _units;
            set => SetProperty(ref _units, value);
        }

        public double UnitPrice
        {
            get => _unitPrice;
            set => SetProperty(ref _unitPrice, value);
        }

        public double Cost
        {
            get => _cost;
            set => SetProperty(ref _cost, value);
        }

        public bool IsSummary
        {
            get => _isSummary;
            set => SetProperty(ref _isSummary, value);
        }
    }
}
