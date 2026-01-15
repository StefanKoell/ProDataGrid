using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Selection;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels;

public class SelectionItemsSourceResetViewModel : ObservableObject
{
    private const int MaxLogEntries = 40;
    private readonly ObservableCollection<SelectionFilterItem> _fullItems;
    private readonly ObservableCollection<SelectionFilterItem> _shortItems;
    private readonly DataGridCollectionView _fullView;
    private readonly DataGridCollectionView _shortView;
    private IEnumerable? _itemsSource;
    private string _status = string.Empty;

    public SelectionItemsSourceResetViewModel()
    {
        _fullItems = BuildItems(8);
        _shortItems = BuildItems(3);
        _fullView = new DataGridCollectionView(_fullItems);
        _shortView = new DataGridCollectionView(_shortItems);

        SelectionModel = new SelectionModel<SelectionFilterItem> { SingleSelect = false };
        SelectionLog = new ObservableCollection<string>();

        SelectLastRowCommand = new RelayCommand(_ => SelectLastRow());
        FilterOutSelectedCommand = new RelayCommand(_ => FilterOutSelected());
        ClearFilterCommand = new RelayCommand(_ => ClearFilter());
        UseShortListCommand = new RelayCommand(_ => UseShortList());
        UseFullListCommand = new RelayCommand(_ => UseFullList());

        SelectionModel.SelectionChanged += OnSelectionChanged;
        SelectionModel.PropertyChanged += OnSelectionModelPropertyChanged;

        ItemsSource = _fullView;
        UpdateStatus();
    }

    public SelectionModel<SelectionFilterItem> SelectionModel { get; }

    public ObservableCollection<string> SelectionLog { get; }

    public IEnumerable? ItemsSource
    {
        get => _itemsSource;
        private set => SetProperty(ref _itemsSource, value);
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public RelayCommand SelectLastRowCommand { get; }
    public RelayCommand FilterOutSelectedCommand { get; }
    public RelayCommand ClearFilterCommand { get; }
    public RelayCommand UseShortListCommand { get; }
    public RelayCommand UseFullListCommand { get; }

    private void SelectLastRow()
    {
        var view = CurrentView;
        if (view == null || view.Count == 0)
        {
            return;
        }

        SelectionModel.Select(view.Count - 1);
    }

    private void FilterOutSelected()
    {
        var view = CurrentView;
        if (view == null)
        {
            return;
        }

        var selected = SelectionModel.SelectedItems.OfType<SelectionFilterItem>().FirstOrDefault();
        if (selected == null)
        {
            view.Filter = null;
        }
        else
        {
            view.Filter = item => !ReferenceEquals(item, selected);
        }

        UpdateStatus();
    }

    private void ClearFilter()
    {
        var view = CurrentView;
        if (view == null)
        {
            return;
        }

        view.Filter = null;
        UpdateStatus();
    }

    private void UseShortList()
    {
        ItemsSource = _shortView;
        UpdateStatus();
    }

    private void UseFullList()
    {
        ItemsSource = _fullView;
        UpdateStatus();
    }

    private DataGridCollectionView? CurrentView => ItemsSource as DataGridCollectionView;

    private void OnSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        var selected = SelectionModel.SelectedItems
            .OfType<SelectionFilterItem>()
            .Select(item => item.Name)
            .ToList();

        var added = e.SelectedItems?.Count ?? 0;
        var removed = e.DeselectedItems?.Count ?? 0;
        var summary = selected.Count == 0 ? "none" : string.Join(", ", selected);

        SelectionLog.Insert(0, $"Add: {added}, Remove: {removed}, Selected: {summary}");

        if (SelectionLog.Count > MaxLogEntries)
        {
            SelectionLog.RemoveAt(SelectionLog.Count - 1);
        }

        UpdateStatus();
    }

    private void OnSelectionModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectionModel<SelectionFilterItem>.Source) ||
            e.PropertyName == nameof(SelectionModel<SelectionFilterItem>.SelectedIndex) ||
            e.PropertyName == nameof(SelectionModel<SelectionFilterItem>.SelectedItem))
        {
            UpdateStatus();
        }
    }

    private void UpdateStatus()
    {
        var view = CurrentView;
        var source = ReferenceEquals(view, _fullView)
            ? "Full list"
            : ReferenceEquals(view, _shortView)
                ? "Short list"
                : "None";
        var count = view?.Count ?? 0;
        var filter = view?.Filter != null ? "on" : "off";

        Status = $"Source: {source} | Count: {count} | Filter: {filter} | Selected: {SelectionModel.SelectedItems.Count}";
    }

    private static ObservableCollection<SelectionFilterItem> BuildItems(int count)
    {
        var items = new ObservableCollection<SelectionFilterItem>();
        for (var i = 0; i < count; i++)
        {
            items.Add(new SelectionFilterItem
            {
                Name = $"Item {i + 1}",
                Group = i % 2 == 0 ? "A" : "B",
                Value = i
            });
        }

        return items;
    }
}
