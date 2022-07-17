using PicoController.Plugin;

namespace PicoController.Core.BuiltInActions.Other;

internal class PrintToConsole : IPluginAction
{
    public void Execute(string? argument)
    {
        Console.WriteLine(argument);
    }

    public async Task ExecuteAsync(string? argument)
    {
        await Task.Yield();
        Console.WriteLine(argument);
    }
}
