using PicoController.Core;

namespace PicoController.Core.BuiltInActions.Other;

internal class PrintToConsole : IPluginAction
{
    public async Task ExecuteAsync(string? argument)
    {
        await Task.Yield();
        Console.WriteLine(argument);
    }
}
