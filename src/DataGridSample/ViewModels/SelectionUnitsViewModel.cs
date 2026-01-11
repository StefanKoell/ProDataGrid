using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class SelectionUnitsViewModel : ObservableObject
    {
        private DataGridSelectionMode _selectionMode = DataGridSelectionMode.Extended;

        public SelectionUnitsViewModel()
        {
            Items = new ObservableCollection<Country>(Countries.All.Take(18).ToList());
            SelectionModes = Enum.GetValues<DataGridSelectionMode>();
        }

        public ObservableCollection<Country> Items { get; }

        public Array SelectionModes { get; }

        public DataGridSelectionMode SelectionMode
        {
            get => _selectionMode;
            set => SetProperty(ref _selectionMode, value);
        }
    }
}
