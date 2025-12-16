// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace Avalonia.Controls
{
    /// <summary>
    /// Defines formats that can be exported to the clipboard.
    /// </summary>
    [Flags]
#if !DATAGRID_INTERNAL
    public
#endif
    enum DataGridClipboardExportFormat
    {
        None = 0,
        Text = 1,
        Csv = 2,
        Html = 4,
        Markdown = 8,
        Xml = 16,
        Yaml = 32
    }
}
