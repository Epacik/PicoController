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

    public async Task ExecuteAsync(int inputValue, string? data)
    {
        await Task.Yield();
        ActionRequested?.Invoke(null, new(Enum.Parse<RequestedAction>(data ?? "None")));
    }
}
