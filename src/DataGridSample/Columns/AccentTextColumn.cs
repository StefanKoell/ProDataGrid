using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;

namespace DataGridSample.Columns;

public class AccentTextColumn : DataGridTextColumn
{
    public string Watermark { get; set; } = "Add details";

    protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
    {
        var textBox = new AccentTextBox
        {
            Name = "AccentCellTextBox",
            Watermark = Watermark,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap
        };

        if (CellTextBoxTheme is { } theme)
        {
            textBox.Theme = theme;
        }

        return textBox;
    }

    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        var textBlock = new AccentTextBlock
        {
            Name = "AccentCellTextBlock",
            TextWrapping = TextWrapping.Wrap
        };

        if (CellTextBlockTheme is { } theme)
        {
            textBlock.Theme = theme;
        }

        if (Binding != null && !ReferenceEquals(dataItem, DataGridCollectionView.NewItemPlaceholder))
        {
            textBlock.Bind(TextBlock.TextProperty, Binding);
        }

        return textBlock;
    }
}

public class AccentTextBox : TextBox
{
}

public class AccentTextBlock : TextBlock
{
}
