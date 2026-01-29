# ProCharts Axes and Scales

Axes are configured through `ChartAxisDefinition`. Each chart has primary and secondary category/value axes.

## Axis kinds

- `Category`
- `Value`
- `DateTime`
- `Logarithmic`

```csharp
model.ValueAxis.Kind = ChartAxisKind.Logarithmic;
model.CategoryAxis.Kind = ChartAxisKind.DateTime;
```

## Axis ranges

You can set explicit bounds:

```csharp
model.ValueAxis.Minimum = 0;
model.ValueAxis.Maximum = 1000;
```

## Label formatting

Provide a formatter for values and dates:

```csharp
model.ValueAxis.LabelFormatter = v => v.ToString("#,##0");
```

## Axis crossing and offsets

- `Crossing`: auto, min, max, or a specific value.
- `CrossingValue`: used when crossing is set to `Value`.
- `Offset`: shifts the axis away from the plot area.

```csharp
model.ValueAxis.Crossing = ChartAxisCrossing.Value;
model.ValueAxis.CrossingValue = 0;
```

## Minor ticks and gridlines

- `MinorTickCount`
- `ShowMinorTicks`
- `ShowMinorGridlines`

```csharp
model.ValueAxis.MinorTickCount = 4;
model.ValueAxis.ShowMinorGridlines = true;
```

## Secondary axes

Assign series to secondary axes with `ChartValueAxisAssignment.Secondary`.

```csharp
new ChartSeriesSnapshot(
    name: "Revenue",
    kind: ChartSeriesKind.Line,
    values: values,
    valueAxisAssignment: ChartValueAxisAssignment.Secondary)
```
