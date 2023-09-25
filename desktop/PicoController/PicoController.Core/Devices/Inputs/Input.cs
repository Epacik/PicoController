using PicoController.Core.Extensions;
using Serilog;
using Serilog.Core;
using System.Collections.Immutable;
using System.Text;
using Tiger.Clock;

namespace PicoController.Core.Devices.Inputs;

public abstract class Input
{
    private readonly IHandlerProvider _handlerProvider;
    private readonly Func<string, Func<int, string, Task>?> _getAction;
    protected readonly ILogger _logger;

    private readonly string _deviceId;
    public byte Id { get; }
    public abstract InputType InputType { get; }
    public bool Split { get;}
    
    protected Input(
        string deviceId,
        byte inputId,
        IHandlerProvider handlerProvider,
        Func<string, Func<int, string, Task>?> getAction,
        ILogger logger,
        bool split = false)
    {
        _deviceId = deviceId;
        Id = inputId;
        _handlerProvider = handlerProvider;
        _getAction = getAction;
        _logger = logger;
        Split = split;
    }
    public abstract ImmutableArray<string> GetActions();

    public async Task Execute(InputMessage message)
    {
        if (Id != message.InputId)
            throw new ArgumentException("Message Id does not match Input Id");
        if(InputType != message.InputType)
            throw new ArgumentException("Message Type does not match Input Type");

        await ExecuteInternal(message);
    }

    protected abstract Task ExecuteInternal(InputMessage message);

    protected async Task InvokeAction(int inputValue, string actionName)
    {
        _logger.Information("Device: {DeviceId}, input: {Id}, action: {ActionName}", _deviceId, Id, actionName);

        var handler = await _handlerProvider.GetHandler(
            _deviceId,
            Id,
            actionName);

        if (handler is null || handler.Handler is null)
        {
            if (_logger.ExistsAndIsEnabled(Serilog.Events.LogEventLevel.Verbose))
            {
                _logger?.Verbose(
                    "Handler for action not found: {ActionName}",
                    actionName);
            }
            
            return;
        }

        var action = _getAction(handler.Handler);

        if (action is null)
        {
            if (_logger.ExistsAndIsEnabled(Serilog.Events.LogEventLevel.Verbose))
            {
                _logger?.Verbose(
                    "Invalid handler: {Handler}",
                    handler.Handler);
            }
            return;
        }

        try
        {
            await action!.Invoke(handler.InputValueOverride ?? inputValue, handler.Data!);
        }
        catch (Exception ex)
        {
            _logger.Error(ex,
                "An action thrown an exception\n" +
                "Device: {DeviceId}, input: {Id}, action: {ActionName}\n", _deviceId, Id, actionName);
        }

        //if (Actions.TryGetValue(actionName, out Func<int, Task>? value) && value is not null)
        //{
            
        //}
        //else if (_logger.ExistsAndIsEnabled(Serilog.Events.LogEventLevel.Verbose))
        //{
        //    _logger?.Verbose("Action not found: {ActionName}", actionName);
        //}
    }

}
