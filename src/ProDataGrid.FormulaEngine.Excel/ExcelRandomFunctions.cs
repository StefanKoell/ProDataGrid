// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal static class ExcelRandomUtilities
    {
        private static readonly object s_lock = new();
        private static readonly Random s_random = new();

        public static double NextDouble()
        {
            lock (s_lock)
            {
                return s_random.NextDouble();
            }
        }

        public static int NextInt(int minValue, int maxValue)
        {
            lock (s_lock)
            {
                return s_random.Next(minValue, maxValue);
            }
        }
    }

    internal sealed class RandFunction : ExcelFunctionBase
    {
        public RandFunction()
            : base("RAND", new FormulaFunctionInfo(0, 0, isVolatile: true))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.CreateNumber(context, ExcelRandomUtilities.NextDouble());
        }
    }

    internal sealed class RandBetweenFunction : ExcelFunctionBase
    {
        public RandBetweenFunction()
            : base("RANDBETWEEN", new FormulaFunctionInfo(2, 2, isVolatile: true))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            if (!ExcelFunctionUtilities.TryCoerceToInteger(context, args[0], out var bottom, out var error) ||
                !ExcelFunctionUtilities.TryCoerceToInteger(context, args[1], out var top, out error))
            {
                return FormulaValue.FromError(error);
            }

            if (bottom > top)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
            }

            if (bottom == top)
            {
                return ExcelFunctionUtilities.CreateNumber(context, bottom);
            }

            var range = (long)top - bottom + 1;
            if (range <= 0)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
            }

            int result;
            if (range <= int.MaxValue && top < int.MaxValue)
            {
                result = ExcelRandomUtilities.NextInt(bottom, top + 1);
            }
            else
            {
                var sample = ExcelRandomUtilities.NextDouble();
                result = (int)Math.Floor(bottom + (sample * range));
            }

            return ExcelFunctionUtilities.CreateNumber(context, result);
        }
    }
}
