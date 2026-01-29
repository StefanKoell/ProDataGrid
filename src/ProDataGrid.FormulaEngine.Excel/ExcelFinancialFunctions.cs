// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal sealed class PvFunction : ExcelFunctionBase
    {
        public PvFunction()
            : base("PV", new FormulaFunctionInfo(3, 5))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            if (!ExcelFinancialUtilities.TryReadRateArgs(context.EvaluationContext.Workbook.Settings, args, out var rate, out var nper, out var pmt, out var fv, out var type, out var error))
            {
                return FormulaValue.FromError(error);
            }

            if (rate == 0d)
            {
                return ExcelFunctionUtilities.CreateNumber(context, -(fv + (pmt * nper)));
            }

            var pow = Math.Pow(1d + rate, nper);
            var pv = -(fv + (pmt * (1d + rate * type) * (pow - 1d) / rate)) / pow;
            return ExcelFunctionUtilities.CreateNumber(context, pv);
        }
    }

    internal sealed class FvFunction : ExcelFunctionBase
    {
        public FvFunction()
            : base("FV", new FormulaFunctionInfo(3, 5))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            if (!ExcelFinancialUtilities.TryReadRateArgs(context.EvaluationContext.Workbook.Settings, args, out var rate, out var nper, out var pmt, out var pv, out var type, out var error))
            {
                return FormulaValue.FromError(error);
            }

            if (rate == 0d)
            {
                return ExcelFunctionUtilities.CreateNumber(context, -(pv + (pmt * nper)));
            }

            var pow = Math.Pow(1d + rate, nper);
            var fv = -(pv * pow + (pmt * (1d + rate * type) * (pow - 1d) / rate));
            return ExcelFunctionUtilities.CreateNumber(context, fv);
        }
    }

    internal sealed class PmtFunction : ExcelFunctionBase
    {
        public PmtFunction()
            : base("PMT", new FormulaFunctionInfo(3, 5))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            if (!ExcelFinancialUtilities.TryReadRateArgs(context.EvaluationContext.Workbook.Settings, args, out var rate, out var nper, out var pv, out var fv, out var type, out var error))
            {
                return FormulaValue.FromError(error);
            }

            if (nper == 0d)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Div0));
            }

            if (rate == 0d)
            {
                return ExcelFunctionUtilities.CreateNumber(context, -(pv + fv) / nper);
            }

            var pow = Math.Pow(1d + rate, nper);
            var pmt = -(pv * pow + fv) * rate / ((1d + rate * type) * (pow - 1d));
            return ExcelFunctionUtilities.CreateNumber(context, pmt);
        }
    }

    internal sealed class NperFunction : ExcelFunctionBase
    {
        public NperFunction()
            : base("NPER", new FormulaFunctionInfo(3, 5))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            if (!ExcelFunctionUtilities.TryCoerceToNumber(context, args[0], out var rate, out var error) ||
                !ExcelFunctionUtilities.TryCoerceToNumber(context, args[1], out var pmt, out error) ||
                !ExcelFunctionUtilities.TryCoerceToNumber(context, args[2], out var pv, out error))
            {
                return FormulaValue.FromError(error);
            }

            var fv = 0d;
            var type = 0d;
            if (args.Count > 3)
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, args[3], out fv, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            if (args.Count > 4)
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, args[4], out type, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            if (pmt == 0d && rate == 0d)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Div0));
            }

            if (rate == 0d)
            {
                return ExcelFunctionUtilities.CreateNumber(context, -(pv + fv) / pmt);
            }

            var adjusted = pmt * (1d + rate * type) / rate;
            var numerator = adjusted - fv;
            var denominator = adjusted + pv;
            if (denominator == 0d || numerator <= 0d || denominator <= 0d)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
            }

            var nper = Math.Log(numerator / denominator) / Math.Log(1d + rate);
            return ExcelFunctionUtilities.CreateNumber(context, nper);
        }
    }

    internal sealed class RateFunction : ExcelFunctionBase
    {
        public RateFunction()
            : base("RATE", new FormulaFunctionInfo(3, 6))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            if (!ExcelFunctionUtilities.TryCoerceToNumber(context, args[0], out var nper, out var error) ||
                !ExcelFunctionUtilities.TryCoerceToNumber(context, args[1], out var pmt, out error) ||
                !ExcelFunctionUtilities.TryCoerceToNumber(context, args[2], out var pv, out error))
            {
                return FormulaValue.FromError(error);
            }

            var fv = 0d;
            var type = 0d;
            if (args.Count > 3)
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, args[3], out fv, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            if (args.Count > 4)
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, args[4], out type, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            var guess = 0.1d;
            if (args.Count > 5)
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, args[5], out guess, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            if (nper <= 0d)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
            }

            if (ExcelFinancialUtilities.TrySolveRate(nper, pmt, pv, fv, type, guess, out var rate))
            {
                return ExcelFunctionUtilities.CreateNumber(context, rate);
            }

            return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
        }
    }

    internal sealed class NpvFunction : ExcelFunctionBase
    {
        public NpvFunction()
            : base("NPV", new FormulaFunctionInfo(2, -1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            if (!ExcelFunctionUtilities.TryCoerceToNumber(context, args[0], out var rate, out var error))
            {
                return FormulaValue.FromError(error);
            }

            var sum = 0d;
            var period = 0;
            for (var i = 1; i < args.Count; i++)
            {
                var arg = args[i];
                if (arg.Kind == FormulaValueKind.Array)
                {
                    foreach (var value in arg.AsArray().Flatten())
                    {
                        if (!ExcelFinancialUtilities.TryReadCashFlow(value, out var cash, out error))
                        {
                            return FormulaValue.FromError(error);
                        }

                        period++;
                        sum += cash / Math.Pow(1d + rate, period);
                    }
                    continue;
                }

                if (!ExcelFinancialUtilities.TryReadScalarCashFlow(context.EvaluationContext.Workbook.Settings, arg, out var scalarCash, out error))
                {
                    return FormulaValue.FromError(error);
                }

                period++;
                sum += scalarCash / Math.Pow(1d + rate, period);
            }

            return ExcelFunctionUtilities.CreateNumber(context, sum);
        }
    }

    internal sealed class IrrFunction : ExcelFunctionBase
    {
        public IrrFunction()
            : base("IRR", new FormulaFunctionInfo(1, 2))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var values = new List<double>();
            var error = default(FormulaError);

            var input = args[0];
            if (input.Kind == FormulaValueKind.Array)
            {
                foreach (var value in input.AsArray().Flatten())
                {
                    if (!ExcelFinancialUtilities.TryReadCashFlow(value, out var cash, out error))
                    {
                        return FormulaValue.FromError(error);
                    }

                    values.Add(cash);
                }
            }
            else
            {
                if (!ExcelFinancialUtilities.TryReadScalarCashFlow(context.EvaluationContext.Workbook.Settings, input, out var cash, out error))
                {
                    return FormulaValue.FromError(error);
                }
                values.Add(cash);
            }

            if (values.Count == 0)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
            }

            var guess = 0.1d;
            if (args.Count > 1)
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, args[1], out guess, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            var hasPositive = false;
            var hasNegative = false;
            for (var i = 0; i < values.Count; i++)
            {
                if (values[i] > 0d)
                {
                    hasPositive = true;
                }
                else if (values[i] < 0d)
                {
                    hasNegative = true;
                }
            }

            if (!hasPositive || !hasNegative)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
            }

            if (ExcelFinancialUtilities.TrySolveIrr(values, guess, out var irr))
            {
                return ExcelFunctionUtilities.CreateNumber(context, irr);
            }

            return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
        }
    }

    internal static class ExcelFinancialUtilities
    {
        public static bool TryReadRateArgs(
            FormulaCalculationSettings settings,
            IReadOnlyList<FormulaValue> args,
            out double rate,
            out double nper,
            out double pmt,
            out double fv,
            out double type,
            out FormulaError error,
            int startIndex = 0)
        {
            rate = 0;
            nper = 0;
            pmt = 0;
            fv = 0;
            type = 0;
            error = default;

            if (!ExcelFunctionUtilities.TryCoerceToNumber(settings, args[startIndex], out rate, out error) ||
                !ExcelFunctionUtilities.TryCoerceToNumber(settings, args[startIndex + 1], out nper, out error) ||
                !ExcelFunctionUtilities.TryCoerceToNumber(settings, args[startIndex + 2], out pmt, out error))
            {
                return false;
            }

            if (args.Count > startIndex + 3)
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(settings, args[startIndex + 3], out fv, out error))
                {
                    return false;
                }
            }

            if (args.Count > startIndex + 4)
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(settings, args[startIndex + 4], out type, out error))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool TryReadCashFlow(FormulaValue value, out double cash, out FormulaError error)
        {
            cash = 0;
            error = default;

            if (value.Kind == FormulaValueKind.Error)
            {
                error = value.AsError();
                return false;
            }

            if (value.Kind == FormulaValueKind.Blank)
            {
                cash = 0d;
                return true;
            }

            if (value.Kind == FormulaValueKind.Number)
            {
                cash = value.AsNumber();
                return true;
            }

            return true;
        }

        public static bool TryReadScalarCashFlow(
            FormulaCalculationSettings settings,
            FormulaValue value,
            out double cash,
            out FormulaError error)
        {
            cash = 0;
            if (!ExcelFunctionUtilities.TryCoerceToNumber(settings, value, out cash, out error))
            {
                return false;
            }

            return true;
        }

        public static bool TrySolveIrr(IReadOnlyList<double> values, double guess, out double irr)
        {
            irr = guess;
            var rate = guess;
            for (var iteration = 0; iteration < 100; iteration++)
            {
                var npv = 0d;
                var derivative = 0d;
                for (var i = 0; i < values.Count; i++)
                {
                    var denom = Math.Pow(1d + rate, i + 1);
                    npv += values[i] / denom;
                    derivative -= (i + 1) * values[i] / (denom * (1d + rate));
                }

                if (Math.Abs(derivative) < 1e-12)
                {
                    break;
                }

                var next = rate - (npv / derivative);
                if (double.IsNaN(next) || double.IsInfinity(next))
                {
                    break;
                }

                if (Math.Abs(next - rate) < 1e-8)
                {
                    irr = next;
                    return true;
                }

                rate = next;
            }

            irr = rate;
            return false;
        }

        public static bool TrySolveRate(
            double nper,
            double pmt,
            double pv,
            double fv,
            double type,
            double guess,
            out double rate)
        {
            rate = guess;
            var current = guess;
            for (var iteration = 0; iteration < 100; iteration++)
            {
                var value = EvaluateRate(current, nper, pmt, pv, fv, type);
                if (Math.Abs(value) < 1e-8)
                {
                    rate = current;
                    return true;
                }

                var delta = 1e-6;
                var valuePlus = EvaluateRate(current + delta, nper, pmt, pv, fv, type);
                var derivative = (valuePlus - value) / delta;
                if (Math.Abs(derivative) < 1e-12)
                {
                    break;
                }

                var next = current - (value / derivative);
                if (double.IsNaN(next) || double.IsInfinity(next))
                {
                    break;
                }

                if (Math.Abs(next - current) < 1e-8)
                {
                    rate = next;
                    return true;
                }

                current = next;
            }

            rate = current;
            return false;
        }

        private static double EvaluateRate(double rate, double nper, double pmt, double pv, double fv, double type)
        {
            if (Math.Abs(rate) < 1e-12)
            {
                return pv + (pmt * nper) + fv;
            }

            var pow = Math.Pow(1d + rate, nper);
            return (pv * pow) + (pmt * (1d + rate * type) * (pow - 1d) / rate) + fv;
        }
    }
}
