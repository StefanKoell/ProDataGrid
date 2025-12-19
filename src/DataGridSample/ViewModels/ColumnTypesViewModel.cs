using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ColumnTypesViewModel : ObservableObject
    {
        private readonly Random _random = new();

        public ColumnTypesViewModel()
        {
            // Category suggestions for AutoCompleteBox
            CategorySuggestions = new List<string>
            {
                "Electronics",
                "Computers",
                "Accessories",
                "Audio",
                "Video",
                "Storage",
                "Networking",
                "Gaming",
                "Office",
                "Mobile"
            };

            Products = new ObservableCollection<Product>
            {
                new("Laptop Pro", 1299.99m, 0.10m, new DateTime(2024, 1, 15), new TimeSpan(9, 0, 0), 4.5, true, 75, "Electronics", "(555) 123-4567", true, null),
                new("Wireless Mouse", 49.99m, 0.15m, new DateTime(2024, 2, 20), new TimeSpan(8, 30, 0), 4.2, true, 90, "Accessories", "(555) 234-5678", false, null),
                new("Mechanical Keyboard", 129.99m, 0.05m, new DateTime(2024, 3, 10), new TimeSpan(10, 0, 0), 4.8, true, 45, "Accessories", "(555) 345-6789", true, null),
                new("USB-C Hub", 79.99m, 0.20m, new DateTime(2024, 4, 5), new TimeSpan(9, 30, 0), 3.9, false, 20, "Accessories", "(555) 456-7890", false, null),
                new("Monitor 27\"", 399.99m, 0.00m, new DateTime(2024, 5, 1), new TimeSpan(11, 0, 0), 4.6, true, 60, "Video", "(555) 567-8901", true, null),
                new("Webcam HD", 89.99m, 0.25m, new DateTime(2024, 6, 15), new TimeSpan(8, 0, 0), 4.1, true, 85, "Video", "(555) 678-9012", false, null),
                new("Headset Gaming", 149.99m, 0.10m, new DateTime(2024, 7, 20), new TimeSpan(12, 0, 0), 4.4, false, 30, "Audio", "(555) 789-0123", true, null),
                new("External SSD 1TB", 119.99m, 0.05m, new DateTime(2024, 8, 25), new TimeSpan(9, 0, 0), 4.7, true, 55, "Storage", "(555) 890-1234", false, null)
            };

            AddProductCommand = new RelayCommand(_ => AddProduct());
            UpdateProgressCommand = new RelayCommand(_ => UpdateProgress());
            ToggleActiveCommand = new RelayCommand(_ => ToggleActive());
            DeleteProductCommand = new RelayCommand(DeleteProduct, CanDeleteProduct);
            ViewDetailsCommand = new RelayCommand(ViewDetails);
        }

        public ObservableCollection<Product> Products { get; }
        
        public List<string> CategorySuggestions { get; }

        public RelayCommand AddProductCommand { get; }
        public RelayCommand UpdateProgressCommand { get; }
        public RelayCommand ToggleActiveCommand { get; }
        public RelayCommand DeleteProductCommand { get; }
        public RelayCommand ViewDetailsCommand { get; }

        private void AddProduct()
        {
            var names = new[] { "Tablet", "Smartwatch", "Speaker", "Charger", "Cable Pack" };
            var categories = CategorySuggestions;
            var name = names[_random.Next(names.Length)] + " " + (_random.Next(100, 999));
            var price = Math.Round((decimal)(_random.NextDouble() * 500 + 20), 2);
            var discount = Math.Round((decimal)(_random.NextDouble() * 0.3), 2);
            var releaseDate = DateTime.Now.AddDays(-_random.Next(1, 365));
            var availableFrom = new TimeSpan(_random.Next(6, 12), _random.Next(0, 4) * 15, 0);
            var rating = Math.Round(_random.NextDouble() * 5, 1);
            var isActive = _random.Next(2) == 1;
            var stockLevel = _random.Next(0, 101);
            var category = categories[_random.Next(categories.Count)];
            var phone = $"(555) {_random.Next(100, 999):D3}-{_random.Next(1000, 9999):D4}";
            var isFavorite = _random.Next(2) == 1;

            Products.Add(new Product(name, price, discount, releaseDate, availableFrom, rating, isActive, stockLevel, category, phone, isFavorite, null));
        }

        private void UpdateProgress()
        {
            foreach (var product in Products)
            {
                product.StockLevel = Math.Min(100, Math.Max(0, product.StockLevel + _random.Next(-20, 21)));
            }
        }

        private void ToggleActive()
        {
            foreach (var product in Products)
            {
                product.IsActive = !product.IsActive;
            }
        }

        private void DeleteProduct(object? parameter)
        {
            if (parameter is Product product)
            {
                Products.Remove(product);
            }
        }

        private bool CanDeleteProduct(object? parameter)
        {
            return parameter is Product;
        }

        private void ViewDetails(object? parameter)
        {
            if (parameter is Product product)
            {
                // In a real app, this would open a details view
                System.Diagnostics.Debug.WriteLine($"View details for: {product.Name}");
            }
        }
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
    public class Product : ObservableObject
    {
        private string _name;
        private decimal _price;
        private decimal _discountPercent;
        private DateTime? _releaseDate;
        private TimeSpan? _availableFrom;
        private double _rating;
        private bool _isActive;
        private double _stockLevel;
        private string? _category;
        private string? _phone;
        private bool _isFavorite;
        private string? _imagePath;

        public Product(string name, decimal price, decimal discountPercent, DateTime? releaseDate, 
                       TimeSpan? availableFrom, double rating, bool isActive, double stockLevel,
                       string? category, string? phone, bool isFavorite, string? imagePath)
        {
            _name = name;
            _price = price;
            _discountPercent = discountPercent;
            _releaseDate = releaseDate;
            _availableFrom = availableFrom;
            _rating = rating;
            _isActive = isActive;
            _stockLevel = stockLevel;
            _category = category;
            _phone = phone;
            _isFavorite = isFavorite;
            _imagePath = imagePath;
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public decimal DiscountPercent
        {
            get => _discountPercent;
            set => SetProperty(ref _discountPercent, value);
        }

        public DateTime? ReleaseDate
        {
            get => _releaseDate;
            set => SetProperty(ref _releaseDate, value);
        }

        public TimeSpan? AvailableFrom
        {
            get => _availableFrom;
            set => SetProperty(ref _availableFrom, value);
        }

        public double Rating
        {
            get => _rating;
            set => SetProperty(ref _rating, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public double StockLevel
        {
            get => _stockLevel;
            set => SetProperty(ref _stockLevel, value);
        }

        public string? Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string? Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public bool IsFavorite
        {
            get => _isFavorite;
            set => SetProperty(ref _isFavorite, value);
        }

        public string? ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }
    }
}
