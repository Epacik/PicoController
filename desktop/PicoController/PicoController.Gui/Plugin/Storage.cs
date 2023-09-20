using PicoController.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Plugin
{
    internal class Storage : IStorage
    {
        private readonly object _locker = new();
        private readonly Dictionary<string, object?> _data = new();
        public object? Get(string key)
        {
            lock (_locker)
            {
                if (_data.TryGetValue(key, out object? value))
                    return value;
            }

            return null;
        }

        public T? GetAs<T>(string key)
        {
            var value = Get(key);

            if (value is T tval)
                return tval;

            return default;
        }

        public void Set(string key, object? value)
        {
            lock (_locker) { _data[key] = value; }
        }

        public void SetAs<T>(string key, T? value)
        {
            lock (_locker) { _data[key] = value; }
        }
    }
}
