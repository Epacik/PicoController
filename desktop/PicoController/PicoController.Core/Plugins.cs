using McMaster.NETCore.Plugins;
using PicoController.Core;
using PicoController.Plugin;
using PicoController.Plugin.Attributes;
using Splat;
using SuccincT.Functional;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core
{
    public static class Plugins
    {
        private static Dictionary<string, PluginLoader> _loaders = new();
        private static List<Assembly> _assemblies = new();
        public static bool AreLoaded { get; private set; }
        public static void LoadPlugins()
        {
            var directory = Path.Combine(Config.ConfigRepository.ConfigDirectory(), "Plugins");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var pluginDirs = Directory.GetDirectories(directory);
            foreach (var dir in pluginDirs)
            {
                var dirName = Path.GetFileName(dir);
                var dllPath = Path.Combine(dir, dirName + ".dll");
                if (File.Exists(dllPath))
                {
                    var loader = PluginLoader.CreateFromAssemblyFile(
                        dllPath,
                        sharedTypes: new Type[] { typeof(IPluginAction), typeof(IDisplayInfo) },
                        config => { config.PreferSharedTypes = true; config.IsUnloadable = true; });
                    _loaders.Add(dirName, loader);
                }
            }
            AreLoaded = true;
        }

        public static void UnloadPlugins()
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

        private static void ClearLookups()
        {
            foreach (var action in LoadedActions)
            {
                if (typeof(IDisposable).IsAssignableFrom(action.Value.GetType()))
                {
                    var disposable = action.Value as IDisposable;
                    disposable?.Dispose();
                }
            }

            LoadedActions.Clear();
        }

        internal static Func<int, Task>? LookupActions(Config.InputAction value)
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

        private static Func<int, Task>? LookupPluginAction(Config.InputAction value, string handler)
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

        private static Func<int, Task>? LookupBuildtInAction(Config.InputAction value, string handler)
        {
            var typename = handler.TrimStart('/');
            var assembly = Assembly.GetExecutingAssembly();
            var allActionsInAssembly = assembly?.DefinedTypes.Where(IsPluginAction);
            return LookupActionsFromAssembly(allActionsInAssembly, value, typename, handler);
        }

        private static Func<int, Task>? LookupActionsFromAssembly(IEnumerable<TypeInfo>? allActionsInAssembly, Config.InputAction value, string typename, string handler)
        {
            if (allActionsInAssembly is null)
                return null;
            
            var actionType = allActionsInAssembly.FirstOrDefault(t => t.Name == typename);
            if (actionType is null)
                return null;

            var action = Activator.CreateInstance(actionType.AsType()) as IPluginAction;
            if (action is null)
                return null;

            InjectDependencies(action, actionType);

            LoadedActions[handler] = action;
            return IPluginActionToFuncOfTask(value, action);
        }

        private static void InjectDependencies(IPluginAction action, Type actionType)
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
                    var value = Locator.Current.GetService(type);
                    if (set is not null && value is not null)
                        set.Invoke(action, new[] { value });
                }
            }
        }

        private static bool IsPluginAction(TypeInfo t) => typeof(IPluginAction).IsAssignableFrom(t);
        private static bool IsVisiblePluginAction(TypeInfo t) => 
            IsPluginAction(t) && t.GetCustomAttribute<HideHandlerAttribute>(false) is null;

        private static Func<int, Task> IPluginActionToFuncOfTask(Config.InputAction value, IPluginAction action)
        {
            return async inputValue => await action.ExecuteAsync(value.InputValueOverride ?? inputValue, value.Data);
        }

        private static readonly Dictionary<string, IPluginAction> LoadedActions = new();

        public static IEnumerable<string> AllAvailableActions()
        {
            var result = new List<string>();

            result.AddRange(
                Assembly.GetExecutingAssembly().DefinedTypes
                    .Where(IsVisiblePluginAction).Select(x => "/" + x.Name));

            foreach(var loader in _loaders)
            {
                result.AddRange(
                    loader.Value
                        .LoadDefaultAssembly().DefinedTypes
                        .Where(IsVisiblePluginAction)
                        .Select(x => $"{loader.Key}/{x.Name}"));
            }

            return result;
        }
        public static HandlerInfo? GetHandlerInfo(string handler)
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
                if(instance is not null)
                {
                    validValues = instance.ValidValues;
                }
            }

            return new(description, validValues);
        }

        private static TypeInfo? GetBuiltInHandlerInfo(string handler) =>
            Assembly.GetExecutingAssembly()
                .DefinedTypes
                .FirstOrDefault(
                    x => IsPluginAction(x) 
                      && string.Equals(
                            x.Name,
                            handler.Trim('/'), 
                            StringComparison.CurrentCultureIgnoreCase));

        private static TypeInfo? GetPluginHandlerInfo(string handler)
        {
            var (plugin, action) = handler.Split('/');

            throw new NotImplementedException();
        }
    }
}
