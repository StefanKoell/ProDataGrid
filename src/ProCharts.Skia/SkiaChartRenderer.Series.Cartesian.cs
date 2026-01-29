// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using ProCharts;
using SkiaSharp;

namespace ProCharts.Skia
{
    public sealed partial class SkiaChartRenderer
    {
        private static float GetBubbleRadius(double sizeValue, double minSize, double maxSize, SkiaChartStyle style)
        {
            if (IsInvalidNumber(sizeValue) || sizeValue <= 0)
            {
                return 0f;
            }

            var minRadius = Math.Max(0f, style.BubbleMinRadius);
            var maxRadius = Math.Max(minRadius, style.BubbleMaxRadius);
            if (maxRadius <= 0f)
            {
                return 0f;
            }

            if (maxSize <= minSize || double.IsNaN(maxSize) || double.IsNaN(minSize))
            {
                return (minRadius + maxRadius) / 2f;
            }

            var normalized = Clamp((sizeValue - minSize) / (maxSize - minSize), 0d, 1d);
            var scaled = Math.Sqrt(normalized);
            return (float)(minRadius + (maxRadius - minRadius) * scaled);
        }

        private static void DrawLineSeries(
            SKCanvas canvas,
            SKRect plot,
            ChartSeriesSnapshot series,
            int seriesIndex,
            double minValue,
            double maxValue,
            ChartAxisKind valueAxisKind,
            SkiaChartStyle style)
        {
            var overrides = GetSeriesStyleOverrides(style, seriesIndex);
            var themeStyle = GetThemeSeriesStyle(style, seriesIndex);
            var strokeColor = ResolveSeriesStrokeColor(style, seriesIndex, overrides, themeStyle);
            var strokeWidth = ResolveSeriesStrokeWidth(overrides, themeStyle, style.SeriesStrokeWidth);
            var lineStyle = ResolveSeriesLineStyle(overrides, themeStyle);
            var dashPattern = ResolveSeriesDashPattern(overrides, themeStyle);
            var interpolation = ResolveSeriesLineInterpolation(overrides, themeStyle);
            var markerShape = ResolveMarkerShape(overrides, themeStyle, SkiaMarkerShape.Circle);
            var markerSize = ResolveMarkerSize(overrides, themeStyle, DefaultLineMarkerSize);
            var markerFill = ResolveMarkerFillColor(strokeColor, overrides, themeStyle);
            var markerStroke = ResolveMarkerStrokeColor(strokeColor, overrides, themeStyle);
            var markerStrokeWidth = ResolveMarkerStrokeWidth(overrides, themeStyle);

            using var lineEffect = CreateLineEffect(lineStyle, strokeWidth, dashPattern);
            using var linePaint = new SKPaint
            {
                Color = strokeColor,
                StrokeWidth = strokeWidth,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                PathEffect = lineEffect
            };

            using var markerPaint = new SKPaint
            {
                Color = markerFill,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var markerStrokePaint = markerStrokeWidth > 0f
                ? new SKPaint
                {
                    Color = markerStroke,
                    StrokeWidth = markerStrokeWidth,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                }
                : null;

            var path = SkiaChartPools.RentPath();
            var points = SkiaChartPools.RentList<SKPoint>();
            try
            {
                var count = series.Values.Count;
                for (var i = 0; i < count; i++)
                {
                    var value = series.Values[i];
                    if (!value.HasValue || IsInvalidAxisValue(value.Value, valueAxisKind))
                    {
                        DrawInterpolatedLineSegment(canvas, linePaint, path, points, interpolation);

                        continue;
                    }

                    var x = MapX(plot, i, count);
                    var y = MapY(plot, value.Value, minValue, maxValue, valueAxisKind);
                    points.Add(new SKPoint(x, y));

                    DrawMarker(canvas, new SKPoint(x, y), markerSize, markerShape, markerPaint, markerStrokePaint);
                }

                DrawInterpolatedLineSegment(canvas, linePaint, path, points, interpolation);
            }
            finally
            {
                SkiaChartPools.ReturnPath(path);
                SkiaChartPools.ReturnList(points);
            }
        }

        private static void DrawAreaSeries(
            SKCanvas canvas,
            SKRect plot,
            ChartSeriesSnapshot series,
            int seriesIndex,
            double minValue,
            double maxValue,
            ChartAxisKind valueAxisKind,
            SkiaChartStyle style)
        {
            var overrides = GetSeriesStyleOverrides(style, seriesIndex);
            var themeStyle = GetThemeSeriesStyle(style, seriesIndex);
            var strokeColor = ResolveSeriesStrokeColor(style, seriesIndex, overrides, themeStyle);
            var fillColor = ResolveSeriesFillColor(strokeColor, overrides, themeStyle);
            var gradient = ResolveSeriesGradient(overrides, themeStyle);
            var strokeWidth = ResolveSeriesStrokeWidth(overrides, themeStyle, style.SeriesStrokeWidth);
            var lineStyle = ResolveSeriesLineStyle(overrides, themeStyle);
            var dashPattern = ResolveSeriesDashPattern(overrides, themeStyle);
            var interpolation = ResolveSeriesLineInterpolation(overrides, themeStyle);
            var markerShape = ResolveMarkerShape(overrides, themeStyle, SkiaMarkerShape.None);
            var markerSize = ResolveMarkerSize(overrides, themeStyle, DefaultLineMarkerSize);
            var markerFill = ResolveMarkerFillColor(strokeColor, overrides, themeStyle);
            var markerStroke = ResolveMarkerStrokeColor(strokeColor, overrides, themeStyle);
            var markerStrokeWidth = ResolveMarkerStrokeWidth(overrides, themeStyle);

            var baseline = valueAxisKind == ChartAxisKind.Logarithmic
                ? minValue
                : (minValue <= 0 && maxValue >= 0 ? 0d : minValue);
            var baselineY = MapY(plot, baseline, minValue, maxValue, valueAxisKind);

            using var fillPaint = new SKPaint
            {
                Color = ApplyOpacity(fillColor, style.AreaFillOpacity),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var fillShader = gradient != null
                ? CreateGradientShader(plot, gradient, style.AreaFillOpacity)
                : null;

            if (fillShader != null)
            {
                fillPaint.Shader = fillShader;
            }

            using var lineEffect = CreateLineEffect(lineStyle, strokeWidth, dashPattern);
            using var linePaint = new SKPaint
            {
                Color = strokeColor,
                StrokeWidth = strokeWidth,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                PathEffect = lineEffect
            };

            using var markerPaint = new SKPaint
            {
                Color = markerFill,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var markerStrokePaint = markerStrokeWidth > 0f
                ? new SKPaint
                {
                    Color = markerStroke,
                    StrokeWidth = markerStrokeWidth,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                }
                : null;

            var path = SkiaChartPools.RentPath();
            var points = SkiaChartPools.RentList<SKPoint>();
            try
            {
                var count = series.Values.Count;

                for (var i = 0; i < count; i++)
                {
                    var value = series.Values[i];
                    if (!value.HasValue || IsInvalidAxisValue(value.Value, valueAxisKind))
                    {
                        DrawInterpolatedAreaSegment(canvas, fillPaint, linePaint, path, points, interpolation, baselineY);

                        continue;
                    }

                    var x = MapX(plot, i, count);
                    var y = MapY(plot, value.Value, minValue, maxValue, valueAxisKind);
                    points.Add(new SKPoint(x, y));
                    DrawMarker(canvas, new SKPoint(x, y), markerSize, markerShape, markerPaint, markerStrokePaint);
                }

                DrawInterpolatedAreaSegment(canvas, fillPaint, linePaint, path, points, interpolation, baselineY);
            }
            finally
            {
                SkiaChartPools.ReturnPath(path);
                SkiaChartPools.ReturnList(points);
            }
        }

        private static void DrawScatterSeries(
            SKCanvas canvas,
            SKRect plot,
            ChartSeriesSnapshot series,
            int seriesIndex,
            double minValue,
            double maxValue,
            ChartAxisKind valueAxisKind,
            SkiaChartStyle style,
            bool useNumericCategoryAxis,
            ChartAxisKind categoryAxisKind,
            double minCategory,
            double maxCategory)
        {
            var overrides = GetSeriesStyleOverrides(style, seriesIndex);
            var themeStyle = GetThemeSeriesStyle(style, seriesIndex);
            var strokeColor = ResolveSeriesStrokeColor(style, seriesIndex, overrides, themeStyle);
            var fillColor = ResolveSeriesFillColor(strokeColor, overrides, themeStyle);
            var markerShape = ResolveMarkerShape(overrides, themeStyle, SkiaMarkerShape.Circle);
            var markerSize = ResolveMarkerSize(overrides, themeStyle, DefaultScatterMarkerSize);
            var markerFill = ResolveMarkerFillColor(fillColor, overrides, themeStyle);
            var markerStroke = ResolveMarkerStrokeColor(strokeColor, overrides, themeStyle);
            var markerStrokeWidth = ResolveMarkerStrokeWidth(overrides, themeStyle);

            using var markerPaint = new SKPaint
            {
                Color = markerFill,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var markerStrokePaint = markerStrokeWidth > 0f
                ? new SKPaint
                {
                    Color = markerStroke,
                    StrokeWidth = markerStrokeWidth,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                }
                : null;

            var count = series.Values.Count;
            var xValues = series.XValues;
            var hasValidX = false;
            double minX = 0;
            double maxX = 1;
            if (xValues != null && xValues.Count == count)
            {
                if (useNumericCategoryAxis)
                {
                    minX = minCategory;
                    maxX = maxCategory;
                    hasValidX = !IsInvalidAxisValue(minX, categoryAxisKind) &&
                                !IsInvalidAxisValue(maxX, categoryAxisKind) &&
                                maxX > minX;
                }
                else
                {
                    minX = double.MaxValue;
                    maxX = double.MinValue;
                    foreach (var x in xValues)
                    {
                        if (IsInvalidAxisValue(x, categoryAxisKind))
                        {
                            continue;
                        }

                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        hasValidX = true;
                    }

                    if (hasValidX && Math.Abs(maxX - minX) < double.Epsilon)
                    {
                        maxX = minX + 1d;
                    }
                }
            }

            for (var i = 0; i < count; i++)
            {
                var value = series.Values[i];
                if (!value.HasValue || IsInvalidAxisValue(value.Value, valueAxisKind))
                {
                    continue;
                }

                var hasX = xValues != null && xValues.Count == count && hasValidX;
                var xValue = hasX ? xValues![i] : 0d;
                if (hasX && IsInvalidAxisValue(xValue, categoryAxisKind))
                {
                    continue;
                }

                var x = hasX
                    ? MapValueX(plot, xValue, minX, maxX, categoryAxisKind)
                    : MapX(plot, i, count);

                var y = MapY(plot, value.Value, minValue, maxValue, valueAxisKind);
                DrawMarker(canvas, new SKPoint(x, y), markerSize, markerShape, markerPaint, markerStrokePaint);
            }
        }

        private static void DrawBubbleSeries(
            SKCanvas canvas,
            SKRect plot,
            ChartSeriesSnapshot series,
            int seriesIndex,
            double minValue,
            double maxValue,
            ChartAxisKind valueAxisKind,
            SkiaChartStyle style,
            bool useNumericCategoryAxis,
            ChartAxisKind categoryAxisKind,
            double minCategory,
            double maxCategory,
            double minBubbleSize,
            double maxBubbleSize)
        {
            var overrides = GetSeriesStyleOverrides(style, seriesIndex);
            var themeStyle = GetThemeSeriesStyle(style, seriesIndex);
            var strokeColor = ResolveSeriesStrokeColor(style, seriesIndex, overrides, themeStyle);
            var fillColor = ResolveSeriesFillColor(strokeColor, overrides, themeStyle);
            var gradient = ResolveSeriesGradient(overrides, themeStyle);
            var strokeWidth = ResolveSeriesStrokeWidth(overrides, themeStyle, style.BubbleStrokeWidth);

            using var fillPaint = new SKPaint
            {
                Color = ApplyOpacity(fillColor, style.BubbleFillOpacity),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var fillShader = gradient != null
                ? CreateGradientShader(plot, gradient, style.BubbleFillOpacity)
                : null;

            if (fillShader != null)
            {
                fillPaint.Shader = fillShader;
            }

            using var strokePaint = strokeWidth > 0f
                ? new SKPaint
                {
                    Color = strokeColor,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = strokeWidth
                }
                : null;

            var count = series.Values.Count;
            var xValues = series.XValues;
            var hasValidX = false;
            double minX = 0;
            double maxX = 1;
            if (xValues != null && xValues.Count == count)
            {
                if (useNumericCategoryAxis)
                {
                    minX = minCategory;
                    maxX = maxCategory;
                    hasValidX = !IsInvalidAxisValue(minX, categoryAxisKind) &&
                                !IsInvalidAxisValue(maxX, categoryAxisKind) &&
                                maxX > minX;
                }
                else
                {
                    minX = double.MaxValue;
                    maxX = double.MinValue;
                    foreach (var x in xValues)
                    {
                        if (IsInvalidAxisValue(x, categoryAxisKind))
                        {
                            continue;
                        }

                        minX = Math.Min(minX, x);
                        maxX = Math.Max(maxX, x);
                        hasValidX = true;
                    }

                    if (hasValidX && Math.Abs(maxX - minX) < double.Epsilon)
                    {
                        maxX = minX + 1d;
                    }
                }
            }

            var sizeValues = series.SizeValues;
            var hasSizes = sizeValues != null && sizeValues.Count == count;

            for (var i = 0; i < count; i++)
            {
                var value = series.Values[i];
                if (!value.HasValue || IsInvalidAxisValue(value.Value, valueAxisKind))
                {
                    continue;
                }

                var xValue = hasValidX ? xValues![i] : 0d;
                if (hasValidX && IsInvalidAxisValue(xValue, categoryAxisKind))
                {
                    continue;
                }

                double sizeValue;
                if (hasSizes)
                {
                    var size = sizeValues![i];
                    if (!size.HasValue || IsInvalidNumber(size.Value) || size.Value <= 0)
                    {
                        continue;
                    }

                    sizeValue = size.Value;
                }
                else
                {
                    sizeValue = minBubbleSize;
                }

                var radius = GetBubbleRadius(sizeValue, minBubbleSize, maxBubbleSize, style);
                if (radius <= 0f)
                {
                    continue;
                }

                var x = hasValidX
                    ? MapValueX(plot, xValue, minX, maxX, categoryAxisKind)
                    : MapX(plot, i, count);

                var y = MapY(plot, value.Value, minValue, maxValue, valueAxisKind);
                canvas.DrawCircle(x, y, radius, fillPaint);
                if (strokePaint != null)
                {
                    canvas.DrawCircle(x, y, radius, strokePaint);
                }
            }
        }

        private static void DrawColumnSeries(
            SKCanvas canvas,
            SKRect plot,
            int categoryCount,
            ChartSeriesSnapshot series,
            int seriesIndex,
            int seriesCount,
            double minValue,
            double maxValue,
            ChartAxisKind valueAxisKind,
            SkiaChartStyle style)
        {
            if (categoryCount == 0)
            {
                return;
            }

            var overrides = GetSeriesStyleOverrides(style, seriesIndex);
            var themeStyle = GetThemeSeriesStyle(style, seriesIndex);
            var strokeColor = ResolveSeriesStrokeColor(style, seriesIndex, overrides, themeStyle);
            var fillColor = ResolveSeriesFillColor(strokeColor, overrides, themeStyle);
            var gradient = ResolveSeriesGradient(overrides, themeStyle);
            var lineStyle = ResolveSeriesLineStyle(overrides, themeStyle);
            var dashPattern = ResolveSeriesDashPattern(overrides, themeStyle);
            var explicitStrokeWidth = overrides?.StrokeWidth ?? themeStyle?.StrokeWidth;
            var hasStroke = explicitStrokeWidth.HasValue ||
                            overrides?.StrokeColor.HasValue == true ||
                            themeStyle?.StrokeColor.HasValue == true;
            var strokeWidth = explicitStrokeWidth ?? (hasStroke ? style.AxisStrokeWidth : 0f);

            var groupWidth = plot.Width / categoryCount;
            var barWidth = groupWidth / Math.Max(1, seriesCount) * 0.75f;
            var offset = (groupWidth - (barWidth * seriesCount)) / 2f;
            var baseline = valueAxisKind == ChartAxisKind.Logarithmic
                ? minValue
                : (minValue <= 0 && maxValue >= 0 ? 0d : minValue);
            var baselineY = MapY(plot, baseline, minValue, maxValue, valueAxisKind);

            using var barPaint = new SKPaint
            {
                Color = fillColor,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var strokeEffect = hasStroke && strokeWidth > 0f
                ? CreateLineEffect(lineStyle, strokeWidth, dashPattern)
                : null;

            using var strokePaint = hasStroke && strokeWidth > 0f
                ? new SKPaint
                {
                    Color = strokeColor,
                    StrokeWidth = strokeWidth,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    PathEffect = strokeEffect
                }
                : null;

            for (var i = 0; i < categoryCount && i < series.Values.Count; i++)
            {
                var value = series.Values[i];
                if (!value.HasValue || IsInvalidAxisValue(value.Value, valueAxisKind))
                {
                    continue;
                }

                var x = plot.Left + (i * groupWidth) + offset + (seriesIndex * barWidth);
                var y = MapY(plot, value.Value, minValue, maxValue, valueAxisKind);
                var top = Math.Min(y, baselineY);
                var bottom = Math.Max(y, baselineY);
                var rect = new SKRect(x, top, x + barWidth, bottom);
                if (gradient != null)
                {
                    using var shader = CreateGradientShader(rect, gradient, 1f);
                    if (shader != null)
                    {
                        barPaint.Shader = shader;
                    }

                    canvas.DrawRect(rect, barPaint);
                    barPaint.Shader = null;
                }
                else
                {
                    canvas.DrawRect(rect, barPaint);
                }

                if (strokePaint != null)
                {
                    canvas.DrawRect(rect, strokePaint);
                }
            }
        }

        private static void DrawBarSeries(
            SKCanvas canvas,
            SKRect plot,
            int categoryCount,
            ChartSeriesSnapshot series,
            int seriesIndex,
            int seriesCount,
            double minValue,
            double maxValue,
            ChartAxisKind valueAxisKind,
            SkiaChartStyle style)
        {
            if (categoryCount == 0)
            {
                return;
            }

            var overrides = GetSeriesStyleOverrides(style, seriesIndex);
            var themeStyle = GetThemeSeriesStyle(style, seriesIndex);
            var strokeColor = ResolveSeriesStrokeColor(style, seriesIndex, overrides, themeStyle);
            var fillColor = ResolveSeriesFillColor(strokeColor, overrides, themeStyle);
            var gradient = ResolveSeriesGradient(overrides, themeStyle);
            var lineStyle = ResolveSeriesLineStyle(overrides, themeStyle);
            var dashPattern = ResolveSeriesDashPattern(overrides, themeStyle);
            var explicitStrokeWidth = overrides?.StrokeWidth ?? themeStyle?.StrokeWidth;
            var hasStroke = explicitStrokeWidth.HasValue ||
                            overrides?.StrokeColor.HasValue == true ||
                            themeStyle?.StrokeColor.HasValue == true;
            var strokeWidth = explicitStrokeWidth ?? (hasStroke ? style.AxisStrokeWidth : 0f);

            var groupHeight = plot.Height / categoryCount;
            var barHeight = groupHeight / Math.Max(1, seriesCount) * 0.75f;
            var offset = (groupHeight - (barHeight * seriesCount)) / 2f;
            var baseline = valueAxisKind == ChartAxisKind.Logarithmic
                ? minValue
                : (minValue <= 0 && maxValue >= 0 ? 0d : minValue);
            var baselineX = MapValueX(plot, baseline, minValue, maxValue, valueAxisKind);

            using var barPaint = new SKPaint
            {
                Color = fillColor,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            using var strokeEffect = hasStroke && strokeWidth > 0f
                ? CreateLineEffect(lineStyle, strokeWidth, dashPattern)
                : null;

            using var strokePaint = hasStroke && strokeWidth > 0f
                ? new SKPaint
                {
                    Color = strokeColor,
                    StrokeWidth = strokeWidth,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    PathEffect = strokeEffect
                }
                : null;

            for (var i = 0; i < categoryCount && i < series.Values.Count; i++)
            {
                var value = series.Values[i];
                if (!value.HasValue || IsInvalidAxisValue(value.Value, valueAxisKind))
                {
                    continue;
                }

                var y = plot.Top + (i * groupHeight) + offset + (seriesIndex * barHeight);
                var valueX = MapValueX(plot, value.Value, minValue, maxValue, valueAxisKind);
                var left = Math.Min(baselineX, valueX);
                var right = Math.Max(baselineX, valueX);
                var rect = new SKRect(left, y, right, y + barHeight);
                if (gradient != null)
                {
                    using var shader = CreateGradientShader(rect, gradient, 1f);
                    if (shader != null)
                    {
                        barPaint.Shader = shader;
                    }

                    canvas.DrawRect(rect, barPaint);
                    barPaint.Shader = null;
                }
                else
                {
                    canvas.DrawRect(rect, barPaint);
                }

                if (strokePaint != null)
                {
                    canvas.DrawRect(rect, strokePaint);
                }
            }
        }

        private static void DrawStackedColumnSeries(
            SKCanvas canvas,
            SKRect plot,
            ChartDataSnapshot snapshot,
            IReadOnlyList<int> seriesIndices,
            double minValue,
            double maxValue,
            ChartAxisKind valueAxisKind,
            SkiaChartStyle style,
            bool normalizeToPercent)
        {
            var categoryCount = snapshot.Categories.Count;
            if (categoryCount == 0 || seriesIndices.Count == 0)
            {
                return;
            }

            var stacked = BuildStackedSeriesValues(snapshot, seriesIndices, valueAxisKind, normalizeToPercent);
            if (stacked.Count == 0)
            {
                return;
            }

            var groupWidth = plot.Width / categoryCount;
            var barWidth = groupWidth * 0.75f;
            var offset = (groupWidth - barWidth) / 2f;

            foreach (var stackedSeries in stacked)
            {
                var series = stackedSeries.Series;
                var overrides = GetSeriesStyleOverrides(style, stackedSeries.SeriesIndex);
                var themeStyle = GetThemeSeriesStyle(style, stackedSeries.SeriesIndex);
                var strokeColor = ResolveSeriesStrokeColor(style, stackedSeries.SeriesIndex, overrides, themeStyle);
                var fillColor = ResolveSeriesFillColor(strokeColor, overrides, themeStyle);
                var gradient = ResolveSeriesGradient(overrides, themeStyle);
                using var barPaint = new SKPaint
                {
                    Color = fillColor,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                var count = Math.Min(categoryCount, series.Values.Count);
                for (var i = 0; i < count; i++)
                {
                    var value = series.Values[i];
                    if (!value.HasValue || IsInvalidAxisValue(value.Value, valueAxisKind))
                    {
                        continue;
                    }

                    var y0 = MapY(plot, stackedSeries.Start[i], minValue, maxValue, valueAxisKind);
                    var y1 = MapY(plot, stackedSeries.End[i], minValue, maxValue, valueAxisKind);
                    var top = Math.Min(y0, y1);
                    var bottom = Math.Max(y0, y1);
                    var x = plot.Left + (i * groupWidth) + offset;
                    var rect = new SKRect(x, top, x + barWidth, bottom);
                    if (gradient != null)
                    {
                        using var shader = CreateGradientShader(rect, gradient, 1f);
                        if (shader != null)
                        {
                            barPaint.Shader = shader;
                        }

                        canvas.DrawRect(rect, barPaint);
                        barPaint.Shader = null;
                    }
                    else
                    {
                        canvas.DrawRect(rect, barPaint);
                    }
                }
            }
        }

        private static void DrawStackedBarSeries(
            SKCanvas canvas,
            SKRect plot,
            ChartDataSnapshot snapshot,
            IReadOnlyList<int> seriesIndices,
            double minValue,
            double maxValue,
            ChartAxisKind valueAxisKind,
            SkiaChartStyle style,
            bool normalizeToPercent)
        {
            var categoryCount = snapshot.Categories.Count;
            if (categoryCount == 0 || seriesIndices.Count == 0)
            {
                return;
            }

            var stacked = BuildStackedSeriesValues(snapshot, seriesIndices, valueAxisKind, normalizeToPercent);
            if (stacked.Count == 0)
            {
                return;
            }

            var groupHeight = plot.Height / categoryCount;
            var barHeight = groupHeight * 0.75f;
            var offset = (groupHeight - barHeight) / 2f;

            foreach (var stackedSeries in stacked)
            {
                var series = stackedSeries.Series;
                var overrides = GetSeriesStyleOverrides(style, stackedSeries.SeriesIndex);
                var themeStyle = GetThemeSeriesStyle(style, stackedSeries.SeriesIndex);
                var strokeColor = ResolveSeriesStrokeColor(style, stackedSeries.SeriesIndex, overrides, themeStyle);
                var fillColor = ResolveSeriesFillColor(strokeColor, overrides, themeStyle);
                var gradient = ResolveSeriesGradient(overrides, themeStyle);
                using var barPaint = new SKPaint
                {
                    Color = fillColor,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                var count = Math.Min(categoryCount, series.Values.Count);
                for (var i = 0; i < count; i++)
                {
                    var value = series.Values[i];
                    if (!value.HasValue || IsInvalidAxisValue(value.Value, valueAxisKind))
                    {
                        continue;
                    }

                    var x0 = MapValueX(plot, stackedSeries.Start[i], minValue, maxValue, valueAxisKind);
                    var x1 = MapValueX(plot, stackedSeries.End[i], minValue, maxValue, valueAxisKind);
                    var left = Math.Min(x0, x1);
                    var right = Math.Max(x0, x1);
                    var y = plot.Top + (i * groupHeight) + offset;
                    var rect = new SKRect(left, y, right, y + barHeight);
                    if (gradient != null)
                    {
                        using var shader = CreateGradientShader(rect, gradient, 1f);
                        if (shader != null)
                        {
                            barPaint.Shader = shader;
                        }

                        canvas.DrawRect(rect, barPaint);
                        barPaint.Shader = null;
                    }
                    else
                    {
                        canvas.DrawRect(rect, barPaint);
                    }
                }
            }
        }

        private static void DrawStackedAreaSeries(
            SKCanvas canvas,
            SKRect plot,
            ChartDataSnapshot snapshot,
            IReadOnlyList<int> seriesIndices,
            double minValue,
            double maxValue,
            ChartAxisKind valueAxisKind,
            SkiaChartStyle style,
            bool normalizeToPercent)
        {
            var categoryCount = snapshot.Categories.Count;
            if (categoryCount == 0 || seriesIndices.Count == 0)
            {
                return;
            }

            var stacked = BuildStackedSeriesValues(snapshot, seriesIndices, valueAxisKind, normalizeToPercent);
            if (stacked.Count == 0)
            {
                return;
            }

            foreach (var stackedSeries in stacked)
            {
                var series = stackedSeries.Series;
                var overrides = GetSeriesStyleOverrides(style, stackedSeries.SeriesIndex);
                var themeStyle = GetThemeSeriesStyle(style, stackedSeries.SeriesIndex);
                var strokeColor = ResolveSeriesStrokeColor(style, stackedSeries.SeriesIndex, overrides, themeStyle);
                var fillColor = ResolveSeriesFillColor(strokeColor, overrides, themeStyle);
                var gradient = ResolveSeriesGradient(overrides, themeStyle);
                var strokeWidth = ResolveSeriesStrokeWidth(overrides, themeStyle, style.SeriesStrokeWidth);
                var lineStyle = ResolveSeriesLineStyle(overrides, themeStyle);
                var dashPattern = ResolveSeriesDashPattern(overrides, themeStyle);
                var interpolation = ResolveSeriesLineInterpolation(overrides, themeStyle);

                using var fillPaint = new SKPaint
                {
                    Color = ApplyOpacity(fillColor, style.AreaFillOpacity),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                using var fillShader = gradient != null
                    ? CreateGradientShader(plot, gradient, style.AreaFillOpacity)
                    : null;

                if (fillShader != null)
                {
                    fillPaint.Shader = fillShader;
                }

                using var lineEffect = CreateLineEffect(lineStyle, strokeWidth, dashPattern);
                using var linePaint = new SKPaint
                {
                    Color = strokeColor,
                    StrokeWidth = strokeWidth,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    PathEffect = lineEffect
                };

                var path = SkiaChartPools.RentPath();
                var topPoints = SkiaChartPools.RentList<SKPoint>(categoryCount);
                var bottomPoints = SkiaChartPools.RentList<SKPoint>(categoryCount);
                try
                {
                    for (var i = 0; i < categoryCount; i++)
                    {
                        var x = MapX(plot, i, categoryCount);
                        var y = MapY(plot, stackedSeries.End[i], minValue, maxValue, valueAxisKind);
                        topPoints.Add(new SKPoint(x, y));
                        var yBase = MapY(plot, stackedSeries.Start[i], minValue, maxValue, valueAxisKind);
                        bottomPoints.Add(new SKPoint(x, yBase));
                    }

                    if (topPoints.Count == 0)
                    {
                        continue;
                    }

                    AppendInterpolatedPath(path, topPoints, interpolation, moveTo: true);
                    topPoints.Clear();
                    for (var i = bottomPoints.Count - 1; i >= 0; i--)
                    {
                        topPoints.Add(bottomPoints[i]);
                    }

                    AppendInterpolatedPath(path, topPoints, interpolation, moveTo: false);

                    path.Close();
                    canvas.DrawPath(path, fillPaint);
                    canvas.DrawPath(path, linePaint);
                }
                finally
                {
                    SkiaChartPools.ReturnPath(path);
                    SkiaChartPools.ReturnList(topPoints);
                    SkiaChartPools.ReturnList(bottomPoints);
                }
            }
        }

        private static List<StackedSeriesValues> BuildStackedSeriesValues(
            ChartDataSnapshot snapshot,
            IReadOnlyList<int> seriesIndices,
            ChartAxisKind axisKind,
            bool normalizeToPercent)
        {
            var result = new List<StackedSeriesValues>();
            var categoryCount = snapshot.Categories.Count;
            if (categoryCount == 0 || seriesIndices.Count == 0)
            {
                return result;
            }

            var positive = new double[categoryCount];
            var negative = new double[categoryCount];
            var positiveTotals = normalizeToPercent ? new double[categoryCount] : Array.Empty<double>();
            var negativeTotals = normalizeToPercent ? new double[categoryCount] : Array.Empty<double>();

            if (normalizeToPercent)
            {
                foreach (var seriesIndex in seriesIndices)
                {
                    var series = snapshot.Series[seriesIndex];
                    var count = Math.Min(categoryCount, series.Values.Count);
                    for (var i = 0; i < count; i++)
                    {
                        var value = series.Values[i];
                        if (!value.HasValue || IsInvalidAxisValue(value.Value, axisKind))
                        {
                            continue;
                        }

                        var v = value.Value;
                        if (v >= 0)
                        {
                            positiveTotals[i] += v;
                        }
                        else
                        {
                            negativeTotals[i] += -v;
                        }
                    }
                }
            }

            foreach (var seriesIndex in seriesIndices)
            {
                var series = snapshot.Series[seriesIndex];
                var start = new double[categoryCount];
                var end = new double[categoryCount];
                var count = Math.Min(categoryCount, series.Values.Count);
                for (var i = 0; i < count; i++)
                {
                    var value = series.Values[i];
                    var v = value.HasValue && !IsInvalidAxisValue(value.Value, axisKind) ? value.Value : 0d;
                    if (normalizeToPercent)
                    {
                        if (v >= 0)
                        {
                            var total = positiveTotals[i];
                            v = total > 0d ? v / total : 0d;
                        }
                        else
                        {
                            var total = negativeTotals[i];
                            v = total > 0d ? v / total : 0d;
                        }
                    }
                    var baseValue = v >= 0 ? positive[i] : negative[i];
                    var next = baseValue + v;
                    start[i] = baseValue;
                    end[i] = next;

                    if (v >= 0)
                    {
                        positive[i] = next;
                    }
                    else
                    {
                        negative[i] = next;
                    }
                }

                result.Add(new StackedSeriesValues(seriesIndex, series, start, end));
            }

            return result;
        }

        private sealed class StackedSeriesValues
        {
            public StackedSeriesValues(int seriesIndex, ChartSeriesSnapshot series, double[] start, double[] end)
            {
                SeriesIndex = seriesIndex;
                Series = series;
                Start = start;
                End = end;
            }

            public int SeriesIndex { get; }

            public ChartSeriesSnapshot Series { get; }

            public double[] Start { get; }

            public double[] End { get; }
        }

        private static void DrawInterpolatedLineSegment(
            SKCanvas canvas,
            SKPaint linePaint,
            SKPath path,
            List<SKPoint> points,
            SkiaLineInterpolation interpolation)
        {
            if (points.Count < 2)
            {
                points.Clear();
                path.Reset();
                return;
            }

            path.Reset();
            AppendInterpolatedPath(path, points, interpolation, moveTo: true);
            canvas.DrawPath(path, linePaint);
            points.Clear();
            path.Reset();
        }

        private static void DrawInterpolatedAreaSegment(
            SKCanvas canvas,
            SKPaint fillPaint,
            SKPaint linePaint,
            SKPath path,
            List<SKPoint> points,
            SkiaLineInterpolation interpolation,
            float baselineY)
        {
            if (points.Count < 2)
            {
                points.Clear();
                path.Reset();
                return;
            }

            path.Reset();
            AppendInterpolatedPath(path, points, interpolation, moveTo: true);
            var last = points[points.Count - 1];
            var first = points[0];
            path.LineTo(last.X, baselineY);
            path.LineTo(first.X, baselineY);
            path.Close();
            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, linePaint);
            points.Clear();
            path.Reset();
        }

        private static void AppendInterpolatedPath(
            SKPath path,
            IReadOnlyList<SKPoint> points,
            SkiaLineInterpolation interpolation,
            bool moveTo)
        {
            if (points.Count == 0)
            {
                return;
            }

            switch (interpolation)
            {
                case SkiaLineInterpolation.Smooth:
                    AppendSmoothPath(path, points, moveTo);
                    break;
                case SkiaLineInterpolation.Step:
                    AppendStepPath(path, points, moveTo);
                    break;
                default:
                    AppendLinearPath(path, points, moveTo);
                    break;
            }
        }

        private static void AppendLinearPath(SKPath path, IReadOnlyList<SKPoint> points, bool moveTo)
        {
            if (points.Count == 0)
            {
                return;
            }

            var start = points[0];
            if (moveTo)
            {
                path.MoveTo(start);
            }
            else
            {
                path.LineTo(start);
            }

            for (var i = 1; i < points.Count; i++)
            {
                path.LineTo(points[i]);
            }
        }

        private static void AppendStepPath(SKPath path, IReadOnlyList<SKPoint> points, bool moveTo)
        {
            if (points.Count == 0)
            {
                return;
            }

            var current = points[0];
            if (moveTo)
            {
                path.MoveTo(current);
            }
            else
            {
                path.LineTo(current);
            }

            for (var i = 1; i < points.Count; i++)
            {
                var next = points[i];
                path.LineTo(next.X, current.Y);
                path.LineTo(next.X, next.Y);
                current = next;
            }
        }

        private static void AppendSmoothPath(SKPath path, IReadOnlyList<SKPoint> points, bool moveTo)
        {
            if (points.Count == 0)
            {
                return;
            }

            var start = points[0];
            if (moveTo)
            {
                path.MoveTo(start);
            }
            else
            {
                path.LineTo(start);
            }

            if (points.Count < 2)
            {
                return;
            }

            for (var i = 0; i < points.Count - 1; i++)
            {
                var p0 = i > 0 ? points[i - 1] : points[i];
                var p1 = points[i];
                var p2 = points[i + 1];
                var p3 = i + 2 < points.Count ? points[i + 2] : p2;

                var cp1 = new SKPoint(
                    p1.X + (p2.X - p0.X) / 6f,
                    p1.Y + (p2.Y - p0.Y) / 6f);
                var cp2 = new SKPoint(
                    p2.X - (p3.X - p1.X) / 6f,
                    p2.Y - (p3.Y - p1.Y) / 6f);

                path.CubicTo(cp1, cp2, p2);
            }
        }

    }
}
