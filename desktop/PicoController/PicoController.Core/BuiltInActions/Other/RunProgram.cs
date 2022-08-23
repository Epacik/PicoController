using PicoController.Core;
using System.Diagnostics;

namespace PicoController.Core.BuildtInActions.Other;

internal class RunProgram : IPluginAction
{

    public async Task ExecuteAsync(string? argument)
    {
        if(argument is null)
            throw new ArgumentNullException("data");

        await Task.Yield();
        var args = argument.Split(';');
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "CMD",
            Arguments = "/C" + argument,
            UseShellExecute = false,
            CreateNoWindow = true,
        });

    }
}
