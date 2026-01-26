# Pivot Calculated Measures

Calculated measures let you define Excel-style formulas that evaluate after aggregation. The formulas are attached to `PivotValueField.Formula` and can reference other value fields by header or key.

## Basic setup

```csharp
var salesField = new PivotValueField
{
    Header = "Sales",
    ValueSelector = item => ((Sale)item!).Sales,
    AggregateType = PivotAggregateType.Sum
};

var profitField = new PivotValueField
{
    Header = "Profit",
    ValueSelector = item => ((Sale)item!).Profit,
    AggregateType = PivotAggregateType.Sum
};

var marginField = new PivotValueField
{
    Header = "Margin",
    AggregateType = PivotAggregateType.None,
    Formula = "Profit / Sales",
    StringFormat = "P1"
};

pivot.ValueFields.Add(salesField);
pivot.ValueFields.Add(profitField);
pivot.ValueFields.Add(marginField);
```

## Formula syntax

- Numbers: `10`, `3.14`, `1e3`.
- Operators: `+`, `-`, `*`, `/` with parentheses.
- Unary minus: `-Sales`.
- Field references by header/key: `Sales`, `Profit`.
- Bracketed names for spaces or punctuation: `[Gross Profit]`.

## Total functions

Use total functions to reference pivot totals in formulas:

- `RowTotal(Field)` - total across columns for the current row.
- `ColumnTotal(Field)` - total across rows for the current column.
- `GrandTotal(Field)` - grand total across the whole pivot.
- `ParentRowTotal(Field)` - total for the parent row group.
- `ParentColumnTotal(Field)` - total for the parent column group.

Example:

```csharp
new PivotValueField
{
    Header = "Share of Total",
    AggregateType = PivotAggregateType.None,
    Formula = "Sales / GrandTotal(Sales)",
    StringFormat = "P1"
};
```

## Notes

- Formulas evaluate on aggregated values, so they stay consistent for detail rows, subtotals, and grand totals.
- Calculated fields ignore `DisplayMode`; use formulas for percent-of totals or index-style measures.
- Formulas can reference other calculated fields, but circular references return `null`.
- The engine calculates required totals even if they are hidden in the layout.

## Sample

Run the sample app and open the "Pivot Calculated Measures" tab to see formula-driven KPIs in action.
