using Avalonia.Controls.DataGridPivoting;
using Xunit;

namespace Avalonia.Controls.DataGridTests.Pivoting;

public class PivotResultModelsTests
{
    [Fact]
    public void PivotRow_Exposes_Level_And_Indent()
    {
        var row = new PivotRow(
            PivotRowType.Detail,
            2,
            new object?[] { "A" },
            new object?[] { "A" },
            "A",
            4d,
            1,
            null,
            null);

        Assert.Equal(2, row.Level);
        Assert.Equal(4d, row.Indent);
    }
}
