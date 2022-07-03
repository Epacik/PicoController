using System.Collections.Immutable;
using System.Text;

namespace PicoController.Core.Devices.Inputs;

public abstract class InputBase
{
    public readonly byte Id;
    public readonly InputType Type;
    public readonly Dictionary<string, Func<Task>?> Actions = new();
    public readonly ImmutableArray<string> AvailableActions;

    public InputBase(byte id, InputType type, IEnumerable<string> availableActions, Dictionary<string, Func<Task>?> actions)
    {
        Id = id;
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

    protected void InvokeAction(string actionName)
    {
        if (Actions.ContainsKey(actionName))
        {
            Actions[actionName]?.Invoke()?.GetAwaiter().GetResult();
        }
    }
}
