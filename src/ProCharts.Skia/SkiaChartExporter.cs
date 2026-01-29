// Copyright (c) Wieslaw Soltes. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using System.IO;
using System.Text;
using ProCharts;
using SkiaSharp;

namespace ProCharts.Skia
{
    public static class SkiaChartExporter
    {
        public static byte[] ExportPng(
            ChartDataSnapshot snapshot,
            int pixelWidth,
            int pixelHeight,
            SkiaChartStyle? style = null)
        {
            using var stream = new MemoryStream();
            ExportPng(snapshot, pixelWidth, pixelHeight, stream, style);
            return stream.ToArray();
        }

        public static void ExportPng(
            ChartDataSnapshot snapshot,
            int pixelWidth,
            int pixelHeight,
            Stream output,
            SkiaChartStyle? style = null)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (pixelWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pixelWidth));
            }

            if (pixelHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pixelHeight));
            }

            var info = new SKImageInfo(pixelWidth, pixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            if (surface == null)
            {
                return;
            }

            var renderer = new SkiaChartRenderer();
            var bounds = SKRect.Create(pixelWidth, pixelHeight);
            renderer.Render(surface.Canvas, bounds, snapshot, style);
            surface.Canvas.Flush();

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(output);
        }

        public static string ExportSvg(
            ChartDataSnapshot snapshot,
            int width,
            int height,
            SkiaChartStyle? style = null)
        {
            using var stream = new MemoryStream();
            ExportSvg(snapshot, width, height, stream, style);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        public static void ExportSvg(
            ChartDataSnapshot snapshot,
            int width,
            int height,
            Stream output,
            SkiaChartStyle? style = null)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            var bounds = SKRect.Create(width, height);
            using var canvas = SKSvgCanvas.Create(bounds, output);
            if (canvas == null)
            {
                return;
            }

            var renderer = new SkiaChartRenderer();
            renderer.Render(canvas, bounds, snapshot, style);
            canvas.Flush();
        }
    }
}
