using Serilog;

namespace PicoController.Plugin;

public interface IInvokable
{
    public Task Run(
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler);
}
