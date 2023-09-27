using CSScriptLib;
using Microsoft.CodeAnalysis.Scripting;
using PicoController.Core;
using PicoController.Plugin;
using PicoController.Plugin.Attributes;
using PicoController.Plugin.DisplayInfos;
using Serilog;
using System.Reflection;
using System.Xml.Linq;

namespace PicoController.CsScript;

[FileCodeEditor(".cs")]
public sealed class CsScriptFile : CsScriptBase
{
    public CsScriptFile(
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler) 
        : base(logger, displayInfo, storage, invokeHandler) { }
    protected override string GetCode(string data)
    {
        return File.Exists(data) ? File.ReadAllText(data) : string.Empty;
    }
}

[CodeEditor(".cs")]
public sealed class CsScript : CsScriptBase
{
    public CsScript(
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler) 
        : base(logger, displayInfo, storage, invokeHandler) {}
    protected override string GetCode(string data) => data;
}

[HideHandler]
public abstract class CsScriptBase : IPluginAction
{
    private readonly ILogger _logger;
    private readonly IDisplayInfo _displayInfo;
    private readonly IStorage _storage;
    private readonly IInvokeHandler _invokeHandler;

    protected CsScriptBase(
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler)
    {
        _logger = logger;
        _displayInfo = displayInfo;
        _storage = storage;
        _invokeHandler = invokeHandler;
    }

    protected abstract string GetCode(string data);
    public async Task ExecuteAsync(int inputValue, string? data)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data));

        var code = GetCode(data);

        var del = CSScript.RoslynEvaluator
            .ReferenceAssembliesFromCode(code)
            .ReferenceAssemblyOf<ILogger>()
            .ReferenceAssemblyOf<IDisplayInfo>()
            .ReferenceDomainAssemblies()
            .CreateDelegate<IInvokable>(code);

        var x = del.Invoke(_logger, _displayInfo);
    }
}

public interface IInvokable
{
    public Task Run(
        ILogger logger,
        IDisplayInfo displayInfo,
        IStorage storage,
        IInvokeHandler invokeHandler);
}