using DataGridSample.Mvvm;

namespace DataGridSample.Models
{
    public sealed class FormulaEditingRow : ObservableObject
    {
        private string _item = string.Empty;
        private double _quantity;
        private double _unitPrice;
        private double _cost;

        public string Item
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        public double Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
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
    }

    public sealed class FormulaSpillRow : ObservableObject
    {
        private string _label = string.Empty;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }
    }

    public sealed class FormulaRecalcRow : ObservableObject
    {
        private double _input;
        private double _factor;

        public double Input
        {
            get => _input;
            set => SetProperty(ref _input, value);
        }

        public double Factor
        {
            get => _factor;
            set => SetProperty(ref _factor, value);
        }
    }
}
