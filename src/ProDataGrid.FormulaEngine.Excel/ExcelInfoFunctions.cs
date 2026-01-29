// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal sealed class IsErrFunction : ExcelFunctionBase
    {
        public IsErrFunction()
            : base("ISERR", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (value.Kind == FormulaValueKind.Error && value.AsError().Type != FormulaErrorType.NA)
                {
                    return FormulaValue.FromBoolean(true);
                }

                return FormulaValue.FromBoolean(false);
            });
        }
    }

    internal sealed class IsNonTextFunction : ExcelFunctionBase
    {
        public IsNonTextFunction()
            : base("ISNONTEXT", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                return FormulaValue.FromBoolean(value.Kind != FormulaValueKind.Text);
            });
        }
    }

    internal sealed class IsEvenFunction : ExcelFunctionBase
    {
        public IsEvenFunction()
            : base("ISEVEN", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, value, out var number, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                var truncated = Math.Truncate(number);
                return FormulaValue.FromBoolean(Math.Abs(truncated) % 2d == 0d);
            });
        }
    }

    internal sealed class IsOddFunction : ExcelFunctionBase
    {
        public IsOddFunction()
            : base("ISODD", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (!ExcelFunctionUtilities.TryCoerceToNumber(context, value, out var number, out var error))
                {
                    return FormulaValue.FromError(error);
                }

                var truncated = Math.Truncate(number);
                return FormulaValue.FromBoolean(Math.Abs(truncated) % 2d == 1d);
            });
        }
    }

    internal sealed class TypeFunction : ExcelFunctionBase
    {
        public TypeFunction()
            : base("TYPE", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], (value) =>
            {
                if (value.Kind == FormulaValueKind.Array)
                {
                    return ExcelFunctionUtilities.CreateNumber(context, 64);
                }

                return value.Kind switch
                {
                    FormulaValueKind.Number => ExcelFunctionUtilities.CreateNumber(context, 1),
                    FormulaValueKind.Blank => ExcelFunctionUtilities.CreateNumber(context, 1),
                    FormulaValueKind.Text => ExcelFunctionUtilities.CreateNumber(context, 2),
                    FormulaValueKind.Boolean => ExcelFunctionUtilities.CreateNumber(context, 4),
                    FormulaValueKind.Error => ExcelFunctionUtilities.CreateNumber(context, 16),
                    _ => ExcelFunctionUtilities.CreateNumber(context, 1)
                };
            });
        }
    }

    internal sealed class ErrorTypeFunction : ExcelFunctionBase
    {
        public ErrorTypeFunction()
            : base("ERROR.TYPE", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var value = args[0];
            if (value.Kind != FormulaValueKind.Error)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.NA));
            }

            var error = value.AsError();
            var code = error.Type switch
            {
                FormulaErrorType.Null => 1,
                FormulaErrorType.Div0 => 2,
                FormulaErrorType.Value => 3,
                FormulaErrorType.Ref => 4,
                FormulaErrorType.Name => 5,
                FormulaErrorType.Num => 6,
                FormulaErrorType.NA => 7,
                _ => 0
            };

            if (code == 0)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.NA));
            }

            return ExcelFunctionUtilities.CreateNumber(context, code);
        }
    }

    internal sealed class NaFunction : ExcelFunctionBase
    {
        public NaFunction()
            : base("NA", new FormulaFunctionInfo(0, 0))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return FormulaValue.FromError(new FormulaError(FormulaErrorType.NA));
        }
    }

    internal sealed class NFunction : ExcelFunctionBase
    {
        public NFunction()
            : base("N", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], value =>
            {
                return value.Kind switch
                {
                    FormulaValueKind.Number => ExcelFunctionUtilities.CreateNumber(context, value.AsNumber()),
                    FormulaValueKind.Boolean => ExcelFunctionUtilities.CreateNumber(context, value.AsBoolean() ? 1d : 0d),
                    FormulaValueKind.Blank => ExcelFunctionUtilities.CreateNumber(context, 0d),
                    FormulaValueKind.Text => ExcelFunctionUtilities.CreateNumber(context, 0d),
                    FormulaValueKind.Error => value,
                    _ => FormulaValue.FromError(new FormulaError(FormulaErrorType.Value))
                };
            });
        }
    }

    internal sealed class TFunction : ExcelFunctionBase
    {
        public TFunction()
            : base("T", new FormulaFunctionInfo(1, 1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return ExcelFunctionUtilities.ApplyUnary(args[0], value =>
            {
                if (value.Kind == FormulaValueKind.Error)
                {
                    return value;
                }

                if (value.Kind == FormulaValueKind.Text)
                {
                    return FormulaValue.FromText(value.AsText());
                }

                return FormulaValue.FromText(string.Empty);
            });
        }
    }
}
