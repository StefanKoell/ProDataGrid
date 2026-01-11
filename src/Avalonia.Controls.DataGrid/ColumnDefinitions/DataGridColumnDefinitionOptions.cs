// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls.DataGridFiltering;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    class DataGridColumnDefinitionOptions : INotifyPropertyChanged
    {
        private bool? _isSearchable;
        private string _searchMemberPath;
        private Func<object, string> _searchTextProvider;
        private IFormatProvider _searchFormatProvider;
        private Func<FilteringDescriptor, Func<object, bool>> _filterPredicateFactory;
        private IDataGridColumnValueAccessor _filterValueAccessor;
        private IDataGridColumnValueAccessor _sortValueAccessor;
        private IComparer _sortValueComparer;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool? IsSearchable
        {
            get => _isSearchable;
            set => SetProperty(ref _isSearchable, value);
        }

        public string SearchMemberPath
        {
            get => _searchMemberPath;
            set => SetProperty(ref _searchMemberPath, value);
        }

        public Func<object, string> SearchTextProvider
        {
            get => _searchTextProvider;
            set => SetProperty(ref _searchTextProvider, value);
        }

        public IFormatProvider SearchFormatProvider
        {
            get => _searchFormatProvider;
            set => SetProperty(ref _searchFormatProvider, value);
        }

        public Func<FilteringDescriptor, Func<object, bool>> FilterPredicateFactory
        {
            get => _filterPredicateFactory;
            set => SetProperty(ref _filterPredicateFactory, value);
        }

        public IDataGridColumnValueAccessor FilterValueAccessor
        {
            get => _filterValueAccessor;
            set => SetProperty(ref _filterValueAccessor, value);
        }

        public IDataGridColumnValueAccessor SortValueAccessor
        {
            get => _sortValueAccessor;
            set => SetProperty(ref _sortValueAccessor, value);
        }

        public IComparer SortValueComparer
        {
            get => _sortValueComparer;
            set => SetProperty(ref _sortValueComparer, value);
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
