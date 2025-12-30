// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DataGridSample.Converters
{
    public class ValidationSeverityBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture)
        {
            if (value is DataGridValidationSeverity severity)
            {
                var resourceKey = severity switch
                {
                    DataGridValidationSeverity.Error => "DataGridCellInvalidBrush",
                    DataGridValidationSeverity.Warning => "DataGridCellWarningBrush",
                    DataGridValidationSeverity.Info => "DataGridCellInfoBrush",
                    _ => null
                };

                if (resourceKey != null && Application.Current?.TryFindResource(resourceKey, out var resource) == true)
                {
                    return resource;
                }
            }

            return Brushes.Transparent;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture)
        {
            throw new NotSupportedException();
        }
    }
}
