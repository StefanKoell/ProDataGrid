using System;
using DataGridSample.Mvvm;

namespace DataGridSample.Models
{
    public sealed class FormulaEngineSalesRecord : ObservableObject
    {
        private DateTime _orderDate;
        private string _region = string.Empty;
        private string _category = string.Empty;
        private double _sales;
        private double _profit;
        private int _quantity;
        private double? _margin;
        private double? _salesPerUnit;
        private double? _profitPerUnit;

        public DateTime OrderDate
        {
            get => _orderDate;
            set => SetProperty(ref _orderDate, value);
        }

        public string Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public double Sales
        {
            get => _sales;
            set => SetProperty(ref _sales, value);
        }

        public double Profit
        {
            get => _profit;
            set => SetProperty(ref _profit, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public double? Margin
        {
            get => _margin;
            set => SetProperty(ref _margin, value);
        }

        public double? SalesPerUnit
        {
            get => _salesPerUnit;
            set => SetProperty(ref _salesPerUnit, value);
        }

        public double? ProfitPerUnit
        {
            get => _profitPerUnit;
            set => SetProperty(ref _profitPerUnit, value);
        }
    }
}
