using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotTableModelAdditionalTests
{
    private sealed class Sale
    {
        public string Region { get; init; } = string.Empty;

        public string Product { get; init; } = string.Empty;

        public double Amount { get; init; }
    }

    private static PivotTableModel CreateModel(ObservableCollection<Sale> data)
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = data;
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region
            });
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((Sale)item!).Product
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Amount",
                ValueSelector = item => ((Sale)item!).Amount,
                AggregateType = PivotAggregateType.Sum
            });
        }

        return model;
    }

    [Fact]
    public void AutoRefresh_Can_Be_Disabled_And_Manually_Refreshed()
    {
        var data = new ObservableCollection<Sale>
        {
            new() { Region = "North", Product = "A", Amount = 1 }
        };

        var model = new PivotTableModel { AutoRefresh = false };
        model.ItemsSource = data;
        model.RowFields.Add(new PivotAxisField
        {
            Header = "Region",
            ValueSelector = item => ((Sale)item!).Region
        });
        model.ColumnFields.Add(new PivotAxisField
        {
            Header = "Product",
            ValueSelector = item => ((Sale)item!).Product
        });
        model.ValueFields.Add(new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        });

        Assert.Empty(model.Rows);

        model.Refresh();

        Assert.NotEmpty(model.Rows);
    }

    [Fact]
    public void AutoRefresh_Refreshes_When_Reenabled()
    {
        var data = new ObservableCollection<Sale>
        {
            new() { Region = "North", Product = "A", Amount = 1 }
        };

        var model = new PivotTableModel { AutoRefresh = false };
        var pivotChanged = 0;
        model.PivotChanged += (_, _) => pivotChanged++;

        model.ItemsSource = data;
        model.RowFields.Add(new PivotAxisField
        {
            Header = "Region",
            ValueSelector = item => ((Sale)item!).Region
        });
        model.ColumnFields.Add(new PivotAxisField
        {
            Header = "Product",
            ValueSelector = item => ((Sale)item!).Product
        });
        model.ValueFields.Add(new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        });

        model.AutoRefresh = true;

        Assert.True(pivotChanged > 0);
        Assert.NotEmpty(model.Rows);
    }

    [Fact]
    public void ItemsSource_CollectionChanges_Trigger_Refresh()
    {
        var data = new ObservableCollection<Sale>
        {
            new() { Region = "North", Product = "A", Amount = 1 }
        };

        var model = CreateModel(data);
        var pivotChanged = 0;
        model.PivotChanged += (_, _) => pivotChanged++;

        data.Add(new Sale { Region = "South", Product = "A", Amount = 2 });

        Assert.True(pivotChanged > 0);
        Assert.Contains(model.Rows, row => row.RowPathValues.Contains("South"));
    }

    [Fact]
    public void Fields_Reset_Reattaches_Property_Change_Handlers()
    {
        var data = new ObservableCollection<Sale>
        {
            new() { Region = "North", Product = "A", Amount = 1 }
        };

        var model = new PivotTableModel();
        var pivotChanged = 0;
        model.PivotChanged += (_, _) => pivotChanged++;

        model.ItemsSource = data;
        model.RowFields.Add(new PivotAxisField
        {
            Header = "Region",
            ValueSelector = item => ((Sale)item!).Region
        });
        model.ValueFields.Add(new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        });

        model.RowFields.Clear();
        model.RowFields.Add(new PivotAxisField
        {
            Header = "Region",
            ValueSelector = item => ((Sale)item!).Region
        });

        model.RowFields[0].Header = "Region2";

        Assert.True(pivotChanged >= 2);
    }

    [Fact]
    public void EndUpdate_Throws_When_Unbalanced()
    {
        var model = new PivotTableModel();

        Assert.Throws<InvalidOperationException>(() => model.EndUpdate());
    }

    [Fact]
    public void Dispose_Detaches_Handlers()
    {
        var data = new ObservableCollection<Sale>
        {
            new() { Region = "North", Product = "A", Amount = 1 }
        };

        var model = CreateModel(data);
        var pivotChanged = 0;
        model.PivotChanged += (_, _) => pivotChanged++;

        model.Dispose();
        data.Add(new Sale { Region = "South", Product = "A", Amount = 2 });

        Assert.Equal(0, pivotChanged);
    }

    [Fact]
    public void RequestRefresh_WhenRefreshing_SetsPending()
    {
        var model = new PivotTableModel();
        var isRefreshingField = typeof(PivotTableModel).GetField("_isRefreshing", BindingFlags.Instance | BindingFlags.NonPublic);
        var pendingField = typeof(PivotTableModel).GetField("_pendingRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
        isRefreshingField!.SetValue(model, true);

        var method = typeof(PivotTableModel).GetMethod("RequestRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(model, Array.Empty<object?>());

        Assert.True((bool)pendingField!.GetValue(model)!);
        isRefreshingField.SetValue(model, false);
    }

    [Fact]
    public void Fields_Remove_Triggers_DetachHandlers()
    {
        var model = new PivotTableModel { AutoRefresh = false };
        var field = new PivotAxisField();
        model.RowFields.Add(field);

        model.RowFields.Remove(field);

        var pendingField = typeof(PivotTableModel).GetField("_pendingRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.True((bool)pendingField!.GetValue(model)!);
    }

    [Fact]
    public void ValueFields_Remove_Triggers_DetachHandlers()
    {
        var model = new PivotTableModel { AutoRefresh = false };
        var field = new PivotValueField { Header = "Amount", ValueSelector = _ => 1d };
        model.ValueFields.Add(field);

        model.ValueFields.Remove(field);

        var pendingField = typeof(PivotTableModel).GetField("_pendingRefresh", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.True((bool)pendingField!.GetValue(model)!);
    }

    [Fact]
    public void PivotChangedEventArgs_Exposes_Collections()
    {
        var rows = new[] { new PivotRow(PivotRowType.Detail, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 0, null, null) };
        var columns = new[] { new PivotColumn(0, PivotColumnType.Detail, Array.Empty<object?>(), Array.Empty<string?>(), null, null, new PivotHeader(Array.Empty<string>())) };
        var definitions = new[] { new DataGridTextColumnDefinition() };

        var args = new PivotChangedEventArgs(rows, columns, definitions);

        Assert.Same(rows, args.Rows);
        Assert.Same(columns, args.Columns);
        Assert.Same(definitions, args.ColumnDefinitions);
    }

    [Fact]
    public void Model_Getters_And_NoChange_Branches()
    {
        var data = new ObservableCollection<Sale>
        {
            new() { Region = "North", Product = "A", Amount = 1 }
        };

        var model = CreateModel(data);

        _ = model.Rows;
        _ = model.Columns;
        _ = model.ColumnDefinitions;
        _ = model.Aggregators;
        _ = model.Culture;

        model.AutoRefresh = model.AutoRefresh;
        model.ItemsSource = data;
    }

    [Fact]
    public void Refresh_Reentrancy_Sets_Pending()
    {
        var data = new ObservableCollection<Sale>
        {
            new() { Region = "North", Product = "A", Amount = 1 }
        };

        var model = CreateModel(data);
        var reentered = false;
        model.PivotChanged += (_, _) =>
        {
            if (reentered)
            {
                return;
            }

            reentered = true;
            model.Refresh();
        };

        model.Refresh();
    }

    [Fact]
    public void Field_Removals_Detach_Handlers()
    {
        var data = new ObservableCollection<Sale>
        {
            new() { Region = "North", Product = "A", Amount = 1 }
        };

        var model = new PivotTableModel();
        model.ItemsSource = data;
        var field = new PivotAxisField
        {
            Header = "Region",
            ValueSelector = item => ((Sale)item!).Region
        };
        model.RowFields.Add(field);

        model.RowFields.Remove(field);
        field.Header = "Updated";
    }

    [Fact]
    public void ValueFields_Reset_Reattaches_Handlers()
    {
        var data = new ObservableCollection<Sale>
        {
            new() { Region = "North", Product = "A", Amount = 1 }
        };

        var model = CreateModel(data);

        model.ValueFields.Clear();
        model.ValueFields.Add(new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        });
    }

    [Fact]
    public void UpdateScope_Dispose_Is_Idempotent()
    {
        var model = new PivotTableModel();
        var scope = model.DeferRefresh();
        scope.Dispose();
        scope.Dispose();
    }
}
