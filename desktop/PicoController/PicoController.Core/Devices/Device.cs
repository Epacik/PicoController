using PicoController.Core.Devices.Communication;
using PicoController.Core.Devices.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encoder = PicoController.Core.Devices.Inputs.Encoder;

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
                var id = i;
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
                        actions[a.Key] = async () => { await Task.Yield(); Console.WriteLine($"device: {id}, input: {inp.Id}, action: {a.Key}"); };
                    }

                    inputs.Add(inp.Type switch
                    {
                        InputType.Button            => new Button(inp.Id, inp.Type, actions),
                        InputType.Encoder           => new Encoder(inp.Id, inp.Type, actions),
                        InputType.EncoderWithButton => new EncoderWithButton(inp.Id, inp.Type, actions),
                        _                           => throw new InvalidDataException(),
                    });
                }
                result.Add(new Device(ifc, inputs));
                i++;
            }
            return result;
        }

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
