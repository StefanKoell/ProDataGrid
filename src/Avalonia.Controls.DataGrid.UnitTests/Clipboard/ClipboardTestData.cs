// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using Avalonia.Controls;

namespace Avalonia.Controls.DataGridTests.Clipboard;

internal static class ClipboardTestData
{
    public static IReadOnlyList<DataGridRowClipboardEventArgs> BuildRows()
    {
        var header = new DataGridRowClipboardEventArgs(null, true);
        header.ClipboardRowContent.Add(new DataGridClipboardCellContent(null, new DataGridTextColumn { Header = "Name" }, "Name"));
        header.ClipboardRowContent.Add(new DataGridClipboardCellContent(null, new DataGridTextColumn { Header = "Value" }, "Value"));

        var row = new DataGridRowClipboardEventArgs(new object(), false);
        row.ClipboardRowContent.Add(new DataGridClipboardCellContent(new object(), new DataGridTextColumn { Header = "Name" }, "Alpha"));
        row.ClipboardRowContent.Add(new DataGridClipboardCellContent(new object(), new DataGridTextColumn { Header = "Value" }, "1"));

        return new List<DataGridRowClipboardEventArgs> { header, row };
    }
}
