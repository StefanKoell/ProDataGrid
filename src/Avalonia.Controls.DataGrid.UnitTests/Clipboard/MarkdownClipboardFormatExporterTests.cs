// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Clipboard;

public class MarkdownClipboardFormatExporterTests
{
    [AvaloniaFact]
    public void MarkdownExporter_Writes_Markdown()
    {
        var rows = ClipboardTestData.BuildRows();
        var item = new DataTransferItem();
        var exporter = new MarkdownClipboardFormatExporter();

        var result = exporter.TryExport(
            new DataGridClipboardExportContext(
                new DataGrid(),
                rows,
                DataGridClipboardCopyMode.IncludeHeader,
                DataGridClipboardExportFormat.Markdown,
                DataGridSelectionUnit.FullRow),
            item);

        Assert.True(result);
        var expected = $"|Name|Value|{System.Environment.NewLine}|---|---|{System.Environment.NewLine}|Alpha|1|{System.Environment.NewLine}";
        var actual = item.TryGetRaw(MarkdownClipboardFormatExporter.MarkdownFormat) as string;
        Assert.Equal(Normalize(expected), Normalize(actual ?? string.Empty));
    }

    private static string Normalize(string value) =>
        value.Replace("\r\n", "\n", System.StringComparison.Ordinal).Replace('\r', '\n');
}
