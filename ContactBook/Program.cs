using Avalonia;
using System;

namespace ContactBook;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .RegisterActiproLicense("Future Actipro Customer", "AVA251-R4GDT-FTQLB-WHVF4-4V3H");
}