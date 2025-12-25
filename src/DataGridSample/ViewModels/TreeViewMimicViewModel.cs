using System.Collections.ObjectModel;
using Avalonia.Controls.DataGridHierarchical;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class TreeViewMimicViewModel
    {
        private const int RootCount = 20;
        private const int BranchesPerRoot = 20;
        private const int LeavesPerBranch = 50;

        public TreeViewMimicViewModel()
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

            ExpandAllCommand = new RelayCommand(_ => Model.ExpandAll());
            CollapseAllCommand = new RelayCommand(_ => Model.CollapseAll());
        }

        public ObservableCollection<TreeItem> Roots { get; }

        public HierarchicalModel<TreeItem> Model { get; }

        public string Summary { get; }

        public RelayCommand ExpandAllCommand { get; }

        public RelayCommand CollapseAllCommand { get; }

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
