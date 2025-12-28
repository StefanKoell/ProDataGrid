// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

#nullable disable

using Avalonia.Controls;
using Avalonia.Controls.Utils;
using Avalonia.Utilities;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System;

namespace Avalonia.Collections
{
    sealed partial class DataGridCollectionView
    {
        /// <summary>
        /// Sets up the ActiveComparer for the CollectionViewGroupRoot specified
        /// </summary>
        /// <param name="groupRoot">The CollectionViewGroupRoot</param>
        private void PrepareGroupingComparer(CollectionViewGroupRoot groupRoot)
        {
            if (groupRoot == _temporaryGroup || PageSize == 0)
            {
                if (groupRoot.ActiveComparer is DataGridCollectionViewGroupInternal.ListComparer listComparer)
                {
                    listComparer.ResetList(InternalList);
                }
                else
                {
                    groupRoot.ActiveComparer = new DataGridCollectionViewGroupInternal.ListComparer(InternalList);
                }
            }
            else if (groupRoot == _group)
            {
                // create the new comparer based on the current _temporaryGroup
                groupRoot.ActiveComparer = new DataGridCollectionViewGroupInternal.CollectionViewGroupComparer(_temporaryGroup);
            }
        }

        /// <summary>
        /// Use the GroupDescriptions to place items into their respective groups.
        /// This assumes that there is no paging, so we just group the entire collection
        /// of items that the CollectionView holds.
        /// </summary>
        private void PrepareGroups()
        {
            using var activity = DataGridDiagnostics.CollectionGroup();
            using var _ = DataGridDiagnostics.BeginCollectionGroup();
            activity?.SetTag(DataGridDiagnostics.Tags.GroupDescriptions, _group.GroupDescriptions.Count);
            activity?.SetTag(DataGridDiagnostics.Tags.Rows, _internalList.Count);

            // we should only use this method if we aren't paging
            Debug.Assert(PageSize == 0, "Unexpected PageSize != 0");

            _group.Clear();
            _group.Initialize();

            _group.IsDataInGroupOrder = CheckFlag(CollectionViewFlags.IsDataInGroupOrder);

            // set to false so that we access internal collection items
            // instead of the group items, as they have been cleared
            _isGrouping = false;

            if (_group.GroupDescriptions.Count > 0)
            {
                for (int num = 0, count = _internalList.Count; num < count; ++num)
                {
                    object item = _internalList[num];
                    if (item != null && (!IsAddingNew || !object.Equals(CurrentAddItem, item)))
                    {
                        _group.AddToSubgroups(item, loading: true);
                    }
                }
                if (IsAddingNew)
                {
                    _group.InsertSpecialItem(_group.Items.Count, CurrentAddItem, true);
                }
            }

            _isGrouping = _group.GroupBy != null;

            // now we set the value to false, so that subsequent adds will insert
            // into the correct groups.
            _group.IsDataInGroupOrder = false;

            // reset the grouping comparer
            PrepareGroupingComparer(_group);
        }

        /// <summary>
        /// Use the GroupDescriptions to place items into their respective groups.
        /// Because of the fact that we have paging, it is possible that we are only
        /// going to need a subset of the items to be displayed. However, before we
        /// actually group the entire collection, we can't display the items in the
        /// correct order. We therefore want to just create a temporary group with
        /// the entire collection, and then using this data we can create the group
        /// that is exposed with just the items we need.
        /// </summary>
        private void PrepareTemporaryGroups()
        {
            using var activity = DataGridDiagnostics.CollectionGroupTemporary();
            using var _ = DataGridDiagnostics.BeginCollectionGroupTemporary();
            activity?.SetTag(DataGridDiagnostics.Tags.GroupDescriptions, _group.GroupDescriptions.Count);
            activity?.SetTag(DataGridDiagnostics.Tags.Rows, _internalList.Count);

            _temporaryGroup = new CollectionViewGroupRoot(this, CheckFlag(CollectionViewFlags.IsDataInGroupOrder));

            foreach (var gd in _group.GroupDescriptions)
            {
                _temporaryGroup.GroupDescriptions.Add(gd);
            }

            _temporaryGroup.Initialize();

            // set to false so that we access internal collection items
            // instead of the group items, as they have been cleared
            _isGrouping = false;

            if (_temporaryGroup.GroupDescriptions.Count > 0)
            {
                for (int num = 0, count = _internalList.Count; num < count; ++num)
                {
                    object item = _internalList[num];
                    if (item != null && (!IsAddingNew || !object.Equals(CurrentAddItem, item)))
                    {
                        _temporaryGroup.AddToSubgroups(item, loading: true);
                    }
                }
                if (IsAddingNew)
                {
                    _temporaryGroup.InsertSpecialItem(_temporaryGroup.Items.Count, CurrentAddItem, true);
                }
            }

            _isGrouping = _temporaryGroup.GroupBy != null;

            // reset the grouping comparer
            PrepareGroupingComparer(_temporaryGroup);
        }

        /// <summary>
        /// Update our Groups private accessor to point to the subset of data
        /// covered by the current page, or to display the entire group if paging is not
        /// being used.
        /// </summary>
        //TODO Paging
        private void PrepareGroupsForCurrentPage()
        {
            using var activity = DataGridDiagnostics.CollectionGroupPage();
            using var _ = DataGridDiagnostics.BeginCollectionGroupPage();
            activity?.SetTag(DataGridDiagnostics.Tags.GroupDescriptions, GroupDescriptions.Count);
            activity?.SetTag(DataGridDiagnostics.Tags.PageSize, PageSize);
            activity?.SetTag(DataGridDiagnostics.Tags.PageIndex, PageIndex);

            _group.Clear();
            _group.Initialize();

            // set to indicate that we will be pulling data from the temporary group data
            _isUsingTemporaryGroup = true;

            // since we are getting our data from the temporary group, it should
            // already be in group order
            _group.IsDataInGroupOrder = true;
            _group.ActiveComparer = null;

            if (GroupDescriptions.Count > 0)
            {
                for (int num = 0, count = Count; num < count; ++num)
                {
                    object item = GetItemAt(num);
                    if (item != null && (!IsAddingNew || !object.Equals(CurrentAddItem, item)))
                    {
                        _group.AddToSubgroups(item, loading: true);
                    }
                }
                if (IsAddingNew)
                {
                    _group.InsertSpecialItem(_group.Items.Count, CurrentAddItem, true);
                }
            }

            // set flag to indicate that we do not need to access the temporary data any longer
            _isUsingTemporaryGroup = false;

            // now we set the value to false, so that subsequent adds will insert
            // into the correct groups.
            _group.IsDataInGroupOrder = false;

            // reset the grouping comparer
            PrepareGroupingComparer(_group);

            _isGrouping = _group.GroupBy != null;
        }

        /// <summary>
        /// GroupBy changed handler
        /// </summary>
        /// <param name="sender">CollectionViewGroup whose GroupBy has changed</param>
        /// <param name="e">Arguments for the NotifyCollectionChanged event</param>
        private void OnGroupByChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsAddingNew || IsEditingItem)
            {
                throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText("Grouping"));
            }

            RefreshOrDefer();
        }

        /// <summary>
        /// GroupDescription changed handler
        /// </summary>
        /// <param name="sender">CollectionViewGroup whose GroupDescription has changed</param>
        /// <param name="e">Arguments for the GroupDescriptionChanged event</param>
        //TODO Paging
        private void OnGroupDescriptionChanged(object sender, EventArgs e)
        {
            if (IsAddingNew || IsEditingItem)
            {
                throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText("Grouping"));
            }

            // we want to make sure that the data is refreshed before we try to move to a page
            // since the refresh would take care of the filtering, sorting, and grouping.
            RefreshOrDefer();

            if (PageSize > 0)
            {
                if (IsRefreshDeferred)
                {
                    // set cached value and flag so that we move to first page on EndDefer
                    _cachedPageIndex = 0;
                    SetFlag(CollectionViewFlags.IsMoveToPageDeferred, true);
                }
                else
                {
                    MoveToFirstPage();
                }
            }
        }

    }
}
