using System;
using Avalonia.Controls;
using Avalonia.Data.Core;

namespace DataGridSample.Helpers
{
    internal static class ColumnDefinitionBindingFactory
    {
        public static DataGridBindingDefinition CreateBinding<TItem, TValue>(
            string name,
            Func<TItem, TValue> getter,
            Action<TItem, TValue>? setter = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Property name is required.", nameof(name));
            }

            if (getter == null)
            {
                throw new ArgumentNullException(nameof(getter));
            }

            var propertyInfo = new ClrPropertyInfo(
                name,
                target => getter((TItem)target),
                setter == null
                    ? null
                    : (target, value) => setter((TItem)target, value is null ? default! : (TValue)value),
                typeof(TValue));

            return DataGridBindingDefinition.Create<TItem, TValue>(propertyInfo, getter, setter);
        }
    }
}
