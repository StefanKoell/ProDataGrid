// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;

namespace Avalonia.Controls.DataGridPivoting
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    interface IPivotRow
    {
        PivotRowType RowType { get; }

        int Level { get; }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotHeader
    {
        public PivotHeader(IReadOnlyList<string> segments)
        {
            Segments = segments;
        }

        public IReadOnlyList<string> Segments { get; }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotColumn
    {
        public PivotColumn(
            int index,
            PivotColumnType columnType,
            object?[] columnPathValues,
            string?[] columnDisplayValues,
            PivotValueField? valueField,
            int? valueFieldIndex,
            PivotHeader header)
        {
            Index = index;
            ColumnType = columnType;
            ColumnPathValues = columnPathValues;
            ColumnDisplayValues = columnDisplayValues;
            ValueField = valueField;
            ValueFieldIndex = valueFieldIndex;
            Header = header;
        }

        public int Index { get; }

        public PivotColumnType ColumnType { get; }

        public object?[] ColumnPathValues { get; }

        public string?[] ColumnDisplayValues { get; }

        public PivotValueField? ValueField { get; }

        public int? ValueFieldIndex { get; }

        public PivotHeader Header { get; }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotRow : IPivotRow
    {
        private readonly object?[] _cellValues;

        public PivotRow(
            PivotRowType rowType,
            int level,
            object?[] rowPathValues,
            object?[] rowDisplayValues,
            string? compactLabel,
            double indent,
            int columnCount,
            PivotValueField? valueField,
            int? valueFieldIndex)
        {
            RowType = rowType;
            Level = level;
            RowPathValues = rowPathValues;
            RowDisplayValues = rowDisplayValues;
            CompactLabel = compactLabel;
            Indent = indent;
            ValueField = valueField;
            ValueFieldIndex = valueFieldIndex;
            _cellValues = new object?[columnCount];
        }

        public PivotRowType RowType { get; }

        public int Level { get; }

        public object?[] RowPathValues { get; }

        public object?[] RowDisplayValues { get; }

        public string? CompactLabel { get; }

        public double Indent { get; }

        public PivotValueField? ValueField { get; }

        public int? ValueFieldIndex { get; }

        public object?[] CellValues => _cellValues;

        internal void SetCellValue(int index, object? value)
        {
            if (index >= 0 && index < _cellValues.Length)
            {
                _cellValues[index] = value;
            }
        }
    }
}
