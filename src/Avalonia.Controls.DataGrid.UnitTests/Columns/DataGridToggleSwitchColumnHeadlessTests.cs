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

public class DataGridToggleSwitchColumnHeadlessTests
{
    [AvaloniaFact]
    public void ToggleSwitchColumn_Binds_Value()
    {
        var vm = new ToggleSwitchTestViewModel();
        var (window, grid) = CreateWindow(vm);

        window.Show();
        grid.ApplyTemplate();
        grid.UpdateLayout();

        var cell = GetCell(grid, "Active", 0);
        var toggleSwitch = Assert.IsType<ToggleSwitch>(cell.Content);

        Assert.True(toggleSwitch.IsChecked);
    }

    [AvaloniaFact]
    public void ToggleSwitchColumn_Respects_OnOffContent()
    {
        var column = new DataGridToggleSwitchColumn
        {
            Header = "Active",
            OnContent = "Yes",
            OffContent = "No"
        };

        Assert.Equal("Yes", column.OnContent);
        Assert.Equal("No", column.OffContent);
    }

    [AvaloniaFact]
    public void ToggleSwitchColumn_Respects_IsThreeState()
    {
        var column = new DataGridToggleSwitchColumn
        {
            Header = "State",
            IsThreeState = true
        };

        Assert.True(column.IsThreeState);
    }

    [AvaloniaFact]
    public void ToggleSwitchColumn_Default_IsThreeState_IsFalse()
    {
        var column = new DataGridToggleSwitchColumn();

        Assert.False(column.IsThreeState);
    }

    private static (Window window, DataGrid grid) CreateWindow(ToggleSwitchTestViewModel vm)
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
                new DataGridToggleSwitchColumn
                {
                    Header = "Active",
                    Binding = new Binding("IsActive")
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

    private sealed class ToggleSwitchTestViewModel
    {
        public ToggleSwitchTestViewModel()
        {
            Items = new ObservableCollection<ToggleSwitchItem>
            {
                new() { Name = "Feature A", IsActive = true },
                new() { Name = "Feature B", IsActive = false },
                new() { Name = "Feature C", IsActive = true }
            };
        }

        public ObservableCollection<ToggleSwitchItem> Items { get; }
    }

    private sealed class ToggleSwitchItem
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
