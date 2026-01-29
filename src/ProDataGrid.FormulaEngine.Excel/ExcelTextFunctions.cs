// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal sealed class TextJoinFunction : ExcelFunctionBase
    {
        public TextJoinFunction()
            : base("TEXTJOIN", new FormulaFunctionInfo(3, -1))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var address = context.EvaluationContext.Address;
            var delimiterValue = ExcelFunctionUtilities.ApplyImplicitIntersection(args[0], address);
            if (!ExcelFunctionUtilities.TryCoerceToText(delimiterValue, out var delimiter, out var error))
            {
                return FormulaValue.FromError(error);
            }

            var ignoreEmptyValue = ExcelFunctionUtilities.ApplyImplicitIntersection(args[1], address);
            if (!ExcelFunctionUtilities.TryCoerceToBoolean(ignoreEmptyValue, address, out var ignoreEmpty, out error))
            {
                return FormulaValue.FromError(error);
            }

            var builder = new StringBuilder();
            var first = true;
            for (var i = 2; i < args.Count; i++)
            {
                foreach (var value in ExcelFunctionUtilities.FlattenValues(args[i]))
                {
                    if (value.Kind == FormulaValueKind.Error)
                    {
                        return value;
                    }

                    if (!ExcelFunctionUtilities.TryCoerceToText(value, out var text, out error))
                    {
                        return FormulaValue.FromError(error);
                    }

                    if (ignoreEmpty && text.Length == 0)
                    {
                        continue;
                    }

                    if (!first)
                    {
                        builder.Append(delimiter);
                    }

                    builder.Append(text);
                    first = false;
                }
            }

            return FormulaValue.FromText(builder.ToString());
        }
    }

    internal sealed class TextSplitFunction : ExcelFunctionBase
    {
        public TextSplitFunction()
            : base("TEXTSPLIT", new FormulaFunctionInfo(2, 6))
        {
        }

        public override FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args)
        {
            var address = context.EvaluationContext.Address;
            var textValue = ExcelFunctionUtilities.ApplyImplicitIntersection(args[0], address);
            if (!ExcelFunctionUtilities.TryCoerceToText(textValue, out var text, out var error))
            {
                return FormulaValue.FromError(error);
            }

            var columnDelimiterValue = ExcelFunctionUtilities.ApplyImplicitIntersection(args[1], address);
            if (!ExcelFunctionUtilities.TryCoerceToText(columnDelimiterValue, out var columnDelimiter, out error))
            {
                return FormulaValue.FromError(error);
            }

            if (columnDelimiter.Length == 0)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Value));
            }

            string? rowDelimiter = null;
            if (args.Count > 2)
            {
                var rowDelimiterValue = ExcelFunctionUtilities.ApplyImplicitIntersection(args[2], address);
                if (rowDelimiterValue.Kind != FormulaValueKind.Blank)
                {
                    if (!ExcelFunctionUtilities.TryCoerceToText(rowDelimiterValue, out rowDelimiter, out error))
                    {
                        return FormulaValue.FromError(error);
                    }

                    if (rowDelimiter.Length == 0)
                    {
                        return FormulaValue.FromError(new FormulaError(FormulaErrorType.Value));
                    }
                }
            }

            var ignoreEmpty = false;
            if (args.Count > 3)
            {
                var ignoreEmptyValue = ExcelFunctionUtilities.ApplyImplicitIntersection(args[3], address);
                if (!ExcelFunctionUtilities.TryCoerceToBoolean(ignoreEmptyValue, address, out ignoreEmpty, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            var matchMode = 0;
            if (args.Count > 4)
            {
                var matchModeValue = ExcelFunctionUtilities.ApplyImplicitIntersection(args[4], address);
                if (!ExcelFunctionUtilities.TryCoerceToInteger(context, matchModeValue, out matchMode, out error))
                {
                    return FormulaValue.FromError(error);
                }
            }

            if (matchMode != 0 && matchMode != 1)
            {
                return FormulaValue.FromError(new FormulaError(FormulaErrorType.Value));
            }

            var padValue = FormulaValue.FromError(new FormulaError(FormulaErrorType.NA));
            if (args.Count > 5)
            {
                padValue = ExcelFunctionUtilities.ApplyImplicitIntersection(args[5], address);
            }

            var comparison = matchMode == 1 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var rows = rowDelimiter == null
                ? new List<string> { text }
                : ExcelTextSplitUtilities.SplitByDelimiter(text, rowDelimiter, ignoreEmpty, comparison);

            if (rows.Count == 0)
            {
                rows.Add(string.Empty);
            }

            var rowSegments = new List<List<string>>(rows.Count);
            var maxColumns = 0;
            foreach (var rowText in rows)
            {
                var columns = ExcelTextSplitUtilities.SplitByDelimiter(rowText, columnDelimiter, ignoreEmpty, comparison);
                rowSegments.Add(columns);
                if (columns.Count > maxColumns)
                {
                    maxColumns = columns.Count;
                }
            }

            if (maxColumns == 0)
            {
                maxColumns = 1;
            }

            var result = new FormulaArray(rows.Count, maxColumns);
            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var columns = rowSegments[rowIndex];
                for (var columnIndex = 0; columnIndex < maxColumns; columnIndex++)
                {
                    if (columnIndex < columns.Count)
                    {
                        result[rowIndex, columnIndex] = FormulaValue.FromText(columns[columnIndex]);
                    }
                    else
                    {
                        result[rowIndex, columnIndex] = padValue;
                    }
                }
            }

            return FormulaValue.FromArray(result);
        }
    }

    internal static class ExcelTextSplitUtilities
    {
        public static List<string> SplitByDelimiter(
            string text,
            string delimiter,
            bool ignoreEmpty,
            StringComparison comparison)
        {
            var result = new List<string>();
            var index = 0;
            while (true)
            {
                var match = text.IndexOf(delimiter, index, comparison);
                if (match < 0)
                {
                    var segment = text.Substring(index);
                    if (!ignoreEmpty || segment.Length > 0)
                    {
                        result.Add(segment);
                    }
                    break;
                }

                var part = text.Substring(index, match - index);
                if (!ignoreEmpty || part.Length > 0)
                {
                    result.Add(part);
                }
                index = match + delimiter.Length;
            }

            return result;
        }
    }
}
