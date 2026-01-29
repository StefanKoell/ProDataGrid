// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;

namespace ProDataGrid.FormulaEngine
{
    public readonly struct FormulaCellAddress : IEquatable<FormulaCellAddress>
    {
        public FormulaCellAddress(int row, int column)
            : this(null, row, column)
        {
        }

        public FormulaCellAddress(string? sheetName, int row, int column)
        {
            if (row <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            if (column <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            SheetName = sheetName;
            Row = row;
            Column = column;
        }

        public string? SheetName { get; }

        public int Row { get; }

        public int Column { get; }

        public FormulaCellAddress WithSheet(string? sheetName)
        {
            return new FormulaCellAddress(sheetName, Row, Column);
        }

        public bool Equals(FormulaCellAddress other)
        {
            return Row == other.Row && Column == other.Column &&
                   string.Equals(SheetName, other.SheetName, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object? obj)
        {
            return obj is FormulaCellAddress other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) + (SheetName == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(SheetName));
                hash = (hash * 31) + Row.GetHashCode();
                hash = (hash * 31) + Column.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return SheetName == null
                ? $"R{Row}C{Column}"
                : $"{SheetName}!R{Row}C{Column}";
        }

        public static bool operator ==(FormulaCellAddress left, FormulaCellAddress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FormulaCellAddress left, FormulaCellAddress right)
        {
            return !left.Equals(right);
        }
    }

    public readonly struct FormulaRangeAddress : IEquatable<FormulaRangeAddress>
    {
        public FormulaRangeAddress(FormulaCellAddress start, FormulaCellAddress end)
        {
            if (!string.Equals(start.SheetName, end.SheetName, StringComparison.Ordinal))
            {
                throw new ArgumentException("Range endpoints must belong to the same sheet.");
            }

            var startRow = Math.Min(start.Row, end.Row);
            var endRow = Math.Max(start.Row, end.Row);
            var startColumn = Math.Min(start.Column, end.Column);
            var endColumn = Math.Max(start.Column, end.Column);

            Start = new FormulaCellAddress(start.SheetName, startRow, startColumn);
            End = new FormulaCellAddress(start.SheetName, endRow, endColumn);
        }

        public FormulaCellAddress Start { get; }

        public FormulaCellAddress End { get; }

        public bool Contains(FormulaCellAddress address)
        {
            if (!string.Equals(address.SheetName, Start.SheetName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return address.Row >= Start.Row && address.Row <= End.Row &&
                   address.Column >= Start.Column && address.Column <= End.Column;
        }

        public bool Equals(FormulaRangeAddress other)
        {
            return Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object? obj)
        {
            return obj is FormulaRangeAddress other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) + Start.GetHashCode();
                hash = (hash * 31) + End.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{Start}:{End}";
        }

        public static bool operator ==(FormulaRangeAddress left, FormulaRangeAddress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FormulaRangeAddress left, FormulaRangeAddress right)
        {
            return !left.Equals(right);
        }
    }
}
