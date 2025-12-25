using System.Collections.ObjectModel;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ListBoxMimicViewModel : ObservableObject
    {
        private const int ItemCount = 20000;
        private string _summary = "Items: 0";

        public ListBoxMimicViewModel()
        {
            Items = new ObservableCollection<ListEntry>();
            Populate();
        }

        public ObservableCollection<ListEntry> Items { get; }

        public string Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
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

        public record ListEntry(int Index, string Name, string Region)
        {
            public string Display => $"{Name} - {Region}";
        }
    }
}
