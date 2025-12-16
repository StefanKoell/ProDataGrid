// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Input;

namespace Avalonia.Controls
{
#if !DATAGRID_INTERNAL
    public
#endif
    interface IDataGridClipboardFormatExporter
    {
        bool TryExport(DataGridClipboardExportContext context, DataTransferItem item);
    }
}
