using Microsoft.Scripting.Hosting;
using PicoController.Core;
using PicoController.Plugin;
using PicoController.Plugin.Attributes;
using Serilog;
using IP = IronPython;

namespace PicoController.Core.BuiltInActions.Scripting;

public sealed class IronPython : IronPythonBase
{
    public IronPython(
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler) : base(false, logger, displayInfo, storage, invokeHandler) { }
}

public sealed class IronPythonFile : IronPythonBase
{
    [PluginConstructor]
    public IronPythonFile(
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler) : base(true, logger, displayInfo, storage, invokeHandler) { }
}

[HideHandler]
public abstract class IronPythonBase : IPluginAction
{
    private readonly bool _useFile;
    private readonly ILogger _logger;
    private readonly IDisplayInfo _displayInfo;
    private readonly IStorage _storage;
    private readonly IInvokeHandler _invokeHandler;
    private readonly ScriptEngine _engine;

    protected IronPythonBase(
        bool useFile,
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler)
    {
        _engine = IP.Hosting.Python.CreateEngine();
        _useFile = useFile;
        _logger = logger;
        _displayInfo = displayInfo;
        _storage = storage;
        _invokeHandler = invokeHandler;
    }

    public async Task ExecuteAsync(int inputValue, string? data)
    {
        await Task.Run(() =>
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var scope = _engine.CreateScope();
        
            scope.SetVariable("__input_value__", inputValue);
            scope.SetVariable("__logger__", _logger);
            scope.SetVariable("__display_info__", _displayInfo);
            scope.SetVariable("__storage__", _storage);
            scope.SetVariable("__invoke_handler__", _invokeHandler);

            if (_useFile)
                _engine.ExecuteFile(data, scope);
            else
                _engine.Execute(data, scope);
        });
    }
}
