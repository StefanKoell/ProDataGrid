// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal static class ExcelSheetNameParser
    {
        public static FormulaSheetReference ParseSheetReference(string text)
        {
            ParseSheetToken(text, out var workbook, out var sheet);
            return new FormulaSheetReference(workbook, sheet);
        }

        public static FormulaSheetReference ParseSheetRange(string startToken, string endToken)
        {
            ParseSheetToken(startToken, out var startWorkbook, out var startSheet);
            ParseSheetToken(endToken, out var endWorkbook, out var endSheet);

            var workbook = string.IsNullOrWhiteSpace(endWorkbook) ? startWorkbook : endWorkbook;
            return new FormulaSheetReference(workbook, startSheet, endSheet);
        }

        public static bool TryParseEmbeddedSheetRange(string text, out FormulaSheetReference? sheet)
        {
            sheet = null;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var separatorIndex = text.IndexOf(':');
            if (separatorIndex <= 0 || separatorIndex >= text.Length - 1)
            {
                return false;
            }

            var startToken = text.Substring(0, separatorIndex);
            var endToken = text.Substring(separatorIndex + 1);
            sheet = ParseSheetRange(startToken, endToken);
            return true;
        }

        private static void ParseSheetToken(string text, out string? workbook, out string? sheet)
        {
            workbook = null;
            sheet = text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var start = text.IndexOf('[');
            var end = text.IndexOf(']');
            if (start == 0 && end > start)
            {
                workbook = text.Substring(start + 1, end - start - 1);
                sheet = end + 1 < text.Length ? text.Substring(end + 1) : string.Empty;
            }
        }
    }
}
