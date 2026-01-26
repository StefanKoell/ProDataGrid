using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotTableBuilderReflectionTests
{
    [Fact]
    public void Private_DisplayMode_Helpers_Are_Reachable()
    {
        var valueField = new PivotValueField
        {
            Header = "Amount",
            AggregateType = PivotAggregateType.Sum,
            DisplayMode = PivotValueDisplayMode.RunningTotal
        };

        var row = new PivotRow(PivotRowType.Detail, 0, new object?[] { "A" }, new object?[] { "A" }, "A", 0d, 1, valueField, 0);
        var column = new PivotColumn(0, PivotColumnType.Detail, new object?[] { "C" }, new[] { "C" }, valueField, 0, new PivotHeader(new[] { "C" }));

        var rows = new List<PivotRow> { row };
        var columns = new List<PivotColumn> { column };

        var usesSequence = (bool)InvokePrivateStatic("UsesSequenceDisplayModes", rows, columns)!;
        Assert.True(usesSequence);

        var usesPercent = (bool)InvokePrivateStatic("UsesDisplayMode", rows, columns, PivotValueDisplayMode.PercentOfRowTotal)!;
        Assert.False(usesPercent);
    }

    [Fact]
    public void Private_CellValue_Helper_Handles_Values()
    {
        var valueField = new PivotValueField
        {
            Header = "Amount",
            AggregateType = PivotAggregateType.Sum
        };

        var row = new PivotRow(PivotRowType.Detail, 0, new object?[] { "R" }, new object?[] { "R" }, null, 0d, 1, null, null);
        var column = new PivotColumn(0, PivotColumnType.Detail, new object?[] { "C" }, new[] { "C" }, valueField, 0, new PivotHeader(new[] { "C" }));

        var cellStates = BuildCellStateDictionary(new object?[] { "R" }, new object?[] { "C" }, valueField, 5d);
        var layout = new PivotLayoutOptions();

        var value = InvokePrivateStatic("GetCellValue", row, column, cellStates, layout, false, CultureInfo.InvariantCulture, null, null, null);
        Assert.Equal(5d, value);

        var emptyColumn = new PivotColumn(0, PivotColumnType.Detail, new object?[] { "C" }, new[] { "C" }, null, null, new PivotHeader(new[] { "C" }));
        var nullValue = InvokePrivateStatic("GetCellValue", row, emptyColumn, cellStates, layout, false, CultureInfo.InvariantCulture, null, null, null);
        Assert.Null(nullValue);
    }

    [Fact]
    public void Private_Key_Equality_Covers_Mismatched_Paths()
    {
        var builderType = typeof(PivotTableBuilder);
        var groupKeyType = builderType.GetNestedType("PivotGroupKey", BindingFlags.NonPublic)!;
        var cellKeyType = builderType.GetNestedType("PivotCellKey", BindingFlags.NonPublic)!;

        var groupKeyA = Activator.CreateInstance(groupKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { "A" }, 1 }, null)!;
        var groupKeyB = Activator.CreateInstance(groupKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { "A", "B" }, 1 }, null)!;

        Assert.False((bool)groupKeyType.GetMethod("Equals", new[] { groupKeyType })!.Invoke(groupKeyA, new[] { groupKeyB })!);
        _ = groupKeyType.GetMethod("GetHashCode")!.Invoke(groupKeyA, null);

        var cellKeyA = Activator.CreateInstance(cellKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { "A" }, new object?[] { "B" } }, null)!;
        var cellKeyB = Activator.CreateInstance(cellKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { "A", "C" }, new object?[] { "B" } }, null)!;

        Assert.False((bool)cellKeyType.GetMethod("Equals", new[] { cellKeyType })!.Invoke(cellKeyA, new[] { cellKeyB })!);
        _ = cellKeyType.GetMethod("GetHashCode")!.Invoke(cellKeyA, null);
    }

    private static object BuildCellStateDictionary(object?[] rowPath, object?[] columnPath, PivotValueField valueField, double value)
    {
        var builderType = typeof(PivotTableBuilder);
        var cellKeyType = builderType.GetNestedType("PivotCellKey", BindingFlags.NonPublic)!;
        var cellStateType = builderType.GetNestedType("PivotCellState", BindingFlags.NonPublic)!;

        var cellState = Activator.CreateInstance(cellStateType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new List<PivotValueField> { valueField }, new PivotAggregatorRegistry() }, null)!;
        cellStateType.GetMethod("Add")!.Invoke(cellState, new object?[] { 0, value });

        var cellKey = Activator.CreateInstance(cellKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { rowPath, columnPath }, null)!;

        var dictType = typeof(Dictionary<,>).MakeGenericType(cellKeyType, cellStateType);
        var dict = Activator.CreateInstance(dictType)!;
        dictType.GetMethod("Add")!.Invoke(dict, new[] { cellKey, cellState });

        return dict;
    }

    private static object? InvokePrivateStatic(string name, params object?[] args)
    {
        var method = typeof(PivotTableBuilder).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        return method!.Invoke(null, args);
    }
}
