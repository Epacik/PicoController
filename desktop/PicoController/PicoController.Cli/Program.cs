using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Text.Json;
using Microsoft.Win32;
using PicoController.Core;
using PicoController.Core.BuiltInActions.Other;

namespace PicoController.Cli
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Program communicating with a PicoController");
            rootCommand.SetHandler(handler => DefaultBehavior.Run());

            var helpCommand = new Command("--help-all", "Show help, and help for all available actions");
            helpCommand.AddAlias("-ha");
            helpCommand.SetHandler(handler => Help.ShowAll(handler));

            var availableActionsCommand = new Command("--available-handlers", "Show all available handlers");
            availableActionsCommand.AddAlias("--handlers");
            availableActionsCommand.AddAlias("-H");
            availableActionsCommand.SetHandler(handler => Help.ShowActions(handler));

            rootCommand.AddCommand(helpCommand);
            rootCommand.AddCommand(availableActionsCommand);

            var parser = new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .UseHelp()
                .Build();
            
            return await parser.InvokeAsync(args);
        }

        public static void PrintInColor(string message, ConsoleColor foreground, ConsoleColor? background = null)
        {
            var tempFg = Console.ForegroundColor;
            var tempBg = Console.BackgroundColor;
            Console.ForegroundColor = foreground;

            if (background is not null)
                Console.BackgroundColor = (ConsoleColor)background;

            Console.WriteLine(message);
            Console.ForegroundColor = tempFg;
            Console.BackgroundColor = tempBg;
        }
    }
}