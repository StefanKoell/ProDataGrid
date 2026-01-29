# Formula Engine Arrays and Spills

The engine supports Excel-style arrays, including array literals, dynamic array functions, and spill behavior.

## Array literals

Array literals use braces with comma-separated columns and semicolon-separated rows:

```text
{1,2,3;4,5,6}
```

- Commas separate columns.
- Semicolons separate rows.

## Dynamic arrays

Functions like `SEQUENCE`, `SORT`, `UNIQUE`, and `FILTER` return arrays. When dynamic arrays are enabled, array results spill into adjacent cells.

```text
=SEQUENCE(3,2)
```

## Implicit intersection

When a formula expects a single value but receives a range or array, the evaluator applies implicit intersection. In grid scenarios, the intersection uses the formula cell address to pick the matching value.

## Spill ranges

- If a spill range overlaps existing content, the engine returns `#SPILL!`.
- Spill ranges are tracked so updates invalidate the correct cells.
- Spill owners and spill values are cached to avoid full refreshes.

## Array evaluation in DataGrid

DataGrid formula columns render spilled values across adjacent formula columns. This enables Excel-style dynamic arrays inside grids with live recalculation.
