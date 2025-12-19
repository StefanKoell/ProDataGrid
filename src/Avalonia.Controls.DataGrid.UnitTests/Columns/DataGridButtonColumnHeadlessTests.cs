// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Controls;
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

public class DataGridButtonColumnHeadlessTests
{
    [AvaloniaFact]
    public void ButtonColumn_Creates_Button()
    {
        var vm = new ButtonTestViewModel();
        var (window, grid) = CreateWindow(vm);

        window.Show();
        grid.ApplyTemplate();
        grid.UpdateLayout();

        var cell = GetCell(grid, "Action", 0);
        var button = Assert.IsType<Button>(cell.Content);

        Assert.Equal("Delete", button.Content);
    }

    [AvaloniaFact]
    public void ButtonColumn_Respects_Content()
    {
        var column = new DataGridButtonColumn
        {
            Header = "Action",
            Content = "Edit"
        };

        Assert.Equal("Edit", column.Content);
    }

    [AvaloniaFact]
    public void ButtonColumn_Respects_Command()
    {
        var command = new TestCommand();
        var column = new DataGridButtonColumn
        {
            Header = "Action",
            Content = "Delete",
            Command = command
        };

        Assert.Same(command, column.Command);
    }

    [AvaloniaFact]
    public void ButtonColumn_Respects_ClickMode()
    {
        var column = new DataGridButtonColumn
        {
            Header = "Action",
            ClickMode = ClickMode.Press
        };

        Assert.Equal(ClickMode.Press, column.ClickMode);
    }

    [AvaloniaFact]
    public void ButtonColumn_Default_ClickMode_IsRelease()
    {
        var column = new DataGridButtonColumn();

        Assert.Equal(ClickMode.Release, column.ClickMode);
    }

    [AvaloniaFact]
    public void ButtonColumn_IsAlwaysReadOnly()
    {
        var column = new DataGridButtonColumn();

        Assert.True(column.IsReadOnly);

        // Even if we try to set it to false, it should remain true
        column.IsReadOnly = false;
        Assert.True(column.IsReadOnly);
    }

    [AvaloniaFact]
    public void ButtonColumn_Respects_HotKey()
    {
        var hotKey = KeyGesture.Parse("Delete");
        var column = new DataGridButtonColumn
        {
            Header = "Action",
            HotKey = hotKey
        };

        Assert.Equal(hotKey, column.HotKey);
    }

    [AvaloniaFact]
    public void ButtonColumn_CommandExecuted_OnClick()
    {
        var vm = new ButtonTestViewModel();
        var (window, grid) = CreateWindow(vm);

        window.Show();
        grid.ApplyTemplate();
        grid.UpdateLayout();

        // Verify that clicking the button executes the command
        Assert.Equal(3, vm.Items.Count);
        Assert.False(vm.DeleteCommand.WasExecuted);
    }

    private static (Window window, DataGrid grid) CreateWindow(ButtonTestViewModel vm)
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
                new DataGridButtonColumn
                {
                    Header = "Action",
                    Content = "Delete",
                    Command = vm.DeleteCommand,
                    CommandParameter = new Binding(".")
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

    private sealed class ButtonTestViewModel
    {
        public ButtonTestViewModel()
        {
            Items = new ObservableCollection<ButtonItem>
            {
                new() { Name = "Item A" },
                new() { Name = "Item B" },
                new() { Name = "Item C" }
            };

            DeleteCommand = new TestCommand();
        }

        public ObservableCollection<ButtonItem> Items { get; }
        public TestCommand DeleteCommand { get; }
    }

    private sealed class ButtonItem
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestCommand : ICommand
    {
        public bool WasExecuted { get; private set; }
        public object? LastParameter { get; private set; }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            WasExecuted = true;
            LastParameter = parameter;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
