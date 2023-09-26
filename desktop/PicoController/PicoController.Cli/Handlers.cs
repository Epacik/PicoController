using PicoController.Core;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Cli;

internal static class Handlers
{
    public static void ShowAll(InvocationContext handler)
    {
        //var helpBuilder = handler.HelpBuilder;
        //var helpContext = new HelpContext(helpBuilder)
    }

    internal static void ShowActions(object h, IPluginManager pluginManager)
    {
        if(!pluginManager.AreLoaded)
            pluginManager.LoadPlugins();
        var handlers = pluginManager.GetAllAvailableActions();
        pluginManager.UnloadPlugins();
        Console.WriteLine("All available handlers:");
        Console.WriteLine(string.Join("\n", handlers));
    }

    internal static void Handler(string handler, IPluginManager pluginManager)
    {
        if (!pluginManager.AreLoaded)
            pluginManager.LoadPlugins();
        var info = pluginManager.GetHandlerInfo(handler);
        if (info?.ValidArguments is object) {
            Console.WriteLine(
                "Handler: {0} - {1}\n\nValid values:\n{2}",
                handler,
                info?.Description,
                string.Join("\n", info?.ValidArguments?.Select(x => $"{x.Key} - {x.Value}") ?? Array.Empty<string>())); 
        }
        else
        {
            Console.WriteLine(
                "Handler: {0} - {1}",
                handler,
                info?.Description);
        }
    }

    public class HandlerBinder : BinderBase<string>
    {
        private readonly Argument<string> _handlerArgument;

        public HandlerBinder(Argument<string> handlerArgument)
        {
            _handlerArgument = handlerArgument;
        }

        protected override string GetBoundValue(BindingContext bindingContext) => bindingContext.ParseResult.GetValueForArgument(_handlerArgument);
    }
}
