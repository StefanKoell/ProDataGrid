// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    internal static class ExcelStructuredReferenceParser
    {
        public static bool TryParse(
            string text,
            FormulaSheetReference? sheet,
            out FormulaStructuredReference reference)
        {
            reference = default;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var bracketIndex = text.IndexOf('[');
            if (bracketIndex < 0)
            {
                return false;
            }

            var tableName = bracketIndex > 0 ? text.Substring(0, bracketIndex) : null;
            var bracketContent = text.Substring(bracketIndex);
            var segments = ExtractSegments(bracketContent);
            if (segments.Count == 0)
            {
                return false;
            }

            var scope = FormulaStructuredReferenceScope.None;
            string? columnStart = null;
            string? columnEnd = null;

            foreach (var segment in segments)
            {
                var trimmed = segment.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                if (TryParseScope(trimmed, out var parsedScope))
                {
                    scope = parsedScope;
                    continue;
                }

                if (trimmed.StartsWith("@", StringComparison.Ordinal))
                {
                    scope = FormulaStructuredReferenceScope.ThisRow;
                    trimmed = trimmed.Substring(1);
                }

                if (columnStart == null)
                {
                    columnStart = trimmed;
                }
                else if (columnEnd == null)
                {
                    columnEnd = trimmed;
                }
            }

            reference = new FormulaStructuredReference(sheet, tableName, scope, columnStart, columnEnd);
            return true;
        }

        private static bool TryParseScope(string text, out FormulaStructuredReferenceScope scope)
        {
            scope = FormulaStructuredReferenceScope.None;
            if (!text.StartsWith("#", StringComparison.Ordinal))
            {
                return false;
            }

            var normalized = text.Replace(" ", string.Empty);
            return normalized switch
            {
                "#All" => SetScope(ref scope, FormulaStructuredReferenceScope.All),
                "#Headers" => SetScope(ref scope, FormulaStructuredReferenceScope.Headers),
                "#Data" => SetScope(ref scope, FormulaStructuredReferenceScope.Data),
                "#Totals" => SetScope(ref scope, FormulaStructuredReferenceScope.Totals),
                "#ThisRow" => SetScope(ref scope, FormulaStructuredReferenceScope.ThisRow),
                _ => false
            };
        }

        private static bool SetScope(ref FormulaStructuredReferenceScope target, FormulaStructuredReferenceScope value)
        {
            target = value;
            return true;
        }

        private static List<string> ExtractSegments(string text)
        {
            var segments = new List<string>();
            var depth = 0;
            var segmentStart = -1;
            var capturedInner = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];
                if (ch == '[')
                {
                    depth++;
                    if (depth == 1)
                    {
                        segmentStart = i + 1;
                    }
                    else if (depth == 2)
                    {
                        capturedInner = true;
                        segmentStart = i + 1;
                    }
                }
                else if (ch == ']')
                {
                    if (depth == 2)
                    {
                        if (segmentStart >= 0 && i > segmentStart)
                        {
                            segments.Add(text.Substring(segmentStart, i - segmentStart));
                        }
                    }
                    else if (depth == 1 && !capturedInner)
                    {
                        if (segmentStart >= 0 && i > segmentStart)
                        {
                            segments.Add(text.Substring(segmentStart, i - segmentStart));
                        }
                    }

                    if (depth > 0)
                    {
                        depth--;
                    }
                }
            }

            return segments;
        }
    }
}
