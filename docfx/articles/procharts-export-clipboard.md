# ProCharts Export and Clipboard

ProCharts supports PNG/SVG export and clipboard integration through `ProChartView` and `SkiaChartExporter`.

## Export from ProChartView

```csharp
var pngBytes = chartView.ExportPng();
var svgText = chartView.ExportSvg();
```

The export size uses the control bounds and render scaling.

## Copy to clipboard

```csharp
await chartView.CopyToClipboardAsync(ChartClipboardFormat.Png);
await chartView.CopyToClipboardAsync(ChartClipboardFormat.Svg);
```

## Headless export

Use `SkiaChartExporter` for server-side or CI rendering:

```csharp
var png = SkiaChartExporter.ExportPng(snapshot, width: 1200, height: 720, style);
var svg = SkiaChartExporter.ExportSvg(snapshot, width: 1200, height: 720, style);
```
