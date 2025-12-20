// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Searching;

public class DataGridSearchNavigationTests
{
    [AvaloniaFact]
    public void Search_Current_Result_Scrolls_Into_View_And_Selects_Cell()
    {
        var items = new ObservableCollection<Person>();
        for (int i = 0; i < 200; i++)
        {
            var name = i == 150 ? "Target" : $"Item {i}";
            items.Add(new Person(name));
        }

        var (grid, root) = CreateGrid(items);
        try
        {
            grid.SearchModel.HighlightMode = SearchHighlightMode.Cell;
            grid.SearchModel.UpdateSelectionOnNavigate = true;

            grid.SearchModel.SetOrUpdate(new SearchDescriptor("Target", comparison: StringComparison.OrdinalIgnoreCase));
            grid.UpdateLayout();

            var targetRow = FindRow(items[150], grid);
            Assert.Equal(150, targetRow.Index);
            Assert.True(grid.CurrentCell.IsValid);
            Assert.Same(items[150], grid.CurrentCell.Item);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void MoveNext_Selects_Next_Result_When_Selection_Update_Enabled()
    {
        var items = new ObservableCollection<Person>
        {
            new("Target One"),
            new("Target Two")
        };

        var (grid, root) = CreateGrid(items);
        try
        {
            grid.SearchModel.HighlightMode = SearchHighlightMode.Cell;
            grid.SearchModel.UpdateSelectionOnNavigate = true;

            grid.SearchModel.SetOrUpdate(new SearchDescriptor("Target", comparison: StringComparison.OrdinalIgnoreCase));
            grid.UpdateLayout();

            Assert.Same(items[0], grid.CurrentCell.Item);

            grid.SearchModel.MoveNext();
            grid.UpdateLayout();

            Assert.Same(items[1], grid.CurrentCell.Item);
        }
        finally
        {
            root.Close();
        }
    }

    private static (DataGrid grid, Window root) CreateGrid(IList<Person> people)
    {
        var root = new Window
        {
            Width = 320,
            Height = 200,
        };

        root.SetThemeStyles();

        var grid = new DataGrid
        {
            HeadersVisibility = DataGridHeadersVisibility.All,
            ItemsSource = people
        };

        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(Person.Name))
        });

        root.Content = grid;
        root.Show();
        grid.UpdateLayout();
        return (grid, root);
    }

    private static DataGridRow FindRow(object item, DataGrid grid)
    {
        return grid
            .GetSelfAndVisualDescendants()
            .OfType<DataGridRow>()
            .First(r => ReferenceEquals(r.DataContext, item));
    }

    private class Person
    {
        public Person(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
