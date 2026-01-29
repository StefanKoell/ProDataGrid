// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;

namespace ProDataGrid.FormulaEngine
{
    internal enum FormulaInstructionKind
    {
        Literal,
        Name,
        Reference,
        StructuredReference,
        Unary,
        Binary,
        FunctionCall,
        LazyFunctionCall,
        ArrayLiteral
    }

    internal readonly struct FormulaInstruction
    {
        public FormulaInstruction(
            FormulaInstructionKind kind,
            FormulaValue literal = default,
            FormulaReference reference = default,
            FormulaStructuredReference structuredReference = default,
            FormulaUnaryOperator unaryOperator = default,
            FormulaBinaryOperator binaryOperator = default,
            string? name = null,
            int argCount = 0,
            int rowCount = 0,
            int columnCount = 0,
            FormulaExpression[]? lazyArguments = null)
        {
            Kind = kind;
            Literal = literal;
            Reference = reference;
            StructuredReference = structuredReference;
            UnaryOperator = unaryOperator;
            BinaryOperator = binaryOperator;
            Name = name;
            ArgCount = argCount;
            RowCount = rowCount;
            ColumnCount = columnCount;
            LazyArguments = lazyArguments;
        }

        public FormulaInstructionKind Kind { get; }

        public FormulaValue Literal { get; }

        public FormulaReference Reference { get; }

        public FormulaStructuredReference StructuredReference { get; }

        public FormulaUnaryOperator UnaryOperator { get; }

        public FormulaBinaryOperator BinaryOperator { get; }

        public string? Name { get; }

        public int ArgCount { get; }

        public int RowCount { get; }

        public int ColumnCount { get; }

        public FormulaExpression[]? LazyArguments { get; }
    }

    internal sealed class FormulaCompiledExpression
    {
        public FormulaCompiledExpression(
            IFormulaFunctionRegistry functionRegistry,
            FormulaInstruction[] instructions,
            int maxStackDepth)
        {
            FunctionRegistry = functionRegistry ?? throw new ArgumentNullException(nameof(functionRegistry));
            Instructions = instructions ?? throw new ArgumentNullException(nameof(instructions));
            MaxStackDepth = maxStackDepth;
        }

        public IFormulaFunctionRegistry FunctionRegistry { get; }

        public FormulaInstruction[] Instructions { get; }

        public int MaxStackDepth { get; }
    }

    internal sealed class FormulaExpressionCompiler
    {
        private readonly IFormulaFunctionRegistry _functionRegistry;
        private readonly List<FormulaInstruction> _instructions = new();
        private int _stackDepth;
        private int _maxStackDepth;

        public FormulaExpressionCompiler(IFormulaFunctionRegistry functionRegistry)
        {
            _functionRegistry = functionRegistry ?? throw new ArgumentNullException(nameof(functionRegistry));
        }

        public FormulaCompiledExpression Compile(FormulaExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            _instructions.Clear();
            _stackDepth = 0;
            _maxStackDepth = 0;

            CompileExpression(expression);

            return new FormulaCompiledExpression(_functionRegistry, _instructions.ToArray(), _maxStackDepth);
        }

        private void CompileExpression(FormulaExpression expression)
        {
            switch (expression.Kind)
            {
                case FormulaExpressionKind.Literal:
                    var literal = ((FormulaLiteralExpression)expression).Value;
                    Emit(new FormulaInstruction(FormulaInstructionKind.Literal, literal: literal), push: 1);
                    break;
                case FormulaExpressionKind.Name:
                    var name = ((FormulaNameExpression)expression).Name;
                    Emit(new FormulaInstruction(FormulaInstructionKind.Name, name: name), push: 1);
                    break;
                case FormulaExpressionKind.Reference:
                    var reference = ((FormulaReferenceExpression)expression).Reference;
                    Emit(new FormulaInstruction(FormulaInstructionKind.Reference, reference: reference), push: 1);
                    break;
                case FormulaExpressionKind.StructuredReference:
                    var structuredReference = ((FormulaStructuredReferenceExpression)expression).Reference;
                    Emit(new FormulaInstruction(FormulaInstructionKind.StructuredReference, structuredReference: structuredReference), push: 1);
                    break;
                case FormulaExpressionKind.Unary:
                    var unary = (FormulaUnaryExpression)expression;
                    CompileExpression(unary.Operand);
                    Emit(new FormulaInstruction(FormulaInstructionKind.Unary, unaryOperator: unary.Operator), pop: 1, push: 1);
                    break;
                case FormulaExpressionKind.Binary:
                    var binary = (FormulaBinaryExpression)expression;
                    CompileExpression(binary.Left);
                    CompileExpression(binary.Right);
                    Emit(new FormulaInstruction(FormulaInstructionKind.Binary, binaryOperator: binary.Operator), pop: 2, push: 1);
                    break;
                case FormulaExpressionKind.FunctionCall:
                    CompileFunctionCall((FormulaFunctionCallExpression)expression);
                    break;
                case FormulaExpressionKind.ArrayLiteral:
                    var array = (FormulaArrayExpression)expression;
                    for (var row = 0; row < array.RowCount; row++)
                    {
                        for (var column = 0; column < array.ColumnCount; column++)
                        {
                            CompileExpression(array[row, column]);
                        }
                    }
                    Emit(new FormulaInstruction(
                            FormulaInstructionKind.ArrayLiteral,
                            rowCount: array.RowCount,
                            columnCount: array.ColumnCount),
                        pop: array.RowCount * array.ColumnCount,
                        push: 1);
                    break;
                default:
                    Emit(new FormulaInstruction(FormulaInstructionKind.Literal, literal: FormulaValue.FromError(new FormulaError(FormulaErrorType.Calc))), push: 1);
                    break;
            }
        }

        private void CompileFunctionCall(FormulaFunctionCallExpression expression)
        {
            if (_functionRegistry.TryGetFunction(expression.Name, out var function) &&
                function is ILazyFormulaFunction)
            {
                Emit(new FormulaInstruction(
                        FormulaInstructionKind.LazyFunctionCall,
                        name: expression.Name,
                        lazyArguments: expression.Arguments is List<FormulaExpression> list
                            ? list.ToArray()
                            : new List<FormulaExpression>(expression.Arguments).ToArray()),
                    push: 1);
                return;
            }

            foreach (var argument in expression.Arguments)
            {
                CompileExpression(argument);
            }

            Emit(new FormulaInstruction(
                    FormulaInstructionKind.FunctionCall,
                    name: expression.Name,
                    argCount: expression.Arguments.Count),
                pop: expression.Arguments.Count,
                push: 1);
        }

        private void Emit(FormulaInstruction instruction, int pop = 0, int push = 0)
        {
            _instructions.Add(instruction);
            _stackDepth -= pop;
            if (_stackDepth < 0)
            {
                _stackDepth = 0;
            }
            _stackDepth += push;
            if (_stackDepth > _maxStackDepth)
            {
                _maxStackDepth = _stackDepth;
            }
        }
    }
}
