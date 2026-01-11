// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Xunit;

namespace Avalonia.Controls.DataGridTests.State;

public class DataGridStateSortingTests
{
    [AvaloniaFact]
    public void CaptureAndRestoreSortingState_ResolvesColumns()
    {
        var items = StateTestHelper.CreateItems(10);
        var (grid, root) = StateTestHelper.CreateGrid(items);

        try
        {
            var nameColumn = grid.ColumnsInternal[1];

            grid.SortingModel.MultiSort = true;
            grid.SortingModel.CycleMode = SortCycleMode.AscendingDescendingNone;
            grid.SortingModel.OwnsViewSorts = true;
            grid.SortingModel.Apply(new[]
            {
                new SortingDescriptor(nameColumn, ListSortDirection.Descending, nameof(StateTestItem.Name)),
            });

            var options = StateTestHelper.CreateKeyedOptions(grid, items);
            var state = grid.CaptureSortingState(options);

            Assert.NotNull(state);
            Assert.Equal("Name", state.Descriptors[0].ColumnId);

            grid.SortingModel.Clear();
            grid.SortingModel.MultiSort = false;
            grid.SortingModel.CycleMode = SortCycleMode.AscendingDescending;
            grid.SortingModel.OwnsViewSorts = false;

            grid.RestoreSortingState(state, options);

            Assert.True(grid.SortingModel.MultiSort);
            Assert.Equal(SortCycleMode.AscendingDescendingNone, grid.SortingModel.CycleMode);
            Assert.True(grid.SortingModel.OwnsViewSorts);

            var restored = Assert.Single(grid.SortingModel.Descriptors);
            Assert.Same(nameColumn, restored.ColumnId);
            Assert.Equal(ListSortDirection.Descending, restored.Direction);
        }
        finally
        {
            root.Close();
        }
    }

    [AvaloniaFact]
    public void CaptureAndRestoreSortingState_Resolves_Definition_Columns()
    {
        var items = StateTestHelper.CreateItems(10);
        var (grid, root, definitions) = StateTestHelper.CreateGridWithDefinitions(items);

        try
        {
            var nameDefinition = definitions[1];
            var nameColumn = grid.ColumnsInternal[1];

            grid.SortingModel.MultiSort = true;
            grid.SortingModel.CycleMode = SortCycleMode.AscendingDescendingNone;
            grid.SortingModel.OwnsViewSorts = true;
            grid.SortingModel.Apply(new[]
            {
                new SortingDescriptor(nameDefinition, ListSortDirection.Descending, nameof(StateTestItem.Name)),
            });

            var options = StateTestHelper.CreateItemKeyOptions(items);
            var state = grid.CaptureSortingState(options);

            Assert.NotNull(state);
            Assert.Same(nameDefinition, state.Descriptors[0].ColumnId);

            grid.SortingModel.Clear();
            grid.SortingModel.MultiSort = false;
            grid.SortingModel.CycleMode = SortCycleMode.AscendingDescending;
            grid.SortingModel.OwnsViewSorts = false;

            grid.RestoreSortingState(state, options);
            Dispatcher.UIThread.RunJobs();

            Assert.True(grid.SortingModel.MultiSort);
            Assert.Equal(SortCycleMode.AscendingDescendingNone, grid.SortingModel.CycleMode);
            Assert.True(grid.SortingModel.OwnsViewSorts);

            var restored = Assert.Single(grid.SortingModel.Descriptors);
            Assert.Same(nameDefinition, restored.ColumnId);
            Assert.Equal(ListSortDirection.Descending, restored.Direction);
            Assert.Equal(ListSortDirection.Descending, nameColumn.SortDirection);
        }
        finally
        {
            root.Close();
        }
    }
}
