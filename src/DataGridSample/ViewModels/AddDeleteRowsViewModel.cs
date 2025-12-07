using System.Collections.ObjectModel;
using Avalonia.Collections;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class AddDeleteRowsViewModel : ObservableObject
    {
        private bool _canUserAddRows = true;
        private bool _canUserDeleteRows = true;

        public AddDeleteRowsViewModel()
        {
            People = new ObservableCollection<Person>();
            PeopleView = new DataGridCollectionView(People);

            ResetCommand = new RelayCommand(_ => ResetPeople());

            ResetPeople();
        }

        public ObservableCollection<Person> People { get; }

        public DataGridCollectionView PeopleView { get; }

        public bool CanUserAddRows
        {
            get => _canUserAddRows;
            set => SetProperty(ref _canUserAddRows, value);
        }

        public bool CanUserDeleteRows
        {
            get => _canUserDeleteRows;
            set => SetProperty(ref _canUserDeleteRows, value);
        }

        public RelayCommand ResetCommand { get; }

        private void ResetPeople()
        {
            People.Clear();
            People.Add(new Person { FirstName = "Ana", LastName = "Pratt", Age = 32 });
            People.Add(new Person { FirstName = "Bruno", LastName = "Costa", Age = 28 });
            People.Add(new Person { FirstName = "Chloe", LastName = "Nguyen", Age = 35, IsBanned = true });
        }
    }
}
