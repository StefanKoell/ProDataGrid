// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;

namespace ProDataGrid.FormulaEngine
{
    public enum FormulaStructuredReferenceScope
    {
        None,
        All,
        Headers,
        Data,
        Totals,
        ThisRow
    }

    public readonly struct FormulaStructuredReference : IEquatable<FormulaStructuredReference>
    {
        public FormulaStructuredReference(
            FormulaSheetReference? sheet,
            string? tableName,
            FormulaStructuredReferenceScope scope,
            string? columnStart,
            string? columnEnd)
        {
            Sheet = sheet;
            TableName = tableName;
            Scope = scope;
            ColumnStart = columnStart;
            ColumnEnd = columnEnd;
        }

        public FormulaSheetReference? Sheet { get; }

        public string? TableName { get; }

        public FormulaStructuredReferenceScope Scope { get; }

        public string? ColumnStart { get; }

        public string? ColumnEnd { get; }

        public bool IsColumnRange => !string.IsNullOrWhiteSpace(ColumnEnd);

        public bool Equals(FormulaStructuredReference other)
        {
            return Nullable.Equals(Sheet, other.Sheet) &&
                   string.Equals(TableName, other.TableName, StringComparison.OrdinalIgnoreCase) &&
                   Scope == other.Scope &&
                   string.Equals(ColumnStart, other.ColumnStart, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ColumnEnd, other.ColumnEnd, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is FormulaStructuredReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) + (Sheet?.GetHashCode() ?? 0);
                hash = (hash * 31) + (TableName == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(TableName));
                hash = (hash * 31) + Scope.GetHashCode();
                hash = (hash * 31) + (ColumnStart == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(ColumnStart));
                hash = (hash * 31) + (ColumnEnd == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(ColumnEnd));
                return hash;
            }
        }

        public override string ToString()
        {
            var scopePart = Scope switch
            {
                FormulaStructuredReferenceScope.All => "#All",
                FormulaStructuredReferenceScope.Headers => "#Headers",
                FormulaStructuredReferenceScope.Data => "#Data",
                FormulaStructuredReferenceScope.Totals => "#Totals",
                FormulaStructuredReferenceScope.ThisRow => "#This Row",
                _ => string.Empty
            };

            var columnPart = ColumnStart ?? string.Empty;
            if (IsColumnRange)
            {
                columnPart = $"{ColumnStart}:{ColumnEnd}";
            }

            var prefix = string.IsNullOrWhiteSpace(TableName) ? string.Empty : TableName;
            var sheetPrefix = Sheet == null ? string.Empty : $"{Sheet}!";

            if (!string.IsNullOrWhiteSpace(scopePart))
            {
                return $"{sheetPrefix}{prefix}[[{scopePart}],[{columnPart}]]";
            }

            return $"{sheetPrefix}{prefix}[{columnPart}]";
        }

        public static bool operator ==(FormulaStructuredReference left, FormulaStructuredReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FormulaStructuredReference left, FormulaStructuredReference right)
        {
            return !left.Equals(right);
        }
    }

    public interface IFormulaStructuredReferenceDependencyResolver
    {
        bool TryGetStructuredReferenceDependencies(
            FormulaStructuredReference reference,
            out IEnumerable<FormulaCellAddress> dependencies);
    }
}
