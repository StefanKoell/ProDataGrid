// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal sealed class ValueFunction : ExcelFunctionBase
    {
        public ValueFunction()
            : base("VALUE", new FormulaFunctionInfo(1, 1))
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

                return ExcelFunctionUtilities.CreateNumber(context, number);
            });
        }
    }
}
