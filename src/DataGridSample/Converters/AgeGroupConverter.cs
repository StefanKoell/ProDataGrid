using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DataGridSample.Converters
{
    public sealed class AgeGroupConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int age)
            {
                if (age < 18)
                {
                    return "Under 18";
                }

                if (age < 30)
                {
                    return "18-29";
                }

                if (age < 45)
                {
                    return "30-44";
                }

                if (age < 60)
                {
                    return "45-59";
                }

                return "60+";
            }

            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value ?? string.Empty;
        }
    }
}
