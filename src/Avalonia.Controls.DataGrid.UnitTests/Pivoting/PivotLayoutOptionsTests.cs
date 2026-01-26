using System.Collections.Generic;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotLayoutOptionsTests
{
    [Fact]
    public void Properties_Raise_PropertyChanged_When_Updated()
    {
        var options = new PivotLayoutOptions();
        var changed = new List<string?>();
        options.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        options.RowLayout = PivotRowLayout.Tabular;
        options.ValuesPosition = PivotValuesPosition.Rows;
        options.ShowRowSubtotals = false;
        options.ShowColumnSubtotals = false;
        options.ShowRowGrandTotals = false;
        options.ShowColumnGrandTotals = false;
        options.RowGrandTotalPosition = PivotTotalPosition.Start;
        options.ColumnGrandTotalPosition = PivotTotalPosition.Start;
        options.RepeatRowLabels = true;
        options.RowHeaderLabel = "Rows";
        options.GrandTotalLabel = "Total";
        options.SubtotalLabelFormat = "{0} Sub";
        options.ValuesHeaderLabel = "Vals";
        options.CompactIndentSize = 24d;
        options.EmptyValueLabel = "(empty)";

        Assert.Contains(nameof(PivotLayoutOptions.RowLayout), changed);
        Assert.Contains(nameof(PivotLayoutOptions.ValuesPosition), changed);
        Assert.Contains(nameof(PivotLayoutOptions.ShowRowSubtotals), changed);
        Assert.Contains(nameof(PivotLayoutOptions.ShowColumnSubtotals), changed);
        Assert.Contains(nameof(PivotLayoutOptions.ShowRowGrandTotals), changed);
        Assert.Contains(nameof(PivotLayoutOptions.ShowColumnGrandTotals), changed);
        Assert.Contains(nameof(PivotLayoutOptions.RowGrandTotalPosition), changed);
        Assert.Contains(nameof(PivotLayoutOptions.ColumnGrandTotalPosition), changed);
        Assert.Contains(nameof(PivotLayoutOptions.RepeatRowLabels), changed);
        Assert.Contains(nameof(PivotLayoutOptions.RowHeaderLabel), changed);
        Assert.Contains(nameof(PivotLayoutOptions.GrandTotalLabel), changed);
        Assert.Contains(nameof(PivotLayoutOptions.SubtotalLabelFormat), changed);
        Assert.Contains(nameof(PivotLayoutOptions.ValuesHeaderLabel), changed);
        Assert.Contains(nameof(PivotLayoutOptions.CompactIndentSize), changed);
        Assert.Contains(nameof(PivotLayoutOptions.EmptyValueLabel), changed);
    }

    [Fact]
    public void Properties_Do_Not_Raise_When_Value_Unchanged()
    {
        var options = new PivotLayoutOptions();
        var raised = false;
        options.PropertyChanged += (_, _) => raised = true;

        options.RowLayout = options.RowLayout;

        Assert.False(raised);
    }
}
