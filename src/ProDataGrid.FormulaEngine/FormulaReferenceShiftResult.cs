// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Collections.Generic;

namespace ProDataGrid.FormulaEngine
{
    public sealed class FormulaReferenceShiftResult
    {
        public FormulaReferenceShiftResult(
            IReadOnlyList<FormulaCellAddress> updatedCells,
            IReadOnlyList<FormulaCellAddress> removedCells)
        {
            UpdatedCells = updatedCells ?? new List<FormulaCellAddress>();
            RemovedCells = removedCells ?? new List<FormulaCellAddress>();
        }

        public IReadOnlyList<FormulaCellAddress> UpdatedCells { get; }

        public IReadOnlyList<FormulaCellAddress> RemovedCells { get; }
    }
}
