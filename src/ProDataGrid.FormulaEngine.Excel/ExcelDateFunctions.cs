// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal sealed class DateFunction : ExcelFunctionBase
    {
        public DateFunction()
            : base("DATE", new FormulaFunctionInfo(3, 3))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var settings = context.EvaluationContext.Workbook.Settings;
            return ExcelFunctionUtilities.ApplyTernary(args[0], args[1], args[2], (yearValue, monthValue, dayValue) =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToInteger(context, yearValue, out var year, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelFunctionUtilities.TryCoerceToInteger(context, monthValue, out var month, out error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelFunctionUtilities.TryCoerceToInteger(context, dayValue, out var day, out error))
                {
                    return FormulaValue.FromError(error);
                }

                if (settings.DateSystem == FormulaDateSystem.Windows1900 &&
                    year == 1900 &&
                    month == 2 &&
                    day == 29)
                {
                    return ExcelFunctionUtilities.CreateNumber(settings, 60);
                }

                if (!ExcelDateUtilities.TryCreateSerialFromDate(year, month, day, settings.DateSystem, out var serial, out error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(settings, serial);
            });
        }
    }

    internal sealed class TimeFunction : ExcelFunctionBase
    {
        public TimeFunction()
            : base("TIME", new FormulaFunctionInfo(3, 3))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyTernary(args[0], args[1], args[2], (hourValue, minuteValue, secondValue) =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToInteger(context, hourValue, out var hour, out var error) ||
                    !ExcelFunctionUtilities.TryCoerceToInteger(context, minuteValue, out var minute, out error) ||
                    !ExcelFunctionUtilities.TryCoerceToInteger(context, secondValue, out var second, out error))
                {
                    return FormulaValue.FromError(error);
                }

                if (hour < 0 || minute < 0 || second < 0)
                {
                    return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
                }

                var totalSeconds = (hour * 3600d) + (minute * 60d) + second;
                return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, totalSeconds / 86400d);
            });
        }
    }

    internal sealed class DateValueFunction : ExcelFunctionBase
    {
        public DateValueFunction()
            : base("DATEVALUE", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelDateFunctionHelpers.TryGetDateSerial(value, context.EvaluationContext.Workbook.Settings, out var serial, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, serial);
            });
        }
    }

    internal sealed class TimeValueFunction : ExcelFunctionBase
    {
        public TimeValueFunction()
            : base("TIMEVALUE", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelDateFunctionHelpers.TryGetTimeSerial(value, context.EvaluationContext.Workbook.Settings, out var serial, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, serial);
            });
        }
    }

    internal sealed class YearFunction : ExcelFunctionBase
    {
        public YearFunction()
            : base("YEAR", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelDateFunctionHelpers.TryGetDateSerial(value, context.EvaluationContext.Workbook.Settings, out var serial, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelDateUtilities.TryGetDateParts(serial, context.EvaluationContext.Workbook.Settings.DateSystem, out var year, out _, out _, out error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, year);
            });
        }
    }

    internal sealed class MonthFunction : ExcelFunctionBase
    {
        public MonthFunction()
            : base("MONTH", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelDateFunctionHelpers.TryGetDateSerial(value, context.EvaluationContext.Workbook.Settings, out var serial, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelDateUtilities.TryGetDateParts(serial, context.EvaluationContext.Workbook.Settings.DateSystem, out _, out var month, out _, out error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, month);
            });
        }
    }

    internal sealed class DayFunction : ExcelFunctionBase
    {
        public DayFunction()
            : base("DAY", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var settings = context.EvaluationContext.Workbook.Settings;
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelDateFunctionHelpers.TryGetDateSerial(value, settings, out var serial, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelDateUtilities.TryGetDateParts(serial, settings.DateSystem, out _, out _, out var day, out error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(settings, day);
            });
        }
    }

    internal sealed class HourFunction : ExcelFunctionBase
    {
        public HourFunction()
            : base("HOUR", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelDateFunctionHelpers.TryGetTimeSerial(value, context.EvaluationContext.Workbook.Settings, out var serial, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelDateUtilities.TryGetTimeParts(serial, out var hour, out _, out _, out error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, hour);
            });
        }
    }

    internal sealed class MinuteFunction : ExcelFunctionBase
    {
        public MinuteFunction()
            : base("MINUTE", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelDateFunctionHelpers.TryGetTimeSerial(value, context.EvaluationContext.Workbook.Settings, out var serial, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelDateUtilities.TryGetTimeParts(serial, out _, out var minute, out _, out error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, minute);
            });
        }
    }

    internal sealed class SecondFunction : ExcelFunctionBase
    {
        public SecondFunction()
            : base("SECOND", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelDateFunctionHelpers.TryGetTimeSerial(value, context.EvaluationContext.Workbook.Settings, out var serial, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelDateUtilities.TryGetTimeParts(serial, out _, out _, out var second, out error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, second);
            });
        }
    }

    internal sealed class TodayFunction : ExcelFunctionBase
    {
        public TodayFunction()
            : base("TODAY", new FormulaFunctionInfo(0, 0, isVolatile: true))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var now = DateTime.Now;
            if (!ExcelDateUtilities.TryCreateSerialFromDate(
                    now.Year,
                    now.Month,
                    now.Day,
                    context.EvaluationContext.Workbook.Settings.DateSystem,
                    out var serial,
                    out var error))
            {
                return FormulaValue.FromError(error);
            }

            return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, serial);
        }
    }

    internal sealed class NowFunction : ExcelFunctionBase
    {
        public NowFunction()
            : base("NOW", new FormulaFunctionInfo(0, 0, isVolatile: true))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var now = DateTime.Now;
            if (!ExcelDateUtilities.TryCreateSerialFromDateTime(
                    now,
                    context.EvaluationContext.Workbook.Settings.DateSystem,
                    out var serial,
                    out var error))
            {
                return FormulaValue.FromError(error);
            }

            return ExcelFunctionUtilities.CreateNumber(context.EvaluationContext.Workbook.Settings, serial);
        }
    }

    internal sealed class EoMonthFunction : ExcelFunctionBase
    {
        public EoMonthFunction()
            : base("EOMONTH", new FormulaFunctionInfo(2, 2))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var settings = context.EvaluationContext.Workbook.Settings;
            return ExcelFunctionUtilities.ApplyBinary(args[0], args[1], (startValue, monthsValue) =>
            {
                if (!ExcelDateFunctionHelpers.TryGetDateSerial(startValue, settings, out var serial, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelFunctionUtilities.TryCoerceToInteger(context, monthsValue, out var months, out error))
                {
                    return FormulaValue.FromError(error);
                }

                if (!ExcelDateUtilities.TryGetDateParts(serial, settings.DateSystem, out var year, out var month, out _, out error))
                {
                    return FormulaValue.FromError(error);
                }

                var baseDate = new DateTime(year, month, 1);
                var targetDate = baseDate.AddMonths(months + 1).AddDays(-1);
                if (!ExcelDateUtilities.TryCreateSerialFromDate(
                        targetDate.Year,
                        targetDate.Month,
                        targetDate.Day,
                        settings.DateSystem,
                        out var resultSerial,
                        out error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(settings, resultSerial);
            });
        }
    }

    internal sealed class WorkdayFunction : ExcelFunctionBase
    {
        public WorkdayFunction()
            : base("WORKDAY", new FormulaFunctionInfo(2, 3))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var settings = context.EvaluationContext.Workbook.Settings;

            if (!ExcelDateFunctionHelpers.TryGetDateSerial(args[0], settings, out var startSerial, out var error))
            {
                return FormulaValue.FromError(error);
            }

            if (!ExcelFunctionUtilities.TryCoerceToInteger(context, args[1], out var days, out error))
            {
                return FormulaValue.FromError(error);
            }

            var holidays = ExcelDateFunctionHelpers.GetHolidaySet(args.Count > 2 ? args[2] : null, settings, out error);
            if (holidays == null)
            {
                return FormulaValue.FromError(error);
            }

            var step = days >= 0 ? 1 : -1;
            var remaining = Math.Abs(days);
            var current = startSerial;
            while (remaining > 0)
            {
                current += step;
                if (ExcelDateFunctionHelpers.IsWorkday(current, settings.DateSystem, holidays))
                {
                    remaining--;
                }
            }

            return ExcelFunctionUtilities.CreateNumber(settings, current);
        }
    }

    internal sealed class NetworkDaysFunction : ExcelFunctionBase
    {
        public NetworkDaysFunction()
            : base("NETWORKDAYS", new FormulaFunctionInfo(2, 3))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var settings = context.EvaluationContext.Workbook.Settings;

            if (!ExcelDateFunctionHelpers.TryGetDateSerial(args[0], settings, out var startSerial, out var error))
            {
                return FormulaValue.FromError(error);
            }

            if (!ExcelDateFunctionHelpers.TryGetDateSerial(args[1], settings, out var endSerial, out error))
            {
                return FormulaValue.FromError(error);
            }

            var holidays = ExcelDateFunctionHelpers.GetHolidaySet(args.Count > 2 ? args[2] : null, settings, out error);
            if (holidays == null)
            {
                return FormulaValue.FromError(error);
            }

            var swapped = false;
            if (startSerial > endSerial)
            {
                (startSerial, endSerial) = (endSerial, startSerial);
                swapped = true;
            }

            var startDay = (int)Math.Floor(startSerial);
            var endDay = (int)Math.Floor(endSerial);
            var count = 0;
            for (var day = startDay; day <= endDay; day++)
            {
                if (ExcelDateFunctionHelpers.IsWorkday(day, settings.DateSystem, holidays))
                {
                    count++;
                }
            }

            if (swapped)
            {
                count = -count;
            }

            return ExcelFunctionUtilities.CreateNumber(settings, count);
        }
    }

    internal static class ExcelDateFunctionHelpers
    {
        public static bool TryGetDateSerial(
            FormulaValue value,
            FormulaCalculationSettings settings,
            out double serial,
            out FormulaError error)
        {
            if (value.Kind == FormulaValueKind.Error)
            {
                serial = 0;
                error = value.AsError();
                return false;
            }

            if (value.Kind == FormulaValueKind.Text)
            {
                return ExcelDateUtilities.TryParseDateText(value.AsText(), settings.DateSystem, settings.Culture, out serial, out error);
            }

            if (!ExcelFunctionUtilities.TryCoerceToNumber(settings, value, out serial, out error))
            {
                return false;
            }

            if (serial < 0)
            {
                error = new FormulaError(FormulaErrorType.Num);
                return false;
            }

            return true;
        }

        public static bool TryGetTimeSerial(
            FormulaValue value,
            FormulaCalculationSettings settings,
            out double serial,
            out FormulaError error)
        {
            if (value.Kind == FormulaValueKind.Error)
            {
                serial = 0;
                error = value.AsError();
                return false;
            }

            if (value.Kind == FormulaValueKind.Text)
            {
                return ExcelDateUtilities.TryParseTimeText(value.AsText(), settings.Culture, out serial, out error);
            }

            if (!ExcelFunctionUtilities.TryCoerceToNumber(settings, value, out serial, out error))
            {
                return false;
            }

            if (serial < 0)
            {
                error = new FormulaError(FormulaErrorType.Num);
                return false;
            }

            serial -= Math.Floor(serial);
            return true;
        }

        public static HashSet<int>? GetHolidaySet(
            FormulaValue? holidaysValue,
            FormulaCalculationSettings settings,
            out FormulaError error)
        {
            error = default;
            var holidays = new HashSet<int>();
            if (holidaysValue == null)
            {
                return holidays;
            }

            foreach (var value in ExcelFunctionUtilities.FlattenValues(holidaysValue.Value))
            {
                if (value.Kind == FormulaValueKind.Blank)
                {
                    continue;
                }

                if (!TryGetDateSerial(value, settings, out var serial, out error))
                {
                    return null;
                }

                holidays.Add((int)Math.Floor(serial));
            }

            return holidays;
        }

        public static bool IsWorkday(double serial, FormulaDateSystem dateSystem, HashSet<int> holidays)
        {
            var day = (int)Math.Floor(serial);
            if (day < 0)
            {
                return false;
            }

            if (holidays.Contains(day))
            {
                return false;
            }

            var dayOfWeek = GetDayOfWeek(day, dateSystem);
            return dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday;
        }

        private static DayOfWeek GetDayOfWeek(int daySerial, FormulaDateSystem dateSystem)
        {
            if (dateSystem == FormulaDateSystem.Windows1900)
            {
                if (daySerial == 0)
                {
                    return DayOfWeek.Sunday;
                }

                if (daySerial > 60)
                {
                    daySerial -= 1;
                }

                var index = (daySerial - 1) % 7;
                if (index < 0)
                {
                    index += 7;
                }

                return IndexToDayOfWeek(index);
            }

            var offsetIndex = (daySerial + 4) % 7;
            if (offsetIndex < 0)
            {
                offsetIndex += 7;
            }

            return IndexToDayOfWeek(offsetIndex);
        }

        private static DayOfWeek IndexToDayOfWeek(int index)
        {
            return index switch
            {
                0 => DayOfWeek.Monday,
                1 => DayOfWeek.Tuesday,
                2 => DayOfWeek.Wednesday,
                3 => DayOfWeek.Thursday,
                4 => DayOfWeek.Friday,
                5 => DayOfWeek.Saturday,
                _ => DayOfWeek.Sunday
            };
        }
    }
}
