using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Data.Core;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ColumnDefinitionsViewModel : ObservableObject
    {
        public ColumnDefinitionsViewModel()
        {
            Items = CreatePeople();

            ColumnDefinitions = new ObservableCollection<DataGridColumnDefinition>
            {
                new DataGridTextColumnDefinition
                {
                    Header = "First Name",
                    Binding = CreateBinding(nameof(Person.FirstName), p => p.FirstName, (p, v) => p.FirstName = v),
                    Width = new DataGridLength(1.2, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumnDefinition
                {
                    Header = "Last Name",
                    Binding = CreateBinding(nameof(Person.LastName), p => p.LastName, (p, v) => p.LastName = v),
                    Width = new DataGridLength(1.2, DataGridLengthUnitType.Star)
                },
                new DataGridNumericColumnDefinition
                {
                    Header = "Age",
                    Binding = CreateBinding(nameof(Person.Age), p => p.Age, (p, v) => p.Age = v),
                    Width = new DataGridLength(0.7, DataGridLengthUnitType.Star),
                    Minimum = 0,
                    Maximum = 120,
                    Increment = 1,
                    FormatString = "N0"
                },
                new DataGridComboBoxColumnDefinition
                {
                    Header = "Status",
                    ItemsSource = Enum.GetValues<PersonStatus>(),
                    SelectedItemBinding = CreateBinding(nameof(Person.Status), p => p.Status, (p, v) => p.Status = v),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                },
                new DataGridTemplateColumnDefinition
                {
                    Header = "Badge",
                    CellTemplateKey = "StatusBadgeTemplate",
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                    IsReadOnly = true
                },
                new DataGridHyperlinkColumnDefinition
                {
                    Header = "Profile",
                    Binding = CreateBinding(nameof(Person.ProfileLink), p => p.ProfileLink, (p, v) => p.ProfileLink = v),
                    ContentBinding = CreateBinding(nameof(Person.ProfileLink), p => p.ProfileLink, (p, v) => p.ProfileLink = v),
                    Width = new DataGridLength(1.6, DataGridLengthUnitType.Star),
                    CellStyleClasses = new[] { "profile-cell" }
                }
            };
        }

        public ObservableCollection<Person> Items { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        private static DataGridBindingDefinition CreateBinding<TValue>(
            string name,
            Func<Person, TValue> getter,
            Action<Person, TValue>? setter = null)
        {
            var propertyInfo = new ClrPropertyInfo(
                name,
                target => getter((Person)target),
                setter == null
                    ? null
                    : (target, value) => setter((Person)target, value is null ? default : (TValue)value),
                typeof(TValue));

            return DataGridBindingDefinition.Create<Person, TValue>(propertyInfo, getter, setter);
        }

        private static ObservableCollection<Person> CreatePeople()
        {
            return new ObservableCollection<Person>
            {
                new Person
                {
                    FirstName = "Ada",
                    LastName = "Lovelace",
                    Age = 36,
                    Status = PersonStatus.Active,
                    ProfileLink = new Uri("https://example.com/ada")
                },
                new Person
                {
                    FirstName = "Alan",
                    LastName = "Turing",
                    Age = 41,
                    Status = PersonStatus.Suspended,
                    ProfileLink = new Uri("https://example.com/alan")
                },
                new Person
                {
                    FirstName = "Grace",
                    LastName = "Hopper",
                    Age = 85,
                    Status = PersonStatus.Active,
                    ProfileLink = new Uri("https://example.com/grace")
                },
                new Person
                {
                    FirstName = "Edsger",
                    LastName = "Dijkstra",
                    Age = 72,
                    Status = PersonStatus.Disabled,
                    ProfileLink = new Uri("https://example.com/edsger")
                }
            };
        }
    }
}
