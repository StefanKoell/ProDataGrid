// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace Avalonia.Controls
{
    /// <summary>
    /// Provides context for clipboard export operations.
    /// </summary>
#if !DATAGRID_INTERNAL
    public
#endif
    sealed class DataGridClipboardExportContext
    {
        public DataGridClipboardExportContext(
            DataGrid grid,
            IReadOnlyList<DataGridRowClipboardEventArgs> rows,
            DataGridClipboardCopyMode copyMode,
            DataGridClipboardExportFormat formats,
            DataGridSelectionUnit selectionUnit)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Rows = rows ?? throw new ArgumentNullException(nameof(rows));
            CopyMode = copyMode;
            Formats = formats;
            SelectionUnit = selectionUnit;
        }

        public DataGrid Grid { get; }

        public IReadOnlyList<DataGridRowClipboardEventArgs> Rows { get; }

        public DataGridClipboardCopyMode CopyMode { get; }

        public DataGridClipboardExportFormat Formats { get; }

        public DataGridSelectionUnit SelectionUnit { get; }
    }
}
