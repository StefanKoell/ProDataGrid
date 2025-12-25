using System.Collections.ObjectModel;
using Avalonia.Controls.DataGridDragDrop;
using Avalonia.Input;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ListBoxMimicDragDropViewModel : ObservableObject
    {
        private const int ItemCount = 20000;
        private string _summary = "Items: 0";

        public ListBoxMimicDragDropViewModel()
        {
            Items = new ObservableCollection<ListEntry>();
            Populate();

            Options = new DataGridRowDragDropOptions
            {
                AllowedEffects = DragDropEffects.Move
            };
            DropHandler = new DataGridRowReorderHandler();
        }

        public ObservableCollection<ListEntry> Items { get; }

        public DataGridRowDragDropOptions Options { get; }

        public IDataGridRowDropHandler DropHandler { get; }

        public string Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
        }

        public record ListEntry(int Index, string Name, string Region)
        {
            public string Display => $"{Name} - {Region}";
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
