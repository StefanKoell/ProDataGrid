// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;

namespace ProDataGrid.FormulaEngine
{
    public enum FormulaErrorType
    {
        Div0,
        NA,
        Name,
        Null,
        Num,
        Ref,
        Value,
        Spill,
        Calc,
        Circ
    }

    public readonly struct FormulaError : IEquatable<FormulaError>
    {
        public FormulaError(FormulaErrorType type, string? message = null)
        {
            Type = type;
            Message = message;
        }

        public FormulaErrorType Type { get; }

        public string? Message { get; }

        public string Code => FormulaErrorText.GetCode(Type);

        public bool Equals(FormulaError other)
        {
            return Type == other.Type && string.Equals(Message, other.Message, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            return obj is FormulaError other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) + Type.GetHashCode();
                hash = (hash * 31) + (Message?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public override string ToString()
        {
            return Code;
        }

        public static bool operator ==(FormulaError left, FormulaError right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FormulaError left, FormulaError right)
        {
            return !left.Equals(right);
        }
    }

    internal static class FormulaErrorText
    {
        public static string GetCode(FormulaErrorType type)
        {
            return type switch
            {
                FormulaErrorType.Div0 => "#DIV/0!",
                FormulaErrorType.NA => "#N/A",
                FormulaErrorType.Name => "#NAME?",
                FormulaErrorType.Null => "#NULL!",
                FormulaErrorType.Num => "#NUM!",
                FormulaErrorType.Ref => "#REF!",
                FormulaErrorType.Value => "#VALUE!",
                FormulaErrorType.Spill => "#SPILL!",
                FormulaErrorType.Calc => "#CALC!",
                FormulaErrorType.Circ => "#CIRC!",
                _ => "#ERROR!"
            };
        }
    }
}
