using PicoController.Core.Devices.Communication;
using PicoController.Core.Devices.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Encoder = PicoController.Core.Devices.Inputs.Encoder;
using PicoController.Core;
using Tiger.Clock;

namespace PicoController.Core.Devices
{
    public class Device : IDisposable
    {

        public readonly DeviceInterface Interface;
        public readonly List<Inputs.Input> Inputs;
        private bool _isDisposed;

        public Device(DeviceInterface @interface, List<Inputs.Input> inputs)
        {
            Interface = @interface;
            Inputs = inputs;
            Interface.NewMessage += Interface_NewMessage;
        }

        private async void Interface_NewMessage(object? sender, InterfaceMessageEventArgs e)
        {
            var input = Inputs.Find(x => x.Id == e.Message.InputId);
            if (input is null)
                return;

            await input.Execute(e.Message);
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
