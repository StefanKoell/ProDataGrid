using Avalonia.Controls;
using Avalonia.Controls.DataGridSearching;

namespace DataGridSample.Adapters
{
    public sealed class HierarchicalSearchAdapterFactory : IDataGridSearchAdapterFactory
    {
        public DataGridSearchAdapter Create(DataGrid grid, ISearchModel model)
        {
            return new AccessorSearchAdapter(model, () => grid.ColumnDefinitions);
        }
    }
}
