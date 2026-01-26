using System.Collections.Generic;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotValueFiltersTests
{
    [Fact]
    public void PivotValueFilter_Raises_Changed_On_Update()
    {
        var filter = new PivotValueFilter();
        var changes = 0;
        filter.Changed += (_, _) => changes++;

        filter.FilterType = PivotValueFilterType.Top;
        filter.ValueField = new PivotValueField { Header = "Amount" };
        filter.Value = 10d;
        filter.Value2 = 20d;
        filter.Count = 2;
        filter.Percent = 50d;

        Assert.Equal(6, changes);
    }

    [Fact]
    public void PivotValueFilter_Does_Not_Raise_On_Same_Value()
    {
        var filter = new PivotValueFilter { Count = 2 };
        var changes = 0;
        filter.Changed += (_, _) => changes++;

        filter.Count = 2;

        Assert.Equal(0, changes);
    }

    [Fact]
    public void PivotValueSort_Raises_Changed_On_Update()
    {
        var sort = new PivotValueSort();
        var changes = 0;
        sort.Changed += (_, _) => changes++;

        sort.ValueField = new PivotValueField { Header = "Amount" };
        sort.SortDirection = System.ComponentModel.ListSortDirection.Ascending;

        Assert.Equal(2, changes);
    }

    [Fact]
    public void PivotValueSort_Does_Not_Raise_On_Same_Value()
    {
        var sort = new PivotValueSort();
        var changes = 0;
        sort.Changed += (_, _) => changes++;

        sort.SortDirection = sort.SortDirection;

        Assert.Equal(0, changes);
    }
}
