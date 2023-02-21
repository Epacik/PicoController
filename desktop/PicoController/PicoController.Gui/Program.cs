using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using PicoController.Gui.Converters;
using PicoController.Gui.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;

namespace PicoController.Gui;

internal static class Program
{
    private const string AppMutexId = "{4157A473-1393-499D-A70F-AA03B561C8FC}";
    private const string FocusWindowMessage = "Focus_Window";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {

        using var mutex = new Mutex(false, AppMutexId);
        if (mutex?.WaitOne(5000, false) == true)
        {
            Start(args);
            mutex?.ReleaseMutex();
        }
        else
        {
            //BringExistingInstanceIntoFocus();
            Environment.Exit(-1);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions
            {
                UseWindowsUIComposition = true,
            })
            .With(new X11PlatformOptions { })
            .With(new AvaloniaNativePlatformOptions { })
            .LogToTrace()
            .UseReactiveUI();

    private static void Start(string[] args)
    {
        const string pluginDirArgName = "-ConfigDir=";
        var customMainDir = Array.Find(args, x => x.StartsWith(pluginDirArgName))
            ?.Replace(pluginDirArgName, "");

        Bootstrapper.Register(Locator.CurrentMutable, Locator.Current, customMainDir);
        var logger = Locator.Current.GetService<Serilog.ILogger>();

        var start = DateTime.Now;

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location)!;
            if ((DateTime.Now - start).TotalSeconds > 10)
                Process.Start(Path.Combine(location, "Restarter", "PicoController.RestartAfterCrash.exe"));

            logger?.Fatal("An unhandled error occured {ExceptionObject}", e.ExceptionObject);
        };

        //ListenToOtherInstances();

        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

    //private static void ListenToOtherInstances()
    //{
    //    new Thread(() =>
    //    {
    //        var logger = Locator.Current.GetService<Serilog.ILogger>();

    //        if (OperatingSystem.IsWindows())
    //        {
    //            using var pipe = new NamedPipeServerStream(AppMutexId, PipeDirection.In);

    //            while (true)
    //            {
    //                try
    //                {
    //                    pipe.WaitForConnection();

    //                    using var reader = new StreamReader(pipe);
    //                    var str = reader.ReadToEnd();

    //                    if(str == FocusWindowMessage)
    //                    {
    //                        Dispatcher.UIThread.Post(() => App.FocusMainWindow());
    //                    }
    //                }
    //                catch (IOException ex)
    //                {
    //                    logger?.Warning("Exception occured while listening to a named pipe {Ex}", ex);
    //                }
    //            }
    //        }
    //    }).Start();
    //}

    //private static void BringExistingInstanceIntoFocus()
    //{
    //    if (OperatingSystem.IsWindows())
    //    {
    //        using var pipe = new NamedPipeClientStream(".", AppMutexId, PipeDirection.Out, PipeOptions.None);
    //        pipe.Connect();

    //        using var writer = new StreamWriter(pipe);
    //        writer.WriteLine(FocusWindowMessage);
    //        //pipe.Close();
    //    }
    //}
}
