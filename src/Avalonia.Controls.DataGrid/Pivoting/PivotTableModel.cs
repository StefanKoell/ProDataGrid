// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Controls;

namespace Avalonia.Controls.DataGridPivoting
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotChangedEventArgs : EventArgs
    {
        public PivotChangedEventArgs(
            IReadOnlyList<PivotRow> rows,
            IReadOnlyList<PivotColumn> columns,
            IReadOnlyList<DataGridColumnDefinition> columnDefinitions)
        {
            Rows = rows;
            Columns = columns;
            ColumnDefinitions = columnDefinitions;
        }

        public IReadOnlyList<PivotRow> Rows { get; }

        public IReadOnlyList<PivotColumn> Columns { get; }

        public IReadOnlyList<DataGridColumnDefinition> ColumnDefinitions { get; }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotTableModel : INotifyPropertyChanged, IDisposable
    {
        private IEnumerable? _itemsSource;
        private INotifyCollectionChanged? _itemsNotifier;
        private bool _autoRefresh = true;
        private int _updateNesting;
        private bool _pendingRefresh;
        private bool _isRefreshing;
        private IReadOnlyList<PivotColumn> _columns = Array.Empty<PivotColumn>();
        private readonly PivotAggregatorRegistry _aggregators = new();
        private readonly HashSet<PivotFieldBase> _subscribedFields = new();
        private CultureInfo _culture = CultureInfo.CurrentCulture;

        public PivotTableModel()
        {
            RowFields = new ObservableCollection<PivotAxisField>();
            ColumnFields = new ObservableCollection<PivotAxisField>();
            ValueFields = new ObservableCollection<PivotValueField>();
            FilterFields = new ObservableCollection<PivotAxisField>();
            Layout = new PivotLayoutOptions();

            Rows = new PivotObservableCollection<PivotRow>();
            ColumnDefinitions = new PivotObservableCollection<DataGridColumnDefinition>();

            RowFields.CollectionChanged += Fields_CollectionChanged;
            ColumnFields.CollectionChanged += Fields_CollectionChanged;
            ValueFields.CollectionChanged += ValueFields_CollectionChanged;
            FilterFields.CollectionChanged += Fields_CollectionChanged;
            Layout.PropertyChanged += Layout_PropertyChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<PivotChangedEventArgs>? PivotChanged;

        public ObservableCollection<PivotAxisField> RowFields { get; }

        public ObservableCollection<PivotAxisField> ColumnFields { get; }

        public ObservableCollection<PivotValueField> ValueFields { get; }

        public ObservableCollection<PivotAxisField> FilterFields { get; }

        public PivotLayoutOptions Layout { get; }

        public PivotObservableCollection<PivotRow> Rows { get; }

        public PivotObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        public IReadOnlyList<PivotColumn> Columns
        {
            get => _columns;
            private set
            {
                if (!ReferenceEquals(_columns, value))
                {
                    _columns = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Columns)));
                }
            }
        }

        public PivotAggregatorRegistry Aggregators => _aggregators;

        public CultureInfo Culture
        {
            get => _culture;
            set
            {
                if (!Equals(_culture, value))
                {
                    _culture = value ?? CultureInfo.CurrentCulture;
                    RequestRefresh();
                }
            }
        }

        public bool AutoRefresh
        {
            get => _autoRefresh;
            set
            {
                if (_autoRefresh == value)
                {
                    return;
                }

                _autoRefresh = value;
                if (_autoRefresh && _pendingRefresh)
                {
                    Refresh();
                }
            }
        }

        public IEnumerable? ItemsSource
        {
            get => _itemsSource;
            set
            {
                if (ReferenceEquals(_itemsSource, value))
                {
                    return;
                }

                DetachItemsNotifier();
                _itemsSource = value;
                AttachItemsNotifier();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemsSource)));
                RequestRefresh();
            }
        }

        public void Refresh()
        {
            if (_isRefreshing)
            {
                _pendingRefresh = true;
                return;
            }

            do
            {
                _pendingRefresh = false;
                _isRefreshing = true;
                try
                {
                    var builder = new PivotTableBuilder(this, _aggregators, _culture);
                    var result = builder.Build();

                    Columns = result.Columns;
                    Rows.ResetWith(result.Rows);
                    ColumnDefinitions.ResetWith(result.ColumnDefinitions);

                    PivotChanged?.Invoke(this, new PivotChangedEventArgs(result.Rows, result.Columns, result.ColumnDefinitions));
                }
                finally
                {
                    _isRefreshing = false;
                }
            }
            while (_pendingRefresh && _autoRefresh && _updateNesting == 0);
        }

        public IDisposable DeferRefresh()
        {
            BeginUpdate();
            return new UpdateScope(this);
        }

        public void BeginUpdate()
        {
            _updateNesting++;
        }

        public void EndUpdate()
        {
            if (_updateNesting == 0)
            {
                throw new InvalidOperationException("EndUpdate called without matching BeginUpdate.");
            }

            _updateNesting--;
            if (_updateNesting == 0 && _pendingRefresh)
            {
                Refresh();
            }
        }

        public void Dispose()
        {
            DetachItemsNotifier();
            DetachAllFieldHandlers();
            RowFields.CollectionChanged -= Fields_CollectionChanged;
            ColumnFields.CollectionChanged -= Fields_CollectionChanged;
            ValueFields.CollectionChanged -= ValueFields_CollectionChanged;
            FilterFields.CollectionChanged -= Fields_CollectionChanged;
            Layout.PropertyChanged -= Layout_PropertyChanged;
        }

        private void RequestRefresh()
        {
            if (_isRefreshing)
            {
                _pendingRefresh = true;
                return;
            }

            if (!AutoRefresh)
            {
                _pendingRefresh = true;
                return;
            }

            if (_updateNesting > 0)
            {
                _pendingRefresh = true;
                return;
            }

            Refresh();
        }

        private void AttachItemsNotifier()
        {
            if (_itemsSource is INotifyCollectionChanged notifier)
            {
                _itemsNotifier = notifier;
                _itemsNotifier.CollectionChanged += Items_CollectionChanged;
            }
        }

        private void DetachItemsNotifier()
        {
            if (_itemsNotifier != null)
            {
                _itemsNotifier.CollectionChanged -= Items_CollectionChanged;
                _itemsNotifier = null;
            }
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RequestRefresh();
        }

        private void Layout_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RequestRefresh();
        }

        private void Fields_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ReattachAllFieldHandlers();

                RequestRefresh();
                return;
            }

            if (e.NewItems != null)
            {
                AttachFieldHandlers(CastFields(e.NewItems));
            }

            if (e.OldItems != null)
            {
                DetachFieldHandlers(CastFields(e.OldItems));
            }

            RequestRefresh();
        }

        private void ValueFields_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ReattachAllFieldHandlers();

                RequestRefresh();
                return;
            }

            if (e.NewItems != null)
            {
                AttachFieldHandlers(CastFields(e.NewItems));
            }

            if (e.OldItems != null)
            {
                DetachFieldHandlers(CastFields(e.OldItems));
            }

            RequestRefresh();
        }

        private void Field_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RequestRefresh();
        }

        private void AttachFieldHandlers(IEnumerable<PivotFieldBase> fields)
        {
            foreach (var field in fields)
            {
                if (_subscribedFields.Add(field))
                {
                    field.PropertyChanged += Field_PropertyChanged;
                }
            }
        }

        private void DetachFieldHandlers(IEnumerable<PivotFieldBase> fields)
        {
            foreach (var field in fields)
            {
                if (_subscribedFields.Remove(field))
                {
                    field.PropertyChanged -= Field_PropertyChanged;
                }
            }
        }

        private static IEnumerable<PivotFieldBase> CastFields(System.Collections.IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item is PivotFieldBase field)
                {
                    yield return field;
                }
            }
        }

        private void DetachAllFieldHandlers()
        {
            foreach (var field in _subscribedFields)
            {
                field.PropertyChanged -= Field_PropertyChanged;
            }

            _subscribedFields.Clear();
        }

        private void ReattachAllFieldHandlers()
        {
            DetachAllFieldHandlers();
            AttachFieldHandlers(RowFields);
            AttachFieldHandlers(ColumnFields);
            AttachFieldHandlers(ValueFields);
            AttachFieldHandlers(FilterFields);
        }

        private sealed class UpdateScope : IDisposable
        {
            private PivotTableModel? _owner;

            public UpdateScope(PivotTableModel owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                var owner = _owner;
                if (owner == null)
                {
                    return;
                }

                _owner = null;
                owner.EndUpdate();
            }
        }
    }
}
