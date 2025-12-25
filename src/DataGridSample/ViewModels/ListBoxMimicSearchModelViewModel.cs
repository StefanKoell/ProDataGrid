using System;
using System.Collections.ObjectModel;
using Avalonia.Controls.DataGridSearching;
using DataGridSample.Models;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ListBoxMimicSearchModelViewModel : ObservableObject
    {
        private const int ItemCount = 20000;

        private string _summary = "Items: 0";
        private string _query = string.Empty;
        private string _resultSummary = "No results";

        public ListBoxMimicSearchModelViewModel()
        {
            Items = new ObservableCollection<ListEntry>();
            Populate();

            SearchModel = new SearchModel
            {
                HighlightMode = SearchHighlightMode.TextAndCell,
                HighlightCurrent = true,
                WrapNavigation = true,
                UpdateSelectionOnNavigate = true
            };

            ClearCommand = new RelayCommand(_ => Query = string.Empty);
            NextCommand = new RelayCommand(_ => SearchModel.MoveNext(), _ => SearchModel.Results.Count > 0);
            PreviousCommand = new RelayCommand(_ => SearchModel.MovePrevious(), _ => SearchModel.Results.Count > 0);

            SearchModel.ResultsChanged += (_, __) => UpdateResultSummary();
            SearchModel.CurrentChanged += (_, __) => UpdateResultSummary();
            UpdateResultSummary();
        }

        public ObservableCollection<ListEntry> Items { get; }

        public SearchModel SearchModel { get; }

        public RelayCommand ClearCommand { get; }

        public RelayCommand NextCommand { get; }

        public RelayCommand PreviousCommand { get; }

        public string Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
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
            set => SetProperty(ref _resultSummary, value);
        }

        public record ListEntry(int Index, string Name, string Region)
        {
            public string Display => $"{Name} - {Region}";
        }

        private void ApplySearch()
        {
            if (string.IsNullOrWhiteSpace(_query))
            {
                SearchModel.Clear();
                UpdateResultSummary();
                return;
            }

            SearchModel.SetOrUpdate(new SearchDescriptor(
                _query.Trim(),
                matchMode: SearchMatchMode.Contains,
                termMode: SearchTermCombineMode.Any,
                scope: SearchScope.VisibleColumns,
                comparison: StringComparison.OrdinalIgnoreCase));
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

        private void Populate()
        {
            Items.Clear();

            var source = Countries.All;
            for (var i = 0; i < ItemCount; i++)
            {
                var country = source[i % source.Count];
                Items.Add(new ListEntry(i + 1, country.Name, country.Region));
            }

            Summary = $"Items: {Items.Count:n0}";
        }
    }
}
