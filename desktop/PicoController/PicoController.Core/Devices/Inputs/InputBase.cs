using System.Collections.Immutable;
using System.Text;
using Tiger.Clock;

namespace PicoController.Core.Devices.Inputs;

public abstract class InputBase
{
    private readonly int deviceId;
    public readonly byte Id;
    public readonly InputType Type;
    public readonly Dictionary<string, Func<Task>?> Actions = new();
    public readonly ImmutableArray<string> AvailableActions;

    public InputBase(int deviceId, byte inputId, InputType type, IEnumerable<string> availableActions, Dictionary<string, Func<Task>?> actions)
    {
        this.deviceId = deviceId;
        Id = inputId;
        Type = type;
        Actions = actions;
        AvailableActions = availableActions.ToImmutableArray();
    }

    public void Execute(InputMessage message)
    {
        if (Id != message.InputId)
            throw new ArgumentException("Message Id does not match Input Id");
        if(Type != message.InputType)
            throw new ArgumentException("Message Type does not match Input Type");

        ExecuteInternal(message);
    }

    protected abstract void ExecuteInternal(InputMessage message);

    protected async void InvokeAction(string actionName)
    {
        RemoveLater.Output.SendMessage(deviceId, Id, actionName);
        Console.WriteLine($" device: {deviceId}, input: {Id}, action: {actionName}");

        if (Actions.ContainsKey(actionName) && Actions[actionName] is not null)
        {
            try
            {
                await Actions[actionName]!.Invoke();
            }
            catch (Exception ex)
            {
                ActionThrownAnException?.Invoke(this, new(deviceId, Id, actionName, ex));
            }
        }
    }

    public event EventHandler<PluginActionExceptionEventArgs>? ActionThrownAnException;
}
