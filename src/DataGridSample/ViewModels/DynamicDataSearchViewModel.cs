// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Avalonia.Controls;
using Avalonia.Controls.DataGridSearching;
using DataGridSample.Adapters;
using DataGridSample.Models;
using DataGridSample.Mvvm;
using DynamicData;

namespace DataGridSample.ViewModels
{
    /// <summary>
    /// Demonstrates wiring SearchModel to a DynamicData pipeline via a custom adapter factory.
    /// </summary>
    public class DynamicDataSearchViewModel : ObservableObject, IDisposable
    {
        private readonly ReadOnlyObservableCollection<Deployment> _view;
        private readonly SourceList<Deployment> _source;
        private readonly CompositeDisposable _cleanup = new();
        private readonly BehaviorSubject<Func<Deployment, bool>> _searchSubject;
        private readonly DynamicDataSearchAdapterFactory _adapterFactory;
        private string _query = string.Empty;
        private SearchMatchMode _matchMode = SearchMatchMode.Contains;
        private SearchTermCombineMode _termMode = SearchTermCombineMode.Any;
        private SearchHighlightMode _highlightMode = SearchHighlightMode.TextAndCell;
        private bool _highlightCurrent = true;
        private bool _wrapNavigation = true;
        private bool _updateSelectionOnNavigate = true;
        private bool _caseSensitive;
        private bool _wholeWord;
        private bool _ignoreDiacritics;
        private bool _normalizeWhitespace = true;
        private int _resultCount;
        private int _currentResultIndex;
        private SearchResultSummary? _selectedResult;
        private bool _suppressSelectionUpdate;

        public DynamicDataSearchViewModel()
        {
            _source = new SourceList<Deployment>();
            _source.AddRange(CreateDeployments(1500));

            _adapterFactory = new DynamicDataSearchAdapterFactory(OnUpstreamSearchChanged);
            _searchSubject = new BehaviorSubject<Func<Deployment, bool>>(_adapterFactory.SearchPredicate);

            var subscription = _source.Connect()
                .Filter(_searchSubject)
                .Bind(out _view)
                .Subscribe();
            _cleanup.Add(subscription);

            SearchModel = new SearchModel
            {
                HighlightMode = _highlightMode,
                HighlightCurrent = _highlightCurrent,
                WrapNavigation = _wrapNavigation,
                UpdateSelectionOnNavigate = _updateSelectionOnNavigate
            };

            SearchModel.SearchChanged += SearchModelOnSearchChanged;
            SearchModel.ResultsChanged += SearchModelOnResultsChanged;
            SearchModel.CurrentChanged += SearchModelOnCurrentChanged;

            NextCommand = new RelayCommand(_ => SearchModel.MoveNext(), _ => SearchModel.Results.Count > 0);
            PreviousCommand = new RelayCommand(_ => SearchModel.MovePrevious(), _ => SearchModel.Results.Count > 0);
            ClearCommand = new RelayCommand(_ => Query = string.Empty);
        }

        public ReadOnlyObservableCollection<Deployment> View => _view;

        public SearchModel SearchModel { get; }

        public DynamicDataSearchAdapterFactory AdapterFactory => _adapterFactory;

        public ObservableCollection<string> UpstreamSearches { get; } = new();

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
                scope: SearchScope.AllColumns,
                comparison: comparison,
                wholeWord: _wholeWord,
                normalizeWhitespace: _normalizeWhitespace,
                ignoreDiacritics: _ignoreDiacritics));
        }

        private void SearchModelOnSearchChanged(object? sender, SearchChangedEventArgs e)
        {
            _adapterFactory.UpdatePredicate(e.NewDescriptors);
            _searchSubject.OnNext(_adapterFactory.SearchPredicate);
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

        private void OnUpstreamSearchChanged(string description)
        {
            UpstreamSearches.Insert(0, $"{DateTime.Now:HH:mm:ss} {description}");
            while (UpstreamSearches.Count > 20)
            {
                UpstreamSearches.RemoveAt(UpstreamSearches.Count - 1);
            }
        }

        private static ObservableCollection<Deployment> CreateDeployments(int count)
        {
            var seed = Deployment.CreateSeed().ToArray();
            var random = new Random(37);
            var baseDate = DateTimeOffset.UtcNow.Date;

            var items = new ObservableCollection<Deployment>();
            for (int i = 0; i < count; i++)
            {
                var template = seed[i % seed.Length];
                var service = $"{template.Service}-{i + 1:0000}";
                var started = baseDate.AddDays(-random.Next(0, 90)).AddMinutes(-random.Next(0, 600));
                var errorRate = Math.Max(0, template.ErrorRate + (random.NextDouble() - 0.5) * 0.02);
                var incidents = Math.Max(0, template.Incidents + random.Next(-1, 3));

                items.Add(new Deployment(
                    service,
                    template.Region,
                    template.Ring,
                    template.Status,
                    started,
                    Math.Round(errorRate, 3),
                    incidents));
            }

            return items;
        }

        public void Dispose()
        {
            _searchSubject.Dispose();
            _cleanup.Dispose();
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
