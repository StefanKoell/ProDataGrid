using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls.DataGridSorting;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ListBoxMimicSortModelViewModel : ObservableObject
    {
        private const int ItemCount = 20000;
        private const string SortPropertyPath = "Display";

        private string _summary = "Items: 0";

        public ListBoxMimicSortModelViewModel()
        {
            Items = new ObservableCollection<ListEntry>();
            Populate();

            SortingModel = new SortingModel();

            SortAscendingCommand = new RelayCommand(_ => ApplySort(ListSortDirection.Ascending));
            SortDescendingCommand = new RelayCommand(_ => ApplySort(ListSortDirection.Descending));
            ClearSortCommand = new RelayCommand(_ => SortingModel.Clear());
        }

        public ObservableCollection<ListEntry> Items { get; }

        public SortingModel SortingModel { get; }

        public RelayCommand SortAscendingCommand { get; }

        public RelayCommand SortDescendingCommand { get; }

        public RelayCommand ClearSortCommand { get; }

        public string Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        public record ListEntry(int Index, string Name, string Region)
        {
            public string Display => $"{Name} - {Region}";
        }

        private void ApplySort(ListSortDirection direction)
        {
            SortingModel.SetOrUpdate(new SortingDescriptor(
                columnId: SortPropertyPath,
                direction: direction,
                propertyPath: SortPropertyPath));
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
