// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

#nullable disable

using Avalonia.Collections;
using Avalonia.Controls.Utils;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Avalonia.Controls
{
    /// <summary>
    /// Selection management
    /// </summary>
#if !DATAGRID_INTERNAL
    public
#endif
    partial class DataGrid
    {

        public void SelectAll()
        {
            SetRowsSelection(0, SlotCount - 1);
        }


        /// <summary>
        /// Selects items and updates currency based on parameters
        /// </summary>
        /// <param name="columnIndex">column index to make current</param>
        /// <param name="item">data item or CollectionViewGroup to make current</param>
        /// <param name="backupSlot">slot to use in case the item is no longer valid</param>
        /// <param name="action">selection action to perform</param>
        /// <param name="scrollIntoView">whether or not the new current item should be scrolled into view</param>
        internal void ProcessSelectionAndCurrency(int columnIndex, object item, int backupSlot, DataGridSelectionAction action, bool scrollIntoView)
        {
            _noSelectionChangeCount++;
            _noCurrentCellChangeCount++;
            try
            {
                int slot = -1;
                if (item is DataGridCollectionViewGroup group)
                {
                    DataGridRowGroupInfo groupInfo = RowGroupInfoFromCollectionViewGroup(group);
                    if (groupInfo != null)
                    {
                        slot = groupInfo.Slot;
                    }
                }
                else
                {
                    slot = SlotFromRowIndex(DataConnection.IndexOf(item));
                }
                if (slot == -1)
                {
                    slot = backupSlot;
                }
                if (slot < 0 || slot > SlotCount)
                {
                    return;
                }

                switch (action)
                {
                    case DataGridSelectionAction.AddCurrentToSelection:
                        SetRowSelection(slot, isSelected: true, setAnchorSlot: true);
                        break;
                    case DataGridSelectionAction.RemoveCurrentFromSelection:
                        SetRowSelection(slot, isSelected: false, setAnchorSlot: false);
                        break;
                    case DataGridSelectionAction.SelectFromAnchorToCurrent:
                        if (SelectionMode == DataGridSelectionMode.Extended && AnchorSlot != -1)
                        {
                            int anchorSlot = AnchorSlot;
                            if (slot <= anchorSlot)
                            {
                                SetRowsSelection(slot, anchorSlot);
                            }
                            else
                            {
                                SetRowsSelection(anchorSlot, slot);
                            }
                        }
                        else
                        {
                            goto case DataGridSelectionAction.SelectCurrent;
                        }
                        break;
                    case DataGridSelectionAction.SelectCurrent:
                        ClearRowSelection(slot, setAnchorSlot: true);
                        break;
                    case DataGridSelectionAction.None:
                        break;
                }

                if (CurrentSlot != slot || (CurrentColumnIndex != columnIndex && columnIndex != -1))
                {
                    if (columnIndex == -1)
                    {
                        if (CurrentColumnIndex != -1)
                        {
                            columnIndex = CurrentColumnIndex;
                        }
                        else
                        {
                            DataGridColumn firstVisibleColumn = ColumnsInternal.FirstVisibleNonFillerColumn;
                            if (firstVisibleColumn != null)
                            {
                                columnIndex = firstVisibleColumn.Index;
                            }
                        }
                    }
                    if (columnIndex != -1)
                    {
                        if (!SetCurrentCellCore(
                                columnIndex, slot,
                                commitEdit: true,
                                endRowEdit: SlotFromRowIndex(SelectedIndex) != slot)
                            || (scrollIntoView &&
                                !ScrollSlotIntoView(
                                    columnIndex, slot,
                                    forCurrentCellChange: true,
                                    forceHorizontalScroll: false)))
                        {
                            return;
                        }
                    }
                }
                _successfullyUpdatedSelection = true;
            }
            finally
            {
                NoCurrentCellChangeCount--;
                NoSelectionChangeCount--;
            }
        }


        internal bool UpdateSelectionAndCurrency(int columnIndex, int slot, DataGridSelectionAction action, bool scrollIntoView)
        {
            _successfullyUpdatedSelection = false;

            _noSelectionChangeCount++;
            _noCurrentCellChangeCount++;
            try
            {
                if (ColumnsInternal.RowGroupSpacerColumn.IsRepresented &&
                    columnIndex == ColumnsInternal.RowGroupSpacerColumn.Index)
                {
                    columnIndex = -1;
                }
                if (IsSlotOutOfSelectionBounds(slot) || (columnIndex != -1 && IsColumnOutOfBounds(columnIndex)))
                {
                    return false;
                }

                int newCurrentPosition = -1;
                object item = ItemFromSlot(slot, ref newCurrentPosition);

                if (EditingRow != null && slot != EditingRow.Slot && !CommitEdit(DataGridEditingUnit.Row, true))
                {
                    return false;
                }

                if (DataConnection.CollectionView != null &&
                    DataConnection.CollectionView.CurrentPosition != newCurrentPosition)
                {
                    DataConnection.MoveCurrentTo(item, slot, columnIndex, action, scrollIntoView);
                }
                else
                {
                    ProcessSelectionAndCurrency(columnIndex, item, slot, action, scrollIntoView);
                }
            }
            finally
            {
                NoCurrentCellChangeCount--;
                NoSelectionChangeCount--;
            }

            return _successfullyUpdatedSelection;
        }


        private void SetAndSelectCurrentCell(int columnIndex,
                                             int slot,
                                             bool forceCurrentCellSelection)
        {
            DataGridSelectionAction action = forceCurrentCellSelection ? DataGridSelectionAction.SelectCurrent : DataGridSelectionAction.None;
            UpdateSelectionAndCurrency(columnIndex, slot, action, scrollIntoView: false);
        }


        private void FlushSelectionChanged()
        {
            if (SelectionHasChanged && _noSelectionChangeCount == 0 && !_makeFirstDisplayedCellCurrentCellPending)
            {
                CoerceSelectedItem();
                if (NoCurrentCellChangeCount != 0)
                {
                    // current cell is changing, don't raise SelectionChanged until it's done
                    return;
                }
                SelectionHasChanged = false;

                if (_flushCurrentCellChanged)
                {
                    FlushCurrentCellChanged();
                }

                SelectionChangedEventArgs e = _selectedItems.GetSelectionChangedEventArgs();
                if (e.AddedItems.Count > 0 || e.RemovedItems.Count > 0)
                {
                    OnSelectionChanged(e);
                }
            }
        }


        /// <summary>
        /// call when: selection changes or SelectedItems object changes
        /// </summary>
        internal void CoerceSelectedItem()
        {
            object selectedItem = null;

            if (SelectionMode == DataGridSelectionMode.Extended &&
                CurrentSlot != -1 &&
                _selectedItems.ContainsSlot(CurrentSlot))
            {
                selectedItem = CurrentItem;
            }
            else if (_selectedItems.Count > 0)
            {
                selectedItem = _selectedItems[0];
            }

            SetValueNoCallback(SelectedItemProperty, selectedItem);

            // Update the SelectedIndex
            int newIndex = -1;

            if (selectedItem != null)
            {
                newIndex = DataConnection.IndexOf(selectedItem);
            }

            SetValueNoCallback(SelectedIndexProperty, newIndex);
        }


        internal IEnumerable<object> GetSelectionInclusive(int startRowIndex, int endRowIndex)
        {
            int endSlot = SlotFromRowIndex(endRowIndex);
            foreach (int slot in _selectedItems.GetSlots(SlotFromRowIndex(startRowIndex)))
            {
                if (slot > endSlot)
                {
                    break;
                }
                yield return DataConnection.GetDataItem(RowIndexFromSlot(slot));
            }
        }


        /// <summary>
        /// Raises the SelectionChanged event and clears the _selectionChanged.
        /// This event won't get raised again until after _selectionChanged is set back to true.
        /// </summary>
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            RaiseEvent(e);
        }

        private int _noSelectionChangeCount;

        private bool _successfullyUpdatedSelection;


        /// <summary>
        /// Occurs when the <see cref="P:Avalonia.Controls.DataGrid.SelectedItem" /> or
        /// <see cref="P:Avalonia.Controls.DataGrid.SelectedItems" /> property value changes.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }


        private int NoSelectionChangeCount
        {
            get
            {
                return _noSelectionChangeCount;
            }
            set
            {
                _noSelectionChangeCount = value;
                if (value == 0)
                {
                    FlushSelectionChanged();
                }
            }
        }


        // This flag indicates whether selection has actually changed during a selection operation,
        // and exists to ensure that FlushSelectionChanged doesn't unnecessarily raise SelectionChanged.
        internal bool SelectionHasChanged
        {
            get;
            set;
        }


        internal int AnchorSlot
        {
            get;
            private set;
        }


        private void OnSelectedIndexChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (!_areHandlersSuspended)
            {
                int index = (int)e.NewValue;

                // GetDataItem returns null if index is >= Count, we do not check newValue
                // against Count here to avoid enumerating through an Enumerable twice
                // Setting SelectedItem coerces the finally value of the SelectedIndex
                object newSelectedItem = (index < 0) ? null : DataConnection.GetDataItem(index);
                SelectedItem = newSelectedItem;
                if (SelectedItem != newSelectedItem)
                {
                    SetValueNoCallback(SelectedIndexProperty, (int)e.OldValue);
                }
            }
        }

        private void OnSelectedItemChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (!_areHandlersSuspended)
            {
                int rowIndex = (e.NewValue == null) ? -1 : DataConnection.IndexOf(e.NewValue);
                if (rowIndex == -1)
                {
                    // If the Item is null or it's not found, clear the Selection
                    if (!CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true))
                    {
                        // Edited value couldn't be committed or aborted
                        SetValueNoCallback(SelectedItemProperty, e.OldValue);
                        return;
                    }

                    // Clear all row selections
                    ClearRowSelection(resetAnchorSlot: true);

                    if (DataConnection.CollectionView != null)
                    {
                        DataConnection.CollectionView.MoveCurrentTo(null);
                    }
                }
                else
                {
                    int slot = SlotFromRowIndex(rowIndex);
                    if (slot != CurrentSlot)
                    {
                        if (!CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true))
                        {
                            // Edited value couldn't be committed or aborted
                            SetValueNoCallback(SelectedItemProperty, e.OldValue);
                            return;
                        }
                        if (slot >= SlotCount || slot < -1)
                        {
                            if (DataConnection.CollectionView != null)
                            {
                                DataConnection.CollectionView.MoveCurrentToPosition(rowIndex);
                            }
                        }
                    }

                    int oldSelectedIndex = SelectedIndex;
                    SetValueNoCallback(SelectedIndexProperty, rowIndex);
                    try
                    {
                        _noSelectionChangeCount++;
                        int columnIndex = CurrentColumnIndex;

                        if (columnIndex == -1)
                        {
                            columnIndex = FirstDisplayedNonFillerColumnIndex;
                        }
                        if (IsSlotOutOfSelectionBounds(slot))
                        {
                            ClearRowSelection(slotException: slot, setAnchorSlot: true);
                            return;
                        }

                        UpdateSelectionAndCurrency(columnIndex, slot, DataGridSelectionAction.SelectCurrent, scrollIntoView: false);
                    }
                    finally
                    {
                        NoSelectionChangeCount--;
                    }

                    if (!_successfullyUpdatedSelection)
                    {
                        SetValueNoCallback(SelectedIndexProperty, oldSelectedIndex);
                        SetValueNoCallback(SelectedItemProperty, e.OldValue);
                    }
                }
            }
        }

        private void OnSelectionModeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (!_areHandlersSuspended)
            {
                ClearRowSelection(resetAnchorSlot: true);
            }
        }

    }
}