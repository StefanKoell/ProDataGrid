using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Data.Core;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ColumnDefinitionsBuilderViewModel : ObservableObject
    {
        public ColumnDefinitionsBuilderViewModel()
        {
            Items = new ObservableCollection<Person>(CreatePeople());

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
                        c.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    }),
                builder.Template(
                    header: "Badge",
                    cellTemplateKey: "StatusBadgeTemplate",
                    configure: c =>
                    {
                        c.ColumnKey = "badge";
                        c.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                        c.IsReadOnly = true;
                    })
            };

            ColumnKeys = new ObservableCollection<ColumnKeyInfo>(
                ColumnDefinitions.Select(d => new ColumnKeyInfo(
                    d.Header?.ToString() ?? string.Empty,
                    d.ColumnKey?.ToString() ?? string.Empty)));
        }

        public ObservableCollection<Person> Items { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        public ObservableCollection<ColumnKeyInfo> ColumnKeys { get; }

        private static IPropertyInfo CreateProperty<TValue>(
            string name,
            Func<Person, TValue> getter,
            Action<Person, TValue>? setter = null)
        {
            return new ClrPropertyInfo(
                name,
                target => target is Person person ? getter(person) : default!,
                setter == null
                    ? null
                    : (target, value) =>
                    {
                        if (target is Person person)
                        {
                            setter(person, value is null ? default! : (TValue)value);
                        }
                    },
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

        public sealed class ColumnKeyInfo
        {
            public ColumnKeyInfo(string header, string key)
            {
                Header = header;
                Key = key;
            }

            public string Header { get; }

            public string Key { get; }
        }
    }
}
