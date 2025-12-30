// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace Avalonia.Controls.Utils
{
    internal static class BindingCloneHelper
    {
        public static bool TryCreateExplicitBinding(IBinding binding, out IBinding explicitBinding)
        {
            switch (binding)
            {
                case Binding avaloniaBinding:
                    explicitBinding = CloneBinding(avaloniaBinding);
                    return true;
                case CompiledBindingExtension compiledBinding:
                    explicitBinding = CloneBinding(compiledBinding);
                    return true;
                default:
                    explicitBinding = binding;
                    return false;
            }
        }

        private static Binding CloneBinding(Binding source)
        {
            return new Binding
            {
                Path = source.Path,
                ElementName = source.ElementName,
                RelativeSource = source.RelativeSource,
                Source = source.Source,
                TypeResolver = source.TypeResolver,
                Delay = source.Delay,
                Converter = source.Converter,
                ConverterCulture = source.ConverterCulture,
                ConverterParameter = source.ConverterParameter,
                FallbackValue = source.FallbackValue,
                TargetNullValue = source.TargetNullValue,
                Mode = source.Mode,
                Priority = source.Priority,
                StringFormat = source.StringFormat,
                DefaultAnchor = source.DefaultAnchor,
                NameScope = source.NameScope,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
        }

        private static CompiledBindingExtension CloneBinding(CompiledBindingExtension source)
        {
            return new CompiledBindingExtension
            {
                Path = source.Path,
                Delay = source.Delay,
                Converter = source.Converter,
                ConverterCulture = source.ConverterCulture,
                ConverterParameter = source.ConverterParameter,
                FallbackValue = source.FallbackValue,
                TargetNullValue = source.TargetNullValue,
                Mode = source.Mode,
                Priority = source.Priority,
                StringFormat = source.StringFormat,
                Source = source.Source,
                DataType = source.DataType,
                DefaultAnchor = source.DefaultAnchor,
                NameScope = source.NameScope,
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };
        }
    }
}
