namespace PicoController.Core.BuiltInActions.Other;

public enum RequestedAction
{
    None,
    Quit,
    Reload
}

public class RequestedActionEventArgs : EventArgs
{
    public readonly RequestedAction RequestedAction;

    public RequestedActionEventArgs(RequestedAction requestedAction)
    {
        RequestedAction = requestedAction;
    }
}

public class PicoControllerActions : IPluginAction
{
    public static event EventHandler<RequestedActionEventArgs>? ActionRequested;

    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        await Task.Yield();
        ActionRequested?.Invoke(this, new(Enum.Parse<RequestedAction>(argument ?? "None")));
    }
}
