// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Xunit;

namespace Avalonia.Controls.DataGridTests.ColumnDefinitions;

public class DataGridColumnDefinitionTests
{
    [Fact]
    public void Bound_Definition_Populates_ValueAccessor_And_Type_From_Binding()
    {
        var definition = new DataGridTextColumnDefinition
        {
            Binding = DataGridBindingDefinition.Create<Person, int>(p => p.Age)
        };

        Assert.NotNull(definition.ValueAccessor);
        Assert.Equal(typeof(int), definition.ValueType);
    }

    private sealed class Person
    {
        public int Age { get; set; }
    }
}
