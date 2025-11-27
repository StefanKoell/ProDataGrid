using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class LargeUniformViewModel : ObservableObject
    {
        private int _itemCount = 200_000;
        private string _summary = "Items: 0";
        private string _selectedEstimator = "Advanced";

        public LargeUniformViewModel()
        {
            Items = new ObservableCollection<PixelItem>();
            Estimators = new[] { "Advanced", "Caching", "Default" };
            RegenerateCommand = new RelayCommand(_ => Populate());
            Populate();
        }

        public ObservableCollection<PixelItem> Items { get; }

        public ICommand RegenerateCommand { get; }

        public IReadOnlyList<string> Estimators { get; }

        public int ItemCount
        {
            get => _itemCount;
            set => SetProperty(ref _itemCount, value);
        }

        public string Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        public string SelectedEstimator
        {
            get => _selectedEstimator;
            set => SetProperty(ref _selectedEstimator, value);
        }

        private void Populate()
        {
            Items.Clear();

            var random = new Random(17);
            for (int i = 1; i <= ItemCount; i++)
            {
                Items.Add(PixelItem.Create(i, random));
            }

            Summary = $"Items: {Items.Count:n0}";
        }
    }
}
