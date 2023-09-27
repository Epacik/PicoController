using PicoController.Core.Extensions;
using PicoController.Plugin;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core;

public class InvokeHandler : IInvokeHandler
{
    private readonly IPluginManager _pluginManager;
    private readonly ILogger? _logger;

    public InvokeHandler(
        IPluginManager pluginManager,
        ILogger? logger)
    {
        _pluginManager = pluginManager;
        _logger = logger;
    }
    public void Invoke(string handler, int inputValue, string? data)
    {
        InvokeAsync(handler, inputValue, data).GetAwaiter().GetResult();
    }

    public async Task InvokeAsync(string handler, int inputValue, string? data)
    {
        if (_logger.ExistsAndIsEnabled(LogEventLevel.Verbose))
            _logger?.Verbose("");

        var action = _pluginManager.GetAction(handler);
        if (action is null)
        {
            if (_logger.ExistsAndIsEnabled(LogEventLevel.Warning))
                _logger?.Warning("Handler not found {Handler}", handler);
            return;
        }

        await action.Invoke(inputValue, data);
    }
}
