// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace DataGridSample.Adapters
{
    /// <summary>
    /// Filtering adapter that relies on column value accessors to avoid reflection.
    /// </summary>
    public sealed class AccessorFilteringAdapter : DataGridFilteringAdapter
    {
        private readonly Func<IEnumerable<DataGridColumn>> _columnProvider;

        public AccessorFilteringAdapter(
            IFilteringModel model,
            Func<IEnumerable<DataGridColumn>> columnProvider,
            Action beforeViewRefresh = null,
            Action afterViewRefresh = null)
            : base(model, columnProvider, beforeViewRefresh, afterViewRefresh)
        {
            _columnProvider = columnProvider ?? throw new ArgumentNullException(nameof(columnProvider));
        }

        protected override bool TryApplyModelToView(
            IReadOnlyList<FilteringDescriptor> descriptors,
            IReadOnlyList<FilteringDescriptor> previousDescriptors,
            out bool changed)
        {
            var view = View;
            if (view == null)
            {
                changed = false;
                return true;
            }

            var predicate = ComposePredicate(descriptors);
            if (ReferenceEquals(view.Filter, predicate))
            {
                changed = false;
                return true;
            }

            using (view.DeferRefresh())
            {
                view.Filter = predicate;
            }

            changed = true;
            return true;
        }

        private Func<object, bool> ComposePredicate(IReadOnlyList<FilteringDescriptor> descriptors)
        {
            if (descriptors == null || descriptors.Count == 0)
            {
                return null;
            }

            var compiled = new List<Func<object, bool>>();
            foreach (var descriptor in descriptors)
            {
                var predicate = Compile(descriptor);
                if (predicate != null)
                {
                    compiled.Add(predicate);
                }
            }

            if (compiled.Count == 0)
            {
                return null;
            }

            if (compiled.Count == 1)
            {
                return compiled[0];
            }

            return item =>
            {
                for (int i = 0; i < compiled.Count; i++)
                {
                    if (!compiled[i](item))
                    {
                        return false;
                    }
                }

                return true;
            };
        }

        private Func<object, bool> Compile(FilteringDescriptor descriptor)
        {
            if (descriptor == null)
            {
                return null;
            }

            var column = FindColumn(descriptor);
            if (column != null)
            {
                var factory = DataGridColumnFilter.GetPredicateFactory(column);
                if (factory != null)
                {
                    return factory(descriptor);
                }
            }

            if (descriptor.Predicate != null)
            {
                return descriptor.Predicate;
            }

            if (column == null)
            {
                return null;
            }

            var accessor = DataGridColumnMetadata.GetValueAccessor(column);
            if (accessor == null)
            {
                return null;
            }

            return item =>
            {
                var value = accessor.GetValue(item);
                switch (descriptor.Operator)
                {
                    case FilteringOperator.Equals:
                        return Equals(value, descriptor.Value);
                    case FilteringOperator.NotEquals:
                        return !Equals(value, descriptor.Value);
                    case FilteringOperator.Contains:
                        return Contains(value, descriptor.Value, descriptor.StringComparisonMode);
                    case FilteringOperator.StartsWith:
                        return StartsWith(value, descriptor.Value, descriptor.StringComparisonMode);
                    case FilteringOperator.EndsWith:
                        return EndsWith(value, descriptor.Value, descriptor.StringComparisonMode);
                    case FilteringOperator.GreaterThan:
                        return Compare(value, descriptor.Value, descriptor.Culture) > 0;
                    case FilteringOperator.GreaterThanOrEqual:
                        return Compare(value, descriptor.Value, descriptor.Culture) >= 0;
                    case FilteringOperator.LessThan:
                        return Compare(value, descriptor.Value, descriptor.Culture) < 0;
                    case FilteringOperator.LessThanOrEqual:
                        return Compare(value, descriptor.Value, descriptor.Culture) <= 0;
                    case FilteringOperator.Between:
                        return Between(value, descriptor.Values, descriptor.Culture);
                    case FilteringOperator.In:
                        return In(value, descriptor.Values);
                    default:
                        return true;
                }
            };
        }

        private DataGridColumn FindColumn(FilteringDescriptor descriptor)
        {
            var columns = _columnProvider?.Invoke();
            if (columns == null)
            {
                return null;
            }

            foreach (var column in columns)
            {
                if (ReferenceEquals(column, descriptor.ColumnId))
                {
                    return column;
                }

                if (descriptor.ColumnId is DataGridColumnDefinition definition)
                {
                    var definitionPath = definition.SortMemberPath;
                    if (!string.IsNullOrEmpty(definitionPath) &&
                        string.Equals(definitionPath, GetSortPropertyName(column), StringComparison.Ordinal))
                    {
                        return column;
                    }
                }

                if (!string.IsNullOrEmpty(descriptor.PropertyPath))
                {
                    var propertyName = GetSortPropertyName(column);
                    if (!string.IsNullOrEmpty(propertyName) &&
                        string.Equals(propertyName, descriptor.PropertyPath, StringComparison.Ordinal))
                    {
                        return column;
                    }
                }
            }

            return null;
        }

        private static string GetSortPropertyName(DataGridColumn column)
        {
            if (column == null)
            {
                return null;
            }

            var result = column.SortMemberPath;
            if (string.IsNullOrEmpty(result) && column is DataGridBoundColumn boundColumn)
            {
                if (boundColumn.Binding is Binding binding)
                {
                    result = binding.Path;
                }
                else if (boundColumn.Binding is CompiledBindingExtension compiledBinding)
                {
                    result = compiledBinding.Path?.ToString();
                }
            }

            return result;
        }

        private static bool Contains(object source, object target, StringComparison? comparison)
        {
            if (source == null || target == null)
            {
                return false;
            }

            if (source is string s && target is string t)
            {
                return s.IndexOf(t, comparison ?? StringComparison.Ordinal) >= 0;
            }

            if (source is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (Equals(item, target))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool StartsWith(object source, object target, StringComparison? comparison)
        {
            if (source is string s && target is string t)
            {
                return s.StartsWith(t, comparison ?? StringComparison.Ordinal);
            }

            return false;
        }

        private static bool EndsWith(object source, object target, StringComparison? comparison)
        {
            if (source is string s && target is string t)
            {
                return s.EndsWith(t, comparison ?? StringComparison.Ordinal);
            }

            return false;
        }

        private static int Compare(object left, object right, CultureInfo culture)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            if (left is IComparable comparable)
            {
                return comparable.CompareTo(right);
            }

            var comparer = culture != null
                ? Comparer<object>.Create((x, y) =>
                    string.Compare(Convert.ToString(x, culture), Convert.ToString(y, culture), StringComparison.Ordinal))
                : Comparer<object>.Default;

            return comparer.Compare(left, right);
        }

        private static bool Between(object value, IReadOnlyList<object> bounds, CultureInfo culture)
        {
            if (bounds == null || bounds.Count < 2)
            {
                return false;
            }

            var lower = Compare(value, bounds[0], culture) >= 0;
            var upper = Compare(value, bounds[1], culture) <= 0;
            return lower && upper;
        }

        private static bool In(object value, IReadOnlyList<object> values)
        {
            if (values == null || values.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (Equals(value, values[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
