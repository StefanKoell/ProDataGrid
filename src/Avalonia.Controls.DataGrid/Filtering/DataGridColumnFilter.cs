// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using Avalonia;
using Avalonia.Controls;

namespace Avalonia.Controls.DataGridFiltering
{
    /// <summary>
    /// Attached properties for filtering configuration on <see cref="DataGridColumn"/>.
    /// </summary>
    #if !DATAGRID_INTERNAL
    public
    #else
    internal
    #endif
    static class DataGridColumnFilter
    {
        /// <summary>
        /// Allows a column to supply a predicate factory for descriptors targeting that column.
        /// </summary>
        public static readonly AttachedProperty<Func<FilteringDescriptor, Func<object, bool>>?> PredicateFactoryProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumn, Func<FilteringDescriptor, Func<object, bool>>?>(
                "PredicateFactory",
                typeof(DataGridColumnFilter));

        /// <summary>
        /// Allows a column to supply a value accessor for filtering predicates.
        /// </summary>
        public static readonly AttachedProperty<IDataGridColumnValueAccessor?> ValueAccessorProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumn, IDataGridColumnValueAccessor?>(
                "ValueAccessor",
                typeof(DataGridColumnFilter));

        public static void SetPredicateFactory(AvaloniaObject target, Func<FilteringDescriptor, Func<object, bool>>? value)
        {
            target.SetValue(PredicateFactoryProperty, value);
        }

        public static Func<FilteringDescriptor, Func<object, bool>>? GetPredicateFactory(AvaloniaObject target)
        {
            return target.GetValue(PredicateFactoryProperty);
        }

        public static void SetValueAccessor(AvaloniaObject target, IDataGridColumnValueAccessor? value)
        {
            target.SetValue(ValueAccessorProperty, value);
        }

        public static IDataGridColumnValueAccessor? GetValueAccessor(AvaloniaObject target)
        {
            return target.GetValue(ValueAccessorProperty);
        }
    }
}
