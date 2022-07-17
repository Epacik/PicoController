using PicoController.Core;
using System;
using System.Collections.Generic;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Cli;

internal static class Help
{
    public static void ShowAll(InvocationContext handler)
    {
        //var helpBuilder = handler.HelpBuilder;
        //var helpContext = new HelpContext(helpBuilder)
    }

    internal static void ShowActions(object handler)
    {
        Plugins.LoadPlugins();
        var handlers = Plugins.AllAvailableActions();
        Plugins.UnloadPlugins();
        Console.WriteLine("All available handlers:");
        Console.WriteLine(String.Join("\n", handlers));
    }
}
