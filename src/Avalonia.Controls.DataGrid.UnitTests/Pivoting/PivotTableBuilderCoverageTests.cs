using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.DataGridPivoting;
using Avalonia.Data.Converters;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotTableBuilderCoverageTests
{
    private sealed class Sale
    {
        public string Region { get; init; } = string.Empty;
        public string Product { get; init; } = string.Empty;
        public string Quarter { get; init; } = string.Empty;
        public int Year { get; init; }
        public double Amount { get; init; }
        public string Note { get; init; } = string.Empty;
    }

    private sealed class NoteConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return string.Concat("note:", value?.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private static readonly List<Sale> SampleSales = new()
    {
        new Sale { Region = "North", Product = "A", Amount = 10 },
        new Sale { Region = "North", Product = "B", Amount = 20 },
        new Sale { Region = "South", Product = "A", Amount = 30 },
        new Sale { Region = "South", Product = "B", Amount = 40 }
    };

    [Fact]
    public void PercentOfColumnTotal_Computes()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales;
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
                DisplayMode = PivotValueDisplayMode.PercentOfColumnTotal
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumn(model, "A");

        Assert.InRange(Convert.ToDouble(northRow.CellValues[columnA.Index]!), 0.2499, 0.2501);
    }

    [Fact]
    public void PercentOfGrandTotal_Computes()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales;
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
                DisplayMode = PivotValueDisplayMode.PercentOfGrandTotal
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumn(model, "A");

        Assert.InRange(Convert.ToDouble(northRow.CellValues[columnA.Index]!), 0.0999, 0.1001);
    }

    [Fact]
    public void Build_Skips_Null_Items()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = new object?[]
            {
                null,
                new Sale { Region = "North", Product = "A", Amount = 10 }
            };
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

        Assert.Contains(model.Rows, row => row.RowPathValues.Contains("North"));
    }

    [Fact]
    public void PercentOfParentColumnTotal_Uses_Parent_Totals()
    {
        var data = new List<Sale>
        {
            new() { Region = "North", Year = 2022, Quarter = "Q1", Amount = 10 },
            new() { Region = "North", Year = 2022, Quarter = "Q2", Amount = 20 },
            new() { Region = "North", Year = 2023, Quarter = "Q1", Amount = 30 },
            new() { Region = "North", Year = 2023, Quarter = "Q2", Amount = 40 }
        };

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
                Header = "Year",
                ValueSelector = item => ((Sale)item!).Year,
                SortDirection = ListSortDirection.Ascending
            });
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Quarter",
                ValueSelector = item => ((Sale)item!).Quarter,
                SortDirection = ListSortDirection.Ascending
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Amount",
                ValueSelector = item => ((Sale)item!).Amount,
                AggregateType = PivotAggregateType.Sum,
                DisplayMode = PivotValueDisplayMode.PercentOfParentColumnTotal
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = true;
        }

        model.Refresh();

        var row = GetRow(model, "North");
        var columnQ1 = model.Columns.Single(column =>
            column.ColumnType == PivotColumnType.Detail &&
            column.ColumnPathValues.Length == 2 &&
            Equals(column.ColumnPathValues[0], 2022) &&
            Equals(column.ColumnPathValues[1], "Q1"));

        Assert.InRange(Convert.ToDouble(row.CellValues[columnQ1.Index]!), 0.3333, 0.3334);

        var grandTotalColumn = model.Columns.Single(column => column.ColumnType == PivotColumnType.GrandTotal);
        Assert.Null(row.CellValues[grandTotalColumn.Index]);
    }

    [Fact]
    public void PercentDifferenceFromPrevious_Computes()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales;
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
                DisplayMode = PivotValueDisplayMode.PercentDifferenceFromPrevious
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumn(model, "A");
        var columnB = GetColumn(model, "B");

        Assert.Null(northRow.CellValues[columnA.Index]);
        Assert.InRange(Convert.ToDouble(northRow.CellValues[columnB.Index]!), 0.99, 1.01);
    }

    [Fact]
    public void GrandTotals_Can_Be_Inserted_At_Start()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales;
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

            model.Layout.RowGrandTotalPosition = PivotTotalPosition.Start;
            model.Layout.ColumnGrandTotalPosition = PivotTotalPosition.Start;
        }

        model.Refresh();

        Assert.Equal(PivotRowType.GrandTotal, model.Rows[0].RowType);
        Assert.Equal(PivotColumnType.GrandTotal, model.Columns[0].ColumnType);
    }

    [Fact]
    public void RepeatRowLabels_Hides_Duplicates_When_Disabled()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 1 },
            new Sale { Region = "North", Product = "B", Amount = 2 }
        };

        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = data;
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
                AggregateType = PivotAggregateType.Sum
            });

            model.Layout.RowLayout = PivotRowLayout.Tabular;
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var firstRow = model.Rows[0];
        var secondRow = model.Rows[1];

        Assert.Equal("North", firstRow.RowDisplayValues[0]);
        Assert.Null(secondRow.RowDisplayValues[0]);

        model.Layout.RepeatRowLabels = true;
        model.Refresh();

        Assert.Equal("North", model.Rows[1].RowDisplayValues[0]);
    }

    [Fact]
    public void CompactLayout_Builds_CompactLabel_For_ValuesInRows()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales;
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

            model.Layout.ValuesPosition = PivotValuesPosition.Rows;
            model.Layout.RowLayout = PivotRowLayout.Compact;
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Contains(model.Rows, row => row.CompactLabel != null && row.CompactLabel.Contains("Amount"));
    }

    [Fact]
    public void ColumnDefinitions_Use_Text_For_NonNumeric_Fields()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 1, Note = "n" }
        };

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
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Note",
                ValueSelector = item => ((Sale)item!).Note,
                AggregateType = PivotAggregateType.First,
                ValueType = typeof(string)
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        Assert.Contains(model.ColumnDefinitions, def => def is DataGridTextColumnDefinition &&
            def.ColumnKey is PivotColumn column && column.ValueField?.Header == "Note");
        Assert.Contains(model.ColumnDefinitions, def => def is DataGridNumericColumnDefinition &&
            def.ColumnKey is PivotColumn column && column.ValueField?.Header == "Amount");
    }

    [Fact]
    public void Invalid_Subtotal_Format_Falls_Back()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 1 },
            new Sale { Region = "North", Product = "B", Amount = 2 }
        };

        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = data;
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
            model.Layout.SubtotalLabelFormat = "{0";
        }

        model.Refresh();

        Assert.Contains(model.Rows, row => row.RowType == PivotRowType.Subtotal &&
            row.RowDisplayValues.Length > 0 &&
            row.RowDisplayValues[0]?.ToString() == "North Total");
    }

    [Fact]
    public void NoColumnFields_Uses_GrandTotal_Header()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 1 }
        };

        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
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

            model.Layout.ValuesPosition = PivotValuesPosition.Rows;
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var header = model.Columns[0].Header;
        Assert.Contains(model.Layout.GrandTotalLabel, header.Segments);
    }

    [Fact]
    public void ValuesInRows_Adds_Value_Field_Column_Definition()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 1 }
        };

        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
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

            model.Layout.ValuesPosition = PivotValuesPosition.Rows;
            model.Layout.RowLayout = PivotRowLayout.Tabular;
        }

        model.Refresh();

        Assert.Contains(model.ColumnDefinitions, def => def.Header?.ToString() == model.Layout.ValuesHeaderLabel);
    }

    [Fact]
    public void Text_Value_Columns_Use_Empty_Label_For_Nulls()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Note = "x" }
        };

        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = data;
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Region",
                ValueSelector = item => ((Sale)item!).Region,
                ShowItemsWithNoData = true,
                ItemsSource = new object?[] { "North", "South" }
            });
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Product",
                ValueSelector = item => ((Sale)item!).Product,
                ShowItemsWithNoData = true,
                ItemsSource = new object?[] { "A", "B" }
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Note",
                ValueSelector = item => ((Sale)item!).Note,
                AggregateType = PivotAggregateType.First,
                ValueType = typeof(string),
                Converter = new NoteConverter()
            });

            model.Layout.EmptyValueLabel = "empty";
            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var row = GetRow(model, "South");
        var column = GetColumn(model, "B");
        var columnDefinition = model.ColumnDefinitions[model.RowFields.Count + column.Index];
        var text = columnDefinition.Options!.SearchTextProvider!(row);

        Assert.Equal("empty", text);
    }

    [Fact]
    public void ColumnBinding_Uses_Format_And_TargetNullValue()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Note = "x" }
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
                Header = "Note",
                ValueSelector = item => ((Sale)item!).Note,
                AggregateType = PivotAggregateType.First,
                ValueType = typeof(string),
                StringFormat = "X{0}",
                NullLabel = "NULL",
                Converter = new NoteConverter(),
                ConverterParameter = "p",
                FormatProvider = CultureInfo.InvariantCulture
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var column = GetColumn(model, "A");
        var definition = (DataGridTextColumnDefinition)model.ColumnDefinitions[model.RowFields.Count + column.Index];
        var binding = definition.Binding;

        Assert.Equal("X{0}", binding.StringFormat);
        Assert.Equal("NULL", binding.TargetNullValue);
        Assert.Equal(CultureInfo.InvariantCulture, binding.ConverterCulture);
        Assert.Equal("p", binding.ConverterParameter);
    }

    [Fact]
    public void ValueFilter_Comparison_Types_Are_Applied()
    {
        var model = new PivotTableModel();
        var amountField = new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        };

        var rowField = new PivotAxisField
        {
            Header = "Region",
            ValueSelector = item => ((Sale)item!).Region,
            ValueFilter = new PivotValueFilter
            {
                FilterType = PivotValueFilterType.GreaterThan,
                Value = 50,
                ValueField = amountField
            }
        };

        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales;
            model.RowFields.Add(rowField);
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

        rowField.ValueFilter!.FilterType = PivotValueFilterType.LessThanOrEqual;
        rowField.ValueFilter.Value = 30;
        model.Refresh();
        Assert.Single(model.Rows);

        rowField.ValueFilter.FilterType = PivotValueFilterType.GreaterThanOrEqual;
        rowField.ValueFilter.Value = 70;
        model.Refresh();
        Assert.Single(model.Rows);

        rowField.ValueFilter.FilterType = PivotValueFilterType.LessThan;
        rowField.ValueFilter.Value = 31;
        model.Refresh();
        Assert.Single(model.Rows);

        rowField.ValueFilter.FilterType = PivotValueFilterType.Equal;
        rowField.ValueFilter.Value = 30;
        model.Refresh();
        Assert.Single(model.Rows);

        rowField.ValueFilter.FilterType = PivotValueFilterType.NotEqual;
        rowField.ValueFilter.Value = 30;
        model.Refresh();
        Assert.Single(model.Rows);

        rowField.ValueFilter.FilterType = PivotValueFilterType.Between;
        rowField.ValueFilter.Value = 20;
        rowField.ValueFilter.Value2 = 60;
        model.Refresh();
        Assert.Single(model.Rows);
    }

    [Fact]
    public void ValueFilter_TopBottom_Percent_Branches_Are_Used()
    {
        var model = new PivotTableModel();
        var amountField = new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
        };

        var rowField = new PivotAxisField
        {
            Header = "Region",
            ValueSelector = item => ((Sale)item!).Region,
            ValueFilter = new PivotValueFilter
            {
                FilterType = PivotValueFilterType.TopPercent,
                Percent = 0,
                ValueField = amountField
            }
        };

        using (model.DeferRefresh())
        {
            model.ItemsSource = SampleSales;
            model.RowFields.Add(rowField);
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
        Assert.Empty(model.Rows);

        rowField.ValueFilter!.FilterType = PivotValueFilterType.Bottom;
        rowField.ValueFilter.Count = 1;
        model.Refresh();
        Assert.Single(model.Rows);

        rowField.ValueFilter.FilterType = PivotValueFilterType.BottomPercent;
        rowField.ValueFilter.Percent = 50;
        model.Refresh();
        Assert.Single(model.Rows);

        rowField.ValueFilter.FilterType = PivotValueFilterType.Top;
        rowField.ValueFilter.Count = 0;
        rowField.ValueFilter.Percent = 50;
        model.Refresh();
        Assert.Single(model.Rows);
    }

    [Fact]
    public void ValueSort_Falls_Back_When_Field_Not_Found()
    {
        var data = new[]
        {
            new Sale { Region = "B", Product = "A", Amount = 1 },
            new Sale { Region = "A", Product = "A", Amount = 2 }
        };

        var model = new PivotTableModel();
        var amountField = new PivotValueField
        {
            Header = "Amount",
            ValueSelector = item => ((Sale)item!).Amount,
            AggregateType = PivotAggregateType.Sum
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
                    ValueField = new PivotValueField { Key = "Missing" },
                    SortDirection = ListSortDirection.Descending
                }
            });
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

        Assert.Equal("A", model.Rows[0].RowPathValues[0]);
        Assert.Equal("B", model.Rows[1].RowPathValues[0]);
    }

    private static PivotRow GetRow(PivotTableModel model, string? region)
    {
        return model.Rows.Single(row => region == null
            ? row.RowType == PivotRowType.GrandTotal
            : row.RowType == PivotRowType.Detail && row.RowPathValues.Length == 1 && Equals(row.RowPathValues[0], region));
    }

    private static PivotColumn GetColumn(PivotTableModel model, string? product)
    {
        return model.Columns.Single(column => product == null
            ? column.ColumnType == PivotColumnType.GrandTotal
            : column.ColumnType == PivotColumnType.Detail && column.ColumnPathValues.Length == 1 && Equals(column.ColumnPathValues[0], product));
    }
}
