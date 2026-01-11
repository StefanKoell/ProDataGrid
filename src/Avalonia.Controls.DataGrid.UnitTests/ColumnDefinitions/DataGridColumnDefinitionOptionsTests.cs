using System;
using System.Collections;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.ColumnDefinitions;

public class DataGridColumnDefinitionOptionsTests
{
    [Fact]
    public void Options_Are_Applied_To_Materialized_Columns()
    {
        var filterAccessor = new DataGridColumnValueAccessor<Person, int>(p => p.Score);
        var sortAccessor = new DataGridColumnValueAccessor<Person, int>(p => p.Score);
        var filterFactory = new Func<FilteringDescriptor, Func<object, bool>>(_ => _ => true);
        var textProvider = new Func<object, string>(item => ((Person)item).Name);
        var formatProvider = CultureInfo.InvariantCulture;

        var options = new DataGridColumnDefinitionOptions
        {
            IsSearchable = false,
            SearchMemberPath = nameof(Person.Name),
            SearchTextProvider = textProvider,
            SearchFormatProvider = formatProvider,
            FilterPredicateFactory = filterFactory,
            FilterValueAccessor = filterAccessor,
            SortValueAccessor = sortAccessor,
            SortValueComparer = Comparer.Default
        };

        var definition = new DataGridTextColumnDefinition
        {
            Header = "Name",
            Options = options
        };

        var column = definition.CreateColumn(new DataGridColumnDefinitionContext(new DataGrid()));

        Assert.False(DataGridColumnSearch.GetIsSearchable(column));
        Assert.Equal(nameof(Person.Name), DataGridColumnSearch.GetSearchMemberPath(column));
        Assert.Same(textProvider, DataGridColumnSearch.GetTextProvider(column));
        Assert.Same(formatProvider, DataGridColumnSearch.GetFormatProvider(column));
        Assert.Same(filterFactory, DataGridColumnFilter.GetPredicateFactory(column));
        Assert.Same(filterAccessor, DataGridColumnFilter.GetValueAccessor(column));
        Assert.Same(sortAccessor, DataGridColumnSort.GetValueAccessor(column));
        Assert.Same(Comparer.Default, DataGridColumnSort.GetValueComparer(column));
    }

    [Fact]
    public void Typed_Compare_Options_Are_Applied_To_Materialized_Columns()
    {
        var options = new DataGridColumnDefinitionOptions<Person>
        {
            CompareAscending = (left, right) => left.Score.CompareTo(right.Score),
            CompareDescending = (left, right) => right.Score.CompareTo(left.Score)
        };

        var definition = new DataGridTextColumnDefinition
        {
            Header = "Score",
            Options = options
        };

        var column = definition.CreateColumn(new DataGridColumnDefinitionContext(new DataGrid()));

        var ascendingComparer = DataGridColumnSort.GetAscendingComparer(column);
        var descendingComparer = DataGridColumnSort.GetDescendingComparer(column);

        Assert.NotNull(ascendingComparer);
        Assert.NotNull(descendingComparer);

        var low = new Person("Low", 1);
        var high = new Person("High", 3);

        Assert.True(ascendingComparer.Compare(low, high) < 0);
        Assert.True(descendingComparer.Compare(low, high) > 0);
    }

    private sealed class Person
    {
        public Person(string name, int score)
        {
            Name = name;
            Score = score;
        }

        public string Name { get; }

        public int Score { get; }
    }
}
