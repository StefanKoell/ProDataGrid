// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia.Themes.Fluent;
using System.Linq;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Columns;

public class DataGridProgressBarColumnHeadlessTests
{
    [AvaloniaFact]
    public void ProgressBarColumn_Binds_Value()
    {
        var vm = new ProgressBarTestViewModel();
        var (window, grid) = CreateWindow(vm);

        window.Show();
        grid.ApplyTemplate();
        grid.UpdateLayout();

        var cell = GetCell(grid, "Progress", 0);
        var progressBar = Assert.IsType<ProgressBar>(cell.Content);

        Assert.Equal(75, progressBar.Value);
    }

    [AvaloniaFact]
    public void ProgressBarColumn_Respects_MinMax()
    {
        var column = new DataGridProgressBarColumn
        {
            Header = "Progress",
            Minimum = 0,
            Maximum = 200
        };

        Assert.Equal(0, column.Minimum);
        Assert.Equal(200, column.Maximum);
    }

    [AvaloniaFact]
    public void ProgressBarColumn_Respects_ShowProgressText()
    {
        var column = new DataGridProgressBarColumn
        {
            Header = "Progress",
            ShowProgressText = true
        };

        Assert.True(column.ShowProgressText);
    }

    [AvaloniaFact]
    public void ProgressBarColumn_IsAlwaysReadOnly()
    {
        var column = new DataGridProgressBarColumn
        {
            Header = "Progress"
        };

        Assert.True(column.IsReadOnly);

        // Try to set it to false - should still be true
        column.IsReadOnly = false;
        Assert.True(column.IsReadOnly);
    }

    [AvaloniaFact]
    public void ProgressBarColumn_Default_MinMax()
    {
        var column = new DataGridProgressBarColumn();

        Assert.Equal(0, column.Minimum);
        Assert.Equal(100, column.Maximum);
    }

    private static (Window window, DataGrid grid) CreateWindow(ProgressBarTestViewModel vm)
    {
        var window = new Window
        {
            Width = 600,
            Height = 400,
            DataContext = vm
        };

        window.SetThemeStyles();

        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            ItemsSource = vm.Items,
            Columns = new ObservableCollection<DataGridColumn>
            {
                new DataGridTextColumn
                {
                    Header = "Name",
                    Binding = new Binding("Name")
                },
                new DataGridProgressBarColumn
                {
                    Header = "Progress",
                    Binding = new Binding("Progress"),
                    ShowProgressText = true
                }
            }
        };

        window.Content = grid;
        return (window, grid);
    }

    private static DataGridCell GetCell(DataGrid grid, string header, int rowIndex)
    {
        return grid
            .GetVisualDescendants()
            .OfType<DataGridCell>()
            .First(c => c.OwningColumn?.Header?.ToString() == header && c.OwningRow?.Index == rowIndex);
    }

    private sealed class ProgressBarTestViewModel
    {
        public ProgressBarTestViewModel()
        {
            Items = new ObservableCollection<ProgressItem>
            {
                new() { Name = "Task 1", Progress = 75 },
                new() { Name = "Task 2", Progress = 30 },
                new() { Name = "Task 3", Progress = 100 }
            };
        }

        public ObservableCollection<ProgressItem> Items { get; }
    }

    private sealed class ProgressItem
    {
        public string Name { get; set; } = string.Empty;
        public double Progress { get; set; }
    }
}
