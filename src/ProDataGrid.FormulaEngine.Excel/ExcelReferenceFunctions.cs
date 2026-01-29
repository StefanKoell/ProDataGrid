// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal sealed class OffsetFunction : ExcelFunctionBase, ILazyFormulaFunction
    {
        public OffsetFunction()
            : base("OFFSET", new FormulaFunctionInfo(3, 5, isVolatile: true))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return FormulaValue.FromError(new FormulaError(FormulaErrorType.Value));
        }

        public FormulaValue InvokeLazy(
            FormulaFunctionContext context,
            IReadOnlyList<FormulaExpression> arguments,
            FormulaEvaluator evaluator,
            IFormulaValueResolver resolver)
        {
            if (arguments.Count < 3)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Value));
            }

            if (arguments[0] is not FormulaReferenceExpression referenceExpression)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Value));
            }

            var origin = context.EvaluationContext.Address;
            if (!FormulaReferenceResolver.TryResolveRange(referenceExpression.Reference, origin, out var baseRange))
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
            }

            var address = context.EvaluationContext.Address;
            var rowOffsetValue = ExcelFunctionUtilities.ApplyImplicitIntersection(
                evaluator.Evaluate(arguments[1], context.EvaluationContext, resolver),
                address);
            if (!ExcelFunctionUtilities.TryCoerceToInteger(context, rowOffsetValue, out var rowOffset, out var error))
            {
                return FormulaValue.FromError(error);
            }

            var columnOffsetValue = ExcelFunctionUtilities.ApplyImplicitIntersection(
                evaluator.Evaluate(arguments[2], context.EvaluationContext, resolver),
                address);
            if (!ExcelFunctionUtilities.TryCoerceToInteger(context, columnOffsetValue, out var columnOffset, out error))
            {
                return FormulaValue.FromError(error);
            }

            var height = baseRange.End.Row - baseRange.Start.Row + 1;
            if (arguments.Count > 3)
            {
                var heightValue = ExcelFunctionUtilities.ApplyImplicitIntersection(
                    evaluator.Evaluate(arguments[3], context.EvaluationContext, resolver),
                    address);
                if (!ExcelFunctionUtilities.TryCoerceToInteger(context, heightValue, out height, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            var width = baseRange.End.Column - baseRange.Start.Column + 1;
            if (arguments.Count > 4)
            {
                var widthValue = ExcelFunctionUtilities.ApplyImplicitIntersection(
                    evaluator.Evaluate(arguments[4], context.EvaluationContext, resolver),
                    address);
                if (!ExcelFunctionUtilities.TryCoerceToInteger(context, widthValue, out width, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            if (height <= 0 || width <= 0)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Value));
            }

            var startRow = baseRange.Start.Row + rowOffset;
            var startColumn = baseRange.Start.Column + columnOffset;
            if (startRow <= 0 || startColumn <= 0)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
            }

            var endRow = startRow + height - 1;
            var endColumn = startColumn + width - 1;
            if (endRow <= 0 || endColumn <= 0)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
            }

            FormulaSheetReference? sheetRef = string.IsNullOrWhiteSpace(baseRange.Start.SheetName)
                ? null
                : new FormulaSheetReference(null, baseRange.Start.SheetName);

            var start = new FormulaReferenceAddress(FormulaReferenceMode.A1, startRow, startColumn, true, true, sheetRef);
            var end = new FormulaReferenceAddress(FormulaReferenceMode.A1, endRow, endColumn, true, true, sheetRef);
            var offsetReference = (height == 1 && width == 1)
                ? new FormulaReference(start)
                : new FormulaReference(start, end);

            if (resolver.TryResolveReference(context.EvaluationContext, offsetReference, out var value))
            {
                return value;
            }

            return FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
        }
    }

    internal sealed class IndirectFunction : ExcelFunctionBase, ILazyFormulaFunction
    {
        private static readonly ExcelFormulaParser s_parser = new();

        public IndirectFunction()
            : base("INDIRECT", new FormulaFunctionInfo(1, 2, isVolatile: true))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            return FormulaValue.FromError(new FormulaError(FormulaErrorType.Value));
        }

        public FormulaValue InvokeLazy(
            FormulaFunctionContext context,
            IReadOnlyList<FormulaExpression> arguments,
            FormulaEvaluator evaluator,
            IFormulaValueResolver resolver)
        {
            var address = context.EvaluationContext.Address;
            var textValue = ExcelFunctionUtilities.ApplyImplicitIntersection(
                evaluator.Evaluate(arguments[0], context.EvaluationContext, resolver),
                address);
            if (!ExcelFunctionUtilities.TryCoerceToText(textValue, out var text, out var error))
            {
                return FormulaValue.FromError(error);
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
            }

            var a1 = true;
            if (arguments.Count > 1)
            {
                var a1Value = ExcelFunctionUtilities.ApplyImplicitIntersection(
                    evaluator.Evaluate(arguments[1], context.EvaluationContext, resolver),
                    address);
                if (!ExcelFunctionUtilities.TryCoerceToBoolean(a1Value, address, out a1, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            var settings = context.EvaluationContext.Workbook.Settings;
            var options = settings.CreateParseOptions(a1 ? FormulaReferenceMode.A1 : FormulaReferenceMode.R1C1);

            FormulaExpression expression;
            try
            {
                expression = s_parser.Parse(text, options);
            }
            catch (FormulaParseException)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
            }

            return evaluator.Evaluate(expression, context.EvaluationContext, resolver);
        }
    }
}
