// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Utils;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Utilities;

namespace Avalonia.Collections
{
#if !DATAGRID_INTERNAL
    public
#endif

    abstract class DataGridGroupDescription : INotifyPropertyChanged
    {
        public AvaloniaList<object> GroupKeys { get; }

        public DataGridGroupDescription()
        {
            GroupKeys = new AvaloniaList<object>();
            GroupKeys.CollectionChanged += (sender, e) => OnPropertyChanged(new PropertyChangedEventArgs(nameof(GroupKeys)));
        }

        protected virtual event PropertyChangedEventHandler PropertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }

            remove
            {
                PropertyChanged -= value;
            }
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public virtual string PropertyName => String.Empty;
        public abstract object GroupKeyFromItem(object item, int level, CultureInfo culture);
        public virtual bool KeysMatch(object groupKey, object itemKey)
        {
            return object.Equals(groupKey, itemKey);
        }
    }

    [RequiresUnreferencedCode("Grouping by property path uses reflection and is not compatible with trimming.")]
#if !DATAGRID_INTERNAL
    public
#endif
    class DataGridPathGroupDescription : DataGridGroupDescription
    {
        private string _propertyPath;
        private Type _propertyType;
        private IValueConverter _valueConverter;
        private StringComparison _stringComparison = StringComparison.Ordinal;

        public DataGridPathGroupDescription(string propertyPath)
        {
            _propertyPath = propertyPath;
        }

        public override object GroupKeyFromItem(object item, int level, CultureInfo culture)
        {
            object GetKey(object o)
            {
                if(o == null)
                    return null;

                if (_propertyType == null)
                    _propertyType = GetPropertyType(o);

                return InvokePath(o, _propertyPath, _propertyType);
            }

            var key = GetKey(item);
            if (key == null)
                key = item;

            var valueConverter = ValueConverter;
            if (valueConverter != null)
                key = valueConverter.Convert(key, typeof(object), level, culture);

            return key;
        }
        public override bool KeysMatch(object groupKey, object itemKey)
        {
            if(groupKey is string k1 && itemKey is string k2)
            {
                return String.Equals(k1, k2, _stringComparison);
            }
            else
                return base.KeysMatch(groupKey, itemKey);
        }
        public override string PropertyName => _propertyPath;

        public IValueConverter ValueConverter { get => _valueConverter; set => _valueConverter = value; }

        private Type GetPropertyType(object o)
        {
            return o.GetType().GetNestedPropertyType(_propertyPath);
        }
        private static object InvokePath(object item, string propertyPath, Type propertyType)
        {
            object propertyValue = TypeHelper.GetNestedPropertyValue(item, propertyPath, propertyType, out Exception exception);
            if (exception != null)
            {
                throw exception;
            }
            return propertyValue;
        }
    }

#if !DATAGRID_INTERNAL
    public
#endif
    abstract class DataGridCollectionViewGroup : INotifyPropertyChanged
    {
        private int _itemCount;

        public object Key { get; }
        public int ItemCount => _itemCount;
        public IAvaloniaReadOnlyList<object> Items => ProtectedItems;

        protected AvaloniaList<object> ProtectedItems { get; }
        protected int ProtectedItemCount
        {
            get { return _itemCount; }
            set
            {
                _itemCount = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemCount)));
            }
        }

        internal abstract DataGridCollectionViewGroupInternal Parent { get; }
        internal abstract DataGridGroupDescription GroupBy { get; set; }

        protected DataGridCollectionViewGroup(object key)
        {
            Key = key;
            ProtectedItems = new AvaloniaList<object>();
        }

        public abstract bool IsBottomLevel { get; }

        protected virtual event PropertyChangedEventHandler PropertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                PropertyChanged += value;
            }

            remove
            {
                PropertyChanged -= value;
            }
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }

}
