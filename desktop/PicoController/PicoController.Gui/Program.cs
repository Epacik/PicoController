using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using PicoController.Gui.Converters;
using System;

namespace PicoController.Gui
{
    internal static class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new Win32PlatformOptions 
                {
                    UseCompositor = true,
                    UseDeferredRendering = false,
                    UseWindowsUIComposition = true,
                })
                .With(new X11PlatformOptions { UseCompositor = true })
                .With(new AvaloniaNativePlatformOptions { UseCompositor = true })
                .LogToTrace()
                .UseReactiveUI();
    }
}
