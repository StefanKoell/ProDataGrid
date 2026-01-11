// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSearching;
using DataGridSample.Helpers;
using DataGridSample.Mvvm;

namespace DataGridSample.ViewModels
{
    public class SearchModelViewModel : ObservableObject
    {
        private string _query = string.Empty;
        private SearchMatchMode _matchMode = SearchMatchMode.Contains;
        private SearchTermCombineMode _termMode = SearchTermCombineMode.Any;
        private SearchScope _scope = SearchScope.AllColumns;
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

        public SearchModelViewModel()
        {
            Items = new ObservableCollection<SearchItem>(CreateItems(1500));
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

            SearchModel.ResultsChanged += SearchModelOnResultsChanged;
            SearchModel.CurrentChanged += SearchModelOnCurrentChanged;

            NextCommand = new RelayCommand(_ => SearchModel.MoveNext(), _ => SearchModel.Results.Count > 0);
            PreviousCommand = new RelayCommand(_ => SearchModel.MovePrevious(), _ => SearchModel.Results.Count > 0);
            ClearCommand = new RelayCommand(_ => Query = string.Empty);

            ColumnDefinitions = CreateColumnDefinitions();
        }

        public ObservableCollection<SearchItem> Items { get; }

        public DataGridCollectionView View { get; }

        public SearchModel SearchModel { get; }

        public ObservableCollection<DataGridColumnDefinition> ColumnDefinitions { get; }

        public ObservableCollection<SearchResultSummary> Results { get; } = new();

        public RelayCommand NextCommand { get; }

        public RelayCommand PreviousCommand { get; }

        public RelayCommand ClearCommand { get; }

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

        public SearchScope Scope
        {
            get => _scope;
            set
            {
                if (SetProperty(ref _scope, value))
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

        private void ApplySearch()
        {
            if (string.IsNullOrWhiteSpace(_query))
            {
                SearchModel.Clear();
                return;
            }

            var comparison = _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            SearchModel.SetOrUpdate(new SearchDescriptor(
                _query.Trim(),
                matchMode: _matchMode,
                termMode: _termMode,
                scope: _scope,
                comparison: comparison,
                wholeWord: _wholeWord,
                normalizeWhitespace: _normalizeWhitespace,
                ignoreDiacritics: _ignoreDiacritics));
        }

        private static ObservableCollection<DataGridColumnDefinition> CreateColumnDefinitions()
        {
            var createdBinding = ColumnDefinitionBindingFactory.CreateBinding<SearchItem, DateTimeOffset>(nameof(SearchItem.Created), s => s.Created);
            createdBinding.StringFormat = "{0:yyyy-MM-dd}";

            return new ObservableCollection<DataGridColumnDefinition>
            {
                new DataGridTextColumnDefinition
                {
                    Header = "ID",
                    Binding = ColumnDefinitionBindingFactory.CreateBinding<SearchItem, int>(nameof(SearchItem.Id), s => s.Id),
                    SortMemberPath = nameof(SearchItem.Id),
                    Width = new DataGridLength(0.6, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumnDefinition
                {
                    Header = "Title",
                    Binding = ColumnDefinitionBindingFactory.CreateBinding<SearchItem, string>(nameof(SearchItem.Title), s => s.Title),
                    SortMemberPath = nameof(SearchItem.Title),
                    Width = new DataGridLength(1.4, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumnDefinition
                {
                    Header = "Owner",
                    Binding = ColumnDefinitionBindingFactory.CreateBinding<SearchItem, string>(nameof(SearchItem.Owner), s => s.Owner),
                    SortMemberPath = nameof(SearchItem.Owner),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumnDefinition
                {
                    Header = "Category",
                    Binding = ColumnDefinitionBindingFactory.CreateBinding<SearchItem, string>(nameof(SearchItem.Category), s => s.Category),
                    SortMemberPath = nameof(SearchItem.Category),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumnDefinition
                {
                    Header = "Status",
                    Binding = ColumnDefinitionBindingFactory.CreateBinding<SearchItem, string>(nameof(SearchItem.Status), s => s.Status),
                    SortMemberPath = nameof(SearchItem.Status),
                    Width = new DataGridLength(0.9, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumnDefinition
                {
                    Header = "Score",
                    Binding = ColumnDefinitionBindingFactory.CreateBinding<SearchItem, int>(nameof(SearchItem.Score), s => s.Score),
                    SortMemberPath = nameof(SearchItem.Score),
                    Width = new DataGridLength(0.7, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumnDefinition
                {
                    Header = "Created",
                    Binding = createdBinding,
                    SortMemberPath = nameof(SearchItem.Created),
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                },
                new DataGridTextColumnDefinition
                {
                    Header = "Notes",
                    Binding = ColumnDefinitionBindingFactory.CreateBinding<SearchItem, string>(nameof(SearchItem.Notes), s => s.Notes),
                    SortMemberPath = nameof(SearchItem.Notes),
                    Width = new DataGridLength(1.6, DataGridLengthUnitType.Star)
                }
            };
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

        private static ObservableCollection<SearchItem> CreateItems(int count)
        {
            var random = new Random(17);
            var owners = new[] { "Alex", "Brett", "Casey", "Devon", "Elliot", "Finley", "Harper" };
            var categories = new[] { "Platform", "Billing", "Analytics", "Growth", "Operations" };
            var statuses = new[] { "New", "Active", "Blocked", "On Hold", "Done" };
            var verbs = new[] { "Review", "Audit", "Launch", "Migrate", "Refactor", "Optimize", "Ship" };
            var nouns = new[] { "Search", "Payments", "Catalog", "Identity", "Mobile", "Ads", "Insights" };

            var items = new ObservableCollection<SearchItem>();
            var baseDate = DateTimeOffset.UtcNow.Date;

            for (int i = 1; i <= count; i++)
            {
                var owner = owners[random.Next(owners.Length)];
                var category = categories[random.Next(categories.Length)];
                var status = statuses[random.Next(statuses.Length)];
                var title = $"{verbs[random.Next(verbs.Length)]} {nouns[random.Next(nouns.Length)]} #{i}";
                var notes = $"{status} work item for {category} owned by {owner}.";
                var score = random.Next(55, 100);
                var created = baseDate.AddDays(-random.Next(0, 365));

                items.Add(new SearchItem(i, title, owner, category, status, score, created, notes));
            }

            return items;
        }

        public sealed class SearchItem
        {
            public SearchItem(int id, string title, string owner, string category, string status, int score, DateTimeOffset created, string notes)
            {
                Id = id;
                Title = title;
                Owner = owner;
                Category = category;
                Status = status;
                Score = score;
                Created = created;
                Notes = notes;
            }

            public int Id { get; }
            public string Title { get; }
            public string Owner { get; }
            public string Category { get; }
            public string Status { get; }
            public int Score { get; }
            public DateTimeOffset Created { get; }
            public string Notes { get; }
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
