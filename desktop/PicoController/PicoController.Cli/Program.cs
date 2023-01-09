using PicoController.Core.Config;
using Serilog.Formatting.Compact;
using Serilog;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Serilog.Events;
using Splat;
using Serilog.Sinks.SystemConsole.Themes;
using PicoController.Cli;
using PicoController.Core;
using PicoController.Core.DependencyInjection;
using PicoController.Core.Extensions;
using PicoController.Core.Devices;

Bootstrapper.Register(Splat.Locator.CurrentMutable, Locator.Current);


var rootCommand = new RootCommand("Program communicating with a PicoController");
var pluginPathOption = new Option<DirectoryInfo?>("PluginDir");
rootCommand.AddOption(pluginPathOption);

rootCommand.SetHandler(
    dir => App.Run(
        dir,
        GetRequiredService<IPluginManager>(),
        GetRequiredService<IDeviceManager>(),
        GetRequiredService<IConfigRepository>(),
        GetRequiredService<ILocationProvider>(),
        GetRequiredService<Serilog.ILogger>()),
    pluginPathOption);


var handlersCommand = new Command("handlers", "Show all available handlers");
handlersCommand.SetHandler(
    h => Handlers.ShowActions(h, GetRequiredService<IPluginManager>()));

var handlerCommand = new Command("handler", "Show informations about specific handler");

var handlerArgument = new Argument<string>("handler")
{
    Arity = ArgumentArity.ExactlyOne,
};

handlerCommand.Add(handlerArgument);
handlerCommand.SetHandler(
    h => Handlers.Handler(h, GetRequiredService<IPluginManager>()),
    new Handlers.HandlerBinder(handlerArgument));


rootCommand.AddCommand(handlersCommand);
rootCommand.AddCommand(handlerCommand);

var builder = new CommandLineBuilder(rootCommand)
    .UseDefaults()
    .UseHelp();

var parser = builder.Build();

return await parser.InvokeAsync(args);


T? GetService<T>() => Locator.Current.GetService<T>();
T GetRequiredService<T>() => Locator.Current.GetRequiredService<T>();
