// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;

namespace ProDataGrid.FormulaEngine
{
    public sealed class FormulaParseOptions
    {
        public FormulaReferenceMode ReferenceMode { get; set; } = FormulaReferenceMode.A1;

        public char ArgumentSeparator { get; set; } = ',';

        public char DecimalSeparator { get; set; } = '.';

        public bool AllowLeadingEquals { get; set; } = true;
    }

    public sealed class FormulaParseException : Exception
    {
        public FormulaParseException(string message, int position)
            : base(message)
        {
            Position = position;
        }

        public int Position { get; }
    }

    public interface IFormulaParser
    {
        FormulaExpression Parse(string formulaText, FormulaParseOptions options);
    }
}
