// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Avalonia.Controls
{
    /// <summary>
    /// Defines the severity of a validation result.
    /// </summary>
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    enum DataGridValidationSeverity
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// Describes a validation message with a severity level.
    /// </summary>
#if !DATAGRID_INTERNAL
    public
#else
    internal
#endif
    sealed class DataGridValidationResult
    {
        public DataGridValidationResult(string message, DataGridValidationSeverity severity = DataGridValidationSeverity.Error)
        {
            Message = message;
            Severity = severity;
        }

        public string Message { get; }

        public DataGridValidationSeverity Severity { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
