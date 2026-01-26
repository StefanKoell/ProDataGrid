using Avalonia;
using Avalonia.Headless;
namespace Avalonia.Controls.DataGridTests;

internal sealed class LeakTestsApp : Application
{
    public LeakTestsApp()
    {
    }

    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<LeakTestsApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}
