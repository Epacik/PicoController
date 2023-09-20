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

public sealed class CsScript : CsScriptBase
{
    public CsScript(ILogger logger, IDisplayInfo displayInfo) : base(logger, displayInfo) {}
    protected override string GetCode(string data)
    {
        return
            """
            using System;
            using Serilog;
            using PicoController.Plugin;
            using PicoController.Plugin.DisplayInfos;
            public void Invoke(ILogger logger, IDisplayInfo displayInfo) 
            {
                logger?.Information("Test");

                var date = DateTime.Now;

                displayInfo.Display(
                    new Text("Test", 30),
                    new Text(date.ToLongDateString()),
                    new Text(date.ToLongTimeString()),
                    new ProgressBar());
            }
            """;
    }

}

[HideHandler]
public abstract class CsScriptBase : IPluginAction
{
    private readonly ILogger _logger;
    private readonly IDisplayInfo _displayInfo;

    protected CsScriptBase(ILogger logger, IDisplayInfo displayInfo)
    {
        _logger = logger;
        _displayInfo = displayInfo;
    }

    protected abstract string GetCode(string data);
    public async Task ExecuteAsync(int inputValue, string? data)
    {
        _ = data ?? throw new ArgumentNullException(nameof(data));

        var code = GetCode(data);

        var del = CSScript.Evaluator
            .ReferenceAssembliesFromCode(code)
            .ReferenceAssemblyOf<ILogger>()
            .ReferenceAssemblyOf<IDisplayInfo>()
            .ReferenceDomainAssemblies()
            .CreateDelegate<IInvokable>(code);

        del.Invoke(_logger, _displayInfo);
    }
}

public interface IInvokable
{
    public void Invoke(ILogger logger, IDisplayInfo displayInfo);
}