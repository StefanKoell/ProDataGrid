using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.DataGridPivoting;
using Avalonia.Data.Core;
using Avalonia.Data.Converters;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotTableBuilderPrivateCoverageTests
{
    private sealed class Sample
    {
        public string Name { get; init; } = string.Empty;
    }

    private sealed class ThrowingComparable : IComparable
    {
        public int CompareTo(object? obj)
        {
            throw new InvalidOperationException("boom");
        }
    }

    private sealed class PrefixConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return string.Concat("p:", value?.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [Fact]
    public void PropertyInfos_And_Accessors_Invoke_Lambdas()
    {
        var row = new PivotRow(
            PivotRowType.Detail,
            0,
            new object?[] { "P" },
            new object?[] { "D" },
            "label",
            0d,
            1,
            null,
            null);
        row.CellValues[0] = "cell";

        var rowDisplayProperty = GetPrivateField<IPropertyInfo>("RowDisplayValuesProperty");
        var cellValuesProperty = GetPrivateField<IPropertyInfo>("CellValuesProperty");

        var nameProperty = rowDisplayProperty.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var typeProperty = rowDisplayProperty.GetType().GetProperty("PropertyType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Equal(nameof(PivotRow.RowDisplayValues), nameProperty?.GetValue(rowDisplayProperty));
        Assert.Equal(typeof(object?[]), typeProperty?.GetValue(rowDisplayProperty));

        var cellNameProperty = cellValuesProperty.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var cellTypeProperty = cellValuesProperty.GetType().GetProperty("PropertyType", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Equal(nameof(PivotRow.CellValues), cellNameProperty?.GetValue(cellValuesProperty));
        Assert.Equal(typeof(object?[]), cellTypeProperty?.GetValue(cellValuesProperty));
        Assert.Same(row.RowDisplayValues, InvokePropertyInfoGetter(rowDisplayProperty, row));
        Assert.Same(row.CellValues, InvokePropertyInfoGetter(cellValuesProperty, row));

        var displayAccessor = InvokePrivateStatic("CreateRowDisplayValueAccessor", 0)!;
        Assert.Null(InvokeAccessorGetValue(displayAccessor, null));
        Assert.Null(InvokeAccessorGetter(displayAccessor, null));

        var displayAccessorOutOfRange = InvokePrivateStatic("CreateRowDisplayValueAccessor", 5)!;
        Assert.Null(InvokeAccessorGetter(displayAccessorOutOfRange, row));

        Assert.Equal("D", InvokeAccessorGetter(displayAccessor, row));

        var pathAccessor = InvokePrivateStatic("CreateRowPathValueAccessor", 0)!;
        Assert.Null(InvokeAccessorGetValue(pathAccessor, null));
        Assert.Null(InvokeAccessorGetter(pathAccessor, null));
        Assert.Equal("P", InvokeAccessorGetter(pathAccessor, row));

        var pathAccessorOutOfRange = InvokePrivateStatic("CreateRowPathValueAccessor", 5)!;
        Assert.Null(InvokeAccessorGetter(pathAccessorOutOfRange, row));

        var cellAccessor = InvokePrivateStatic("CreateCellValueAccessor", 0)!;
        Assert.Null(InvokeAccessorGetValue(cellAccessor, null));
        Assert.Null(InvokeAccessorGetter(cellAccessor, null));
        Assert.Equal("cell", InvokeAccessorGetter(cellAccessor, row));

        var cellAccessorOutOfRange = InvokePrivateStatic("CreateCellValueAccessor", 5)!;
        Assert.Null(InvokeAccessorGetter(cellAccessorOutOfRange, row));

        Assert.Equal(string.Empty, InvokePrivateStatic("GetCompactLabelText", new object()));

        Assert.Equal(string.Empty, InvokePrivateStatic("GetRowDisplayText", new object(), 0, CultureInfo.InvariantCulture));
        Assert.Equal(string.Empty, InvokePrivateStatic("GetRowDisplayText", row, 5, CultureInfo.InvariantCulture));

        Assert.Equal(string.Empty, InvokePrivateStatic("GetCellValueText", new object(), 0, null, new PivotLayoutOptions(), CultureInfo.InvariantCulture, false, false));
        Assert.Equal(string.Empty, InvokePrivateStatic("GetCellValueText", row, 5, null, new PivotLayoutOptions(), CultureInfo.InvariantCulture, false, false));
    }

    [Fact]
    public void Formatting_Helpers_Cover_All_Branches()
    {
        var layout = new PivotLayoutOptions { EmptyValueLabel = "empty" };
        var culture = CultureInfo.InvariantCulture;

        var nullField = new PivotValueField { NullLabel = "n/a" };
        Assert.Equal("n/a", InvokePrivateStatic("FormatRowValue", nullField, null, culture, "empty"));
        Assert.Equal("empty", InvokePrivateStatic("FormatRowValue", new PivotValueField(), null, culture, "empty"));

        var converterField = new PivotValueField { Converter = new PrefixConverter() };
        Assert.Equal("p:val", InvokePrivateStatic("FormatRowValue", converterField, "val", culture, null));

        var formatField = new PivotValueField { StringFormat = "Value={0}", FormatProvider = culture };
        Assert.Equal("Value=5", InvokePrivateStatic("FormatRowValue", formatField, 5, culture, null));

        var invalidFormatField = new PivotValueField { StringFormat = "{0", FormatProvider = culture };
        Assert.Equal(7, InvokePrivateStatic("FormatRowValue", invalidFormatField, 7, culture, null));
        var formattableRowField = new PivotValueField { StringFormat = "N2", FormatProvider = culture };
        Assert.Equal("8.00", InvokePrivateStatic("FormatRowValue", formattableRowField, 8d, culture, null));
        var nonFormattableRowValue = new object();
        var nonFormattableRowField = new PivotValueField { StringFormat = "N2" };
        Assert.Same(nonFormattableRowValue, InvokePrivateStatic("FormatRowValue", nonFormattableRowField, nonFormattableRowValue, culture, null));

        Assert.Equal(" Total", InvokePrivateStatic("FormatSubtotalLabel", "", new PivotLayoutOptions { SubtotalLabelFormat = string.Empty }, culture));

        var valueField = new PivotValueField { Converter = new PrefixConverter(), FormatProvider = culture };
        Assert.Equal("", InvokePrivateStatic("FormatPivotValueText", null, valueField, layout, culture, false, true));
        Assert.Equal("empty", InvokePrivateStatic("FormatPivotValueText", null, valueField, layout, culture, true, false));
        Assert.Equal("n/a", InvokePrivateStatic("FormatPivotValueText", null, new PivotValueField { NullLabel = "n/a" }, layout, culture, false, false));

        Assert.Equal("text", InvokePrivateStatic("FormatPivotValueText", "text", null, layout, culture, true, false));

        var converterValueField = new PivotValueField { Converter = new PrefixConverter() };
        Assert.Equal("p:x", InvokePrivateStatic("FormatPivotValueText", "x", converterValueField, layout, culture, false, false));

        var percentField = new PivotValueField { DisplayMode = PivotValueDisplayMode.PercentOfRowTotal };
        Assert.Equal("10.00 %", InvokePrivateStatic("FormatPivotValueText", 0.1, percentField, layout, culture, false, false));

        var stringFormatField = new PivotValueField { StringFormat = "Value={0}", FormatProvider = culture };
        Assert.Equal("Value=4", InvokePrivateStatic("FormatPivotValueText", 4, stringFormatField, layout, culture, false, false));

        var invalidFormatValueField = new PivotValueField { StringFormat = "{0", FormatProvider = culture };
        Assert.Equal("5", InvokePrivateStatic("FormatPivotValueText", 5, invalidFormatValueField, layout, culture, false, false));

        var fallbackField = new PivotValueField { StringFormat = "N2", FormatProvider = culture };
        Assert.Equal("6.00", InvokePrivateStatic("FormatPivotValueText", 6d, fallbackField, layout, culture, false, false));
        var formattablePivotField = new PivotValueField { StringFormat = "N2", FormatProvider = culture };
        Assert.Equal("9.00", InvokePrivateStatic("FormatPivotValueText", 9d, formattablePivotField, layout, culture, false, false));
        var nonFormattablePivotValue = new object();
        var nonFormattablePivotField = new PivotValueField { StringFormat = "N2" };
        Assert.Equal(nonFormattablePivotValue.ToString(), InvokePrivateStatic("FormatPivotValueText", nonFormattablePivotValue, nonFormattablePivotField, layout, culture, false, false));
    }

    [Fact]
    public void Comparison_And_Resolve_Helpers_Cover_Branches()
    {
        var comparer = Comparer<object?>.Default;
        Assert.Equal(0, InvokePrivateStatic("CompareAggregateValues", null, null, comparer));
        Assert.Equal(1, InvokePrivateStatic("CompareAggregateValues", null, "x", comparer));
        Assert.Equal(-1, InvokePrivateStatic("CompareAggregateValues", "x", null, comparer));
        Assert.Equal(0, InvokePrivateStatic("CompareAggregateValues", "a", "a", comparer));

        var valueFields = new List<PivotValueField>();
        Assert.Equal(-1, InvokePrivateStatic("ResolveValueFieldIndex", null, valueFields));

        valueFields.Add(new PivotValueField { Key = "A" });
        Assert.Equal(0, InvokePrivateStatic("ResolveValueFieldIndex", null, valueFields));
        Assert.Equal(-1, InvokePrivateStatic("ResolveValueFieldIndex", new PivotValueField(), valueFields));
        Assert.Equal(0, InvokePrivateStatic("ResolveValueFieldIndex", new PivotValueField { Key = "A" }, valueFields));

        var filter = new PivotValueFilter { FilterType = PivotValueFilterType.None };
        Assert.False((bool)InvokePrivateStatic("MatchesValueFilter", null, filter)!);
        Assert.True((bool)InvokePrivateStatic("MatchesValueFilter", 1d, filter)!);

        Assert.False((bool)InvokePrivateStatic("IsNumericValueField", (object?)null)!);

        var cellStates = CreateCellStatesDictionary();
        Assert.Null(InvokePrivateStatic("GetTotalValue", cellStates, Array.Empty<object?>(), Array.Empty<object?>(), 0));
    }

    [Fact]
    public void GroupNode_And_Key_Comparisons_Cover_Branches()
    {
        var nodeType = Nested("PivotGroupNode");
        var node = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());

        var parentProperty = nodeType.GetProperty("Parent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.Null(parentProperty!.GetValue(node));

        var filterChildren = nodeType.GetMethod("FilterChildren")!;
        filterChildren.Invoke(node, new object?[] { new Func<object, bool>(_ => true) });

        var groupKeyType = Nested("PivotGroupKey");
        var groupKey = Activator.CreateInstance(groupKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { null }, 1 }, null)!;
        Assert.False((bool)groupKeyType.GetMethod("Equals", new[] { typeof(object) })!.Invoke(groupKey, new object?[] { new object() })!);
        var groupKeyMatch = Activator.CreateInstance(groupKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { null }, 1 }, null)!;
        var groupKeyMismatch = Activator.CreateInstance(groupKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { "A" }, 1 }, null)!;
        var groupEquals = groupKeyType.GetMethod("Equals", new[] { groupKeyType })!;
        Assert.True((bool)groupEquals.Invoke(groupKey, new object?[] { groupKeyMatch })!);
        Assert.False((bool)groupEquals.Invoke(groupKey, new object?[] { groupKeyMismatch })!);
        var groupKeyEqual = Activator.CreateInstance(groupKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { "A" }, 1 }, null)!;
        Assert.True((bool)groupEquals.Invoke(groupKeyMismatch, new object?[] { groupKeyEqual })!);

        var cellKeyType = Nested("PivotCellKey");
        var cellKey = Activator.CreateInstance(cellKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { null }, new object?[] { "B" } }, null)!;
        Assert.False((bool)cellKeyType.GetMethod("Equals", new[] { typeof(object) })!.Invoke(cellKey, new object?[] { new object() })!);
        var cellKeyMismatch = Activator.CreateInstance(cellKeyType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { new object?[] { null }, new object?[] { null } }, null)!;
        var cellEquals = cellKeyType.GetMethod("Equals", new[] { cellKeyType })!;
        Assert.False((bool)cellEquals.Invoke(cellKey, new object?[] { cellKeyMismatch })!);
    }

    [Fact]
    public void ValueComparer_And_NullAggregationState_Cover_Branches()
    {
        var comparerType = Nested("PivotValueComparer");
        var comparer = (IComparer<object?>)Activator.CreateInstance(comparerType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { CultureInfo.InvariantCulture },
            null)!;

        var same = new object();
        Assert.Equal(0, comparer.Compare(same, same));
        Assert.Equal(1, comparer.Compare(null, "x"));
        Assert.Equal(-1, comparer.Compare("x", null));
        Assert.Equal(-1, comparer.Compare(1, 2));
        Assert.Equal(0, comparer.Compare(new Sample { Name = "a" }, new Sample { Name = "a" }));
        Assert.Equal(0, comparer.Compare(new ThrowingComparable(), new ThrowingComparable()));

        var valueField = new PivotValueField { Formula = "1+1" };
        var cellStateType = Nested("PivotCellState");
        var state = Activator.CreateInstance(cellStateType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { new List<PivotValueField> { valueField }, new PivotAggregatorRegistry() },
            null)!;

        var statesField = cellStateType.GetField("_states", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var states = (Array)statesField.GetValue(state)!;
        var nullState = states.GetValue(0)!;

        nullState.GetType().GetMethod("Add")!.Invoke(nullState, new object?[] { 1 });
        nullState.GetType().GetMethod("Merge")!.Invoke(nullState, new object?[] { nullState });
    }

    [Fact]
    public void Filters_And_ItemsWithNoData_Cover_Branches()
    {
        var rowFields = new List<PivotAxisField>
        {
            new()
            {
                ValueSelector = item => ((Sample)item!).Name,
                Filter = new PivotFieldFilter(excluded: new[] { "B" })
            }
        };

        var columnFields = new List<PivotAxisField>
        {
            new()
            {
                ValueSelector = item => ((Sample)item!).Name,
                Filter = new PivotFieldFilter(excluded: new[] { "A" })
            }
        };

        var filterFields = new List<PivotAxisField>
        {
            new()
        };

        var rowValues = new object?[1];
        var columnValues = new object?[1];

        Assert.False((bool)InvokePrivateStatic("PassesFilters", new Sample { Name = "A" }, rowFields, columnFields, filterFields, rowValues, columnValues)!);

        var filterFields2 = new List<PivotAxisField>
        {
            new() { ValueSelector = item => ((Sample)item!).Name },
            new()
            {
                ValueSelector = item => ((Sample)item!).Name,
                Filter = new PivotFieldFilter(excluded: new[] { "A" })
            }
        };
        Assert.False((bool)InvokePrivateStatic(
            "PassesFilters",
            new Sample { Name = "A" },
            new List<PivotAxisField>(),
            new List<PivotAxisField>(),
            filterFields2,
            Array.Empty<object?>(),
            Array.Empty<object?>())!);

        var root = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());
        var fields = new List<PivotAxisField>
        {
            new()
            {
                ShowItemsWithNoData = true,
                ItemsSource = new object?[] { "A", "A" }
            },
            new()
            {
                ShowItemsWithNoData = true,
                ItemsSource = new object?[] { "B" },
                Filter = new PivotFieldFilter(excluded: new[] { "B" })
            }
        };

        InvokePrivateStatic("EnsureItemsWithNoData", root, fields, "empty", CultureInfo.InvariantCulture);

        var child = InvokeGroupChild(root, fields[0], "A", CultureInfo.InvariantCulture, "empty");
        InvokePrivateStatic("EnsureItemsWithNoData", root, fields, "empty", CultureInfo.InvariantCulture);

        var rootMissingParents = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());
        var fieldsMissingParents = new List<PivotAxisField>
        {
            new() { ShowItemsWithNoData = true },
            new() { ShowItemsWithNoData = true, ItemsSource = new object?[] { "B" } }
        };
        InvokePrivateStatic("EnsureItemsWithNoData", rootMissingParents, fieldsMissingParents, "empty", CultureInfo.InvariantCulture);

        var rootWithParents = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());
        var fieldsWithParents = new List<PivotAxisField>
        {
            new() { ShowItemsWithNoData = true, ItemsSource = new object?[] { "A" } },
            new() { ShowItemsWithNoData = true, ItemsSource = new object?[] { "B" } }
        };
        InvokePrivateStatic("EnsureItemsWithNoData", rootWithParents, fieldsWithParents, "empty", CultureInfo.InvariantCulture);

        var nodesByLevel = Array.CreateInstance(typeof(List<>).MakeGenericType(Nested("PivotGroupNode")), 1);
        var nodeSetsByLevel = Array.CreateInstance(typeof(HashSet<>).MakeGenericType(Nested("PivotGroupNode")), 1);
        InvokePrivateStatic("CollectNodesByLevel", root, nodesByLevel, nodeSetsByLevel);
        InvokePrivateStatic("AddNodeToLevel", nodesByLevel, nodeSetsByLevel, -1, child);
    }

    [Fact]
    public void PassesFilters_FilterField_Match_Returns_True()
    {
        var filterFields = new List<PivotAxisField>
        {
            new()
            {
                ValueSelector = item => ((Sample)item!).Name,
                Filter = new PivotFieldFilter(included: new object?[] { "A" })
            }
        };

        var result = (bool)InvokePrivateStatic(
            "PassesFilters",
            new Sample { Name = "A" },
            new List<PivotAxisField>(),
            new List<PivotAxisField>(),
            filterFields,
            Array.Empty<object?>(),
            Array.Empty<object?>())!;

        Assert.True(result);
    }

    [Fact]
    public void Sorting_And_ValueFilters_Cover_Branches()
    {
        var node = CreateGroupNode(null, 0, "A", new object?[] { "A" }, new[] { "A" });
        InvokeGroupChild(node, new PivotAxisField { Header = "Field" }, "B", CultureInfo.InvariantCulture, null);

        InvokePrivateStatic("SortNode", node, new List<PivotAxisField>(), new List<PivotValueField>(), CreateCellStatesDictionary(), true, CreateComparer(), null);

        var valueField = new PivotValueField { Header = "Amount", AggregateType = PivotAggregateType.Sum };
        var valueFields = new List<PivotValueField> { valueField };
        var sortField = new PivotAxisField { ValueSort = new PivotValueSort { ValueField = valueField } };
        InvokePrivateStatic("SortNode", node, new List<PivotAxisField> { sortField }, valueFields, CreateCellStatesDictionary(), true, CreateComparer(), null);

        var filterNode = CreateGroupNode(null, 0, "A", new object?[] { "A" }, new[] { "A" });
        InvokePrivateStatic("FilterChildrenByValue", filterNode, new PivotValueFilter { FilterType = PivotValueFilterType.Top }, valueFields, CreateCellStatesDictionary(), true, null);

        var child = InvokeGroupChild(filterNode, new PivotAxisField { Header = "Field" }, "B", CultureInfo.InvariantCulture, null);
        InvokePrivateStatic("FilterChildrenByValue", filterNode, new PivotValueFilter { FilterType = PivotValueFilterType.Top }, valueFields, CreateCellStatesDictionary(), true, null);

        var children = (System.Collections.IList)GetProperty(filterNode, "Children")!;
        if (children.Count > 0)
        {
            InvokePrivateStatic("FilterChildrenByValue", filterNode, new PivotValueFilter { FilterType = PivotValueFilterType.Top }, valueFields, CreateCellStatesDictionary(), true, null);
        }

        Assert.True((bool)InvokePrivateStatic("ApplyValueFiltersRecursive", filterNode, new List<PivotAxisField> { new PivotAxisField() }, valueFields, CreateCellStatesDictionary(), true, null)!);
    }

    [Fact]
    public void SortNode_ValueSort_NullValues_Uses_TieBreaker()
    {
        var valueField = new PivotValueField { Header = "Amount", AggregateType = PivotAggregateType.Sum };
        var sortField = new PivotAxisField { ValueSort = new PivotValueSort { ValueField = valueField } };

        var root = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());
        InvokeGroupChild(root, sortField, "B", CultureInfo.InvariantCulture, null);
        InvokeGroupChild(root, sortField, "A", CultureInfo.InvariantCulture, null);

        InvokePrivateStatic(
            "SortNode",
            root,
            new List<PivotAxisField> { sortField },
            new List<PivotValueField> { valueField },
            CreateCellStatesDictionary(),
            true,
            CreateComparer(),
            null);
    }

    [Fact]
    public void ValueFilters_EarlyReturn_Branches()
    {
        var field = new PivotAxisField();
        var root = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());
        var child = InvokeGroupChild(root, field, "A", CultureInfo.InvariantCulture, null);
        InvokeGroupChild(child, field, "B", CultureInfo.InvariantCulture, null);

        Assert.True((bool)InvokePrivateStatic(
            "ApplyValueFiltersRecursive",
            child,
            new List<PivotAxisField> { field },
            new List<PivotValueField>(),
            CreateCellStatesDictionary(),
            true,
            null)!);

        var filterNode = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());
        InvokeGroupChild(filterNode, field, "A", CultureInfo.InvariantCulture, null);

        var filter = new PivotValueFilter { FilterType = PivotValueFilterType.Top, ValueField = new PivotValueField() };
        InvokePrivateStatic(
            "FilterChildrenByValue",
            filterNode,
            filter,
            new List<PivotValueField>(),
            CreateCellStatesDictionary(),
            true,
            null);
    }

    [Fact]
    public void BuildRows_Columns_Totals_And_CellValue_Helpers()
    {
        var layout = new PivotLayoutOptions { ValuesPosition = PivotValuesPosition.Rows };
        var rowFields = new List<PivotAxisField>();
        var valueFields = new List<PivotValueField> { new PivotValueField { Header = "Amount", AggregateType = PivotAggregateType.Sum } };

        var root = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());
        var rows = (List<PivotRow>)InvokePrivateStatic("BuildRows", root, rowFields, valueFields, new bool[0], layout, 0, CultureInfo.InvariantCulture)!;
        Assert.Single(rows);

        var parent = CreateGroupNode(null, 0, "A", new object?[] { "A" }, new[] { "A" });
        var child = InvokeGroupChild(parent, new PivotAxisField(), "B", CultureInfo.InvariantCulture, null);
        var rowFields2 = new List<PivotAxisField> { new PivotAxisField(), new PivotAxisField() };
        var subtotals = new[] { true, false };
        InvokePrivateStatic("BuildRows", parent, rowFields2, valueFields, subtotals, layout, 0, CultureInfo.InvariantCulture);

        InvokePrivateStatic("CreateRow", child, rowFields2, new PivotLayoutOptions(), PivotRowType.Subtotal, 0, valueFields[0], 0, CultureInfo.InvariantCulture);

        var columns = new List<PivotColumn>
        {
            new PivotColumn(0, PivotColumnType.Subtotal, new object?[] { "A" }, new[] { "A" }, valueFields[0], 0, new PivotHeader(new[] { "A" }))
        };
        var columnFields = new List<PivotAxisField> { new PivotAxisField { SubtotalPosition = PivotTotalPosition.Start } };
        InvokePrivateStatic("BuildColumns", parent, columnFields, valueFields, new[] { true }, new PivotLayoutOptions(), true, CultureInfo.InvariantCulture);

        var totalsLookupType = Nested("PivotTotalsLookup");
        var totals = Activator.CreateInstance(totalsLookupType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { true, null, null, null, null, null }, null)!;
        Assert.Null(totalsLookupType.GetMethod("GetRowTotal")!.Invoke(totals, new object?[] { 0, 0 }));
        Assert.Null(totalsLookupType.GetMethod("GetColumnTotal")!.Invoke(totals, new object?[] { 0, 0 }));

        var row = new PivotRow(PivotRowType.Detail, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, valueFields[0], null);
        var column = new PivotColumn(0, PivotColumnType.Detail, Array.Empty<object?>(), Array.Empty<string?>(), valueFields[0], null, new PivotHeader(new[] { "A" }));
        var cellStates = CreateCellStatesDictionary();
        var tryGet = typeof(PivotTableBuilder).GetMethod("TryGetCellRawValue", BindingFlags.NonPublic | BindingFlags.Static);
        var tryGetArgs = new object?[] { row, column, cellStates, false, null, null, null, null, -1, null };
        Assert.False((bool)tryGet!.Invoke(null, tryGetArgs)!);
        Assert.Same(valueFields[0], tryGetArgs[7]);
        Assert.Equal(-1, tryGetArgs[8]);
        Assert.Null(tryGetArgs[9]);

        var display = InvokePrivateStatic("GetCellDisplayValue", row, column, cellStates, new PivotLayoutOptions(), false, CultureInfo.InvariantCulture, valueFields[0], 0, "x", 0, 0, totals, null, null);
        Assert.Equal("x", display);
    }

    [Fact]
    public void BuildRows_Columns_Subtotal_Start_End_And_CopyPathValues()
    {
        var culture = CultureInfo.InvariantCulture;
        var valueFields = new List<PivotValueField> { new PivotValueField { Header = "Amount", AggregateType = PivotAggregateType.Sum } };

        var columnRoot = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());
        var columnField0 = new PivotAxisField { SubtotalPosition = PivotTotalPosition.Start };
        var columnField1 = new PivotAxisField();
        var columnChild = InvokeGroupChild(columnRoot, columnField0, "A", culture, null);
        InvokeGroupChild(columnChild, columnField1, "B", culture, null);

        InvokePrivateStatic(
            "BuildColumns",
            columnRoot,
            new List<PivotAxisField> { columnField0, columnField1 },
            valueFields,
            new[] { true, false },
            new PivotLayoutOptions(),
            true,
            culture);

        var rowRoot = CreateGroupNode(null, -1, null, Array.Empty<object?>(), Array.Empty<string?>());
        var rowField0 = new PivotAxisField { SubtotalPosition = PivotTotalPosition.Start };
        var rowField1 = new PivotAxisField();
        var rowChild = InvokeGroupChild(rowRoot, rowField0, "A", culture, null);
        InvokeGroupChild(rowChild, rowField1, "B", culture, null);

        var layout = new PivotLayoutOptions { ValuesPosition = PivotValuesPosition.Rows };
        var rowSubtotals = new[] { true, false };
        InvokePrivateStatic(
            "BuildRows",
            rowRoot,
            new List<PivotAxisField> { rowField0, rowField1 },
            valueFields,
            rowSubtotals,
            layout,
            0,
            culture);

        rowField0.SubtotalPosition = PivotTotalPosition.End;
        InvokePrivateStatic(
            "BuildRows",
            rowRoot,
            new List<PivotAxisField> { rowField0, rowField1 },
            valueFields,
            rowSubtotals,
            layout,
            0,
            culture);

        var copied = (object?[])InvokePrivateStatic("CopyPathValues", (object)Array.Empty<object?>());
        Assert.Empty(copied);
    }

    [Fact]
    public void Totals_And_Display_Branches_Are_Reached()
    {
        var totalsLookupType = Nested("PivotTotalsLookup");
        var totalsByValue = new object?[][] { new object?[] { 1d } };
        var totals = Activator.CreateInstance(totalsLookupType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
            new object?[] { true, null, null, null, totalsByValue, null }, null)!;
        Assert.Equal(1d, totalsLookupType.GetMethod("GetColumnTotal")!.Invoke(totals, new object?[] { 0, 0 }));

        var valueField = new PivotValueField { DisplayMode = PivotValueDisplayMode.PercentOfRowTotal };
        var row = new PivotRow(PivotRowType.Detail, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, valueField, 0);
        var column = new PivotColumn(0, PivotColumnType.Detail, Array.Empty<object?>(), Array.Empty<string?>(), valueField, 0, new PivotHeader(new[] { "A" }));
        var cellStates = CreateCellStatesDictionary();

        var display = InvokePrivateStatic("GetCellDisplayValue", row, column, cellStates, new PivotLayoutOptions(), false, CultureInfo.InvariantCulture, valueField, 0, "x", 0, 0, totals, null, null);
        Assert.Null(display);

        display = InvokePrivateStatic("GetCellDisplayValue", row, column, cellStates, new PivotLayoutOptions(), false, CultureInfo.InvariantCulture, valueField, 0, 1d, 0, 0, totals, null, null);
        Assert.Null(display);

        var indexValue = InvokePrivateStatic("GetIndexValue", row, column, cellStates, new PivotLayoutOptions(), false, CultureInfo.InvariantCulture, valueField, 0, "x", 0, 0, totals);
        Assert.Null(indexValue);

        var groupKey = CreateGroupKey();
        var runningTotals = (System.Collections.IDictionary)CreateGroupKeyDictionary();
        var previousValues = (System.Collections.IDictionary)CreateGroupKeyDictionary();
        var sequenceValue = InvokePrivateStatic("GetSequenceDisplayValue", PivotValueDisplayMode.PercentDifferenceFromPrevious, "x", valueField, runningTotals, previousValues, groupKey, false, CultureInfo.InvariantCulture, null);
        Assert.Null(sequenceValue);

        runningTotals = (System.Collections.IDictionary)CreateGroupKeyDictionary();
        var previousValuesWithEntry = (System.Collections.IDictionary)CreateGroupKeyDictionary();
        previousValuesWithEntry[groupKey] = 0d;
        sequenceValue = InvokePrivateStatic("GetSequenceDisplayValue", PivotValueDisplayMode.PercentDifferenceFromPrevious, 1d, valueField, runningTotals, previousValuesWithEntry, groupKey, false, CultureInfo.InvariantCulture, null);
        Assert.Null(sequenceValue);

        runningTotals = (System.Collections.IDictionary)CreateGroupKeyDictionary();
        previousValues = (System.Collections.IDictionary)CreateGroupKeyDictionary();
        sequenceValue = InvokePrivateStatic("GetSequenceDisplayValue", PivotValueDisplayMode.Value, 2d, valueField, runningTotals, previousValues, groupKey, false, CultureInfo.InvariantCulture, null);
        Assert.Equal(2d, sequenceValue);

        var useSequence = InvokePrivateStatic("UsesSequenceDisplayModes", new List<PivotRow>(), new List<PivotColumn>());
        Assert.False((bool)useSequence!);

        var columnKey = new PivotColumn(0, PivotColumnType.Subtotal, Array.Empty<object?>(), Array.Empty<string?>(), valueField, 0, new PivotHeader(Array.Empty<string>()));
        InvokePrivateStatic("BuildColumnGroupKeys", new List<PivotColumn> { columnKey }, false, null);

        var denominator = InvokePrivateStatic("GetDisplayDenominator", row, column, cellStates, valueField, 0, 0, 0, totals, null, null);
        Assert.Null(denominator);

        var buildParentLookup = typeof(PivotTableBuilder).GetMethod("BuildParentPathLookup", BindingFlags.NonPublic | BindingFlags.Static);
        var genericParentLookup = buildParentLookup!.MakeGenericMethod(typeof(PivotRow));
        var parentLookup = genericParentLookup.Invoke(null, new object?[] { new[] { row } });
        Assert.NotNull(parentLookup);

        Assert.True((bool)InvokePrivateStatic("UsesDisplayMode", new List<PivotRow> { row }, new List<PivotColumn>(), PivotValueDisplayMode.PercentOfRowTotal)!);
    }

    [Fact]
    public void TotalsLookup_ValuesInRows_And_RowTotal_Null()
    {
        var valueField = new PivotValueField { Header = "Amount", AggregateType = PivotAggregateType.Sum };
        var rows = new List<PivotRow>
        {
            new PivotRow(PivotRowType.Detail, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, valueField, 0)
        };
        var columns = new List<PivotColumn>
        {
            new PivotColumn(0, PivotColumnType.Detail, Array.Empty<object?>(), Array.Empty<string?>(), valueField, 0, new PivotHeader(Array.Empty<string>()))
        };

        _ = InvokePrivateStatic(
            "BuildTotalsLookup",
            rows,
            columns,
            CreateCellStatesDictionary(),
            1,
            true,
            false,
            true,
            false);

        var emptyTotals = CreateTotalsLookup(false, null, null, null, null, null);
        var totalsLookupType = Nested("PivotTotalsLookup");
        Assert.Null(totalsLookupType.GetMethod("GetRowTotal")!.Invoke(emptyTotals, new object?[] { 0, 0 }));
    }

    [Fact]
    public void DisplayValue_Percent_And_Index_Branches()
    {
        var layout = new PivotLayoutOptions();
        var culture = CultureInfo.InvariantCulture;
        var valueField = new PivotValueField { DisplayMode = PivotValueDisplayMode.PercentOfRowTotal };
        var row = new PivotRow(PivotRowType.Detail, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, valueField, 0);
        var column = new PivotColumn(0, PivotColumnType.Detail, Array.Empty<object?>(), Array.Empty<string?>(), valueField, 0, new PivotHeader(new[] { "A" }));
        var cellStates = CreateCellStatesDictionary();

        var numericTotals = CreateTotalsLookup(true, new object?[] { 1d }, null, null, null, null);
        Assert.Null(InvokePrivateStatic("GetCellDisplayValue", row, column, cellStates, layout, true, culture, valueField, 0, "x", 0, 0, numericTotals, null, null));

        var nonNumericTotals = CreateTotalsLookup(true, new object?[] { "x" }, null, null, null, null);
        Assert.Null(InvokePrivateStatic("GetCellDisplayValue", row, column, cellStates, layout, true, culture, valueField, 0, 1d, 0, 0, nonNumericTotals, null, null));

        var zeroTotals = CreateTotalsLookup(true, new object?[] { 0d }, null, null, null, null);
        Assert.Null(InvokePrivateStatic("GetCellDisplayValue", row, column, cellStates, layout, true, culture, valueField, 0, 1d, 0, 0, zeroTotals, null, null));

        var invalidDisplayField = new PivotValueField { DisplayMode = (PivotValueDisplayMode)999 };
        Assert.Equal(1d, InvokePrivateStatic("GetCellDisplayValue", row, column, cellStates, layout, true, culture, invalidDisplayField, 0, 1d, 0, 0, numericTotals, null, null));

        var indexField = new PivotValueField { DisplayMode = PivotValueDisplayMode.Index };
        var indexTotals = CreateTotalsLookup(true, new object?[] { 0d }, null, null, new object?[][] { new object?[] { 1d } }, new object?[] { 1d });
        Assert.Null(InvokePrivateStatic("GetIndexValue", row, column, cellStates, layout, true, culture, indexField, 0, 1d, 0, 0, indexTotals));
    }

    [Fact]
    public void SequenceDisplayModes_And_UsesDisplayMode_Branches()
    {
        var sequenceField = new PivotValueField { DisplayMode = PivotValueDisplayMode.RunningTotal };
        var nonSequenceField = new PivotValueField { DisplayMode = PivotValueDisplayMode.Value };

        var columns = new List<PivotColumn>
        {
            new PivotColumn(0, PivotColumnType.Detail, Array.Empty<object?>(), Array.Empty<string?>(), nonSequenceField, 0, new PivotHeader(Array.Empty<string>()))
        };
        var rows = new List<PivotRow>
        {
            new PivotRow(PivotRowType.Detail, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, sequenceField, 0)
        };

        Assert.True((bool)InvokePrivateStatic("UsesSequenceDisplayModes", rows, columns)!);

        var nonSequenceRows = new List<PivotRow>
        {
            new PivotRow(PivotRowType.Detail, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, nonSequenceField, 0)
        };
        Assert.False((bool)InvokePrivateStatic("UsesSequenceDisplayModes", nonSequenceRows, columns)!);
        Assert.True((bool)InvokePrivateStatic("UsesDisplayMode", new List<PivotRow>(), columns, PivotValueDisplayMode.Value)!);
    }

    [Fact]
    public void ParentPathLookup_And_Denominator_Branches()
    {
        var row = new PivotRow(PivotRowType.Detail, 0, Array.Empty<object?>(), Array.Empty<object?>(), null, 0d, 1, null, null);
        var column = new PivotColumn(0, PivotColumnType.Detail, Array.Empty<object?>(), Array.Empty<string?>(), null, null, new PivotHeader(Array.Empty<string>()));
        var cellStates = CreateCellStatesDictionary();
        var totals = CreateTotalsLookup(true, new object?[] { 1d }, null, null, null, null);

        var parentField = new PivotValueField { DisplayMode = PivotValueDisplayMode.PercentOfParentRowTotal };
        Assert.Null(InvokePrivateStatic("GetDisplayDenominator", row, column, cellStates, parentField, 0, 0, 0, totals, null, null));

        var defaultField = new PivotValueField { DisplayMode = PivotValueDisplayMode.Value };
        Assert.Null(InvokePrivateStatic("GetDisplayDenominator", row, column, cellStates, defaultField, 0, 0, 0, totals, null, null));

        var buildParentLookup = typeof(PivotTableBuilder).GetMethod("BuildParentPathLookup", BindingFlags.NonPublic | BindingFlags.Static);
        var generic = buildParentLookup!.MakeGenericMethod(typeof(object));
        var lookup = generic.Invoke(null, new object?[] { new object[] { new object() } });
        Assert.NotNull(lookup);
    }

    [Fact]
    public void NumericColumn_Uses_NumberFormatInfo()
    {
        var model = new PivotTableModel();
        using (model.DeferRefresh())
        {
            model.ItemsSource = new[] { new Sample { Name = "A" } };
            model.RowFields.Add(new PivotAxisField
            {
                Header = "Name",
                ValueSelector = item => ((Sample)item!).Name
            });
            model.ValueFields.Add(new PivotValueField
            {
                Header = "Amount",
                ValueSelector = _ => 1d,
                AggregateType = PivotAggregateType.Sum,
                FormatProvider = new NumberFormatInfo()
            });
        }

        model.Refresh();

        Assert.Contains(model.ColumnDefinitions, def => def is DataGridNumericColumnDefinition numeric && numeric.NumberFormat != null);
    }

    private static object InvokePrivateStatic(string name, params object?[] args)
    {
        var method = typeof(PivotTableBuilder).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        return method!.Invoke(null, args)!;
    }

    private static object? InvokePropertyInfoGetValue(object propertyInfo, object target)
    {
        var methods = propertyInfo.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        MethodInfo? method = null;
        foreach (var candidate in methods)
        {
            if ((candidate.Name == "GetValue" || candidate.Name.EndsWith(".GetValue", StringComparison.Ordinal)) &&
                candidate.GetParameters().Length >= 1)
            {
                method = candidate;
                break;
            }
        }

        if (method == null)
        {
            return null;
        }

        var parameters = method.GetParameters();
        var args = new object?[parameters.Length];
        args[0] = target;
        for (var i = 1; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.HasDefaultValue)
            {
                args[i] = parameter.DefaultValue;
            }
            else
            {
                args[i] = parameter.ParameterType.IsValueType
                    ? Activator.CreateInstance(parameter.ParameterType)
                    : null;
            }
        }

        return method.Invoke(propertyInfo, args);
    }

    private static object? InvokePropertyInfoGetter(object propertyInfo, object target)
    {
        var fields = propertyInfo.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            if (!typeof(Delegate).IsAssignableFrom(field.FieldType))
            {
                continue;
            }

            var getter = field.GetValue(propertyInfo) as Delegate;
            if (getter == null)
            {
                continue;
            }

            var parameters = getter.Method.GetParameters();
            if (parameters.Length != 1)
            {
                continue;
            }

            return getter.DynamicInvoke(target);
        }

        return InvokePropertyInfoGetValue(propertyInfo, target);
    }

    private static T GetPrivateField<T>(string name)
    {
        var field = typeof(PivotTableBuilder).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
        return (T)field!.GetValue(null)!;
    }

    private static object? InvokeAccessorGetValue(object accessor, object? item)
    {
        var method = accessor.GetType().GetMethod(
            "GetValue",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new[] { typeof(object) },
            null);
        return method!.Invoke(accessor, new object?[] { item });
    }

    private static object? InvokeAccessorGetter(object accessor, object? item)
    {
        var field = accessor.GetType().GetField("_getter", BindingFlags.Instance | BindingFlags.NonPublic);
        var getter = (Delegate)field!.GetValue(accessor)!;
        var parameters = getter.Method.GetParameters();
        var args = new object?[parameters.Length];
        if (args.Length > 0)
        {
            args[0] = item;
        }

        return getter.DynamicInvoke(args);
    }

    private static object CreateGroupNode(object? parent, int level, object? key, object?[] pathValues, string?[] pathDisplayValues)
    {
        var nodeType = Nested("PivotGroupNode");
        return Activator.CreateInstance(nodeType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { parent, level, key, pathValues, pathDisplayValues },
            null)!;
    }

    private static object InvokeGroupChild(object parent, PivotAxisField field, object? value, CultureInfo culture, string? emptyValueLabel)
    {
        var method = parent.GetType().GetMethod("GetOrCreateChild");
        return method!.Invoke(parent, new object?[] { field, value, culture, emptyValueLabel })!;
    }

    private static object? GetProperty(object instance, string name)
    {
        return instance.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(instance);
    }

    private static object CreateCellStatesDictionary()
    {
        var cellKeyType = Nested("PivotCellKey");
        var cellStateType = Nested("PivotCellState");
        var dictType = typeof(Dictionary<,>).MakeGenericType(cellKeyType, cellStateType);
        return Activator.CreateInstance(dictType)!;
    }

    private static object CreateGroupKey()
    {
        var groupKeyType = Nested("PivotGroupKey");
        return Activator.CreateInstance(groupKeyType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { Array.Empty<object?>(), null },
            null)!;
    }

    private static object CreateGroupKeyDictionary()
    {
        var groupKeyType = Nested("PivotGroupKey");
        var dictType = typeof(Dictionary<,>).MakeGenericType(groupKeyType, typeof(double));
        return Activator.CreateInstance(dictType)!;
    }

    private static object CreateTotalsLookup(
        bool valuesInRows,
        object?[]? rowTotals,
        object?[][]? rowTotalsByValueIndex,
        object?[]? columnTotals,
        object?[][]? columnTotalsByValueIndex,
        object?[]? grandTotals)
    {
        var totalsLookupType = Nested("PivotTotalsLookup");
        return Activator.CreateInstance(
            totalsLookupType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { valuesInRows, rowTotals, rowTotalsByValueIndex, columnTotals, columnTotalsByValueIndex, grandTotals },
            null)!;
    }

    private static IComparer<object?> CreateComparer()
    {
        var comparerType = Nested("PivotValueComparer");
        return (IComparer<object?>)Activator.CreateInstance(comparerType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            new object?[] { CultureInfo.InvariantCulture },
            null)!;
    }

    private static Type Nested(string name)
    {
        return typeof(PivotTableBuilder).GetNestedType(name, BindingFlags.NonPublic)!;
    }
}
