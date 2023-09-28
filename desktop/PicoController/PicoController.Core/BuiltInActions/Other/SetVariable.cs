using PicoController.Plugin;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.BuiltInActions.Other;

public class SetVariable : IPluginAction
{
    private readonly IStorage _storage;
    private readonly ILogger? _logger;

    public SetVariable(IStorage storage, ILogger? logger)
    {
        _storage = storage;
        _logger = logger;
    }
    public Task ExecuteAsync(int inputValue, string? data)
    {
        var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        var splitted = data?.Split(";", options) ?? throw new ArgumentNullException(nameof(data));
        if (splitted.Length == 0)
            throw new ArgumentException("No variable name specified", nameof(data));

        return Task.Run(() =>
        {
            var variable = splitted[0];
            var value = splitted.Length >= 2 ? splitted[1] : null;

            if(_logger?.IsEnabled(LogEventLevel.Verbose) == true)
            {
                _logger.Verbose("Setting {Variable} to {Value}", variable, value ?? "<NULL>");
            }

            _storage.Set(variable, value);
        });
    }
}
