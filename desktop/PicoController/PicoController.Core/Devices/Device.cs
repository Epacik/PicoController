using PicoController.Core.Devices.Communication;
using PicoController.Core.Devices.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Encoder = PicoController.Core.Devices.Inputs.Encoder;
using PicoController.Plugin.Interfaces;

namespace PicoController.Core.Devices
{
    public class Device : IDisposable
    {

        public readonly Communication.InterfaceBase Interface;
        public readonly List<Inputs.InputBase> Inputs;
        private bool _isDisposed;

        public Device(InterfaceBase @interface, List<Inputs.InputBase> inputs)
        {
            Interface = @interface;
            Inputs = inputs;
            Interface.NewMessage += Interface_NewMessage;
        }

        private void Interface_NewMessage(object? sender, InterfaceMessageEventArgs e)
        {
            var input = Inputs.Find(x => x.Id == e.Message.InputId);
            if (input is null)
                return;

            input.Execute(e.Message);
        }

        public void Connect() => Interface.Connect();
        public void Disconnect() => Interface.Disconnect();

        public static List<Device> FromConfig(IEnumerable<Config.Device> devices)
        {
            var result = new List<Device>();
            int i = 0;
            foreach (var dev in devices)
            {
                var deviceId = i;
                InterfaceBase ifc = dev.Interface.Type switch
                {
                    "COM" => new Serial(dev.Interface.Data),
                    "WiFi" => new WiFi(dev.Interface.Data),
                    _ => throw new InvalidDataException(),
                };

                var inputs = new List<InputBase>();

                foreach(var inp in dev.Inputs)
                {
                    var actions = new Dictionary<string, Func<Task>?>();

                    foreach(var a in inp.Actions)
                    {
                        actions[a.Key] = LookupActions(a.Value);
                    }

                    inputs.Add(inp.Type switch
                    {
                        InputType.Button            => new Button(deviceId, inp.Id, inp.Type, actions),
                        InputType.Encoder           => new Encoder(deviceId, inp.Id, inp.Type, actions),
                        InputType.EncoderWithButton => new EncoderWithButton(deviceId, inp.Id, inp.Type, actions),
                        _                           => throw new InvalidDataException(),
                    });
                }
                result.Add(new Device(ifc, inputs));
                i++;
            }
            return result;
        }

        private static Func<Task>? LookupActions(Config.Action value)
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
                var typename = handler.TrimStart('/');
                var assembly = Assembly.GetExecutingAssembly();
                var allBuildtInActions = assembly.DefinedTypes.Where(t => typeof(IPluginAction).IsAssignableFrom(t));
                var actionType = allBuildtInActions.FirstOrDefault(t => t.Name == typename);
                if(actionType is null)
                    return null;

                var action = Activator.CreateInstance(actionType.AsType()) as IPluginAction; 
                if (action is null)
                    return null;

                LoadedActions[handler] = action;
                return IPluginActionToFuncOfTask(value, action);
            }
            else
            {
                return null;
            }
        }

        private static Func<Task> IPluginActionToFuncOfTask(Config.Action value, IPluginAction action)
        {
            return async () => await action.ExecuteAsync(value.Data);
        }

        private static Dictionary<string, IPluginAction> LoadedActions = new Dictionary<string, IPluginAction>();

        #region IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Interface.NewMessage -= Interface_NewMessage;
                }

                ((IDisposable)Interface).Dispose();
                _isDisposed = true;
            }
        }

        ~Device()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}
