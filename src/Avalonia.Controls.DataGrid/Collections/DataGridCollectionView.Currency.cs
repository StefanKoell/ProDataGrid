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
        /// Move to the given item.
        /// </summary>
        /// <param name="item">Item we want to move the currency to</param>
        /// <returns>Whether the operation was successful</returns>
        public bool MoveCurrentTo(object item)
        {
            VerifyRefreshNotDeferred();

            // if already on item, don't do anything
            if (Object.Equals(CurrentItem, item))
            {
                // also check that we're not fooled by a false null currentItem
                if (item != null || IsCurrentInView)
                {
                    return IsCurrentInView;
                }
            }

            // if the item is not found IndexOf() will return -1, and
            // the MoveCurrentToPosition() below will move current to BeforeFirst
            // The IndexOf function takes into account paging, filtering, and sorting
            return MoveCurrentToPosition(IndexOf(item));
        }

        /// <summary>
        /// Move to the first item.
        /// </summary>
        /// <returns>Whether the operation was successful</returns>
        public bool MoveCurrentToFirst()
        {
            VerifyRefreshNotDeferred();

            return MoveCurrentToPosition(0);
        }

        /// <summary>
        /// Move to the last item.
        /// </summary>
        /// <returns>Whether the operation was successful</returns>
        public bool MoveCurrentToLast()
        {
            VerifyRefreshNotDeferred();

            int index = Count - 1;

            return MoveCurrentToPosition(index);
        }

        /// <summary>
        /// Move to the next item.
        /// </summary>
        /// <returns>Whether the operation was successful</returns>
        public bool MoveCurrentToNext()
        {
            VerifyRefreshNotDeferred();

            int index = CurrentPosition + 1;

            if (index <= Count)
            {
                return MoveCurrentToPosition(index);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Move CurrentItem to this index
        /// </summary>
        /// <param name="position">Position we want to move the currency to</param>
        /// <returns>True if the resulting CurrentItem is an item within the view; otherwise False</returns>
        public bool MoveCurrentToPosition(int position)
        {
            VerifyRefreshNotDeferred();

            // We want to allow the user to set the currency to just
            // beyond the last item. EnumerableCollectionView in WPF
            // also checks (position > Count) though the ListCollectionView
            // looks for (position >= Count).
            if (position < -1 || position > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            if ((position != CurrentPosition || !IsCurrentInSync)
            && OkToChangeCurrent())
            {
                bool oldIsCurrentAfterLast = IsCurrentAfterLast;
                bool oldIsCurrentBeforeFirst = IsCurrentBeforeFirst;

                SetCurrentToPosition(position);
                OnCurrentChanged();

                if (IsCurrentAfterLast != oldIsCurrentAfterLast)
                {
                    OnPropertyChanged(nameof(IsCurrentAfterLast));
                }

                if (IsCurrentBeforeFirst != oldIsCurrentBeforeFirst)
                {
                    OnPropertyChanged(nameof(IsCurrentBeforeFirst));
                }

                OnPropertyChanged(nameof(CurrentPosition));
                OnPropertyChanged(nameof(CurrentItem));
            }

            return IsCurrentInView;
        }

        /// <summary>
        /// Move to the previous item.
        /// </summary>
        /// <returns>Whether the operation was successful</returns>
        public bool MoveCurrentToPrevious()
        {
            VerifyRefreshNotDeferred();

            int index = CurrentPosition - 1;

            if (index >= -1)
            {
                return MoveCurrentToPosition(index);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Ask listeners (via ICollectionView.CurrentChanging event) if it's OK to change currency
        /// </summary>
        /// <returns>False if a listener cancels the change, True otherwise</returns>
        private bool OkToChangeCurrent()
        {
            DataGridCurrentChangingEventArgs args = new DataGridCurrentChangingEventArgs();
            OnCurrentChanging(args);
            return !args.Cancel;
        }

        /// <summary>
        /// Raises the CurrentChanged event
        /// </summary>
        private void OnCurrentChanged()
        {
            if (CurrentChanged != null && _currentChangedMonitor.Enter())
            {
                using (_currentChangedMonitor)
                {
                    CurrentChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Raise a CurrentChanging event that is not cancelable.
        /// This is called by CollectionChanges (Add, Remove, and Refresh) that
        /// affect the CurrentItem.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// This CurrentChanging event cannot be canceled.
        /// </exception>
        private void OnCurrentChanging()
        {
            OnCurrentChanging(uncancelableCurrentChangingEventArgs);
        }

        /// <summary>
        /// Set CurrentItem and CurrentPosition, no questions asked!
        /// </summary>
        /// <remarks>
        /// CollectionViews (and sub-classes) should use this method to update
        /// the Current values.
        /// </remarks>
        /// <param name="newItem">New CurrentItem</param>
        /// <param name="newPosition">New CurrentPosition</param>
        private void SetCurrent(object newItem, int newPosition)
        {
            int count = (newItem != null) ? 0 : (IsEmpty ? 0 : Count);
            SetCurrent(newItem, newPosition, count);
        }

        /// <summary>
        /// Just move it. No argument check, no events, just move current to position.
        /// </summary>
        /// <param name="position">Position to move the current item to</param>
        private void SetCurrentToPosition(int position)
        {
            if (position < 0)
            {
                SetFlag(CollectionViewFlags.IsCurrentBeforeFirst, true);
                SetCurrent(null, -1);
            }
            else if (position >= Count)
            {
                SetFlag(CollectionViewFlags.IsCurrentAfterLast, true);
                SetCurrent(null, Count);
            }
            else
            {
                SetFlag(CollectionViewFlags.IsCurrentBeforeFirst | CollectionViewFlags.IsCurrentAfterLast, false);
                SetCurrent(GetItemAt(position), position);
            }
        }

        /// <summary>
        /// Raises Currency Change events
        /// </summary>
        /// <param name="fireChangedEvent">Whether to fire the CurrentChanged event even if the parameters have not changed</param>
        /// <param name="oldCurrentItem">CurrentItem before processing changes</param>
        /// <param name="oldCurrentPosition">CurrentPosition before processing changes</param>
        /// <param name="oldIsCurrentBeforeFirst">IsCurrentBeforeFirst before processing changes</param>
        /// <param name="oldIsCurrentAfterLast">IsCurrentAfterLast before processing changes</param>
        private void RaiseCurrencyChanges(bool fireChangedEvent, object oldCurrentItem, int oldCurrentPosition, bool oldIsCurrentBeforeFirst, bool oldIsCurrentAfterLast)
        {
            // fire events for currency changes
            if (fireChangedEvent || CurrentItem != oldCurrentItem || CurrentPosition != oldCurrentPosition)
            {
                OnCurrentChanged();
            }
            if (CurrentItem != oldCurrentItem)
            {
                OnPropertyChanged(nameof(CurrentItem));
            }
            if (CurrentPosition != oldCurrentPosition)
            {
                OnPropertyChanged(nameof(CurrentPosition));
            }
            if (IsCurrentAfterLast != oldIsCurrentAfterLast)
            {
                OnPropertyChanged(nameof(IsCurrentAfterLast));
            }
            if (IsCurrentBeforeFirst != oldIsCurrentBeforeFirst)
            {
                OnPropertyChanged(nameof(IsCurrentBeforeFirst));
            }
        }

    }
}
