using Microsoft.Scripting.Hosting;
using PicoController.Core;
using PicoController.Plugin.Attributes;
using IP = IronPython;

namespace PicoController.Core.BuiltInActions.Scripting;

public class IronPython : IronPythonBase
{
    public IronPython() : base(false) { }
}

public class IronPythonFile : IronPythonBase
{
    public IronPythonFile() : base(true) { }
}

[HideHandler]
public abstract class IronPythonBase : IPluginAction
{
    private readonly bool _useFile;
    private ScriptEngine _engine;

    protected IronPythonBase(bool useFile)
    {
        _engine = IP.Hosting.Python.CreateEngine();
        _useFile = useFile;
    }

    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        await Task.Yield();

        if (argument is null)
            throw new ArgumentNullException("data");

        var scope = _engine.CreateScope();

        scope.SetVariable("__input_value__", inputValue);
        //scope.SetVariable("__user_argument__", argument);

        if (_useFile)
            _engine.ExecuteFile(argument, scope);
        else
            _engine.Execute(argument, scope);
    }
}
