# ProCharts Skia Style Reference

`SkiaChartStyle` exposes renderer-specific knobs for layout, axes, labels, and per-series styling. This is useful when you need fine control over the Skia renderer beyond the renderer-agnostic `ChartTheme`.

## Layout and padding

- `PaddingLeft`, `PaddingRight`, `PaddingTop`, `PaddingBottom`
- `Background`

## Axes and gridlines

- `Axis`, `AxisStrokeWidth`
- `ShowAxisLabels`, `ShowCategoryLabels`
- `AxisTickCount`
- `ShowGridlines`, `Gridline`, `GridlineStrokeWidth`
- `ShowCategoryGridlines`

Axis visibility and titles:

- `ShowCategoryAxisLine`, `ShowValueAxisLine`
- `CategoryAxisTitle`, `ValueAxisTitle`
- Secondary axis equivalents

Axis ranges and kinds:

- `CategoryAxisKind`, `ValueAxisKind`
- `CategoryAxisMinimum`, `CategoryAxisMaximum`
- `ValueAxisMinimum`, `ValueAxisMaximum`
- Secondary axis equivalents

Minor ticks and gridlines:

- `CategoryAxisMinorTickCount`, `ShowCategoryMinorTicks`, `ShowCategoryMinorGridlines`
- `ValueAxisMinorTickCount`, `ShowValueMinorTicks`, `ShowValueMinorGridlines`
- Secondary axis equivalents

Axis crossing and offsets:

- `CategoryAxisCrossing`, `CategoryAxisCrossingValue`, `CategoryAxisOffset`
- `ValueAxisCrossing`, `ValueAxisCrossingValue`, `ValueAxisOffset`
- Secondary axis equivalents

## Legend

- `ShowLegend`, `LegendPosition`
- `LegendFlow`, `LegendWrap`, `LegendMaxWidth`
- `LegendPadding`, `LegendSwatchSize`, `LegendSpacing`
- Grouping for stacked series: `LegendGroupStackedSeries`, `LegendStackedGroupTitle`, `LegendStandardGroupTitle`

## Data labels

- `ShowDataLabels`, `DataLabelTextSize`
- `DataLabelPadding`, `DataLabelOffset`
- `DataLabelBackground`, `DataLabelText`
- `DataLabelFormatter`, `SeriesDataLabelFormatter`

## Pie and donut

- `PieLabelPlacement`
- `PieLabelLeaderLineLength`, `PieLabelLeaderLineOffset`
- `PieLabelLeaderLineColor`, `PieLabelLeaderLineWidth`
- `PieInnerRadiusRatio`

## Series styling

- `SeriesStrokeWidth`, `AreaFillOpacity`
- `SeriesColors`
- `SeriesStyles` for per-series overrides

## Bubble, waterfall, and special charts

- Bubble: `BubbleMinRadius`, `BubbleMaxRadius`, `BubbleFillOpacity`, `BubbleStrokeWidth`
- Waterfall: `WaterfallIncreaseColor`, `WaterfallDecreaseColor`, `WaterfallConnectorColor`, `ShowWaterfallConnectors`
- Histogram: `HistogramBinCount`
- Box and whisker: `BoxWhiskerFillOpacity`, `BoxWhiskerOutlierRadius`, `BoxWhiskerShowOutliers`
- Radar: `RadarPointRadius`
- Funnel: `FunnelGap`, `FunnelMinWidthRatio`

## Trendlines and error bars

- `TrendlineStrokeWidth`
- `ErrorBarStrokeWidth`, `ErrorBarCapSize`

## Hit testing

- `HitTestRadius`

## Theme bridging

`SkiaChartStyle` can blend renderer-agnostic styles with Skia-specific ones:

- `CoreTheme` and `CoreSeriesStyles` are pulled from `ChartModel`.
- `Theme` and `SeriesStyles` apply Skia overrides on top.
