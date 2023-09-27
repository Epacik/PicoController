using Neo.IronLua;
using PicoController.Core;
using PicoController.Plugin;
using PicoController.Plugin.Attributes;
using Serilog;

namespace PicoController.Core.BuiltInActions.Scripting;

public class NeoLua : NeoLuaBase
{
    public NeoLua(
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler) : base(false, logger, displayInfo, storage, invokeHandler) { }
}

public class NeoLuaFile : NeoLuaBase
{
    public NeoLuaFile(
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler) : base(true, logger, displayInfo, storage, invokeHandler) { }
}

[HideHandler]
public abstract class NeoLuaBase : IPluginAction, IDisposable
{
    private readonly bool _useFile;
    private readonly ILogger _logger;
    private readonly IDisplayInfo _displayInfo;
    private readonly IStorage _storage;
    private readonly IInvokeHandler _invokeHandler;
    private bool disposedValue;
    private readonly Lua _lua;

    protected NeoLuaBase(bool useFile, ILogger logger, IDisplayInfo displayInfo, IStorage storage, IInvokeHandler invokeHandler)
    {
        _lua = new Lua();
        _useFile = useFile;
        _logger = logger;
        _displayInfo = displayInfo;
        _storage = storage;
        _invokeHandler = invokeHandler;
    }

    public async Task ExecuteAsync(int inputValue, string? data)
    {
        await Task.Yield();

        if (data is null)
            throw new ArgumentNullException("data");

        var env = _lua.CreateEnvironment();
        env["__input_value__"] = inputValue;
        env["__logger__"] = _logger;
        env["__display_info__"] = _displayInfo;
        env["__storage__"] = _storage;
        env["__invoke_handler__"] = _invokeHandler;

        if (_useFile)
            env.DoChunk(data);
        else
            env.DoChunk(data, nameof(NeoLuaBase));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _lua.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
