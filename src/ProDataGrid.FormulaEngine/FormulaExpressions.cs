// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;

namespace ProDataGrid.FormulaEngine
{
    public enum FormulaExpressionKind
    {
        Literal,
        Unary,
        Binary,
        FunctionCall,
        Name,
        Reference,
        ArrayLiteral,
        StructuredReference
    }

    public enum FormulaUnaryOperator
    {
        Plus,
        Negate,
        Percent
    }

    public enum FormulaBinaryOperator
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Power,
        Concat,
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual,
        Union,
        Intersection
    }

    public abstract class FormulaExpression
    {
        protected FormulaExpression(FormulaExpressionKind kind)
        {
            Kind = kind;
        }

        public FormulaExpressionKind Kind { get; }
    }

    public sealed class FormulaLiteralExpression : FormulaExpression
    {
        public FormulaLiteralExpression(FormulaValue value)
            : base(FormulaExpressionKind.Literal)
        {
            Value = value;
        }

        public FormulaValue Value { get; }
    }

    public sealed class FormulaUnaryExpression : FormulaExpression
    {
        public FormulaUnaryExpression(FormulaUnaryOperator op, FormulaExpression operand)
            : base(FormulaExpressionKind.Unary)
        {
            Operator = op;
            Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        }

        public FormulaUnaryOperator Operator { get; }

        public FormulaExpression Operand { get; }
    }

    public sealed class FormulaBinaryExpression : FormulaExpression
    {
        public FormulaBinaryExpression(FormulaBinaryOperator op, FormulaExpression left, FormulaExpression right)
            : base(FormulaExpressionKind.Binary)
        {
            Operator = op;
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public FormulaBinaryOperator Operator { get; }

        public FormulaExpression Left { get; }

        public FormulaExpression Right { get; }
    }

    public sealed class FormulaFunctionCallExpression : FormulaExpression
    {
        public FormulaFunctionCallExpression(string name, IReadOnlyList<FormulaExpression> arguments)
            : base(FormulaExpressionKind.FunctionCall)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        }

        public string Name { get; }

        public IReadOnlyList<FormulaExpression> Arguments { get; }
    }

    public sealed class FormulaNameExpression : FormulaExpression
    {
        public FormulaNameExpression(string name)
            : base(FormulaExpressionKind.Name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }

    public sealed class FormulaReferenceExpression : FormulaExpression
    {
        public FormulaReferenceExpression(FormulaReference reference)
            : base(FormulaExpressionKind.Reference)
        {
            Reference = reference;
        }

        public FormulaReference Reference { get; }
    }

    public sealed class FormulaArrayExpression : FormulaExpression
    {
        private readonly FormulaExpression[,] _items;

        public FormulaArrayExpression(FormulaExpression[,] items)
            : base(FormulaExpressionKind.ArrayLiteral)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        public int RowCount => _items.GetLength(0);

        public int ColumnCount => _items.GetLength(1);

        public FormulaExpression this[int row, int column] => _items[row, column];
    }

    public sealed class FormulaStructuredReferenceExpression : FormulaExpression
    {
        public FormulaStructuredReferenceExpression(FormulaStructuredReference reference)
            : base(FormulaExpressionKind.StructuredReference)
        {
            Reference = reference;
        }

        public FormulaStructuredReference Reference { get; }
    }
}
