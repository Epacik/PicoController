using McMaster.NETCore.Plugins;
using PicoController.Core;
using PicoController.Core.Config;
using PicoController.Core.Extensions;
using PicoController.Plugin;
using PicoController.Plugin.Attributes;
using Serilog;
using Serilog.Events;
using Splat;
using SuccincT.Functional;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core;

public interface IPluginManager
{
    bool AreLoaded { get; }
    IEnumerable<string> AllAvailableActions();
    HandlerInfo? GetHandlerInfo(string handler);
    void LoadPlugins(string? directory = null);
    void UnloadPlugins();
    Func<int, string?, Task>? GetAction(string handler);
}

internal class PluginInfo : IPluginInfo
{
    public PluginInfo(string location)
    {
        Location = location;
    }

    public string Location { get; }
}

public class PluginManager : IPluginManager
{
    private readonly Dictionary<string, (PluginLoader, string)> _loaders = new();
    private readonly List<Assembly> _assemblies = new();

    private readonly Dictionary<string, IPluginAction> LoadedActions = new();
    private readonly ILocationProvider _locationProvider;
    private readonly IFileSystem _fileSystem;
    private readonly IReadonlyDependencyResolver _resolver;
    private Serilog.ILogger? _logger;

    public PluginManager(
        ILocationProvider locationProvider,
        IFileSystem fileSystem,
        IReadonlyDependencyResolver resolver,
        Serilog.ILogger? logger)
    {
        _locationProvider = locationProvider;
        _fileSystem = fileSystem;
        _resolver = resolver;
        _logger = logger;
    }

    public bool AreLoaded { get; private set; }

    private static readonly Type[] SharedTypes =
    {
        typeof(IPluginAction),
        typeof(IDisplayInfo),
        typeof(Serilog.ILogger),
    };
    public void LoadPlugins(string? directory = null)
    {
        directory ??= _locationProvider.PluginsDirectory;

        if (_logger?.IsEnabled(LogEventLevel.Information) == true)
            _logger?.Information("Loading plugins from {Directory}", directory);

        if (!_fileSystem.DirectoryExists(directory))
        {
            if (_logger?.IsEnabled(LogEventLevel.Information) == true)
                _logger?.Information("{Directory}", directory);

            _fileSystem.CreateDirectory(directory);
        }

        var pluginDirs = _fileSystem.GetDirectories(directory) ?? Array.Empty<string>();
        foreach (var dir in pluginDirs)
        {
            var dirName = Path.GetFileName(dir);
            var dllPath = Path.Combine(dir, dirName + ".dll");
            if (_fileSystem.FileExists(dllPath))
            {
                var loader = PluginLoader.CreateFromAssemblyFile(
                    dllPath,
                    sharedTypes: SharedTypes,
                    config => { 
                        config.PreferSharedTypes = true;
                        config.IsUnloadable = true;
                        //config.EnableHotReload = true;
                        config.IsLazyLoaded = true;
                        config.LoadInMemory = true;
                    });
                _loaders.Add(dirName, (loader, dir));
            }
        }
        AreLoaded = true;
    }

    public void UnloadPlugins()
    {
        _assemblies.Clear();
        foreach (var (key, (loader, location)) in _loaders)
        {
            loader.Dispose();
        }
        _loaders.Clear();
        ClearLookups();
        AreLoaded = true;
    }

    private void ClearLookups()
    {
        foreach (var (key, action) in LoadedActions)
        {
            if (action is IDisposable disposable)
            {
                disposable?.Dispose();
            }
        }

        LoadedActions.Clear();
    }

    public Func<int, string?, Task>? GetAction(string handler)
    {
        if (string.IsNullOrWhiteSpace(handler))
            return null;

        if (LoadedActions.ContainsKey(handler) && LoadedActions[handler] is not null)
        {
            var action = LoadedActions[handler];
            return IPluginActionToFuncOfTask(action);
        }

        if (handler.StartsWith('/') || !handler.Contains('/')) //built in actions
        {
            return LookupBuiltInAction(handler);
        }
        else
        {
            return LookupPluginAction(handler);
        }
    }

    private Func<int, string?, Task>? LookupPluginAction(string handler)
    {
        var splittedHandler = handler.Split('/');
        var (assemblyName, typename) = (splittedHandler[0], splittedHandler[1]);

        if (!_loaders.ContainsKey(assemblyName))
            return null;

        var (loader, location) = _loaders[assemblyName];
        using (loader.EnterContextualReflection())
        {
            var assembly = loader.LoadDefaultAssembly();
            var allActionsInAssembly = assembly?.DefinedTypes.Where(IsPluginAction);
            return LookupActionsFromAssembly(allActionsInAssembly, typename, handler, location);
        }
    }

    private Func<int, string?, Task>? LookupBuiltInAction(string handler)
    {
        var typename = handler.TrimStart('/');
        var assembly = Assembly.GetExecutingAssembly();
        var allActionsInAssembly = assembly?.DefinedTypes.Where(IsPluginAction);
        return LookupActionsFromAssembly(allActionsInAssembly, typename, handler, assembly?.Location);
    }

    private Func<int, string?, Task>? LookupActionsFromAssembly(IEnumerable<TypeInfo>? allActionsInAssembly, string typename, string handler, string? location)
    {
        if (allActionsInAssembly is null)
            return null;

        var actionType = allActionsInAssembly.FirstOrDefault(t => t.Name == typename);
        if (actionType is null)
            return null;

        IPluginAction? action = null;

        var constructors = actionType.DeclaredConstructors;
        var numberOfConstructors = constructors.Count(x => x.IsPublic);
        if (numberOfConstructors > 1)
            throw new InvalidOperationException($"Action '{actionType.FullName}' has more than one public constructor");
        else if (numberOfConstructors == 0)
            throw new InvalidOperationException($"Action '{actionType.FullName}' has no public constructors");

        if (actionType.DeclaredConstructors.Any(x => x.GetParameters().Length == 0))
        {

            action = Activator.CreateInstance(actionType.AsType()) as IPluginAction;
            if (action is null)
                return null;
        }
        else
        {
            List<object?> arguments = new();

            var preferredCtor = constructors
                .FirstOrDefault(x =>
                    x.CustomAttributes.Any(p => p.AttributeType == typeof(PluginConstructorAttribute)));

            var ctor = preferredCtor ?? constructors.First();

            foreach (var type in ctor.GetParameters().Select(x => x.ParameterType))
            {
                if (type == typeof(IPluginInfo))
                {
                    arguments.Add(new PluginInfo(location!));
                }
                else if (type == typeof(IInvokeHandler))
                {
                    arguments.Add(new InvokeHandler(this, _logger));
                }
                else
                {
                    arguments.Add(_resolver.GetService(type));
                }
            }
            action = ctor.Invoke(arguments.ToArray()) as IPluginAction;
        }


        if (action is null)
            return null;
        LoadedActions[handler] = action;
        return IPluginActionToFuncOfTask(action);
    }

    private bool IsPluginAction(TypeInfo t) => typeof(IPluginAction).IsAssignableFrom(t);
    private bool IsVisiblePluginAction(TypeInfo t) =>
        IsPluginAction(t) && t.GetCustomAttribute<HideHandlerAttribute>(false) is null;

    private Func<int, string?, Task> IPluginActionToFuncOfTask(IPluginAction action)
    {
        return action.ExecuteAsync;
    }

    public IEnumerable<string> AllAvailableActions()
    {
        var result = new List<string>();

        result.AddRange(
            Assembly.GetExecutingAssembly().DefinedTypes
                .Where(IsVisiblePluginAction).Select(x => "/" + x.Name));

        foreach (var (key, (loader, location)) in _loaders)
        {
            result.AddRange(
                loader
                    .LoadDefaultAssembly().DefinedTypes
                    .Where(IsVisiblePluginAction)
                    .Select(x => $"{key}/{x.Name}"));
        }

        return result;
    }
    public HandlerInfo? GetHandlerInfo(string handler)
    {
        TypeInfo? ha = handler.StartsWith('/')
            ? GetBuiltInHandlerInfo(handler)
            : GetPluginHandlerInfo(handler);
        if (ha is null)
            return null;

        var description = ha.GetCustomAttribute<DescriptionAttribute>()?.Description;
        IDictionary<string, string>? validValues = null;

        if (typeof(IValidValues).IsAssignableFrom(ha))
        {
            var instance = (IValidValues?)Activator.CreateInstance(ha);
            if (instance is not null)
            {
                validValues = instance.ValidValues;
            }
        }

        return new(description, validValues);
    }

    private TypeInfo? GetBuiltInHandlerInfo(string handler) =>
        Assembly.GetExecutingAssembly()
            .DefinedTypes
            .FirstOrDefault(
                x => IsPluginAction(x)
                  && string.Equals(
                        x.Name,
                        handler.Trim('/'),
                        StringComparison.CurrentCultureIgnoreCase));

    private TypeInfo? GetPluginHandlerInfo(string handler)
    {
        var (plugin, action) = handler.Split('/');

        throw new NotImplementedException();
    }
}

internal record class PluginActionAndInfo(IPluginAction Action, IPluginInfo Info)
{

}