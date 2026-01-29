# ProCharts Styling and Interaction

ProCharts provides renderer-agnostic styling types in `ProCharts`, plus Skia-specific styling in `ProCharts.Skia`. The Skia renderer adapts the core theme and series styles to SkiaSharp colors and paints.

## Core themes and series styles

Use `ChartTheme` and `ChartSeriesStyle` on `ChartModel` so any renderer can consume styling metadata.

```csharp
model.Theme = new ChartTheme
{
    Background = new ChartColor(250, 250, 250),
    Axis = new ChartColor(60, 60, 60),
    Gridline = new ChartColor(220, 220, 220),
    SeriesColors = new[]
    {
        ChartColor.FromRgb(33, 150, 243),
        ChartColor.FromRgb(255, 152, 0),
        ChartColor.FromRgb(76, 175, 80)
    }
};

model.SeriesStyles = new[]
{
    new ChartSeriesStyle
    {
        LineStyle = ChartLineStyle.Dashed,
        MarkerShape = ChartMarkerShape.Diamond,
        MarkerSize = 4f
    }
};
```

## Skia-specific theme overrides

Use `SkiaChartStyle.Theme` for renderer-specific colors. Palettes apply to all series unless overridden by per-series styles.

```csharp
var style = new SkiaChartStyle
{
    Theme = new SkiaChartTheme
    {
        Background = new SKColor(250, 250, 250),
        Axis = new SKColor(60, 60, 60),
        Gridline = new SKColor(220, 220, 220),
        SeriesColors = new[]
        {
            new SKColor(33, 150, 243),
            new SKColor(255, 152, 0),
            new SKColor(76, 175, 80)
        }
    }
};
```

## Per-series styling

Use `SeriesStyles` for markers, line dashes, gradients, and label formats. Styles can be set by index or by series id.

```csharp
style.SeriesStyles = new[]
{
    new SkiaChartSeriesStyle
    {
        LineStyle = SkiaLineStyle.Dashed,
        MarkerShape = SkiaMarkerShape.Diamond,
        MarkerSize = 4f,
        DataLabelFormat = "#,##0.0"
    },
    new SkiaChartSeriesStyle
    {
        FillGradient = new SkiaChartGradient
        {
            Direction = SkiaGradientDirection.Vertical,
            Colors = new[] { new SKColor(255, 152, 0), new SKColor(255, 224, 178) }
        }
    }
};
```

## Axes, gridlines, and labels

- Axis kinds: category, value, log, and date-time.
- Gridline control for major and minor ticks.
- Label rotation, ellipsis, and collision handling are supported.
- Per-axis label formatters allow precise numeric/date display.

## Legends and layout

- Legends support row/column flow and wrapping.
- You can group stacked series or display each series individually.
- Legend items inherit series colors and marker styles.

## Tooltips and hit testing

`ProChartView` includes hit testing and tooltip hooks. The view provides the nearest data point and series metadata so you can build custom tooltips.

## Export and clipboard

```csharp
var png = chartView.ExportPng();
var svg = chartView.ExportSvg();
await chartView.CopyToClipboardAsync(ChartClipboardFormat.Svg);
```

For headless scenarios:

```csharp
var pngBytes = SkiaChartExporter.ExportPng(snapshot, width: 1200, height: 720, style);
var svgText = SkiaChartExporter.ExportSvg(snapshot, width: 1200, height: 720, style);
```
