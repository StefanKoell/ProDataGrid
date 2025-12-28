using System;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace Avalonia.Controls.DataGridTests;

internal static class DiagnosticsTestInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        AppContext.SetSwitch(DataGridDiagnostics.AppContextSwitchName, true);
    }
}
