// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.ComponentModel;

namespace Avalonia.Controls.DataGridPivoting
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotValueFilter
    {
        private PivotValueFilterType _filterType;
        private PivotValueField? _valueField;
        private double? _value;
        private double? _value2;
        private int? _count;
        private double? _percent;

        public event EventHandler? Changed;

        public PivotValueFilterType FilterType
        {
            get => _filterType;
            set => SetProperty(ref _filterType, value);
        }

        public PivotValueField? ValueField
        {
            get => _valueField;
            set => SetProperty(ref _valueField, value);
        }

        public double? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public double? Value2
        {
            get => _value2;
            set => SetProperty(ref _value2, value);
        }

        public int? Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }

        public double? Percent
        {
            get => _percent;
            set => SetProperty(ref _percent, value);
        }

        private void SetProperty<T>(ref T field, T value)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotValueSort
    {
        private PivotValueField? _valueField;
        private ListSortDirection _sortDirection = ListSortDirection.Descending;

        public event EventHandler? Changed;

        public PivotValueField? ValueField
        {
            get => _valueField;
            set => SetProperty(ref _valueField, value);
        }

        public ListSortDirection SortDirection
        {
            get => _sortDirection;
            set => SetProperty(ref _sortDirection, value);
        }

        private void SetProperty<T>(ref T field, T value)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
