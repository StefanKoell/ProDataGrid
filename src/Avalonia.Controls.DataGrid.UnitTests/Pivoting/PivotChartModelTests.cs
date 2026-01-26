using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotChartModelTests
{
    private sealed class Sale
    {
        public string Region { get; init; } = string.Empty;
        public string Product { get; init; } = string.Empty;
        public double Amount { get; init; }
        public int Units { get; init; }
    }

    private static PivotTableModel CreatePivot(IEnumerable<Sale> data, bool valuesInRows = false, bool includeMissing = false)
    {
        var model = new PivotTableModel { Culture = CultureInfo.InvariantCulture };
        using (model.DeferRefresh())
        {
            model.ItemsSource = data.ToList();
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                ShowItemsWithNoData = includeMissing,
                ItemsSource = includeMissing ? new object?[] { "North", "South", "West" } : null
            });
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((Sale)item!).Product,
                SortDirection = ListSortDirection.Ascending
            });

            var amountField = new PivotValueField
            {
                Header = "Amount",
                Key = "AmountKey",
                ValueSelector = item => ((Sale)item!).Amount,
                AggregateType = PivotAggregateType.Sum
            };

            var unitsField = new PivotValueField
            {
                Header = "Units",
                ValueSelector = item => ((Sale)item!).Units,
                AggregateType = PivotAggregateType.Sum
            };

            model.ValueFields.Add(amountField);
            model.ValueFields.Add(unitsField);

            if (valuesInRows)
            {
                model.Layout.ValuesPosition = PivotValuesPosition.Rows;
                model.Layout.RowLayout = PivotRowLayout.Tabular;
            }

            model.Layout.ShowRowSubtotals = false;
            model.Layout.ShowColumnSubtotals = false;
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        return model;
    }

    [Fact]
    public void ChartModel_Builds_Series_From_Rows()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 10, Units = 1 },
            new Sale { Region = "South", Product = "B", Amount = 20, Units = 2 }
        };

        var pivot = CreatePivot(data);
        var chart = new PivotChartModel
        {
            Pivot = pivot,
            SeriesSource = PivotChartSeriesSource.Rows,
            IncludeSubtotals = false,
            IncludeGrandTotals = false
        };

        Assert.Equal(4, chart.Categories.Count);
        Assert.Equal(2, chart.Series.Count);
    }

    [Fact]
    public void ChartModel_Builds_Series_From_Columns()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 10, Units = 1 },
            new Sale { Region = "South", Product = "B", Amount = 20, Units = 2 }
        };

        var pivot = CreatePivot(data);
        var chart = new PivotChartModel
        {
            Pivot = pivot,
            SeriesSource = PivotChartSeriesSource.Columns,
            IncludeSubtotals = false,
            IncludeGrandTotals = false
        };

        Assert.Equal(2, chart.Categories.Count);
        Assert.Equal(4, chart.Series.Count);
    }

    [Fact]
    public void ChartModel_Filters_By_ValueField_Key_And_Header()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 10, Units = 1 },
            new Sale { Region = "South", Product = "B", Amount = 20, Units = 2 }
        };

        var pivot = CreatePivot(data);
        var chart = new PivotChartModel
        {
            Pivot = pivot,
            SeriesSource = PivotChartSeriesSource.Rows,
            IncludeSubtotals = false,
            IncludeGrandTotals = false,
            ValueField = new PivotValueField { Key = "AmountKey" }
        };

        Assert.Equal(2, chart.Categories.Count);

        chart.ValueField = new PivotValueField { Header = "Units" };

        Assert.Equal(2, chart.Categories.Count);
    }

    [Fact]
    public void ChartModel_Uses_Label_Selectors()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 10, Units = 1 }
        };

        var pivot = CreatePivot(data);
        var chart = new PivotChartModel
        {
            SeriesSource = PivotChartSeriesSource.Rows,
            RowLabelSelector = _ => "row",
            ColumnLabelSelector = _ => "col",
            Pivot = pivot
        };

        Assert.Equal(2, chart.Categories.Count);
        Assert.All(chart.Categories, label => Assert.Equal("col", label));
        Assert.Single(chart.Series);
        Assert.Equal("row", chart.Series[0].Name);
    }

    [Fact]
    public void ChartModel_Includes_Empty_Series_When_Configured()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 10, Units = 1 }
        };

        var pivot = CreatePivot(data, includeMissing: true);
        var chart = new PivotChartModel
        {
            Pivot = pivot,
            SeriesSource = PivotChartSeriesSource.Rows,
            IncludeEmptySeries = false
        };

        var withoutEmpty = chart.Series.Count;

        chart.IncludeEmptySeries = true;

        Assert.True(chart.Series.Count >= withoutEmpty);
    }

    [Fact]
    public void ChartModel_Parses_Percent_Strings_For_ValuesInRows()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 10, Units = 1 },
            new Sale { Region = "North", Product = "B", Amount = 20, Units = 2 }
        };

        var pivot = CreatePivot(data, valuesInRows: true);
        var percentField = pivot.ValueFields[0];
        percentField.DisplayMode = PivotValueDisplayMode.PercentOfRowTotal;
        percentField.StringFormat = "P0";

        pivot.Refresh();

        var chart = new PivotChartModel
        {
            Pivot = pivot,
            SeriesSource = PivotChartSeriesSource.Rows,
            ValueField = percentField
        };

        var values = chart.Series.SelectMany(series => series.Values).Where(value => value.HasValue).ToList();
        Assert.NotEmpty(values);
        Assert.All(values, value => Assert.InRange(value!.Value, 0.3, 0.7));
    }

    [Fact]
    public void ChartModel_AutoRefresh_Can_Be_Disabled_And_Reenabled()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 10, Units = 1 }
        };

        var pivot = CreatePivot(data);
        var chart = new PivotChartModel
        {
            AutoRefresh = false
        };

        chart.Pivot = pivot;
        Assert.Empty(chart.Categories);

        chart.AutoRefresh = true;

        Assert.NotEmpty(chart.Categories);
    }

    [Fact]
    public void ChartModel_EndUpdate_Throws_When_Unbalanced()
    {
        var chart = new PivotChartModel();

        Assert.Throws<InvalidOperationException>(() => chart.EndUpdate());
    }
}
