// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Columns;

public class DataGridTextColumnThemeTests
{
    [AvaloniaFact]
    public void Derived_Text_Column_Reuses_Cell_Themes()
        {
            var textBoxTheme = new ControlTheme(typeof(TextBox));
            var textBlockTheme = new ControlTheme(typeof(TextBlock));

            var grid = new DataGrid();
        grid.Resources.Add("DataGridCellTextBoxTheme", textBoxTheme);
        grid.Resources.Add("DataGridCellTextBlockTheme", textBlockTheme);

        var column = new DerivedTextColumn
        {
            Header = "Name",
            Binding = new Binding("Name")
        };

        grid.ColumnsInternal.Add(column);

        var displayElement = column.CreateDisplayElement(new DataGridCell(), new object());
        var editingElement = column.CreateEditingElement(new DataGridCell(), new object());

        Assert.Same(textBlockTheme, displayElement.Theme);
        Assert.Same(textBoxTheme, editingElement.Theme);
    }

    [AvaloniaFact]
    public void Theme_Lookup_Allows_Missing_Resources()
    {
        var grid = new DataGrid();
        var column = new DerivedTextColumn
        {
            Header = "Name",
            Binding = new Binding("Name")
        };

        grid.ColumnsInternal.Add(column);

        var (textBoxTheme, textBlockTheme) = column.GetThemes();

        Assert.Null(textBoxTheme);
        Assert.Null(textBlockTheme);
    }

    [AvaloniaFact]
    public void Theme_Is_Not_Locked_To_Null_When_Accessed_Before_Attach()
    {
        var textBoxTheme = new ControlTheme(typeof(TextBox));
        var textBlockTheme = new ControlTheme(typeof(TextBlock));

        var grid = new DataGrid();
        grid.Resources.Add("DataGridCellTextBoxTheme", textBoxTheme);
        grid.Resources.Add("DataGridCellTextBlockTheme", textBlockTheme);

        var column = new DerivedTextColumn
        {
            Header = "Name",
            Binding = new Binding("Name")
        };

        // Touch themes before the column is attached to the grid.
        var pre = column.GetThemes();
        Assert.Null(pre.textBoxTheme);
        Assert.Null(pre.textBlockTheme);

        grid.ColumnsInternal.Add(column);

        var post = column.GetThemes();
        Assert.Same(textBoxTheme, post.textBoxTheme);
        Assert.Same(textBlockTheme, post.textBlockTheme);
    }

    [AvaloniaFact]
    public void TextColumn_Respects_Watermark()
    {
        var column = new WatermarkTextColumn
        {
            Watermark = "Enter name"
        };

        var editingElement = column.CreateEditingElement(new DataGridCell(), new object());

        Assert.Equal("Enter name", editingElement.Watermark);
    }

    private class DerivedTextColumn : DataGridTextColumn
    {
        public CustomTextBox CreateEditingElement(DataGridCell cell, object dataItem)
        {
            return (CustomTextBox)GenerateEditingElementDirect(cell, dataItem);
        }

        public CustomTextBlock CreateDisplayElement(DataGridCell cell, object dataItem)
        {
            return (CustomTextBlock)GenerateElement(cell, dataItem);
        }

        public (ControlTheme textBoxTheme, ControlTheme textBlockTheme) GetThemes()
        {
            return (CellTextBoxTheme, CellTextBlockTheme);
        }

        protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
        {
            var textBox = new CustomTextBox
            {
                Name = "CustomCellTextBox"
            };

            if (CellTextBoxTheme is { } theme)
            {
                textBox.Theme = theme;
            }

            return textBox;
        }

        protected override Control GenerateElement(DataGridCell cell, object dataItem)
        {
            var textBlock = new CustomTextBlock
            {
                Name = "CustomCellTextBlock"
            };

            if (CellTextBlockTheme is { } theme)
            {
                textBlock.Theme = theme;
            }

            return textBlock;
        }
    }

    private class CustomTextBox : TextBox
    {
    }

    private class CustomTextBlock : TextBlock
    {
    }

    private sealed class WatermarkTextColumn : DataGridTextColumn
    {
        public TextBox CreateEditingElement(DataGridCell cell, object dataItem) =>
            (TextBox)GenerateEditingElementDirect(cell, dataItem);
    }
}
