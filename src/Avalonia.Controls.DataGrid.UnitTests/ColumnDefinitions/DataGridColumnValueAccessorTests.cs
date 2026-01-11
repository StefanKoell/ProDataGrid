using System.Globalization;
using Avalonia.Controls;
using Xunit;

namespace Avalonia.Controls.DataGridTests.ColumnDefinitions;

public class DataGridColumnValueAccessorTests
{
    [Fact]
    public void TypedAccessor_Gets_And_Sets_Value()
    {
        IDataGridColumnValueAccessor<Person, int> accessor =
            new DataGridColumnValueAccessor<Person, int>(p => p.Age, (p, v) => p.Age = v);

        var person = new Person { Age = 10 };

        Assert.Equal(10, accessor.GetValue(person));

        accessor.SetValue(person, 42);

        Assert.Equal(42, person.Age);
    }

    [Fact]
    public void TypedComparer_Orders_ValueTypes()
    {
        var accessor = new DataGridColumnValueAccessor<Person, int>(p => p.Age);
        var comparer = new DataGridColumnValueAccessorComparer<Person, int>(accessor, CultureInfo.InvariantCulture);

        var left = new Person { Age = 1 };
        var right = new Person { Age = 2 };

        Assert.True(comparer.Compare(left, right) < 0);
        Assert.True(comparer.Compare(right, left) > 0);
    }

    [Fact]
    public void Comparer_Create_Caches_By_Accessor_And_Culture()
    {
        var accessor = new DataGridColumnValueAccessor<Person, int>(p => p.Age);

        var comparer = DataGridColumnValueAccessorComparer.Create(accessor, CultureInfo.InvariantCulture);
        var cached = DataGridColumnValueAccessorComparer.Create(accessor, CultureInfo.InvariantCulture);
        var different = DataGridColumnValueAccessorComparer.Create(accessor, CultureInfo.GetCultureInfo("tr-TR"));

        Assert.Same(comparer, cached);
        Assert.NotSame(comparer, different);
    }

    private sealed class Person
    {
        public int Age { get; set; }
    }
}
