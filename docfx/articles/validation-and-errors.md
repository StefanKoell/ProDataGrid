# Validation and Error States

ProDataGrid uses Avalonia validation to surface invalid cells and rows. Any validation source that Avalonia recognizes (for example, `DataValidationException`, `INotifyDataErrorInfo`, or `IDataErrorInfo`) will mark the cell and row as invalid.

## Throw DataValidationException

Raise `DataValidationException` from your model setters to trigger inline errors:

```csharp
using Avalonia.Data;

public string Name
{
    get => _name;
    set
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DataValidationException("Name is required.");
        }

        _name = value;
        OnPropertyChanged();
    }
}
```

## Severity Levels

Use `DataGridValidationResult` to distinguish blocking errors from non-blocking warnings and info messages. Errors prevent commit; warnings and info allow commit but keep the cell highlighted.

```csharp
using Avalonia.Controls;
using Avalonia.Data;

public int Risk
{
    get => _risk;
    set
    {
        _risk = value;
        OnPropertyChanged();

        if (value >= 8)
        {
            throw new DataValidationException(
                new DataGridValidationResult("Risk of 8+ is a warning.", DataGridValidationSeverity.Warning));
        }
    }
}
```

## Style Warning and Info States

Warning and info states are exposed as `:warning` and `:info` on `DataGridCell`:

```xml
<Style Selector="DataGridCell:warning">
  <Setter Property="BorderBrush" Value="#F59E0B" />
  <Setter Property="BorderThickness" Value="1" />
</Style>

<Style Selector="DataGridCell:info">
  <Setter Property="BorderBrush" Value="#22C55E" />
  <Setter Property="BorderThickness" Value="1" />
</Style>

```

Built-in DataValidationErrors themes include `DataGridCellDataValidationErrorsTheme`, `DataGridCellDataValidationWarningsTheme`, and `DataGridCellDataValidationInfoTheme`. Warning/info cells surface the same inline indicator and tooltip as errors.

## Style Invalid Rows and Cells

Invalid rows and cells expose the `:invalid` pseudo-class:

```xml
<Style Selector="DataGridCell:invalid">
  <Setter Property="BorderBrush" Value="#E11D48" />
  <Setter Property="BorderThickness" Value="1" />
</Style>

<Style Selector="DataGridRow:invalid">
  <Setter Property="Background" Value="#FFF7E6" />
</Style>
```

You can also restyle the `DataValidationErrors` theme used by cell templates via `DataGridCellDataValidationErrorsTheme` in `Themes/Generic.xaml`.

## Sample Reference

See the `Validation` page in `src/DataGridSample` for validation across every editable column type (numeric, date/time, masked text, combo boxes, toggles, hyperlinks, and more) and `Validation Styling` for severity-based warnings/info.
