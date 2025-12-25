using System;
using System.Collections.ObjectModel;
using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Controls.DataGridSearching;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class TreeViewMimicSearchModelViewModel : ObservableObject
    {
        private const int RootCount = 20;
        private const int BranchesPerRoot = 20;
        private const int LeavesPerBranch = 50;

        private string _query = string.Empty;
        private string _resultSummary = "No results";

        public TreeViewMimicSearchModelViewModel()
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

            SearchModel = new SearchModel
            {
                HighlightMode = SearchHighlightMode.TextAndCell,
                HighlightCurrent = true,
                WrapNavigation = true,
                UpdateSelectionOnNavigate = true
            };

            ExpandAllCommand = new RelayCommand(_ => Model.ExpandAll());
            CollapseAllCommand = new RelayCommand(_ => Model.CollapseAll());
            ClearCommand = new RelayCommand(_ => Query = string.Empty);
            NextCommand = new RelayCommand(_ => SearchModel.MoveNext(), _ => SearchModel.Results.Count > 0);
            PreviousCommand = new RelayCommand(_ => SearchModel.MovePrevious(), _ => SearchModel.Results.Count > 0);

            SearchModel.ResultsChanged += (_, __) => UpdateResultSummary();
            SearchModel.CurrentChanged += (_, __) => UpdateResultSummary();
            UpdateResultSummary();

            Model.FlattenedChangedTyped += (_, __) => ApplySearch(forceRefresh: true);
        }

        public ObservableCollection<TreeItem> Roots { get; }

        public HierarchicalModel<TreeItem> Model { get; }

        public SearchModel SearchModel { get; }

        public string Summary { get; }

        public RelayCommand ExpandAllCommand { get; }

        public RelayCommand CollapseAllCommand { get; }

        public RelayCommand ClearCommand { get; }

        public RelayCommand NextCommand { get; }

        public RelayCommand PreviousCommand { get; }

        public string Query
        {
            get => _query;
            set
            {
                if (SetProperty(ref _query, value))
                {
                    ApplySearch();
                }
            }
        }

        public string ResultSummary
        {
            get => _resultSummary;
            set => SetProperty(ref _resultSummary, value);
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

        private void ApplySearch(bool forceRefresh = false)
        {
            if (string.IsNullOrWhiteSpace(_query))
            {
                SearchModel.Clear();
                UpdateResultSummary();
                return;
            }

            var descriptor = new SearchDescriptor(
                _query.Trim(),
                matchMode: SearchMatchMode.Contains,
                termMode: SearchTermCombineMode.Any,
                scope: SearchScope.VisibleColumns,
                comparison: StringComparison.OrdinalIgnoreCase);

            if (forceRefresh)
            {
                SearchModel.Clear();
            }

            SearchModel.SetOrUpdate(descriptor);
        }

        private void UpdateResultSummary()
        {
            var count = SearchModel.Results.Count;
            var current = SearchModel.CurrentIndex >= 0 ? SearchModel.CurrentIndex + 1 : 0;

            if (count == 0)
            {
                ResultSummary = "No results";
            }
            else if (current == 0)
            {
                ResultSummary = $"{count:n0} results";
            }
            else
            {
                ResultSummary = $"{current:n0} of {count:n0}";
            }

            NextCommand.RaiseCanExecuteChanged();
            PreviousCommand.RaiseCanExecuteChanged();
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
