// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;

namespace ProDataGrid.FormulaEngine
{
    public enum FormulaValueKind
    {
        Blank,
        Number,
        Text,
        Boolean,
        Error,
        Array,
        Reference
    }

    public sealed class FormulaArray
    {
        private readonly FormulaValue[,] _values;
        private readonly bool[,]? _present;

        public FormulaArray(int rows, int columns, FormulaCellAddress? origin = null, bool sparse = false)
        {
            if (rows <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rows));
            }
            if (columns <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columns));
            }

            _values = new FormulaValue[rows, columns];
            _present = sparse ? new bool[rows, columns] : null;
            Origin = origin;
        }

        public FormulaCellAddress? Origin { get; }

        public int RowCount => _values.GetLength(0);

        public int ColumnCount => _values.GetLength(1);

        public bool HasMask => _present != null;

        public FormulaValue this[int row, int column]
        {
            get => _values[row, column];
            set
            {
                _values[row, column] = value;
                if (_present != null)
                {
                    _present[row, column] = true;
                }
            }
        }

        public bool IsPresent(int row, int column)
        {
            return _present == null || _present[row, column];
        }

        public void SetValue(int row, int column, FormulaValue value, bool present)
        {
            _values[row, column] = value;
            if (_present != null)
            {
                _present[row, column] = present;
            }
        }

        public IEnumerable<FormulaValue> Flatten()
        {
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    if (_present == null || _present[row, column])
                    {
                        yield return _values[row, column];
                    }
                }
            }
        }
    }

    public readonly struct FormulaValue : IEquatable<FormulaValue>
    {
        private readonly double _number;
        private readonly bool _boolean;
        private readonly string? _text;
        private readonly FormulaError _error;
        private readonly FormulaArray? _array;
        private readonly FormulaReference _reference;

        private FormulaValue(
            FormulaValueKind kind,
            double number = 0,
            bool boolean = false,
            string? text = null,
            FormulaError error = default,
            FormulaArray? array = null,
            FormulaReference reference = default)
        {
            Kind = kind;
            _number = number;
            _boolean = boolean;
            _text = text;
            _error = error;
            _array = array;
            _reference = reference;
        }

        public FormulaValueKind Kind { get; }

        public static FormulaValue Blank => new(FormulaValueKind.Blank);

        public static FormulaValue FromNumber(double value) => new(FormulaValueKind.Number, number: value);

        public static FormulaValue FromText(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new FormulaValue(FormulaValueKind.Text, text: value);
        }

        public static FormulaValue FromBoolean(bool value) => new(FormulaValueKind.Boolean, boolean: value);

        public static FormulaValue FromError(FormulaError error) => new(FormulaValueKind.Error, error: error);

        public static FormulaValue FromArray(FormulaArray array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return new FormulaValue(FormulaValueKind.Array, array: array);
        }

        public static FormulaValue FromReference(FormulaReference reference) => new(FormulaValueKind.Reference, reference: reference);

        public double AsNumber()
        {
            if (Kind != FormulaValueKind.Number)
            {
                throw new InvalidOperationException($"Cannot access {Kind} as number.");
            }
            return _number;
        }

        public bool AsBoolean()
        {
            if (Kind != FormulaValueKind.Boolean)
            {
                throw new InvalidOperationException($"Cannot access {Kind} as boolean.");
            }
            return _boolean;
        }

        public string AsText()
        {
            if (Kind != FormulaValueKind.Text)
            {
                throw new InvalidOperationException($"Cannot access {Kind} as text.");
            }
            return _text ?? string.Empty;
        }

        public FormulaError AsError()
        {
            if (Kind != FormulaValueKind.Error)
            {
                throw new InvalidOperationException($"Cannot access {Kind} as error.");
            }
            return _error;
        }

        public FormulaArray AsArray()
        {
            if (Kind != FormulaValueKind.Array)
            {
                throw new InvalidOperationException($"Cannot access {Kind} as array.");
            }
            return _array!;
        }

        public FormulaReference AsReference()
        {
            if (Kind != FormulaValueKind.Reference)
            {
                throw new InvalidOperationException($"Cannot access {Kind} as reference.");
            }
            return _reference;
        }

        public bool Equals(FormulaValue other)
        {
            if (Kind != other.Kind)
            {
                return false;
            }

            return Kind switch
            {
                FormulaValueKind.Blank => true,
                FormulaValueKind.Number => _number.Equals(other._number),
                FormulaValueKind.Text => string.Equals(_text, other._text, StringComparison.Ordinal),
                FormulaValueKind.Boolean => _boolean == other._boolean,
                FormulaValueKind.Error => _error.Equals(other._error),
                FormulaValueKind.Array => ReferenceEquals(_array, other._array),
                FormulaValueKind.Reference => _reference.Equals(other._reference),
                _ => false
            };
        }

        public override bool Equals(object? obj)
        {
            return obj is FormulaValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) + Kind.GetHashCode();
                switch (Kind)
                {
                    case FormulaValueKind.Number:
                        hash = (hash * 31) + _number.GetHashCode();
                        break;
                    case FormulaValueKind.Text:
                        hash = (hash * 31) + (_text?.GetHashCode() ?? 0);
                        break;
                    case FormulaValueKind.Boolean:
                        hash = (hash * 31) + _boolean.GetHashCode();
                        break;
                    case FormulaValueKind.Error:
                        hash = (hash * 31) + _error.GetHashCode();
                        break;
                    case FormulaValueKind.Array:
                        hash = (hash * 31) + (_array?.GetHashCode() ?? 0);
                        break;
                    case FormulaValueKind.Reference:
                        hash = (hash * 31) + _reference.GetHashCode();
                        break;
                }

                return hash;
            }
        }

        public override string ToString()
        {
            return Kind switch
            {
                FormulaValueKind.Blank => string.Empty,
                FormulaValueKind.Number => _number.ToString(),
                FormulaValueKind.Text => _text ?? string.Empty,
                FormulaValueKind.Boolean => _boolean ? "TRUE" : "FALSE",
                FormulaValueKind.Error => _error.ToString(),
                FormulaValueKind.Array => $"Array({RowCount}x{ColumnCount})",
                FormulaValueKind.Reference => _reference.ToString(),
                _ => string.Empty
            };
        }

        private int RowCount => _array?.RowCount ?? 0;

        private int ColumnCount => _array?.ColumnCount ?? 0;

        public static bool operator ==(FormulaValue left, FormulaValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FormulaValue left, FormulaValue right)
        {
            return !left.Equals(right);
        }
    }
}
