using System;
using System.Collections.ObjectModel;
using Avalonia.Controls.DataGridFiltering;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ListBoxMimicFilterModelViewModel : ObservableObject
    {
        private const int ItemCount = 20000;
        private const string FilterPropertyPath = "Display";

        private string _summary = "Items: 0";
        private string _filterText = string.Empty;

        public ListBoxMimicFilterModelViewModel()
        {
            Items = new ObservableCollection<ListEntry>();
            Populate();

            FilteringModel = new FilteringModel();
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);
        }

        public ObservableCollection<ListEntry> Items { get; }

        public FilteringModel FilteringModel { get; }

        public RelayCommand ClearFilterCommand { get; }

        public string Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public record ListEntry(int Index, string Name, string Region)
        {
            public string Display => $"{Name} - {Region}";
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(_filterText))
            {
                FilteringModel.Remove(FilterPropertyPath);
                return;
            }

            FilteringModel.SetOrUpdate(new FilteringDescriptor(
                columnId: FilterPropertyPath,
                @operator: FilteringOperator.Contains,
                propertyPath: FilterPropertyPath,
                value: _filterText.Trim(),
                stringComparison: StringComparison.OrdinalIgnoreCase));
        }

        private void Populate()
        {
            Items.Clear();

            var source = Countries.All;
            for (var i = 0; i < ItemCount; i++)
            {
                var country = source[i % source.Count];
                Items.Add(new ListEntry(i + 1, country.Name, country.Region));
            }

            Summary = $"Items: {Items.Count:n0}";
        }
    }
}
