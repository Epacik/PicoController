using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Plugin
{
    public interface IStorage
    {
        public object? Get(string key);
        public T? GetAs<T>(string key);
        public void Set(string key, object? value);
        public void SetAs<T>(string key, T? value);
    }
}
