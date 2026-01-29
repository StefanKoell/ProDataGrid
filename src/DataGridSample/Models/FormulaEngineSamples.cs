using System;
using System.Collections.Generic;
using ProDataGrid.FormulaEngine;

namespace DataGridSample.Models
{
    public sealed class FormulaSampleItem
    {
        public FormulaSampleItem(string description, string formula, string result, string kind, string? notes = null)
        {
            Description = description;
            Formula = formula;
            Result = result;
            Kind = kind;
            Notes = notes;
        }

        public string Description { get; }

        public string Formula { get; }

        public string Result { get; }

        public string Kind { get; }

        public string? Notes { get; }
    }

    public sealed class FormulaCellSnapshot
    {
        public FormulaCellSnapshot(string addressText, string? formula, string result, string kind)
        {
            AddressText = addressText;
            Formula = formula ?? string.Empty;
            Result = result;
            Kind = kind;
        }

        public string AddressText { get; }

        public string Formula { get; }

        public string Result { get; }

        public string Kind { get; }
    }

    public sealed class FormulaSampleTable
    {
        public FormulaSampleTable(
            string name,
            FormulaSampleWorksheet worksheet,
            int headerRow,
            int dataStartRow,
            int dataEndRow,
            IReadOnlyDictionary<string, int> columns,
            int? totalsRow = null)
        {
            Name = name;
            Worksheet = worksheet;
            HeaderRow = headerRow;
            DataStartRow = dataStartRow;
            DataEndRow = dataEndRow;
            Columns = columns;
            TotalsRow = totalsRow;
        }

        public string Name { get; }

        public FormulaSampleWorksheet Worksheet { get; }

        public int HeaderRow { get; }

        public int DataStartRow { get; }

        public int DataEndRow { get; }

        public int? TotalsRow { get; }

        public IReadOnlyDictionary<string, int> Columns { get; }

        public bool TryGetColumnIndex(string? name, out int column)
        {
            column = 0;
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return Columns.TryGetValue(name, out column);
        }
    }
}
