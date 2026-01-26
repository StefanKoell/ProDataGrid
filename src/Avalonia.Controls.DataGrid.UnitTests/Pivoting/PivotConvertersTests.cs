using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls.DataGridPivoting;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotConvertersTests
{
    private sealed class PrefixConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var prefix = parameter?.ToString() ?? string.Empty;
            return string.Concat(prefix, value?.ToString() ?? string.Empty);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [Fact]
    public void PivotArrayIndexConverter_Handles_Invalid_Input()
    {
        var converter = new PivotArrayIndexConverter(1);

        Assert.Null(converter.Convert("not-array", typeof(object), null, CultureInfo.InvariantCulture));
        Assert.Null(converter.Convert(new object?[] { 1 }, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void PivotArrayIndexConverter_Delegates_To_Inner_Converter()
    {
        var inner = new PrefixConverter();
        var converter = new PivotArrayIndexConverter(0, inner, "x=");

        var result = converter.Convert(new object?[] { 5 }, typeof(string), null, CultureInfo.InvariantCulture);

        Assert.Equal("x=5", result);
    }

    [Fact]
    public void PivotArrayIndexConverter_Returns_Cell_Value_When_No_Inner_Converter()
    {
        var converter = new PivotArrayIndexConverter(0);

        var result = converter.Convert(new object?[] { "cell" }, typeof(object), null, CultureInfo.InvariantCulture);

        Assert.Equal("cell", result);
    }

    [Fact]
    public void PivotArrayIndexConverter_ConvertBack_Throws()
    {
        var converter = new PivotArrayIndexConverter(0);

        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack(1, typeof(object), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void PivotIndentToThicknessConverter_Handles_Values()
    {
        var converter = new PivotIndentToThicknessConverter();

        Assert.Equal(new Thickness(8, 0, 0, 0),
            converter.Convert(8d, typeof(Thickness), null, CultureInfo.InvariantCulture));

        Assert.Equal(new Thickness(0),
            converter.Convert("not-double", typeof(Thickness), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void PivotIndentToThicknessConverter_ConvertBack_Throws()
    {
        var converter = new PivotIndentToThicknessConverter();

        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack(new Thickness(0), typeof(double), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void PivotRowTypeToFontWeightConverter_Maps_Row_Types()
    {
        var converter = new PivotRowTypeToFontWeightConverter
        {
            DetailWeight = FontWeight.Thin,
            SubtotalWeight = FontWeight.Bold,
            GrandTotalWeight = FontWeight.ExtraBold
        };

        Assert.Equal(FontWeight.Bold,
            converter.Convert(PivotRowType.Subtotal, typeof(FontWeight), null, CultureInfo.InvariantCulture));
        Assert.Equal(FontWeight.ExtraBold,
            converter.Convert(PivotRowType.GrandTotal, typeof(FontWeight), null, CultureInfo.InvariantCulture));
        Assert.Equal(FontWeight.Thin,
            converter.Convert(PivotRowType.Detail, typeof(FontWeight), null, CultureInfo.InvariantCulture));
        Assert.Equal(FontWeight.Thin,
            converter.Convert("not-row", typeof(FontWeight), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void PivotRowTypeToFontWeightConverter_ConvertBack_Throws()
    {
        var converter = new PivotRowTypeToFontWeightConverter();

        Assert.Throws<NotSupportedException>(() =>
            converter.ConvertBack(FontWeight.Bold, typeof(PivotRowType), null, CultureInfo.InvariantCulture));
    }
}
