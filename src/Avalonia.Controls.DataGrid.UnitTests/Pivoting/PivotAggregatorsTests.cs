using System;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotAggregatorsTests
{
    [Fact]
    public void PivotNumeric_TryGetDouble_Handles_Invalid_Values()
    {
        Assert.False(PivotNumeric.TryGetDouble(null, out _));
        Assert.False(PivotNumeric.TryGetDouble("not-number", out _));
        Assert.False(PivotNumeric.TryGetDouble(new object(), out _));
        Assert.False(PivotNumeric.TryGetDouble(double.NaN, out _));
        Assert.False(PivotNumeric.TryGetDouble(double.PositiveInfinity, out _));

        Assert.True(PivotNumeric.TryGetDouble(5, out var number));
        Assert.Equal(5d, number);
    }

    [Fact]
    public void AggregatorRegistry_Registers_And_Gets()
    {
        var registry = new PivotAggregatorRegistry();

        Assert.NotNull(registry.Get(PivotAggregateType.Sum));
        Assert.Throws<ArgumentNullException>(() => registry.Register(null!));
    }

    [Fact]
    public void SumAggregator_Adds_And_Merges()
    {
        var agg = new SumAggregator();
        var variance = new VarianceAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        left.Add(1d);
        left.Add("skip");
        right.Add(2d);

        left.Merge(right);

        var another = variance.CreateState();
        another.Add(10d);
        left.Merge(another);

        Assert.Equal(3d, left.GetResult());

        var empty = agg.CreateState();
        Assert.Null(empty.GetResult());
    }

    [Fact]
    public void CountAggregator_Counts_Non_Null_Values()
    {
        var agg = new CountAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        left.Add(1);
        left.Add(null);
        right.Add("x");

        left.Merge(right);

        Assert.Equal(2, left.GetResult());
    }

    [Fact]
    public void CountNumbersAggregator_Counts_Numeric_Values()
    {
        var agg = new CountNumbersAggregator();
        var state = agg.CreateState();

        state.Add(1);
        state.Add("2");
        state.Add("bad");

        Assert.Equal(2, state.GetResult());
    }

    [Fact]
    public void AverageAggregator_Computes_Mean_And_Merges()
    {
        var agg = new AverageAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        left.Add(2d);
        left.Add(4d);
        right.Add(6d);

        left.Merge(right);

        Assert.Equal(4d, left.GetResult());

        var empty = agg.CreateState();
        Assert.Null(empty.GetResult());
    }

    [Fact]
    public void ProductAggregator_Multiplies_And_Merges()
    {
        var agg = new ProductAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        left.Add(2d);
        right.Add(3d);

        left.Merge(right);

        Assert.Equal(6d, left.GetResult());

        var empty = agg.CreateState();
        left.Merge(empty);
        Assert.Equal(6d, left.GetResult());
    }

    [Fact]
    public void MinMaxAggregators_Select_Extremes_And_Merge()
    {
        var minAgg = new MinAggregator();
        var maxAgg = new MaxAggregator();
        var minLeft = minAgg.CreateState();
        var minRight = minAgg.CreateState();
        var maxLeft = maxAgg.CreateState();
        var maxRight = maxAgg.CreateState();

        minLeft.Add(5);
        minLeft.Add(3);
        minRight.Add(4);
        minLeft.Merge(minRight);

        maxLeft.Add(1);
        maxLeft.Add(7);
        maxRight.Add(2);
        maxLeft.Merge(maxRight);

        Assert.Equal(3, minLeft.GetResult());
        Assert.Equal(7, maxLeft.GetResult());
    }

    [Fact]
    public void CountDistinctAggregator_Counts_Unique_Values()
    {
        var agg = new CountDistinctAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        left.Add(1);
        left.Add(1);
        left.Add(null);
        right.Add(2);

        left.Merge(right);

        Assert.Equal(2, left.GetResult());
    }

    [Fact]
    public void VarianceAggregators_Compute_Variance()
    {
        var variance = new VarianceAggregator();
        var varianceP = new VariancePAggregator();
        var sampleState = variance.CreateState();
        var popState = varianceP.CreateState();

        foreach (var value in new[] { 1d, 2d, 3d })
        {
            sampleState.Add(value);
            popState.Add(value);
        }

        Assert.InRange((double)sampleState.GetResult()!, 0.99, 1.01);
        Assert.InRange((double)popState.GetResult()!, 0.65, 0.68);
    }

    [Fact]
    public void VarianceState_Merges_Empty_And_Populated_States()
    {
        var agg = new VarianceAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        right.Add(2d);
        right.Add(4d);
        left.Merge(right);

        Assert.InRange((double)left.GetResult()!, 2.0, 2.1);
    }

    [Fact]
    public void StdDevAggregators_Compute_StdDev()
    {
        var std = new StdDevAggregator();
        var stdP = new StdDevPAggregator();
        var sampleState = std.CreateState();
        var popState = stdP.CreateState();

        foreach (var value in new[] { 1d, 2d, 3d })
        {
            sampleState.Add(value);
            popState.Add(value);
        }

        Assert.InRange((double)sampleState.GetResult()!, 0.99, 1.01);
        Assert.InRange((double)popState.GetResult()!, 0.81, 0.83);
    }

    [Fact]
    public void FirstAggregator_Returns_First_NonNull()
    {
        var agg = new FirstAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        left.Add(null);
        left.Add("first");
        left.Add("second");

        right.Add("other");
        left.Merge(right);

        Assert.Equal("first", left.GetResult());

        var empty = agg.CreateState();
        empty.Merge(left);
        Assert.Equal("first", empty.GetResult());
    }

    [Fact]
    public void LastAggregator_Returns_Last_NonNull()
    {
        var agg = new LastAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        left.Add("a");
        left.Add("b");
        right.Add("c");

        left.Merge(right);

        Assert.Equal("c", left.GetResult());
    }

    [Fact]
    public void Aggregator_Names_Are_Stable()
    {
        Assert.Equal("Sum", new SumAggregator().Name);
        Assert.Equal("Count", new CountAggregator().Name);
        Assert.Equal("Count Numbers", new CountNumbersAggregator().Name);
        Assert.Equal("Average", new AverageAggregator().Name);
        Assert.Equal("Product", new ProductAggregator().Name);
        Assert.Equal("Min", new MinAggregator().Name);
        Assert.Equal("Max", new MaxAggregator().Name);
        Assert.Equal("Distinct Count", new CountDistinctAggregator().Name);
        Assert.Equal("Variance", new VarianceAggregator().Name);
        Assert.Equal("Variance (Population)", new VariancePAggregator().Name);
        Assert.Equal("StdDev", new StdDevAggregator().Name);
        Assert.Equal("StdDev (Population)", new StdDevPAggregator().Name);
        Assert.Equal("First", new FirstAggregator().Name);
        Assert.Equal("Last", new LastAggregator().Name);
    }

    [Fact]
    public void CountNumbersAggregator_Merges_Counts()
    {
        var agg = new CountNumbersAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        left.Add(1);
        right.Add(2);

        left.Merge(right);

        Assert.Equal(2, left.GetResult());
    }

    [Fact]
    public void VarianceState_Handles_NonNumeric_And_Merge_Branches()
    {
        var variance = new VarianceAggregator();
        var left = variance.CreateState();
        var right = variance.CreateState();

        left.Add("skip");
        right.Add(2d);
        right.Add(4d);

        left.Merge(right);

        Assert.InRange((double)left.GetResult()!, 1.9, 2.1);

        var empty = variance.CreateState();
        Assert.Null(empty.GetResult());

        left.Merge(new SumAggregator().CreateState());
    }

    [Fact]
    public void StdDevState_Merges_And_Empty_Returns_Null()
    {
        var std = new StdDevAggregator();
        var left = std.CreateState();
        var right = std.CreateState();

        right.Add(1d);
        right.Add(3d);
        left.Merge(right);

        var empty = std.CreateState();
        Assert.Null(empty.GetResult());
    }

    [Fact]
    public void VarianceAggregator_Merges_NonEmpty_States()
    {
        var agg = new VarianceAggregator();
        var left = agg.CreateState();
        var right = agg.CreateState();

        left.Add(2d);
        left.Add(4d);
        right.Add(6d);

        left.Merge(right);

        Assert.NotNull(left.GetResult());
    }
}
