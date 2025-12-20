// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSearching;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class ColumnSearchViewModel : ObservableObject
    {
        private string _query = string.Empty;
        private SearchMatchMode _matchMode = SearchMatchMode.Contains;
        private SearchTermCombineMode _termMode = SearchTermCombineMode.Any;
        private SearchHighlightMode _highlightMode = SearchHighlightMode.TextAndCell;
        private bool _highlightCurrent = true;
        private bool _wrapNavigation = true;
        private bool _updateSelectionOnNavigate;
        private bool _caseSensitive;
        private bool _wholeWord;
        private bool _ignoreDiacritics;
        private bool _normalizeWhitespace = true;
        private int _resultCount;
        private int _currentResultIndex;
        private SearchResultSummary? _selectedResult;
        private bool _suppressSelectionUpdate;

        public ColumnSearchViewModel()
        {
            Items = new ObservableCollection<PersonRecord>(CreateItems(1200));
            View = new DataGridCollectionView(Items)
            {
                Culture = CultureInfo.InvariantCulture
            };

            SearchModel = new SearchModel
            {
                HighlightMode = _highlightMode,
                HighlightCurrent = _highlightCurrent,
                WrapNavigation = _wrapNavigation,
                UpdateSelectionOnNavigate = _updateSelectionOnNavigate
            };

            Columns = new ObservableCollection<ColumnOption>(new[]
            {
                new ColumnOption("FirstName", "First Name", true),
                new ColumnOption("LastName", "Last Name", true),
                new ColumnOption("Team", "Team", true),
                new ColumnOption("Title", "Title", true),
                new ColumnOption("Location", "Location", false),
                new ColumnOption("Notes", "Notes", false)
            });

            foreach (var option in Columns)
            {
                option.Changed += OnColumnChanged;
            }

            SearchModel.ResultsChanged += SearchModelOnResultsChanged;
            SearchModel.CurrentChanged += SearchModelOnCurrentChanged;

            NextCommand = new RelayCommand(_ => SearchModel.MoveNext(), _ => SearchModel.Results.Count > 0);
            PreviousCommand = new RelayCommand(_ => SearchModel.MovePrevious(), _ => SearchModel.Results.Count > 0);
            ClearCommand = new RelayCommand(_ => Query = string.Empty);
            SelectAllColumnsCommand = new RelayCommand(_ => SetColumnSelection(true));
            SelectNoneColumnsCommand = new RelayCommand(_ => SetColumnSelection(false));
        }

        public ObservableCollection<PersonRecord> Items { get; }

        public DataGridCollectionView View { get; }

        public SearchModel SearchModel { get; }

        public ObservableCollection<ColumnOption> Columns { get; }

        public ObservableCollection<SearchResultSummary> Results { get; } = new();

        public RelayCommand NextCommand { get; }

        public RelayCommand PreviousCommand { get; }

        public RelayCommand ClearCommand { get; }

        public RelayCommand SelectAllColumnsCommand { get; }

        public RelayCommand SelectNoneColumnsCommand { get; }

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

        public SearchMatchMode MatchMode
        {
            get => _matchMode;
            set
            {
                if (SetProperty(ref _matchMode, value))
                {
                    ApplySearch();
                }
            }
        }

        public SearchTermCombineMode TermMode
        {
            get => _termMode;
            set
            {
                if (SetProperty(ref _termMode, value))
                {
                    ApplySearch();
                }
            }
        }

        public SearchHighlightMode HighlightMode
        {
            get => _highlightMode;
            set
            {
                if (SetProperty(ref _highlightMode, value))
                {
                    SearchModel.HighlightMode = value;
                }
            }
        }

        public bool HighlightCurrent
        {
            get => _highlightCurrent;
            set
            {
                if (SetProperty(ref _highlightCurrent, value))
                {
                    SearchModel.HighlightCurrent = value;
                }
            }
        }

        public bool WrapNavigation
        {
            get => _wrapNavigation;
            set
            {
                if (SetProperty(ref _wrapNavigation, value))
                {
                    SearchModel.WrapNavigation = value;
                }
            }
        }

        public bool UpdateSelectionOnNavigate
        {
            get => _updateSelectionOnNavigate;
            set
            {
                if (SetProperty(ref _updateSelectionOnNavigate, value))
                {
                    SearchModel.UpdateSelectionOnNavigate = value;
                }
            }
        }

        public bool CaseSensitive
        {
            get => _caseSensitive;
            set
            {
                if (SetProperty(ref _caseSensitive, value))
                {
                    ApplySearch();
                }
            }
        }

        public bool WholeWord
        {
            get => _wholeWord;
            set
            {
                if (SetProperty(ref _wholeWord, value))
                {
                    ApplySearch();
                }
            }
        }

        public bool IgnoreDiacritics
        {
            get => _ignoreDiacritics;
            set
            {
                if (SetProperty(ref _ignoreDiacritics, value))
                {
                    ApplySearch();
                }
            }
        }

        public bool NormalizeWhitespace
        {
            get => _normalizeWhitespace;
            set
            {
                if (SetProperty(ref _normalizeWhitespace, value))
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

        public SearchResultSummary? SelectedResult
        {
            get => _selectedResult;
            set
            {
                if (SetProperty(ref _selectedResult, value) && !_suppressSelectionUpdate)
                {
                    if (value != null)
                    {
                        SearchModel.MoveTo(value.Index);
                    }
                }
            }
        }

        private void SetColumnSelection(bool selected)
        {
            foreach (var option in Columns)
            {
                option.IsSelected = selected;
            }
        }

        private void OnColumnChanged(object? sender, EventArgs e)
        {
            ApplySearch();
        }

        private void ApplySearch()
        {
            if (string.IsNullOrWhiteSpace(_query))
            {
                SearchModel.Clear();
                return;
            }

            var selectedColumns = Columns
                .Where(c => c.IsSelected)
                .Select(c => (object)c.Id)
                .ToArray();

            if (selectedColumns.Length == 0)
            {
                SearchModel.Clear();
                return;
            }

            var comparison = _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            SearchModel.SetOrUpdate(new SearchDescriptor(
                _query.Trim(),
                matchMode: _matchMode,
                termMode: _termMode,
                scope: SearchScope.ExplicitColumns,
                columnIds: selectedColumns,
                comparison: comparison,
                wholeWord: _wholeWord,
                normalizeWhitespace: _normalizeWhitespace,
                ignoreDiacritics: _ignoreDiacritics));
        }

        private void SearchModelOnResultsChanged(object? sender, SearchResultsChangedEventArgs e)
        {
            Results.Clear();

            if (SearchModel.Results != null)
            {
                for (int i = 0; i < SearchModel.Results.Count; i++)
                {
                    var result = SearchModel.Results[i];
                    if (result == null)
                    {
                        continue;
                    }

                    Results.Add(new SearchResultSummary(
                        i,
                        result.RowIndex + 1,
                        GetColumnLabel(result),
                        result.Text ?? string.Empty,
                        result.Matches.Count));
                }
            }

            ResultCount = Results.Count;
            CurrentResultIndex = SearchModel.CurrentIndex >= 0 ? SearchModel.CurrentIndex + 1 : 0;
            SyncSelectedResult();

            NextCommand.RaiseCanExecuteChanged();
            PreviousCommand.RaiseCanExecuteChanged();
        }

        private void SearchModelOnCurrentChanged(object? sender, SearchCurrentChangedEventArgs e)
        {
            CurrentResultIndex = SearchModel.CurrentIndex >= 0 ? SearchModel.CurrentIndex + 1 : 0;
            SyncSelectedResult();
        }

        private void SyncSelectedResult()
        {
            _suppressSelectionUpdate = true;
            try
            {
                if (SearchModel.CurrentIndex >= 0 && SearchModel.CurrentIndex < Results.Count)
                {
                    SelectedResult = Results[SearchModel.CurrentIndex];
                }
                else
                {
                    SelectedResult = null;
                }
            }
            finally
            {
                _suppressSelectionUpdate = false;
            }
        }

        private static string GetColumnLabel(SearchResult result)
        {
            if (result.ColumnId is DataGridColumn column)
            {
                return column.Header?.ToString() ?? column.SortMemberPath ?? "(column)";
            }

            return result.ColumnId?.ToString() ?? "(column)";
        }

        private static ObservableCollection<PersonRecord> CreateItems(int count)
        {
            var random = new Random(23);
            var firstNames = new[] { "Alex", "Brooke", "Casey", "Drew", "Ellis", "Frankie", "Gray", "Hayden" };
            var lastNames = new[] { "Rivera", "Nguyen", "Patel", "Kim", "Morales", "Carter", "Baker", "Fisher" };
            var teams = new[] { "Platform", "Operations", "Finance", "Growth", "Design" };
            var titles = new[] { "Analyst", "Engineer", "Manager", "Coordinator", "Strategist" };
            var locations = new[] { "Austin", "Denver", "Portland", "Raleigh", "Seattle" };

            var items = new ObservableCollection<PersonRecord>();

            for (int i = 1; i <= count; i++)
            {
                var first = firstNames[random.Next(firstNames.Length)];
                var last = lastNames[random.Next(lastNames.Length)];
                var team = teams[random.Next(teams.Length)];
                var title = titles[random.Next(titles.Length)];
                var location = locations[random.Next(locations.Length)];
                var notes = $"{title} in {team} based in {location}.";

                items.Add(new PersonRecord(i, first, last, team, title, location, notes));
            }

            return items;
        }

        public sealed class PersonRecord
        {
            public PersonRecord(int id, string firstName, string lastName, string team, string title, string location, string notes)
            {
                Id = id;
                FirstName = firstName;
                LastName = lastName;
                Team = team;
                Title = title;
                Location = location;
                Notes = notes;
            }

            public int Id { get; }
            public string FirstName { get; }
            public string LastName { get; }
            public string Team { get; }
            public string Title { get; }
            public string Location { get; }
            public string Notes { get; }
        }

        public sealed class ColumnOption : ObservableObject
        {
            private bool _isSelected;

            public ColumnOption(string id, string label, bool isSelected)
            {
                Id = id;
                Label = label;
                _isSelected = isSelected;
            }

            public string Id { get; }
            public string Label { get; }

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (SetProperty(ref _isSelected, value))
                    {
                        Changed?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            public event EventHandler? Changed;
        }

        public sealed class SearchResultSummary
        {
            public SearchResultSummary(int index, int row, string column, string preview, int matchCount)
            {
                Index = index;
                Row = row;
                Column = column;
                Preview = preview;
                MatchCount = matchCount;
            }

            public int Index { get; }
            public int Row { get; }
            public string Column { get; }
            public string Preview { get; }
            public int MatchCount { get; }
        }
    }
}
