// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System;
using System.Collections;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    class DataGridColumnDefinitionOptions<TItem> : DataGridColumnDefinitionOptions, IDataGridColumnDefinitionSortComparerProvider
    {
        private Comparison<TItem> _compareAscending;
        private Comparison<TItem> _compareDescending;
        private IComparer _ascendingComparer;
        private IComparer _descendingComparer;

        public Comparison<TItem> CompareAscending
        {
            get => _compareAscending;
            set => SetComparison(ref _compareAscending, value, ref _ascendingComparer, nameof(CompareAscending));
        }

        public Comparison<TItem> CompareDescending
        {
            get => _compareDescending;
            set => SetComparison(ref _compareDescending, value, ref _descendingComparer, nameof(CompareDescending));
        }

        IComparer IDataGridColumnDefinitionSortComparerProvider.AscendingComparer => _ascendingComparer;

        IComparer IDataGridColumnDefinitionSortComparerProvider.DescendingComparer => _descendingComparer;

        private void SetComparison(
            ref Comparison<TItem> field,
            Comparison<TItem> value,
            ref IComparer comparerField,
            string propertyName)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            comparerField = value != null ? new ComparisonComparer<TItem>(value) : null;
            RaisePropertyChanged(propertyName);
        }

        private sealed class ComparisonComparer<T> : IComparer
        {
            private readonly Comparison<T> _comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                _comparison = comparison ?? throw new ArgumentNullException(nameof(comparison));
            }

            public int Compare(object x, object y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x is null)
                {
                    return -1;
                }

                if (y is null)
                {
                    return 1;
                }

                if (x is T left && y is T right)
                {
                    return _comparison(left, right);
                }

                return Comparer.Default.Compare(x, y);
            }
        }
    }

    internal interface IDataGridColumnDefinitionSortComparerProvider
    {
        IComparer AscendingComparer { get; }

        IComparer DescendingComparer { get; }
    }
}
