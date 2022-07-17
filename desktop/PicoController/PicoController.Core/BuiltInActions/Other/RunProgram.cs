using PicoController.Plugin;
using System.Diagnostics;

namespace PicoController.Core.BuildtInActions.Other;

internal class RunProgram : IPluginAction
{
    public void Execute(string? argument)
    {
        if (argument is null)
            throw new ArgumentNullException("data");

        var args = argument.Split(';');
        var process = args.Length < 2 ? Process.Start(argument) : Process.Start(args[0], args[1]);
    }

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
