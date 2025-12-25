using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Controls.DataGridSorting;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class TreeViewMimicSortModelViewModel : ObservableObject
    {
        private const int RootCount = 20;
        private const int BranchesPerRoot = 20;
        private const int LeavesPerBranch = 50;
        private const string SortPropertyPath = "Item.Name";

        private readonly IComparer<TreeItem> _defaultComparer = Comparer<TreeItem>.Create((x, y) =>
            string.Compare(x?.Name, y?.Name, StringComparison.OrdinalIgnoreCase));

        public TreeViewMimicSortModelViewModel()
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
                VirtualizeChildren = true,
                SiblingComparer = _defaultComparer
            };

            Model = new HierarchicalModel<TreeItem>(options);
            Model.SetRoots(Roots);

            SortingModel = new SortingModel();
            SortingModel.SortingChanged += OnSortingChanged;

            SortAscendingCommand = new RelayCommand(_ => ApplySort(ListSortDirection.Ascending));
            SortDescendingCommand = new RelayCommand(_ => ApplySort(ListSortDirection.Descending));
            ClearSortCommand = new RelayCommand(_ => ClearSort());

            ExpandAllCommand = new RelayCommand(_ => Model.ExpandAll());
            CollapseAllCommand = new RelayCommand(_ => Model.CollapseAll());
        }

        public ObservableCollection<TreeItem> Roots { get; }

        public HierarchicalModel<TreeItem> Model { get; }

        public SortingModel SortingModel { get; }

        public string Summary { get; }

        public RelayCommand SortAscendingCommand { get; }

        public RelayCommand SortDescendingCommand { get; }

        public RelayCommand ClearSortCommand { get; }

        public RelayCommand ExpandAllCommand { get; }

        public RelayCommand CollapseAllCommand { get; }

        private bool _suppressSortingApply;

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

        private void ApplySort(ListSortDirection direction)
        {
            var descriptor = new SortingDescriptor(
                columnId: SortPropertyPath,
                direction: direction,
                propertyPath: SortPropertyPath);

            ApplySortDescriptors(new[] { descriptor });

            _suppressSortingApply = true;
            try
            {
                SortingModel.SetOrUpdate(descriptor);
            }
            finally
            {
                _suppressSortingApply = false;
            }
        }

        private void ClearSort()
        {
            ApplySortDescriptors(Array.Empty<SortingDescriptor>());

            _suppressSortingApply = true;
            try
            {
                SortingModel.Clear();
            }
            finally
            {
                _suppressSortingApply = false;
            }
        }

        private void OnSortingChanged(object? sender, SortingChangedEventArgs e)
        {
            if (_suppressSortingApply)
            {
                return;
            }

            ApplySortDescriptors(e.NewDescriptors);
        }

        private void ApplySortDescriptors(IReadOnlyList<SortingDescriptor> descriptors)
        {
            if (descriptors == null || descriptors.Count == 0)
            {
                Model.ApplySiblingComparer(_defaultComparer, recursive: true);
                return;
            }

            var comparer = BuildComparer(descriptors);
            Model.ApplySiblingComparer(comparer, recursive: true);
        }

        private static IComparer<TreeItem> BuildComparer(IReadOnlyList<SortingDescriptor> descriptors)
        {
            return Comparer<TreeItem>.Create((x, y) =>
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                var left = x?.Name;
                var right = y?.Name;

                foreach (var descriptor in descriptors)
                {
                    var result = string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
                    if (result != 0)
                    {
                        return descriptor.Direction == ListSortDirection.Descending ? -result : result;
                    }
                }

                return 0;
            });
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
