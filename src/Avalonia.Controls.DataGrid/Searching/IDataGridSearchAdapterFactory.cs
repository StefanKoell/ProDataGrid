// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using Avalonia.Controls;

namespace Avalonia.Controls.DataGridSearching
{
    /// <summary>
    /// Factory for creating a <see cref="DataGridSearchAdapter"/> without subclassing <see cref="DataGrid"/>.
    /// </summary>
    public interface IDataGridSearchAdapterFactory
    {
        /// <summary>
        /// Creates a new search adapter for the grid.
        /// </summary>
        /// <param name="grid">Target grid.</param>
        /// <param name="model">Search model to bridge.</param>
        DataGridSearchAdapter Create(DataGrid grid, ISearchModel model);
    }
}
