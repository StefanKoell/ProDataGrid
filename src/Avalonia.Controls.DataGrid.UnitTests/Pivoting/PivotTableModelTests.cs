using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotTableModelTests
{
    private sealed class Sale
    {
        public string Region { get; init; } = string.Empty;

        public string Product { get; init; } = string.Empty;

        public double Amount { get; init; }

        public int Units { get; init; }
    }

    private sealed class NoteSale
    {
        public string Region { get; init; } = string.Empty;

        public string Product { get; init; } = string.Empty;

        public string? Note { get; init; }
    }

    private sealed class Metric
    {
        public int Score { get; init; }
    }

    private static readonly Sale[] SampleSales =
    {
        new() { Region = "North", Product = "A", Amount = 10, Units = 1 },
        new() { Region = "North", Product = "B", Amount = 20, Units = 2 },
        new() { Region = "South", Product = "A", Amount = 30, Units = 3 },
        new() { Region = "South", Product = "B", Amount = 40, Units = 4 }
    };

    [Fact]
    public void Builds_Sum_With_GrandTotals()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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

        model.Refresh();

        Assert.Equal(3, model.Rows.Count);
        Assert.Equal(3, model.Columns.Count);

        var northRow = GetRow(model, "North");
        var southRow = GetRow(model, "South");
        var totalRow = GetRow(model, null);

        var columnA = GetColumn(model, "A");
        var columnB = GetColumn(model, "B");
        var totalColumn = GetColumn(model, null);

        Assert.Equal(10d, Convert.ToDouble(northRow.CellValues[columnA.Index]!));
        Assert.Equal(20d, Convert.ToDouble(northRow.CellValues[columnB.Index]!));
        Assert.Equal(30d, Convert.ToDouble(northRow.CellValues[totalColumn.Index]!));

        Assert.Equal(30d, Convert.ToDouble(southRow.CellValues[columnA.Index]!));
        Assert.Equal(40d, Convert.ToDouble(southRow.CellValues[columnB.Index]!));
        Assert.Equal(70d, Convert.ToDouble(southRow.CellValues[totalColumn.Index]!));

        Assert.Equal(40d, Convert.ToDouble(totalRow.CellValues[columnA.Index]!));
        Assert.Equal(60d, Convert.ToDouble(totalRow.CellValues[columnB.Index]!));
        Assert.Equal(100d, Convert.ToDouble(totalRow.CellValues[totalColumn.Index]!));
    }

    [Fact]
    public void PercentOfRowTotal_UsesHiddenTotals()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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
                AggregateType = PivotAggregateType.Sum,
                DisplayMode = PivotValueDisplayMode.PercentOfRowTotal
            });

            model.Layout.ShowColumnGrandTotals = false;
            model.Layout.ShowRowGrandTotals = false;
        }

        model.Refresh();

        Assert.Equal(2, model.Columns.Count);
        Assert.Equal(2, model.Rows.Count);

        var northRow = GetRow(model, "North");
        var southRow = GetRow(model, "South");
        var columnA = GetColumn(model, "A");
        var columnB = GetColumn(model, "B");

        var northA = Convert.ToDouble(northRow.CellValues[columnA.Index]!);
        var northB = Convert.ToDouble(northRow.CellValues[columnB.Index]!);
        Assert.InRange(northA, 0.3333, 0.3334);
        Assert.InRange(northB, 0.6666, 0.6667);

        var southA = Convert.ToDouble(southRow.CellValues[columnA.Index]!);
        var southB = Convert.ToDouble(southRow.CellValues[columnB.Index]!);
        Assert.InRange(southA, 0.4285, 0.4286);
        Assert.InRange(southB, 0.5714, 0.5715);
    }

    [Fact]
    public void ValuesInRows_AddsValueRowsAndColumn()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Units",
                ValueSelector = item => ((Sale)item!).Units,
                AggregateType = PivotAggregateType.Sum
            });

            model.Layout.ValuesPosition = PivotValuesPosition.Rows;
            model.Layout.RowLayout = PivotRowLayout.Tabular;
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Equal(2, model.Columns.Count);
        Assert.Equal(4, model.Rows.Count);
        Assert.Equal(4, model.ColumnDefinitions.Count);

        Assert.Equal(2, model.Rows.Count(row => row.ValueField?.Header == "Amount"));
        Assert.Equal(2, model.Rows.Count(row => row.ValueField?.Header == "Units"));
    }

    [Fact]
    public void ValuesInRows_Uses_NullLabel_For_Nulls()
    {
        var data = new[]
        {
            new NoteSale { Region = "North", Product = "A", Note = null }
        };

        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = data;
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((NoteSale)item!).Region
            });
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((NoteSale)item!).Product
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Note",
                ValueSelector = item => ((NoteSale)item!).Note,
                AggregateType = PivotAggregateType.First,
                NullLabel = "NULL"
            });

            model.Layout.ValuesPosition = PivotValuesPosition.Rows;
            model.Layout.EmptyValueLabel = "EMPTY";
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var row = GetRow(model, "North");
        var column = GetColumn(model, "A");

        Assert.Equal("NULL", row.CellValues[column.Index]);
    }

    [Fact]
    public void Filters_Limit_RowValues()
    {
        var model = new PivotTableModel();

        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
            var regionField = new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                Filter = new PivotFieldFilter(included: new[] { "North" })
            };

            model.RowFields.Add(regionField);
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

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Single(model.Rows);
        var row = model.Rows[0];
        Assert.Single(row.RowPathValues);
        Assert.Equal("North", row.RowPathValues[0]);
    }

    [Fact]
    public void ShowItemsWithNoData_AddsMissingMembers()
    {
        var model = new PivotTableModel();

        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                ShowItemsWithNoData = true,
                ItemsSource = new[] { "North", "South", "West" }
            });
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((Sale)item!).Product,
                ShowItemsWithNoData = true,
                ItemsSource = new[] { "A", "B", "C" }
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Amount",
                ValueSelector = item => ((Sale)item!).Amount,
                AggregateType = PivotAggregateType.Sum
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var westRow = model.Rows.Single(row => row.RowType == PivotRowType.Detail &&
            row.RowPathValues.Length == 1 &&
            Equals(row.RowPathValues[0], "West"));
        var columnC = model.Columns.Single(column => column.ColumnType == PivotColumnType.Detail &&
            column.ColumnPathValues.Length == 1 &&
            Equals(column.ColumnPathValues[0], "C"));

        Assert.Null(westRow.CellValues[columnC.Index]);
    }

    [Fact]
    public void SubtotalPosition_Start_InsertsSubtotalBeforeDetails()
    {
        var model = new PivotTableModel();

        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                SubtotalPosition = PivotTotalPosition.Start
            });
            model.RowFields.Add(new PivotAxisField
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

            model.Layout.ShowRowSubtotals = true;
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var subtotalIndex = model.Rows
            .Select((row, index) => new { row, index })
            .FirstOrDefault(entry => entry.row.RowType == PivotRowType.Subtotal &&
                entry.row.RowPathValues.Length == 1 &&
                Equals(entry.row.RowPathValues[0], "North"))
            ?.index ?? -1;

        var detailIndex = model.Rows
            .Select((row, index) => new { row, index })
            .FirstOrDefault(entry => entry.row.RowType == PivotRowType.Detail &&
                entry.row.RowPathValues.Length == 2 &&
                Equals(entry.row.RowPathValues[0], "North"))
            ?.index ?? -1;

        Assert.True(subtotalIndex >= 0);
        Assert.True(detailIndex >= 0);
        Assert.True(subtotalIndex < detailIndex);
    }

    [Fact]
    public void NullGroupKeys_AreHandled()
    {
        var data = SampleSales
            .Concat(new[] { new Sale { Region = null!, Product = "A", Amount = 5, Units = 1 } })
            .ToList();

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

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Contains(model.Rows, row => row.RowType == PivotRowType.Detail &&
            row.RowPathValues.Length == 1 &&
            row.RowPathValues[0] == null);
    }

    [Fact]
    public void ItemsSource_AppliesGroupSelector_WhenEnabled()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = new[] { new Metric { Score = 1 } };
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Bucket",
                ValueSelector = item => ((Metric)item!).Score,
                GroupSelector = value => value is int score && score >= 2 ? "High" : "Low",
                ShowItemsWithNoData = true,
                ApplyGroupSelectorToItemsSource = true,
                ItemsSource = new object?[] { 1, 2 }
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Count",
                ValueSelector = _ => 1,
                AggregateType = PivotAggregateType.Count
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Contains(model.Rows, row => row.RowType == PivotRowType.Detail &&
            row.RowPathValues.Length == 1 &&
            Equals(row.RowPathValues[0], "High"));
    }

    [Fact]
    public void ColumnDefinitions_ExposeRowAndValueAccessors()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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

            model.Layout.RowLayout = PivotRowLayout.Tabular;
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumn(model, "A");

        var rowHeaderDefinition = model.ColumnDefinitions[0];
        Assert.NotNull(rowHeaderDefinition.ValueAccessor);
        Assert.Equal("North", rowHeaderDefinition.ValueAccessor!.GetValue(northRow));
        Assert.NotNull(rowHeaderDefinition.Options?.SearchTextProvider);
        Assert.Equal("North", rowHeaderDefinition.Options!.SearchTextProvider!(northRow));

        var valueDefinitionIndex = model.RowFields.Count + columnA.Index;
        var valueDefinition = model.ColumnDefinitions[valueDefinitionIndex];
        Assert.NotNull(valueDefinition.ValueAccessor);
        var value = valueDefinition.ValueAccessor!.GetValue(northRow);
        Assert.Equal(10d, Convert.ToDouble(value));
    }

    [Fact]
    public void ColumnDefinitions_SearchTextProvider_FormatsValueFields()
    {
        var model = new PivotTableModel { Culture = CultureInfo.InvariantCulture };
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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
                AggregateType = PivotAggregateType.Sum,
                StringFormat = "N2",
                FormatProvider = CultureInfo.InvariantCulture
            });

            model.Layout.RowLayout = PivotRowLayout.Tabular;
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumn(model, "A");
        var valueDefinitionIndex = model.RowFields.Count + columnA.Index;
        var valueDefinition = model.ColumnDefinitions[valueDefinitionIndex];
        Assert.NotNull(valueDefinition.Options?.SearchTextProvider);
        var text = valueDefinition.Options!.SearchTextProvider!(northRow);
        Assert.Equal("10.00", text);
    }

    [Fact]
    public void ValueFilter_TopCount_LimitsRows()
    {
        var model = new PivotTableModel();
        var amountField = new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        };

        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
            var regionField = new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                ValueFilter = new PivotValueFilter
                {
                    FilterType = PivotValueFilterType.Top,
                    Count = 1,
                    ValueField = amountField
                }
            };

            model.RowFields.Add(regionField);
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((Sale)item!).Product
            });
            model.ValueFields.Add(amountField);

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Single(model.Rows);
        Assert.Equal("South", model.Rows[0].RowPathValues[0]);
    }

    [Fact]
    public void ValueFilter_TopPercent_LimitsRows()
    {
        var model = new PivotTableModel();
        var amountField = new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        };

        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
            var regionField = new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                ValueFilter = new PivotValueFilter
                {
                    FilterType = PivotValueFilterType.TopPercent,
                    Percent = 50,
                    ValueField = amountField
                }
            };

            model.RowFields.Add(regionField);
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((Sale)item!).Product
            });
            model.ValueFields.Add(amountField);

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Single(model.Rows);
        Assert.Equal("South", model.Rows[0].RowPathValues[0]);
    }

    [Fact]
    public void ValueSort_ByAggregateValue_OrdersRows()
    {
        var model = new PivotTableModel();
        var amountField = new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        };

        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
            var regionField = new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                ValueSort = new PivotValueSort
                {
                    ValueField = amountField,
                    SortDirection = ListSortDirection.Descending
                }
            };

            model.RowFields.Add(regionField);
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((Sale)item!).Product
            });
            model.ValueFields.Add(amountField);

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Equal("South", model.Rows[0].RowPathValues[0]);
        Assert.Equal("North", model.Rows[1].RowPathValues[0]);
    }

    [Fact]
    public void ValueSort_Nulls_Last()
    {
        var model = new PivotTableModel();
        var amountField = new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        };

        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
            var regionField = new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                ShowItemsWithNoData = true,
                ItemsSource = new object?[] { "North", "South", "West" },
                ValueSort = new PivotValueSort
                {
                    ValueField = amountField,
                    SortDirection = ListSortDirection.Descending
                }
            };

            model.RowFields.Add(regionField);
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((Sale)item!).Product
            });
            model.ValueFields.Add(amountField);

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Equal(3, model.Rows.Count);
        Assert.Equal("South", model.Rows[0].RowPathValues[0]);
        Assert.Equal("North", model.Rows[1].RowPathValues[0]);
        Assert.Equal("West", model.Rows[2].RowPathValues[0]);
    }

    [Fact]
    public void PercentOfParentRowTotal_UsesParentTotals()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region
            });
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((Sale)item!).Product
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Amount",
                ValueSelector = item => ((Sale)item!).Amount,
                AggregateType = PivotAggregateType.Sum,
                DisplayMode = PivotValueDisplayMode.PercentOfParentRowTotal
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northA = GetRowByPath(model, "North", "A");
        var northB = GetRowByPath(model, "North", "B");

        var column = model.Columns.Single();
        var northAPercent = Convert.ToDouble(northA.CellValues[column.Index]!);
        var northBPercent = Convert.ToDouble(northB.CellValues[column.Index]!);

        Assert.InRange(northAPercent, 0.3333, 0.3334);
        Assert.InRange(northBPercent, 0.6666, 0.6667);
    }

    [Fact]
    public void PercentOfParentRowTotal_SingleAxis_UsesGrandParentTotals()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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
                AggregateType = PivotAggregateType.Sum,
                DisplayMode = PivotValueDisplayMode.PercentOfParentRowTotal
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var southRow = GetRow(model, "South");
        var columnA = GetColumn(model, "A");
        var columnB = GetColumn(model, "B");

        Assert.InRange(Convert.ToDouble(northRow.CellValues[columnA.Index]!), 0.2499, 0.2501);
        Assert.InRange(Convert.ToDouble(northRow.CellValues[columnB.Index]!), 0.3333, 0.3334);
        Assert.InRange(Convert.ToDouble(southRow.CellValues[columnA.Index]!), 0.7499, 0.7501);
        Assert.InRange(Convert.ToDouble(southRow.CellValues[columnB.Index]!), 0.6666, 0.6667);
    }

    [Fact]
    public void DifferenceFromPrevious_UsesColumnOrder()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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
                AggregateType = PivotAggregateType.Sum,
                DisplayMode = PivotValueDisplayMode.DifferenceFromPrevious
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumn(model, "A");
        var columnB = GetColumn(model, "B");

        Assert.Null(northRow.CellValues[columnA.Index]);
        Assert.Equal(10d, Convert.ToDouble(northRow.CellValues[columnB.Index]!));
    }

    [Fact]
    public void RunningTotal_AccumulatesAcrossColumns()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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
                AggregateType = PivotAggregateType.Sum,
                DisplayMode = PivotValueDisplayMode.RunningTotal
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumn(model, "A");
        var columnB = GetColumn(model, "B");

        Assert.Equal(10d, Convert.ToDouble(northRow.CellValues[columnA.Index]!));
        Assert.Equal(30d, Convert.ToDouble(northRow.CellValues[columnB.Index]!));
    }

    [Fact]
    public void Index_ComputesExpectedValue()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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
                AggregateType = PivotAggregateType.Sum,
                DisplayMode = PivotValueDisplayMode.Index
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumn(model, "A");
        var indexValue = Convert.ToDouble(northRow.CellValues[columnA.Index]!);

        Assert.InRange(indexValue, 0.8333, 0.8334);
    }

    [Fact]
    public void CalculatedField_UsesGrandTotalFormula()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales.ToList();
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
            model.ValueFields.Add(new PivotValueField
            {
                Header = "PctOfTotal",
                Formula = "Amount / GrandTotal(Amount)"
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumnByValueField(model, "A", "PctOfTotal");
        var pctOfTotal = Convert.ToDouble(northRow.CellValues[columnA.Index]!);

        Assert.InRange(pctOfTotal, 0.0999, 0.1001);
    }

    [Fact]
    public void ValueSort_UsesCalculatedField()
    {
        var data = new[]
        {
            new Sale { Region = "A", Product = "X", Amount = 10, Units = 1 },
            new Sale { Region = "B", Product = "X", Amount = 10, Units = 5 }
        };

        var model = new PivotTableModel();
        var amountField = new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        };
        var unitsField = new PivotValueField
        {
            Header = "Units",
            ValueSelector = item => ((Sale)item!).Units,
            AggregateType = PivotAggregateType.Sum
        };
        var rateField = new PivotValueField
        {
            Header = "Rate",
            Formula = "Amount / Units"
        };

        using (model.DeferRefresh())
        {
            model.ItemsSource = data;
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                ValueSort = new PivotValueSort
                {
                    ValueField = rateField,
                    SortDirection = ListSortDirection.Descending
                }
            });

            model.ValueFields.Add(amountField);
            model.ValueFields.Add(unitsField);
            model.ValueFields.Add(rateField);

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Equal("A", model.Rows[0].RowPathValues[0]);
        Assert.Equal("B", model.Rows[1].RowPathValues[0]);
    }

    [Fact]
    public void ValueFilter_UsesCalculatedField()
    {
        var data = new[]
        {
            new Sale { Region = "A", Product = "X", Amount = 10, Units = 1 },
            new Sale { Region = "B", Product = "X", Amount = 10, Units = 5 }
        };

        var model = new PivotTableModel();
        var amountField = new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        };
        var unitsField = new PivotValueField
        {
            Header = "Units",
            ValueSelector = item => ((Sale)item!).Units,
            AggregateType = PivotAggregateType.Sum
        };
        var rateField = new PivotValueField
        {
            Header = "Rate",
            Formula = "Amount / Units"
        };

        using (model.DeferRefresh())
        {
            model.ItemsSource = data;
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                ValueFilter = new PivotValueFilter
                {
                    FilterType = PivotValueFilterType.Top,
                    Count = 1,
                    ValueField = rateField
                }
            });

            model.ValueFields.Add(amountField);
            model.ValueFields.Add(unitsField);
            model.ValueFields.Add(rateField);

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Single(model.Rows);
        Assert.Equal("A", model.Rows[0].RowPathValues[0]);
    }

    private static PivotRow GetRow(PivotTableModel model, string? region)
    {
        return model.Rows.Single(row => region == null
            ? row.RowType == PivotRowType.GrandTotal
            : row.RowType == PivotRowType.Detail && row.RowPathValues.Length == 1 && Equals(row.RowPathValues[0], region));
    }

    private static PivotRow GetRowByPath(PivotTableModel model, params string[] path)
    {
        return model.Rows.Single(row => row.RowType == PivotRowType.Detail &&
            row.RowPathValues.Length == path.Length &&
            row.RowPathValues.Zip(path, (value, segment) => Equals(value, segment)).All(match => match));
    }

    private static PivotColumn GetColumn(PivotTableModel model, string? product)
    {
        return model.Columns.Single(column => product == null
            ? column.ColumnType == PivotColumnType.GrandTotal
            : column.ColumnType == PivotColumnType.Detail && column.ColumnPathValues.Length == 1 && Equals(column.ColumnPathValues[0], product));
    }

    private static PivotColumn GetColumnByValueField(PivotTableModel model, string product, string valueHeader)
    {
        return model.Columns.Single(column => column.ColumnType == PivotColumnType.Detail &&
            column.ColumnPathValues.Length == 1 &&
            Equals(column.ColumnPathValues[0], product) &&
            string.Equals(column.ValueField?.Header, valueHeader, StringComparison.Ordinal));
    }
}
