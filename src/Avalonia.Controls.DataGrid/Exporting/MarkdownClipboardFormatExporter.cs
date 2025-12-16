// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Input;

namespace Avalonia.Controls
{
    internal sealed class MarkdownClipboardFormatExporter : IDataGridClipboardFormatExporter
    {
        internal static readonly DataFormat<string> MarkdownFormat = DataFormat.CreateStringPlatformFormat("text/markdown");

        public bool TryExport(DataGridClipboardExportContext context, DataTransferItem item)
        {
            if (!context.Formats.HasFlag(DataGridClipboardExportFormat.Markdown))
            {
                return false;
            }

            var markdown = DataGridClipboardFormatting.BuildMarkdown(context.Rows);
            if (string.IsNullOrEmpty(markdown))
            {
                return false;
            }

            item.Set(MarkdownFormat, markdown);
            return true;
        }
    }
}
