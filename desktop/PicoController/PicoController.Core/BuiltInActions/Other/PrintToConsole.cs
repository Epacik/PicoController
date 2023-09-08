using PicoController.Core;

namespace PicoController.Core.BuiltInActions.Other;

internal class PrintToConsole : IPluginAction
{
    public async Task ExecuteAsync(int inputValue, string? data)
    {
        await Task.Yield();
        Console.WriteLine(data);
    }
}
