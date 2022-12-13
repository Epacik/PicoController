using System.Collections.Immutable;
using System.Text;
using Tiger.Clock;

namespace PicoController.Core.Devices.Inputs;

public abstract class InputBase
{
    private readonly int _deviceId;
    public byte Id { get; }
    public InputType Type { get; }
    public Dictionary<string, Func<int, Task>?> Actions { get; } = new();
    public bool Split { get;}

    public ImmutableArray<string> AvailableActions { get; }

    protected InputBase(int deviceId, byte inputId, InputType type, IEnumerable<string> availableActions, Dictionary<string, Func<int, Task>?> actions, bool split = false)
    {
        _deviceId = deviceId;
        Id = inputId;
        Type = type;
        Actions = actions;
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
        RemoveLater.Output.SendMessage(_deviceId, Id, actionName);
        Console.WriteLine($" device: {_deviceId}, input: {Id}, action: {actionName}");

        if (Actions.ContainsKey(actionName) && Actions[actionName] is not null)
        {
            try
            {
                await Actions[actionName]!.Invoke(inputValue);
            }
            catch (Exception ex)
            {
                ActionThrownAnException?.Invoke(this, new(_deviceId, Id, actionName, ex));
            }
        }
    }

    public event EventHandler<PluginActionExceptionEventArgs>? ActionThrownAnException;
}
