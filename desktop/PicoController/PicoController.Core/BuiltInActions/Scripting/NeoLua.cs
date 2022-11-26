using Neo.IronLua;
using PicoController.Core;
using PicoController.Plugin.Attributes;

namespace PicoController.Core.BuiltInActions.Scripting;

public class NeoLua : NeoLuaBase
{
    public NeoLua() : base(false) { }
}

public class NeoLuaFile : NeoLuaBase
{
    public NeoLuaFile() : base(true) { }
}

[HideHandler]
public abstract class NeoLuaBase : IPluginAction, IDisposable
{
    private readonly bool _useFile;
    private bool disposedValue;
    private Lua _lua;

    public NeoLuaBase(bool useFile)
    {
        _lua = new Lua();
        _useFile = useFile;
    }

    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        await Task.Yield();

        if (argument is null)
            throw new ArgumentNullException("data");

        var env = _lua.CreateEnvironment();
        env["__input_value__"] = inputValue;
        //env["__user_argument__"] = argument;

        if (_useFile)
            env.DoChunk(argument);
        else
            env.DoChunk(argument, nameof(NeoLuaBase));
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
