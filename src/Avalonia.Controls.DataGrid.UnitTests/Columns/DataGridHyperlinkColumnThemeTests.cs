// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Columns;

public class DataGridHyperlinkColumnThemeTests
{
    [AvaloniaFact]
    public void HyperlinkColumn_Reuses_Button_And_Editor_Themes()
    {
        var buttonTheme = new ControlTheme(typeof(HyperlinkButton));
        var editorTheme = new ControlTheme(typeof(TextBox));

        var grid = new DataGrid();
        grid.Resources.Add("DataGridCellHyperlinkButtonTheme", buttonTheme);
        grid.Resources.Add("DataGridCellTextBoxTheme", editorTheme);

        var column = new DerivedHyperlinkColumn
        {
            Binding = new Binding(nameof(LinkInfo.Target)),
            ContentBinding = new Binding(nameof(LinkInfo.Label))
        };

        grid.ColumnsInternal.Add(column);

        var displayElement = column.CreateDisplayElement(new DataGridCell(), new LinkInfo());
        var editingElement = column.CreateEditingElement(new DataGridCell(), new LinkInfo());
        var (hyperlink, editor) = column.GetThemes();

        Assert.Same(buttonTheme, displayElement.Theme);
        Assert.Same(editorTheme, editingElement.Theme);
        Assert.Same(buttonTheme, hyperlink);
        Assert.Same(editorTheme, editor);
    }

    [AvaloniaFact]
    public void Hyperlink_Themes_Not_Locked_When_Accessed_Before_Attach()
    {
        var buttonTheme = new ControlTheme(typeof(HyperlinkButton));
        var editorTheme = new ControlTheme(typeof(TextBox));

        var grid = new DataGrid();
        grid.Resources.Add("DataGridCellHyperlinkButtonTheme", buttonTheme);
        grid.Resources.Add("DataGridCellTextBoxTheme", editorTheme);

        var column = new DerivedHyperlinkColumn
        {
            Binding = new Binding(nameof(LinkInfo.Target)),
            ContentBinding = new Binding(nameof(LinkInfo.Label))
        };

        // Touch themes before adding to grid.
        var pre = column.GetThemes();
        Assert.Null(pre.hyperlinkTheme);
        Assert.Null(pre.editorTheme);

        grid.ColumnsInternal.Add(column);

        var post = column.GetThemes();
        Assert.Same(buttonTheme, post.hyperlinkTheme);
        Assert.Same(editorTheme, post.editorTheme);
    }

    [AvaloniaFact]
    public void HyperlinkColumn_Respects_Watermark()
    {
        var column = new DerivedHyperlinkColumn
        {
            Watermark = "Enter link"
        };

        var editor = column.CreateEditingElement(new DataGridCell(), new LinkInfo());

        Assert.Equal("Enter link", editor.Watermark);
    }

    private class DerivedHyperlinkColumn : DataGridHyperlinkColumn
    {
        public HyperlinkButton CreateDisplayElement(DataGridCell cell, object dataItem) =>
            (HyperlinkButton)base.GenerateElement(cell, dataItem);

        public TextBox CreateEditingElement(DataGridCell cell, object dataItem) =>
            (TextBox)GenerateEditingElementDirect(cell, dataItem);

        public (ControlTheme hyperlinkTheme, ControlTheme editorTheme) GetThemes() =>
            (CellHyperlinkButtonTheme, CellTextBoxTheme);
    }

    private class LinkInfo
    {
        public string Label { get; set; } = "Go";
        public string Target { get; set; } = "https://example.com";
    }
}
