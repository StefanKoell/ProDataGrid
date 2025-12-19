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

public class DataGridTimePickerColumnHeadlessTests
{
    [AvaloniaFact]
    public void TimePickerColumn_Binds_Value()
    {
        var vm = new TimePickerTestViewModel();
        var (window, grid) = CreateWindow(vm);

        window.Show();
        grid.ApplyTemplate();
        grid.UpdateLayout();

        var cell = GetCell(grid, "Time", 0);
        var textBlock = Assert.IsType<TextBlock>(cell.Content);

        Assert.NotNull(textBlock.Text);
    }

    [AvaloniaFact]
    public void TimePickerColumn_Respects_ClockIdentifier()
    {
        var column = new DataGridTimePickerColumn
        {
            Header = "Time",
            ClockIdentifier = "24HourClock"
        };

        Assert.Equal("24HourClock", column.ClockIdentifier);
    }

    [AvaloniaFact]
    public void TimePickerColumn_Respects_MinuteIncrement()
    {
        var column = new DataGridTimePickerColumn
        {
            Header = "Time",
            MinuteIncrement = 15
        };

        Assert.Equal(15, column.MinuteIncrement);
    }

    [AvaloniaFact]
    public void TimePickerColumn_Respects_UseSeconds()
    {
        var column = new DataGridTimePickerColumn
        {
            Header = "Time",
            UseSeconds = true
        };

        Assert.True(column.UseSeconds);
    }

    [AvaloniaFact]
    public void TimePickerColumn_Default_ClockIdentifier_Is12Hour()
    {
        var column = new DataGridTimePickerColumn();

        Assert.Equal("12HourClock", column.ClockIdentifier);
    }

    private static (Window window, DataGrid grid) CreateWindow(TimePickerTestViewModel vm)
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
                new DataGridTimePickerColumn
                {
                    Header = "Time",
                    Binding = new Binding("Time")
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

    private sealed class TimePickerTestViewModel
    {
        public TimePickerTestViewModel()
        {
            Items = new ObservableCollection<TimeItem>
            {
                new() { Name = "Morning", Time = new TimeSpan(9, 0, 0) },
                new() { Name = "Afternoon", Time = new TimeSpan(14, 30, 0) },
                new() { Name = "Evening", Time = new TimeSpan(18, 45, 0) }
            };
        }

        public ObservableCollection<TimeItem> Items { get; }
    }

    private sealed class TimeItem
    {
        public string Name { get; set; } = string.Empty;
        public TimeSpan? Time { get; set; }
    }
}
