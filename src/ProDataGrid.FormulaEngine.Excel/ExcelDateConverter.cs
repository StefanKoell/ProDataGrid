// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#nullable enable

using System;
using ProDataGrid.FormulaEngine;

namespace ProDataGrid.FormulaEngine.Excel
{
    public static class ExcelDateConverter
    {
        public static bool TryConvert(DateTime value, FormulaDateSystem dateSystem, out double serial)
        {
            return ExcelDateUtilities.TryCreateSerialFromDateTime(value, dateSystem, out serial, out _);
        }
    }
}
