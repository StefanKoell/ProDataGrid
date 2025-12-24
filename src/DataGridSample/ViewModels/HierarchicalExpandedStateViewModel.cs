// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls.DataGridHierarchical;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class HierarchicalExpandedStateViewModel : ObservableObject
    {
        public class TreeItem : ObservableObject
        {
            public TreeItem(string name)
            {
                Name = name;
                Children = new ObservableCollection<TreeItem>();
            }

            public string Name { get; }

            public ObservableCollection<TreeItem> Children { get; }

            private bool _isExpanded;

            public bool IsExpanded
            {
                get => _isExpanded;
                set => SetProperty(ref _isExpanded, value);
            }
        }

        private readonly TreeItem _root;

        public HierarchicalExpandedStateViewModel()
        {
            _root = BuildSample();

            var options = new HierarchicalOptions<TreeItem>
            {
                ItemsSelector = item => item.Children,
                IsExpandedSelector = item => item.IsExpanded,
                IsExpandedSetter = (item, value) => item.IsExpanded = value
            };

            Model = new HierarchicalModel<TreeItem>(options);
            Model.SetRoot(_root);

            ExpandAllCommand = new RelayCommand(_ => Model.ExpandAll());
            CollapseAllCommand = new RelayCommand(_ => Model.CollapseAll());
            ToggleRootCommand = new RelayCommand(_ => _root.IsExpanded = !_root.IsExpanded);
            ToggleFirstChildCommand = new RelayCommand(_ =>
            {
                var first = _root.Children.FirstOrDefault();
                if (first != null)
                {
                    first.IsExpanded = !first.IsExpanded;
                }
            });
        }

        public HierarchicalModel<TreeItem> Model { get; }

        public RelayCommand ExpandAllCommand { get; }

        public RelayCommand CollapseAllCommand { get; }

        public RelayCommand ToggleRootCommand { get; }

        public RelayCommand ToggleFirstChildCommand { get; }

        private static TreeItem BuildSample()
        {
            var root = new TreeItem("Root") { IsExpanded = true };

            var alpha = new TreeItem("Alpha");
            alpha.Children.Add(new TreeItem("Alpha-1"));
            alpha.Children.Add(new TreeItem("Alpha-2"));

            var beta = new TreeItem("Beta") { IsExpanded = true };
            var betaChild = new TreeItem("Beta-1") { IsExpanded = true };
            betaChild.Children.Add(new TreeItem("Beta-1-a"));
            beta.Children.Add(betaChild);

            root.Children.Add(alpha);
            root.Children.Add(beta);

            return root;
        }
    }
}
