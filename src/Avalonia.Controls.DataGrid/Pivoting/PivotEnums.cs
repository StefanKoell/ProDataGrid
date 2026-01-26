// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace Avalonia.Controls.DataGridPivoting
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotAggregateType
    {
        None,
        Sum,
        Count,
        Average,
        Min,
        Max,
        Product,
        CountNumbers,
        CountDistinct,
        StdDev,
        StdDevP,
        Variance,
        VarianceP,
        First,
        Last,
        Custom
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotValuesPosition
    {
        Columns,
        Rows
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotRowLayout
    {
        Compact,
        Tabular
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotRowType
    {
        Detail,
        Subtotal,
        GrandTotal
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotColumnType
    {
        Detail,
        Subtotal,
        GrandTotal
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotTotalPosition
    {
        End,
        Start
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotValueDisplayMode
    {
        Value,
        PercentOfRowTotal,
        PercentOfColumnTotal,
        PercentOfGrandTotal,
        PercentOfParentRowTotal,
        PercentOfParentColumnTotal,
        DifferenceFromPrevious,
        PercentDifferenceFromPrevious,
        RunningTotal,
        Index
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotValueFilterType
    {
        None,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Equal,
        NotEqual,
        Between,
        Top,
        Bottom,
        TopPercent,
        BottomPercent
    }
}
