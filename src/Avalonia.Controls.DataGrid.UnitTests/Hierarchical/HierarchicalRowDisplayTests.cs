// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Threading;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Hierarchical;

public class HierarchicalRowDisplayTests
{
    private sealed class Item
    {
        public Item(string name)
        {
            Name = name;
            Children = new ObservableCollection<Item>();
        }

        public string Name { get; }

        public ObservableCollection<Item> Children { get; }
    }

    [AvaloniaFact]
    public void LoadRowVisualsForDisplay_Clears_Clip_On_Visible_Row()
    {
        var root = new Item("root");
        root.Children.Add(new Item("child"));

        var model = new HierarchicalModel<Item>(new HierarchicalOptions<Item>
        {
            ChildrenSelector = item => item.Children,
            AutoExpandRoot = true,
            MaxAutoExpandDepth = 1
        });
        model.SetRoot(root);

        using var themeScope = UseApplicationTheme(DataGridTheme.SimpleV2);

        var grid = new DataGrid
        {
            HierarchicalModel = model,
            HierarchicalRowsEnabled = true,
            AutoGenerateColumns = false,
            ItemsSource = model.Flattened
        };

        grid.ColumnsInternal.Add(new DataGridHierarchicalColumn
        {
            Header = "Name",
            Binding = new Binding("Item.Name")
        });

        var window = new Window
        {
            Width = 320,
            Height = 200,
            Content = grid
        };

        window.SetThemeStyles(DataGridTheme.SimpleV2);
        window.Show();
        PumpLayout(grid);

        var row = GetVisibleRows(grid).FirstOrDefault();
        Assert.NotNull(row);

        row!.Clip = new RectangleGeometry();

        var loadMethod = typeof(DataGrid).GetMethod(
            "LoadRowVisualsForDisplay",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(loadMethod);

        loadMethod!.Invoke(grid, new object[] { row });

        Assert.Null(row.Clip);

        window.Close();
    }

    private static IReadOnlyList<DataGridRow> GetVisibleRows(DataGrid grid)
    {
        return grid.GetSelfAndVisualDescendants()
            .OfType<DataGridRow>()
            .Where(row => row.IsVisible)
            .ToList();
    }

    private static void PumpLayout(DataGrid grid)
    {
        Dispatcher.UIThread.RunJobs();
        if (grid.GetVisualRoot() is Window window)
        {
            window.ApplyTemplate();
            window.UpdateLayout();
        }
        grid.ApplyTemplate();
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private static IDisposable UseApplicationTheme(DataGridTheme theme)
    {
        var styles = ThemeHelper.GetThemeStyles(theme);
        var appStyles = Application.Current?.Styles;
        appStyles?.Add(styles);
        return new ThemeScope(appStyles, styles);
    }

    private sealed class ThemeScope : IDisposable
    {
        private readonly Styles? _appStyles;
        private readonly Styles _styles;

        public ThemeScope(Styles? appStyles, Styles styles)
        {
            _appStyles = appStyles;
            _styles = styles;
        }

        public void Dispose()
        {
            _appStyles?.Remove(_styles);
        }
    }
}
