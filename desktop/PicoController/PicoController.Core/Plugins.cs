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
    Func<int, Task>? LookupActions(InputAction value);
    void UnloadPlugins();
}

public class PluginManager : IPluginManager
{
    private readonly Dictionary<string, PluginLoader> _loaders = new();
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

    private static Type[] SharedTypes =
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
                _loaders.Add(dirName, loader);
            }
        }
        AreLoaded = true;
    }

    public void UnloadPlugins()
    {
        _assemblies.Clear();
        foreach (var loader in _loaders)
        {
            loader.Value.Dispose();
        }
        _loaders.Clear();
        ClearLookups();
        AreLoaded = true;
    }

    private void ClearLookups()
    {
        foreach (var action in LoadedActions)
        {
            if (action.Value is IDisposable disposable)
            {
                disposable?.Dispose();
            }
        }

        LoadedActions.Clear();
    }

    public Func<int, Task>? LookupActions(Config.InputAction value)
    {
        if (string.IsNullOrWhiteSpace(value.Handler))
            return null;

        var handler = value.Handler;

        if (LoadedActions.ContainsKey(handler) && LoadedActions[handler] is not null)
        {
            var action = LoadedActions[handler];
            return IPluginActionToFuncOfTask(value, action);
        }

        if (handler.StartsWith("/") || !value.Handler.Contains("/")) //built in actions
        {
            return LookupBuildtInAction(value, handler);
        }
        else
        {
            return LookupPluginAction(value, handler);
        }
    }

    private Func<int, Task>? LookupPluginAction(Config.InputAction value, string handler)
    {
        var splittedHandler = handler.Split('/');
        var (assemblyName, typename) = (splittedHandler[0], splittedHandler[1]);

        if (!_loaders.ContainsKey(assemblyName))
            return null;

        var loader = _loaders[assemblyName];
        using (loader.EnterContextualReflection())
        {
            var assembly = loader.LoadDefaultAssembly();
            var allActionsInAssembly = assembly?.DefinedTypes.Where(IsPluginAction);
            return LookupActionsFromAssembly(allActionsInAssembly, value, typename, handler);
        }
    }

    private Func<int, Task>? LookupBuildtInAction(Config.InputAction value, string handler)
    {
        var typename = handler.TrimStart('/');
        var assembly = Assembly.GetExecutingAssembly();
        var allActionsInAssembly = assembly?.DefinedTypes.Where(IsPluginAction);
        return LookupActionsFromAssembly(allActionsInAssembly, value, typename, handler);
    }

    private Func<int, Task>? LookupActionsFromAssembly(IEnumerable<TypeInfo>? allActionsInAssembly, Config.InputAction value, string typename, string handler)
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

            InjectDependencies(action, actionType);
        }
        else
        {
            List<object?> arguments = new();
            var ctor = constructors.First();
            foreach ( var parameter in ctor.GetParameters() )
            {
                arguments.Add(_resolver.GetService(parameter.ParameterType));
            }
            action = ctor.Invoke(arguments.ToArray()) as IPluginAction;
            //action = Activator.CreateInstance(actionType.AsType(), arguments) as IPluginAction;
        }


        if (action is null)
            return null;
        LoadedActions[handler] = action;
        return IPluginActionToFuncOfTask(value, action);
    }

    private void InjectDependencies(IPluginAction action, Type actionType)
    {
        var flags = BindingFlags.Public | BindingFlags.Instance;
        var props = actionType.GetProperties(flags);
        if (props is null)
            return;

        Type[] types = { typeof(IDisplayInfo), };

        foreach (var type in types)
        {
            foreach (PropertyInfo prop in props.Where(x => x.PropertyType == type))
            {
                var set = prop.GetSetMethod(true);
                var value = _resolver.GetService(type);
                if (set is not null && value is not null)
                    set.Invoke(action, new[] { value });
            }
        }
    }

    private bool IsPluginAction(TypeInfo t) => typeof(IPluginAction).IsAssignableFrom(t);
    private bool IsVisiblePluginAction(TypeInfo t) =>
        IsPluginAction(t) && t.GetCustomAttribute<HideHandlerAttribute>(false) is null;

    private Func<int, Task> IPluginActionToFuncOfTask(Config.InputAction value, IPluginAction action)
    {
        return async inputValue =>
        {
            if (_logger.ExistsAndIsEnabled(LogEventLevel.Information))
            {
                if(value.InputValueOverride is not null)
                {
                    _logger?.Information(
                        """
                        Executing action.Handler: {Handler}
                        Data: {Data}
                        Input value Override: {InputValueOverride}

                        IPluginAction Type: {Type}
                        """,
                        value.Handler,
                        value.Data,
                        value.InputValueOverride,
                        action.GetType().FullName);
                }
                else
                {
                    _logger?.Information(
                        """
                        Executing action.Handler: {Handler}
                        Data: {Data}

                        IPluginAction Type: {Type}
                        """,
                        value.Handler,
                        value.Data,
                        action.GetType().FullName);
                }
            }

            await action.ExecuteAsync(value.InputValueOverride ?? inputValue, value.Data);
        };
    }

    public IEnumerable<string> AllAvailableActions()
    {
        var result = new List<string>();

        result.AddRange(
            Assembly.GetExecutingAssembly().DefinedTypes
                .Where(IsVisiblePluginAction).Select(x => "/" + x.Name));

        foreach (var loader in _loaders)
        {
            result.AddRange(
                loader.Value
                    .LoadDefaultAssembly().DefinedTypes
                    .Where(IsVisiblePluginAction)
                    .Select(x => $"{loader.Key}/{x.Name}"));
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
