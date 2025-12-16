// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Input;

namespace Avalonia.Controls
{
    /// <summary>
    /// Contract for building clipboard payloads for a DataGrid copy operation.
    /// </summary>
#if !DATAGRID_INTERNAL
    public
#endif
    interface IDataGridClipboardExporter
    {
        /// <summary>
        /// Builds an <see cref="IAsyncDataTransfer"/> with the requested formats.
        /// </summary>
        /// <param name="context">Export context.</param>
        /// <returns>Clipboard data or null to cancel copying.</returns>
        IAsyncDataTransfer? BuildClipboardData(DataGridClipboardExportContext context);
    }
}
