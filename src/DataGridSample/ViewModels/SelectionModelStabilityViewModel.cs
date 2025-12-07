using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels;

public class SelectionModelStabilityViewModel : ObservableObject
{
    private readonly Random _random = new();
    private bool _syncingSelection;

    public SelectionModelStabilityViewModel()
    {
        Items = new ObservableCollection<Country>(Countries.All.Take(20).ToList());
        SelectedItems = new ObservableCollection<object>();
        SelectionLog = new ObservableCollection<string>();

        SelectedItems.CollectionChanged += OnSelectedItemsChanged;

        ShuffleCommand = new RelayCommand(_ => ShuffleItems());
        SortByNameCommand = new RelayCommand(_ => SortByName());
        AddAtTopCommand = new RelayCommand(_ => AddAtTop());
        RemoveFirstCommand = new RelayCommand(_ => RemoveFirst());
        ClearSelectionCommand = new RelayCommand(_ => SelectedItems.Clear());
    }

    public ObservableCollection<Country> Items { get; }

    public ObservableCollection<object> SelectedItems { get; }

    public ObservableCollection<string> SelectionLog { get; }

    public RelayCommand ShuffleCommand { get; }

    public RelayCommand SortByNameCommand { get; }

    public RelayCommand AddAtTopCommand { get; }

    public RelayCommand RemoveFirstCommand { get; }

    public RelayCommand ClearSelectionCommand { get; }

    private void OnSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        var added = e.NewItems?.Count ?? 0;
        var removed = e.OldItems?.Count ?? 0;
        if (_syncingSelection)
        {
            return;
        }
        SelectionLog.Insert(0, $"Add: {added}, Remove: {removed}, Total: {SelectedItems.Count}");

        if (SelectionLog.Count > 40)
        {
            SelectionLog.RemoveAt(SelectionLog.Count - 1);
        }
    }

    private void ShuffleItems()
    {
        WithSelectionPreserved(() =>
        {
            var shuffled = Items.OrderBy(_ => _random.Next()).ToList();
            Reorder(shuffled);
        });
    }

    private void SortByName()
    {
        WithSelectionPreserved(() =>
        {
            var sorted = Items.OrderBy(x => x.Name).ToList();
            Reorder(sorted);
        });
    }

    private void AddAtTop()
    {
        var nextIndex = Items.Count + 1;
        var population = _random.Next(1_000_000, 10_000_000);
        var area = _random.Next(50_000, 500_000);
        var density = (double)population / area;
        var coast = Math.Round(_random.NextDouble(), 2);
        var birthRate = Math.Round(_random.NextDouble() * 20, 2);
        var deathRate = Math.Round(_random.NextDouble() * 10, 2);

        var country = new Country(
            $"New {nextIndex}",
            "Inserted",
            population,
            area,
            density,
            coast,
            migration: null,
            infantMorality: null,
            gdp: _random.Next(8_000, 30_000),
            literacy: 0.95,
            phones: 0.8,
            birth: birthRate,
            death: deathRate);
        Items.Insert(0, country);
    }

    private void RemoveFirst()
    {
        if (Items.Count > 0)
        {
            Items.RemoveAt(0);
        }
    }

    private void Reorder(IList<Country> ordered)
    {
        for (int targetIndex = 0; targetIndex < ordered.Count; targetIndex++)
        {
            var item = ordered[targetIndex];
            var currentIndex = Items.IndexOf(item);
            if (currentIndex >= 0 && currentIndex != targetIndex)
            {
                Items.Move(currentIndex, targetIndex);
            }
        }
    }

    private void WithSelectionPreserved(Action mutate)
    {
        var snapshot = SelectedItems.OfType<Country>().ToList();
        mutate();

        _syncingSelection = true;
        try
        {
            SelectedItems.Clear();
            foreach (var item in snapshot)
            {
                if (Items.Contains(item))
                {
                    SelectedItems.Add(item);
                }
            }
        }
        finally
        {
            _syncingSelection = false;
        }
    }
}
