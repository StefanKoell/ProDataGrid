// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable disable

using System;
using System.Collections.Generic;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Controls.DataGridSearching;

namespace Avalonia.Controls
{
    internal sealed class DataGridSearchTextBlock : TextBlock
    {
        public static readonly DirectProperty<DataGridSearchTextBlock, IReadOnlyList<SearchMatch>> SearchMatchesProperty =
            AvaloniaProperty.RegisterDirect<DataGridSearchTextBlock, IReadOnlyList<SearchMatch>>(
                nameof(SearchMatches),
                o => o.SearchMatches,
                (o, v) => o.SearchMatches = v);

        public static readonly DirectProperty<DataGridSearchTextBlock, string> SearchTextProperty =
            AvaloniaProperty.RegisterDirect<DataGridSearchTextBlock, string>(
                nameof(SearchText),
                o => o.SearchText,
                (o, v) => o.SearchText = v);

        public static readonly DirectProperty<DataGridSearchTextBlock, bool> IsSearchCurrentProperty =
            AvaloniaProperty.RegisterDirect<DataGridSearchTextBlock, bool>(
                nameof(IsSearchCurrent),
                o => o.IsSearchCurrent,
                (o, v) => o.IsSearchCurrent = v);

        public static readonly DirectProperty<DataGridSearchTextBlock, SearchHighlightMode> HighlightModeProperty =
            AvaloniaProperty.RegisterDirect<DataGridSearchTextBlock, SearchHighlightMode>(
                nameof(HighlightMode),
                o => o.HighlightMode,
                (o, v) => o.HighlightMode = v);

        private IReadOnlyList<SearchMatch> _searchMatches;
        private string _searchText;
        private bool _isSearchCurrent;
        private SearchHighlightMode _highlightMode;

        public IReadOnlyList<SearchMatch> SearchMatches
        {
            get => _searchMatches;
            set
            {
                if (!ReferenceEquals(_searchMatches, value))
                {
                    SetAndRaise(SearchMatchesProperty, ref _searchMatches, value);
                    UpdateInlines();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    SetAndRaise(SearchTextProperty, ref _searchText, value);
                    UpdateInlines();
                }
            }
        }

        public bool IsSearchCurrent
        {
            get => _isSearchCurrent;
            set
            {
                if (_isSearchCurrent != value)
                {
                    SetAndRaise(IsSearchCurrentProperty, ref _isSearchCurrent, value);
                    UpdateInlines();
                }
            }
        }

        public SearchHighlightMode HighlightMode
        {
            get => _highlightMode;
            set
            {
                if (_highlightMode != value)
                {
                    SetAndRaise(HighlightModeProperty, ref _highlightMode, value);
                    UpdateInlines();
                }
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TextProperty)
            {
                UpdateInlines();
            }
        }

        private void UpdateInlines()
        {
            if (HighlightMode != SearchHighlightMode.TextAndCell ||
                SearchMatches == null ||
                SearchMatches.Count == 0)
            {
                Inlines?.Clear();
                return;
            }

            var text = SearchText ?? Text ?? string.Empty;
            if (string.IsNullOrEmpty(text))
            {
                Inlines?.Clear();
                return;
            }

            var matchBrush = TryFindBrush("DataGridSearchMatchBrush");
            var currentBrush = TryFindBrush("DataGridSearchCurrentBrush") ?? matchBrush;
            var foregroundBrush = TryFindBrush("DataGridSearchMatchForegroundBrush");
            var highlightBrush = IsSearchCurrent ? currentBrush : matchBrush;

            Inlines?.Clear();

            int lastIndex = 0;
            foreach (var match in SearchMatches)
            {
                if (match == null || match.Length <= 0)
                {
                    continue;
                }

                if (match.Start >= text.Length)
                {
                    break;
                }

                var safeLength = Math.Min(match.Length, text.Length - match.Start);
                if (match.Start > lastIndex)
                {
                    var prefix = text.Substring(lastIndex, match.Start - lastIndex);
                    Inlines?.Add(new Run(prefix));
                }

                var segment = text.Substring(match.Start, safeLength);
                var run = new Run(segment);
                if (highlightBrush != null)
                {
                    run.Background = highlightBrush;
                }

                if (foregroundBrush != null)
                {
                    run.Foreground = foregroundBrush;
                }

                Inlines?.Add(run);
                lastIndex = match.Start + safeLength;
            }

            if (lastIndex < text.Length)
            {
                Inlines?.Add(new Run(text.Substring(lastIndex)));
            }
        }

        private IBrush TryFindBrush(string key)
        {
            if (this.TryFindResource(key, out var value) && value is IBrush brush)
            {
                return brush;
            }

            return null;
        }
    }
}
