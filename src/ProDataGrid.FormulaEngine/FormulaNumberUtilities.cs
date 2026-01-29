// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Globalization;

namespace ProDataGrid.FormulaEngine
{
    public static class FormulaNumberUtilities
    {
        public static bool TryParse(
            string text,
            FormulaCalculationSettings settings,
            out double number)
        {
            number = 0d;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var culture = settings.Culture ?? CultureInfo.InvariantCulture;
            var styles = NumberStyles.Float | NumberStyles.AllowThousands | NumberStyles.AllowLeadingWhite |
                         NumberStyles.AllowTrailingWhite;
            var trimmed = text.Trim();
            var percentSymbol = culture.NumberFormat.PercentSymbol ?? "%";
            var isPercent = !string.IsNullOrEmpty(percentSymbol) &&
                            trimmed.EndsWith(percentSymbol, StringComparison.Ordinal);
            if (isPercent)
            {
                trimmed = trimmed.Substring(0, trimmed.Length - percentSymbol.Length).Trim();
            }

            if (!double.TryParse(trimmed, styles, culture, out number))
            {
                return false;
            }

            if (isPercent)
            {
                number /= 100d;
            }

            return true;
        }

        public static double ApplyPrecision(double value, int digits)
        {
            if (digits <= 0 || double.IsNaN(value) || double.IsInfinity(value) || value == 0d)
            {
                return value;
            }

            var abs = Math.Abs(value);
            var scale = Math.Pow(10d, Math.Floor(Math.Log10(abs)) + 1);
            if (double.IsInfinity(scale) || scale == 0d)
            {
                return value;
            }

            var rounded = Math.Round(value / scale, digits, MidpointRounding.AwayFromZero) * scale;
            if (Math.Abs(rounded - Math.Round(rounded, 0, MidpointRounding.AwayFromZero)) <= 1e-9)
            {
                return Math.Round(rounded, 0, MidpointRounding.AwayFromZero);
            }

            return rounded;
        }
    }
}
