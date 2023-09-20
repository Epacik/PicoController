using CliWrap;
using PicoController.Core;
using System.Diagnostics;

namespace PicoController.Core.BuildtInActions.Other;

internal class RunProgram : IPluginAction
{
    public async Task ExecuteAsync(int inputValue, string? data)
    {
        if(data is null)
            throw new ArgumentNullException("data");

        await Task.Yield();
        var args = data.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var wrapper = Cli.Wrap(args[0]);

        if (args.Length > 0)
        {
            for (int i = 1; i < args.Length; i++)
            {
                wrapper = wrapper.WithArguments(args[i]);
            }
        }

        _ = wrapper.ExecuteAsync();

    }
}
