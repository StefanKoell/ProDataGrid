// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace DataGridSample.Pages;

public partial class ClipboardExportPage : UserControl
{
    private readonly JsonClipboardExporter _jsonExporter = new();

    public ClipboardExportPage()
    {
        InitializeComponent();

        ItemsGrid.ItemsSource = BuildItems();
        ItemsGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
        ItemsGrid.SelectionMode = DataGridSelectionMode.Extended;
        ItemsGrid.SelectionUnit = DataGridSelectionUnit.CellOrRowHeader;

        UpdateExportSettings(null, null);
    }

    private void CopyTextFormat(object? sender, RoutedEventArgs e) =>
        CopyFormat(DataGridClipboardExportFormat.Text);

    private void CopyCsvFormat(object? sender, RoutedEventArgs e) =>
        CopyFormat(DataGridClipboardExportFormat.Csv);

    private void CopyHtmlFormat(object? sender, RoutedEventArgs e) =>
        CopyFormat(DataGridClipboardExportFormat.Html);

    private void CopyMarkdownFormat(object? sender, RoutedEventArgs e) =>
        CopyFormat(DataGridClipboardExportFormat.Markdown);

    private void CopyXmlFormat(object? sender, RoutedEventArgs e) =>
        CopyFormat(DataGridClipboardExportFormat.Xml);

    private void CopyYamlFormat(object? sender, RoutedEventArgs e) =>
        CopyFormat(DataGridClipboardExportFormat.Yaml);

    private void CopyFormat(DataGridClipboardExportFormat format)
    {
        ItemsGrid.CopySelectionToClipboard(format, exporter: null);
    }

    private void UpdateExportSettings(object? sender, RoutedEventArgs? e)
    {
        var formats = DataGridClipboardExportFormat.None;

        if (TextFormatCheckBox.IsChecked == true)
        {
            formats |= DataGridClipboardExportFormat.Text;
        }

        if (CsvFormatCheckBox.IsChecked == true)
        {
            formats |= DataGridClipboardExportFormat.Csv;
        }

        if (HtmlFormatCheckBox.IsChecked == true)
        {
            formats |= DataGridClipboardExportFormat.Html;
        }

        if (MarkdownFormatCheckBox.IsChecked == true)
        {
            formats |= DataGridClipboardExportFormat.Markdown;
        }

        if (XmlFormatCheckBox.IsChecked == true)
        {
            formats |= DataGridClipboardExportFormat.Xml;
        }

        if (YamlFormatCheckBox.IsChecked == true)
        {
            formats |= DataGridClipboardExportFormat.Yaml;
        }

        if (formats == DataGridClipboardExportFormat.None)
        {
            formats = DataGridClipboardExportFormat.Text;
            TextFormatCheckBox.IsChecked = true;
        }

        ItemsGrid.ClipboardExportFormats = formats;
        ItemsGrid.ClipboardExporter = CustomExporterCheckBox.IsChecked == true ? _jsonExporter : null;
    }

    private static IReadOnlyList<ClipboardSampleItem> BuildItems() => new List<ClipboardSampleItem>
    {
        new("Kumquat", "Citrus", 12.50m, new DateTime(2025, 2, 3)),
        new("Morel", "Foraged", 42.00m, new DateTime(2025, 1, 18)),
        new("Radicchio", "Greens", 8.25m, new DateTime(2025, 2, 9)),
        new("Habanero", "Spice", 5.75m, new DateTime(2025, 1, 27)),
        new("Black garlic", "Condiment", 14.90m, new DateTime(2025, 2, 4)),
        new("Quinoa", "Grain", 6.10m, new DateTime(2025, 2, 8)),
        new("Elderflower", "Floral", 18.60m, new DateTime(2025, 1, 31)),
    };

    private sealed record ClipboardSampleItem(string Name, string Category, decimal Price, DateTime LastOrder);

    private sealed class JsonClipboardExporter : IDataGridClipboardExporter
    {
        private static readonly DataFormat<string> JsonFormat = DataFormat.CreateStringPlatformFormat("application/json");

        public IAsyncDataTransfer? BuildClipboardData(DataGridClipboardExportContext context)
        {
            if (context.Rows.Count == 0)
            {
                return null;
            }

            var transfer = new DataTransfer();
            var item = new DataTransferItem();

            var text = BuildDelimited(context.Rows, '\t');
            if (!string.IsNullOrEmpty(text))
            {
                item.Set(DataFormat.Text, text);
            }

            var json = BuildJson(context.Rows);
            if (!string.IsNullOrEmpty(json))
            {
                item.Set(JsonFormat, json);
            }

            transfer.Add(item);
            return transfer;
        }

        private static string BuildDelimited(IReadOnlyList<DataGridRowClipboardEventArgs> rows, char delimiter)
        {
            var builder = new StringBuilder();

            foreach (var row in rows)
            {
                var cells = row.ClipboardRowContent;
                if (cells.Count == 0)
                {
                    builder.Append("\r\n");
                    continue;
                }

                for (int i = 0; i < cells.Count; i++)
                {
                    var value = cells[i].Content?.ToString() ?? string.Empty;
                    value = value.Replace("\"", "\"\"");
                    builder.Append('"').Append(value).Append('"');
                    builder.Append(i == cells.Count - 1 ? "\r\n" : delimiter);
                }
            }

            return builder.ToString();
        }

        private static string BuildJson(IReadOnlyList<DataGridRowClipboardEventArgs> rows)
        {
            var builder = new StringBuilder();
            builder.Append("{\"rows\":[");

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                builder.Append("{\"isHeader\":").Append(row.IsColumnHeadersRow.ToString().ToLowerInvariant()).Append(",\"cells\":[");
                for (int c = 0; c < row.ClipboardRowContent.Count; c++)
                {
                    var cell = row.ClipboardRowContent[c];
                    var value = cell.Content?.ToString() ?? string.Empty;
                    builder.Append('"').Append(value.Replace("\"", "\\\"")).Append('"');
                    if (c < row.ClipboardRowContent.Count - 1)
                    {
                        builder.Append(',');
                    }
                }
                builder.Append("]}");
                if (i < rows.Count - 1)
                {
                    builder.Append(',');
                }
            }

            builder.Append("]}");
            return builder.ToString();
        }
    }
}
