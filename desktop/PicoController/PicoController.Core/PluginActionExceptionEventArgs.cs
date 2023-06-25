using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core
{
    public class PluginActionExceptionEventArgs : EventArgs
    {

        public readonly int DeviceNumber;
        public readonly int InputId;
        public readonly string ActionName;
        public readonly Exception Exception;

        public PluginActionExceptionEventArgs(int deviceNumber, int inputId, string actionName, Exception ex)
        {
            DeviceNumber = deviceNumber;
            InputId = inputId;
            ActionName = actionName;
            Exception = ex;
        }
    }
}