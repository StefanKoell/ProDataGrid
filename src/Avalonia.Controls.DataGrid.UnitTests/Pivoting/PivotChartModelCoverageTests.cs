using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotChartModelCoverageTests
{
    private sealed class Sale
    {
        public string Region { get; init; } = string.Empty;
        public string Product { get; init; } = string.Empty;
        public double Amount { get; init; }
    }

    [Fact]
    public void ChartModel_Properties_And_RequestRefresh_Branches()
    {
        var pivot = CreatePivot();
        var chart = new PivotChartModel();

        var series = new PivotChartSeries("name", new double?[] { 1d }, source: "src");
        Assert.Equal("src", series.Source);

        var changeArgs = new PivotChartChangedEventArgs(new[] { "cat" }, new[] { series });
        Assert.Single(changeArgs.Categories);
        Assert.Single(changeArgs.Series);

        chart.Pivot = pivot;
        Assert.Same(pivot, chart.Pivot);
        chart.Pivot = pivot;

        _ = chart.SeriesSource;
        _ = chart.IncludeSubtotals;
        chart.IncludeSubtotals = true;
        _ = chart.IncludeGrandTotals;
        chart.IncludeGrandTotals = true;
        _ = chart.IncludeEmptySeries;
        _ = chart.ValueField;

        var valueField = pivot.ValueFields[0];
        chart.ValueField = valueField;
        chart.ValueField = valueField;

        _ = chart.Culture;
        chart.Culture = CultureInfo.InvariantCulture;
        chart.Culture = CultureInfo.InvariantCulture;

        chart.AutoRefresh = chart.AutoRefresh;

        using (chart.DeferRefresh())
        {
            chart.IncludeEmptySeries = true;
        }

        chart.BeginUpdate();
        chart.IncludeSubtotals = false;
        chart.EndUpdate();

        chart.AutoRefresh = false;
        chart.IncludeGrandTotals = false;
        chart.AutoRefresh = true;

        chart.Pivot = null;
        chart.Refresh();
        Assert.Empty(chart.Categories);
        Assert.Empty(chart.Series);

        chart.Dispose();
    }

    [Fact]
    public void ChartModel_Reentrancy_Sets_PendingRefresh()
    {
        var pivot = CreatePivot();
        var chart = new PivotChartModel
        {
            Pivot = pivot
        };

        var toggled = false;
        chart.ChartChanged += (_, _) =>
        {
            if (toggled)
            {
                return;
            }

            toggled = true;
            chart.Refresh();
            chart.IncludeEmptySeries = true;
        };

        chart.Refresh();
    }

    [Fact]
    public void ChartModel_Private_Building_Helpers_Cover_Branches()
    {
        var chart = new PivotChartModel();

        var row = new PivotRow(
            PivotRowType.Detail,
            0,
            Array.Empty<object?>(),
            Array.Empty<object?>(),
            null,
            0d,
            0,
            null,
            null);
        var column = new PivotColumn(
            5,
            PivotColumnType.Detail,
            Array.Empty<object?>(),
            Array.Empty<string?>(),
            null,
            null,
            new PivotHeader(Array.Empty<string>()));

        InvokePrivate(chart, "BuildSeriesFromRows",
            new List<PivotRow> { row },
            new List<PivotColumn> { column },
            false);

        InvokePrivate(chart, "BuildSeriesFromColumns",
            new List<PivotRow> { row },
            new List<PivotColumn> { column },
            false);

        var valueField = new PivotValueField { Header = "Amount", Key = "Amt" };
        var header = new PivotHeader(new[] { "Seg1", "Seg2" });
        var labelColumn = new PivotColumn(0, PivotColumnType.Detail, new object?[] { "A" }, new[] { "A" }, valueField, 0, header);
        var displayColumn = new PivotColumn(0, PivotColumnType.Detail, new object?[] { "B" }, new[] { "B" }, null, null, new PivotHeader(Array.Empty<string>()));
        var valueColumn = new PivotColumn(0, PivotColumnType.Detail, Array.Empty<object?>(), Array.Empty<string?>(), valueField, 0, new PivotHeader(Array.Empty<string>()));
        var emptyColumn = new PivotColumn(0, PivotColumnType.Detail, Array.Empty<object?>(), Array.Empty<string?>(), null, null, new PivotHeader(Array.Empty<string>()));

        Assert.Equal("Seg1 / Seg2", InvokePrivate(chart, "BuildColumnLabel", labelColumn));
        Assert.Equal("B", InvokePrivate(chart, "BuildColumnLabel", displayColumn));
        Assert.Equal("Amount", InvokePrivate(chart, "BuildColumnLabel", valueColumn));
        Assert.Null(InvokePrivate(chart, "BuildColumnLabel", emptyColumn));

        Assert.Null(InvokePrivate(chart, "JoinPathValues", new object?[] { Array.Empty<object?>() }));
        Assert.Null(InvokePrivate(chart, "JoinPathValues", new object?[] { new object?[] { null, "" } }));

        Assert.Null(InvokePrivateStatic(typeof(PivotChartModel), "JoinSegments", new List<string>()));
        Assert.Null(InvokePrivateStatic(typeof(PivotChartModel), "JoinSegments", new List<string> { string.Empty }));

        var numberFormat = new NumberFormatInfo { PercentSymbol = "pct" };
        var percentField = new PivotValueField { FormatProvider = numberFormat };
        Assert.Equal(0.1, (double)InvokePrivate(chart, "ToNumeric", "10%", percentField)!);

        Assert.Equal(10d, (double)InvokePrivate(chart, "ToNumeric", "10", percentField)!);
        Assert.Null(InvokePrivate(chart, "ToNumeric", "bad%", percentField));
        Assert.Null(InvokePrivate(chart, "ToNumeric", "bad", percentField));

        var cultureField = new PivotValueField { FormatProvider = CultureInfo.InvariantCulture };
        Assert.Equal(1.5, (double)InvokePrivate(chart, "ToNumeric", "1.5", cultureField)!);

        var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = "d";
        var customField = new PivotValueField { FormatProvider = customCulture };
        Assert.Equal(1.5, (double)InvokePrivate(chart, "ToNumeric", "1d5", customField)!);

        var formatInfoField = new PivotValueField { FormatProvider = new NumberFormatInfo() };
        Assert.Equal(2d, (double)InvokePrivate(chart, "ToNumeric", "2", formatInfoField)!);

        var updateScope = chart.DeferRefresh();
        updateScope.Dispose();
        updateScope.Dispose();
    }

    [Fact]
    public void ChartModel_Inclusion_And_ValueField_Matching_Branches()
    {
        var chart = new PivotChartModel();
        var row = new PivotRow(PivotRowType.Subtotal, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, null, null);
        var column = new PivotColumn(0, PivotColumnType.GrandTotal, Array.Empty<object?>(), Array.Empty<string?>(), null, null, new PivotHeader(Array.Empty<string>()));

        Assert.False((bool)InvokePrivate(chart, "IsRowIncluded", row)!);
        Assert.False((bool)InvokePrivate(chart, "IsColumnIncluded", column)!);

        var valueField = new PivotValueField { Header = "Amount", Key = "A" };
        Assert.False((bool)InvokePrivateStatic(typeof(PivotChartModel), "MatchesValueField", null, valueField)!);
        Assert.True((bool)InvokePrivateStatic(typeof(PivotChartModel), "MatchesValueField", valueField, valueField)!);
        Assert.True((bool)InvokePrivateStatic(typeof(PivotChartModel), "MatchesValueField", new PivotValueField { Key = "A" }, valueField)!);
    }

    [Fact]
    public void ChartModel_PivotChanged_And_Inclusion_Defaults()
    {
        var pivot = CreatePivot();
        var chart = new PivotChartModel { Pivot = pivot };

        var changed = false;
        chart.ChartChanged += (_, _) => changed = true;
        pivot.Refresh();

        Assert.True(changed);

        chart.IncludeSubtotals = true;
        chart.IncludeGrandTotals = true;

        var subtotalRow = new PivotRow(PivotRowType.Subtotal, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, null, null);
        var grandRow = new PivotRow(PivotRowType.GrandTotal, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, null, null);
        var unknownRow = new PivotRow((PivotRowType)123, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, null, null);
        Assert.True((bool)InvokePrivate(chart, "IsRowIncluded", subtotalRow)!);
        Assert.True((bool)InvokePrivate(chart, "IsRowIncluded", grandRow)!);
        Assert.False((bool)InvokePrivate(chart, "IsRowIncluded", unknownRow)!);

        var subtotalColumn = new PivotColumn(0, PivotColumnType.Subtotal, Array.Empty<object?>(), Array.Empty<string?>(), null, null, new PivotHeader(Array.Empty<string>()));
        var grandColumn = new PivotColumn(0, PivotColumnType.GrandTotal, Array.Empty<object?>(), Array.Empty<string?>(), null, null, new PivotHeader(Array.Empty<string>()));
        var unknownColumn = new PivotColumn(0, (PivotColumnType)123, Array.Empty<object?>(), Array.Empty<string?>(), null, null, new PivotHeader(Array.Empty<string>()));
        Assert.True((bool)InvokePrivate(chart, "IsColumnIncluded", subtotalColumn)!);
        Assert.True((bool)InvokePrivate(chart, "IsColumnIncluded", grandColumn)!);
        Assert.False((bool)InvokePrivate(chart, "IsColumnIncluded", unknownColumn)!);
    }

    private static PivotTableModel CreatePivot()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 10 },
            new Sale { Region = "South", Product = "B", Amount = 20 }
        };

        var model = new PivotTableModel { Culture = CultureInfo.InvariantCulture };
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

            model.Layout.ShowRowSubtotals = false;
            model.Layout.ShowColumnSubtotals = false;
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        return model;
    }

    private static object? InvokePrivate(object instance, string name, params object?[] args)
    {
        var method = instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        return method!.Invoke(instance, args);
    }

    private static object? InvokePrivateStatic(Type type, string name, params object?[] args)
    {
        var method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
        return method!.Invoke(null, args);
    }
}
