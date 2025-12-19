// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia.Themes.Fluent;
using System.Linq;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Columns;

public class DataGridToggleButtonColumnHeadlessTests
{
    [AvaloniaFact]
    public void ToggleButtonColumn_Binds_Value()
    {
        var vm = new ToggleButtonTestViewModel();
        var (window, grid) = CreateWindow(vm);

        window.Show();
        grid.ApplyTemplate();
        grid.UpdateLayout();

        var cell = GetCell(grid, "Favorite", 0);
        var toggleButton = Assert.IsType<ToggleButton>(cell.Content);

        Assert.True(toggleButton.IsChecked);
    }

    [AvaloniaFact]
    public void ToggleButtonColumn_Respects_Content()
    {
        var column = new DataGridToggleButtonColumn
        {
            Header = "Favorite",
            Content = "★"
        };

        Assert.Equal("★", column.Content);
    }

    [AvaloniaFact]
    public void ToggleButtonColumn_Respects_CheckedContent()
    {
        var column = new DataGridToggleButtonColumn
        {
            Header = "Favorite",
            CheckedContent = "★"
        };

        Assert.Equal("★", column.CheckedContent);
    }

    [AvaloniaFact]
    public void ToggleButtonColumn_Respects_UncheckedContent()
    {
        var column = new DataGridToggleButtonColumn
        {
            Header = "Favorite",
            UncheckedContent = "☆"
        };

        Assert.Equal("☆", column.UncheckedContent);
    }

    [AvaloniaFact]
    public void ToggleButtonColumn_Respects_IsThreeState()
    {
        var column = new DataGridToggleButtonColumn
        {
            Header = "State",
            IsThreeState = true
        };

        Assert.True(column.IsThreeState);
    }

    [AvaloniaFact]
    public void ToggleButtonColumn_Default_IsThreeState_IsFalse()
    {
        var column = new DataGridToggleButtonColumn();

        Assert.False(column.IsThreeState);
    }

    [AvaloniaFact]
    public void ToggleButtonColumn_Respects_ClickMode()
    {
        var column = new DataGridToggleButtonColumn
        {
            Header = "Favorite",
            ClickMode = ClickMode.Press
        };

        Assert.Equal(ClickMode.Press, column.ClickMode);
    }

    [AvaloniaFact]
    public void ToggleButtonColumn_Default_ClickMode_IsRelease()
    {
        var column = new DataGridToggleButtonColumn();

        Assert.Equal(ClickMode.Release, column.ClickMode);
    }

    private static (Window window, DataGrid grid) CreateWindow(ToggleButtonTestViewModel vm)
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
                new DataGridToggleButtonColumn
                {
                    Header = "Favorite",
                    Binding = new Binding("IsFavorite"),
                    Content = "★"
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

    private sealed class ToggleButtonTestViewModel
    {
        public ToggleButtonTestViewModel()
        {
            Items = new ObservableCollection<ToggleButtonItem>
            {
                new() { Name = "Item A", IsFavorite = true },
                new() { Name = "Item B", IsFavorite = false },
                new() { Name = "Item C", IsFavorite = true }
            };
        }

        public ObservableCollection<ToggleButtonItem> Items { get; }
    }

    private sealed class ToggleButtonItem
    {
        public string Name { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
    }
}
