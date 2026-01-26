// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Avalonia.Controls.DataGridPivoting
{
    internal sealed partial class PivotTableBuilder
    {
        private sealed class PivotFormulaEvaluator
        {
            private readonly PivotFormula?[] _formulas;
            private readonly Dictionary<string, int> _fieldLookup;

            public PivotFormulaEvaluator(IList<PivotValueField> valueFields)
            {
                _formulas = new PivotFormula[valueFields.Count];
                _fieldLookup = BuildFieldLookup(valueFields);
                Usage.SetValueFieldCount(valueFields.Count);

                for (var i = 0; i < valueFields.Count; i++)
                {
                    var field = valueFields[i];
                    if (field == null || string.IsNullOrWhiteSpace(field.Formula))
                    {
                        continue;
                    }

                    if (PivotFormula.TryParse(field.Formula!, _fieldLookup, out var formula))
                    {
                        _formulas[i] = formula;
                        Usage.Merge(formula.Usage);
                    }
                }
            }

            public PivotFormulaUsage Usage { get; } = new();

            public bool HasFormulas
            {
                get
                {
                    for (var i = 0; i < _formulas.Length; i++)
                    {
                        if (_formulas[i] != null)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            public PivotFormulaCellContext CreateContext(
                Dictionary<PivotCellKey, PivotCellState> cellStates,
                object?[] rowPathValues,
                object?[] columnPathValues,
                Dictionary<object?[], object?[]>? rowParentPaths,
                Dictionary<object?[], object?[]>? columnParentPaths)
            {
                return new PivotFormulaCellContext(
                    this,
                    cellStates,
                    rowPathValues,
                    columnPathValues,
                    rowParentPaths,
                    columnParentPaths);
            }

            internal PivotFormula? GetFormula(int valueIndex)
            {
                return valueIndex >= 0 && valueIndex < _formulas.Length ? _formulas[valueIndex] : null;
            }

            internal object? GetAggregateValue(
                Dictionary<PivotCellKey, PivotCellState> cellStates,
                object?[] rowKey,
                object?[] columnKey,
                int valueIndex)
            {
                if (!cellStates.TryGetValue(new PivotCellKey(rowKey, columnKey), out var state))
                {
                    return null;
                }

                return state.GetResult(valueIndex);
            }

            internal object? EvaluateAt(
                int valueIndex,
                Dictionary<PivotCellKey, PivotCellState> cellStates,
                object?[] rowPathValues,
                object?[] columnPathValues,
                Dictionary<object?[], object?[]>? rowParentPaths,
                Dictionary<object?[], object?[]>? columnParentPaths)
            {
                var context = CreateContext(cellStates, rowPathValues, columnPathValues, rowParentPaths, columnParentPaths);
                return context.ResolveValue(valueIndex);
            }

            private static Dictionary<string, int> BuildFieldLookup(IList<PivotValueField> valueFields)
            {
                var lookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < valueFields.Count; i++)
                {
                    var field = valueFields[i];
                    if (field == null)
                    {
                        continue;
                    }

                    AddLookupValue(lookup, field.Key?.ToString(), i);
                    AddLookupValue(lookup, field.Header, i);
                }

                return lookup;
            }

            private static void AddLookupValue(Dictionary<string, int> lookup, string? key, int index)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return;
                }

                if (!lookup.ContainsKey(key))
                {
                    lookup[key] = index;
                }
            }
        }

        private sealed class PivotFormulaCellContext
        {
            private static readonly object?[] EmptyPath = Array.Empty<object?>();

            private readonly PivotFormulaEvaluator _owner;
            private readonly Dictionary<PivotCellKey, PivotCellState> _cellStates;
            private readonly object?[] _rowPathValues;
            private readonly object?[] _columnPathValues;
            private readonly Dictionary<object?[], object?[]>? _rowParentPaths;
            private readonly Dictionary<object?[], object?[]>? _columnParentPaths;
            private readonly object?[] _cache;
            private readonly bool[] _cacheSet;
            private readonly bool[] _evaluating;

            public PivotFormulaCellContext(
                PivotFormulaEvaluator owner,
                Dictionary<PivotCellKey, PivotCellState> cellStates,
                object?[] rowPathValues,
                object?[] columnPathValues,
                Dictionary<object?[], object?[]>? rowParentPaths,
                Dictionary<object?[], object?[]>? columnParentPaths)
            {
                _owner = owner;
                _cellStates = cellStates;
                _rowPathValues = rowPathValues;
                _columnPathValues = columnPathValues;
                _rowParentPaths = rowParentPaths;
                _columnParentPaths = columnParentPaths;
                _cache = new object?[owner.Usage.ValueFieldCount];
                _cacheSet = new bool[owner.Usage.ValueFieldCount];
                _evaluating = new bool[owner.Usage.ValueFieldCount];
            }

            public object? ResolveValue(int valueIndex)
            {
                if (valueIndex < 0 || valueIndex >= _cache.Length)
                {
                    return null;
                }

                if (_cacheSet[valueIndex])
                {
                    return _cache[valueIndex];
                }

                if (_evaluating[valueIndex])
                {
                    return null;
                }

                _evaluating[valueIndex] = true;
                object? result;

                var formula = _owner.GetFormula(valueIndex);
                if (formula != null)
                {
                    result = formula.Evaluate(this);
                }
                else
                {
                    result = _owner.GetAggregateValue(_cellStates, _rowPathValues, _columnPathValues, valueIndex);
                }

                _cache[valueIndex] = result;
                _cacheSet[valueIndex] = true;
                _evaluating[valueIndex] = false;
                return result;
            }

            public object? ResolveRowTotal(int valueIndex)
            {
                return _owner.EvaluateAt(valueIndex, _cellStates, _rowPathValues, EmptyPath, _rowParentPaths, _columnParentPaths);
            }

            public object? ResolveColumnTotal(int valueIndex)
            {
                return _owner.EvaluateAt(valueIndex, _cellStates, EmptyPath, _columnPathValues, _rowParentPaths, _columnParentPaths);
            }

            public object? ResolveGrandTotal(int valueIndex)
            {
                return _owner.EvaluateAt(valueIndex, _cellStates, EmptyPath, EmptyPath, _rowParentPaths, _columnParentPaths);
            }

            public object? ResolveParentRowTotal(int valueIndex)
            {
                var parentRow = GetParentPath(_rowPathValues, _rowParentPaths);
                return _owner.EvaluateAt(valueIndex, _cellStates, parentRow, _columnPathValues, _rowParentPaths, _columnParentPaths);
            }

            public object? ResolveParentColumnTotal(int valueIndex)
            {
                var parentColumn = GetParentPath(_columnPathValues, _columnParentPaths);
                return _owner.EvaluateAt(valueIndex, _cellStates, _rowPathValues, parentColumn, _rowParentPaths, _columnParentPaths);
            }

            private static object?[] GetParentPath(object?[] pathValues, Dictionary<object?[], object?[]>? parentPaths)
            {
                if (parentPaths != null && parentPaths.TryGetValue(pathValues, out var parent))
                {
                    return parent;
                }

                if (pathValues.Length == 0)
                {
                    return pathValues;
                }

                var parentPath = new object?[pathValues.Length - 1];
                if (parentPath.Length > 0)
                {
                    Array.Copy(pathValues, parentPath, parentPath.Length);
                }

                return parentPath;
            }
        }

        private sealed class PivotFormulaUsage
        {
            public bool UsesRowTotals { get; private set; }

            public bool UsesColumnTotals { get; private set; }

            public bool UsesGrandTotals { get; private set; }

            public bool UsesParentRowTotals { get; private set; }

            public bool UsesParentColumnTotals { get; private set; }

            public int ValueFieldCount { get; private set; }

            public void Merge(PivotFormulaUsage usage)
            {
                if (usage == null)
                {
                    return;
                }

                UsesRowTotals |= usage.UsesRowTotals;
                UsesColumnTotals |= usage.UsesColumnTotals;
                UsesGrandTotals |= usage.UsesGrandTotals;
                UsesParentRowTotals |= usage.UsesParentRowTotals;
                UsesParentColumnTotals |= usage.UsesParentColumnTotals;
                ValueFieldCount = Math.Max(ValueFieldCount, usage.ValueFieldCount);
            }

            public void SetValueFieldCount(int valueFieldCount)
            {
                ValueFieldCount = Math.Max(ValueFieldCount, valueFieldCount);
            }

            public void MarkRowTotals() => UsesRowTotals = true;

            public void MarkColumnTotals() => UsesColumnTotals = true;

            public void MarkGrandTotals() => UsesGrandTotals = true;

            public void MarkParentRowTotals() => UsesParentRowTotals = true;

            public void MarkParentColumnTotals() => UsesParentColumnTotals = true;
        }

        private sealed class PivotFormula
        {
            private readonly PivotFormulaToken[] _tokens;

            private PivotFormula(PivotFormulaToken[] tokens, PivotFormulaUsage usage)
            {
                _tokens = tokens;
                Usage = usage;
            }

            public PivotFormulaUsage Usage { get; }

            public static bool TryParse(string formula, Dictionary<string, int> fieldLookup, out PivotFormula parsed)
            {
                parsed = null!;
                if (string.IsNullOrWhiteSpace(formula))
                {
                    return false;
                }

                var usage = new PivotFormulaUsage();
                usage.SetValueFieldCount(fieldLookup.Count);

                if (!PivotFormulaParser.TryParse(formula, fieldLookup, usage, out var tokens))
                {
                    return false;
                }

                parsed = new PivotFormula(tokens, usage);
                return true;
            }

            public object? Evaluate(PivotFormulaCellContext context)
            {
                if (_tokens.Length == 0)
                {
                    return null;
                }

                var stack = new Stack<double?>();
                for (var i = 0; i < _tokens.Length; i++)
                {
                    var token = _tokens[i];
                    switch (token.Kind)
                    {
                        case PivotFormulaTokenKind.Constant:
                            stack.Push(token.ConstantValue);
                            break;
                        case PivotFormulaTokenKind.Value:
                            stack.Push(ToNumber(context.ResolveValue(token.ValueIndex)));
                            break;
                        case PivotFormulaTokenKind.RowTotal:
                            stack.Push(ToNumber(context.ResolveRowTotal(token.ValueIndex)));
                            break;
                        case PivotFormulaTokenKind.ColumnTotal:
                            stack.Push(ToNumber(context.ResolveColumnTotal(token.ValueIndex)));
                            break;
                        case PivotFormulaTokenKind.GrandTotal:
                            stack.Push(ToNumber(context.ResolveGrandTotal(token.ValueIndex)));
                            break;
                        case PivotFormulaTokenKind.ParentRowTotal:
                            stack.Push(ToNumber(context.ResolveParentRowTotal(token.ValueIndex)));
                            break;
                        case PivotFormulaTokenKind.ParentColumnTotal:
                            stack.Push(ToNumber(context.ResolveParentColumnTotal(token.ValueIndex)));
                            break;
                        case PivotFormulaTokenKind.Negate:
                            if (!TryPop(stack, out var value))
                            {
                                return null;
                            }

                            stack.Push(value.HasValue ? -value.Value : null);
                            break;
                        case PivotFormulaTokenKind.Add:
                            if (!TryBinary(stack, static (a, b) => a + b))
                            {
                                return null;
                            }
                            break;
                        case PivotFormulaTokenKind.Subtract:
                            if (!TryBinary(stack, static (a, b) => a - b))
                            {
                                return null;
                            }
                            break;
                        case PivotFormulaTokenKind.Multiply:
                            if (!TryBinary(stack, static (a, b) => a * b))
                            {
                                return null;
                            }
                            break;
                        case PivotFormulaTokenKind.Divide:
                            if (!TryBinary(stack, static (a, b) => b == 0d ? null : a / b))
                            {
                                return null;
                            }
                            break;
                        default:
                            return null;
                    }
                }

                return stack.Count == 1 ? stack.Pop() : null;
            }

            private static double? ToNumber(object? value)
            {
                if (PivotNumeric.TryGetDouble(value, out var number))
                {
                    return number;
                }

                return null;
            }

            private static bool TryBinary(Stack<double?> stack, Func<double, double, double?> op)
            {
                if (!TryPop(stack, out var right) || !TryPop(stack, out var left))
                {
                    return false;
                }

                if (!left.HasValue || !right.HasValue)
                {
                    stack.Push(null);
                    return true;
                }

                var result = op(left.Value, right.Value);
                stack.Push(result);
                return true;
            }

            private static bool TryPop(Stack<double?> stack, out double? value)
            {
                if (stack.Count == 0)
                {
                    value = null;
                    return false;
                }

                value = stack.Pop();
                return true;
            }
        }

        private sealed class PivotFormulaParser
        {
            public static bool TryParse(
                string formula,
                Dictionary<string, int> fieldLookup,
                PivotFormulaUsage usage,
                out PivotFormulaToken[] tokens)
            {
                tokens = Array.Empty<PivotFormulaToken>();
                if (string.IsNullOrWhiteSpace(formula))
                {
                    return false;
                }

                var output = new List<PivotFormulaToken>();
                var operators = new Stack<PivotFormulaToken>();
                var scanner = new PivotFormulaScanner(formula);
                var expectUnary = true;

                while (scanner.TryReadToken(fieldLookup, usage, out var token))
                {
                    if (!HandleToken(token, output, operators, ref expectUnary))
                    {
                        return false;
                    }
                }

                if (scanner.HasError)
                {
                    return false;
                }

                while (operators.Count > 0)
                {
                    var op = operators.Pop();
                    if (op.Kind == PivotFormulaTokenKind.LeftParen || op.Kind == PivotFormulaTokenKind.RightParen)
                    {
                        return false;
                    }

                    output.Add(op);
                }

                tokens = output.ToArray();
                return tokens.Length > 0;
            }

            private static bool HandleToken(
                PivotFormulaToken token,
                List<PivotFormulaToken> output,
                Stack<PivotFormulaToken> operators,
                ref bool expectUnary)
            {
                switch (token.Kind)
                {
                    case PivotFormulaTokenKind.Constant:
                    case PivotFormulaTokenKind.Value:
                    case PivotFormulaTokenKind.RowTotal:
                    case PivotFormulaTokenKind.ColumnTotal:
                    case PivotFormulaTokenKind.GrandTotal:
                    case PivotFormulaTokenKind.ParentRowTotal:
                    case PivotFormulaTokenKind.ParentColumnTotal:
                        output.Add(token);
                        expectUnary = false;
                        return true;
                    case PivotFormulaTokenKind.LeftParen:
                        operators.Push(token);
                        expectUnary = true;
                        return true;
                    case PivotFormulaTokenKind.RightParen:
                        while (operators.Count > 0 && operators.Peek().Kind != PivotFormulaTokenKind.LeftParen)
                        {
                            output.Add(operators.Pop());
                        }

                        if (operators.Count == 0 || operators.Peek().Kind != PivotFormulaTokenKind.LeftParen)
                        {
                            return false;
                        }

                        operators.Pop();
                        expectUnary = false;
                        return true;
                    case PivotFormulaTokenKind.Add:
                    case PivotFormulaTokenKind.Subtract:
                    case PivotFormulaTokenKind.Multiply:
                    case PivotFormulaTokenKind.Divide:
                    case PivotFormulaTokenKind.Negate:
                        var op = token;
                        if (op.Kind == PivotFormulaTokenKind.Subtract && expectUnary)
                        {
                            op = PivotFormulaToken.Negate();
                        }

                        while (operators.Count > 0 && operators.Peek().Kind != PivotFormulaTokenKind.LeftParen)
                        {
                            var top = operators.Peek();
                            if (!ShouldPopOperator(op, top))
                            {
                                break;
                            }

                            output.Add(operators.Pop());
                        }

                        operators.Push(op);
                        expectUnary = true;
                        return true;
                    default:
                        return false;
                }
            }

            private static bool ShouldPopOperator(PivotFormulaToken current, PivotFormulaToken top)
            {
                var currentPrecedence = GetPrecedence(current.Kind);
                var topPrecedence = GetPrecedence(top.Kind);
                if (currentPrecedence < topPrecedence)
                {
                    return true;
                }

                if (currentPrecedence == topPrecedence && IsLeftAssociative(current.Kind))
                {
                    return true;
                }

                return false;
            }

            private static int GetPrecedence(PivotFormulaTokenKind kind)
            {
                return kind switch
                {
                    PivotFormulaTokenKind.Negate => 3,
                    PivotFormulaTokenKind.Multiply => 2,
                    PivotFormulaTokenKind.Divide => 2,
                    PivotFormulaTokenKind.Add => 1,
                    PivotFormulaTokenKind.Subtract => 1,
                    _ => 0
                };
            }

            private static bool IsLeftAssociative(PivotFormulaTokenKind kind)
            {
                return kind != PivotFormulaTokenKind.Negate;
            }
        }

        private sealed class PivotFormulaScanner
        {
            private readonly string _formula;
            private int _index;

            public PivotFormulaScanner(string formula)
            {
                _formula = formula;
            }

            public bool HasError { get; private set; }

            public bool TryReadToken(
                Dictionary<string, int> fieldLookup,
                PivotFormulaUsage usage,
                out PivotFormulaToken token)
            {
                token = PivotFormulaToken.None;
                SkipWhitespace();

                if (_index >= _formula.Length)
                {
                    return false;
                }

                var ch = _formula[_index];
                if (char.IsDigit(ch) || ch == '.')
                {
                    return TryReadNumber(out token);
                }

                if (IsIdentifierStart(ch))
                {
                    return TryReadIdentifier(fieldLookup, usage, out token);
                }

                if (ch == '[')
                {
                    return TryReadBracketedIdentifier(fieldLookup, out token);
                }

                switch (ch)
                {
                    case '+':
                        _index++;
                        token = PivotFormulaToken.Operator(PivotFormulaTokenKind.Add);
                        return true;
                    case '-':
                        _index++;
                        token = PivotFormulaToken.Operator(PivotFormulaTokenKind.Subtract);
                        return true;
                    case '*':
                        _index++;
                        token = PivotFormulaToken.Operator(PivotFormulaTokenKind.Multiply);
                        return true;
                    case '/':
                        _index++;
                        token = PivotFormulaToken.Operator(PivotFormulaTokenKind.Divide);
                        return true;
                    case '(':
                        _index++;
                        token = PivotFormulaToken.LeftParen();
                        return true;
                    case ')':
                        _index++;
                        token = PivotFormulaToken.RightParen();
                        return true;
                }

                HasError = true;
                return false;
            }

            private bool TryReadNumber(out PivotFormulaToken token)
            {
                token = PivotFormulaToken.None;
                var start = _index;
                var hasDecimal = false;

                while (_index < _formula.Length)
                {
                    var ch = _formula[_index];
                    if (char.IsDigit(ch))
                    {
                        _index++;
                        continue;
                    }

                    if (ch == '.' && !hasDecimal)
                    {
                        hasDecimal = true;
                        _index++;
                        continue;
                    }

                    if ((ch == 'e' || ch == 'E') && _index + 1 < _formula.Length)
                    {
                        var next = _formula[_index + 1];
                        if (char.IsDigit(next) || next == '+' || next == '-')
                        {
                            _index += 2;
                            while (_index < _formula.Length && char.IsDigit(_formula[_index]))
                            {
                                _index++;
                            }
                        }

                        break;
                    }

                    break;
                }

                var text = _formula.Substring(start, _index - start);
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    HasError = true;
                    return false;
                }

                token = PivotFormulaToken.Constant(value);
                return true;
            }

            private bool TryReadIdentifier(
                Dictionary<string, int> fieldLookup,
                PivotFormulaUsage usage,
                out PivotFormulaToken token)
            {
                token = PivotFormulaToken.None;
                var start = _index;
                _index++;
                while (_index < _formula.Length && IsIdentifierPart(_formula[_index]))
                {
                    _index++;
                }

                var name = _formula.Substring(start, _index - start).Trim();
                SkipWhitespace();

                if (_index < _formula.Length && _formula[_index] == '(')
                {
                    _index++;
                    if (!TryReadFunctionArgument(fieldLookup, out var fieldIndex))
                    {
                        HasError = true;
                        return false;
                    }

                    SkipWhitespace();
                    if (_index >= _formula.Length || _formula[_index] != ')')
                    {
                        HasError = true;
                        return false;
                    }

                    _index++;
                    if (!TryCreateFunctionToken(name, fieldIndex, usage, out token))
                    {
                        HasError = true;
                        return false;
                    }

                    return true;
                }

                if (!fieldLookup.TryGetValue(name, out var index))
                {
                    HasError = true;
                    return false;
                }

                token = PivotFormulaToken.Value(index);
                return true;
            }

            private bool TryReadBracketedIdentifier(
                Dictionary<string, int> fieldLookup,
                out PivotFormulaToken token)
            {
                token = PivotFormulaToken.None;
                _index++;
                var start = _index;
                while (_index < _formula.Length && _formula[_index] != ']')
                {
                    _index++;
                }

                if (_index >= _formula.Length)
                {
                    HasError = true;
                    return false;
                }

                var name = _formula.Substring(start, _index - start).Trim();
                _index++;

                if (!fieldLookup.TryGetValue(name, out var index))
                {
                    HasError = true;
                    return false;
                }

                token = PivotFormulaToken.Value(index);
                return true;
            }

            private bool TryReadFunctionArgument(Dictionary<string, int> fieldLookup, out int fieldIndex)
            {
                fieldIndex = -1;
                SkipWhitespace();

                if (_index >= _formula.Length)
                {
                    return false;
                }

                if (_formula[_index] == '[')
                {
                    _index++;
                    var start = _index;
                    while (_index < _formula.Length && _formula[_index] != ']')
                    {
                        _index++;
                    }

                    if (_index >= _formula.Length)
                    {
                        return false;
                    }

                    var name = _formula.Substring(start, _index - start).Trim();
                    _index++;
                    return fieldLookup.TryGetValue(name, out fieldIndex);
                }

                if (!IsIdentifierStart(_formula[_index]))
                {
                    return false;
                }

                var argStart = _index;
                _index++;
                while (_index < _formula.Length && IsIdentifierPart(_formula[_index]))
                {
                    _index++;
                }

                var arg = _formula.Substring(argStart, _index - argStart).Trim();
                return fieldLookup.TryGetValue(arg, out fieldIndex);
            }

        private static bool TryCreateFunctionToken(
            string name,
            int fieldIndex,
            PivotFormulaUsage usage,
            out PivotFormulaToken token)
        {
            token = PivotFormulaToken.None;
            if (fieldIndex < 0)
            {
                return false;
            }

            if (string.Equals(name, "RowTotal", StringComparison.OrdinalIgnoreCase))
            {
                usage.MarkRowTotals();
                token = PivotFormulaToken.RowTotal(fieldIndex);
                return true;
            }

            if (string.Equals(name, "ColumnTotal", StringComparison.OrdinalIgnoreCase))
            {
                usage.MarkColumnTotals();
                token = PivotFormulaToken.ColumnTotal(fieldIndex);
                return true;
            }

            if (string.Equals(name, "GrandTotal", StringComparison.OrdinalIgnoreCase))
            {
                usage.MarkGrandTotals();
                token = PivotFormulaToken.GrandTotal(fieldIndex);
                return true;
            }

            if (string.Equals(name, "ParentRowTotal", StringComparison.OrdinalIgnoreCase))
            {
                usage.MarkParentRowTotals();
                token = PivotFormulaToken.ParentRowTotal(fieldIndex);
                return true;
            }

            if (string.Equals(name, "ParentColumnTotal", StringComparison.OrdinalIgnoreCase))
            {
                usage.MarkParentColumnTotals();
                token = PivotFormulaToken.ParentColumnTotal(fieldIndex);
                return true;
            }

            return false;
        }

            private void SkipWhitespace()
            {
                while (_index < _formula.Length && char.IsWhiteSpace(_formula[_index]))
                {
                    _index++;
                }
            }

            private static bool IsIdentifierStart(char ch)
            {
                return char.IsLetter(ch) || ch == '_';
            }

            private static bool IsIdentifierPart(char ch)
            {
                return char.IsLetterOrDigit(ch) || ch == '_' || ch == '.';
            }
        }

        private readonly struct PivotFormulaToken
        {
            public static readonly PivotFormulaToken None = new(PivotFormulaTokenKind.None, 0d, -1);

            public PivotFormulaToken(PivotFormulaTokenKind kind, double constant, int valueIndex)
            {
                Kind = kind;
                ConstantValue = constant;
                ValueIndex = valueIndex;
            }

            public PivotFormulaTokenKind Kind { get; }

            public double ConstantValue { get; }

            public int ValueIndex { get; }

            public static PivotFormulaToken Constant(double value) => new(PivotFormulaTokenKind.Constant, value, -1);

            public static PivotFormulaToken Value(int index) => new(PivotFormulaTokenKind.Value, 0d, index);

            public static PivotFormulaToken RowTotal(int index) => new(PivotFormulaTokenKind.RowTotal, 0d, index);

            public static PivotFormulaToken ColumnTotal(int index) => new(PivotFormulaTokenKind.ColumnTotal, 0d, index);

            public static PivotFormulaToken GrandTotal(int index) => new(PivotFormulaTokenKind.GrandTotal, 0d, index);

            public static PivotFormulaToken ParentRowTotal(int index) => new(PivotFormulaTokenKind.ParentRowTotal, 0d, index);

            public static PivotFormulaToken ParentColumnTotal(int index) => new(PivotFormulaTokenKind.ParentColumnTotal, 0d, index);

            public static PivotFormulaToken Operator(PivotFormulaTokenKind kind) => new(kind, 0d, -1);

            public static PivotFormulaToken Negate() => new(PivotFormulaTokenKind.Negate, 0d, -1);

            public static PivotFormulaToken LeftParen() => new(PivotFormulaTokenKind.LeftParen, 0d, -1);

            public static PivotFormulaToken RightParen() => new(PivotFormulaTokenKind.RightParen, 0d, -1);
        }

        private enum PivotFormulaTokenKind
        {
            None,
            Constant,
            Value,
            RowTotal,
            ColumnTotal,
            GrandTotal,
            ParentRowTotal,
            ParentColumnTotal,
            Add,
            Subtract,
            Multiply,
            Divide,
            Negate,
            LeftParen,
            RightParen
        }
    }
}
