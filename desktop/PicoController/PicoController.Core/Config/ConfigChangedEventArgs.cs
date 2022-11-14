using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.Config
{
    public class ConfigChangedEventArgs : EventArgs
    {
        public Config NewConfig { get; }

        public ConfigChangedEventArgs(Config newConfig)
        {
            NewConfig = newConfig;
        }
    }
}
