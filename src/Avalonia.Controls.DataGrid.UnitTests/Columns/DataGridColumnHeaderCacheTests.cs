// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Columns;

public class DataGridColumnHeaderCacheTests
{
    [AvaloniaFact]
    public void HeaderCell_Binding_Does_Not_Update_After_ClearElementCache()
    {
        var column = new DataGridTextColumn
        {
            Header = "Initial"
        };

        var header = column.HeaderCell;

        Assert.Equal("Initial", header.Content);

        column.ClearElementCache();
        column.Header = "Updated";

        Assert.Null(header.OwningColumn);
        Assert.Null(header.Content);
    }

    [AvaloniaFact]
    public void HeaderCell_Binding_Does_Not_Update_After_Column_Removed_From_Grid()
    {
        var grid = new DataGrid();
        var column = new DataGridTextColumn
        {
            Header = "Initial"
        };

        grid.ColumnsInternal.Add(column);

        var header = column.HeaderCell;

        Assert.Equal("Initial", header.Content);

        grid.ColumnsInternal.Remove(column);
        column.Header = "Updated";

        Assert.Null(header.OwningColumn);
        Assert.Null(header.Content);
    }

    [AvaloniaFact]
    public void HeaderCell_Is_Removed_From_Presenter_When_Column_Removed()
    {
        var items = new ObservableCollection<Item>
        {
            new("Row")
        };

        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = items,
            HeadersVisibility = DataGridHeadersVisibility.Column
        };

        var column = new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(Item.Name))
        };
        grid.Columns = new ObservableCollection<DataGridColumn>
        {
            column
        };

        var window = new Window
        {
            Width = 400,
            Height = 300,
        };
        window.SetThemeStyles();
        window.Content = grid;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.ApplyTemplate();
            window.UpdateLayout();
            grid.ApplyTemplate();
            grid.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var header = column.HeaderCell;
            var presenter = grid.ColumnHeaders;
            Assert.NotNull(presenter);

            Assert.Contains(header, presenter.Children);

            grid.Columns.Remove(column);
            grid.UpdateLayout();

            Assert.DoesNotContain(header, presenter.Children);
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class Item
    {
        public Item(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
