using System;

namespace Avalonia.Controls;

internal static partial class DataGridDiagnostics
{
    public static bool IsEnabled { get; }

    static DataGridDiagnostics()
    {
        IsEnabled = InitializeIsEnabled();
        if (!IsEnabled)
        {
            return;
        }

        InitActivitySource();
        InitMetrics();
    }

    private static bool InitializeIsEnabled()
        => IsSwitchEnabled(AppContextSwitchName);

    private static bool IsSwitchEnabled(string name)
        => AppContext.TryGetSwitch(name, out var isEnabled) && isEnabled;
}
