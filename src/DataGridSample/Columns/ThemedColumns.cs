using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Utils;

namespace DataGridSample.Columns;

public class ThemedCheckBoxColumn : DataGridCheckBoxColumn
{
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        var checkBox = (CheckBox)base.GenerateElement(cell, dataItem);

        if (CellCheckBoxTheme is { } theme)
        {
            checkBox.Theme = theme;
        }

        checkBox.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
        checkBox.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        return checkBox;
    }

    protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
    {
        var checkBox = (CheckBox)base.GenerateEditingElementDirect(cell, dataItem);

        if (CellCheckBoxTheme is { } theme)
        {
            checkBox.Theme = theme;
        }

        return checkBox;
    }
}

public class ThemedComboBoxColumn : DataGridComboBoxColumn
{
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        var combo = (ComboBox)base.GenerateElement(cell, dataItem);

        if (CellComboBoxDisplayTheme is { } theme)
        {
            combo.Theme = theme;
        }

        combo.Padding = new Thickness(4, 0, 4, 0);
        combo.MaxDropDownHeight = 240;
        return combo;
    }

    protected override Control GenerateEditingElement(DataGridCell cell, object dataItem, out ICellEditBinding editBinding)
    {
        var combo = (ComboBox)base.GenerateEditingElement(cell, dataItem, out editBinding);

        if (CellComboBoxTheme is { } theme)
        {
            combo.Theme = theme;
        }

        combo.Padding = new Thickness(6, 2);
        combo.MaxDropDownHeight = 240;
        return combo;
    }
}

public class ThemedHyperlinkColumn : DataGridHyperlinkColumn
{
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        var hyperlink = (HyperlinkButton)base.GenerateElement(cell, dataItem);

        if (CellHyperlinkButtonTheme is { } theme)
        {
            hyperlink.Theme = theme;
        }

        hyperlink.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        return hyperlink;
    }

    protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
    {
        var textBox = (TextBox)base.GenerateEditingElementDirect(cell, dataItem);

        if (CellTextBoxTheme is { } theme)
        {
            textBox.Theme = theme;
        }

        textBox.TextAlignment = TextAlignment.Left;
        return textBox;
    }
}
