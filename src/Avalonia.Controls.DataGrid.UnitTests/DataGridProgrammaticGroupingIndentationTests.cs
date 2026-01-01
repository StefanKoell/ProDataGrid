// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.DataGridTests;

public class DataGridProgrammaticGroupingIndentationTests
{
    [AvaloniaFact]
    public void ProgrammaticGrouping_Recalculates_Indentation_Across_Grouping_Changes()
    {
        var items = new List<Item>
        {
            new("Prices", "Identification", "Base price", "es-ES"),
            new("Prices", "Encoding", "Currency", "en-US"),
            new("Metadata", "Identification", "Name", "en-US"),
            new("Metadata", "Encoding", "Language", "es-ES"),
        };

        var view = new DataGridCollectionView(items);

        var root = new Window
        {
            Width = 600,
            Height = 400,
        };

        root.SetThemeStyles();

        var grid = new DataGrid
        {
            ItemsSource = view,
            HeadersVisibility = DataGridHeadersVisibility.Column,
        };

        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Category",
            Binding = new Binding(nameof(Item.Category)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Group",
            Binding = new Binding(nameof(Item.Group)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Locale",
            Binding = new Binding(nameof(Item.Locale)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(Item.Name)),
        });

        root.Content = grid;
        root.Show();
        PumpLayout(grid);

        try
        {
            SetGroupDescriptions(view, groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Group)));
            });
            PumpLayout(grid);

            var level1Header = GetHeaderForLevel(grid, 1);

            Assert.NotNull(grid.RowGroupSublevelIndents);
            var expectedIndent = grid.RowGroupSublevelIndents![0];
            Assert.Equal(expectedIndent, GetIndentSpacerWidth(level1Header), precision: 3);
            Assert.True(grid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented);
            Assert.Equal(grid.RowGroupSublevelIndents[^1], grid.ColumnsInternal.RowGroupSpacerColumn.Width.Value, precision: 3);

            SetGroupDescriptions(view, groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
            });
            PumpLayout(grid);

            var topLevelHeader = GetHeaderForLevel(grid, 0);
            Assert.Equal(0d, GetIndentSpacerWidth(topLevelHeader), precision: 3);

            SetGroupDescriptions(view, groups =>
            {
                var locale = new DataGridPathGroupDescription(nameof(Item.Locale));
                locale.GroupKeys.Add("es-ES");
                locale.GroupKeys.Add("en-US");
                locale.GroupKeys.Add("(none)");

                groups.Add(locale);
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
            });
            PumpLayout(grid);

            var localeHeader = GetHeaderForLevel(grid, 1);

            Assert.NotNull(grid.RowGroupSublevelIndents);
            expectedIndent = grid.RowGroupSublevelIndents![0];
            Assert.Equal(expectedIndent, GetIndentSpacerWidth(localeHeader), precision: 3);

            SetGroupDescriptions(view, groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Group)));
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
            });
            PumpLayout(grid);

            var reorderedHeader = GetHeaderForLevel(grid, 1);

            Assert.NotNull(grid.RowGroupSublevelIndents);
            expectedIndent = grid.RowGroupSublevelIndents![0];
            Assert.Equal(expectedIndent, GetIndentSpacerWidth(reorderedHeader), precision: 3);

            view.GroupDescriptions.Clear();
            view.Refresh();
            PumpLayout(grid);

            Assert.True(grid.RowGroupHeadersTable.IsEmpty);
            Assert.False(grid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void ProgrammaticGrouping_Reapplies_Indentation_After_Toggling_Configurations()
    {
        var items = new ObservableCollection<Item>(new[]
        {
            new Item("Prices", "Identification", "Base price", "es-ES"),
            new Item("Prices", "Identification", "Wholesale", "en-US"),
            new Item("Prices", "Encoding", "Currency", "es-ES"),
            new Item("Prices", "Encoding", "Currency", "en-US"),
            new Item("Metadata", "Identification", "Name", "es-ES"),
            new Item("Metadata", "Identification", "Name", "en-US"),
            new Item("Metadata", "Encoding", "Language", "es-ES"),
            new Item("Metadata", "Encoding", "Language", "en-US"),
        });

        var view = new DataGridCollectionView(items);
        var (root, grid) = CreateGrid(view, width: 520, height: 220);

        try
        {
            for (var i = 0; i < 3; i++)
            {
                SetGroupDescriptions(view, groups =>
                {
                    groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
                    groups.Add(new DataGridPathGroupDescription(nameof(Item.Group)));
                });
                grid.ExpandAllGroups();
                PumpLayout(grid);
                AssertDisplayedGroupHeaderIndentation(grid);

                SetGroupDescriptions(view, groups =>
                {
                    groups.Add(new DataGridPathGroupDescription(nameof(Item.Group)));
                    groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
                });
                grid.ExpandAllGroups();
                PumpLayout(grid);
                AssertDisplayedGroupHeaderIndentation(grid);
            }
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void ProgrammaticGrouping_Indentation_Is_Correct_For_All_Visible_Headers_Across_Mutations()
    {
        var items = new ObservableCollection<Item>(new[]
        {
            new Item("Prices", "Identification", "Base price", "es-ES"),
            new Item("Prices", "Identification", "Wholesale", "en-US"),
            new Item("Prices", "Encoding", "Currency", "es-ES"),
            new Item("Prices", "Encoding", "Currency", "en-US"),
            new Item("Metadata", "Identification", "Name", "es-ES"),
            new Item("Metadata", "Identification", "Name", "en-US"),
            new Item("Metadata", "Encoding", "Language", "es-ES"),
            new Item("Metadata", "Encoding", "Language", "en-US"),
        });

        var view = new DataGridCollectionView(items);
        var (root, grid) = CreateGrid(view, width: 600, height: 320);

        try
        {
            SetGroupDescriptions(view, groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Group)));
            });
            grid.ExpandAllGroups();
            PumpLayout(grid);
            AssertGroupHeaderIndentation(grid);

            SetGroupDescriptions(view, groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
                groups.Insert(1, new DataGridPathGroupDescription(nameof(Item.Group)));
            });
            grid.ExpandAllGroups();
            PumpLayout(grid);
            AssertGroupHeaderIndentation(grid);

            SetGroupDescriptions(view, groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Group)));
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
            });
            grid.ExpandAllGroups();
            PumpLayout(grid);
            AssertGroupHeaderIndentation(grid);

            SetGroupDescriptions(view, groups =>
            {
                var locale = new DataGridPathGroupDescription(nameof(Item.Locale));
                locale.GroupKeys.Add("es-ES");
                locale.GroupKeys.Add("en-US");
                locale.GroupKeys.Add("(none)");

                groups.Add(locale);
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
            });
            grid.ExpandAllGroups();
            PumpLayout(grid);
            AssertGroupHeaderIndentation(grid);

            SetGroupDescriptions(view, groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
            });
            grid.ExpandAllGroups();
            PumpLayout(grid);
            AssertGroupHeaderIndentation(grid);

            SetGroupDescriptions(view, groups =>
            {
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
                groups.Add(new DataGridPathGroupDescription(nameof(Item.Group)));
            });
            items.Insert(0, new Item("Prices", "Identification", "Generated", "en-US"));
            grid.ExpandAllGroups();
            PumpLayout(grid);
            AssertGroupHeaderIndentation(grid);

            view.GroupDescriptions.Clear();
            view.Refresh();
            PumpLayout(grid);
            Assert.Empty(GetGroupHeaders(grid).Where(header => header.IsVisible));
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void ProgrammaticGrouping_Reapplies_Indentation_After_Recycling_GroupHeaders()
    {
        var items = new ObservableCollection<Item>();
        var categories = new[] { "A", "B", "C", "D" };
        var groups = new[] { "One", "Two", "Three" };
        var locales = new[] { "es-ES", "en-US" };

        for (var i = 0; i < 60; i++)
        {
            items.Add(new Item(
                categories[i % categories.Length],
                groups[i % groups.Length],
                $"Name {i}",
                locales[i % locales.Length]));
        }

        var view = new DataGridCollectionView(items);
        var (root, grid) = CreateGrid(view, width: 500, height: 200);

        try
        {
            SetGroupDescriptions(view, groupDescriptions =>
            {
                groupDescriptions.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
                groupDescriptions.Add(new DataGridPathGroupDescription(nameof(Item.Group)));
            });
            grid.ExpandAllGroups();
            PumpLayout(grid);
            AssertGroupHeaderIndentation(grid);

            var lastHeaderSlot = grid.RowGroupHeadersTable.GetIndexes().Last();
            ScrollSlotIntoView(grid, lastHeaderSlot);

            SetGroupDescriptions(view, groupDescriptions =>
            {
                groupDescriptions.Add(new DataGridPathGroupDescription(nameof(Item.Group)));
                groupDescriptions.Add(new DataGridPathGroupDescription(nameof(Item.Category)));
            });
            grid.ExpandAllGroups();
            PumpLayout(grid);

            ScrollSlotIntoView(grid, 0);
            AssertGroupHeaderIndentation(grid);
        }
        finally
        {
            root.Close();
        }
    }

    private static void SetGroupDescriptions(
        DataGridCollectionView view,
        System.Action<AvaloniaList<DataGridGroupDescription>> configure)
    {
        var groups = view.GroupDescriptions;
        groups.Clear();
        configure(groups);
        view.Refresh();
    }

    private static (Window root, DataGrid grid) CreateGrid(DataGridCollectionView view, double width, double height)
    {
        var root = new Window
        {
            Width = width,
            Height = height,
        };

        root.SetThemeStyles();

        var grid = new DataGrid
        {
            ItemsSource = view,
            HeadersVisibility = DataGridHeadersVisibility.Column,
        };

        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Category",
            Binding = new Binding(nameof(Item.Category)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Group",
            Binding = new Binding(nameof(Item.Group)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Locale",
            Binding = new Binding(nameof(Item.Locale)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(Item.Name)),
        });

        root.Content = grid;
        root.Show();
        PumpLayout(grid);

        return (root, grid);
    }

    private static IReadOnlyList<DataGridRowGroupHeader> GetGroupHeaders(DataGrid grid)
    {
        return grid.GetVisualDescendants()
            .OfType<DataGridRowGroupHeader>()
            .Where(header => !header.IsRecycled)
            .ToList();
    }

    private static IReadOnlyList<DataGridRowGroupInfo> GetRowGroupInfos(DataGrid grid)
    {
        return grid.RowGroupHeadersTable.GetIndexes()
            .Select(slot => grid.RowGroupHeadersTable.GetValueAt(slot))
            .Where(info => info != null)
            .ToList();
    }

    private static DataGridRowGroupHeader GetHeaderForLevel(DataGrid grid, int level)
    {
        var info = GetRowGroupInfos(grid).First(group => group.Level == level);
        return GetHeaderForGroupInfo(grid, info);
    }

    private static DataGridRowGroupHeader GetHeaderForGroupInfo(DataGrid grid, DataGridRowGroupInfo info)
    {
        var columnIndex = grid.ColumnsInternal.FirstVisibleNonFillerColumn?.Index ?? 0;
        if (grid.ColumnDefinitions.Count > 0)
        {
            grid.ScrollSlotIntoView(columnIndex, info.Slot, forCurrentCellChange: false, forceHorizontalScroll: false);
        }

        PumpLayout(grid);

        if (grid.DisplayData.GetDisplayedElement(info.Slot) is DataGridRowGroupHeader header)
        {
            return header;
        }

        throw new InvalidOperationException("Group header was not realized.");
    }

    private static void AssertGroupHeaderIndentation(DataGrid grid)
    {
        var rowGroupInfos = GetRowGroupInfos(grid);
        Assert.NotEmpty(rowGroupInfos);

        var hasSubGroups = rowGroupInfos.Any(info => info.Level > 0);
        var indents = grid.RowGroupSublevelIndents ?? System.Array.Empty<double>();

        if (hasSubGroups)
        {
            Assert.NotNull(grid.RowGroupSublevelIndents);
        }

        foreach (var info in rowGroupInfos)
        {
            var header = GetHeaderForGroupInfo(grid, info);
            var level = info.Level;
            var expected = level <= 0 || indents.Length == 0
                ? 0
                : indents[System.Math.Min(level - 1, indents.Length - 1)];

            Assert.Equal(expected, GetIndentSpacerWidth(header), precision: 3);
        }
    }

    private static void AssertDisplayedGroupHeaderIndentation(DataGrid grid)
    {
        var rowGroupInfos = GetRowGroupInfos(grid);
        if (rowGroupInfos.Count == 0)
        {
            for (var attempt = 0; attempt < 3 && rowGroupInfos.Count == 0; attempt++)
            {
                PumpLayout(grid);
                rowGroupInfos = GetRowGroupInfos(grid);
            }
        }
        Assert.NotEmpty(rowGroupInfos);

        var indents = grid.RowGroupSublevelIndents ?? System.Array.Empty<double>();
        var displayedInfos = GetDisplayedGroupInfos(grid, rowGroupInfos);
        if (displayedInfos.Count == 0)
        {
            ScrollSlotIntoView(grid, rowGroupInfos[0].Slot);
            displayedInfos = GetDisplayedGroupInfos(grid, rowGroupInfos);
        }

        Assert.NotEmpty(displayedInfos);

        foreach (var info in displayedInfos)
        {
            if (grid.DisplayData.GetDisplayedElement(info.Slot) is not DataGridRowGroupHeader header)
            {
                throw new InvalidOperationException("Group header was not realized.");
            }

            var level = info.Level;
            var expected = level <= 0 || indents.Length == 0
                ? 0
                : indents[Math.Min(level - 1, indents.Length - 1)];

            Assert.Equal(expected, GetIndentSpacerWidth(header), precision: 3);
        }
    }

    private static List<DataGridRowGroupInfo> GetDisplayedGroupInfos(
        DataGrid grid,
        IReadOnlyList<DataGridRowGroupInfo> rowGroupInfos)
    {
        var firstSlot = grid.DisplayData.FirstScrollingSlot;
        var lastSlot = grid.DisplayData.LastScrollingSlot;
        if (firstSlot < 0 || lastSlot < firstSlot)
        {
            return new List<DataGridRowGroupInfo>();
        }

        return rowGroupInfos
            .Where(info => info.Slot >= firstSlot && info.Slot <= lastSlot && grid.IsSlotVisible(info.Slot))
            .ToList();
    }

    private static void ScrollSlotIntoView(DataGrid grid, int slot)
    {
        if (grid.ColumnDefinitions.Count > 0)
        {
            var columnIndex = grid.ColumnsInternal.FirstVisibleNonFillerColumn?.Index ?? 0;
            grid.ScrollSlotIntoView(columnIndex, slot, forCurrentCellChange: false, forceHorizontalScroll: false);
        }

        PumpLayout(grid);
    }

    private static double GetIndentSpacerWidth(DataGridRowGroupHeader header)
    {
        var spacer = header.GetVisualDescendants()
            .OfType<Rectangle>()
            .FirstOrDefault(rect => rect.Name == "PART_IndentSpacer");

        Assert.NotNull(spacer);
        return spacer!.Width;
    }

    private static void PumpLayout(Control control)
    {
        Dispatcher.UIThread.RunJobs();
        if (control.GetVisualRoot() is Window window)
        {
            window.ApplyTemplate();
            window.UpdateLayout();
        }
        control.ApplyTemplate();
        control.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
        control.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    private sealed record Item(string Category, string Group, string Name, string Locale);
}
