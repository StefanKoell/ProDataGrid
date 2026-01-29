using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace DataGridSample.Models
{
    public sealed class FormulaSampleWorkbook : IFormulaWorkbook,
        IFormulaWorkbookResolver,
        IFormulaNameProvider,
        IFormulaNameChangeNotifier,
        IFormulaStructuredReferenceResolver,
        IFormulaStructuredReferenceDependencyResolver
    {
        private readonly Dictionary<string, FormulaSampleWorksheet> _worksheets =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly List<IFormulaWorksheet> _worksheetList = new();
        private readonly FormulaNameTable _names = new();
        private readonly Dictionary<string, FormulaSampleTable> _tables =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IFormulaWorkbook> _externalWorkbooks =
            new(StringComparer.OrdinalIgnoreCase);

        public FormulaSampleWorkbook(string name, FormulaCalculationSettings settings)
        {
            Name = name;
            Settings = settings;
            _names.NameChanged += (_, args) => NameChanged?.Invoke(this, args);
        }

        public event EventHandler<FormulaNameChangedEventArgs>? NameChanged;

        public string Name { get; }

        public IReadOnlyList<IFormulaWorksheet> Worksheets => _worksheetList;

        public FormulaCalculationSettings Settings { get; }

        public FormulaNameTable NameTable => _names;

        public FormulaSampleWorksheet AddWorksheet(string name)
        {
            var sheet = new FormulaSampleWorksheet(name, this);
            _worksheets[name] = sheet;
            _worksheetList.Add(sheet);
            return sheet;
        }

        public void AddExternalWorkbook(IFormulaWorkbook workbook)
        {
            _externalWorkbooks[workbook.Name] = workbook;
        }

        public void AddTable(FormulaSampleTable table)
        {
            _tables[table.Name] = table;
        }

        public IFormulaWorksheet GetWorksheet(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Sheet name is required.", nameof(name));
            }

            if (_worksheets.TryGetValue(name, out var sheet))
            {
                return sheet;
            }

            throw new InvalidOperationException($"Worksheet '{name}' not found.");
        }

        public bool TryGetWorkbook(string name, out IFormulaWorkbook workbook)
        {
            return _externalWorkbooks.TryGetValue(name, out workbook!);
        }

        public bool TryGetName(string name, out FormulaExpression expression)
        {
            return _names.TryGetName(name, out expression);
        }

        public bool TryResolveStructuredReference(
            FormulaEvaluationContext context,
            FormulaStructuredReference reference,
            out FormulaValue value)
        {
            value = FormulaValue.FromError(new FormulaError(FormulaErrorType.Name));
            if (!TryResolveTable(context, reference, out var table))
            {
                return true;
            }

            if (!TryResolveStructuredReferenceRange(context, reference, table, out var rowStart, out var rowEnd, out var columnStart, out var columnEnd))
            {
                value = FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
                return true;
            }

            var rows = rowEnd - rowStart + 1;
            var columns = columnEnd - columnStart + 1;
            if (rows <= 0 || columns <= 0)
            {
                value = FormulaValue.FromError(new FormulaError(FormulaErrorType.Ref));
                return true;
            }

            if (rows == 1 && columns == 1)
            {
                var cell = table.Worksheet.GetCell(rowStart, columnStart);
                value = cell.Value;
                return true;
            }

            var origin = new FormulaCellAddress(table.Worksheet.Name, rowStart, columnStart);
            var array = new FormulaArray(rows, columns, origin);
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    var cell = table.Worksheet.GetCell(rowStart + row, columnStart + column);
                    array[row, column] = cell.Value;
                }
            }

            value = FormulaValue.FromArray(array);
            return true;
        }

        public bool TryGetStructuredReferenceDependencies(
            FormulaStructuredReference reference,
            out IEnumerable<FormulaCellAddress> dependencies)
        {
            dependencies = Array.Empty<FormulaCellAddress>();
            if (!TryResolveTable(null, reference, out var table))
            {
                return false;
            }

            if (!TryResolveStructuredReferenceRange(null, reference, table, out var rowStart, out var rowEnd, out var columnStart, out var columnEnd))
            {
                return false;
            }

            var list = new List<FormulaCellAddress>();
            for (var row = rowStart; row <= rowEnd; row++)
            {
                for (var column = columnStart; column <= columnEnd; column++)
                {
                    list.Add(new FormulaCellAddress(table.Worksheet.Name, row, column));
                }
            }

            dependencies = list;
            return true;
        }

        private bool TryResolveTable(
            FormulaEvaluationContext? context,
            FormulaStructuredReference reference,
            out FormulaSampleTable table)
        {
            table = null!;
            if (!string.IsNullOrWhiteSpace(reference.TableName))
            {
                if (_tables.TryGetValue(reference.TableName, out table))
                {
                    return IsSheetMatch(reference, table);
                }

                return false;
            }

            if (context == null)
            {
                return false;
            }

            foreach (var candidate in _tables.Values)
            {
                if (string.Equals(candidate.Worksheet.Name, context.Worksheet.Name, StringComparison.OrdinalIgnoreCase))
                {
                    table = candidate;
                    return IsSheetMatch(reference, table);
                }
            }

            return false;
        }

        private static bool IsSheetMatch(FormulaStructuredReference reference, FormulaSampleTable table)
        {
            if (reference.Sheet == null)
            {
                return true;
            }

            var sheetName = reference.Sheet.Value.StartSheetName;
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                return true;
            }

            return string.Equals(sheetName, table.Worksheet.Name, StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryResolveStructuredReferenceRange(
            FormulaEvaluationContext? context,
            FormulaStructuredReference reference,
            FormulaSampleTable table,
            out int rowStart,
            out int rowEnd,
            out int columnStart,
            out int columnEnd)
        {
            rowStart = 0;
            rowEnd = 0;
            columnStart = 0;
            columnEnd = 0;

            if (string.IsNullOrWhiteSpace(reference.ColumnStart))
            {
                if (table.Columns.Count == 0)
                {
                    return false;
                }

                var min = int.MaxValue;
                var max = int.MinValue;
                foreach (var column in table.Columns.Values)
                {
                    if (column < min)
                    {
                        min = column;
                    }
                    if (column > max)
                    {
                        max = column;
                    }
                }

                columnStart = min;
                columnEnd = max;
            }
            else if (!table.TryGetColumnIndex(reference.ColumnStart, out columnStart))
            {
                return false;
            }

            if (reference.IsColumnRange && !string.IsNullOrWhiteSpace(reference.ColumnStart))
            {
                if (!table.TryGetColumnIndex(reference.ColumnEnd, out columnEnd))
                {
                    return false;
                }
            }
            else
            {
                if (columnEnd == 0)
                {
                    columnEnd = columnStart;
                }
            }

            switch (reference.Scope)
            {
                case FormulaStructuredReferenceScope.Headers:
                    rowStart = table.HeaderRow;
                    rowEnd = table.HeaderRow;
                    return true;
                case FormulaStructuredReferenceScope.ThisRow:
                {
                    if (context == null)
                    {
                        return false;
                    }

                    var row = context.Address.Row;
                    if (row < table.DataStartRow || row > table.DataEndRow)
                    {
                        return false;
                    }

                    rowStart = row;
                    rowEnd = row;
                    return true;
                }
                case FormulaStructuredReferenceScope.Totals:
                    if (!table.TotalsRow.HasValue)
                    {
                        return false;
                    }

                    rowStart = table.TotalsRow.Value;
                    rowEnd = table.TotalsRow.Value;
                    return true;
                case FormulaStructuredReferenceScope.All:
                    rowStart = table.HeaderRow;
                    rowEnd = table.TotalsRow ?? table.DataEndRow;
                    return true;
                case FormulaStructuredReferenceScope.Data:
                case FormulaStructuredReferenceScope.None:
                    rowStart = table.DataStartRow;
                    rowEnd = table.DataEndRow;
                    return true;
                default:
                    return false;
            }
        }
    }

    public sealed class FormulaSampleWorksheet : IFormulaWorksheet, IFormulaNameProvider, IFormulaNameChangeNotifier
    {
        private readonly Dictionary<(int Row, int Column), FormulaSampleCell> _cells = new();
        private readonly FormulaNameTable _names = new();

        public FormulaSampleWorksheet(string name, IFormulaWorkbook workbook)
        {
            Name = name;
            Workbook = workbook;
            _names.NameChanged += (_, args) => NameChanged?.Invoke(this, args);
        }

        public event EventHandler<FormulaNameChangedEventArgs>? NameChanged;

        public string Name { get; }

        public IFormulaWorkbook Workbook { get; }

        public FormulaNameTable NameTable => _names;

        public IEnumerable<FormulaSampleCell> Cells => _cells.Values;

        public IFormulaCell GetCell(int row, int column)
        {
            if (!_cells.TryGetValue((row, column), out var cell))
            {
                cell = new FormulaSampleCell(new FormulaCellAddress(Name, row, column));
                _cells[(row, column)] = cell;
            }

            return cell;
        }

        public bool TryGetCell(int row, int column, out IFormulaCell cell)
        {
            if (_cells.TryGetValue((row, column), out var found))
            {
                cell = found;
                return true;
            }

            cell = null!;
            return false;
        }

        public void SetValue(int row, int column, FormulaValue value)
        {
            var cell = (FormulaSampleCell)GetCell(row, column);
            cell.Value = value;
        }

        public bool TryGetName(string name, out FormulaExpression expression)
        {
            return _names.TryGetName(name, out expression);
        }
    }

    public sealed class FormulaSampleCell : IFormulaCell
    {
        public FormulaSampleCell(FormulaCellAddress address)
        {
            Address = address;
            Value = FormulaValue.Blank;
        }

        public FormulaCellAddress Address { get; }

        public string? Formula { get; set; }

        public FormulaExpression? Expression { get; set; }

        public FormulaValue Value { get; set; }
    }
}
