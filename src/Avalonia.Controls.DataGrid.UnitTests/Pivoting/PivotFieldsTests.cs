using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.DataGridPivoting;
using Avalonia.Data.Converters;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotFieldsTests
{
    private sealed class Sample
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }

        public Sample? Child { get; set; }
    }

    private sealed class UpperConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString()?.ToUpperInvariant();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [Fact]
    public void GetValue_Uses_ValueSelector_Over_Binding()
    {
        var field = new PivotAxisField
        {
            ValueSelector = item => "selector",
            Binding = DataGridBindingDefinition.Create<Sample, string>(x => x.Name)
        };

        var value = field.GetValue(new Sample { Name = "binding" });

        Assert.Equal("selector", value);
    }

    [Fact]
    public void GetValue_Uses_Binding_When_No_Selector()
    {
        var field = new PivotAxisField
        {
            Binding = DataGridBindingDefinition.Create<Sample, string>(x => x.Name)
        };

        var value = field.GetValue(new Sample { Name = "binding" });

        Assert.Equal("binding", value);
    }

    [Fact]
    public void GetValue_Uses_PropertyPath_When_No_Selector_Or_Binding()
    {
        var field = new PivotAxisField
        {
            PropertyPath = "Child.Name"
        };

        var value = field.GetValue(new Sample { Child = new Sample { Name = "path" } });

        Assert.Equal("path", value);
    }

    [Fact]
    public void GetValue_Returns_Null_For_Null_Item()
    {
        var field = new PivotAxisField
        {
            PropertyPath = "Name"
        };

        Assert.Null(field.GetValue(null));
    }

    [Fact]
    public void GetValue_Returns_Null_When_No_Source_Configured()
    {
        var field = new PivotAxisField();

        Assert.Null(field.GetValue(new Sample()));
    }

    [Fact]
    public void GetGroupValue_Uses_GroupSelector()
    {
        var field = new PivotAxisField
        {
            ValueSelector = item => ((Sample)item!).Count,
            GroupSelector = value => value is int count ? count * 2 : null
        };

        var value = field.GetGroupValue(new Sample { Count = 4 });

        Assert.Equal(8, value);
    }

    [Fact]
    public void Binding_Sets_ValueType_When_Not_Provided()
    {
        var field = new PivotAxisField
        {
            Binding = DataGridBindingDefinition.Create<Sample, int>(x => x.Count)
        };

        Assert.Equal(typeof(int), field.ValueType);
    }

    [Fact]
    public void FormatValue_Uses_Null_Labels_And_Converters()
    {
        var field = new PivotAxisField
        {
            NullLabel = "n/a",
            Converter = new UpperConverter()
        };

        Assert.Equal("n/a", field.FormatValue(null, CultureInfo.InvariantCulture, "empty"));
        Assert.Equal("TEXT", field.FormatValue("text", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void FormatValue_Uses_StringFormat_And_Falls_Back_On_Errors()
    {
        var field = new PivotAxisField
        {
            StringFormat = "{0:0.0}",
            FormatProvider = CultureInfo.InvariantCulture
        };

        Assert.Equal("1.2", field.FormatValue(1.23, CultureInfo.InvariantCulture));

        field.StringFormat = "N2";
        Assert.Equal("2.00", field.FormatValue(2d, CultureInfo.InvariantCulture));

        field.StringFormat = "{0";
        Assert.Equal("3", field.FormatValue(3, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void FormatValue_NonFormattable_StringFormat_Falls_Back()
    {
        var field = new PivotAxisField
        {
            StringFormat = "N2"
        };
        var value = new object();

        Assert.Equal(value.ToString(), field.FormatValue(value, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void FormatValue_Uses_IFormattable_Path()
    {
        var field = new PivotAxisField
        {
            StringFormat = "D2",
            FormatProvider = CultureInfo.InvariantCulture
        };

        Assert.Equal("05", field.FormatValue(5, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void FormatValue_Uses_IFormattable_Path_With_Numeric_Format()
    {
        var field = new PivotAxisField
        {
            StringFormat = "N2"
        };

        Assert.Equal("1.50", field.FormatValue(1.5m, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void AxisField_Raises_PropertyChanged_For_Filter_Updates()
    {
        var field = new PivotAxisField();
        var raised = 0;
        field.PropertyChanged += (_, _) => raised++;

        field.Filter = new PivotFieldFilter();
        field.ValueFilter = new PivotValueFilter();
        field.ValueSort = new PivotValueSort();

        Assert.Equal(3, raised);
    }

    [Fact]
    public void AxisField_Propagates_Filter_Changes()
    {
        var field = new PivotAxisField();
        var raised = 0;
        field.PropertyChanged += (_, _) => raised++;

        var filter = new PivotFieldFilter();
        field.Filter = filter;
        filter.Included.Add("A");

        Assert.True(raised >= 2);
    }

    [Fact]
    public void AxisField_PropertySetters_Handle_NoChange_And_Unsubscribe()
    {
        var field = new PivotAxisField();

        var filter = new PivotFieldFilter();
        var valueFilter = new PivotValueFilter();
        var valueSort = new PivotValueSort();
        field.Filter = filter;
        field.ValueFilter = valueFilter;
        field.ValueSort = valueSort;

        field.Filter = filter;
        field.ValueFilter = valueFilter;
        field.ValueSort = valueSort;

        field.ShowSubtotals = false;
        field.ValueSort = new PivotValueSort();
    }

    [Fact]
    public void AxisField_ValueSort_Change_Raises_PropertyChanged()
    {
        var field = new PivotAxisField();
        var raised = 0;
        field.PropertyChanged += (_, _) => raised++;

        var valueSort = new PivotValueSort();
        field.ValueSort = valueSort;

        valueSort.SortDirection = System.ComponentModel.ListSortDirection.Ascending;

        Assert.True(raised >= 2);
    }

    [Fact]
    public void ValueField_Allows_Custom_Aggregator()
    {
        var field = new PivotValueField
        {
            CustomAggregator = new SumAggregator()
        };

        Assert.NotNull(field.CustomAggregator);
    }

    [Fact]
    public void PivotFieldFilter_Defaults_And_SetOperations_Cover_Branches()
    {
        var filter = new PivotFieldFilter();

        Assert.True(filter.IsMatch("x"));
        Assert.False(filter.Included.IsReadOnly);

        var predicate = new Func<object?, bool>(_ => true);
        filter.Predicate = predicate;
        filter.Predicate = predicate;

        var includeSet = (System.Collections.Generic.ICollection<object?>)filter.Included;
        includeSet.Add("A");
        includeSet.Clear();
        includeSet.Clear();

        filter.Included.Add("A");
        filter.Included.Add("B");
        filter.Included.IntersectWith(new[] { "A" });
        filter.Included.SymmetricExceptWith(new[] { "A" });

        _ = filter.Included.GetEnumerator();
        _ = ((System.Collections.IEnumerable)filter.Included).GetEnumerator();
    }

    [Fact]
    public void ValueField_IsCalculated_When_Formula_Set()
    {
        var field = new PivotValueField { Formula = "Amount / 2" };

        Assert.True(field.IsCalculated);
    }
}
