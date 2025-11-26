// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

#nullable disable

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
        /// Return true if the item belongs to this view.  No assumptions are
        /// made about the item. This method will behave similarly to IList.Contains().
        /// If the caller knows that the item belongs to the
        /// underlying collection, it is more efficient to call PassesFilter.
        /// </summary>
        /// <param name="item">The item we are checking to see whether it is within the collection</param>
        /// <returns>Boolean value of whether or not the collection contains the item</returns>
        public bool Contains(object item)
        {
            EnsureCollectionInSync();
            VerifyRefreshNotDeferred();
            return IndexOf(item) >= 0;
        }

        /// <summary>
        /// Enter a Defer Cycle.
        /// Defer cycles are used to coalesce changes to the ICollectionView.
        /// </summary>
        /// <returns>IDisposable used to notify that we no longer need to defer, when we dispose</returns>
        public IDisposable DeferRefresh()
        {
            if (IsAddingNew || IsEditingItem)
            {
                throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText(nameof(DeferRefresh)));
            }

            ++_deferLevel;
            return new DeferHelper(this);
        }

        /// <summary>
        /// Implementation of IEnumerable.GetEnumerator().
        /// This provides a way to enumerate the members of the collection
        /// without changing the currency.
        /// </summary>
        /// <returns>IEnumerator for the collection</returns>
        //TODO Paging
        public IEnumerator GetEnumerator()
        {
            EnsureCollectionInSync();
            VerifyRefreshNotDeferred();

            if (IsGrouping)
            {
                return RootGroup?.GetLeafEnumerator();
            }

            // if we are paging
            if (PageSize > 0)
            {
                List<object> list = new List<object>();

                // if we are in the middle of asynchronous load
                if (PageIndex < 0)
                {
                    return list.GetEnumerator();
                }

                for (int index = _pageSize * PageIndex;
                index < (int)Math.Min(_pageSize * (PageIndex + 1), InternalList.Count);
                index++)
                {
                    list.Add(InternalList[index]);
                }

                return new NewItemAwareEnumerator(this, list.GetEnumerator(), CurrentAddItem);
            }
            else
            {
                return new NewItemAwareEnumerator(this, InternalList.GetEnumerator(), CurrentAddItem);
            }
        }

        /// <summary>
        /// Retrieve item at the given zero-based index in this DataGridCollectionView, after the source collection
        /// is filtered, sorted, and paged.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if index is out of range
        /// </exception>
        /// <param name="index">Index of the item we want to retrieve</param>
        /// <returns>Item at specified index</returns>
        public object GetItemAt(int index)
        {
            EnsureCollectionInSync();
            VerifyRefreshNotDeferred();

            // for indices larger than the count
            if (index >= Count || index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (IsGrouping)
            {
                return RootGroup?.LeafAt(_isUsingTemporaryGroup ? ConvertToInternalIndex(index) : index);
            }

            if (IsAddingNew && UsesLocalArray && index == Count - 1)
            {
                return CurrentAddItem;
            }

            return InternalItemAt(ConvertToInternalIndex(index));
        }

        /// <summary>
        /// Return the index where the given item appears, or -1 if doesn't appear.
        /// </summary>
        /// <param name="item">Item we are searching for</param>
        /// <returns>Index of specified item</returns>
        //TODO Paging
        public int IndexOf(object item)
        {
            EnsureCollectionInSync();
            VerifyRefreshNotDeferred();

            if (IsGrouping)
            {
                return RootGroup?.LeafIndexOf(item) ?? -1;
            }
            if (IsAddingNew && Object.Equals(item, CurrentAddItem) && UsesLocalArray)
            {
                return Count - 1;
            }

            int internalIndex = InternalIndexOf(item);

            if (PageSize > 0 && internalIndex != -1)
            {
                if ((internalIndex >= (PageIndex * _pageSize)) &&
                (internalIndex < ((PageIndex + 1) * _pageSize)))
                {
                    return internalIndex - (PageIndex * _pageSize);
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return internalIndex;
            }
        }

        /// <summary>
        /// Return true if the item belongs to this view.  The item is assumed to belong to the
        /// underlying DataCollection;  this method merely takes filters into account.
        /// It is commonly used during collection-changed notifications to determine if the added/removed
        /// item requires processing.
        /// Returns true if no filter is set on collection view.
        /// </summary>
        /// <param name="item">The item to compare against the Filter</param>
        /// <returns>Whether the item passes the filter</returns>
        public bool PassesFilter(object item)
        {
            if (Filter != null)
            {
                return Filter(item);
            }

            return true;
        }

        /// <summary>
        /// Re-create the view, using any SortDescriptions and/or Filters.
        /// </summary>
        public void Refresh()
        {
            if (this is IDataGridEditableCollectionView ecv && (ecv.IsAddingNew || ecv.IsEditingItem))
            {
                throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText(nameof(Refresh)));
            }

            RefreshInternal();
        }

        /// <summary>
        /// Remove the given item from the underlying collection. It
        /// needs to be in the current filtered, sorted, and paged view
        /// to call
        /// </summary>
        /// <param name="item">Item we want to remove</param>
        public void Remove(object item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
            }
        }

        /// <summary>
        /// Remove the item at the given index from the underlying collection.
        /// The index is interpreted with respect to the view (filtered, sorted,
        /// and paged list).
        /// </summary>
        /// <param name="index">Index of the item we want to remove</param>
        //TODO Paging
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range. Must be non-negative and less than the size of the collection.");
            }

            if (IsEditingItem || IsAddingNew)
            {
                throw new InvalidOperationException(GetOperationNotAllowedDuringAddOrEditText(nameof(RemoveAt)));
            }
            else if (!CanRemove)
            {
                throw new InvalidOperationException("Remove/RemoveAt is not supported.");
            }

            VerifyRefreshNotDeferred();

            // convert the index from "view-relative" to "list-relative"
            object item = GetItemAt(index);

            // before we remove the item, see if we are not on the last page
            // and will have to bring in a new item to replace it
            bool replaceItem = PageSize > 0 && !OnLastLocalPage;

            try
            {
                // temporarily disable the CollectionChanged event
                // handler so filtering, sorting, or grouping
                // doesn't get applied yet
                SetFlag(CollectionViewFlags.ShouldProcessCollectionChanged, false);

                if (SourceList != null)
                {
                    SourceList.Remove(item);
                }
            }
            finally
            {
                SetFlag(CollectionViewFlags.ShouldProcessCollectionChanged, true);
            }

            // Modify our _trackingEnumerator so that it shows that our collection is "up to date"
            // and will not refresh for now.
            _trackingEnumerator = _sourceCollection.GetEnumerator();

            Debug.Assert(index == IndexOf(item), "IndexOf returned unexpected value");

            // remove the item from the internal list
            _internalList.Remove(item);

            if (IsGrouping)
            {
                if (PageSize > 0)
                {
                    _temporaryGroup.RemoveFromSubgroups(item);
                }
                _group.RemoveFromSubgroups(item);
            }

            object oldCurrentItem = CurrentItem;
            int oldCurrentPosition = CurrentPosition;
            bool oldIsCurrentAfterLast = IsCurrentAfterLast;
            bool oldIsCurrentBeforeFirst = IsCurrentBeforeFirst;

            AdjustCurrencyForRemove(index);

            // fire remove notification
            OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Remove,
            item,
            index));

            RaiseCurrencyChanges(false, oldCurrentItem, oldCurrentPosition, oldIsCurrentBeforeFirst, oldIsCurrentAfterLast);

            // if we removed all items from the current page,
            // move to the previous page. we do not need to
            // fire additional notifications, as moving the page will
            // trigger a reset.
            if (NeedToMoveToPreviousPage)
            {
                MoveToPreviousPage();
                return;
            }

            // if we are paging, we may have to fire another notification for the item
            // that needs to replace the one we removed on this page.
            if (replaceItem)
            {
                // we first need to add the item into the current group
                if (IsGrouping)
                {
                    object newItem = _temporaryGroup.LeafAt((PageSize * (PageIndex + 1)) - 1);
                    if (newItem != null)
                    {
                        _group.AddToSubgroups(newItem, loading: false);
                    }
                }

                // fire the add notification
                OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                GetItemAt(PageSize - 1),
                PageSize - 1));
            }
        }

        /// <summary>
        /// Process an Add operation from an INotifyCollectionChanged event handler.
        /// </summary>
        /// <param name="addedItem">Item added to the source collection</param>
        /// <param name="addIndex">Index item was added into</param>
        //TODO Paging
        private void ProcessAddEvent(object addedItem, int addIndex)
        {
            // item to fire remove notification for if necessary
            object removeNotificationItem = null;
            if (PageSize > 0 && !IsGrouping)
            {
                removeNotificationItem = (Count == PageSize) ?
                GetItemAt(PageSize - 1) : null;
            }

            // process the add by filtering and sorting the item
            ProcessInsertToCollection(
            addedItem,
            addIndex);

            // next check if we need to add an item into the current group
            bool needsGrouping = false;
            if (Count == 1 && GroupDescriptions.Count > 0)
            {
                // if this is the first item being added
                // we want to setup the groups with the
                // correct element type comparer
                if (PageSize > 0)
                {
                    PrepareGroupingComparer(_temporaryGroup);
                }
                PrepareGroupingComparer(_group);
            }

            if (IsGrouping)
            {
                int leafIndex = -1;

                if (PageSize > 0)
                {
                    _temporaryGroup.AddToSubgroups(addedItem, false /*loading*/);
                    leafIndex = _temporaryGroup.LeafIndexOf(addedItem);
                }

                // if we are not paging, we should just be able to add the item.
                // otherwise, we need to validate that it is within the current page.
                if (PageSize == 0 || (PageIndex + 1) * PageSize > leafIndex)
                {
                    needsGrouping = true;

                    int pageStartIndex = PageIndex * PageSize;

                    // if the item was inserted on a previous page
                    if (pageStartIndex > leafIndex && PageSize > 0)
                    {
                        addedItem = _temporaryGroup.LeafAt(pageStartIndex);
                    }

                    // if we're grouping and have more items than the
                    // PageSize will allow, remove the last item
                    if (PageSize > 0 && _group.ItemCount == PageSize)
                    {
                        removeNotificationItem = _group.LeafAt(PageSize - 1);
                        _group.RemoveFromSubgroups(removeNotificationItem);
                    }
                }
            }

            // if we are paging, we may have to fire another notification for the item
            // that needs to be removed for the one we added on this page.
            if (PageSize > 0 && !OnLastLocalPage &&
            (((IsGrouping && removeNotificationItem != null) ||
            (!IsGrouping && (PageIndex + 1) * PageSize > InternalIndexOf(addedItem)))))
            {
                if (removeNotificationItem != null && removeNotificationItem != addedItem)
                {
                    AdjustCurrencyForRemove(PageSize - 1);

                    OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    removeNotificationItem,
                    PageSize - 1));
                }
            }

            // if we need to add the item into the current group
            // that will be displayed
            if (needsGrouping)
            {
                this._group.AddToSubgroups(addedItem, false /*loading*/);
            }

            int addedIndex = IndexOf(addedItem);

            // if the item is within the current page
            if (addedIndex >= 0)
            {
                object oldCurrentItem = CurrentItem;
                int oldCurrentPosition = CurrentPosition;
                bool oldIsCurrentAfterLast = IsCurrentAfterLast;
                bool oldIsCurrentBeforeFirst = IsCurrentBeforeFirst;

                AdjustCurrencyForAdd(null, addedIndex);

                // fire add notification
                OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Add,
                addedItem,
                addedIndex));

                RaiseCurrencyChanges(false, oldCurrentItem, oldCurrentPosition, oldIsCurrentBeforeFirst, oldIsCurrentAfterLast);
            }
            else if (PageSize > 0)
            {
                // otherwise if the item was added into a previous page
                int internalIndex = InternalIndexOf(addedItem);

                if (internalIndex < ConvertToInternalIndex(0))
                {
                    // fire add notification for item pushed in
                    OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    GetItemAt(0),
                    0));
                }
            }
        }

        /// <summary>
        /// Process CollectionChanged event on source collection
        /// that implements INotifyCollectionChanged.
        /// </summary>
        /// <param name="args">
        /// The NotifyCollectionChangedEventArgs to be processed.
        /// </param>
        private void ProcessCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            // if we do not want to handle the CollectionChanged event, return
            if (!CheckFlag(CollectionViewFlags.ShouldProcessCollectionChanged))
            {
                return;
            }

            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                // if we have no items now, clear our own internal list
                if (!SourceCollection.GetEnumerator().MoveNext())
                {
                    _internalList.Clear();
                }

                // calling Refresh, will fire the collectionchanged event
                RefreshOrDefer();
                return;
            }

            // fire notifications for removes
            if (args.OldItems != null &&
            (args.Action == NotifyCollectionChangedAction.Remove ||
            args.Action == NotifyCollectionChangedAction.Replace))
            {
                foreach (var removedItem in args.OldItems)
                {
                    ProcessRemoveEvent(removedItem, args.Action == NotifyCollectionChangedAction.Replace);
                }
            }

            // fire notifications for adds
            if (args.NewItems != null &&
            (args.Action == NotifyCollectionChangedAction.Add ||
            args.Action == NotifyCollectionChangedAction.Replace))
            {
                for (var i = 0; i < args.NewItems.Count; i++)
                {
                    if (Filter == null || PassesFilter(args.NewItems[i]))
                    {
                        ProcessAddEvent(args.NewItems[i], args.NewStartingIndex + i);
                    }
                }
            }
            if (args.Action != NotifyCollectionChangedAction.Replace)
            {
                OnPropertyChanged(nameof(ItemCount));
            }
        }

        /// <summary>
        /// Process a Remove operation from an INotifyCollectionChanged event handler.
        /// </summary>
        /// <param name="removedItem">Item removed from the source collection</param>
        /// <param name="isReplace">Whether this was part of a Replace operation</param>
        //TODO Paging
        private void ProcessRemoveEvent(object removedItem, bool isReplace)
        {
            int internalRemoveIndex = -1;

            if (IsGrouping)
            {
                internalRemoveIndex = PageSize > 0 ? _temporaryGroup.LeafIndexOf(removedItem) :
                _group.LeafIndexOf(removedItem);
            }
            else
            {
                internalRemoveIndex = InternalIndexOf(removedItem);
            }

            int removeIndex = IndexOf(removedItem);

            // remove the item from the collection
            _internalList.Remove(removedItem);

            // only fire the remove if it was removed from either the current page, or a previous page
            bool needToRemove = (PageSize == 0 && removeIndex >= 0) || (internalRemoveIndex < (PageIndex + 1) * PageSize);

            if (IsGrouping)
            {
                if (PageSize > 0)
                {
                    _temporaryGroup.RemoveFromSubgroups(removedItem);
                }

                if (needToRemove)
                {
                    _group.RemoveFromSubgroups(removeIndex >= 0 ? removedItem : _group.LeafAt(0));
                }
            }

            if (needToRemove)
            {
                object oldCurrentItem = CurrentItem;
                int oldCurrentPosition = CurrentPosition;
                bool oldIsCurrentAfterLast = IsCurrentAfterLast;
                bool oldIsCurrentBeforeFirst = IsCurrentBeforeFirst;

                AdjustCurrencyForRemove(removeIndex);

                // fire remove notification
                // if we removed from current page, remove from removeIndex,
                // if we removed from previous page, remove first item (index=0)
                OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(
                NotifyCollectionChangedAction.Remove,
                removedItem,
                Math.Max(0, removeIndex)));

                RaiseCurrencyChanges(false, oldCurrentItem, oldCurrentPosition, oldIsCurrentBeforeFirst, oldIsCurrentAfterLast);

                // if we removed all items from the current page,
                // move to the previous page. we do not need to
                // fire additional notifications, as moving the page will
                // trigger a reset.
                if (NeedToMoveToPreviousPage && !isReplace)
                {
                    MoveToPreviousPage();
                    return;
                }

                // if we are paging, we may have to fire another notification for the item
                // that needs to replace the one we removed on this page.
                if (PageSize > 0 && Count == PageSize)
                {
                    // we first need to add the item into the current group
                    if (IsGrouping)
                    {
                        object newItem = _temporaryGroup.LeafAt((PageSize * (PageIndex + 1)) - 1);
                        if (newItem != null)
                        {
                            _group.AddToSubgroups(newItem, false /*loading*/);
                        }
                    }

                    // fire the add notification
                    OnCollectionChanged(
                    new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    GetItemAt(PageSize - 1),
                    PageSize - 1));
                }
            }
        }

        /// <summary>
        /// Handles adding an item into the collection, and applying sorting, filtering, grouping, paging.
        /// </summary>
        /// <param name="item">Item to insert in the collection</param>
        /// <param name="index">Index to insert item into</param>
        private void ProcessInsertToCollection(object item, int index)
        {
            // first check to see if it passes the filter
            if (Filter == null || PassesFilter(item))
            {
                if (SortDescriptions.Count > 0)
                {
                    var itemType = ItemType;
                    foreach (var sort in SortDescriptions)
                    sort.Initialize(itemType);

                    // create the SortFieldComparer to use
                    var sortFieldComparer = new MergedComparer(this);

                    // check if the item would be in sorted order if inserted into the specified index
                    // otherwise, calculate the correct sorted index
                    if (index < 0 || /* if item was not originally part of list */
                    (index > 0 && (sortFieldComparer.Compare(item, InternalItemAt(index - 1)) < 0)) || /* item has moved up in the list */
                    ((index < InternalList.Count - 1) && (sortFieldComparer.Compare(item, InternalItemAt(index)) > 0))) /* item has moved down in the list */
                    {
                        index = sortFieldComparer.FindInsertIndex(item, _internalList);
                    }
                }

                // make sure that the specified insert index is within the valid range
                // otherwise, just add it to the end. the index can be set to an invalid
                // value if the item was originally not in the collection, on a different
                // page, or if it had been previously filtered out.
                if (index < 0 || index > _internalList.Count)
                {
                    index = _internalList.Count;
                }

                _internalList.Insert(index, item);
            }
        }

        /// <summary>
        /// Will call RefreshOverride and clear the NeedsRefresh flag
        /// </summary>
        private void RefreshInternal()
        {
            RefreshOverride();
            SetFlag(CollectionViewFlags.NeedsRefresh, false);
        }

        /// <summary>
        /// Refresh, or mark that refresh is needed when defer cycle completes.
        /// </summary>
        private void RefreshOrDefer()
        {
            if (IsRefreshDeferred)
            {
                SetFlag(CollectionViewFlags.NeedsRefresh, true);
            }
            else
            {
                RefreshInternal();
            }
        }

        /// <summary>
        /// Re-create the view, using any SortDescriptions.
        /// Also updates currency information.
        /// </summary>
        //TODO Paging
        private void RefreshOverride()
        {
            object oldCurrentItem = CurrentItem;
            int oldCurrentPosition = CurrentPosition;
            bool oldIsCurrentAfterLast = IsCurrentAfterLast;
            bool oldIsCurrentBeforeFirst = IsCurrentBeforeFirst;

            // set IsGrouping to false
            _isGrouping = false;

            // force currency off the collection (gives user a chance to save dirty information)
            OnCurrentChanging();

            // if there's no sort/filter/paging/grouping, just use the collection's array
            if (UsesLocalArray)
            {
                try
                {
                    // apply filtering/sorting through the PrepareLocalArray method
                    _internalList = PrepareLocalArray(_sourceCollection);

                    // apply grouping
                    if (PageSize == 0)
                    {
                        PrepareGroups();
                    }
                    else
                    {
                        PrepareTemporaryGroups();
                        PrepareGroupsForCurrentPage();
                    }
                }
                catch (TargetInvocationException e)
                {
                    // If there's an exception while invoking PrepareLocalArray,
                    // we want to unwrap it and throw its inner exception
                    if (e.InnerException != null)
                    {
                        throw e.InnerException;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                CopySourceToInternalList();
            }

            // check if PageIndex is still valid after filter/sort
            if (PageSize > 0 &&
            PageIndex > 0 &&
            PageIndex >= PageCount)
            {
                MoveToPage(PageCount - 1);
            }

            // reset currency values
            ResetCurrencyValues(oldCurrentItem, oldIsCurrentBeforeFirst, oldIsCurrentAfterLast);

            OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Reset));

            // now raise currency changes at the end
            RaiseCurrencyChanges(false, oldCurrentItem, oldCurrentPosition, oldIsCurrentBeforeFirst, oldIsCurrentAfterLast);
        }

        /// <summary>
        ///     Notify listeners that this View has changed
        /// </summary>
        /// <remarks>
        ///     CollectionViews (and sub-classes) should take their filter/sort/grouping/paging
        ///     into account before calling this method to forward CollectionChanged events.
        /// </remarks>
        /// <param name="args">
        ///     The NotifyCollectionChangedEventArgs to be passed to the EventHandler
        /// </param>
        //TODO Paging
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            unchecked
            {
                // invalidate enumerators because of a change
                ++_timestamp;
            }

            if (CollectionChanged != null)
            {
                if (args.Action != NotifyCollectionChangedAction.Add || PageSize == 0 || args.NewStartingIndex < Count)
                {
                    CollectionChanged(this, args);
                }
            }

            // Collection changes change the count unless an item is being
            // replaced within the collection.
            if (args.Action != NotifyCollectionChangedAction.Replace)
            {
                OnPropertyChanged(nameof(Count));
            }

            bool listIsEmpty = IsEmpty;
            if (listIsEmpty != CheckFlag(CollectionViewFlags.CachedIsEmpty))
            {
                SetFlag(CollectionViewFlags.CachedIsEmpty, listIsEmpty);
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

    }
}
