using System.Collections.ObjectModel;
using Avalonia.Collections;
using Avalonia.Controls.Selection;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels;

public class SharedSelectionViewModel : ObservableObject
{
    public SharedSelectionViewModel()
    {
        Items = new ObservableCollection<Country>(Countries.All);
        ItemsView = new DataGridCollectionView(Items);
        SelectionModel = new SelectionModel<Country>
        {
            SingleSelect = false,
            Source = ItemsView
        };
    }

    public ObservableCollection<Country> Items { get; }

    public DataGridCollectionView ItemsView { get; }

    public SelectionModel<Country> SelectionModel { get; }
}
