// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;

namespace DataGridSample.Adapters
{
    /// <summary>
    /// Factory that creates the accessor-only filtering adapter.
    /// </summary>
    public sealed class AccessorFilteringAdapterFactory : IDataGridFilteringAdapterFactory
    {
        public DataGridFilteringAdapter Create(DataGrid grid, IFilteringModel model)
        {
            return new AccessorFilteringAdapter(model, () => grid.ColumnDefinitions);
        }
    }
}
