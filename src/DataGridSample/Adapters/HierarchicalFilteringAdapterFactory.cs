using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;

namespace DataGridSample.Adapters
{
    public sealed class HierarchicalFilteringAdapterFactory : IDataGridFilteringAdapterFactory
    {
        public DataGridFilteringAdapter Create(DataGrid grid, IFilteringModel model)
        {
            return new AccessorFilteringAdapter(model, () => grid.ColumnDefinitions);
        }
    }
}
