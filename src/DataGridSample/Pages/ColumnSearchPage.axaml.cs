// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace DataGridSample.Pages
{
    public partial class ColumnSearchPage : UserControl
    {
        public ColumnSearchPage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
