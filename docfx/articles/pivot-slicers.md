# Pivot Slicers and Report Filters

Pivot slicers expose facet lists with counts that drive `PivotFieldFilter` selections. Use them for Excel-style report filters alongside `PivotTableModel`.

## Slicer model

```csharp
var regionField = new PivotAxisField
{
    Header = "Region",
    ValueSelector = item => ((Sale)item!).Region
};

pivot.FilterFields.Add(regionField);

var slicer = new PivotSlicerModel
{
    ItemsSource = sales,
    Field = regionField,
    FilterMode = PivotSlicerFilterMode.Include
};
```

Bind the slicer items in XAML:

```xml
<ItemsControl ItemsSource="{Binding RegionSlicer.Items}">
  <ItemsControl.ItemTemplate>
    <DataTemplate>
      <StackPanel Orientation="Horizontal" Spacing="6">
        <CheckBox Content="{Binding Display}"
                  IsChecked="{Binding IsSelected, Mode=TwoWay}" />
        <TextBlock Text="{Binding Count}" Opacity="0.6" />
      </StackPanel>
    </DataTemplate>
  </ItemsControl.ItemTemplate>
</ItemsControl>
```

## Selection helpers

`PivotSlicerModel` includes helper methods:

- `SelectAll()`
- `ClearSelection()`
- `InvertSelection()`

`FilterMode` toggles whether selected items are included or excluded. `SelectionMode` can be `Single` or `Multiple`.

## Value filters

For numeric filters (Top/Bottom, thresholds), use `PivotValueFilterModel` to update `PivotAxisField.ValueFilter`:

```csharp
var valueFilter = new PivotValueFilterModel
{
    Field = categoryField,
    ValueField = salesField,
    FilterType = PivotValueFilterType.Top,
    Count = 5
};
```

## Notes

- Slicer counts are computed from the `ItemsSource` using the field's `GroupSelector` and formatting rules.
- Use `PivotAxisField.ShowItemsWithNoData` and `ItemsSource` to include values with zero counts.

## Sample

Run the sample app and open the "Pivot Slicers" tab for interactive slicers and a top-N value filter.
