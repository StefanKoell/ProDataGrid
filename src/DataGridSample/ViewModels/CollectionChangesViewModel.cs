using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class CollectionChangesViewModel : ObservableObject
    {
        private readonly Random _random = new Random();
        private ChangeItem? _selectedItem;
        private int _seed;

        public CollectionChangesViewModel()
        {
            Items = new ObservableCollection<ChangeItem>();
            View = new DataGridCollectionView(Items);
            Events = new ObservableCollection<string>();

            View.CollectionChanged += OnCollectionChanged;

            AddCommand = new RelayCommand(_ => AddItem());
            RemoveCommand = new RelayCommand(_ => RemoveSelected());
            ReplaceCommand = new RelayCommand(_ => ReplaceSelected());
            MoveUpCommand = new RelayCommand(_ => MoveSelected(-1));
            MoveDownCommand = new RelayCommand(_ => MoveSelected(1));
            ResetCommand = new RelayCommand(_ => ResetItems());

            ResetItems();
        }

        public ObservableCollection<ChangeItem> Items { get; }

        public DataGridCollectionView View { get; }

        public ObservableCollection<string> Events { get; }

        public ChangeItem? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand RemoveCommand { get; }
        public RelayCommand ReplaceCommand { get; }
        public RelayCommand MoveUpCommand { get; }
        public RelayCommand MoveDownCommand { get; }
        public RelayCommand ResetCommand { get; }

        private void AddItem()
        {
            Items.Add(CreateItem());
        }

        private void RemoveSelected()
        {
            if (SelectedItem != null)
            {
                Items.Remove(SelectedItem);
                return;
            }

            if (Items.Count > 0)
            {
                Items.RemoveAt(Items.Count - 1);
            }
        }

        private void ReplaceSelected()
        {
            if (SelectedItem == null)
            {
                return;
            }

            var index = Items.IndexOf(SelectedItem);
            if (index < 0)
            {
                return;
            }

            var updated = new ChangeItem
            {
                Id = SelectedItem.Id,
                Name = $"{SelectedItem.Name}*",
                Value = SelectedItem.Value + 10
            };

            Items[index] = updated;
            SelectedItem = updated;
        }

        private void MoveSelected(int delta)
        {
            if (SelectedItem == null)
            {
                return;
            }

            var index = Items.IndexOf(SelectedItem);
            if (index < 0)
            {
                return;
            }

            var target = index + delta;
            if (target < 0 || target >= Items.Count)
            {
                return;
            }

            Items.Move(index, target);
        }

        private void ResetItems()
        {
            Items.Clear();
            foreach (var item in Enumerable.Range(1, 6).Select(i => CreateItem($"Item {i}")))
            {
                Items.Add(item);
            }
            SelectedItem = Items.FirstOrDefault();
        }

        private ChangeItem CreateItem(string? name = null)
        {
            return new ChangeItem
            {
                Id = ++_seed,
                Name = name ?? $"Item {_seed}",
                Value = _random.Next(1, 100)
            };
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            string message = e.Action switch
            {
                NotifyCollectionChangedAction.Add => $"Add: {DescribeItems(e.NewItems)} at {e.NewStartingIndex}",
                NotifyCollectionChangedAction.Remove => $"Remove: {DescribeItems(e.OldItems)} from {e.OldStartingIndex}",
                NotifyCollectionChangedAction.Move => $"Move: {DescribeItems(e.OldItems)} from {e.OldStartingIndex} to {e.NewStartingIndex}",
                NotifyCollectionChangedAction.Replace => $"Replace: {DescribeItems(e.OldItems)} -> {DescribeItems(e.NewItems)} at {e.NewStartingIndex}",
                NotifyCollectionChangedAction.Reset => "Reset",
                _ => e.Action.ToString()
            };

            Events.Insert(0, message);

            // keep the log from growing unbounded in the sample
            if (Events.Count > 50)
            {
                Events.RemoveAt(Events.Count - 1);
            }
        }

        private static string DescribeItems(IList? list)
        {
            if (list == null || list.Count == 0)
            {
                return "(none)";
            }

            return string.Join(", ", list.Cast<ChangeItem>().Select(i => $"#{i.Id}:{i.Name}"));
        }
    }
}
