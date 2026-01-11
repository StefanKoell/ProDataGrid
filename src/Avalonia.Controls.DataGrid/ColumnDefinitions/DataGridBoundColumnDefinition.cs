// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    abstract class DataGridBoundColumnDefinition : DataGridColumnDefinition
    {
        private DataGridBindingDefinition _binding;
        private DataGridBindingDefinition _clipboardContentBinding;

        public DataGridBindingDefinition Binding
        {
            get => _binding;
            set
            {
                var previous = _binding;
                if (SetProperty(ref _binding, value))
                {
                    UpdateBindingMetadata(previous, value);
                }
            }
        }

        public DataGridBindingDefinition ClipboardContentBinding
        {
            get => _clipboardContentBinding;
            set => SetProperty(ref _clipboardContentBinding, value);
        }

        private void UpdateBindingMetadata(DataGridBindingDefinition previous, DataGridBindingDefinition current)
        {
            if (ValueAccessor == null || ReferenceEquals(ValueAccessor, previous?.ValueAccessor))
            {
                ValueAccessor = current?.ValueAccessor;
            }

            if (ValueType == null || ReferenceEquals(ValueType, previous?.ValueType))
            {
                ValueType = current?.ValueType;
            }
        }

        protected override void ApplyColumnProperties(DataGridColumn column, DataGridColumnDefinitionContext context)
        {
            if (column is DataGridBoundColumn boundColumn)
            {
                boundColumn.Binding = Binding?.CreateBinding();
                boundColumn.ClipboardContentBinding = ClipboardContentBinding?.CreateBinding();

                if (ValueAccessor == null && Binding?.ValueAccessor != null &&
                    (ValueType == null || Binding.ValueType == ValueType))
                {
                    DataGridColumnMetadata.SetValueAccessor(column, Binding.ValueAccessor);
                }
            }
        }

        protected override bool ApplyColumnPropertyChange(
            DataGridColumn column,
            DataGridColumnDefinitionContext context,
            string propertyName)
        {
            if (column is not DataGridBoundColumn boundColumn)
            {
                return false;
            }

            switch (propertyName)
            {
                case nameof(Binding):
                    boundColumn.Binding = Binding?.CreateBinding();
                    ApplyBindingMetadata(column);
                    return true;
                case nameof(ClipboardContentBinding):
                    boundColumn.ClipboardContentBinding = ClipboardContentBinding?.CreateBinding();
                    return true;
                case nameof(ValueAccessor):
                case nameof(ValueType):
                    ApplyBindingMetadata(column);
                    return true;
            }

            return false;
        }

        private void ApplyBindingMetadata(DataGridColumn column)
        {
            if (ValueAccessor == null && Binding?.ValueAccessor != null &&
                (ValueType == null || Binding.ValueType == ValueType))
            {
                DataGridColumnMetadata.SetValueAccessor(column, Binding.ValueAccessor);
            }
        }
    }
}
