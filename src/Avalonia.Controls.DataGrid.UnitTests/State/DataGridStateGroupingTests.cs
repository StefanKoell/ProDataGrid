// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.DataGridTests;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.DataGridTests.State;

public class DataGridStateGroupingTests
{
    [AvaloniaFact]
    public void CaptureAndRestoreGroupingState_RestoresDescriptionsAndExpansion()
    {
        var items = StateTestHelper.CreateItems(12);
        var view = new DataGridCollectionView(items);
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Category)));
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Group)));
        view.Refresh();

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
            Binding = new Binding(nameof(StateTestItem.Category)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Group",
            Binding = new Binding(nameof(StateTestItem.Group)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(StateTestItem.Name)),
        });

        root.Content = grid;
        root.Show();
        grid.UpdateLayout();

        try
        {
            var targetGroup = FindGroup(view, "A");
            Assert.NotNull(targetGroup);

            grid.CollapseRowGroup(targetGroup, collapseAllSubgroups: false);
            grid.UpdateLayout();

            var state = grid.CaptureGroupingState();
            Assert.NotNull(state);

            view.GroupDescriptions.Clear();
            view.Refresh();
            grid.UpdateLayout();

            grid.RestoreGroupingState(state);
            grid.UpdateLayout();

            Assert.Equal(2, view.GroupDescriptions.Count);

            var restoredGroup = FindGroup(view, "A");
            Assert.NotNull(restoredGroup);

            var info = grid.RowGroupInfoFromCollectionViewGroup(restoredGroup);
            Assert.NotNull(info);
            Assert.False(info.IsVisible);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void RestoreGroupingState_RefreshesIndentationForDisplayedHeaders()
    {
        var items = StateTestHelper.CreateItems(18);
        var view = new DataGridCollectionView(items);
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Category)));
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Group)));
        view.Refresh();

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
            Binding = new Binding(nameof(StateTestItem.Category)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Group",
            Binding = new Binding(nameof(StateTestItem.Group)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(StateTestItem.Name)),
        });

        root.Content = grid;
        root.Show();
        grid.UpdateLayout();
        Dispatcher.UIThread.RunJobs();

        try
        {
            grid.ExpandAllGroups();
            grid.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            var state = grid.CaptureGroupingState();
            Assert.NotNull(state);

            view.GroupDescriptions.Clear();
            view.Refresh();
            grid.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            grid.RestoreGroupingState(state);
            grid.ExpandAllGroups();
            grid.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Assert.NotNull(grid.RowGroupSublevelIndents);

            var index = grid.RowGroupSublevelIndents.Length - 1;
            var expectedIndent = grid.RowGroupSublevelIndents[index];

            Assert.True(grid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented);
            Assert.Equal(DataGridLengthUnitType.Pixel, grid.ColumnsInternal.RowGroupSpacerColumn.Width.UnitType);
            Assert.Equal(expectedIndent, grid.ColumnsInternal.RowGroupSpacerColumn.Width.Value);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void RestoreGroupingState_ReappliesIndentationToVisibleHeaders()
    {
        var items = StateTestHelper.CreateItems(18);
        var view = new DataGridCollectionView(items);
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Category)));
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Group)));
        view.Refresh();

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
            Binding = new Binding(nameof(StateTestItem.Category)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Group",
            Binding = new Binding(nameof(StateTestItem.Group)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(StateTestItem.Name)),
        });

        root.Content = grid;
        root.Show();
        PumpLayout(grid);

        try
        {
            grid.ExpandAllGroups();
            PumpLayout(grid);

            var state = grid.CaptureGroupingState();
            Assert.NotNull(state);

            view.GroupDescriptions.Clear();
            view.Refresh();
            PumpLayout(grid);

            grid.RestoreGroupingState(state);
            grid.ExpandAllGroups();
            PumpLayout(grid);

            AssertGroupHeaderIndentation(grid);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void RestoreGroupingState_ReappliesIndentation_AfterMultipleRestores()
    {
        var items = StateTestHelper.CreateItems(18);
        var view = new DataGridCollectionView(items);
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Category)));
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Group)));
        view.Refresh();

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
            Binding = new Binding(nameof(StateTestItem.Category)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Group",
            Binding = new Binding(nameof(StateTestItem.Group)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(StateTestItem.Name)),
        });

        root.Content = grid;
        root.Show();
        PumpLayout(grid);

        try
        {
            grid.ExpandAllGroups();
            PumpLayout(grid);

            var state = grid.CaptureGroupingState();
            Assert.NotNull(state);

            for (var i = 0; i < 3; i++)
            {
                view.GroupDescriptions.Clear();
                view.Refresh();
                PumpLayout(grid);

                grid.RestoreGroupingState(state);
                grid.ExpandAllGroups();
                PumpLayout(grid);

                AssertGroupHeaderIndentation(grid);
            }
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void RestoreGroupingState_ReappliesIndentation_WhenDescriptionsUnchanged()
    {
        var items = StateTestHelper.CreateItems(18);
        var view = new DataGridCollectionView(items);
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Category)));
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Group)));
        view.Refresh();

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
            AreRowGroupHeadersFrozen = true,
        };

        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Category",
            Binding = new Binding(nameof(StateTestItem.Category)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Group",
            Binding = new Binding(nameof(StateTestItem.Group)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(StateTestItem.Name)),
        });

        root.Content = grid;
        root.Show();
        PumpLayout(grid);

        try
        {
            grid.CollapseAllGroups();
            PumpLayout(grid);

            var groupA = FindGroup(view, "A");
            var groupB = FindGroup(view, "B");
            Assert.NotNull(groupA);
            Assert.NotNull(groupB);

            grid.ExpandRowGroup(groupA, expandAllSubgroups: false);
            grid.ExpandRowGroup(groupB, expandAllSubgroups: false);
            PumpLayout(grid);

            var state = grid.CaptureGroupingState();
            Assert.NotNull(state);

            for (var i = 0; i < 2; i++)
            {
                grid.RestoreGroupingState(state);
                PumpLayout(grid);
                AssertVisibleGroupHeaderIndentation(grid);
            }
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void RestoreGroupingState_ReappliesIndentation_After_ClearGrouping_With_Partial_Expansion()
    {
        var items = StateTestHelper.CreateItems(18);
        var view = new DataGridCollectionView(items);
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Category)));
        view.GroupDescriptions.Add(new DataGridPathGroupDescription(nameof(StateTestItem.Group)));
        view.Refresh();

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
            Binding = new Binding(nameof(StateTestItem.Category)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Group",
            Binding = new Binding(nameof(StateTestItem.Group)),
        });
        grid.ColumnsInternal.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding(nameof(StateTestItem.Name)),
        });

        root.Content = grid;
        root.Show();
        PumpLayout(grid);

        try
        {
            grid.CollapseAllGroups();
            PumpLayout(grid);

            var groupA = FindGroup(view, "A");
            var groupB = FindGroup(view, "B");
            Assert.NotNull(groupA);
            Assert.NotNull(groupB);

            grid.ExpandRowGroup(groupA, expandAllSubgroups: false);
            grid.ExpandRowGroup(groupB, expandAllSubgroups: false);
            PumpLayout(grid);

            var state = grid.CaptureGroupingState();
            Assert.NotNull(state);

            for (var i = 0; i < 2; i++)
            {
                view.GroupDescriptions.Clear();
                view.Refresh();
                PumpLayout(grid);

                grid.RestoreGroupingState(state);
                PumpLayout(grid);

                AssertDisplayedGroupHeaderIndentationByInfo(grid);
            }
        }
        finally
        {
            root.Close();
        }
    }

    private static DataGridCollectionViewGroup? FindGroup(DataGridCollectionView view, params object[] pathKeys)
    {
        IEnumerable<DataGridCollectionViewGroup> current = view.Groups?.Cast<DataGridCollectionViewGroup>();

        DataGridCollectionViewGroup? matched = null;
        foreach (var key in pathKeys)
        {
            matched = current?.FirstOrDefault(group => Equals(group.Key, key));
            if (matched == null)
            {
                return null;
            }

            current = matched.Items.OfType<DataGridCollectionViewGroup>();
        }

        return matched;
    }

    private static IReadOnlyList<DataGridRowGroupInfo> GetRowGroupInfos(DataGrid grid)
    {
        return grid.RowGroupHeadersTable.GetIndexes()
            .Select(slot => grid.RowGroupHeadersTable.GetValueAt(slot))
            .Where(info => info != null)
            .ToList();
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
        var indents = grid.RowGroupSublevelIndents ?? Array.Empty<double>();

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
                : indents[Math.Min(level - 1, indents.Length - 1)];

            Assert.Equal(expected, GetIndentSpacerWidth(header), precision: 3);
        }
    }

    private static double GetIndentSpacerWidth(DataGridRowGroupHeader header)
    {
        var spacer = header.GetVisualDescendants()
            .OfType<Rectangle>()
            .FirstOrDefault(rect => rect.Name == "PART_IndentSpacer");

        Assert.NotNull(spacer);
        return spacer!.Width;
    }

    private static void AssertVisibleGroupHeaderIndentation(DataGrid grid)
    {
        var indents = grid.RowGroupSublevelIndents ?? Array.Empty<double>();

        foreach (var slot in grid.RowGroupHeadersTable.GetIndexes())
        {
            if (!grid.IsSlotVisible(slot))
            {
                continue;
            }

            var columnIndex = grid.ColumnsInternal.FirstVisibleNonFillerColumn?.Index ?? 0;
            if (grid.ColumnDefinitions.Count > 0)
            {
                grid.ScrollSlotIntoView(columnIndex, slot, forCurrentCellChange: false, forceHorizontalScroll: false);
            }

            PumpLayout(grid);

            if (grid.DisplayData.GetDisplayedElement(slot) is not DataGridRowGroupHeader header)
            {
                throw new InvalidOperationException("Group header was not realized.");
            }

            var level = header.Level;
            var expected = level <= 0 || indents.Length == 0
                ? 0
                : indents[Math.Min(level - 1, indents.Length - 1)];

            Assert.Equal(expected, GetIndentSpacerWidth(header), precision: 3);
        }
    }

    private static void AssertDisplayedGroupHeaderIndentationByInfo(DataGrid grid)
    {
        var rowGroupInfos = GetRowGroupInfos(grid);
        Assert.NotEmpty(rowGroupInfos);

        var displayedInfos = GetDisplayedRowGroupInfos(grid, rowGroupInfos);
        if (displayedInfos.Count == 0)
        {
            _ = GetHeaderForGroupInfo(grid, rowGroupInfos[0]);
            displayedInfos = GetDisplayedRowGroupInfos(grid, rowGroupInfos);
        }

        Assert.NotEmpty(displayedInfos);

        var indents = grid.RowGroupSublevelIndents ?? Array.Empty<double>();

        foreach (var info in displayedInfos)
        {
            if (grid.DisplayData.GetDisplayedElement(info.Slot) is not DataGridRowGroupHeader header)
            {
                throw new InvalidOperationException("Group header was not realized.");
            }

            Assert.Same(info, header.RowGroupInfo);

            var expected = info.Level <= 0 || indents.Length == 0
                ? 0
                : indents[Math.Min(info.Level - 1, indents.Length - 1)];

            Assert.Equal(expected, GetIndentSpacerWidth(header), precision: 3);
        }
    }

    private static List<DataGridRowGroupInfo> GetDisplayedRowGroupInfos(
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
}
