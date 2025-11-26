// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

#nullable disable

using Avalonia.Interactivity;
using System;

namespace Avalonia.Controls
{
    /// <summary>
    /// Event declarations
    /// </summary>
#if !DATAGRID_INTERNAL
    public
#endif
    partial class DataGrid
    {
        /// <summary>
        /// Occurs one time for each public, non-static property in the bound data type when the
        /// <see cref="P:Avalonia.Controls.DataGrid.ItemsSource" /> property is changed and the
        /// <see cref="P:Avalonia.Controls.DataGrid.AutoGenerateColumns" /> property is true.
        /// </summary>
        public event EventHandler<DataGridAutoGeneratingColumnEventArgs> AutoGeneratingColumn;

        /// <summary>
        /// Occurs when the <see cref="P:Avalonia.Controls.DataGridColumn.DisplayIndex" />
        /// property of a column changes.
        /// </summary>
        public event EventHandler<DataGridColumnEventArgs> ColumnDisplayIndexChanged;

        /// <summary>
        /// Raised when column reordering ends, to allow subscribers to clean up.
        /// </summary>
        public event EventHandler<DataGridColumnEventArgs> ColumnReordered;

        /// <summary>
        /// Raised when starting a column reordering action.  Subscribers to this event can
        /// set tooltip and caret UIElements, constrain tooltip position, indicate that
        /// a preview should be shown, or cancel reordering.
        /// </summary>
        public event EventHandler<DataGridColumnReorderingEventArgs> ColumnReordering;

        /// <summary>
        /// Occurs after a <see cref="T:Avalonia.Controls.DataGridRow" />
        /// is instantiated, so that you can customize it before it is used.
        /// </summary>
        public event EventHandler<DataGridRowEventArgs> LoadingRow;

        /// <summary>
        /// Identifies the <see cref="SelectionChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
            RoutedEvent.Register<DataGrid, SelectionChangedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);

        /// <summary>
        /// Occurs when the <see cref="DataGridColumn"/> sorting request is triggered.
        /// </summary>
        public event EventHandler<DataGridColumnEventArgs> Sorting;

        /// <summary>
        /// Occurs when a <see cref="T:Avalonia.Controls.DataGridRow" />
        /// object becomes available for reuse.
        /// </summary>
        public event EventHandler<DataGridRowEventArgs> UnloadingRow;

        /// <summary>
        /// Raises the AutoGeneratingColumn event.
        /// </summary>
        protected virtual void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
        {
            AutoGeneratingColumn?.Invoke(this, e);
        }
    }
}
