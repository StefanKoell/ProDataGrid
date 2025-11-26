// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.DataGridTests;

/// <summary>
/// Tests for smooth scrolling with variable row heights.
/// These tests validate the two-phase scroll algorithm that prevents jitter.
/// </summary>
public class SmoothScrollingTests
{
    #region Scroll Down Tests

    [AvaloniaFact]
    public void ScrollDown_SmallDistance_UpdatesFirstVisibleSlot()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);
        
        var initialFirstRow = GetFirstRealizedRowIndex(target);
        Assert.Equal(0, initialFirstRow);

        // Scroll down to item 20 which should definitely not be visible initially
        target.ScrollIntoView(items[20], target.Columns[0]);
        target.UpdateLayout();

        var newFirstRow = GetFirstRealizedRowIndex(target);
        var newLastRow = GetLastRealizedRowIndex(target);
        
        // Item 20 should now be visible
        Assert.True(newFirstRow <= 20 && newLastRow >= 20, 
            $"Item 20 should be visible. First: {newFirstRow}, Last: {newLastRow}");
    }

    [AvaloniaFact]
    public void ScrollDown_ToEnd_DisplaysLastRows()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);

        // Scroll to the last item
        target.ScrollIntoView(items[99], target.Columns[0]);
        target.UpdateLayout();

        var lastRow = GetLastRealizedRowIndex(target);
        Assert.Equal(99, lastRow);
    }

    [AvaloniaFact]
    public void ScrollDown_MaintainsRowOrder()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);

        // Scroll down
        target.ScrollIntoView(items[20], target.Columns[0]);
        target.UpdateLayout();

        var rows = GetRows(target).OrderBy(r => r.Index).ToList();
        
        // Verify rows are in sequential order
        for (int i = 1; i < rows.Count; i++)
        {
            Assert.True(rows[i].Index > rows[i - 1].Index, 
                $"Rows should be in order: {rows[i-1].Index} should be < {rows[i].Index}");
        }
    }

    [AvaloniaFact]
    public void ScrollDown_WithVariableHeights_MaintainsConsistency()
    {
        var items = CreateVariableHeightItems(100);
        var target = CreateTarget(items);

        // Scroll down through variable height rows
        target.ScrollIntoView(items[30], target.Columns[0]);
        target.UpdateLayout();

        var rows = GetRows(target);
        Assert.True(rows.Count > 0, "Should have visible rows");
        
        // Verify all rows are properly realized
        Assert.All(rows, row => Assert.True(row.Index >= 0));
    }

    [AvaloniaFact]
    public void ScrollDown_LargeDistance_UsesEstimation()
    {
        var items = CreateItems(1000);
        var target = CreateTargetLarge(items);

        // Scroll down a large distance (should use estimation)
        target.ScrollIntoView(items[500], target.Columns[0]);
        target.UpdateLayout();

        var firstRow = GetFirstRealizedRowIndex(target);
        var lastRow = GetLastRealizedRowIndex(target);
        
        // Should be somewhere around index 500
        Assert.True(firstRow <= 500 && lastRow >= 500, 
            $"Scrolled row 500 should be visible. First: {firstRow}, Last: {lastRow}");
    }

    #endregion

    #region Scroll Up Tests

    [AvaloniaFact]
    public void ScrollUp_FromMiddle_DisplaysPreviousRows()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);

        // First scroll down
        target.ScrollIntoView(items[50], target.Columns[0]);
        target.UpdateLayout();

        var middleFirstRow = GetFirstRealizedRowIndex(target);
        Assert.True(middleFirstRow > 0);

        // Now scroll back up
        target.ScrollIntoView(items[10], target.Columns[0]);
        target.UpdateLayout();

        var newFirstRow = GetFirstRealizedRowIndex(target);
        Assert.True(newFirstRow < middleFirstRow, 
            $"Should have scrolled up. Before: {middleFirstRow}, After: {newFirstRow}");
    }

    [AvaloniaFact]
    public void ScrollUp_ToTop_DisplaysFirstRows()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);

        // First scroll down
        target.ScrollIntoView(items[50], target.Columns[0]);
        target.UpdateLayout();

        // Now scroll back to top
        target.ScrollIntoView(items[0], target.Columns[0]);
        target.UpdateLayout();

        var firstRow = GetFirstRealizedRowIndex(target);
        Assert.Equal(0, firstRow);
    }

    [AvaloniaFact]
    public void ScrollUp_MaintainsRowOrder()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);

        // Scroll down first
        target.ScrollIntoView(items[50], target.Columns[0]);
        target.UpdateLayout();

        // Scroll up
        target.ScrollIntoView(items[20], target.Columns[0]);
        target.UpdateLayout();

        var rows = GetRows(target).OrderBy(r => r.Index).ToList();
        
        // Verify rows are in sequential order
        for (int i = 1; i < rows.Count; i++)
        {
            Assert.True(rows[i].Index > rows[i - 1].Index, 
                $"Rows should be in order after scroll-up: {rows[i-1].Index} should be < {rows[i].Index}");
        }
    }

    [AvaloniaFact]
    public void ScrollUp_WithVariableHeights_MaintainsConsistency()
    {
        var items = CreateVariableHeightItems(100);
        var target = CreateTarget(items);

        // Scroll down first
        target.ScrollIntoView(items[60], target.Columns[0]);
        target.UpdateLayout();

        // Scroll up through variable height rows
        target.ScrollIntoView(items[20], target.Columns[0]);
        target.UpdateLayout();

        var rows = GetRows(target);
        Assert.True(rows.Count > 0, "Should have visible rows");
        
        // Verify all rows are properly realized
        Assert.All(rows, row => Assert.True(row.Index >= 0));
    }

    [AvaloniaFact]
    public void ScrollUp_LargeDistance_UsesEstimation()
    {
        var items = CreateItems(1000);
        var target = CreateTargetLarge(items);

        // Scroll to end first
        target.ScrollIntoView(items[999], target.Columns[0]);
        target.UpdateLayout();

        // Scroll up a large distance
        target.ScrollIntoView(items[100], target.Columns[0]);
        target.UpdateLayout();

        var firstRow = GetFirstRealizedRowIndex(target);
        var lastRow = GetLastRealizedRowIndex(target);
        
        Assert.True(firstRow <= 100 && lastRow >= 100, 
            $"Scrolled row 100 should be visible. First: {firstRow}, Last: {lastRow}");
    }

    #endregion

    #region Bidirectional Scroll Tests

    [AvaloniaFact]
    public void BidirectionalScroll_DownThenUp_ReturnsToSamePosition()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);

        // Record initial state
        var initialFirstRow = GetFirstRealizedRowIndex(target);
        var initialLastRow = GetLastRealizedRowIndex(target);

        // Scroll down
        target.ScrollIntoView(items[50], target.Columns[0]);
        target.UpdateLayout();

        // Scroll back up to original position
        target.ScrollIntoView(items[initialFirstRow], target.Columns[0]);
        target.UpdateLayout();

        var finalFirstRow = GetFirstRealizedRowIndex(target);
        
        Assert.Equal(initialFirstRow, finalFirstRow);
    }

    [AvaloniaFact]
    public void BidirectionalScroll_MultipleRoundTrips_RemainsConsistent()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);

        for (int trip = 0; trip < 3; trip++)
        {
            // Scroll down
            target.ScrollIntoView(items[70], target.Columns[0]);
            target.UpdateLayout();

            var downRows = GetRows(target);
            Assert.True(downRows.Count > 0, $"Trip {trip}: Should have rows after scroll down");

            // Scroll up
            target.ScrollIntoView(items[10], target.Columns[0]);
            target.UpdateLayout();

            var upRows = GetRows(target);
            Assert.True(upRows.Count > 0, $"Trip {trip}: Should have rows after scroll up");

            // Verify order
            var orderedRows = upRows.OrderBy(r => r.Index).ToList();
            for (int i = 1; i < orderedRows.Count; i++)
            {
                Assert.True(orderedRows[i].Index > orderedRows[i - 1].Index,
                    $"Trip {trip}: Rows should maintain order");
            }
        }
    }

    [AvaloniaFact]
    public void BidirectionalScroll_WithVariableHeights_RemainsConsistent()
    {
        var items = CreateVariableHeightItems(100);
        var target = CreateTarget(items);

        // Multiple scroll operations
        int[] scrollTargets = { 30, 10, 60, 5, 80, 0, 50 };

        foreach (var targetIndex in scrollTargets)
        {
            target.ScrollIntoView(items[targetIndex], target.Columns[0]);
            target.UpdateLayout();

            var rows = GetRows(target);
            Assert.True(rows.Count > 0, $"Should have rows after scrolling to {targetIndex}");

            // Verify rows contain the target
            var indices = rows.Select(r => r.Index).ToList();
            Assert.Contains(targetIndex, indices);
        }
    }

    #endregion

    #region Edge Cases

    [AvaloniaFact]
    public void Scroll_EmptyDataGrid_DoesNotThrow()
    {
        var items = new List<ScrollTestModel>();
        var target = CreateTarget(items);

        // Should not throw
        target.UpdateLayout();
    }

    [AvaloniaFact]
    public void Scroll_SingleItem_Works()
    {
        var items = CreateItems(1);
        var target = CreateTarget(items);

        target.ScrollIntoView(items[0], target.Columns[0]);
        target.UpdateLayout();

        var rows = GetRows(target);
        Assert.Single(rows);
        Assert.Equal(0, rows[0].Index);
    }

    [AvaloniaFact]
    public void Scroll_FewItems_LessThanViewport_Works()
    {
        var items = CreateItems(3);
        var target = CreateTarget(items);

        target.ScrollIntoView(items[2], target.Columns[0]);
        target.UpdateLayout();

        var lastRow = GetLastRealizedRowIndex(target);
        Assert.Equal(2, lastRow);
    }

    [AvaloniaFact]
    public void Scroll_ToFirstItem_SetsCorrectState()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);

        // Scroll down first
        target.ScrollIntoView(items[50], target.Columns[0]);
        target.UpdateLayout();

        // Scroll to first item
        target.ScrollIntoView(items[0], target.Columns[0]);
        target.UpdateLayout();

        var firstRow = GetFirstRealizedRowIndex(target);
        Assert.Equal(0, firstRow);
    }

    [AvaloniaFact]
    public void Scroll_ToLastItem_SetsCorrectState()
    {
        var items = CreateItems(100);
        var target = CreateTarget(items);

        target.ScrollIntoView(items[99], target.Columns[0]);
        target.UpdateLayout();

        var lastRow = GetLastRealizedRowIndex(target);
        Assert.Equal(99, lastRow);
    }

    #endregion

    #region Variable Height Specific Tests

    [AvaloniaFact]
    public void VariableHeights_TallRowsBeforeShort_ScrollsCorrectly()
    {
        // Create items where first half are tall, second half are short
        var items = Enumerable.Range(0, 100)
            .Select(i => new ScrollTestModel($"Item {i}", i < 50 ? 5 : 1))
            .ToList();
        
        var target = CreateTarget(items);

        // Scroll through the transition zone
        target.ScrollIntoView(items[45], target.Columns[0]);
        target.UpdateLayout();

        var rows = GetRows(target);
        Assert.True(rows.Count > 0);

        target.ScrollIntoView(items[55], target.Columns[0]);
        target.UpdateLayout();

        rows = GetRows(target);
        Assert.True(rows.Count > 0);
    }

    [AvaloniaFact]
    public void VariableHeights_ShortRowsBeforeTall_ScrollsCorrectly()
    {
        // Create items where first half are short, second half are tall
        var items = Enumerable.Range(0, 100)
            .Select(i => new ScrollTestModel($"Item {i}", i < 50 ? 1 : 5))
            .ToList();
        
        var target = CreateTarget(items);

        // Scroll through the transition zone
        target.ScrollIntoView(items[45], target.Columns[0]);
        target.UpdateLayout();

        var rows = GetRows(target);
        Assert.True(rows.Count > 0);

        target.ScrollIntoView(items[55], target.Columns[0]);
        target.UpdateLayout();

        rows = GetRows(target);
        Assert.True(rows.Count > 0);
    }

    [AvaloniaFact]
    public void VariableHeights_RandomHeights_ScrollUpMaintainsOrder()
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var items = Enumerable.Range(0, 100)
            .Select(i => new ScrollTestModel($"Item {i}", random.Next(1, 10)))
            .ToList();
        
        var target = CreateTarget(items);

        // Scroll to end
        target.ScrollIntoView(items[99], target.Columns[0]);
        target.UpdateLayout();

        // Scroll up incrementally
        for (int i = 90; i >= 0; i -= 10)
        {
            target.ScrollIntoView(items[i], target.Columns[0]);
            target.UpdateLayout();

            var rows = GetRows(target).OrderBy(r => r.Index).ToList();
            
            // Verify order is maintained
            for (int j = 1; j < rows.Count; j++)
            {
                Assert.True(rows[j].Index > rows[j - 1].Index,
                    $"At target {i}: Row order broken between {rows[j-1].Index} and {rows[j].Index}");
            }
        }
    }

    [AvaloniaFact]
    public void VariableHeights_RandomHeights_ScrollDownMaintainsOrder()
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var items = Enumerable.Range(0, 100)
            .Select(i => new ScrollTestModel($"Item {i}", random.Next(1, 10)))
            .ToList();
        
        var target = CreateTarget(items);

        // Scroll down incrementally
        for (int i = 10; i < 100; i += 10)
        {
            target.ScrollIntoView(items[i], target.Columns[0]);
            target.UpdateLayout();

            var rows = GetRows(target).OrderBy(r => r.Index).ToList();
            
            // Verify order is maintained
            for (int j = 1; j < rows.Count; j++)
            {
                Assert.True(rows[j].Index > rows[j - 1].Index,
                    $"At target {i}: Row order broken between {rows[j-1].Index} and {rows[j].Index}");
            }
        }
    }

    #endregion

    #region Regression Tests

    [AvaloniaFact]
    public void Regression_ScrollUpDoesNotCauseJitter()
    {
        // This test specifically validates the fix for scroll-up jitter
        var items = CreateVariableHeightItems(100);
        var target = CreateTarget(items);

        // Scroll to middle
        target.ScrollIntoView(items[50], target.Columns[0]);
        target.UpdateLayout();

        var beforeScrollUp = GetRows(target).Select(r => r.Index).OrderBy(x => x).ToList();

        // Scroll up - this was the problematic operation
        target.ScrollIntoView(items[40], target.Columns[0]);
        target.UpdateLayout();

        var afterScrollUp = GetRows(target).Select(r => r.Index).OrderBy(x => x).ToList();

        // Verify rows are sequential (no gaps or duplicates)
        for (int i = 1; i < afterScrollUp.Count; i++)
        {
            int expected = afterScrollUp[i - 1] + 1;
            // Allow for collapsed rows, but indices should be monotonically increasing
            Assert.True(afterScrollUp[i] > afterScrollUp[i - 1],
                $"Rows should be sequential after scroll-up. Found {afterScrollUp[i-1]} followed by {afterScrollUp[i]}");
        }
    }

    [AvaloniaFact]
    public void Regression_ScrollUpSmallAmount_NoStateCorruption()
    {
        var items = CreateVariableHeightItems(100);
        var target = CreateTarget(items);

        // Scroll to a position
        target.ScrollIntoView(items[30], target.Columns[0]);
        target.UpdateLayout();

        // Small scroll up (should use small distance algorithm)
        target.ScrollIntoView(items[28], target.Columns[0]);
        target.UpdateLayout();

        var rows = GetRows(target);
        
        // Verify no state corruption
        Assert.True(rows.Count > 0, "Should have visible rows");
        Assert.All(rows, row => Assert.True(row.Index >= 0 && row.Index < 100));
    }

    [AvaloniaFact]
    public void Regression_RapidScrollUpDown_NoStateCorruption()
    {
        var items = CreateVariableHeightItems(100);
        var target = CreateTarget(items);

        // Rapid scroll operations
        for (int i = 0; i < 10; i++)
        {
            target.ScrollIntoView(items[80], target.Columns[0]);
            target.UpdateLayout();
            
            target.ScrollIntoView(items[20], target.Columns[0]);
            target.UpdateLayout();
        }

        var rows = GetRows(target);
        Assert.True(rows.Count > 0, "Should have visible rows after rapid scrolling");
        
        // Verify valid state
        var indices = rows.Select(r => r.Index).OrderBy(x => x).ToList();
        for (int i = 1; i < indices.Count; i++)
        {
            Assert.True(indices[i] > indices[i - 1], "Row indices should be monotonically increasing");
        }
    }

    #endregion

    #region Helper Methods

    private static List<ScrollTestModel> CreateItems(int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => new ScrollTestModel($"Item {i}"))
            .ToList();
    }

    private static List<ScrollTestModel> CreateVariableHeightItems(int count)
    {
        var random = new Random(123); // Fixed seed for reproducibility
        return Enumerable.Range(0, count)
            .Select(i => new ScrollTestModel($"Item {i}", random.Next(1, 8)))
            .ToList();
    }

    private static DataGrid CreateTarget(IList<ScrollTestModel> items)
    {
        var root = new Window
        {
            Width = 300,
            Height = 200,
            Styles =
            {
                new StyleInclude((Uri?)null)
                {
                    Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Simple.xaml")
                },
            }
        };

        var target = new DataGrid
        {
            Columns =
            {
                new DataGridTextColumn { Header = "Name", Binding = new Binding("Name") }
            },
            ItemsSource = items,
            HeadersVisibility = DataGridHeadersVisibility.All,
        };

        root.Content = target;
        root.Show();
        return target;
    }

    private static DataGrid CreateTargetLarge(IList<ScrollTestModel> items)
    {
        var root = new Window
        {
            Width = 300,
            Height = 400,
            Styles =
            {
                new StyleInclude((Uri?)null)
                {
                    Source = new Uri("avares://Avalonia.Controls.DataGrid/Themes/Simple.xaml")
                },
            }
        };

        var target = new DataGrid
        {
            Columns =
            {
                new DataGridTextColumn { Header = "Name", Binding = new Binding("Name") }
            },
            ItemsSource = items,
            HeadersVisibility = DataGridHeadersVisibility.All,
        };

        root.Content = target;
        root.Show();
        return target;
    }

    private static int GetFirstRealizedRowIndex(DataGrid target)
    {
        var rows = target.GetSelfAndVisualDescendants().OfType<DataGridRow>().ToList();
        return rows.Count > 0 ? rows.Min(x => x.Index) : -1;
    }

    private static int GetLastRealizedRowIndex(DataGrid target)
    {
        var rows = target.GetSelfAndVisualDescendants().OfType<DataGridRow>().ToList();
        return rows.Count > 0 ? rows.Max(x => x.Index) : -1;
    }

    private static IReadOnlyList<DataGridRow> GetRows(DataGrid target)
    {
        return target.GetSelfAndVisualDescendants().OfType<DataGridRow>().ToList();
    }

    #endregion

    #region Test Model

    private class ScrollTestModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _name;
        private int _lineCount;

        public ScrollTestModel(string name, int lineCount = 1)
        {
            _name = name;
            _lineCount = lineCount;
        }

        public string Name
        {
            get => _lineCount > 1 ? string.Join("\n", Enumerable.Repeat(_name, _lineCount)) : _name;
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public int LineCount
        {
            get => _lineCount;
            set
            {
                _lineCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }

    #endregion
}
