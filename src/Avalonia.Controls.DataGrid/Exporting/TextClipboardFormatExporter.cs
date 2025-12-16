// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Input;

namespace Avalonia.Controls
{
    internal sealed class TextClipboardFormatExporter : IDataGridClipboardFormatExporter
    {
        public bool TryExport(DataGridClipboardExportContext context, DataTransferItem item)
        {
            if (!context.Formats.HasFlag(DataGridClipboardExportFormat.Text))
            {
                return false;
            }

            var text = DataGridClipboardFormatting.BuildDelimitedText(context.Rows, '\t', quoteAlways: true);
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            item.Set(DataFormat.Text, text);
            return true;
        }
    }
}
