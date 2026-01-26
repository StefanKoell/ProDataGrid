// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Avalonia.Controls.DataGridPivoting
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotSlicerFilterMode
    {
        Include,
        Exclude
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotSlicerSelectionMode
    {
        Multiple,
        Single
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotSlicerItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        internal PivotSlicerItem(object? value, string? display, int count)
        {
            Value = value;
            Display = display;
            Count = count;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? SelectionChanged;

        public object? Value { get; }

        public string? Display { get; }

        public int Count { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotSlicerModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly object NullKey = new();
        private IEnumerable? _itemsSource;
        private INotifyCollectionChanged? _itemsNotifier;
        private PivotAxisField? _field;
        private PivotFieldFilter? _filter;
        private bool _autoRefresh = true;
        private int _updateNesting;
        private bool _pendingRefresh;
        private bool _isRefreshing;
        private bool _suppressSelectionChanges;
        private bool _suppressFilterSync;
        private CultureInfo _culture = CultureInfo.CurrentCulture;
        private string? _emptyValueLabel = "(blank)";
        private PivotSlicerFilterMode _filterMode = PivotSlicerFilterMode.Include;
        private PivotSlicerSelectionMode _selectionMode = PivotSlicerSelectionMode.Multiple;

        public PivotSlicerModel()
        {
            Items = new PivotObservableCollection<PivotSlicerItem>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PivotObservableCollection<PivotSlicerItem> Items { get; }

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

        public PivotAxisField? Field
        {
            get => _field;
            set
            {
                if (ReferenceEquals(_field, value))
                {
                    return;
                }

                DetachFieldHandlers();
                _field = value;
                AttachFieldHandlers();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Field)));
                RequestRefresh();
            }
        }

        public PivotFieldFilter? Filter
        {
            get => _filter;
            set
            {
                if (ReferenceEquals(_filter, value))
                {
                    return;
                }

                SetFilter(value, updateField: true);
            }
        }

        public PivotSlicerFilterMode FilterMode
        {
            get => _filterMode;
            set
            {
                if (_filterMode == value)
                {
                    return;
                }

                _filterMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterMode)));
                SyncSelectionFromFilter();
            }
        }

        public PivotSlicerSelectionMode SelectionMode
        {
            get => _selectionMode;
            set
            {
                if (_selectionMode == value)
                {
                    return;
                }

                _selectionMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionMode)));
                if (_selectionMode == PivotSlicerSelectionMode.Single)
                {
                    EnsureSingleSelection();
                }
            }
        }

        public string? EmptyValueLabel
        {
            get => _emptyValueLabel;
            set
            {
                if (Equals(_emptyValueLabel, value))
                {
                    return;
                }

                _emptyValueLabel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EmptyValueLabel)));
                RequestRefresh();
            }
        }

        public CultureInfo Culture
        {
            get => _culture;
            set
            {
                if (Equals(_culture, value))
                {
                    return;
                }

                _culture = value ?? CultureInfo.CurrentCulture;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
                RequestRefresh();
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
                    var items = BuildItems();
                    ResetItems(items);
                    SyncSelectionFromFilter();
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

        public void SelectAll()
        {
            if (Items.Count == 0)
            {
                return;
            }

            SetSelection(() =>
            {
                foreach (var item in Items)
                {
                    item.IsSelected = true;
                }
            });
        }

        public void ClearSelection()
        {
            if (Items.Count == 0)
            {
                return;
            }

            SetSelection(() =>
            {
                foreach (var item in Items)
                {
                    item.IsSelected = false;
                }
            });
        }

        public void InvertSelection()
        {
            if (Items.Count == 0)
            {
                return;
            }

            SetSelection(() =>
            {
                foreach (var item in Items)
                {
                    item.IsSelected = !item.IsSelected;
                }
            });
        }

        public void Dispose()
        {
            DetachItemsNotifier();
            DetachFieldHandlers();
            DetachFilterHandlers();
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

        private void AttachFieldHandlers()
        {
            if (_field == null)
            {
                return;
            }

            _field.PropertyChanged += Field_PropertyChanged;
            EnsureFilter();
        }

        private void DetachFieldHandlers()
        {
            if (_field == null)
            {
                return;
            }

            _field.PropertyChanged -= Field_PropertyChanged;
        }

        private void Field_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_field == null)
            {
                return;
            }

            if (e.PropertyName == nameof(PivotAxisField.Filter))
            {
                if (!_suppressFilterSync)
                {
                    SetFilter(_field.Filter, updateField: false);
                    SyncSelectionFromFilter();
                }

                return;
            }

            RequestRefresh();
        }

        private void EnsureFilter()
        {
            if (_field == null)
            {
                return;
            }

            if (_filter == null)
            {
                SetFilter(_field.Filter ?? new PivotFieldFilter(), updateField: true);
            }
            else if (!ReferenceEquals(_field.Filter, _filter))
            {
                _suppressFilterSync = true;
                _field.Filter = _filter;
                _suppressFilterSync = false;
            }
        }

        private void SetFilter(PivotFieldFilter? filter, bool updateField)
        {
            if (ReferenceEquals(_filter, filter))
            {
                return;
            }

            DetachFilterHandlers();
            _filter = filter;
            AttachFilterHandlers();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Filter)));

            if (updateField && _field != null && !ReferenceEquals(_field.Filter, _filter))
            {
                _suppressFilterSync = true;
                _field.Filter = _filter;
                _suppressFilterSync = false;
            }
        }

        private void AttachFilterHandlers()
        {
            if (_filter != null)
            {
                _filter.Changed += Filter_Changed;
            }
        }

        private void DetachFilterHandlers()
        {
            if (_filter != null)
            {
                _filter.Changed -= Filter_Changed;
            }
        }

        private void Filter_Changed(object? sender, EventArgs e)
        {
            if (_suppressFilterSync)
            {
                return;
            }

            SyncSelectionFromFilter();
        }

        private List<PivotSlicerItem> BuildItems()
        {
            if (_itemsSource == null || _field == null)
            {
                return new List<PivotSlicerItem>();
            }

            var comparer = EqualityComparer<object?>.Default;
            var counts = new Dictionary<object, int>(EqualityComparer<object>.Default);

            foreach (var item in _itemsSource)
            {
                var value = _field.GetGroupValue(item);
                var key = value ?? NullKey;
                counts[key] = counts.TryGetValue(key, out var count) ? count + 1 : 1;
            }

            List<object?> values;
            if (_field.ShowItemsWithNoData && _field.ItemsSource != null)
            {
                values = BuildValuesFromItemsSource(_field, comparer);
            }
            else
            {
                values = counts.Keys
                    .Select(key => ReferenceEquals(key, NullKey) ? null : key)
                    .ToList();
            }

            SortValues(values);

            var items = new List<PivotSlicerItem>(values.Count);
            foreach (var value in values)
            {
                var key = value ?? NullKey;
                counts.TryGetValue(key, out var count);
                var display = _field.FormatValue(value, _culture, _emptyValueLabel);
                items.Add(new PivotSlicerItem(value, display, count));
            }

            return items;
        }

        private static List<object?> BuildValuesFromItemsSource(PivotAxisField field, IEqualityComparer<object?> comparer)
        {
            var values = new List<object?>();
            var seen = new HashSet<object?>(comparer);
            foreach (var item in field.ItemsSource ?? Array.Empty<object?>())
            {
                var value = item;
                if (field.ApplyGroupSelectorToItemsSource && field.GroupSelector != null)
                {
                    value = field.GroupSelector(item);
                }

                if (seen.Add(value))
                {
                    values.Add(value);
                }
            }

            return values;
        }

        private void SortValues(List<object?> values)
        {
            if (values.Count <= 1)
            {
                return;
            }

            var comparer = _field?.Comparer ?? new PivotSlicerComparer(_culture);
            var direction = _field?.SortDirection ?? ListSortDirection.Ascending;

            values.Sort((left, right) =>
            {
                var result = comparer.Compare(left, right);
                return direction == ListSortDirection.Descending ? -result : result;
            });
        }

        private void ResetItems(List<PivotSlicerItem> items)
        {
            foreach (var item in Items)
            {
                item.SelectionChanged -= Item_SelectionChanged;
            }

            _suppressSelectionChanges = true;
            Items.ResetWith(items);
            foreach (var item in Items)
            {
                item.SelectionChanged += Item_SelectionChanged;
            }
            _suppressSelectionChanges = false;
        }

        private void Item_SelectionChanged(object? sender, EventArgs e)
        {
            if (_suppressSelectionChanges || sender is not PivotSlicerItem changedItem)
            {
                return;
            }

            if (_selectionMode == PivotSlicerSelectionMode.Single && changedItem.IsSelected)
            {
                SetSelection(() =>
                {
                    foreach (var item in Items)
                    {
                        if (!ReferenceEquals(item, changedItem))
                        {
                            item.IsSelected = false;
                        }
                    }
                });
                return;
            }

            ApplySelectionToFilter();
        }

        private void SetSelection(Action update)
        {
            _suppressSelectionChanges = true;
            update();
            _suppressSelectionChanges = false;
            ApplySelectionToFilter();
        }

        private void EnsureSingleSelection()
        {
            if (Items.Count == 0)
            {
                return;
            }

            PivotSlicerItem? selected = null;
            foreach (var item in Items)
            {
                if (item.IsSelected)
                {
                    selected ??= item;
                    if (!ReferenceEquals(item, selected))
                    {
                        item.IsSelected = false;
                    }
                }
            }
        }

        private void SyncSelectionFromFilter()
        {
            if (_filter == null)
            {
                _suppressSelectionChanges = true;
                foreach (var item in Items)
                {
                    item.IsSelected = _filterMode == PivotSlicerFilterMode.Include;
                }
                _suppressSelectionChanges = false;
                return;
            }

            var includes = _filter.Included;
            var excludes = _filter.Excluded;
            var hasIncludes = includes.Count > 0;
            var hasExcludes = excludes.Count > 0;

            _suppressSelectionChanges = true;
            foreach (var item in Items)
            {
                var value = item.Value;
                if (_filterMode == PivotSlicerFilterMode.Include)
                {
                    if (hasIncludes)
                    {
                        item.IsSelected = includes.Contains(value);
                    }
                    else if (hasExcludes)
                    {
                        item.IsSelected = !excludes.Contains(value);
                    }
                    else
                    {
                        item.IsSelected = true;
                    }
                }
                else
                {
                    if (hasExcludes)
                    {
                        item.IsSelected = excludes.Contains(value);
                    }
                    else if (hasIncludes)
                    {
                        item.IsSelected = !includes.Contains(value);
                    }
                    else
                    {
                        item.IsSelected = false;
                    }
                }
            }
            _suppressSelectionChanges = false;
        }

        private void ApplySelectionToFilter()
        {
            if (_field == null || Items.Count == 0)
            {
                return;
            }

            var selected = Items.Where(item => item.IsSelected).Select(item => item.Value).ToList();

            PivotFieldFilter next;
            if (_filterMode == PivotSlicerFilterMode.Include)
            {
                next = selected.Count == 0 || selected.Count == Items.Count
                    ? new PivotFieldFilter()
                    : new PivotFieldFilter(included: selected);
            }
            else
            {
                next = selected.Count == 0
                    ? new PivotFieldFilter()
                    : new PivotFieldFilter(excluded: selected);
            }

            _suppressFilterSync = true;
            SetFilter(next, updateField: true);
            _suppressFilterSync = false;
            SyncSelectionFromFilter();
        }

        private sealed class PivotSlicerComparer : IComparer<object?>
        {
            private readonly CompareInfo _compareInfo;

            public PivotSlicerComparer(CultureInfo culture)
            {
                _compareInfo = (culture ?? CultureInfo.CurrentCulture).CompareInfo;
            }

            public int Compare(object? x, object? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x == null)
                {
                    return 1;
                }

                if (y == null)
                {
                    return -1;
                }

                if (PivotNumeric.TryGetDouble(x, out var xNumber) && PivotNumeric.TryGetDouble(y, out var yNumber))
                {
                    return xNumber.CompareTo(yNumber);
                }

                if (x is string xString && y is string yString)
                {
                    return _compareInfo.Compare(xString, yString, CompareOptions.StringSort);
                }

                if (x is IComparable comparable && y is IComparable)
                {
                    try
                    {
                        return comparable.CompareTo(y);
                    }
                    catch
                    {
                        // fall through
                    }
                }

                var xText = x.ToString() ?? string.Empty;
                var yText = y.ToString() ?? string.Empty;
                return _compareInfo.Compare(xText, yText, CompareOptions.StringSort);
            }
        }

        private sealed class UpdateScope : IDisposable
        {
            private PivotSlicerModel? _owner;

            public UpdateScope(PivotSlicerModel owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner == null)
                {
                    return;
                }

                _owner.EndUpdate();
                _owner = null;
            }
        }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotValueFilterModel : INotifyPropertyChanged
    {
        private PivotValueFilterType _filterType;
        private PivotValueField? _valueField;
        private double? _value;
        private double? _value2;
        private int? _count;
        private double? _percent;
        private PivotAxisField? _field;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PivotAxisField? Field
        {
            get => _field;
            set
            {
                if (ReferenceEquals(_field, value))
                {
                    return;
                }

                _field = value;
                ApplyToField();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Field)));
            }
        }

        public PivotValueFilterType FilterType
        {
            get => _filterType;
            set => SetProperty(ref _filterType, value, nameof(FilterType));
        }

        public PivotValueField? ValueField
        {
            get => _valueField;
            set => SetProperty(ref _valueField, value, nameof(ValueField));
        }

        public double? Value
        {
            get => _value;
            set => SetProperty(ref _value, value, nameof(Value));
        }

        public double? Value2
        {
            get => _value2;
            set => SetProperty(ref _value2, value, nameof(Value2));
        }

        public int? Count
        {
            get => _count;
            set => SetProperty(ref _count, value, nameof(Count));
        }

        public double? Percent
        {
            get => _percent;
            set => SetProperty(ref _percent, value, nameof(Percent));
        }

        private void ApplyToField()
        {
            if (_field == null)
            {
                return;
            }

            _field.ValueFilter = new PivotValueFilter
            {
                FilterType = _filterType,
                ValueField = _valueField,
                Value = _value,
                Value2 = _value2,
                Count = _count,
                Percent = _percent
            };
        }

        private void SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            ApplyToField();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
