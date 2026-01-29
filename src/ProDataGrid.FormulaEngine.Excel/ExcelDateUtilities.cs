// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Globalization;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal static class ExcelDateUtilities
    {
        private static readonly DateTime Epoch1900 = new DateTime(1899, 12, 31);
        private static readonly DateTime Epoch1904 = new DateTime(1904, 1, 1);
        private static readonly DateTime LeapBugThreshold = new DateTime(1900, 3, 1);

        public static bool TryCreateSerialFromDate(
            int year,
            int month,
            int day,
            FormulaDateSystem dateSystem,
            out double serial,
            out FormulaError error)
        {
            error = default;
            serial = 0;

            if (year >= 0 && year < 1900)
            {
                year += 1900;
            }

            if (dateSystem == FormulaDateSystem.Windows1900 &&
                year == 1900 &&
                month == 2 &&
                day == 29)
            {
                serial = 60;
                return true;
            }

            try
            {
                var baseDate = new DateTime(year, 1, 1);
                var date = baseDate.AddMonths(month - 1).AddDays(day - 1);
                var epoch = dateSystem == FormulaDateSystem.Windows1900 ? Epoch1900 : Epoch1904;
                serial = (date - epoch).TotalDays;

                if (dateSystem == FormulaDateSystem.Windows1900 && date >= LeapBugThreshold)
                {
                    serial += 1;
                }

                if (serial < 0)
                {
                    error = new FormulaError(FormulaErrorType.Num);
                    return false;
                }

                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                error = new FormulaError(FormulaErrorType.Num);
                return false;
            }
        }

        public static bool TryCreateSerialFromDateTime(
            DateTime value,
            FormulaDateSystem dateSystem,
            out double serial,
            out FormulaError error)
        {
            if (!TryCreateSerialFromDate(value.Year, value.Month, value.Day, dateSystem, out serial, out error))
            {
                return false;
            }

            serial += value.TimeOfDay.TotalDays;
            return true;
        }

        public static bool TryGetDateParts(
            double serial,
            FormulaDateSystem dateSystem,
            out int year,
            out int month,
            out int day,
            out FormulaError error)
        {
            year = 0;
            month = 0;
            day = 0;
            error = default;

            if (double.IsNaN(serial) || double.IsInfinity(serial) || serial < 0)
            {
                error = new FormulaError(FormulaErrorType.Num);
                return false;
            }

            var days = (int)Math.Floor(serial);
            if (dateSystem == FormulaDateSystem.Windows1900)
            {
                if (days == 60)
                {
                    year = 1900;
                    month = 2;
                    day = 29;
                    return true;
                }

                if (days > 60)
                {
                    days -= 1;
                }

                var date = Epoch1900.AddDays(days);
                year = date.Year;
                month = date.Month;
                day = date.Day;
                return true;
            }

            var date1904 = Epoch1904.AddDays(days);
            year = date1904.Year;
            month = date1904.Month;
            day = date1904.Day;
            return true;
        }

        public static bool TryGetTimeParts(
            double serial,
            out int hour,
            out int minute,
            out int second,
            out FormulaError error)
        {
            hour = 0;
            minute = 0;
            second = 0;
            error = default;

            if (double.IsNaN(serial) || double.IsInfinity(serial) || serial < 0)
            {
                error = new FormulaError(FormulaErrorType.Num);
                return false;
            }

            var fraction = serial - Math.Floor(serial);
            if (fraction < 0)
            {
                error = new FormulaError(FormulaErrorType.Num);
                return false;
            }

            var totalSeconds = (int)Math.Floor((fraction * 86400d) + 1e-7);
            hour = totalSeconds / 3600;
            minute = (totalSeconds % 3600) / 60;
            second = totalSeconds % 60;
            return true;
        }

        public static bool TryParseDateText(
            string text,
            FormulaDateSystem dateSystem,
            CultureInfo culture,
            out double serial,
            out FormulaError error)
        {
            if (DateTime.TryParse(
                text,
                culture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out var dateTime))
            {
                return TryCreateSerialFromDate(dateTime.Year, dateTime.Month, dateTime.Day, dateSystem, out serial, out error);
            }

            serial = 0;
            error = new FormulaError(FormulaErrorType.Value);
            return false;
        }

        public static bool TryParseTimeText(
            string text,
            CultureInfo culture,
            out double serial,
            out FormulaError error)
        {
            if (DateTime.TryParse(
                text,
                culture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeLocal,
                out var dateTime))
            {
                serial = dateTime.TimeOfDay.TotalDays;
                error = default;
                return true;
            }

            serial = 0;
            error = new FormulaError(FormulaErrorType.Value);
            return false;
        }
    }
}
