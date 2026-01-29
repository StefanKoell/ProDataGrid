// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;

namespace ProDataGrid.FormulaEngine
{
    public interface IFormulaCalculationObserver
    {
        void OnRecalculationStarted(IFormulaWorkbook workbook, IReadOnlyCollection<FormulaCellAddress> dirtyCells);

        void OnRecalculationCompleted(
            IFormulaWorkbook workbook,
            IReadOnlyList<FormulaCellAddress> recalculated,
            IReadOnlyList<FormulaCellAddress> cycle,
            TimeSpan duration);

        void OnCellEvaluated(FormulaCellAddress address, FormulaValue value, TimeSpan duration);

        void OnExpressionParsed(FormulaCellAddress address, string formulaText, TimeSpan duration);

        void OnExpressionCompiled(
            FormulaExpression expression,
            int instructionCount,
            TimeSpan duration,
            bool fromCache);
    }

    public sealed class FormulaCalculationTelemetry : IFormulaCalculationObserver
    {
        private long _parseTicks;
        private long _compileTicks;
        private long _evaluationTicks;
        private long _recalcTicks;
        private int _parsedExpressions;
        private int _compiledExpressions;
        private int _compileCacheHits;
        private int _cellsEvaluated;
        private int _recalculations;

        public int ParsedExpressions => _parsedExpressions;

        public int CompiledExpressions => _compiledExpressions;

        public int CompileCacheHits => _compileCacheHits;

        public int CellsEvaluated => _cellsEvaluated;

        public int Recalculations => _recalculations;

        public TimeSpan ParseTime => TimeSpan.FromTicks(_parseTicks);

        public TimeSpan CompileTime => TimeSpan.FromTicks(_compileTicks);

        public TimeSpan EvaluationTime => TimeSpan.FromTicks(_evaluationTicks);

        public TimeSpan RecalculationTime => TimeSpan.FromTicks(_recalcTicks);

        public void Reset()
        {
            _parseTicks = 0;
            _compileTicks = 0;
            _evaluationTicks = 0;
            _recalcTicks = 0;
            _parsedExpressions = 0;
            _compiledExpressions = 0;
            _compileCacheHits = 0;
            _cellsEvaluated = 0;
            _recalculations = 0;
        }

        public void OnRecalculationStarted(IFormulaWorkbook workbook, IReadOnlyCollection<FormulaCellAddress> dirtyCells)
        {
        }

        public void OnRecalculationCompleted(
            IFormulaWorkbook workbook,
            IReadOnlyList<FormulaCellAddress> recalculated,
            IReadOnlyList<FormulaCellAddress> cycle,
            TimeSpan duration)
        {
            Interlocked.Increment(ref _recalculations);
            Interlocked.Add(ref _recalcTicks, duration.Ticks);
        }

        public void OnCellEvaluated(FormulaCellAddress address, FormulaValue value, TimeSpan duration)
        {
            Interlocked.Increment(ref _cellsEvaluated);
            Interlocked.Add(ref _evaluationTicks, duration.Ticks);
        }

        public void OnExpressionParsed(FormulaCellAddress address, string formulaText, TimeSpan duration)
        {
            Interlocked.Increment(ref _parsedExpressions);
            Interlocked.Add(ref _parseTicks, duration.Ticks);
        }

        public void OnExpressionCompiled(
            FormulaExpression expression,
            int instructionCount,
            TimeSpan duration,
            bool fromCache)
        {
            if (fromCache)
            {
                Interlocked.Increment(ref _compileCacheHits);
                return;
            }

            Interlocked.Increment(ref _compiledExpressions);
            Interlocked.Add(ref _compileTicks, duration.Ticks);
        }
    }
}
