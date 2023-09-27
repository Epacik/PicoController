using PicoController.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core;

public class HandlerProvider : IHandlerProvider
{
    private readonly IConfigRepository _configRepository;

    public HandlerProvider(IConfigRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public async Task<InputAction?> GetHandler(string deviceId, int inputId, string action)
    {
        var config = await _configRepository.ReadAsync();

        if (config is null)
            return null;

        var device = config.Devices.Find(x => x.Id == deviceId);

        if (device is null)
            return null;

        var input = Array.Find(device.Inputs, x => x.Id == inputId);

        if (input is null)
            return null;

        var handler = input.Actions
            .FirstOrDefault(x => x.Key == action)
            .Value;

        if (handler is null)
            return null;

        return handler;
    }
}

public interface IHandlerProvider
{
    Task<InputAction?> GetHandler(string deviceId, int inputId, string action);
}