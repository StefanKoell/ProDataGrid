// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Input;

namespace Avalonia.Controls
{
    internal sealed class XmlClipboardFormatExporter : IDataGridClipboardFormatExporter
    {
        internal static readonly DataFormat<string> XmlFormat = DataFormat.CreateStringPlatformFormat("text/xml");

        public bool TryExport(DataGridClipboardExportContext context, DataTransferItem item)
        {
            if (!context.Formats.HasFlag(DataGridClipboardExportFormat.Xml))
            {
                return false;
            }

            var xml = DataGridClipboardFormatting.BuildXml(context.Rows);
            if (string.IsNullOrEmpty(xml))
            {
                return false;
            }

            item.Set(XmlFormat, xml);
            return true;
        }
    }
}
