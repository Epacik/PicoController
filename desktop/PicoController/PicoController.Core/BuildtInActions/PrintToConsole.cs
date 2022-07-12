using PicoController.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.BuildtInActions;

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
