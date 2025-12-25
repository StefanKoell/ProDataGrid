using System;
using Avalonia.Controls;
using Avalonia.Rendering;

namespace DataGridSample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ApplyTabFilter();
        
        // RendererDiagnostics.DebugOverlays = RendererDebugOverlays.Fps | RendererDebugOverlays.LayoutTimeGraph | RendererDebugOverlays.RenderTimeGraph;
    }

    private void OnTabFilterChanged(object? sender, TextChangedEventArgs e)
    {
        ApplyTabFilter();
    }

    private void ApplyTabFilter()
    {
        if (SampleTabs == null)
        {
            return;
        }

        var filter = TabFilterBox?.Text;
        var hasFilter = !string.IsNullOrWhiteSpace(filter);
        var trimmed = hasFilter ? filter!.Trim() : string.Empty;

        TabItem? firstVisible = null;
        var selectionHidden = false;

        foreach (var item in SampleTabs.Items)
        {
            if (item is not TabItem tabItem)
            {
                continue;
            }

            var headerText = tabItem.Header?.ToString() ?? string.Empty;
            var isVisible = !hasFilter || headerText.Contains(trimmed, StringComparison.OrdinalIgnoreCase);
            tabItem.IsVisible = isVisible;

            if (isVisible && firstVisible == null)
            {
                firstVisible = tabItem;
            }

            if (ReferenceEquals(SampleTabs.SelectedItem, tabItem) && !isVisible)
            {
                selectionHidden = true;
            }
        }

        if (selectionHidden || (SampleTabs.SelectedItem == null && firstVisible != null))
        {
            SampleTabs.SelectedItem = firstVisible;
        }
    }
}
