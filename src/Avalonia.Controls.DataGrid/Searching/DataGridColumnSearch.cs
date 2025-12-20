// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System;
using Avalonia;
using Avalonia.Controls;

namespace Avalonia.Controls.DataGridSearching
{
    /// <summary>
    /// Attached properties for search configuration on <see cref="DataGridColumn"/>.
    /// </summary>
    public static class DataGridColumnSearch
    {
        public static readonly AttachedProperty<bool> IsSearchableProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumn, bool>(
                "IsSearchable",
                typeof(DataGridColumnSearch),
                defaultValue: true);

        public static readonly AttachedProperty<string> SearchMemberPathProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumn, string>(
                "SearchMemberPath",
                typeof(DataGridColumnSearch));

        public static readonly AttachedProperty<Func<object, string>> TextProviderProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumn, Func<object, string>>(
                "TextProvider",
                typeof(DataGridColumnSearch));

        public static readonly AttachedProperty<IFormatProvider> FormatProviderProperty =
            AvaloniaProperty.RegisterAttached<DataGridColumn, IFormatProvider>(
                "FormatProvider",
                typeof(DataGridColumnSearch));

        public static void SetIsSearchable(AvaloniaObject target, bool value)
        {
            target.SetValue(IsSearchableProperty, value);
        }

        public static bool GetIsSearchable(AvaloniaObject target)
        {
            return target.GetValue(IsSearchableProperty);
        }

        public static void SetSearchMemberPath(AvaloniaObject target, string value)
        {
            target.SetValue(SearchMemberPathProperty, value);
        }

        public static string GetSearchMemberPath(AvaloniaObject target)
        {
            return target.GetValue(SearchMemberPathProperty);
        }

        public static void SetTextProvider(AvaloniaObject target, Func<object, string> value)
        {
            target.SetValue(TextProviderProperty, value);
        }

        public static Func<object, string> GetTextProvider(AvaloniaObject target)
        {
            return target.GetValue(TextProviderProperty);
        }

        public static void SetFormatProvider(AvaloniaObject target, IFormatProvider value)
        {
            target.SetValue(FormatProviderProperty, value);
        }

        public static IFormatProvider GetFormatProvider(AvaloniaObject target)
        {
            return target.GetValue(FormatProviderProperty);
        }
    }
}
