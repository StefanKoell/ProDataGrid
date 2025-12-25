using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridHierarchical;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class TreeViewMimicFilterModelViewModel : ObservableObject
    {
        private const int RootCount = 20;
        private const int BranchesPerRoot = 20;
        private const int LeavesPerBranch = 50;
        private const string FilterPropertyPath = "Item.Name";

        private string _filterText = string.Empty;

        public TreeViewMimicFilterModelViewModel()
        {
            Roots = BuildSample();

            var leafCount = RootCount * BranchesPerRoot * LeavesPerBranch;
            var branchCount = RootCount * BranchesPerRoot;
            var totalCount = RootCount + branchCount + leafCount;
            Summary = $"Roots: {RootCount:n0}  Branches: {branchCount:n0}  Leaves: {leafCount:n0}  Total: {totalCount:n0}";

            var options = new HierarchicalOptions<TreeItem>
            {
                ChildrenSelector = item => item.Children,
                IsLeafSelector = item => item.Children.Count == 0,
                VirtualizeChildren = true
            };

            Model = new HierarchicalModel<TreeItem>(options);
            Model.SetRoots(Roots);

            FilteringModel = new FilteringModel();
            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);

            ExpandAllCommand = new RelayCommand(_ => Model.ExpandAll());
            CollapseAllCommand = new RelayCommand(_ => Model.CollapseAll());
        }

        public ObservableCollection<TreeItem> Roots { get; }

        public HierarchicalModel<TreeItem> Model { get; }

        public FilteringModel FilteringModel { get; }

        public string Summary { get; }

        public RelayCommand ExpandAllCommand { get; }

        public RelayCommand CollapseAllCommand { get; }

        public RelayCommand ClearFilterCommand { get; }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public sealed class TreeItem
        {
            public TreeItem(string name)
            {
                Name = name;
                Children = new ObservableCollection<TreeItem>();
            }

            public string Name { get; }

            public ObservableCollection<TreeItem> Children { get; }
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(_filterText))
            {
                FilteringModel.Remove(FilterPropertyPath);
                return;
            }

            var text = _filterText.Trim();
            var matches = BuildMatchSet(Roots, text);
            FilteringModel.SetOrUpdate(new FilteringDescriptor(
                columnId: FilterPropertyPath,
                @operator: FilteringOperator.Custom,
                propertyPath: FilterPropertyPath,
                predicate: item => MatchesFilter(item, matches)));
        }

        private static bool MatchesFilter(object item, HashSet<TreeItem> matches)
        {
            if (item is HierarchicalNode node)
            {
                if (node.Item is TreeItem treeItem)
                {
                    return matches.Contains(treeItem);
                }
            }
            else if (item is TreeItem treeItem)
            {
                return matches.Contains(treeItem);
            }

            return false;
        }

        private static HashSet<TreeItem> BuildMatchSet(ObservableCollection<TreeItem> roots, string text)
        {
            var matches = new HashSet<TreeItem>();

            foreach (var root in roots)
            {
                CollectMatches(root, text, matches);
            }

            return matches;
        }

        private static bool CollectMatches(TreeItem item, string text, HashSet<TreeItem> matches)
        {
            var isMatch = item.Name.Contains(text, StringComparison.OrdinalIgnoreCase);
            var childMatch = false;

            foreach (var child in item.Children)
            {
                if (CollectMatches(child, text, matches))
                {
                    childMatch = true;
                }
            }

            if (isMatch || childMatch)
            {
                matches.Add(item);
                return true;
            }

            return false;
        }

        private static ObservableCollection<TreeItem> BuildSample()
        {
            var roots = new ObservableCollection<TreeItem>();

            for (var groupIndex = 1; groupIndex <= RootCount; groupIndex++)
            {
                var group = new TreeItem($"Group {groupIndex:00}");

                for (var branchIndex = 1; branchIndex <= BranchesPerRoot; branchIndex++)
                {
                    var branch = new TreeItem($"Section {groupIndex:00}.{branchIndex:00}");

                    for (var itemIndex = 1; itemIndex <= LeavesPerBranch; itemIndex++)
                    {
                        branch.Children.Add(new TreeItem($"Item {groupIndex:00}.{branchIndex:00}.{itemIndex:000}"));
                    }

                    group.Children.Add(branch);
                }

                roots.Add(group);
            }

            return roots;
        }
    }
}
