using PicoController.Core.Extensions;
using Serilog;
using Serilog.Core;
using System.Collections.Immutable;
using System.Text;
using Tiger.Clock;

namespace PicoController.Core.Devices.Inputs;

public abstract class Input
{
    private readonly int _deviceId;
    protected readonly ILogger _logger;

    public byte Id { get; }
    public InputType Type { get; }
    public Dictionary<string, Func<int, Task>?> Actions { get; } = new();
    public bool Split { get;}

    public ImmutableArray<string> AvailableActions { get; }
    
    protected Input(int deviceId, byte inputId, InputType type, IEnumerable<string> availableActions, Dictionary<string, Func<int, Task>?> actions, ILogger logger, bool split = false)
    {
        _deviceId = deviceId;
        Id = inputId;
        Type = type;
        Actions = actions;
        _logger = logger;
        Split = split;
        AvailableActions = availableActions.ToImmutableArray();
    }

    public async Task Execute(InputMessage message)
    {
        if (Id != message.InputId)
            throw new ArgumentException("Message Id does not match Input Id");
        if(Type != message.InputType)
            throw new ArgumentException("Message Type does not match Input Type");

        await ExecuteInternal(message);
    }

    protected abstract Task ExecuteInternal(InputMessage message);

    protected async Task InvokeAction(int inputValue, string actionName)
    {
        _logger.Information("Device: {DeviceId}, input: {Id}, action: {ActionName}", _deviceId, Id, actionName);

        if (Actions.TryGetValue(actionName, out Func<int, Task>? value) && value is not null)
        {
            try
            {
                await value!.Invoke(inputValue);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "An action thrown an exception\n" +
                    "Device: {DeviceId}, input: {Id}, action: {ActionName}\n", _deviceId, Id, actionName);
            }
        }
        else if (_logger.ExistsAndIsEnabled(Serilog.Events.LogEventLevel.Verbose))
        {
            _logger?.Verbose("Action not found: {ActionName}", actionName);
        }
    }

}
