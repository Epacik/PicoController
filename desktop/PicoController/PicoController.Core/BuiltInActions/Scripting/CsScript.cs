using CSScriptLib;
using PicoController.Core;
using PicoController.Plugin;
using PicoController.Plugin.Attributes;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

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

    static CsScriptBase()
    {
        CSScript.RoslynEvaluator.IsCachingEnabled = true;
    }

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

        code = WrapCode(code);

        var del = CSScript.RoslynEvaluator
            .ReferenceAssembliesFromCode(code)
            .ReferenceAssemblyOf<ILogger>()
            .ReferenceAssemblyOf<IDisplayInfo>()
            .ReferenceAssemblyOf<IStorage>()
            .ReferenceAssemblyOf<IInvokeHandler>()
            .ReferenceAssemblyOf<IInvokable>()
            .ReferenceDomainAssemblies()
            .LoadCode<IInvokable>(code);

        await del.Run(_logger, _displayInfo, _storage, _invokeHandler);
    }

    private string WrapCode(string code)
    {
        var sb = new StringBuilder();

        var usings = Regex.Matches(code, @"(using\s[\w\.]*;)|(using\s+[\w]*\s*=\s*[\w\.]*;)");

        foreach (Match us in usings)
        {
            sb.AppendLine(us.Value);
        }

        InsertLineIfNotPresent(
            sb,
            code,
            "using System;",
            @"(using\s+System;)");
        InsertLineIfNotPresent(
            sb,
            code,
            "using System.Threading.Tasks;",
            @"(using\s+System\.Threading\.Tasks;)");
        InsertLineIfNotPresent(
            sb,
            code,
            "using Serilog;",
            @"(using\s+Serilog;)");
        InsertLineIfNotPresent(
            sb,
            code,
            "using PicoController.Plugin;",
            @"(using\s+PicoController\.Plugin;)");
        InsertLineIfNotPresent(
            sb,
            code,
            "using PicoController.Plugin.DisplayInfos;",
            @"(using\s+PicoController\.Plugin\.DisplayInfos;)");

        sb.AppendLine();

        var classMatch = Regex.Match(
            code,
            @"((public|private)\s+class\s+\w+\s*:\s*IInvokable)");

        var classes = Regex
            .Matches(
                code,
                @"((public|private)\s+(class|interface|record|(record class)|(record struct))\s+\w+)")
            .Where(x => !x.Value.Contains("IInvokable"));

        var methodMatch = Regex.Match(code, "(Task\\s+Run)");

        if (!classMatch.Success)
        {
            sb.AppendLine("public class Script : IInvokable\n{");
        }

        if (!methodMatch.Success)
        {
            sb.AppendLine("\n\npublic async Task Run(ILogger logger, IDisplayInfo displayInfo, IStorage storage, IInvokeHandler invokeHandler)\n{\n");
        }

        var lastUsing = usings.LastOrDefault();
        var firstClass = classes.FirstOrDefault();

        var start = lastUsing is not null ? lastUsing.Index + lastUsing.Length : 0;
        var end = firstClass is not null ? firstClass.Index : code.Length - 1;

        sb.AppendLine(code.Substring(start, end - start));

        if(!methodMatch.Success)
        {
            sb.AppendLine( "}");
        }

        if (!classMatch.Success)
        {
            sb.AppendLine("}");
        }

        if (firstClass is not null)
        {
            sb.AppendLine("\n" + code.Substring(end));
        }

        return sb.ToString();
    }

    private void InsertLineIfNotPresent(StringBuilder sb, string code, string line, [StringSyntax(StringSyntaxAttribute.Regex)] string regex)
    {
        if (!Regex.Match(code, regex).Success)
        {
            sb.AppendLine(line);
        }
    }
}