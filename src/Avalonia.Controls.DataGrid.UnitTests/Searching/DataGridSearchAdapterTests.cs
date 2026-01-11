using System;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Headless.XUnit;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Searching;

public class DataGridSearchAdapterTests
{
    [AvaloniaFact]
    public void ValueAccessor_Is_Used_When_No_Path()
    {
        var items = new[]
        {
            new Person("Alpha"),
            new Person("Beta")
        };
        var view = new DataGridCollectionView(items);
        var model = new SearchModel();

        var column = new DataGridTextColumn();
        DataGridColumnMetadata.SetValueAccessor(column, new DataGridColumnValueAccessor<Person, string>(p => p.Name));

        var adapter = new DataGridSearchAdapter(model, () => new[] { column });
        adapter.AttachView(view);

        model.SetOrUpdate(new SearchDescriptor("Beta", comparison: StringComparison.OrdinalIgnoreCase));

        var result = Assert.Single(model.Results);
        Assert.Same(items[1], result.Item);
        Assert.Same(column, result.ColumnId);
    }

    [AvaloniaFact]
    public void Column_Definition_Id_Is_Selected()
    {
        var items = new[]
        {
            new Person("Alpha"),
            new Person("Beta")
        };
        var view = new DataGridCollectionView(items);
        var model = new SearchModel();

        var definition = new DataGridTextColumnDefinition
        {
            Header = "Name",
            Binding = DataGridBindingDefinition.Create<Person, string>(p => p.Name)
        };

        var grid = new DataGrid
        {
            ColumnDefinitionsSource = new[] { definition }
        };

        var adapter = new DataGridSearchAdapter(model, () => grid.Columns);
        adapter.AttachView(view);

        model.SetOrUpdate(new SearchDescriptor(
            "Beta",
            scope: SearchScope.ExplicitColumns,
            columnIds: new object[] { definition },
            comparison: StringComparison.OrdinalIgnoreCase));

        var result = Assert.Single(model.Results);
        Assert.Same(items[1], result.Item);
    }

    private sealed class Person
    {
        public Person(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
