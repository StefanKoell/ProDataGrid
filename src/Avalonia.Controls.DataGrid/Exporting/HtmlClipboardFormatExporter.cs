// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Input;

namespace Avalonia.Controls
{
    internal sealed class HtmlClipboardFormatExporter : IDataGridClipboardFormatExporter
    {
        internal static readonly DataFormat<string> HtmlFormat = DataFormat.CreateStringPlatformFormat("text/html");
        internal static readonly DataFormat<string> HtmlWindowsFormat = DataFormat.CreateStringPlatformFormat("HTML Format");

        public bool TryExport(DataGridClipboardExportContext context, DataTransferItem item)
        {
            if (!context.Formats.HasFlag(DataGridClipboardExportFormat.Html))
            {
                return false;
            }

            var html = DataGridClipboardFormatting.BuildHtml(context.Rows);
            if (string.IsNullOrEmpty(html))
            {
                return false;
            }

            item.Set(HtmlFormat, html);
            item.Set(HtmlWindowsFormat, html);
            return true;
        }
    }
}
