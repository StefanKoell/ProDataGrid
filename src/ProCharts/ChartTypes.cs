// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

namespace ProCharts
{
    public enum ChartSeriesKind
    {
        Line,
        Column,
        Bar,
        Area,
        Scatter,
        Bubble,
        Waterfall,
        Histogram,
        Pareto,
        Radar,
        BoxWhisker,
        Funnel,
        Pie,
        Donut,
        StackedColumn,
        StackedBar,
        StackedArea,
        StackedColumn100,
        StackedBar100,
        StackedArea100
    }

    public enum ChartAxisKind
    {
        Category,
        Value,
        DateTime,
        Logarithmic
    }

    public enum ChartAxisCrossing
    {
        Auto,
        Minimum,
        Maximum,
        Value
    }

    public enum ChartValueAxisAssignment
    {
        Primary,
        Secondary
    }

    public enum ChartTrendlineType
    {
        None,
        Linear,
        Exponential,
        Logarithmic,
        Polynomial,
        Power,
        MovingAverage
    }

    public enum ChartErrorBarType
    {
        None,
        Fixed,
        Percentage,
        StandardDeviation,
        StandardError
    }

    public enum ChartLegendPosition
    {
        None,
        Top,
        Bottom,
        Left,
        Right
    }

    public enum ChartDownsampleMode
    {
        None,
        Bucket,
        MinMax,
        Lttb,
        Adaptive
    }

    public enum ChartLineInterpolation
    {
        Linear,
        Smooth,
        Step
    }
}
