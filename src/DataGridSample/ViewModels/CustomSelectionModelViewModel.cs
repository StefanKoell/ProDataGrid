using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.Selection;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels;

public class CustomSelectionModelViewModel : ObservableObject
{
    private int _counter = 1000;

    public CustomSelectionModelViewModel()
    {
        Items = new ObservableCollection<Country>(Countries.All.Take(12).ToList());

        SelectionModel = new SelectionModel<Country>
        {
            SingleSelect = false
        };

        SelectEvenCommand = new RelayCommand(_ => SelectEvenRows());
        ClearCommand = new RelayCommand(_ => ClearSelection());
        AddDynamicCommand = new RelayCommand(_ => AddDynamicItem());
        RemoveLastCommand = new RelayCommand(_ => RemoveLast());
        ShuffleCommand = new RelayCommand(_ => Shuffle());
    }

    public ObservableCollection<Country> Items { get; }

    public SelectionModel<Country> SelectionModel { get; }

    public RelayCommand SelectEvenCommand { get; }

    public RelayCommand ClearCommand { get; }

    public RelayCommand AddDynamicCommand { get; }

    public RelayCommand RemoveLastCommand { get; }

    public RelayCommand ShuffleCommand { get; }

    private void SelectEvenRows()
    {
        using (SelectionModel.BatchUpdate())
        {
            SelectionModel.Clear();
            for (int i = 0; i < Items.Count; i += 2)
            {
                SelectionModel.Select(i);
            }
        }
    }

    private void ClearSelection()
    {
        SelectionModel.Clear();
    }

    private void AddDynamicItem()
    {
        _counter++;
        var population = 500_000 + _counter * 1000;
        var area = 40_000 + _counter % 5 * 5_000;
        var density = (double)population / area;
        var coast = Math.Round(_counter % 10 * 0.1, 2);
        var birthRate = Math.Round(8 + _counter % 5 * 0.5, 2);
        var deathRate = Math.Round(4 + _counter % 3 * 0.3, 2);
        var country = new Country(
            $"Dynamic {_counter}",
            _counter % 2 == 0 ? "Dynamic Even" : "Dynamic Odd",
            population,
            area,
            density,
            coast,
            migration: null,
            infantMorality: null,
            gdp: 12_000 + _counter % 5 * 2_000,
            literacy: 0.9,
            phones: 0.7,
            birth: birthRate,
            death: deathRate);
        Items.Insert(0, country);
    }

    private void RemoveLast()
    {
        if (Items.Count == 0)
        {
            return;
        }

        Items.RemoveAt(Items.Count - 1);
    }

    private void Shuffle()
    {
        var random = new Random();
        var shuffled = Items.OrderBy(_ => random.Next()).ToList();
        Reorder(shuffled);
    }

    private void Reorder(IList<Country> ordered)
    {
        var snapshot = SelectionModel.SelectedItems.ToList();

        for (int targetIndex = 0; targetIndex < ordered.Count; targetIndex++)
        {
            var item = ordered[targetIndex];
            var currentIndex = Items.IndexOf(item);
            if (currentIndex >= 0 && currentIndex != targetIndex)
            {
                Items.Move(currentIndex, targetIndex);
            }
        }

        using (SelectionModel.BatchUpdate())
        {
            SelectionModel.Clear();
            foreach (var item in snapshot)
            {
                var index = Items.IndexOf(item);
                if (index >= 0)
                {
                    SelectionModel.Select(index);
                }
            }
        }
    }
}
