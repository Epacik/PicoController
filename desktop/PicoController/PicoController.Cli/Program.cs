using PicoController.Core.Config;
using Serilog.Formatting.Compact;
using Serilog;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Serilog.Events;
using Splat;

namespace PicoController.Cli;

internal static class Program
{
    private static ConsoleColor _defaultForeground;
    private static ConsoleColor _defaultBackground;

    static async Task<int> Main(string[] args)
    {
        var logger = CreateLogger();
        Log.Logger = logger;
        Splat.Locator.CurrentMutable.RegisterLazySingleton<Serilog.ILogger>(() => logger);

        _defaultForeground = Console.ForegroundColor;
        _defaultBackground = Console.BackgroundColor;
        var rootCommand = new RootCommand("Program communicating with a PicoController");
        rootCommand.SetHandler(handler => DefaultBehavior.Run());

        var handlersCommand = new Command("handlers", "Show all available handlers");
        handlersCommand.SetHandler(handler => Handlers.ShowActions(handler));

        var handlerCommand = new Command("handler", "Show informations about specific handler");
        
        var handlerArgument = new Argument<string>("handler")
        {
            Arity = ArgumentArity.ExactlyOne,
        };

        handlerCommand.Add(handlerArgument);
        handlerCommand.SetHandler(
            (handler) => Handlers.Handler(handler),
            new Handlers.HandlerBinder(handlerArgument));
        

        rootCommand.AddCommand(handlersCommand);
        rootCommand.AddCommand(handlerCommand);

        var builder = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .UseHelp();

        var parser = builder.Build();
        
        return await parser.InvokeAsync(args);
    }


    public static void PrintInColor(string message, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        if (foreground is not null)
            Console.ForegroundColor = (ConsoleColor)foreground;

        if (background is not null)
            Console.BackgroundColor = (ConsoleColor)background;

        Console.WriteLine(message);
        Console.ForegroundColor = _defaultForeground;
        Console.BackgroundColor = _defaultBackground;
    }

    private static Serilog.ILogger CreateLogger()
    {
        var cfgPath = ConfigRepository.ConfigDirectory();
        var jsonFormatter = new CompactJsonFormatter();
        var config = new LoggerConfiguration()
            .WriteTo.Async(
                x => x.File(
                    Path.Combine(cfgPath, "Logs", "Text", "log-.log"),
                    LogEventLevel.Information,
                    rollingInterval: RollingInterval.Hour))
            .WriteTo.Async(
                x => x.File(
                    jsonFormatter,
                    Path.Combine(cfgPath, "Logs", "JSON", "log-.json"),
                    LogEventLevel.Information,
                    rollingInterval: RollingInterval.Hour))
            .WriteTo.Async(
                x => x.EventLog("PicoController GUI", restrictedToMinimumLevel: LogEventLevel.Warning))
            .WriteTo.Console();

        //if (Environment.GetEnvironmentVariable("SERILOG_DISCORD_WEBHOOK") is string x)
        //{
        //    var (idStr, token) = x.Split('/');
        //    if (ulong.TryParse(idStr, out ulong id))
        //        config.WriteTo.Async(x => x.Discord(id, token, restrictedToMinimumLevel: LogEventLevel.Error));
        //}

        return config.CreateLogger();
    }
}