using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Plugin
{
    public interface IInvokeHandler
    {
        public void Invoke(string handler, int inputValue, string? data);
        public Task InvokeAsync(string handler, int inputValue, string? data);
    }
}
