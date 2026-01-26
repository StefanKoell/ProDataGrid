// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Avalonia.Controls.DataGridPivoting
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotObservableCollection<T> : ObservableCollection<T>
    {
        public void ResetWith(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            CheckReentrancy();

            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
