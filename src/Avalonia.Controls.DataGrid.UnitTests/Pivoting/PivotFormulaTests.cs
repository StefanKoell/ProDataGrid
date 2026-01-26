using System;
using System.Globalization;
using System.Linq;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotFormulaTests
{
    private sealed class Sale
    {
        public string Region { get; init; } = string.Empty;
        public string Product { get; init; } = string.Empty;
        public int Year { get; init; }
        public string Quarter { get; init; } = string.Empty;
        public double Amount { get; init; }
        public int Units { get; init; }
    }

    private static readonly Sale[] SampleSales =
    {
        new() { Region = "North", Product = "A", Amount = 10, Units = 2 },
        new() { Region = "North", Product = "B", Amount = 20, Units = 4 },
        new() { Region = "South", Product = "A", Amount = 30, Units = 6 }
    };

    [Fact]
    public void CalculatedFields_Use_Functions_And_Bracketed_Identifiers()
    {
        var model = new PivotTableModel { Culture = CultureInfo.InvariantCulture };
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
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Units",
                ValueSelector = item => ((Sale)item!).Units,
                AggregateType = PivotAggregateType.Sum
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "RowShare",
                Formula = "Amount / RowTotal(Amount)"
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "ColumnShare",
                Formula = "Amount / ColumnTotal(Amount)"
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "GrandShare",
                Formula = "Amount / GrandTotal(Amount)"
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Bracketed",
                Formula = "[Amount] * 2"
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Unary",
                Formula = "-Amount"
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "ConstantAdd",
                Formula = "Amount + 1.5e2"
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northRow = GetRow(model, "North");
        var columnA = GetColumnByValueHeader(model, "A", "RowShare");
        var columnB = GetColumnByValueHeader(model, "A", "ColumnShare");
        var columnC = GetColumnByValueHeader(model, "A", "GrandShare");
        var columnD = GetColumnByValueHeader(model, "A", "Bracketed");
        var columnE = GetColumnByValueHeader(model, "A", "Unary");
        var columnF = GetColumnByValueHeader(model, "A", "ConstantAdd");

        Assert.InRange(Convert.ToDouble(northRow.CellValues[columnA.Index]!), 0.3333, 0.3334);
        Assert.InRange(Convert.ToDouble(northRow.CellValues[columnB.Index]!), 0.2499, 0.2501);
        Assert.InRange(Convert.ToDouble(northRow.CellValues[columnC.Index]!), 0.1666, 0.1667);
        Assert.Equal(20d, Convert.ToDouble(northRow.CellValues[columnD.Index]!));
        Assert.Equal(-10d, Convert.ToDouble(northRow.CellValues[columnE.Index]!));
        Assert.Equal(160d, Convert.ToDouble(northRow.CellValues[columnF.Index]!));
    }

    [Fact]
    public void CalculatedFields_Use_Parent_Totals()
    {
        var data = new[]
        {
            new Sale { Region = "North", Product = "A", Amount = 10 },
            new Sale { Region = "North", Product = "B", Amount = 20 }
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
            model.ValueFields.Add(new PivotValueField
            {
                Header = "ParentRowShare",
                Formula = "Amount / ParentRowTotal(Amount)"
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var northA = GetRowByPath(model, "North", "A");
        var column = GetColumnByValueHeader(model, null, "ParentRowShare");

        Assert.InRange(Convert.ToDouble(northA.CellValues[column.Index]!), 0.3333, 0.3334);
    }

    [Fact]
    public void CalculatedFields_Use_Parent_Column_Totals()
    {
        var data = new[]
        {
            new Sale { Region = "North", Year = 2022, Quarter = "Q1", Amount = 10 },
            new Sale { Region = "North", Year = 2022, Quarter = "Q2", Amount = 20 }
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
                Header = "Year",
                ValueSelector = item => ((Sale)item!).Year
            });
            model.ColumnFields.Add(new PivotAxisField
            {
                Header = "Quarter",
                ValueSelector = item => ((Sale)item!).Quarter
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Amount",
                ValueSelector = item => ((Sale)item!).Amount,
                AggregateType = PivotAggregateType.Sum
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "ParentColumnShare",
                Formula = "Amount / ParentColumnTotal(Amount)"
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var row = GetRow(model, "North");
        var column = model.Columns.Single(col => col.ValueField?.Header == "ParentColumnShare" &&
            col.ColumnPathValues.Length == 2 && Equals(col.ColumnPathValues[1], "Q1"));

        Assert.InRange(Convert.ToDouble(row.CellValues[column.Index]!), 0.3333, 0.3334);
    }

    [Fact]
    public void Invalid_And_Self_Referential_Formulas_Return_Null()
    {
        var model = new PivotTableModel { Culture = CultureInfo.InvariantCulture };
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
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Bad",
                Formula = "Unknown(Amount)"
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Broken",
                Formula = "Amount +"
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "ZeroDiv",
                Formula = "Amount / 0"
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Self",
                Formula = "Self + 1"
            });

            model.Layout.ShowRowGrandTotals = false;
            model.Layout.ShowColumnGrandTotals = false;
        }

        model.Refresh();

        var row = GetRow(model, "North");
        var bad = GetColumnByValueHeader(model, "A", "Bad");
        var broken = GetColumnByValueHeader(model, "A", "Broken");
        var zero = GetColumnByValueHeader(model, "A", "ZeroDiv");
        var self = GetColumnByValueHeader(model, "A", "Self");

        Assert.Null(row.CellValues[bad.Index]);
        Assert.Null(row.CellValues[broken.Index]);
        Assert.Null(row.CellValues[zero.Index]);
        Assert.Null(row.CellValues[self.Index]);
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

    private static PivotColumn GetColumnByValueHeader(PivotTableModel model, string? product, string valueHeader)
    {
        return model.Columns.Single(column =>
            column.ValueField?.Header == valueHeader &&
            (product == null || (column.ColumnPathValues.Length == 1 && Equals(column.ColumnPathValues[0], product))));
    }
}
