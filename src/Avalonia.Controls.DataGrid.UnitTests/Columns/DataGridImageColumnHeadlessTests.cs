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
using Avalonia.Media;
using System.Linq;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Columns;

public class DataGridImageColumnHeadlessTests
{
    [AvaloniaFact]
    public void ImageColumn_Generates_ImageElement()
    {
        var vm = new ImageTestViewModel();
        var (window, grid) = CreateWindow(vm);

        window.Show();
        grid.ApplyTemplate();
        grid.UpdateLayout();

        var cell = GetCell(grid, "Avatar", 0);
        var image = Assert.IsType<Image>(cell.Content);

        Assert.NotNull(image);
    }

    [AvaloniaFact]
    public void ImageColumn_Respects_Dimensions()
    {
        var column = new DataGridImageColumn
        {
            Header = "Avatar",
            ImageWidth = 32,
            ImageHeight = 32
        };

        Assert.Equal(32, column.ImageWidth);
        Assert.Equal(32, column.ImageHeight);
    }

    [AvaloniaFact]
    public void ImageColumn_Respects_Stretch()
    {
        var column = new DataGridImageColumn
        {
            Header = "Avatar",
            Stretch = Stretch.UniformToFill
        };

        Assert.Equal(Stretch.UniformToFill, column.Stretch);
    }

    [AvaloniaFact]
    public void ImageColumn_IsReadOnlyByDefault()
    {
        var column = new DataGridImageColumn
        {
            Header = "Avatar"
        };

        Assert.True(column.IsReadOnly);
    }

    [AvaloniaFact]
    public void ImageColumn_AllowEditing_MakesEditable()
    {
        var column = new DataGridImageColumn
        {
            Header = "Avatar",
            AllowEditing = true
        };

        Assert.False(column.IsReadOnly);
    }

    [AvaloniaFact]
    public void ImageColumn_Default_Stretch()
    {
        var column = new DataGridImageColumn();

        Assert.Equal(Stretch.Uniform, column.Stretch);
    }

    private static (Window window, DataGrid grid) CreateWindow(ImageTestViewModel vm)
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
                new DataGridImageColumn
                {
                    Header = "Avatar",
                    Binding = new Binding("ImagePath"),
                    ImageWidth = 32,
                    ImageHeight = 32
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

    private sealed class ImageTestViewModel
    {
        public ImageTestViewModel()
        {
            Items = new ObservableCollection<ImageItem>
            {
                new() { Name = "User 1", ImagePath = "avares://DataGridSample/Assets/user1.png" },
                new() { Name = "User 2", ImagePath = "avares://DataGridSample/Assets/user2.png" },
                new() { Name = "User 3", ImagePath = "avares://DataGridSample/Assets/user3.png" }
            };
        }

        public ObservableCollection<ImageItem> Items { get; }
    }

    private sealed class ImageItem
    {
        public string Name { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
    }
}
