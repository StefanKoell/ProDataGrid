// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Columns;

public class DataGridCheckBoxColumnThemeTests
{
    [AvaloniaFact]
    public void CheckBoxColumn_Reuses_Cell_Theme()
    {
        var checkBoxTheme = new ControlTheme(typeof(CheckBox));

        var grid = new DataGrid();
        grid.Resources.Add("DataGridCellCheckBoxTheme", checkBoxTheme);

        var column = new DerivedCheckBoxColumn
        {
            Binding = new Binding(nameof(ToggleState.IsDone))
        };

        grid.ColumnsInternal.Add(column);

        var displayElement = column.CreateDisplayElement(new DataGridCell(), new ToggleState());
        var editingElement = column.CreateEditingElement(new DataGridCell(), new ToggleState());
        var theme = column.GetTheme();

        Assert.Same(checkBoxTheme, displayElement.Theme);
        Assert.Same(checkBoxTheme, editingElement.Theme);
        Assert.Same(checkBoxTheme, theme);
    }

    [AvaloniaFact]
    public void CheckBox_Theme_Not_Locked_When_Accessed_Before_Attach()
    {
        var checkBoxTheme = new ControlTheme(typeof(CheckBox));

        var grid = new DataGrid();
        grid.Resources.Add("DataGridCellCheckBoxTheme", checkBoxTheme);

        var column = new DerivedCheckBoxColumn
        {
            Binding = new Binding(nameof(ToggleState.IsDone))
        };

        // Touch theme before adding to grid.
        Assert.Null(column.GetTheme());

        grid.ColumnsInternal.Add(column);

        Assert.Same(checkBoxTheme, column.GetTheme());
    }

    private class DerivedCheckBoxColumn : DataGridCheckBoxColumn
    {
        public CheckBox CreateDisplayElement(DataGridCell cell, object dataItem) =>
            (CheckBox)base.GenerateElement(cell, dataItem);

        public CheckBox CreateEditingElement(DataGridCell cell, object dataItem) =>
            (CheckBox)GenerateEditingElementDirect(cell, dataItem);

        public ControlTheme GetTheme() => CellCheckBoxTheme;
    }

    private class ToggleState
    {
        public bool IsDone { get; set; }
    }
}
