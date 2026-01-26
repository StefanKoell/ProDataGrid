using System;
using System.Collections.Specialized;
using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotObservableCollectionTests
{
    [Fact]
    public void ResetWith_Replaces_Items_And_Raises_Reset()
    {
        var collection = new PivotObservableCollection<int> { 1 };
        NotifyCollectionChangedEventArgs? args = null;
        collection.CollectionChanged += (_, e) => args = e;

        collection.ResetWith(new[] { 2, 3 });

        Assert.Equal(2, collection.Count);
        Assert.Contains(2, collection);
        Assert.Contains(3, collection);
        Assert.NotNull(args);
        Assert.Equal(NotifyCollectionChangedAction.Reset, args!.Action);
    }

    [Fact]
    public void ResetWith_Throws_On_Null_Items()
    {
        var collection = new PivotObservableCollection<int>();

        Assert.Throws<ArgumentNullException>(() => collection.ResetWith(null!));
    }
}
