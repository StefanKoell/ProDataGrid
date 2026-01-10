using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls.DataGridSearching;
using Avalonia.Controls.DataGridSorting;
using Avalonia.Data.Core;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ColumnDefinitionsFastPathShowcaseViewModel : ObservableObject
    {
        private const string FirstNameKey = "first-name";
        private const string LastNameKey = "last-name";
        private const string FullNameKey = "full-name";
        private const string AgeKey = "age";
        private const string StatusKey = "status";
        private const string BadgeKey = "badge";

        private readonly DataGridColumnValueAccessor<Person, string> _fullNameAccessor;
        private readonly DataGridColumnValueAccessor<Person, string> _fullNameSortAccessor;
        private readonly DataGridColumnValueAccessor<Person, string> _badgeAccessor;
        private readonly DataGridColumnValueAccessor<Person, int> _ageAccessor;
        private int _resultCount;
        private int _currentResultIndex;
        private string _query = string.Empty;

        public ColumnDefinitionsFastPathShowcaseViewModel()
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

            _fullNameAccessor = new DataGridColumnValueAccessor<Person, string>(p => $"{p.FirstName} {p.LastName}");
            _fullNameSortAccessor = new DataGridColumnValueAccessor<Person, string>(p => $"{p.LastName}, {p.FirstName}");
            _badgeAccessor = new DataGridColumnValueAccessor<Person, string>(p => p.Status.ToString());
            _ageAccessor = new DataGridColumnValueAccessor<Person, int>(p => p.Age, (p, v) => p.Age = v);

            var builder = DataGridColumnDefinitionBuilder.For<Person>();

            var firstNameProperty = CreateProperty(nameof(Person.FirstName), p => p.FirstName, (p, v) => p.FirstName = v);
            var lastNameProperty = CreateProperty(nameof(Person.LastName), p => p.LastName, (p, v) => p.LastName = v);
            var ageProperty = CreateProperty(nameof(Person.Age), p => p.Age, (p, v) => p.Age = v);
            var statusProperty = CreateProperty(nameof(Person.Status), p => p.Status, (p, v) => p.Status = v);

            ColumnDefinitions = new ObservableCollection<DataGridColumnDefinition>
            {
                builder.Text(
                    header: "First Name",
                    property: firstNameProperty,
                    getter: p => p.FirstName,
                    setter: (p, v) => p.FirstName = v,
                    configure: c =>
                    {
                        c.ColumnKey = FirstNameKey;
                        c.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Text(
                    header: "Last Name",
                    property: lastNameProperty,
                    getter: p => p.LastName,
                    setter: (p, v) => p.LastName = v,
                    configure: c =>
                    {
                        c.ColumnKey = LastNameKey;
                        c.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Template(
                    header: "Full Name",
                    cellTemplateKey: "FullNameTemplate",
                    configure: c =>
                    {
                        c.ColumnKey = FullNameKey;
                        c.SortMemberPath = "FullName";
                        c.ValueAccessor = _fullNameAccessor;
                        c.ValueType = typeof(string);
                        c.Options = new DataGridColumnDefinitionOptions
                        {
                            SearchTextProvider = item => item is Person p ? $"{p.FirstName} {p.LastName}" : string.Empty,
                            SortValueAccessor = _fullNameSortAccessor
                        };
                        c.IsReadOnly = true;
                        c.Width = new DataGridLength(1.6, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Age",
                    property: ageProperty,
                    getter: p => p.Age,
                    setter: (p, v) => p.Age = v,
                    configure: c =>
                    {
                        c.ColumnKey = AgeKey;
                        c.Width = new DataGridLength(0.7, DataGridLengthUnitType.Star);
                        c.Minimum = 0;
                        c.Maximum = 120;
                        c.Increment = 1;
                        c.FormatString = "N0";
                        c.ValueAccessor = _ageAccessor;
                    }),
                builder.ComboBoxSelectedItem(
                    header: "Status",
                    property: statusProperty,
                    getter: p => p.Status,
                    setter: (p, v) => p.Status = v,
                    configure: c =>
                    {
                        c.ColumnKey = StatusKey;
                        c.ItemsSource = Enum.GetValues<PersonStatus>();
                        c.Options = new DataGridColumnDefinitionOptions
                        {
                            SortValueComparer = CreateStatusComparer()
                        };
                        c.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    }),
                builder.Template(
                    header: "Badge",
                    cellTemplateKey: "StatusBadgeTemplate",
                    configure: c =>
                    {
                        c.ColumnKey = BadgeKey;
                        c.ValueAccessor = _badgeAccessor;
                        c.ValueType = typeof(string);
                        c.IsReadOnly = true;
                        c.Width = new DataGridLength(0.9, DataGridLengthUnitType.Star);
                    })
            };

            FastPathOptions = new DataGridFastPathOptions
            {
                UseAccessorsOnly = true,
                ThrowOnMissingAccessor = true
            };

            NameFilter = new TextFilterContext(
                "First name contains",
                apply: text => ApplyTextFilter(FirstNameKey, text),
                clear: () => ClearFilter(FirstNameKey, () => NameFilter.Text = string.Empty));

            AgeFilter = new NumberFilterContext(
                "Age between",
                apply: (min, max) => ApplyNumberFilter(AgeKey, min, max),
                clear: () => ClearFilter(AgeKey, () =>
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
            ClearSortsCommand = new RelayCommand(_ => SortingModel.Clear(), _ => SortingModel.Descriptors.Count > 0);
            SortByAgeDescendingCommand = new RelayCommand(_ => ApplySort(AgeKey, ListSortDirection.Descending));
            SortByNameCommand = new RelayCommand(_ => ApplySort(FullNameKey, ListSortDirection.Ascending));
            NextCommand = new RelayCommand(_ => SearchModel.MoveNext(), _ => SearchModel.Results.Count > 0);
            PreviousCommand = new RelayCommand(_ => SearchModel.MovePrevious(), _ => SearchModel.Results.Count > 0);

            SortingModel.SortingChanged += (_, __) => ClearSortsCommand.RaiseCanExecuteChanged();
        }

        public ObservableCollection<Person> Items { get; }

        public DataGridCollectionView View { get; }

        public FilteringModel FilteringModel { get; }

        public SearchModel SearchModel { get; }

        public SortingModel SortingModel { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        public DataGridFastPathOptions FastPathOptions { get; }

        public TextFilterContext NameFilter { get; }

        public NumberFilterContext AgeFilter { get; }

        public EnumFilterContext StatusFilter { get; }

        public RelayCommand ClearFiltersCommand { get; }

        public RelayCommand ClearSearchCommand { get; }

        public RelayCommand ClearSortsCommand { get; }

        public RelayCommand SortByAgeDescendingCommand { get; }

        public RelayCommand SortByNameCommand { get; }

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

        private void ApplySort(object columnKey, ListSortDirection direction)
        {
            SortingModel.Apply(new[]
            {
                new SortingDescriptor(columnKey, direction)
            });
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

        private static IPropertyInfo CreateProperty<TValue>(
            string name,
            Func<Person, TValue> getter,
            Action<Person, TValue>? setter = null)
        {
            return new ClrPropertyInfo(
                name,
                target => getter((Person)target),
                setter == null
                    ? null
                    : (target, value) => setter((Person)target, value is null ? default! : (TValue)value),
                typeof(TValue));
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
                new Person { FirstName = "Katherine", LastName = "Johnson", Age = 101, Status = PersonStatus.Active },
                new Person { FirstName = "Evelyn", LastName = "Boyd", Age = 85, Status = PersonStatus.Active }
            };
        }
    }
}
