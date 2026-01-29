// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using ProCharts;
using SkiaSharp;

namespace ProCharts.Skia
{
    internal enum SkiaChartDataSegmentKind
    {
        Full,
        Series,
        StackedColumnPrimary,
        StackedColumnSecondary,
        StackedColumn100Primary,
        StackedColumn100Secondary,
        StackedBarPrimary,
        StackedBarSecondary,
        StackedBar100Primary,
        StackedBar100Secondary,
        StackedAreaPrimary,
        StackedAreaSecondary,
        StackedArea100Primary,
        StackedArea100Secondary,
        Trendlines,
        ErrorBars
    }

    internal readonly struct SkiaChartDataSegmentKey : IEquatable<SkiaChartDataSegmentKey>
    {
        public SkiaChartDataSegmentKey(SkiaChartDataSegmentKind kind, int seriesIndex)
        {
            Kind = kind;
            SeriesIndex = seriesIndex;
        }

        public SkiaChartDataSegmentKind Kind { get; }
        public int SeriesIndex { get; }

        public bool Equals(SkiaChartDataSegmentKey other)
        {
            return Kind == other.Kind && SeriesIndex == other.SeriesIndex;
        }

        public override bool Equals(object? obj)
        {
            return obj is SkiaChartDataSegmentKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) + Kind.GetHashCode();
                hash = (hash * 31) + SeriesIndex;
                return hash;
            }
        }
    }

    internal sealed class SkiaChartRenderState
    {
        public SkiaChartRenderState(
            int renderKind,
            bool barOnly,
            bool useNumericCategoryAxis,
            bool hasSecondaryRange,
            int seriesCount,
            int categoryCount,
            int seriesLayoutHash,
            double minValue,
            double maxValue,
            double minSecondaryValue,
            double maxSecondaryValue,
            double minCategory,
            double maxCategory,
            int categoriesHash,
            int legendHash,
            SKRect plot,
            SKRect? legendRect)
        {
            RenderKind = renderKind;
            BarOnly = barOnly;
            UseNumericCategoryAxis = useNumericCategoryAxis;
            HasSecondaryRange = hasSecondaryRange;
            SeriesCount = seriesCount;
            CategoryCount = categoryCount;
            SeriesLayoutHash = seriesLayoutHash;
            MinValue = minValue;
            MaxValue = maxValue;
            MinSecondaryValue = minSecondaryValue;
            MaxSecondaryValue = maxSecondaryValue;
            MinCategory = minCategory;
            MaxCategory = maxCategory;
            CategoriesHash = categoriesHash;
            LegendHash = legendHash;
            Plot = plot;
            LegendRect = legendRect;
        }

        public int RenderKind { get; }

        public bool BarOnly { get; }

        public bool UseNumericCategoryAxis { get; }

        public bool HasSecondaryRange { get; }

        public int SeriesCount { get; }

        public int CategoryCount { get; }

        public int SeriesLayoutHash { get; }

        public double MinValue { get; }

        public double MaxValue { get; }

        public double MinSecondaryValue { get; }

        public double MaxSecondaryValue { get; }

        public double MinCategory { get; }

        public double MaxCategory { get; }

        public int CategoriesHash { get; }

        public int LegendHash { get; }

        public SKRect Plot { get; }

        public SKRect? LegendRect { get; }

        public bool MatchesBase(SkiaChartRenderState other)
        {
            return RenderKind == other.RenderKind &&
                   BarOnly == other.BarOnly &&
                   UseNumericCategoryAxis == other.UseNumericCategoryAxis &&
                   HasSecondaryRange == other.HasSecondaryRange &&
                   SeriesCount == other.SeriesCount &&
                   CategoryCount == other.CategoryCount &&
                   SeriesLayoutHash == other.SeriesLayoutHash &&
                   MinValue.Equals(other.MinValue) &&
                   MaxValue.Equals(other.MaxValue) &&
                   MinSecondaryValue.Equals(other.MinSecondaryValue) &&
                   MaxSecondaryValue.Equals(other.MaxSecondaryValue) &&
                   MinCategory.Equals(other.MinCategory) &&
                   MaxCategory.Equals(other.MaxCategory) &&
                   CategoriesHash == other.CategoriesHash &&
                   Plot.Equals(other.Plot);
        }

        public bool MatchesLegend(SkiaChartRenderState other)
        {
            if (RenderKind != other.RenderKind)
            {
                return false;
            }

            if (LegendHash != other.LegendHash)
            {
                return false;
            }

            if (LegendRect.HasValue != other.LegendRect.HasValue)
            {
                return false;
            }

            return !LegendRect.HasValue || LegendRect.Value.Equals(other.LegendRect!.Value);
        }
    }

    internal sealed class SkiaChartLabelSegment
    {
        public SkiaChartLabelSegment(SKPicture picture, SKRect[] placements)
        {
            Picture = picture;
            Placements = placements;
        }

        public SKPicture Picture { get; }

        public SKRect[] Placements { get; }
    }

    public sealed class SkiaChartRenderCache : IDisposable
    {
        private SKPicture? _axesPicture;
        private SKPicture? _dataPicture;
        private SKPicture? _axisTextPicture;
        private SKPicture? _legendPicture;
        private SKRect _bounds;
        private int _styleHash;
        private int _snapshotVersion;
        private ChartDataSnapshot? _snapshot;
        private SkiaChartRenderState? _state;
        private Dictionary<SkiaChartDataSegmentKey, SKPicture>? _dataSegments;
        private Dictionary<SkiaChartDataSegmentKey, SkiaChartLabelSegment>? _dataLabelSegments;

        public bool TryDraw(SKCanvas canvas, SKRect bounds, ChartDataSnapshot snapshot, int styleHash)
        {
            if (_axesPicture == null && _dataPicture == null && _axisTextPicture == null && _legendPicture == null && _dataLabelSegments == null && _dataSegments == null)
            {
                return false;
            }

            if (!_bounds.Equals(bounds) || _styleHash != styleHash)
            {
                return false;
            }

            if (_dataPicture != null || _axisTextPicture != null || _dataLabelSegments != null || _dataSegments != null)
            {
                if (snapshot.Version != 0)
                {
                    if (_snapshotVersion != snapshot.Version)
                    {
                        return false;
                    }
                }
                else if (!ReferenceEquals(_snapshot, snapshot))
                {
                    return false;
                }
            }

            DrawLayers(canvas);
            return true;
        }

        public void Store(SKRect bounds, ChartDataSnapshot snapshot, int styleHash, SKPicture picture)
        {
            _axesPicture?.Dispose();
            _dataPicture?.Dispose();
            _axisTextPicture?.Dispose();
            _legendPicture?.Dispose();
            ClearDataSegments();
            ClearDataLabelSegments();
            _axesPicture = picture;
            _dataPicture = null;
            _axisTextPicture = null;
            _legendPicture = null;
            _bounds = bounds;
            _styleHash = styleHash;
            _snapshotVersion = snapshot.Version;
            _snapshot = snapshot;
            _state = null;
        }

        public void Invalidate()
        {
            _axesPicture?.Dispose();
            _dataPicture?.Dispose();
            _axisTextPicture?.Dispose();
            _legendPicture?.Dispose();
            ClearDataSegments();
            ClearDataLabelSegments();
            _axesPicture = null;
            _dataPicture = null;
            _axisTextPicture = null;
            _legendPicture = null;
            _snapshot = null;
            _snapshotVersion = 0;
            _styleHash = 0;
            _bounds = default;
            _state = null;
        }

        public void Dispose()
        {
            Invalidate();
        }

        internal bool HasAxes(SKRect bounds, int styleHash, SkiaChartRenderState state)
        {
            if (_axesPicture == null || _state == null)
            {
                return false;
            }

            return _styleHash == styleHash && _bounds.Equals(bounds) && _state.MatchesBase(state);
        }

        internal bool HasData(SKRect bounds, int styleHash, SkiaChartRenderState state, ChartDataDelta delta)
        {
            if (_dataPicture == null || _state == null)
            {
                return false;
            }

            if (_styleHash != styleHash || !_bounds.Equals(bounds))
            {
                return false;
            }

            if (!_state.MatchesBase(state))
            {
                return false;
            }

            return delta.Kind == ChartDataDeltaKind.None;
        }

        internal bool HasAxisText(SKRect bounds, int styleHash, SkiaChartRenderState state)
        {
            if (_axisTextPicture == null || _state == null)
            {
                return false;
            }

            if (_styleHash != styleHash || !_bounds.Equals(bounds))
            {
                return false;
            }

            if (!_state.MatchesBase(state))
            {
                return false;
            }

            return true;
        }

        internal bool HasLegend(SKRect bounds, int styleHash, SkiaChartRenderState state)
        {
            if (state.LegendRect.HasValue == false)
            {
                return _legendPicture == null;
            }

            if (_legendPicture == null || _state == null)
            {
                return false;
            }

            return _styleHash == styleHash && _bounds.Equals(bounds) && _state.MatchesLegend(state);
        }

        internal void StoreAxes(SKRect bounds, int styleHash, SkiaChartRenderState state, SKPicture picture)
        {
            _axesPicture?.Dispose();
            _axesPicture = picture;
            _bounds = bounds;
            _styleHash = styleHash;
            _state = state;
        }

        internal bool IsCompatible(SKRect bounds, int styleHash, SkiaChartRenderState state)
        {
            return _state != null &&
                   _bounds.Equals(bounds) &&
                   _styleHash == styleHash &&
                   _state.MatchesBase(state);
        }

        internal void StoreData(SKRect bounds, int styleHash, SkiaChartRenderState state, ChartDataSnapshot snapshot, SKPicture picture)
        {
            _dataPicture?.Dispose();
            _dataPicture = picture;
            _bounds = bounds;
            _styleHash = styleHash;
            _state = state;
            _snapshotVersion = snapshot.Version;
            _snapshot = snapshot;
        }

        internal bool TryGetDataSegment(SkiaChartDataSegmentKey key, out SKPicture picture)
        {
            if (_dataSegments != null && _dataSegments.TryGetValue(key, out var cached))
            {
                picture = cached;
                return true;
            }

            picture = null!;
            return false;
        }

        internal void StoreDataSegment(
            SkiaChartDataSegmentKey key,
            SKPicture picture,
            SKRect bounds,
            int styleHash,
            SkiaChartRenderState state,
            ChartDataSnapshot snapshot)
        {
            _dataSegments ??= new Dictionary<SkiaChartDataSegmentKey, SKPicture>();
            if (_dataSegments.TryGetValue(key, out var existing))
            {
                existing.Dispose();
            }

            _dataPicture?.Dispose();
            _dataPicture = null;
            _dataSegments[key] = picture;
            _bounds = bounds;
            _styleHash = styleHash;
            _state = state;
            _snapshotVersion = snapshot.Version;
            _snapshot = snapshot;
        }

        internal void ClearDataSegments()
        {
            if (_dataSegments == null)
            {
                return;
            }

            foreach (var picture in _dataSegments.Values)
            {
                picture.Dispose();
            }

            _dataSegments.Clear();
        }

        internal void StoreAxisText(SKRect bounds, int styleHash, SkiaChartRenderState state, ChartDataSnapshot snapshot, SKPicture picture)
        {
            _axisTextPicture?.Dispose();
            _axisTextPicture = picture;
            _bounds = bounds;
            _styleHash = styleHash;
            _state = state;
            _snapshotVersion = snapshot.Version;
            _snapshot = snapshot;
        }

        internal bool TryGetDataLabelSegment(SkiaChartDataSegmentKey key, out SkiaChartLabelSegment segment)
        {
            if (_dataLabelSegments != null && _dataLabelSegments.TryGetValue(key, out var cached))
            {
                segment = cached;
                return true;
            }

            segment = null!;
            return false;
        }

        internal void StoreDataLabelSegment(
            SkiaChartDataSegmentKey key,
            SKPicture picture,
            SKRect[] placements,
            SKRect bounds,
            int styleHash,
            SkiaChartRenderState state,
            ChartDataSnapshot snapshot)
        {
            _dataLabelSegments ??= new Dictionary<SkiaChartDataSegmentKey, SkiaChartLabelSegment>();
            if (_dataLabelSegments.TryGetValue(key, out var existing))
            {
                existing.Picture.Dispose();
            }

            _dataLabelSegments[key] = new SkiaChartLabelSegment(picture, placements);
            _bounds = bounds;
            _styleHash = styleHash;
            _state = state;
            _snapshotVersion = snapshot.Version;
            _snapshot = snapshot;
        }

        internal void ClearDataLabelSegments()
        {
            if (_dataLabelSegments == null)
            {
                return;
            }

            foreach (var segment in _dataLabelSegments.Values)
            {
                segment.Picture.Dispose();
            }

            _dataLabelSegments.Clear();
        }

        internal void StoreLegend(SKRect bounds, int styleHash, SkiaChartRenderState state, SKPicture picture)
        {
            _legendPicture?.Dispose();
            _legendPicture = picture;
            _bounds = bounds;
            _styleHash = styleHash;
            _state = state;
        }

        internal void ClearLegend()
        {
            _legendPicture?.Dispose();
            _legendPicture = null;
        }

        internal void DrawLayers(
            SKCanvas canvas,
            IReadOnlyList<SkiaChartDataSegmentKey>? dataOrder = null,
            IReadOnlyList<SkiaChartDataSegmentKey>? labelOrder = null)
        {
            if (_axesPicture != null)
            {
                canvas.DrawPicture(_axesPicture);
            }

            if (_dataSegments != null && dataOrder != null)
            {
                for (var i = 0; i < dataOrder.Count; i++)
                {
                    var key = dataOrder[i];
                    if (_dataSegments.TryGetValue(key, out var picture))
                    {
                        canvas.DrawPicture(picture);
                    }
                }
            }
            else if (_dataPicture != null)
            {
                canvas.DrawPicture(_dataPicture);
            }

            if (_axisTextPicture != null)
            {
                canvas.DrawPicture(_axisTextPicture);
            }

            if (_dataLabelSegments != null)
            {
                if (labelOrder != null)
                {
                    for (var i = 0; i < labelOrder.Count; i++)
                    {
                        var key = labelOrder[i];
                        if (_dataLabelSegments.TryGetValue(key, out var segment))
                        {
                            canvas.DrawPicture(segment.Picture);
                        }
                    }
                }
                else
                {
                    foreach (var segment in _dataLabelSegments.Values)
                    {
                        canvas.DrawPicture(segment.Picture);
                    }
                }
            }

            if (_legendPicture != null)
            {
                canvas.DrawPicture(_legendPicture);
            }
        }
    }
}
