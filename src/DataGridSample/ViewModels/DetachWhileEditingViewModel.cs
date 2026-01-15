using System.Collections.ObjectModel;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class DetachWhileEditingViewModel : ObservableObject
    {
        private bool _showGrid = true;

        public DetachWhileEditingViewModel()
        {
            Items = new ObservableCollection<DetachWhileEditingItem>
            {
                new DetachWhileEditingItem { Name = "Alpha", Quantity = 12, Note = "Edit and detach" },
                new DetachWhileEditingItem { Name = "Beta", Quantity = 7, Note = "Toggle below" },
                new DetachWhileEditingItem { Name = "Gamma", Quantity = 3, Note = "Should not throw" }
            };
        }

        public ObservableCollection<DetachWhileEditingItem> Items { get; }

        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                if (SetProperty(ref _showGrid, value))
                {
                    OnPropertyChanged(nameof(GridHost));
                }
            }
        }

        public object? GridHost => ShowGrid ? this : null;
    }

    public class DetachWhileEditingItem
    {
        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string Note { get; set; } = string.Empty;
    }
}
