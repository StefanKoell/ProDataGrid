# Chart Gallery (ProCharts)

ProCharts provides Excel-quality charting with SkiaSharp rendering and Avalonia integration. The sample app includes a full chart gallery that showcases chart types, axis options, labels, tooltips, and data models wired to ProDataGrid.

## Chart types

The gallery includes:

- Line, area, column, bar, stacked variants, and combo charts.
- Scatter and bubble charts with secondary axes.
- Pie and donut charts.
- Waterfall, histogram, pareto, radar, box-and-whisker, and funnel charts.

## Sample pages

Run the sample app and open:

- "Charts" for the gallery with axis controls, legend options, and label toggles.
- "Pivot Chart Model" for pivot-driven chart series.
- "Calculated Measures" for formula-based chart measures.

```bash
dotnet run --project src/DataGridSample/DataGridSample.csproj
```

## Styling and themes

Use `SkiaChartStyle.Theme` to apply a chart theme and `SeriesStyles` for per-series formatting (markers, line styles, gradients):

```csharp
var style = new SkiaChartStyle
{
    Theme = new SkiaChartTheme
    {
        Background = new SKColor(250, 250, 250),
        Axis = new SKColor(64, 64, 64),
        Gridline = new SKColor(220, 220, 220),
        SeriesColors = new[]
        {
            new SKColor(33, 150, 243),
            new SKColor(255, 152, 0),
            new SKColor(76, 175, 80)
        }
    },
    SeriesStyles = new[]
    {
        new SkiaChartSeriesStyle
        {
            LineStyle = SkiaLineStyle.Dashed,
            MarkerShape = SkiaMarkerShape.Diamond,
            MarkerSize = 3.5f
        },
        new SkiaChartSeriesStyle
        {
            FillGradient = new SkiaChartGradient
            {
                Direction = SkiaGradientDirection.Vertical,
                Colors = new[] { new SKColor(255, 152, 0), new SKColor(255, 224, 178) }
            }
        }
    }
};
```

## Export and clipboard

`ProChartView` can export and copy charts:

```csharp
var pngBytes = chartView.ExportPng();
var svgText = chartView.ExportSvg();
await chartView.CopyToClipboardAsync(ChartClipboardFormat.Png);
await chartView.CopyToClipboardAsync(ChartClipboardFormat.Svg);
```

For headless export, use `SkiaChartExporter.ExportPng` and `SkiaChartExporter.ExportSvg`.
