using McMaster.NETCore.Plugins;
using PicoController.Plugin.Interfaces;
using System;
using System.Collections.Generic;
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
        public static void LoadPlugins()
        {
            var directory = Path.Combine(Config.Config.ConfigDirectory(), "Plugins");
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
                        sharedTypes: new Type[] { typeof(IPluginAction) },
                        config => { config.PreferSharedTypes = true; config.IsUnloadable = true; });
                    _loaders.Add(dirName, loader);
                }
            }

            //foreach(var loader in _loaders)
            //{
            //    using (loader.Value.EnterContextualReflection())
            //        _assemblies.Add(loader.Value.LoadDefaultAssembly());
            //}
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

        internal static Func<Task>? LookupActions(Config.Action value)
        {
            if (string.IsNullOrWhiteSpace(value.Handler))
                return null;

            var handler = value.Handler;

            if (LoadedActions.ContainsKey(handler) && LoadedActions[handler] is not null)
            {
                var action = LoadedActions[handler];
                return IPluginActionToFuncOfTask(value, action);
            }

            if (handler.StartsWith("/")) //buildt in actions
            {
                return LookupBuildtInAction(value, handler);
            }
            else
            {
                return LookupPluginAction(value, handler);
            }
        }

        private static Func<Task>? LookupPluginAction(Config.Action value, string handler)
        {
            var splittedHandler = handler.Split('/');
            var (assemblyName, typename) = (splittedHandler[0], splittedHandler[1]);

            if (!_loaders.ContainsKey(assemblyName))
                return null;

            var loader = _loaders[assemblyName];
            using (loader.EnterContextualReflection())
            {
                var assembly = loader.LoadDefaultAssembly();
                var allActionsInAssembly = assembly?.DefinedTypes.Where(IsPlugin);
                return LookupActionsFromAssembly(allActionsInAssembly, value, typename, handler);
            }
        }

        private static Func<Task>? LookupBuildtInAction(Config.Action value, string handler)
        {
            var typename = handler.TrimStart('/');
            var assembly = Assembly.GetExecutingAssembly();
            var allActionsInAssembly = assembly?.DefinedTypes.Where(IsPlugin);
            return LookupActionsFromAssembly(allActionsInAssembly, value, typename, handler);
        }

        private static Func<Task>? LookupActionsFromAssembly(IEnumerable<TypeInfo>? allActionsInAssembly, Config.Action value, string typename, string handler)
        {
            if (allActionsInAssembly is null)
                return null;
            
            var actionType = allActionsInAssembly.FirstOrDefault(t => t.Name == typename);
            if (actionType is null)
                return null;

            var action = Activator.CreateInstance(actionType.AsType()) as IPluginAction;
            if (action is null)
                return null;

            LoadedActions[handler] = action;
            return IPluginActionToFuncOfTask(value, action);
        }

        private static bool IsPlugin(TypeInfo t) => typeof(IPluginAction).IsAssignableFrom(t);

        private static Func<Task> IPluginActionToFuncOfTask(Config.Action value, IPluginAction action)
        {
            return async () => await action.ExecuteAsync(value.Data);
        }

        private static Dictionary<string, IPluginAction> LoadedActions = new Dictionary<string, IPluginAction>();
    }
}
