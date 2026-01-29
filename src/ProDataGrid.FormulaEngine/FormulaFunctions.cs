// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Collections.Generic;

namespace ProDataGrid.FormulaEngine
{
    public sealed class FormulaFunctionInfo
    {
        public FormulaFunctionInfo(int minArgs, int maxArgs, bool isVolatile = false)
        {
            MinArgs = minArgs;
            MaxArgs = maxArgs;
            IsVolatile = isVolatile;
        }

        public int MinArgs { get; }

        public int MaxArgs { get; }

        public bool IsVolatile { get; }

        public bool IsVariadic => MaxArgs < 0;
    }

    public sealed class FormulaFunctionContext
    {
        public FormulaFunctionContext(FormulaEvaluationContext evaluationContext)
        {
            EvaluationContext = evaluationContext;
        }

        public FormulaEvaluationContext EvaluationContext { get; }
    }

    public interface IFormulaFunction
    {
        string Name { get; }

        FormulaFunctionInfo Info { get; }

        FormulaValue Invoke(FormulaFunctionContext context, IReadOnlyList<FormulaValue> args);
    }

    public interface ILazyFormulaFunction : IFormulaFunction
    {
        FormulaValue InvokeLazy(
            FormulaFunctionContext context,
            IReadOnlyList<FormulaExpression> arguments,
            FormulaEvaluator evaluator,
            IFormulaValueResolver resolver);
    }

    public interface IFormulaFunctionRegistry
    {
        bool TryGetFunction(string name, out IFormulaFunction function);

        IEnumerable<IFormulaFunction> GetAll();
    }
}
