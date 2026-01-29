# ProCharts Interaction

`ProChartView` provides built-in interaction for tooltips, hit testing, and pan/zoom.

## Tooltips and hit testing

Enable tooltips and provide a formatter:

```csharp
chartView.ShowToolTips = true;
chartView.ToolTipFormatter = hit =>
    $"{hit.SeriesName}: {hit.Value}";
```

The formatter receives `SkiaChartHitTestResult`, which includes the series name, category index, and value.

## Pan and zoom

Pan and zoom operate by updating `ChartModel.Request.WindowStart` and `WindowCount`. This keeps the interaction data-driven and works with downsampling.

```csharp
chartView.EnablePanZoom = true;
chartView.PanButton = MouseButton.Left;
chartView.PanModifiers = KeyModifiers.None;
chartView.ZoomModifiers = KeyModifiers.Control;
chartView.ZoomStep = 0.2;
```

Use `MinWindowCount` to prevent zooming in too far.

## Keyboard and mouse settings

You can customize gesture behavior:

- `PanButton` and `PanModifiers`
- `ZoomModifiers`
- `ZoomStep`

These settings allow you to align ProCharts with your application's interaction conventions.
