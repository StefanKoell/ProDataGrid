using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridHierarchical;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using DataGridSample.Helpers;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ColumnDefinitionsHierarchicalViewModel : ObservableObject
    {
        public class TreeItem
        {
            public TreeItem(int id, string name, string kind, int items, bool isExpanded)
            {
                Id = id;
                Name = name;
                Kind = kind;
                Items = items;
                IsExpanded = isExpanded;
                Children = new ObservableCollection<TreeItem>();
            }

            public int Id { get; }

            public string Name { get; }

            public string Kind { get; }

            public int Items { get; }

            public bool IsExpanded { get; set; }

            public ObservableCollection<TreeItem> Children { get; }

            public override string ToString()
            {
                return Name;
            }
        }

        private const string NamePropertyPath = "Item.Name";

        private readonly DataGridColumnDefinition _nameColumn;
        private readonly DataGridColumnDefinition _idColumn;
        private readonly DataGridColumnDefinition _kindColumn;
        private readonly DataGridColumnDefinition _itemsColumn;
        private readonly Dictionary<object, IComparer<TreeItem>> _sortComparers;
        private readonly IComparer<TreeItem> _defaultComparer;

        private int _nextId = 1;
        private string _filterText = string.Empty;
        private string _query = string.Empty;
        private string _resultSummary = "No results";

        public ColumnDefinitionsHierarchicalViewModel()
        {
            RootItems = new ObservableCollection<TreeItem>();

            _defaultComparer = Comparer<TreeItem>.Create((x, y) =>
                string.Compare(x?.Name, y?.Name, StringComparison.OrdinalIgnoreCase));

            var options = new HierarchicalOptions<TreeItem>
            {
                ChildrenSelector = item => item.Children,
                IsExpandedSelector = item => item.IsExpanded,
                IsExpandedSetter = (item, value) => item.IsExpanded = value,
                SiblingComparer = _defaultComparer
            };

            Model = new HierarchicalModel<TreeItem>(options);
            Model.SetRoots(RootItems);

            _nameColumn = new DataGridHierarchicalColumnDefinition
            {
                Header = "Item",
                Binding = CreateNodeBinding<TreeItem>("Item", item => item),
                CellTemplateKey = "ColumnDefinitionsHierarchyNameTemplate",
                SortMemberPath = NamePropertyPath,
                Width = new DataGridLength(2, DataGridLengthUnitType.Star)
            };
            _idColumn = new DataGridTextColumnDefinition
            {
                Header = "Id",
                Binding = CreateNodeBinding<int>("Id", item => item.Id),
                SortMemberPath = "Item.Id",
                Width = new DataGridLength(0.6, DataGridLengthUnitType.Star)
            };
            _kindColumn = new DataGridTextColumnDefinition
            {
                Header = "Kind",
                Binding = CreateNodeBinding<string>("Kind", item => item.Kind),
                SortMemberPath = "Item.Kind",
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };
            _itemsColumn = new DataGridNumericColumnDefinition
            {
                Header = "Items",
                Binding = CreateNodeBinding<int>("Items", item => item.Items),
                SortMemberPath = "Item.Items",
                Width = new DataGridLength(0.7, DataGridLengthUnitType.Star),
                FormatString = "N0"
            };

            ColumnDefinitions = new ObservableCollection<DataGridColumnDefinition>
            {
                _nameColumn,
                _idColumn,
                _kindColumn,
                _itemsColumn
            };

            _sortComparers = new Dictionary<object, IComparer<TreeItem>>
            {
                { _nameColumn, Comparer<TreeItem>.Create((x, y) => string.Compare(x?.Name, y?.Name, StringComparison.OrdinalIgnoreCase)) },
                { _idColumn, Comparer<TreeItem>.Create((x, y) => Nullable.Compare(x?.Id, y?.Id)) },
                { _kindColumn, Comparer<TreeItem>.Create((x, y) => string.Compare(x?.Kind, y?.Kind, StringComparison.OrdinalIgnoreCase)) },
                { _itemsColumn, Comparer<TreeItem>.Create((x, y) => Nullable.Compare(x?.Items, y?.Items)) }
            };

            SortingModel = new SortingModel
            {
                MultiSort = true,
                CycleMode = SortCycleMode.AscendingDescendingNone,
                OwnsViewSorts = true
            };
            SortingModel.SortingChanged += OnSortingChanged;

            FilteringModel = new FilteringModel();
            SearchModel = new SearchModel
            {
                HighlightMode = SearchHighlightMode.TextAndCell,
                HighlightCurrent = true,
                WrapNavigation = true,
                UpdateSelectionOnNavigate = true
            };

            ClearFilterCommand = new RelayCommand(_ => FilterText = string.Empty);
            ClearSearchCommand = new RelayCommand(_ => Query = string.Empty);
            ClearSortsCommand = new RelayCommand(_ => SortingModel.Clear());
            NextCommand = new RelayCommand(_ => SearchModel.MoveNext(), _ => SearchModel.Results.Count > 0);
            PreviousCommand = new RelayCommand(_ => SearchModel.MovePrevious(), _ => SearchModel.Results.Count > 0);
            ExpandAllCommand = new RelayCommand(_ => Model.ExpandAll());
            CollapseAllCommand = new RelayCommand(_ => Model.CollapseAll());

            SearchModel.ResultsChanged += (_, __) => UpdateResultSummary();
            SearchModel.CurrentChanged += (_, __) => UpdateResultSummary();
            UpdateResultSummary();

            SeedSample();
        }

        public ObservableCollection<TreeItem> RootItems { get; }

        public HierarchicalModel<TreeItem> Model { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        public SortingModel SortingModel { get; }

        public FilteringModel FilteringModel { get; }

        public SearchModel SearchModel { get; }

        public RelayCommand ClearFilterCommand { get; }

        public RelayCommand ClearSearchCommand { get; }

        public RelayCommand ClearSortsCommand { get; }

        public RelayCommand NextCommand { get; }

        public RelayCommand PreviousCommand { get; }

        public RelayCommand ExpandAllCommand { get; }

        public RelayCommand CollapseAllCommand { get; }

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
            private set => SetProperty(ref _resultSummary, value);
        }

        private static DataGridBindingDefinition CreateNodeBinding<TValue>(string name, Func<TreeItem, TValue> getter)
        {
            return ColumnDefinitionBindingFactory.CreateBinding<HierarchicalNode, TValue>(
                name,
                node => getter((TreeItem)node.Item));
        }

        private void SeedSample()
        {
            RootItems.Clear();

            var launch = CreateRoot("Nova Launch", "Program", 12);
            var design = CreateChild("Design", "Workstream", 4);
            design.Children.Add(CreateChild("Brand", "Task", 2));
            design.Children.Add(CreateChild("UX", "Task", 2));
            var build = CreateChild("Build", "Workstream", 6);
            build.Children.Add(CreateChild("API", "Task", 3));
            build.Children.Add(CreateChild("UI", "Task", 3));
            launch.Children.Add(design);
            launch.Children.Add(build);

            var foundation = CreateRoot("Foundation", "Program", 8);
            foundation.Children.Add(CreateChild("Security", "Workstream", 3));
            foundation.Children.Add(CreateChild("Observability", "Workstream", 2));
            foundation.Children.Add(CreateChild("Docs", "Workstream", 1));

            RootItems.Add(launch);
            RootItems.Add(foundation);
        }

        private TreeItem CreateRoot(string name, string kind, int items)
        {
            return new TreeItem(_nextId++, name, kind, items, isExpanded: true);
        }

        private TreeItem CreateChild(string name, string kind, int items)
        {
            return new TreeItem(_nextId++, name, kind, items, isExpanded: false);
        }

        private void OnSortingChanged(object? sender, SortingChangedEventArgs e)
        {
            ApplySortDescriptors(e.NewDescriptors);
        }

        private void ApplySortDescriptors(IReadOnlyList<SortingDescriptor> descriptors)
        {
            if (descriptors == null || descriptors.Count == 0)
            {
                Model.ApplySiblingComparer(_defaultComparer, recursive: true);
                return;
            }

            var comparers = new List<(IComparer<TreeItem> Comparer, ListSortDirection Direction)>();
            foreach (var descriptor in descriptors)
            {
                if (descriptor == null)
                {
                    continue;
                }

                if (!_sortComparers.TryGetValue(descriptor.ColumnId, out var comparer))
                {
                    continue;
                }

                comparers.Add((comparer, descriptor.Direction));
            }

            if (comparers.Count == 0)
            {
                Model.ApplySiblingComparer(_defaultComparer, recursive: true);
                return;
            }

            var composite = Comparer<TreeItem>.Create((x, y) =>
            {
                foreach (var entry in comparers)
                {
                    var result = entry.Comparer.Compare(x, y);
                    if (result != 0)
                    {
                        return entry.Direction == ListSortDirection.Descending ? -result : result;
                    }
                }

                return 0;
            });

            Model.ApplySiblingComparer(composite, recursive: true);
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(_filterText))
            {
                FilteringModel.Remove(_nameColumn);
                return;
            }

            var text = _filterText.Trim();
            var matches = BuildMatchSet(RootItems, text);
            FilteringModel.SetOrUpdate(new FilteringDescriptor(
                columnId: _nameColumn,
                @operator: FilteringOperator.Custom,
                propertyPath: NamePropertyPath,
                predicate: item => MatchesFilter(item, matches)));
        }

        private void ApplySearch()
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

        private static bool MatchesFilter(object item, HashSet<TreeItem> matches)
        {
            if (item is HierarchicalNode node && node.Item is TreeItem treeItem)
            {
                return matches.Contains(treeItem);
            }

            if (item is TreeItem typed)
            {
                return matches.Contains(typed);
            }

            return false;
        }

        private static HashSet<TreeItem> BuildMatchSet(IEnumerable<TreeItem> roots, string text)
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
            var isMatch = ItemMatches(item, text);
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

        private static bool ItemMatches(TreeItem item, string text)
        {
            return item.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                || item.Kind.Contains(text, StringComparison.OrdinalIgnoreCase);
        }
    }
}
