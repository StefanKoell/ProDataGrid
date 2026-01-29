// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal sealed class PiFunction : ExcelFunctionBase
    {
        public PiFunction()
            : base("PI", new FormulaFunctionInfo(0, 0))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.CreateNumber(context, Math.PI);
        }
    }

    internal sealed class PowerFunction : ExcelFunctionBase
    {
        public PowerFunction()
            : base("POWER", new FormulaFunctionInfo(2, 2))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyBinary(args[0], args[1], (left, right) =>
            {
                if (left.Kind == FormulaValueKind.Error)
                {
                    return left;
                }

                if (right.Kind == FormulaValueKind.Error)
                {
                    return right;
                }

                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, left, out var leftNumber, out var error) ||
                    !ExcelFunctionUtilities.TryCoerceToNumber(context, right, out var rightNumber, out error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(context, Math.Pow(leftNumber, rightNumber));
            });
        }
    }

    internal sealed class SqrtFunction : ExcelFunctionBase
    {
        public SqrtFunction()
            : base("SQRT", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], value =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, value, out var number, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (number < 0d)
                {
                    return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
                }

                return ExcelFunctionUtilities.CreateNumber(context, Math.Sqrt(number));
            });
        }
    }

    internal sealed class LnFunction : ExcelFunctionBase
    {
        public LnFunction()
            : base("LN", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], value =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, value, out var number, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (number <= 0d)
                {
                    return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
                }

                return ExcelFunctionUtilities.CreateNumber(context, Math.Log(number));
            });
        }
    }

    internal sealed class LogFunction : ExcelFunctionBase
    {
        public LogFunction()
            : base("LOG", new FormulaFunctionInfo(1, 2))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var baseValue = args.Count > 1 ? args[1] : FormulaValue.FromNumber(10d);
            return ExcelFunctionUtilities.ApplyBinary(args[0], baseValue, (left, right) =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, left, out var number, out var error) ||
                    !ExcelFunctionUtilities.TryCoerceToNumber(context, right, out var baseNumber, out error))
                {
                    return FormulaValue.FromError(error);
                }

                if (number <= 0d || baseNumber <= 0d || Math.Abs(baseNumber - 1d) < double.Epsilon)
                {
                    return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
                }

                return ExcelFunctionUtilities.CreateNumber(context, Math.Log(number, baseNumber));
            });
        }
    }

    internal sealed class Log10Function : ExcelFunctionBase
    {
        public Log10Function()
            : base("LOG10", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], value =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, value, out var number, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                if (number <= 0d)
                {
                    return FormulaValue.FromError(new FormulaError(FormulaErrorType.Num));
                }

                return ExcelFunctionUtilities.CreateNumber(context, Math.Log10(number));
            });
        }
    }

    internal sealed class ExpFunction : ExcelFunctionBase
    {
        public ExpFunction()
            : base("EXP", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], value =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, value, out var number, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                return ExcelFunctionUtilities.CreateNumber(context, Math.Exp(number));
            });
        }
    }

    internal sealed class SignFunction : ExcelFunctionBase
    {
        public SignFunction()
            : base("SIGN", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], value =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, value, out var number, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                var sign = number > 0d ? 1d : number < 0d ? -1d : 0d;
                return ExcelFunctionUtilities.CreateNumber(context, sign);
            });
        }
    }
}
