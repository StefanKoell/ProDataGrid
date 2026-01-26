using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotValueFilterModelTests
{
    [Fact]
    public void PivotValueFilterModel_Updates_Field_Filter()
    {
        var field = new PivotAxisField();
        var valueField = new PivotValueField { Header = "Amount" };

        var model = new PivotValueFilterModel
        {
            Field = field,
            ValueField = valueField,
            FilterType = PivotValueFilterType.Top,
            Count = 2,
            Value = 5d,
            Value2 = 7d,
            Percent = 10d
        };

        Assert.NotNull(field.ValueFilter);
        Assert.Equal(PivotValueFilterType.Top, field.ValueFilter!.FilterType);
        Assert.Equal(2, field.ValueFilter.Count);
        Assert.Equal(5d, field.ValueFilter.Value);
        Assert.Equal(7d, field.ValueFilter.Value2);
        Assert.Equal(10d, field.ValueFilter.Percent);
        Assert.Same(valueField, field.ValueFilter.ValueField);

        model.FilterType = PivotValueFilterType.Bottom;

        Assert.Equal(PivotValueFilterType.Bottom, field.ValueFilter.FilterType);
    }

    [Fact]
    public void PivotValueFilterModel_Allows_Null_Field()
    {
        var model = new PivotValueFilterModel
        {
            Field = null,
            FilterType = PivotValueFilterType.None
        };

        Assert.Null(model.Field);
    }
}
