using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DataGridSample.Converters;

public sealed class FocusIndicatorBrushConverter : IValueConverter
{
    public IBrush FocusedBrush { get; set; } = Brushes.SeaGreen;

    public IBrush UnfocusedBrush { get; set; } = Brushes.Gray;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool isFocused && isFocused ? FocusedBrush : UnfocusedBrush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return AvaloniaProperty.UnsetValue;
    }
}
