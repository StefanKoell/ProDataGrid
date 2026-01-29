// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System.Globalization;

namespace ProDataGrid.FormulaEngine
{
    public sealed class FormulaFormatOptions
    {
        public FormulaReferenceMode ReferenceMode { get; set; } = FormulaReferenceMode.A1;

        public char ArgumentSeparator { get; set; } = ',';

        public char DecimalSeparator { get; set; } = '.';

        public bool IncludeLeadingEquals { get; set; }

        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
    }

    public interface IFormulaFormatter
    {
        string Format(FormulaExpression expression, FormulaFormatOptions options);
    }
}
