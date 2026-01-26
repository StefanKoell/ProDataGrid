// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Avalonia.Controls.DataGridPivoting
{
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum PivotChartSeriesSource
    {
        Rows,
        Columns
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotChartSeries
    {
        public PivotChartSeries(string? name, IReadOnlyList<double?> values, object? source)
        {
            Name = name;
            Values = values;
            Source = source;
        }

        public string? Name { get; }

        public IReadOnlyList<double?> Values { get; }

        public object? Source { get; }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotChartChangedEventArgs : EventArgs
    {
        public PivotChartChangedEventArgs(IReadOnlyList<string?> categories, IReadOnlyList<PivotChartSeries> series)
        {
            Categories = categories;
            Series = series;
        }

        public IReadOnlyList<string?> Categories { get; }

        public IReadOnlyList<PivotChartSeries> Series { get; }
    }

#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class PivotChartModel : INotifyPropertyChanged, IDisposable
    {
        private PivotTableModel? _pivot;
        private bool _autoRefresh = true;
        private int _updateNesting;
        private bool _pendingRefresh;
        private bool _isRefreshing;
        private PivotChartSeriesSource _seriesSource = PivotChartSeriesSource.Rows;
        private bool _includeSubtotals;
        private bool _includeGrandTotals;
        private bool _includeEmptySeries;
        private PivotValueField? _valueField;
        private CultureInfo _culture = CultureInfo.CurrentCulture;

        public PivotChartModel()
        {
            Categories = new PivotObservableCollection<string?>();
            Series = new PivotObservableCollection<PivotChartSeries>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<PivotChartChangedEventArgs>? ChartChanged;

        public PivotObservableCollection<string?> Categories { get; }

        public PivotObservableCollection<PivotChartSeries> Series { get; }

        public PivotTableModel? Pivot
        {
            get => _pivot;
            set
            {
                if (ReferenceEquals(_pivot, value))
                {
                    return;
                }

                DetachPivot();
                _pivot = value;
                AttachPivot();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pivot)));
                RequestRefresh();
            }
        }

        public PivotChartSeriesSource SeriesSource
        {
            get => _seriesSource;
            set
            {
                if (_seriesSource == value)
                {
                    return;
                }

                _seriesSource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SeriesSource)));
                RequestRefresh();
            }
        }

        public bool IncludeSubtotals
        {
            get => _includeSubtotals;
            set
            {
                if (_includeSubtotals == value)
                {
                    return;
                }

                _includeSubtotals = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncludeSubtotals)));
                RequestRefresh();
            }
        }

        public bool IncludeGrandTotals
        {
            get => _includeGrandTotals;
            set
            {
                if (_includeGrandTotals == value)
                {
                    return;
                }

                _includeGrandTotals = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncludeGrandTotals)));
                RequestRefresh();
            }
        }

        public bool IncludeEmptySeries
        {
            get => _includeEmptySeries;
            set
            {
                if (_includeEmptySeries == value)
                {
                    return;
                }

                _includeEmptySeries = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncludeEmptySeries)));
                RequestRefresh();
            }
        }

        public PivotValueField? ValueField
        {
            get => _valueField;
            set
            {
                if (ReferenceEquals(_valueField, value))
                {
                    return;
                }

                _valueField = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueField)));
                RequestRefresh();
            }
        }

        public CultureInfo Culture
        {
            get => _culture;
            set
            {
                if (Equals(_culture, value))
                {
                    return;
                }

                _culture = value ?? CultureInfo.CurrentCulture;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Culture)));
                RequestRefresh();
            }
        }

        public Func<PivotRow, string?>? RowLabelSelector { get; set; }

        public Func<PivotColumn, string?>? ColumnLabelSelector { get; set; }

        public bool AutoRefresh
        {
            get => _autoRefresh;
            set
            {
                if (_autoRefresh == value)
                {
                    return;
                }

                _autoRefresh = value;
                if (_autoRefresh && _pendingRefresh)
                {
                    Refresh();
                }
            }
        }

        public void Refresh()
        {
            if (_isRefreshing)
            {
                _pendingRefresh = true;
                return;
            }

            do
            {
                _pendingRefresh = false;
                _isRefreshing = true;
                try
                {
                    BuildChart();
                }
                finally
                {
                    _isRefreshing = false;
                }
            }
            while (_pendingRefresh && _autoRefresh && _updateNesting == 0);
        }

        public IDisposable DeferRefresh()
        {
            BeginUpdate();
            return new UpdateScope(this);
        }

        public void BeginUpdate()
        {
            _updateNesting++;
        }

        public void EndUpdate()
        {
            if (_updateNesting == 0)
            {
                throw new InvalidOperationException("EndUpdate called without matching BeginUpdate.");
            }

            _updateNesting--;
            if (_updateNesting == 0 && _pendingRefresh)
            {
                Refresh();
            }
        }

        public void Dispose()
        {
            DetachPivot();
        }

        private void RequestRefresh()
        {
            if (_isRefreshing)
            {
                _pendingRefresh = true;
                return;
            }

            if (!AutoRefresh)
            {
                _pendingRefresh = true;
                return;
            }

            if (_updateNesting > 0)
            {
                _pendingRefresh = true;
                return;
            }

            Refresh();
        }

        private void AttachPivot()
        {
            if (_pivot == null)
            {
                return;
            }

            _pivot.PivotChanged += Pivot_PivotChanged;
        }

        private void DetachPivot()
        {
            if (_pivot == null)
            {
                return;
            }

            _pivot.PivotChanged -= Pivot_PivotChanged;
        }

        private void Pivot_PivotChanged(object? sender, PivotChangedEventArgs e)
        {
            RequestRefresh();
        }

        private void BuildChart()
        {
            if (_pivot == null)
            {
                Categories.ResetWith(Array.Empty<string?>());
                Series.ResetWith(Array.Empty<PivotChartSeries>());
                return;
            }

            var valuesInRows = _pivot.Layout.ValuesPosition == PivotValuesPosition.Rows;
            var rowItems = _pivot.Rows.Where(IsRowIncluded).ToList();
            var columnItems = _pivot.Columns.Where(IsColumnIncluded).ToList();

            if (_valueField != null)
            {
                if (valuesInRows)
                {
                    rowItems = rowItems
                        .Where(row => MatchesValueField(row.ValueField, _valueField))
                        .ToList();
                }
                else
                {
                    columnItems = columnItems
                        .Where(column => MatchesValueField(column.ValueField, _valueField))
                        .ToList();
                }
            }

            if (_seriesSource == PivotChartSeriesSource.Rows)
            {
                BuildSeriesFromRows(rowItems, columnItems, valuesInRows);
            }
            else
            {
                BuildSeriesFromColumns(rowItems, columnItems, valuesInRows);
            }
        }

        private void BuildSeriesFromRows(IReadOnlyList<PivotRow> rows, IReadOnlyList<PivotColumn> columns, bool valuesInRows)
        {
            var categoryLabels = columns.Select(BuildColumnLabel).ToList();
            Categories.ResetWith(categoryLabels);

            var columnIndexes = columns.Select(column => column.Index).ToList();
            var series = new List<PivotChartSeries>(rows.Count);

            foreach (var row in rows)
            {
                var values = new double?[columnIndexes.Count];
                var hasValue = false;
                for (var i = 0; i < columnIndexes.Count; i++)
                {
                    var columnIndex = columnIndexes[i];
                    if (columnIndex < 0 || columnIndex >= row.CellValues.Length)
                    {
                        continue;
                    }

                    var raw = row.CellValues[columnIndex];
                    var numeric = ToNumeric(raw, row.ValueField);
                    values[i] = numeric;
                    hasValue |= numeric.HasValue;
                }

                if (!hasValue && !_includeEmptySeries)
                {
                    continue;
                }

                var name = BuildRowLabel(row, valuesInRows);
                series.Add(new PivotChartSeries(name, values, row));
            }

            Series.ResetWith(series);
            ChartChanged?.Invoke(this, new PivotChartChangedEventArgs(Categories, Series));
        }

        private void BuildSeriesFromColumns(IReadOnlyList<PivotRow> rows, IReadOnlyList<PivotColumn> columns, bool valuesInRows)
        {
            var categoryLabels = rows.Select(row => BuildRowLabel(row, valuesInRows)).ToList();
            Categories.ResetWith(categoryLabels);

            var series = new List<PivotChartSeries>(columns.Count);
            foreach (var column in columns)
            {
                var values = new double?[rows.Count];
                var hasValue = false;
                for (var i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (column.Index < 0 || column.Index >= row.CellValues.Length)
                    {
                        continue;
                    }

                    var raw = row.CellValues[column.Index];
                    var numeric = ToNumeric(raw, valuesInRows ? row.ValueField : column.ValueField);
                    values[i] = numeric;
                    hasValue |= numeric.HasValue;
                }

                if (!hasValue && !_includeEmptySeries)
                {
                    continue;
                }

                var name = BuildColumnLabel(column);
                series.Add(new PivotChartSeries(name, values, column));
            }

            Series.ResetWith(series);
            ChartChanged?.Invoke(this, new PivotChartChangedEventArgs(Categories, Series));
        }

        private bool IsRowIncluded(PivotRow row)
        {
            return row.RowType switch
            {
                PivotRowType.Detail => true,
                PivotRowType.Subtotal => _includeSubtotals,
                PivotRowType.GrandTotal => _includeGrandTotals,
                _ => false
            };
        }

        private bool IsColumnIncluded(PivotColumn column)
        {
            return column.ColumnType switch
            {
                PivotColumnType.Detail => true,
                PivotColumnType.Subtotal => _includeSubtotals,
                PivotColumnType.GrandTotal => _includeGrandTotals,
                _ => false
            };
        }

        private static bool MatchesValueField(PivotValueField? candidate, PivotValueField target)
        {
            if (candidate == null)
            {
                return false;
            }

            if (ReferenceEquals(candidate, target))
            {
                return true;
            }

            if (candidate.Key != null && target.Key != null)
            {
                return Equals(candidate.Key, target.Key);
            }

            if (!string.IsNullOrEmpty(candidate.Header) && !string.IsNullOrEmpty(target.Header))
            {
                return string.Equals(candidate.Header, target.Header, StringComparison.Ordinal);
            }

            return false;
        }

        private string? BuildRowLabel(PivotRow row, bool valuesInRows)
        {
            if (RowLabelSelector != null)
            {
                return RowLabelSelector(row);
            }

            if (!string.IsNullOrEmpty(row.CompactLabel))
            {
                return row.CompactLabel;
            }

            var label = JoinPathValues(row.RowPathValues);
            if (valuesInRows && row.ValueField != null)
            {
                var valueLabel = row.ValueField.Header ?? row.ValueField.Key?.ToString() ?? "Value";
                label = string.IsNullOrEmpty(label) ? valueLabel : string.Concat(label, " / ", valueLabel);
            }

            return label;
        }

        private string? BuildColumnLabel(PivotColumn column)
        {
            if (ColumnLabelSelector != null)
            {
                return ColumnLabelSelector(column);
            }

            if (column.Header?.Segments != null && column.Header.Segments.Count > 0)
            {
                return JoinSegments(column.Header.Segments);
            }

            if (column.ColumnDisplayValues.Length > 0)
            {
                return JoinPathValues(column.ColumnDisplayValues);
            }

            if (column.ValueField != null)
            {
                return column.ValueField.Header ?? column.ValueField.Key?.ToString();
            }

            return null;
        }

        private string? JoinPathValues(object?[] values)
        {
            if (values.Length == 0)
            {
                return null;
            }

            var parts = new List<string>();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (value == null)
                {
                    continue;
                }

                var text = Convert.ToString(value, _culture) ?? string.Empty;
                if (text.Length > 0)
                {
                    parts.Add(text);
                }
            }

            return parts.Count == 0 ? null : string.Join(" / ", parts);
        }

        private static string? JoinSegments(IReadOnlyList<string> segments)
        {
            if (segments.Count == 0)
            {
                return null;
            }

            var parts = new List<string>(segments.Count);
            foreach (var segment in segments)
            {
                if (!string.IsNullOrEmpty(segment))
                {
                    parts.Add(segment);
                }
            }

            return parts.Count == 0 ? null : string.Join(" / ", parts);
        }

        private double? ToNumeric(object? value, PivotValueField? valueField)
        {
            if (PivotNumeric.TryGetDouble(value, out var number))
            {
                return number;
            }

            if (value is string text)
            {
                var numberFormat = GetNumberFormat(valueField);
                if (double.TryParse(text, NumberStyles.Any, numberFormat, out number))
                {
                    return number;
                }

                if (TryParsePercent(text, numberFormat, out number))
                {
                    return number;
                }
            }

            return null;
        }

        private NumberFormatInfo GetNumberFormat(PivotValueField? valueField)
        {
            if (valueField?.FormatProvider is CultureInfo cultureInfo)
            {
                return cultureInfo.NumberFormat;
            }

            if (valueField?.FormatProvider is NumberFormatInfo numberFormat)
            {
                return numberFormat;
            }

            return _culture.NumberFormat;
        }

        private static bool TryParsePercent(string text, NumberFormatInfo numberFormat, out double value)
        {
            value = 0d;
            var symbol = numberFormat.PercentSymbol;
            var hasSymbol = !string.IsNullOrEmpty(symbol) && text.IndexOf(symbol, StringComparison.Ordinal) >= 0;
            if (!hasSymbol && text.IndexOf("%", StringComparison.Ordinal) >= 0)
            {
                symbol = "%";
                hasSymbol = true;
            }

            if (!hasSymbol)
            {
                return false;
            }

            var cleaned = text.Replace(symbol, string.Empty).Trim();
            if (!double.TryParse(cleaned, NumberStyles.Any, numberFormat, out value))
            {
                return false;
            }

            value /= 100d;
            return true;
        }

        private sealed class UpdateScope : IDisposable
        {
            private PivotChartModel? _owner;

            public UpdateScope(PivotChartModel owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_owner == null)
                {
                    return;
                }

                _owner.EndUpdate();
                _owner = null;
            }
        }
    }
}
