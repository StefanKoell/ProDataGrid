using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using DataGridSample.Helpers;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ColumnDefinitionsIncrementalUpdatesViewModel : ObservableObject
    {
        private const string FirstNameKey = "first-name";
        private const string AgeKey = "age";

        private readonly List<Func<DataGridColumnDefinition>> _extraFactories;
        private int _extraIndex;
        private readonly RelayCommand _addColumnCommand;
        private readonly RelayCommand _removeLastCommand;
        private readonly RelayCommand _moveFirstToEndCommand;
        private readonly RelayCommand _replaceSecondCommand;
        private readonly RelayCommand _resetColumnsCommand;
        private readonly RelayCommand _toggleFirstNameHeaderCommand;
        private readonly RelayCommand _toggleAgeReadOnlyCommand;
        private bool _useAlternateHeader;

        public ColumnDefinitionsIncrementalUpdatesViewModel()
        {
            Items = new ObservableCollection<Person>(CreatePeople());

            ColumnDefinitions = new ObservableCollection<DataGridColumnDefinition>
            {
                CreateFirstNameColumn(),
                CreateLastNameColumn(),
                CreateAgeColumn()
            };

            _extraFactories = new List<Func<DataGridColumnDefinition>>
            {
                CreateStatusColumn,
                CreateBadgeColumn,
                CreateProfileColumn,
                CreateFullNameColumn
            };

            ActivityLog = new ObservableCollection<string>();

            _addColumnCommand = new RelayCommand(_ => AddColumn(), _ => _extraIndex < _extraFactories.Count);
            _removeLastCommand = new RelayCommand(_ => RemoveLast(), _ => ColumnDefinitions.Count > 0);
            _moveFirstToEndCommand = new RelayCommand(_ => MoveFirstToEnd(), _ => ColumnDefinitions.Count > 1);
            _replaceSecondCommand = new RelayCommand(_ => ReplaceSecond(), _ => ColumnDefinitions.Count > 1);
            _resetColumnsCommand = new RelayCommand(_ => ResetColumns());
            _toggleFirstNameHeaderCommand = new RelayCommand(_ => ToggleFirstNameHeader(), _ => GetFirstNameDefinition() != null);
            _toggleAgeReadOnlyCommand = new RelayCommand(_ => ToggleAgeReadOnly(), _ => GetAgeDefinition() != null);

            ColumnDefinitions.CollectionChanged += OnColumnsChanged;
            UpdateCommandStates();
        }

        public ObservableCollection<Person> Items { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        public ObservableCollection<string> ActivityLog { get; }

        public RelayCommand AddColumnCommand => _addColumnCommand;

        public RelayCommand RemoveLastCommand => _removeLastCommand;

        public RelayCommand MoveFirstToEndCommand => _moveFirstToEndCommand;

        public RelayCommand ReplaceSecondCommand => _replaceSecondCommand;

        public RelayCommand ResetColumnsCommand => _resetColumnsCommand;

        public RelayCommand ToggleFirstNameHeaderCommand => _toggleFirstNameHeaderCommand;

        public RelayCommand ToggleAgeReadOnlyCommand => _toggleAgeReadOnlyCommand;

        private void AddColumn()
        {
            if (_extraIndex >= _extraFactories.Count)
            {
                return;
            }

            var column = _extraFactories[_extraIndex++]();
            ColumnDefinitions.Add(column);
            Log($"Added column: {column.Header}");
            UpdateCommandStates();
        }

        private void RemoveLast()
        {
            if (ColumnDefinitions.Count == 0)
            {
                return;
            }

            var index = ColumnDefinitions.Count - 1;
            var column = ColumnDefinitions[index];
            ColumnDefinitions.RemoveAt(index);
            Log($"Removed column: {column.Header}");
            UpdateCommandStates();
        }

        private void MoveFirstToEnd()
        {
            if (ColumnDefinitions.Count <= 1)
            {
                return;
            }

            ColumnDefinitions.Move(0, ColumnDefinitions.Count - 1);
            Log("Moved first column to end");
        }

        private void ReplaceSecond()
        {
            if (ColumnDefinitions.Count <= 1)
            {
                return;
            }

            var replacement = CreateFullNameColumn();
            ColumnDefinitions[1] = replacement;
            Log($"Replaced second column with: {replacement.Header}");
        }

        private void ResetColumns()
        {
            _extraIndex = 0;
            _useAlternateHeader = false;
            ColumnDefinitions.Clear();
            ColumnDefinitions.Add(CreateFirstNameColumn());
            ColumnDefinitions.Add(CreateLastNameColumn());
            ColumnDefinitions.Add(CreateAgeColumn());
            Log("Reset columns to defaults");
            UpdateCommandStates();
        }

        private void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {
            _addColumnCommand.RaiseCanExecuteChanged();
            _removeLastCommand.RaiseCanExecuteChanged();
            _moveFirstToEndCommand.RaiseCanExecuteChanged();
            _replaceSecondCommand.RaiseCanExecuteChanged();
            _toggleFirstNameHeaderCommand.RaiseCanExecuteChanged();
            _toggleAgeReadOnlyCommand.RaiseCanExecuteChanged();
        }

        private void Log(string message)
        {
            ActivityLog.Insert(0, $"{DateTime.Now:HH:mm:ss} - {message}");
            if (ActivityLog.Count > 50)
            {
                ActivityLog.RemoveAt(ActivityLog.Count - 1);
            }
        }

        private void ToggleFirstNameHeader()
        {
            var definition = GetFirstNameDefinition();
            if (definition == null)
            {
                return;
            }

            _useAlternateHeader = !_useAlternateHeader;
            definition.Header = _useAlternateHeader ? "Given Name" : "First Name";
            Log("Toggled first name header");
        }

        private void ToggleAgeReadOnly()
        {
            var definition = GetAgeDefinition();
            if (definition == null)
            {
                return;
            }

            definition.IsReadOnly = !(definition.IsReadOnly ?? false);
            Log($"Age column read-only: {definition.IsReadOnly}");
        }

        private DataGridTextColumnDefinition? GetFirstNameDefinition()
        {
            return ColumnDefinitions.OfType<DataGridTextColumnDefinition>()
                .FirstOrDefault(definition => Equals(definition.ColumnKey, FirstNameKey));
        }

        private DataGridNumericColumnDefinition? GetAgeDefinition()
        {
            return ColumnDefinitions.OfType<DataGridNumericColumnDefinition>()
                .FirstOrDefault(definition => Equals(definition.ColumnKey, AgeKey));
        }

        private static DataGridColumnDefinition CreateFirstNameColumn()
        {
            return new DataGridTextColumnDefinition
            {
                Header = "First Name",
                ColumnKey = FirstNameKey,
                Binding = ColumnDefinitionBindingFactory.CreateBinding<Person, string>(
                    nameof(Person.FirstName),
                    p => p.FirstName,
                    (p, v) => p.FirstName = v),
                Width = new DataGridLength(1.2, DataGridLengthUnitType.Star)
            };
        }

        private static DataGridColumnDefinition CreateLastNameColumn()
        {
            return new DataGridTextColumnDefinition
            {
                Header = "Last Name",
                Binding = ColumnDefinitionBindingFactory.CreateBinding<Person, string>(
                    nameof(Person.LastName),
                    p => p.LastName,
                    (p, v) => p.LastName = v),
                Width = new DataGridLength(1.2, DataGridLengthUnitType.Star)
            };
        }

        private static DataGridColumnDefinition CreateFullNameColumn()
        {
            return new DataGridTemplateColumnDefinition
            {
                Header = "Full Name",
                CellTemplateKey = "FullNameTemplate",
                IsReadOnly = true,
                Width = new DataGridLength(1.4, DataGridLengthUnitType.Star)
            };
        }

        private static DataGridColumnDefinition CreateAgeColumn()
        {
            return new DataGridNumericColumnDefinition
            {
                Header = "Age",
                ColumnKey = AgeKey,
                Binding = ColumnDefinitionBindingFactory.CreateBinding<Person, int>(
                    nameof(Person.Age),
                    p => p.Age,
                    (p, v) => p.Age = v),
                Width = new DataGridLength(0.7, DataGridLengthUnitType.Star),
                Minimum = 0,
                Maximum = 120,
                Increment = 1,
                FormatString = "N0"
            };
        }

        private static DataGridColumnDefinition CreateStatusColumn()
        {
            return new DataGridComboBoxColumnDefinition
            {
                Header = "Status",
                ItemsSource = Enum.GetValues<PersonStatus>(),
                SelectedItemBinding = ColumnDefinitionBindingFactory.CreateBinding<Person, PersonStatus>(
                    nameof(Person.Status),
                    p => p.Status,
                    (p, v) => p.Status = v),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };
        }

        private static DataGridColumnDefinition CreateBadgeColumn()
        {
            return new DataGridTemplateColumnDefinition
            {
                Header = "Badge",
                CellTemplateKey = "StatusBadgeTemplate",
                IsReadOnly = true,
                Width = new DataGridLength(0.9, DataGridLengthUnitType.Star)
            };
        }

        private static DataGridColumnDefinition CreateProfileColumn()
        {
            return new DataGridHyperlinkColumnDefinition
            {
                Header = "Profile",
                Binding = ColumnDefinitionBindingFactory.CreateBinding<Person, Uri?>(
                    nameof(Person.ProfileLink),
                    p => p.ProfileLink,
                    (p, v) => p.ProfileLink = v),
                ContentBinding = ColumnDefinitionBindingFactory.CreateBinding<Person, Uri?>(
                    nameof(Person.ProfileLink),
                    p => p.ProfileLink,
                    (p, v) => p.ProfileLink = v),
                Width = new DataGridLength(1.4, DataGridLengthUnitType.Star)
            };
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
