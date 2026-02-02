using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using ProDataGrid.ExcelSample.Models;

namespace ProDataGrid.ExcelSample.Helpers;

public sealed class DataGridSelectionBinder
{
    public static readonly AttachedProperty<SpreadsheetSelectionState?> SelectionStateProperty =
        AvaloniaProperty.RegisterAttached<DataGridSelectionBinder, DataGrid, SpreadsheetSelectionState?>(
            "SelectionState");

    private static readonly ConditionalWeakTable<DataGrid, SelectionSubscription> Subscriptions = new();

    static DataGridSelectionBinder()
    {
        SelectionStateProperty.Changed.AddClassHandler<DataGrid>(OnSelectionStateChanged);
    }

    public static void SetSelectionState(AvaloniaObject element, SpreadsheetSelectionState? value)
    {
        element.SetValue(SelectionStateProperty, value);
    }

    public static SpreadsheetSelectionState? GetSelectionState(AvaloniaObject element)
    {
        return element.GetValue(SelectionStateProperty);
    }

    private static void OnSelectionStateChanged(DataGrid grid, AvaloniaPropertyChangedEventArgs args)
    {
        var subscription = Subscriptions.GetOrCreateValue(grid);
        subscription.Detach();
        subscription.Attach(grid, args.NewValue as SpreadsheetSelectionState);
    }

    private sealed class SelectionSubscription
    {
        private DataGrid? _grid;
        private SpreadsheetSelectionState? _state;
        private bool _isUpdating;
        private AvaloniaList<DataGridCellInfo>? _boundSelectedCells;
        private IList<DataGridCellInfo>? _previousSelectedCells;

        public void Attach(DataGrid grid, SpreadsheetSelectionState? state)
        {
            _grid = grid;
            _state = state;
            if (_grid == null || _state == null)
            {
                return;
            }

            _previousSelectedCells = _grid.SelectedCells;
            _boundSelectedCells = new AvaloniaList<DataGridCellInfo>();
            _grid.SelectedCells = _boundSelectedCells;

            _grid.CurrentCellChanged += GridOnCurrentCellChanged;
            _grid.SelectedCellsChanged += GridOnSelectedCellsChanged;
            _state.PropertyChanged += StateOnPropertyChanged;

            if (_state.CurrentCell.HasValue || _state.SelectedRange.HasValue)
            {
                UpdateGridSelection();
            }
            else
            {
                UpdateState();
            }
        }

        public void Detach()
        {
            if (_grid != null)
            {
                _grid.CurrentCellChanged -= GridOnCurrentCellChanged;
                _grid.SelectedCellsChanged -= GridOnSelectedCellsChanged;

                if (_previousSelectedCells != null)
                {
                    _grid.SelectedCells = _previousSelectedCells;
                }
            }

            if (_state != null)
            {
                _state.PropertyChanged -= StateOnPropertyChanged;
            }

            _grid = null;
            _state = null;
            _isUpdating = false;
            _boundSelectedCells = null;
            _previousSelectedCells = null;
        }

        private void GridOnCurrentCellChanged(object? sender, DataGridCurrentCellChangedEventArgs e)
        {
            UpdateState();
        }

        private void GridOnSelectedCellsChanged(object? sender, DataGridSelectedCellsChangedEventArgs e)
        {
            UpdateState();
        }

        private void StateOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SpreadsheetSelectionState.CurrentCell) &&
                e.PropertyName != nameof(SpreadsheetSelectionState.SelectedRange))
            {
                return;
            }

            UpdateGridSelection();
        }

        private void UpdateState()
        {
            if (_grid == null || _state == null || _isUpdating)
            {
                return;
            }

            _isUpdating = true;
            try
            {
                var current = _grid.CurrentCell;
                _state.CurrentCell = current.IsValid
                    ? new SpreadsheetCellReference(current.RowIndex, current.ColumnIndex)
                    : null;

                var selected = _grid.SelectedCells;
                if (selected == null || selected.Count == 0)
                {
                    _state.SelectedRange = _state.CurrentCell.HasValue
                        ? new SpreadsheetCellRange(_state.CurrentCell.Value, _state.CurrentCell.Value)
                        : null;
                    return;
                }

                var minRow = int.MaxValue;
                var maxRow = int.MinValue;
                var minColumn = int.MaxValue;
                var maxColumn = int.MinValue;

                for (var i = 0; i < selected.Count; i++)
                {
                    var cell = selected[i];
                    if (!cell.IsValid)
                    {
                        continue;
                    }

                    minRow = Math.Min(minRow, cell.RowIndex);
                    maxRow = Math.Max(maxRow, cell.RowIndex);
                    minColumn = Math.Min(minColumn, cell.ColumnIndex);
                    maxColumn = Math.Max(maxColumn, cell.ColumnIndex);
                }

                if (minRow == int.MaxValue)
                {
                    _state.SelectedRange = null;
                    return;
                }

                var start = new SpreadsheetCellReference(minRow, minColumn);
                var end = new SpreadsheetCellReference(maxRow, maxColumn);
                _state.SelectedRange = new SpreadsheetCellRange(start, end);
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateGridSelection()
        {
            if (_grid == null || _state == null || _isUpdating)
            {
                return;
            }

            var range = _state.SelectedRange ?? (_state.CurrentCell.HasValue
                ? new SpreadsheetCellRange(_state.CurrentCell.Value, _state.CurrentCell.Value)
                : (SpreadsheetCellRange?)null);

            if (!range.HasValue)
            {
                _isUpdating = true;
                try
                {
                    _grid.SelectedCells.Clear();
                    _grid.CurrentCell = DataGridCellInfo.Unset;
                }
                finally
                {
                    _isUpdating = false;
                }

                return;
            }

            if (_grid.ItemsSource is not IList items)
            {
                return;
            }

            var columnCount = _grid.Columns.Count;
            var rowCount = items.Count;
            if (columnCount == 0 || rowCount == 0)
            {
                return;
            }

            var startRow = Math.Clamp(range.Value.Start.RowIndex, 0, rowCount - 1);
            var endRow = Math.Clamp(range.Value.End.RowIndex, 0, rowCount - 1);
            var startColumn = Math.Clamp(range.Value.Start.ColumnIndex, 0, columnCount - 1);
            var endColumn = Math.Clamp(range.Value.End.ColumnIndex, 0, columnCount - 1);

            _isUpdating = true;
            try
            {
                var selectedCells = _boundSelectedCells ?? _grid.SelectedCells;
                selectedCells.Clear();

                for (var rowIndex = startRow; rowIndex <= endRow; rowIndex++)
                {
                    var item = items[rowIndex];
                    for (var columnIndex = startColumn; columnIndex <= endColumn; columnIndex++)
                    {
                        var column = _grid.Columns[columnIndex];
                        selectedCells.Add(new DataGridCellInfo(item, column, rowIndex, columnIndex, isValid: true));
                    }
                }

                var currentItem = items[startRow];
                var currentColumn = _grid.Columns[startColumn];
                _grid.CurrentCell = new DataGridCellInfo(currentItem, currentColumn, startRow, startColumn, isValid: true);
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}
