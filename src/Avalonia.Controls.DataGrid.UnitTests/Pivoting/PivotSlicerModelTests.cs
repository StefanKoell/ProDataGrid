using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotSlicerModelTests
{
    private sealed class Item
    {
        public string? Category { get; init; }
    }

    private sealed class ThrowingComparable : IComparable
    {
        private readonly string _value;

        public ThrowingComparable(string value)
        {
            _value = value;
        }

        public int CompareTo(object? obj)
        {
            throw new InvalidOperationException("boom");
        }

        public override string ToString() => _value;
    }

    [Fact]
    public void Refresh_Builds_Items_And_Sorts()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "b" },
            new() { Category = "a" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category,
            SortDirection = System.ComponentModel.ListSortDirection.Descending
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field
        };

        Assert.Equal(2, slicer.Items.Count);
        Assert.Equal("b", slicer.Items[0].Display);
        Assert.Equal("a", slicer.Items[1].Display);
    }

    [Fact]
    public void ShowItemsWithNoData_Uses_ItemsSource()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category,
            ShowItemsWithNoData = true,
            ItemsSource = new object?[] { "A", "B" }
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field
        };

        Assert.Contains(slicer.Items, item => item.Value?.ToString() == "B" && item.Count == 0);
    }

    [Fact]
    public void ApplyGroupSelectorToItemsSource_Transforms_Values()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "a" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category,
            ShowItemsWithNoData = true,
            ApplyGroupSelectorToItemsSource = true,
            GroupSelector = value => value?.ToString()?.ToUpperInvariant(),
            ItemsSource = new object?[] { "a", "b" }
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field
        };

        Assert.Contains(slicer.Items, item => item.Value?.ToString() == "B");
    }

    [Fact]
    public void FilterMode_Include_And_Exclude_Sync_Selection()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" },
            new() { Category = "B" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category,
            Filter = new PivotFieldFilter(included: new object?[] { "A" })
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field
        };

        Assert.True(slicer.Items.Single(item => item.Value?.ToString() == "A").IsSelected);
        Assert.False(slicer.Items.Single(item => item.Value?.ToString() == "B").IsSelected);

        slicer.FilterMode = PivotSlicerFilterMode.Exclude;

        Assert.False(slicer.Items.Single(item => item.Value?.ToString() == "A").IsSelected);
        Assert.True(slicer.Items.Single(item => item.Value?.ToString() == "B").IsSelected);
    }

    [Fact]
    public void SelectionMode_Single_Ensures_One_Selection()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" },
            new() { Category = "B" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field
        };

        slicer.SelectionMode = PivotSlicerSelectionMode.Single;

        Assert.Equal(1, slicer.Items.Count(item => item.IsSelected));
    }

    [Fact]
    public void Selection_Applies_To_Field_Filter()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" },
            new() { Category = "B" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field,
            FilterMode = PivotSlicerFilterMode.Include
        };

        slicer.Items[0].IsSelected = true;
        slicer.Items[1].IsSelected = false;

        Assert.Contains("A", field.Filter!.Included);
        Assert.DoesNotContain("B", field.Filter.Included);

        slicer.FilterMode = PivotSlicerFilterMode.Exclude;
        slicer.Items[0].IsSelected = true;

        Assert.Contains("A", field.Filter.Excluded);
    }

    [Fact]
    public void SelectAll_Clear_And_Invert_Selection()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" },
            new() { Category = "B" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field,
            FilterMode = PivotSlicerFilterMode.Exclude
        };

        slicer.SelectAll();
        Assert.All(slicer.Items, item => Assert.True(item.IsSelected));

        slicer.ClearSelection();
        Assert.All(slicer.Items, item => Assert.False(item.IsSelected));

        slicer.InvertSelection();
        Assert.All(slicer.Items, item => Assert.True(item.IsSelected));
    }

    [Fact]
    public void AutoRefresh_Can_Be_Disabled_And_Reenabled()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            AutoRefresh = false
        };

        slicer.ItemsSource = items;
        slicer.Field = field;

        Assert.Empty(slicer.Items);

        slicer.AutoRefresh = true;

        Assert.NotEmpty(slicer.Items);
    }

    [Fact]
    public void DeferRefresh_Delays_Updates()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            AutoRefresh = true
        };

        using (slicer.DeferRefresh())
        {
            slicer.ItemsSource = items;
            slicer.Field = field;
            Assert.Empty(slicer.Items);
        }

        Assert.NotEmpty(slicer.Items);
    }

    [Fact]
    public void EndUpdate_Throws_When_Unbalanced()
    {
        var slicer = new PivotSlicerModel();

        Assert.Throws<InvalidOperationException>(() => slicer.EndUpdate());
    }

    [Fact]
    public void Filter_Changes_Sync_Selection()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" },
            new() { Category = "B" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field
        };

        field.Filter!.Included.Add("B");

        Assert.True(slicer.Items.Single(item => item.Value?.ToString() == "B").IsSelected);
    }

    [Fact]
    public void Handles_Empty_Value_Label_And_Custom_Comparer()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = null },
            new() { Category = "a" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category,
            Comparer = null
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field,
            EmptyValueLabel = "(empty)"
        };

        Assert.Contains(slicer.Items, item => item.Display == "(empty)");
    }

    [Fact]
    public void Sorter_Falls_Back_When_IComparable_Throws()
    {
        var values = new object?[]
        {
            new ThrowingComparable("b"),
            new ThrowingComparable("a")
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => item
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = new object?[] { values[0], values[1] },
            Field = field
        };

        Assert.Equal(2, slicer.Items.Count);
    }

    [Fact]
    public void Property_Setters_Handle_NoChange_And_Getters()
    {
        var items = new ObservableCollection<Item> { new() { Category = "A" } };
        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Filter = new PivotFieldFilter()
        };

        slicer.Field = field;

        slicer.ItemsSource = items;
        slicer.Field = field;
        slicer.Filter = slicer.Filter;
        slicer.SelectionMode = slicer.SelectionMode;
        slicer.EmptyValueLabel = slicer.EmptyValueLabel;
        slicer.Culture = slicer.Culture;

        _ = slicer.ItemsSource;
        _ = slicer.Field;
        _ = slicer.Filter;
        _ = slicer.FilterMode;
        _ = slicer.SelectionMode;
        _ = slicer.EmptyValueLabel;
        _ = slicer.Culture;

        slicer.Field = null;

        var fieldChanged = typeof(PivotSlicerModel).GetMethod("Field_PropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldChanged!.Invoke(slicer, new object?[] { null, new PropertyChangedEventArgs(nameof(PivotAxisField.SortDirection)) });
    }

    [Fact]
    public void RequestRefresh_Branches_Are_Reached()
    {
        var items = new ObservableCollection<Item> { new() { Category = "A" } };
        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field
        };

        var reentered = false;
        slicer.Items.CollectionChanged += (_, _) =>
        {
            if (reentered)
            {
                return;
            }

            reentered = true;
            slicer.Refresh();
        };

        slicer.Refresh();

        slicer.AutoRefresh = false;
        slicer.EmptyValueLabel = "x";
        slicer.AutoRefresh = true;

        slicer.BeginUpdate();
        slicer.EmptyValueLabel = "pending";
        slicer.EndUpdate();

        var scope = slicer.DeferRefresh();
        scope.Dispose();
        scope.Dispose();

        slicer.Dispose();
    }

    [Fact]
    public void Selection_Methods_ShortCircuit_When_Empty()
    {
        var slicer = new PivotSlicerModel();

        slicer.SelectAll();
        slicer.ClearSelection();
        slicer.InvertSelection();

        slicer.SelectionMode = PivotSlicerSelectionMode.Single;
    }

    [Fact]
    public void SingleSelection_Deselects_Other_Items()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" },
            new() { Category = "B" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field
        };

        slicer.SelectionMode = PivotSlicerSelectionMode.Single;

        slicer.Items[0].IsSelected = true;
        slicer.Items[1].IsSelected = true;

        Assert.False(slicer.Items[0].IsSelected);
        Assert.True(slicer.Items[1].IsSelected);
    }

    [Fact]
    public void SyncSelectionFromFilter_Covers_FilterModes()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" },
            new() { Category = "B" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field
        };

        slicer.FilterMode = PivotSlicerFilterMode.Include;
        var filter = new PivotFieldFilter();
        slicer.Filter = filter;
        filter.Excluded.Add("B");

        Assert.True(slicer.Items.Single(item => item.Value?.ToString() == "A").IsSelected);
        Assert.False(slicer.Items.Single(item => item.Value?.ToString() == "B").IsSelected);
    }

    [Fact]
    public void SyncSelectionFromFilter_NoFilter_Uses_FilterMode()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" },
            new() { Category = "B" }
        };

        var field = new PivotAxisField
        {
            ValueSelector = item => ((Item)item!).Category
        };

        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field,
            FilterMode = PivotSlicerFilterMode.Exclude
        };

        Assert.All(slicer.Items, item => Assert.False(item.IsSelected));
    }

    [Fact]
    public void ApplySelectionToFilter_Returns_When_Field_Is_Null()
    {
        var slicer = new PivotSlicerModel();

        var method = typeof(PivotSlicerModel).GetMethod("ApplySelectionToFilter", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(slicer, Array.Empty<object?>());
    }

    [Fact]
    public void PivotSlicerComparer_Covers_Null_And_Numeric_Branches()
    {
        var comparerType = typeof(PivotSlicerModel).GetNestedType("PivotSlicerComparer", BindingFlags.NonPublic);
        var comparer = (IComparer<object?>)Activator.CreateInstance(comparerType!,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { CultureInfo.InvariantCulture },
            null)!;

        Assert.Equal(0, comparer.Compare("x", "x"));
        Assert.Equal(1, comparer.Compare(null, "x"));
        Assert.Equal(-1, comparer.Compare("x", null));
        Assert.Equal(-1, comparer.Compare(1, 2));
    }

    [Fact]
    public void Filter_Changed_Suppressed_Ignores_Update()
    {
        var slicer = new PivotSlicerModel();
        var filter = new PivotFieldFilter();

        slicer.Filter = filter;

        var suppressField = typeof(PivotSlicerModel).GetField("_suppressFilterSync", BindingFlags.Instance | BindingFlags.NonPublic);
        suppressField!.SetValue(slicer, true);

        var method = typeof(PivotSlicerModel).GetMethod("Filter_Changed", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(slicer, new object?[] { null, EventArgs.Empty });

        suppressField.SetValue(slicer, false);
    }

    [Fact]
    public void PivotValueFilterModel_Reads_Properties_And_Ignores_Null_Field()
    {
        var model = new PivotValueFilterModel
        {
            FilterType = PivotValueFilterType.GreaterThan,
            Value = 1,
            Value2 = 2,
            Count = 3,
            Percent = 10
        };

        _ = model.FilterType;
        _ = model.ValueField;
        _ = model.Value;
        _ = model.Value2;
        _ = model.Count;
        _ = model.Percent;
    }

    [Fact]
    public void Culture_Set_To_Null_Uses_CurrentCulture()
    {
        var slicer = new PivotSlicerModel();

        slicer.Culture = null;

        Assert.NotNull(slicer.Culture);
    }

    [Fact]
    public void RequestRefresh_WhenRefreshing_SetsPending()
    {
        var slicer = new PivotSlicerModel();
        var isRefreshingField = typeof(PivotSlicerModel).GetField("_isRefreshing", BindingFlags.Instance | BindingFlags.NonPublic);
        var pendingField = typeof(PivotSlicerModel).GetField("_pendingRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
        isRefreshingField!.SetValue(slicer, true);

        var method = typeof(PivotSlicerModel).GetMethod("RequestRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(slicer, Array.Empty<object?>());

        Assert.True((bool)pendingField!.GetValue(slicer)!);
        isRefreshingField.SetValue(slicer, false);
    }

    [Fact]
    public void Items_CollectionChanged_Triggers_RequestRefresh()
    {
        var items = new ObservableCollection<Item>();
        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = new PivotAxisField { ValueSelector = item => ((Item)item!).Category },
            AutoRefresh = false
        };

        var pendingField = typeof(PivotSlicerModel).GetField("_pendingRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
        pendingField!.SetValue(slicer, false);

        items.Add(new Item { Category = "A" });

        Assert.True((bool)pendingField.GetValue(slicer)!);
    }

    [Fact]
    public void Field_PropertyChanged_Refreshes_For_NonFilter()
    {
        var slicer = new PivotSlicerModel { AutoRefresh = false };
        var field = new PivotAxisField { Header = "A", ValueSelector = item => ((Item)item!).Category };
        slicer.Field = field;

        field.Header = "B";

        var pendingField = typeof(PivotSlicerModel).GetField("_pendingRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.True((bool)pendingField!.GetValue(slicer)!);
    }

    [Fact]
    public void EnsureFilter_Returns_When_Field_Null()
    {
        var slicer = new PivotSlicerModel();

        var method = typeof(PivotSlicerModel).GetMethod("EnsureFilter", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(slicer, Array.Empty<object?>());
    }

    [Fact]
    public void SyncSelectionFromFilter_NoFilter_Updates_Items()
    {
        var items = new ObservableCollection<Item>
        {
            new() { Category = "A" },
            new() { Category = "B" }
        };

        var field = new PivotAxisField { ValueSelector = item => ((Item)item!).Category };
        var slicer = new PivotSlicerModel
        {
            ItemsSource = items,
            Field = field,
            FilterMode = PivotSlicerFilterMode.Include
        };

        slicer.Filter = null;

        var method = typeof(PivotSlicerModel).GetMethod("SyncSelectionFromFilter", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(slicer, Array.Empty<object?>());

        Assert.All(slicer.Items, item => Assert.True(item.IsSelected));
    }
}
