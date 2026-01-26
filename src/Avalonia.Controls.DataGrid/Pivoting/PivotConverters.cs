// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Avalonia.Controls.DataGridPivoting
{
    internal sealed class PivotArrayIndexConverter : IValueConverter
    {
        private readonly int _index;
        private readonly IValueConverter? _valueConverter;
        private readonly object? _converterParameter;

        public PivotArrayIndexConverter(int index, IValueConverter? valueConverter = null, object? converterParameter = null)
        {
            _index = index;
            _valueConverter = valueConverter;
            _converterParameter = converterParameter;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not object?[] values || _index < 0 || _index >= values.Length)
            {
                return null;
            }

            var cellValue = values[_index];
            if (_valueConverter != null)
            {
                return _valueConverter.Convert(cellValue, targetType, _converterParameter, culture);
            }

            return cellValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    internal sealed class PivotIndentToThicknessConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double indent)
            {
                return new Thickness(indent, 0, 0, 0);
            }

            return new Thickness(0);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    internal sealed class PivotRowTypeToFontWeightConverter : IValueConverter
    {
        public FontWeight DetailWeight { get; set; } = FontWeight.Normal;

        public FontWeight SubtotalWeight { get; set; } = FontWeight.SemiBold;

        public FontWeight GrandTotalWeight { get; set; } = FontWeight.Bold;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is PivotRowType rowType)
            {
                return rowType switch
                {
                    PivotRowType.Subtotal => SubtotalWeight,
                    PivotRowType.GrandTotal => GrandTotalWeight,
                    _ => DetailWeight
                };
            }

            return DetailWeight;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
