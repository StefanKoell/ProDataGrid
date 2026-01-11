using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Core;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ColumnDefinitionsTypedAccessorsViewModel : ObservableObject
    {
        private readonly DataGridColumnValueAccessor<Person, int> _ageAccessor;
        private readonly DataGridColumnValueAccessor<Person, string> _fullNameAccessor;
        private readonly DataGridColumnValueAccessor<Person, PersonStatus> _statusAccessor;
        private readonly RelayCommand _sortAgeAscendingCommand;
        private readonly RelayCommand _sortAgeDescendingCommand;
        private readonly RelayCommand _sortNameCommand;
        private readonly RelayCommand _sortStatusCommand;
        private readonly RelayCommand _clearSortsCommand;

        public ColumnDefinitionsTypedAccessorsViewModel()
        {
            Items = new ObservableCollection<Person>(CreatePeople());
            ItemsView = new DataGridCollectionView(Items)
            {
                Culture = CultureInfo.InvariantCulture
            };

            _ageAccessor = new DataGridColumnValueAccessor<Person, int>(p => p.Age, (p, v) => p.Age = v);
            _fullNameAccessor = new DataGridColumnValueAccessor<Person, string>(p => $"{p.FirstName} {p.LastName}");
            _statusAccessor = new DataGridColumnValueAccessor<Person, PersonStatus>(p => p.Status, (p, v) => p.Status = v);

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
                        c.ColumnKey = "first-name";
                        c.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Text(
                    header: "Last Name",
                    property: lastNameProperty,
                    getter: p => p.LastName,
                    setter: (p, v) => p.LastName = v,
                    configure: c =>
                    {
                        c.ColumnKey = "last-name";
                        c.Width = new DataGridLength(1.2, DataGridLengthUnitType.Star);
                    }),
                builder.Template(
                    header: "Full Name",
                    cellTemplateKey: "FullNameTemplate",
                    configure: c =>
                    {
                        c.ColumnKey = "full-name";
                        c.SortMemberPath = "FullName";
                        c.ValueAccessor = _fullNameAccessor;
                        c.ValueType = typeof(string);
                        c.IsReadOnly = true;
                        c.Width = new DataGridLength(1.5, DataGridLengthUnitType.Star);
                    }),
                builder.Numeric(
                    header: "Age",
                    property: ageProperty,
                    getter: p => p.Age,
                    setter: (p, v) => p.Age = v,
                    configure: c =>
                    {
                        c.ColumnKey = "age";
                        c.Width = new DataGridLength(0.7, DataGridLengthUnitType.Star);
                        c.Minimum = 0;
                        c.Maximum = 120;
                        c.Increment = 1;
                        c.FormatString = "N0";
                    }),
                builder.ComboBoxSelectedItem(
                    header: "Status",
                    property: statusProperty,
                    getter: p => p.Status,
                    setter: (p, v) => p.Status = v,
                    configure: c =>
                    {
                        c.ColumnKey = "status";
                        c.ItemsSource = Enum.GetValues<PersonStatus>();
                        c.Width = new DataGridLength(1.1, DataGridLengthUnitType.Star);
                    })
            };

            _sortAgeAscendingCommand = new RelayCommand(_ => ApplySorts(
                DataGridSortDescription.FromAccessor(_ageAccessor, ListSortDirection.Ascending, ItemsView.Culture, nameof(Person.Age))));
            _sortAgeDescendingCommand = new RelayCommand(_ => ApplySorts(
                DataGridSortDescription.FromAccessor(_ageAccessor, ListSortDirection.Descending, ItemsView.Culture, nameof(Person.Age))));
            _sortNameCommand = new RelayCommand(_ => ApplySorts(
                DataGridSortDescription.FromAccessor(_fullNameAccessor, ListSortDirection.Ascending, ItemsView.Culture, "FullName")));
            _sortStatusCommand = new RelayCommand(_ => ApplySorts(CreateStatusSortDescription()));
            _clearSortsCommand = new RelayCommand(_ => ItemsView.SortDescriptions.Clear(), _ => ItemsView.SortDescriptions.Count > 0);

            ItemsView.SortDescriptions.CollectionChanged += (_, __) => _clearSortsCommand.RaiseCanExecuteChanged();
        }

        public ObservableCollection<Person> Items { get; }

        public DataGridCollectionView ItemsView { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        public RelayCommand SortAgeAscendingCommand => _sortAgeAscendingCommand;

        public RelayCommand SortAgeDescendingCommand => _sortAgeDescendingCommand;

        public RelayCommand SortNameCommand => _sortNameCommand;

        public RelayCommand SortStatusCommand => _sortStatusCommand;

        public RelayCommand ClearSortsCommand => _clearSortsCommand;

        private void ApplySorts(params DataGridSortDescription[] sorts)
        {
            var sortDescriptions = ItemsView.SortDescriptions;
            sortDescriptions.Clear();

            foreach (var sort in sorts)
            {
                if (sort != null)
                {
                    sortDescriptions.Add(sort);
                }
            }
        }

        private DataGridSortDescription CreateStatusSortDescription()
        {
            var order = new Dictionary<PersonStatus, int>
            {
                [PersonStatus.Active] = 0,
                [PersonStatus.New] = 1,
                [PersonStatus.Suspended] = 2,
                [PersonStatus.Disabled] = 3
            };

            var comparer = Comparer<PersonStatus>.Create((x, y) =>
            {
                order.TryGetValue(x, out var left);
                order.TryGetValue(y, out var right);
                return left.CompareTo(right);
            });

            var sortComparer = new DataGridColumnValueAccessorComparer<Person, PersonStatus>(_statusAccessor, comparer, ItemsView.Culture);
            return DataGridSortDescription.FromComparer(sortComparer, ListSortDirection.Ascending, nameof(Person.Status));
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
                new Person { FirstName = "Donald", LastName = "Knuth", Age = 86, Status = PersonStatus.Active }
            };
        }
    }
}
