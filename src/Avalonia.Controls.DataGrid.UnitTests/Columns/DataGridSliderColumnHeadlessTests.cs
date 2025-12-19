// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia.Themes.Fluent;
using System.Linq;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Columns;

public class DataGridSliderColumnHeadlessTests
{
    [AvaloniaFact]
    public void SliderColumn_Binds_Value()
    {
        var vm = new SliderTestViewModel();
        var (window, grid) = CreateWindow(vm);

        window.Show();
        grid.ApplyTemplate();
        grid.UpdateLayout();

        var cell = GetCell(grid, "Rating", 0);
        var slider = Assert.IsType<Slider>(cell.Content);

        Assert.Equal(4.5, slider.Value, 1);
    }

    [AvaloniaFact]
    public void SliderColumn_Respects_MinMax()
    {
        var column = new DataGridSliderColumn
        {
            Header = "Rating",
            Minimum = 1,
            Maximum = 10
        };

        Assert.Equal(1, column.Minimum);
        Assert.Equal(10, column.Maximum);
    }

    [AvaloniaFact]
    public void SliderColumn_Respects_TickFrequency()
    {
        var column = new DataGridSliderColumn
        {
            Header = "Rating",
            TickFrequency = 0.5,
            IsSnapToTickEnabled = true
        };

        Assert.Equal(0.5, column.TickFrequency);
        Assert.True(column.IsSnapToTickEnabled);
    }

    [AvaloniaFact]
    public void SliderColumn_ShowValueText_DisplaysTextBlock()
    {
        var vm = new SliderTestViewModel();
        var (window, grid) = CreateWindow(vm, showValueText: true);

        window.Show();
        grid.ApplyTemplate();
        grid.UpdateLayout();

        var cell = GetCell(grid, "Rating", 0);
        var textBlock = Assert.IsType<TextBlock>(cell.Content);

        Assert.NotNull(textBlock.Text);
    }

    [AvaloniaFact]
    public void SliderColumn_Default_MinMax()
    {
        var column = new DataGridSliderColumn();

        Assert.Equal(0, column.Minimum);
        Assert.Equal(100, column.Maximum);
    }

    private static (Window window, DataGrid grid) CreateWindow(SliderTestViewModel vm, bool showValueText = false)
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
                new DataGridSliderColumn
                {
                    Header = "Rating",
                    Binding = new Binding("Rating"),
                    Minimum = 0,
                    Maximum = 5,
                    ShowValueText = showValueText
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

    private sealed class SliderTestViewModel
    {
        public SliderTestViewModel()
        {
            Items = new ObservableCollection<SliderItem>
            {
                new() { Name = "Product A", Rating = 4.5 },
                new() { Name = "Product B", Rating = 3.0 },
                new() { Name = "Product C", Rating = 5.0 }
            };
        }

        public ObservableCollection<SliderItem> Items { get; }
    }

    private sealed class SliderItem
    {
        public string Name { get; set; } = string.Empty;
        public double Rating { get; set; }
    }
}
