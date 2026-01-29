// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.Collections.Generic;

namespace ProCharts
{
    public enum ChartLineStyle
    {
        Solid,
        Dashed,
        Dotted,
        DashDot,
        DashDotDot
    }

    public enum ChartMarkerShape
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

    public enum ChartGradientDirection
    {
        Vertical,
        Horizontal,
        DiagonalDown,
        DiagonalUp
    }

    public readonly struct ChartColor : IEquatable<ChartColor>
    {
        public ChartColor(byte red, byte green, byte blue, byte alpha = 255)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public byte Alpha { get; }

        public byte Red { get; }

        public byte Green { get; }

        public byte Blue { get; }

        public static ChartColor FromArgb(byte alpha, byte red, byte green, byte blue)
        {
            return new ChartColor(red, green, blue, alpha);
        }

        public static ChartColor FromRgb(byte red, byte green, byte blue)
        {
            return new ChartColor(red, green, blue, 255);
        }

        public uint ToArgb()
        {
            return (uint)((Alpha << 24) | (Red << 16) | (Green << 8) | Blue);
        }

        public bool Equals(ChartColor other)
        {
            return Alpha == other.Alpha &&
                   Red == other.Red &&
                   Green == other.Green &&
                   Blue == other.Blue;
        }

        public override bool Equals(object? obj)
        {
            return obj is ChartColor other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)ToArgb();
        }

        public override string ToString()
        {
            return $"#{Alpha:X2}{Red:X2}{Green:X2}{Blue:X2}";
        }

        public static bool operator ==(ChartColor left, ChartColor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ChartColor left, ChartColor right)
        {
            return !left.Equals(right);
        }
    }

    public sealed class ChartGradient
    {
        public ChartGradientDirection Direction { get; set; } = ChartGradientDirection.Vertical;

        public IReadOnlyList<ChartColor> Colors { get; set; } = new[]
        {
            new ChartColor(255, 255, 255),
            new ChartColor(0, 0, 0)
        };

        public IReadOnlyList<float>? Stops { get; set; }
    }

    public sealed class ChartSeriesStyle
    {
        public ChartColor? StrokeColor { get; set; }

        public ChartColor? FillColor { get; set; }

        public float? StrokeWidth { get; set; }

        public ChartLineStyle? LineStyle { get; set; }

        public ChartLineInterpolation? LineInterpolation { get; set; }

        public float[]? DashPattern { get; set; }

        public ChartMarkerShape? MarkerShape { get; set; }

        public float? MarkerSize { get; set; }

        public ChartColor? MarkerFillColor { get; set; }

        public ChartColor? MarkerStrokeColor { get; set; }

        public float? MarkerStrokeWidth { get; set; }

        public ChartGradient? FillGradient { get; set; }
    }

    public sealed class ChartTheme
    {
        public ChartColor? Background { get; set; }

        public ChartColor? Axis { get; set; }

        public ChartColor? Text { get; set; }

        public ChartColor? Gridline { get; set; }

        public ChartColor? DataLabelBackground { get; set; }

        public ChartColor? DataLabelText { get; set; }

        public IReadOnlyList<ChartColor>? SeriesColors { get; set; }

        public IReadOnlyList<ChartSeriesStyle>? SeriesStyles { get; set; }
    }
}
