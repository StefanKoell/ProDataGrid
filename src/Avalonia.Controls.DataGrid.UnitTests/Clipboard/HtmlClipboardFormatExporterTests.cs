// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Clipboard;

public class HtmlClipboardFormatExporterTests
{
    [AvaloniaFact]
    public void HtmlExporter_Writes_Html()
    {
        var rows = ClipboardTestData.BuildRows();
        var item = new DataTransferItem();
        var exporter = new HtmlClipboardFormatExporter();

        var result = exporter.TryExport(
            new DataGridClipboardExportContext(
                new DataGrid(),
                rows,
                DataGridClipboardCopyMode.IncludeHeader,
                DataGridClipboardExportFormat.Html,
                DataGridSelectionUnit.FullRow),
            item);

        Assert.True(result);
        var html = item.TryGetRaw(HtmlClipboardFormatExporter.HtmlFormat) as string;
        Assert.NotNull(html);
        Assert.Contains("StartHTML:", html);
        Assert.Contains("<table>", html);
        Assert.Contains("<th>Name</th>", html);
        Assert.Contains("<td>Alpha</td>", html);
    }
}
