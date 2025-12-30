// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

#nullable disable

using Avalonia.Collections;
using Avalonia.Controls.Utils;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Avalonia.Controls
{
    /// <summary>
    /// Validation handling
    /// </summary>
#if !DATAGRID_INTERNAL
public
#else
internal
#endif
    partial class DataGrid
    {

        //TODO Validation UI
        private void ResetValidationStatus(DataGridCell editingCell = null)
        {
            // Clear the invalid status of the Cell, Row and DataGrid
            if (EditingRow != null)
            {
                if (EditingRow.Index != -1)
                {
                    if (editingCell != null)
                    {
                        ClearCellValidation(editingCell);
                    }
                    else
                    {
                        foreach (DataGridCell cell in EditingRow.Cells)
                        {
                            ClearCellValidation(cell);
                        }
                    }
                    UpdateRowValidationStateFromCells(EditingRow);
                }
            }
            UpdateGridValidationState();

            _validationSubscription?.Dispose();
            _validationSubscription = null;
        }

        private static void ClearCellValidation(DataGridCell cell)
        {
            if (!cell.IsValid || cell.ValidationSeverity != DataGridValidationSeverity.None)
            {
                cell.IsValid = true;
                cell.ValidationSeverity = DataGridValidationSeverity.None;
                cell.UpdatePseudoClasses();
            }

            DataValidationErrors.ClearErrors(cell);
        }

        private void RestoreRowValidationState(DataGridRow row, object item, bool clearIfNoIndei = true)
        {
            if (row is null)
            {
                return;
            }

            if (item is null || ReferenceEquals(item, DataGridCollectionView.NewItemPlaceholder))
            {
                ClearRowValidation(row);
                return;
            }

            if (item is not INotifyDataErrorInfo notifyDataErrorInfo)
            {
                if (clearIfNoIndei)
                {
                    ClearRowValidation(row);
                }
                return;
            }

            foreach (var column in ColumnsItemsInternal)
            {
                if (column.Index < 0 || column.Index >= row.Cells.Count)
                {
                    continue;
                }

                var cell = row.Cells[column.Index];
                var bindingPath = GetColumnBindingPath(column);

                if (string.IsNullOrWhiteSpace(bindingPath))
                {
                    continue;
                }

                var errors = notifyDataErrorInfo.GetErrors(bindingPath);
                if (errors is null)
                {
                    ClearCellValidation(cell);
                    continue;
                }

                var exceptions = CreateValidationExceptions(errors);
                if (exceptions.Count == 0)
                {
                    ClearCellValidation(cell);
                    continue;
                }

                var severity = ValidationUtil.GetValidationSeverity(exceptions);
                cell.IsValid = severity != DataGridValidationSeverity.Error;
                cell.ValidationSeverity = severity;
                cell.UpdatePseudoClasses();

                var errorException = exceptions.Count == 1
                    ? exceptions[0]
                    : new AggregateException(exceptions);
                DataValidationErrors.SetError(cell, errorException);
            }

            UpdateRowValidationStateFromCells(row);
            UpdateGridValidationState();
        }

        private void ClearRowValidation(DataGridRow row)
        {
            if (row is null)
            {
                return;
            }

            row.IsValid = true;
            row.ValidationSeverity = DataGridValidationSeverity.None;

            foreach (DataGridCell cell in row.Cells)
            {
                ClearCellValidation(cell);
            }

            row.ApplyState();
            UpdateGridValidationState();
        }

        private static bool RowHasError(DataGridRow row)
        {
            foreach (DataGridCell cell in row.Cells)
            {
                if (cell.ValidationSeverity == DataGridValidationSeverity.Error)
                {
                    return true;
                }
            }

            return false;
        }

        private void UpdateRowValidationStateFromCells(DataGridRow row)
        {
            if (row is null)
            {
                return;
            }

            var hasError = RowHasError(row);
            row.IsValid = !hasError;
            row.ValidationSeverity = hasError ? DataGridValidationSeverity.Error : DataGridValidationSeverity.None;
            row.ApplyState();
        }

        private void UpdateGridValidationState()
        {
            var hasError = false;

            if (DisplayData != null)
            {
                for (int slot = DisplayData.FirstScrollingSlot;
                    slot > -1 && slot <= DisplayData.LastScrollingSlot;
                    slot++)
                {
                    if (DisplayData.GetDisplayedElement(slot) is DataGridRow row &&
                        (!row.IsValid || row.ValidationSeverity == DataGridValidationSeverity.Error))
                    {
                        hasError = true;
                        break;
                    }
                }
            }

            if (!hasError && EditingRow != null && !EditingRow.IsValid)
            {
                hasError = true;
            }

            if (!hasError && CollectionViewHasError())
            {
                hasError = true;
            }

            IsValid = !hasError;
        }

        private bool CollectionViewHasError()
        {
            var collectionView = DataConnection?.CollectionView;
            if (collectionView == null)
            {
                return false;
            }

            foreach (var item in collectionView)
            {
                if (item == null || ReferenceEquals(item, DataGridCollectionView.NewItemPlaceholder))
                {
                    continue;
                }

                if (item is not INotifyDataErrorInfo notifyDataErrorInfo || !notifyDataErrorInfo.HasErrors)
                {
                    continue;
                }

                if (ItemHasError(notifyDataErrorInfo))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ItemHasError(INotifyDataErrorInfo notifyDataErrorInfo)
        {
            foreach (var column in ColumnsItemsInternal)
            {
                var bindingPath = GetColumnBindingPath(column);
                if (string.IsNullOrWhiteSpace(bindingPath))
                {
                    continue;
                }

                var errors = notifyDataErrorInfo.GetErrors(bindingPath);
                if (errors is null)
                {
                    continue;
                }

                var exceptions = CreateValidationExceptions(errors);
                if (exceptions.Count == 0)
                {
                    continue;
                }

                if (ValidationUtil.GetValidationSeverity(exceptions) == DataGridValidationSeverity.Error)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetBindingPath(IBinding binding)
        {
            if (binding is Binding avaloniaBinding)
            {
                return avaloniaBinding.Path;
            }

            if (binding is CompiledBindingExtension compiledBinding)
            {
                return compiledBinding.Path?.ToString();
            }

            return null;
        }

        private static string GetColumnBindingPath(DataGridColumn column)
        {
            if (column is DataGridBoundColumn boundColumn)
            {
                return boundColumn.Binding is { } binding
                    ? GetBindingPath(binding)
                    : null;
            }

            if (column is DataGridComboBoxColumn comboBoxColumn)
            {
                return comboBoxColumn.EffectiveBinding is { } binding
                    ? GetBindingPath(binding)
                    : null;
            }

            return null;
        }

        private static List<Exception> CreateValidationExceptions(IEnumerable errors)
        {
            var exceptions = new List<Exception>();

            foreach (var error in errors)
            {
                if (error is null)
                {
                    continue;
                }

                if (error is Exception exception)
                {
                    exceptions.Add(exception);
                }
                else
                {
                    exceptions.Add(new DataValidationException(error));
                }
            }

            return exceptions;
        }

        private List<Exception> _bindingValidationErrors;

        private IDisposable _validationSubscription;

        private bool _isValid = true;


        public bool IsValid
        {
            get { return _isValid; }
            internal set
            {
                SetAndRaise(IsValidProperty, ref _isValid, value);
                PseudoClassesHelper.Set(PseudoClasses, ":invalid", !value);
            }
        }

    }
}
