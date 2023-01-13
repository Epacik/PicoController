using Microsoft.Scripting.Hosting;
using PicoController.Core;
using PicoController.Plugin;
using PicoController.Plugin.Attributes;
using Serilog;
using IP = IronPython;

namespace PicoController.Core.BuiltInActions.Scripting;

public class IronPython : IronPythonBase
{
    public IronPython(ILogger logger, IDisplayInfo displayInfo) : base(false, logger, displayInfo) { }
}

public class IronPythonFile : IronPythonBase
{
    public IronPythonFile(ILogger logger, IDisplayInfo displayInfo) : base(true, logger, displayInfo) { }
}

[HideHandler]
public abstract class IronPythonBase : IPluginAction
{
    private readonly bool _useFile;
    private readonly ILogger _logger;
    private readonly IDisplayInfo _displayInfo;
    private readonly ScriptEngine _engine;

    protected IronPythonBase(bool useFile, ILogger logger, IDisplayInfo displayInfo)
    {
        _engine = IP.Hosting.Python.CreateEngine();
        _useFile = useFile;
        _logger = logger;
        _displayInfo = displayInfo;
    }

    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        await Task.Run(() =>
        {
            if (argument is null)
                throw new ArgumentNullException("data");

            var scope = _engine.CreateScope();


            scope.SetVariable("__input_value__", inputValue);
            scope.SetVariable("__logger__", _logger);
            scope.SetVariable("__display_info__", _displayInfo);

            if (_useFile)
                _engine.ExecuteFile(argument, scope);
            else
                _engine.Execute(argument, scope);
        });
    }
}
