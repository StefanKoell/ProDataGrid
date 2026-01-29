// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProCharts
{
    public sealed class ChartDataRequest : INotifyPropertyChanged
    {
        private int? _maxPoints;
        private ChartDownsampleMode _downsampleMode = ChartDownsampleMode.Adaptive;
        private int? _windowStart;
        private int? _windowCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int? MaxPoints
        {
            get => _maxPoints;
            set
            {
                if (_maxPoints == value)
                {
                    return;
                }

                _maxPoints = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxPoints)));
            }
        }

        public ChartDownsampleMode DownsampleMode
        {
            get => _downsampleMode;
            set
            {
                if (_downsampleMode == value)
                {
                    return;
                }

                _downsampleMode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownsampleMode)));
            }
        }

        public int? WindowStart
        {
            get => _windowStart;
            set
            {
                if (_windowStart == value)
                {
                    return;
                }

                _windowStart = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowStart)));
            }
        }

        public int? WindowCount
        {
            get => _windowCount;
            set
            {
                if (_windowCount == value)
                {
                    return;
                }

                _windowCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WindowCount)));
            }
        }
    }

    public sealed class ChartAxisDefinition : INotifyPropertyChanged
    {
        private ChartAxisKind _kind;
        private string? _title;
        private double? _minimum;
        private double? _maximum;
        private bool _isVisible = true;
        private Func<double, string>? _labelFormatter;
        private ChartAxisCrossing _crossing = ChartAxisCrossing.Auto;
        private double? _crossingValue;
        private float _offset;
        private int _minorTickCount = 4;
        private bool _showMinorTicks;
        private bool _showMinorGridlines;

        public ChartAxisDefinition(ChartAxisKind kind)
        {
            _kind = kind;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ChartAxisKind Kind
        {
            get => _kind;
            set
            {
                if (_kind == value)
                {
                    return;
                }

                _kind = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Kind)));
            }
        }

        public string? Title
        {
            get => _title;
            set
            {
                if (_title == value)
                {
                    return;
                }

                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        public double? Minimum
        {
            get => _minimum;
            set
            {
                if (_minimum == value)
                {
                    return;
                }

                _minimum = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Minimum)));
            }
        }

        public double? Maximum
        {
            get => _maximum;
            set
            {
                if (_maximum == value)
                {
                    return;
                }

                _maximum = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Maximum)));
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                _isVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }

        public Func<double, string>? LabelFormatter
        {
            get => _labelFormatter;
            set
            {
                if (_labelFormatter == value)
                {
                    return;
                }

                _labelFormatter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LabelFormatter)));
            }
        }

        public ChartAxisCrossing Crossing
        {
            get => _crossing;
            set
            {
                if (_crossing == value)
                {
                    return;
                }

                _crossing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Crossing)));
            }
        }

        public double? CrossingValue
        {
            get => _crossingValue;
            set
            {
                if (_crossingValue == value)
                {
                    return;
                }

                _crossingValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CrossingValue)));
            }
        }

        public float Offset
        {
            get => _offset;
            set
            {
                if (Math.Abs(_offset - value) < float.Epsilon)
                {
                    return;
                }

                _offset = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Offset)));
            }
        }

        public int MinorTickCount
        {
            get => _minorTickCount;
            set
            {
                var count = value < 0 ? 0 : value;
                if (_minorTickCount == count)
                {
                    return;
                }

                _minorTickCount = count;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinorTickCount)));
            }
        }

        public bool ShowMinorTicks
        {
            get => _showMinorTicks;
            set
            {
                if (_showMinorTicks == value)
                {
                    return;
                }

                _showMinorTicks = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowMinorTicks)));
            }
        }

        public bool ShowMinorGridlines
        {
            get => _showMinorGridlines;
            set
            {
                if (_showMinorGridlines == value)
                {
                    return;
                }

                _showMinorGridlines = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowMinorGridlines)));
            }
        }
    }

    public sealed class ChartLegendDefinition : INotifyPropertyChanged
    {
        private ChartLegendPosition _position = ChartLegendPosition.Right;
        private bool _isVisible = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ChartLegendPosition Position
        {
            get => _position;
            set
            {
                if (_position == value)
                {
                    return;
                }

                _position = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Position)));
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                _isVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }
    }

    public sealed class ChartSeriesSnapshot
    {
        public ChartSeriesSnapshot(
            string? name,
            ChartSeriesKind kind,
            IReadOnlyList<double?> values,
            IReadOnlyList<double>? xValues = null,
            IReadOnlyList<double?>? sizeValues = null,
            Func<double, string>? dataLabelFormatter = null,
            ChartValueAxisAssignment valueAxisAssignment = ChartValueAxisAssignment.Primary,
            ChartTrendlineType trendlineType = ChartTrendlineType.None,
            int trendlinePeriod = 2,
            ChartErrorBarType errorBarType = ChartErrorBarType.None,
            double errorBarValue = 1d,
            ChartSeriesStyle? style = null)
        {
            Name = name;
            Kind = kind;
            Values = values ?? throw new ArgumentNullException(nameof(values));
            XValues = xValues;
            SizeValues = sizeValues;
            DataLabelFormatter = dataLabelFormatter;
            ValueAxisAssignment = valueAxisAssignment;
            TrendlineType = trendlineType;
            TrendlinePeriod = trendlinePeriod;
            ErrorBarType = errorBarType;
            ErrorBarValue = errorBarValue;
            Style = style;
        }

        public string? Name { get; }

        public ChartSeriesKind Kind { get; }

        public IReadOnlyList<double?> Values { get; }

        public IReadOnlyList<double>? XValues { get; }

        public IReadOnlyList<double?>? SizeValues { get; }

        public Func<double, string>? DataLabelFormatter { get; }

        public ChartValueAxisAssignment ValueAxisAssignment { get; }

        public ChartTrendlineType TrendlineType { get; }

        public int TrendlinePeriod { get; }

        public ChartErrorBarType ErrorBarType { get; }

        public double ErrorBarValue { get; }

        public ChartSeriesStyle? Style { get; }
    }

    public sealed class ChartDataSnapshot
    {
        public ChartDataSnapshot(
            IReadOnlyList<string?> categories,
            IReadOnlyList<ChartSeriesSnapshot> series)
            : this(categories, series, 0)
        {
        }

        public ChartDataSnapshot(
            IReadOnlyList<string?> categories,
            IReadOnlyList<ChartSeriesSnapshot> series,
            int version)
        {
            Categories = categories ?? throw new ArgumentNullException(nameof(categories));
            Series = series ?? throw new ArgumentNullException(nameof(series));
            Version = version;
        }

        public static ChartDataSnapshot Empty { get; } =
            new ChartDataSnapshot(Array.Empty<string?>(), Array.Empty<ChartSeriesSnapshot>());

        public IReadOnlyList<string?> Categories { get; }

        public IReadOnlyList<ChartSeriesSnapshot> Series { get; }

        public int Version { get; }
    }

    public interface IChartDataSource
    {
        event EventHandler? DataInvalidated;

        ChartDataSnapshot BuildSnapshot(ChartDataRequest request);
    }

    public interface IChartWindowInfoProvider
    {
        int? GetTotalCategoryCount();
    }

    public interface IChartIncrementalDataSource : IChartDataSource
    {
        bool TryBuildUpdate(ChartDataRequest request, ChartDataSnapshot previousSnapshot, out ChartDataUpdate update);
    }
}
