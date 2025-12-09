// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Sorting;

public class DataGridSortingPropertyTests
{
    [AvaloniaFact]
    public void Custom_Sorting_Model_Applies_To_View()
    {
        var items = new ObservableCollection<Item>
        {
            new("B"),
            new("A"),
            new("C")
        };

        var sortingModel = new SortingModel();
        var grid = CreateGrid(items, sortingModel);
        grid.UpdateLayout();

        sortingModel.Apply(new[]
        {
            new SortingDescriptor("Name", ListSortDirection.Ascending, nameof(Item.Name))
        });

        grid.UpdateLayout();

        var view = Assert.IsType<DataGridCollectionView>(grid.ItemsSource);
        var sort = Assert.Single(view.SortDescriptions);
        Assert.Equal(nameof(Item.Name), sort.PropertyPath);
        Assert.Equal(ListSortDirection.Ascending, sort.Direction);
        Assert.Same(sortingModel, grid.SortingModel);
    }

    [Fact]
    public void Sorting_Property_Raises_PropertyChanged_On_Replace()
    {
        var grid = new DataGrid();
        var newModel = new SortingModel();
        var propertyNames = new List<string>();

        grid.PropertyChanged += (_, e) =>
        {
            if (e.Property == DataGrid.SortingModelProperty)
            {
                propertyNames.Add(e.Property.Name);
                Assert.Same(newModel, e.NewValue);
            }
        };

        grid.SortingModel = newModel;

        Assert.Equal(new[] { nameof(DataGrid.SortingModel) }, propertyNames);
    }

    private static DataGrid CreateGrid(IEnumerable<Item> items, ISortingModel sortingModel)
    {
        var root = new Window
        {
            Width = 250,
            Height = 150,
            Styles =
            {
                new StyleInclude((Uri?)null)
                {
                    Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Simple.xaml")
                },
            }
        };

        var view = new DataGridCollectionView(items);

        var grid = new DataGrid
        {
            ItemsSource = view,
            SortingModel = sortingModel,
            CanUserSortColumns = true,
            AutoGenerateColumns = false
        };

        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(Item.Name)),
            SortMemberPath = nameof(Item.Name)
        });

        root.Content = grid;
        root.Show();
        return grid;
    }

    public record Item(string Name);
}
