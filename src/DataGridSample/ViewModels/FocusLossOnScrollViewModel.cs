using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using DataGridSample.Models;
using ReactiveUI;

namespace DataGridSample.ViewModels;

internal sealed class FocusLossOnScrollViewModel : ReactiveObject
{
    private readonly DispatcherTimer _focusTimer;
    private IInputElement? _lastFocusedElement;
    private string _focusedElementDescription = "None";

    public FocusLossOnScrollViewModel()
    {
        Items = new ObservableCollection<Person>(CreateItems());
        _focusTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(120)
        };
        _focusTimer.Tick += (_, _) => UpdateFocusedElement();
        _focusTimer.Start();
        UpdateFocusedElement();
    }

    public ObservableCollection<Person> Items { get; }

    public string FocusedElementDescription
    {
        get => _focusedElementDescription;
        private set => this.RaiseAndSetIfChanged(ref _focusedElementDescription, value);
    }

    private static List<Person> CreateItems()
    {
        var firstNames = new[]
        {
            "Ada", "Alan", "Grace", "Linus", "Margaret", "Tim", "Dennis", "Barbara", "Ken", "Guido",
            "Edsger", "Radia", "Leslie", "Donald", "Frances", "James", "Anita", "Bjarne", "Sophie", "Niklaus"
        };

        var lastNames = new[]
        {
            "Lovelace", "Turing", "Hopper", "Torvalds", "Hamilton", "Berners-Lee", "Ritchie", "Liskov",
            "Thompson", "van Rossum", "Dijkstra", "Perlman", "Lamport", "Knuth", "Allen", "Gosling",
            "Borg", "Stroustrup", "Wilson", "Wirth"
        };

        var items = new List<Person>(220);
        for (var i = 0; i < 220; i++)
        {
            var first = firstNames[i % firstNames.Length];
            var last = lastNames[(i + 7) % lastNames.Length];

            items.Add(new Person
            {
                FirstName = first,
                LastName = last,
                Age = 20 + (i % 50),
                IsBanned = i % 9 == 0
            });
        }

        return items;
    }

    private void UpdateFocusedElement()
    {
        var focusedElement = GetFocusedElement();
        if (ReferenceEquals(focusedElement, _lastFocusedElement))
        {
            return;
        }

        _lastFocusedElement = focusedElement;
        FocusedElementDescription = DescribeFocusedElement(focusedElement);
    }

    private static IInputElement? GetFocusedElement()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return null;
        }

        return desktop.MainWindow?.FocusManager?.GetFocusedElement();
    }

    private static string DescribeFocusedElement(IInputElement? element)
    {
        if (element == null)
        {
            return "None";
        }

        if (element is DataGridCell cell)
        {
            var rowIndex = cell.OwningRow?.Index ?? -1;
            var columnHeader = cell.OwningColumn?.Header?.ToString();
            if (rowIndex >= 0 || !string.IsNullOrWhiteSpace(columnHeader))
            {
                var headerText = string.IsNullOrWhiteSpace(columnHeader) ? "Column" : columnHeader;
                return $"DataGridCell (Row {rowIndex}, {headerText})";
            }

            return "DataGridCell";
        }

        if (element is DataGridRow row)
        {
            var rowIndex = row.Index;
            return rowIndex >= 0 ? $"DataGridRow (Row {rowIndex})" : "DataGridRow";
        }

        if (element is Control control)
        {
            var name = string.IsNullOrWhiteSpace(control.Name) ? null : control.Name;
            return name == null ? control.GetType().Name : $"{control.GetType().Name} \"{name}\"";
        }

        return element.GetType().Name;
    }
}
