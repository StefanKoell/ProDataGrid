// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Collections.Generic;
using SkiaSharp;

namespace ProCharts.Skia
{
    public enum SkiaLineStyle
    {
        Solid,
        Dashed,
        Dotted,
        DashDot,
        DashDotDot
    }

    public enum SkiaLineInterpolation
    {
        Linear,
        Smooth,
        Step
    }

    public enum SkiaMarkerShape
    {
        None,
        Circle,
        Square,
        Diamond,
        Triangle,
        TriangleDown,
        Cross,
        X,
        Plus
    }

    public enum SkiaGradientDirection
    {
        Vertical,
        Horizontal,
        DiagonalDown,
        DiagonalUp
    }

    public sealed class SkiaChartGradient
    {
        public SkiaGradientDirection Direction { get; set; } = SkiaGradientDirection.Vertical;

        public IReadOnlyList<SKColor> Colors { get; set; } = new[] { SKColors.White, SKColors.Black };

        public IReadOnlyList<float>? Stops { get; set; }
    }

    public sealed class SkiaChartSeriesStyle
    {
        public SKColor? StrokeColor { get; set; }

        public SKColor? FillColor { get; set; }

        public float? StrokeWidth { get; set; }

        public SkiaLineStyle? LineStyle { get; set; }

        public SkiaLineInterpolation? LineInterpolation { get; set; }

        public float[]? DashPattern { get; set; }

        public SkiaMarkerShape? MarkerShape { get; set; }

        public float? MarkerSize { get; set; }

        public SKColor? MarkerFillColor { get; set; }

        public SKColor? MarkerStrokeColor { get; set; }

        public float? MarkerStrokeWidth { get; set; }

        public SkiaChartGradient? FillGradient { get; set; }
    }

    public sealed class SkiaChartTheme
    {
        public SKColor? Background { get; set; }

        public SKColor? Axis { get; set; }

        public SKColor? Text { get; set; }

        public SKColor? Gridline { get; set; }

        public SKColor? DataLabelBackground { get; set; }

        public SKColor? DataLabelText { get; set; }

        public IReadOnlyList<SKColor>? SeriesColors { get; set; }

        public IReadOnlyList<SkiaChartSeriesStyle>? SeriesStyles { get; set; }
    }
}
