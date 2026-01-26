using System;
using System.Collections.Generic;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotFieldFilterTests
{
    [Fact]
    public void PivotFieldFilter_Uses_Included_Values()
    {
        var filter = new PivotFieldFilter(included: new object?[] { "A" });

        Assert.True(filter.IsMatch("A"));
        Assert.False(filter.IsMatch("B"));
    }

    [Fact]
    public void PivotFieldFilter_Uses_Excluded_Values_When_No_Includes()
    {
        var filter = new PivotFieldFilter(excluded: new object?[] { "A" });

        Assert.False(filter.IsMatch("A"));
        Assert.True(filter.IsMatch("B"));
    }

    [Fact]
    public void PivotFieldFilter_Uses_Predicate_When_Set()
    {
        var filter = new PivotFieldFilter(included: new object?[] { "A" })
        {
            Predicate = _ => false
        };

        Assert.False(filter.IsMatch("A"));
    }

    [Fact]
    public void PivotFieldFilter_Raises_Changed_When_Modified()
    {
        var filter = new PivotFieldFilter();
        var changes = 0;
        filter.Changed += (_, _) => changes++;

        filter.Included.Add("A");
        filter.Excluded.Add("B");
        filter.Included.Remove("A");
        filter.Excluded.Clear();

        Assert.Equal(4, changes);
    }

    [Fact]
    public void PivotFieldFilter_Sets_Respond_To_Set_Operations()
    {
        var filter = new PivotFieldFilter(included: new object?[] { "A", "B" });

        Assert.True(filter.Included.Contains("A"));

        var buffer = new object?[2];
        filter.Included.CopyTo(buffer, 0);

        filter.Included.ExceptWith(new[] { "B" });
        filter.Included.IntersectWith(new[] { "A", "C" });
        filter.Included.SymmetricExceptWith(new[] { "A", "D" });
        filter.Included.UnionWith(new[] { "E" });

        Assert.True(filter.Included.Overlaps(new[] { "D" }));
        Assert.True(filter.Included.IsSubsetOf(new[] { "D", "E" }));
        Assert.True(filter.Included.IsSupersetOf(new[] { "D" }));
        Assert.True(filter.Included.IsProperSupersetOf(new[] { "D" }));
        Assert.True(filter.Included.IsProperSubsetOf(new[] { "D", "E", "F" }));
        Assert.True(filter.Included.SetEquals(new[] { "D", "E" }));
    }

    [Fact]
    public void PivotFieldFilter_Set_Operations_Handle_Null_Collections()
    {
        var filter = new PivotFieldFilter();

        filter.Included.ExceptWith(null!);
        filter.Included.IntersectWith(null!);
        filter.Included.SymmetricExceptWith(null!);
        filter.Included.UnionWith(null!);
    }
}
