using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using DataGridSample.Helpers;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ColumnDefinitionsOptionsDiagnosticsViewModel : ObservableObject
    {
        private const string FirstNameKey = "first-name";
        private const string LastNameKey = "last-name";
        private const string FullNameKey = "full-name";
        private const string AgeGroupKey = "age-group";
        private const string StatusKey = "status";
        private const string BadgeKey = "badge";

        private readonly DataGridColumnValueAccessor<Person, int> _ageAccessor;
        private readonly DataGridColumnValueAccessor<Person, string> _ageGroupAccessor;
        private readonly DataGridColumnValueAccessor<Person, string> _fullNameSortAccessor;
        private readonly DataGridColumnDefinition _badgeColumn;
        private readonly DataGridColumnDefinitionOptions _badgeOptions;
        private int _resultCount;
        private int _currentResultIndex;
        private string _query = string.Empty;
        private bool _includeBadgeInSearch = true;

        public ColumnDefinitionsOptionsDiagnosticsViewModel()
        {
            Items = new ObservableCollection<Person>(CreatePeople());
            View = new DataGridCollectionView(Items)
            {
                Culture = CultureInfo.InvariantCulture
            };

            FilteringModel = new FilteringModel();
            SearchModel = new SearchModel
            {
                HighlightMode = SearchHighlightMode.TextAndCell,
                HighlightCurrent = true,
                WrapNavigation = true
            };
            SortingModel = new SortingModel
            {
                MultiSort = true,
                CycleMode = SortCycleMode.AscendingDescendingNone,
                OwnsViewSorts = true
            };

            SearchModel.ResultsChanged += SearchModelOnResultsChanged;
            SearchModel.CurrentChanged += SearchModelOnCurrentChanged;

            _ageAccessor = new DataGridColumnValueAccessor<Person, int>(p => p.Age, (p, v) => p.Age = v);
            _ageGroupAccessor = new DataGridColumnValueAccessor<Person, string>(p => GetAgeGroup(p.Age));
            _fullNameSortAccessor = new DataGridColumnValueAccessor<Person, string>(p => $"{p.LastName}, {p.FirstName}");

            _badgeOptions = new DataGridColumnDefinitionOptions
            {
                IsSearchable = true
            };

            ColumnDefinitions = new ObservableCollection<DataGridColumnDefinition>
            {
                new DataGridTextColumnDefinition
                {
                    Header = "First Name",
                    ColumnKey = FirstNameKey,
                    Binding = ColumnDefinitionBindingFactory.CreateBinding<Person, string>(
                        nameof(Person.FirstName),
                        p => p.FirstName,
                        (p, v) => p.FirstName = v),
                    Width = new DataGridLength(1.1, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumnDefinition
                {
                    Header = "Last Name",
                    ColumnKey = LastNameKey,
                    Binding = ColumnDefinitionBindingFactory.CreateBinding<Person, string>(
                        nameof(Person.LastName),
                        p => p.LastName,
                        (p, v) => p.LastName = v),
                    Width = new DataGridLength(1.1, DataGridLengthUnitType.Star)
                },
                new DataGridTemplateColumnDefinition
                {
                    Header = "Full Name",
                    ColumnKey = FullNameKey,
                    CellTemplateKey = "FullNameTemplate",
                    SortMemberPath = "FullName",
                    IsReadOnly = true,
                    Options = new DataGridColumnDefinitionOptions
                    {
                        SearchTextProvider = item => item is Person p ? $"{p.FirstName} {p.LastName}" : string.Empty,
                        SortValueAccessor = _fullNameSortAccessor
                    },
                    Width = new DataGridLength(1.4, DataGridLengthUnitType.Star)
                },
                new DataGridTemplateColumnDefinition
                {
                    Header = "Age Group",
                    ColumnKey = AgeGroupKey,
                    CellTemplateKey = "AgeGroupTemplate",
                    SortMemberPath = "AgeGroup",
                    IsReadOnly = true,
                    ValueAccessor = _ageGroupAccessor,
                    Options = new DataGridColumnDefinitionOptions
                    {
                        FilterValueAccessor = _ageAccessor,
                        SortValueAccessor = _ageAccessor
                    },
                    Width = new DataGridLength(0.9, DataGridLengthUnitType.Star)
                },
                new DataGridComboBoxColumnDefinition
                {
                    Header = "Status",
                    ColumnKey = StatusKey,
                    ItemsSource = Enum.GetValues<PersonStatus>(),
                    SelectedItemBinding = ColumnDefinitionBindingFactory.CreateBinding<Person, PersonStatus>(
                        nameof(Person.Status),
                        p => p.Status,
                        (p, v) => p.Status = v),
                    Options = new DataGridColumnDefinitionOptions
                    {
                        SortValueComparer = CreateStatusComparer()
                    },
                    Width = new DataGridLength(1.1, DataGridLengthUnitType.Star)
                }
            };

            _badgeColumn = new DataGridTemplateColumnDefinition
            {
                Header = "Badge",
                ColumnKey = BadgeKey,
                CellTemplateKey = "StatusBadgeTemplate",
                IsReadOnly = true,
                Options = _badgeOptions,
                Width = new DataGridLength(0.9, DataGridLengthUnitType.Star)
            };
            ColumnDefinitions.Add(_badgeColumn);

            FastPathOptions = new DataGridFastPathOptions
            {
                UseAccessorsOnly = true,
                ThrowOnMissingAccessor = false
            };
            FastPathOptions.MissingAccessor += OnMissingAccessor;

            Diagnostics = new ObservableCollection<FastPathDiagnostic>();

            NameFilter = new TextFilterContext(
                "First name contains",
                apply: text => ApplyTextFilter(FirstNameKey, text),
                clear: () => ClearFilter(FirstNameKey, () => NameFilter.Text = string.Empty));

            AgeFilter = new NumberFilterContext(
                "Age between",
                apply: (min, max) => ApplyNumberFilter(AgeGroupKey, min, max),
                clear: () => ClearFilter(AgeGroupKey, () =>
                {
                    AgeFilter.MinValue = null;
                    AgeFilter.MaxValue = null;
                }))
            {
                Minimum = 0,
                Maximum = 120
            };

            StatusFilter = new EnumFilterContext(
                "Status (In)",
                Enum.GetNames<PersonStatus>(),
                apply: selected => ApplyStatusFilter(StatusKey, selected),
                clear: () => ClearFilter(StatusKey, () => StatusFilter.SelectNone()));

            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            ClearSearchCommand = new RelayCommand(_ => Query = string.Empty);
            NextCommand = new RelayCommand(_ => SearchModel.MoveNext(), _ => SearchModel.Results.Count > 0);
            PreviousCommand = new RelayCommand(_ => SearchModel.MovePrevious(), _ => SearchModel.Results.Count > 0);
            ClearDiagnosticsCommand = new RelayCommand(_ => ClearDiagnostics(), _ => Diagnostics.Count > 0);
            Diagnostics.CollectionChanged += (_, __) => ClearDiagnosticsCommand.RaiseCanExecuteChanged();
        }

        public ObservableCollection<Person> Items { get; }

        public DataGridCollectionView View { get; }

        public FilteringModel FilteringModel { get; }

        public SearchModel SearchModel { get; }

        public SortingModel SortingModel { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        public DataGridFastPathOptions FastPathOptions { get; }

        public ObservableCollection<FastPathDiagnostic> Diagnostics { get; }

        public TextFilterContext NameFilter { get; }

        public NumberFilterContext AgeFilter { get; }

        public EnumFilterContext StatusFilter { get; }

        public RelayCommand ClearFiltersCommand { get; }

        public RelayCommand ClearSearchCommand { get; }

        public RelayCommand NextCommand { get; }

        public RelayCommand PreviousCommand { get; }

        public RelayCommand ClearDiagnosticsCommand { get; }

        public bool IncludeBadgeInSearch
        {
            get => _includeBadgeInSearch;
            set
            {
                if (SetProperty(ref _includeBadgeInSearch, value))
                {
                    _badgeOptions.IsSearchable = value;
                    ApplySearch();
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

        public int ResultCount
        {
            get => _resultCount;
            private set
            {
                if (SetProperty(ref _resultCount, value))
                {
                    OnPropertyChanged(nameof(ResultSummary));
                }
            }
        }

        public int CurrentResultIndex
        {
            get => _currentResultIndex;
            private set
            {
                if (SetProperty(ref _currentResultIndex, value))
                {
                    OnPropertyChanged(nameof(ResultSummary));
                }
            }
        }

        public string ResultSummary => ResultCount == 0
            ? "No results"
            : $"{CurrentResultIndex} of {ResultCount}";

        private void ApplyTextFilter(object columnKey, string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                FilteringModel.Remove(columnKey);
                return;
            }

            FilteringModel.SetOrUpdate(new FilteringDescriptor(
                columnId: columnKey,
                @operator: FilteringOperator.Contains,
                value: text.Trim(),
                stringComparison: StringComparison.OrdinalIgnoreCase));
        }

        private void ApplyNumberFilter(object columnKey, double? min, double? max)
        {
            if (min == null && max == null)
            {
                FilteringModel.Remove(columnKey);
                return;
            }

            var lower = min ?? double.MinValue;
            var upper = max ?? double.MaxValue;

            FilteringModel.SetOrUpdate(new FilteringDescriptor(
                columnId: columnKey,
                @operator: FilteringOperator.Between,
                values: new object[] { lower, upper }));
        }

        private void ApplyStatusFilter(object columnKey, IReadOnlyList<string> selected)
        {
            if (selected.Count == 0)
            {
                FilteringModel.Remove(columnKey);
                return;
            }

            var values = selected
                .Select(name => Enum.TryParse<PersonStatus>(name, out var status) ? (object)status : null)
                .Where(value => value != null)
                .ToArray();

            FilteringModel.SetOrUpdate(new FilteringDescriptor(
                columnId: columnKey,
                @operator: FilteringOperator.In,
                values: values));
        }

        private void ClearFilter(object columnKey, Action reset)
        {
            reset();
            FilteringModel.Remove(columnKey);
        }

        private void ClearFilters()
        {
            NameFilter.ClearCommand.Execute(null);
            AgeFilter.ClearCommand.Execute(null);
            StatusFilter.ClearCommand.Execute(null);
        }

        private void ApplySearch()
        {
            if (string.IsNullOrWhiteSpace(_query))
            {
                SearchModel.Clear();
                return;
            }

            SearchModel.SetOrUpdate(new SearchDescriptor(
                _query.Trim(),
                matchMode: SearchMatchMode.Contains,
                termMode: SearchTermCombineMode.Any,
                scope: SearchScope.AllColumns,
                comparison: StringComparison.OrdinalIgnoreCase));
        }

        private void SearchModelOnResultsChanged(object? sender, SearchResultsChangedEventArgs e)
        {
            ResultCount = SearchModel.Results.Count;
            CurrentResultIndex = SearchModel.CurrentIndex >= 0 ? SearchModel.CurrentIndex + 1 : 0;
            NextCommand.RaiseCanExecuteChanged();
            PreviousCommand.RaiseCanExecuteChanged();
        }

        private void SearchModelOnCurrentChanged(object? sender, SearchCurrentChangedEventArgs e)
        {
            CurrentResultIndex = SearchModel.CurrentIndex >= 0 ? SearchModel.CurrentIndex + 1 : 0;
        }

        private void OnMissingAccessor(object? sender, DataGridFastPathMissingAccessorEventArgs e)
        {
            var column = e.Column?.Header?.ToString() ?? e.ColumnId?.ToString() ?? "(unknown)";
            Diagnostics.Insert(0, new FastPathDiagnostic(e.Feature.ToString(), column, e.Message));
            if (Diagnostics.Count > 50)
            {
                Diagnostics.RemoveAt(Diagnostics.Count - 1);
            }

            ClearDiagnosticsCommand.RaiseCanExecuteChanged();
        }

        private void ClearDiagnostics()
        {
            Diagnostics.Clear();
            ClearDiagnosticsCommand.RaiseCanExecuteChanged();
        }

        private static string GetAgeGroup(int age)
        {
            if (age < 18)
            {
                return "Under 18";
            }

            if (age < 30)
            {
                return "18-29";
            }

            if (age < 45)
            {
                return "30-44";
            }

            if (age < 60)
            {
                return "45-59";
            }

            return "60+";
        }

        private static IComparer CreateStatusComparer()
        {
            var order = new Dictionary<PersonStatus, int>
            {
                [PersonStatus.Active] = 0,
                [PersonStatus.New] = 1,
                [PersonStatus.Suspended] = 2,
                [PersonStatus.Disabled] = 3
            };

            return Comparer<PersonStatus>.Create((x, y) =>
            {
                order.TryGetValue(x, out var left);
                order.TryGetValue(y, out var right);
                return left.CompareTo(right);
            });
        }

        private static ObservableCollection<Person> CreatePeople()
        {
            return new ObservableCollection<Person>
            {
                new Person { FirstName = "Ada", LastName = "Lovelace", Age = 36, Status = PersonStatus.Active },
                new Person { FirstName = "Alan", LastName = "Turing", Age = 41, Status = PersonStatus.Suspended },
                new Person { FirstName = "Grace", LastName = "Hopper", Age = 85, Status = PersonStatus.Active },
                new Person { FirstName = "Edsger", LastName = "Dijkstra", Age = 72, Status = PersonStatus.Disabled },
                new Person { FirstName = "Barbara", LastName = "Liskov", Age = 84, Status = PersonStatus.Active },
                new Person { FirstName = "Donald", LastName = "Knuth", Age = 86, Status = PersonStatus.Active },
                new Person { FirstName = "Katherine", LastName = "Johnson", Age = 101, Status = PersonStatus.Active }
            };
        }

        public sealed class FastPathDiagnostic
        {
            public FastPathDiagnostic(string feature, string column, string message)
            {
                Feature = feature;
                Column = column;
                Message = message;
            }

            public string Feature { get; }

            public string Column { get; }

            public string Message { get; }
        }
    }
}
