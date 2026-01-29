// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

namespace ProDataGrid.FormulaEngine
{
    public enum FormulaTokenType
    {
        Number,
        Text,
        Name,
        Error,
        Boolean,
        Operator,
        Comma,
        Semicolon,
        Intersection,
        Colon,
        OpenParen,
        CloseParen,
        OpenBrace,
        CloseBrace,
        Exclamation,
        End
    }

    public readonly struct FormulaToken : System.IEquatable<FormulaToken>
    {
        public FormulaToken(FormulaTokenType type, string text, int start, int length)
        {
            Type = type;
            Text = text;
            Start = start;
            Length = length;
        }

        public FormulaTokenType Type { get; }

        public string Text { get; }

        public int Start { get; }

        public int Length { get; }

        public int End => Start + Length;

        public override string ToString()
        {
            return $"{Type} '{Text}' @{Start}";
        }

        public bool Equals(FormulaToken other)
        {
            return Type == other.Type &&
                   Start == other.Start &&
                   Length == other.Length &&
                   string.Equals(Text, other.Text, System.StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            return obj is FormulaToken other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) + Type.GetHashCode();
                hash = (hash * 31) + Start.GetHashCode();
                hash = (hash * 31) + Length.GetHashCode();
                hash = (hash * 31) + (Text?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static bool operator ==(FormulaToken left, FormulaToken right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FormulaToken left, FormulaToken right)
        {
            return !left.Equals(right);
        }
    }
}
