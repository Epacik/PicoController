//using PicoController.Plugin;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PicoController.Core
//{
//    internal class InvokeHandler : IInvokeHandler
//    {
//        private readonly Dictionary<string, Func<int, Task>?> _actions;

//        public InvokeHandler(Dictionary<string, Func<int, Task>?> actions)
//        {
//            _actions = actions;
//        }
//        public void Invoke(string handler, int inputValue, string? data)
//        {
//            throw new NotImplementedException();
//        }

//        public async Task InvokeAsync(string handler, int inputValue, string? data)
//        {
//            if(_actions.TryGetValue(handler, out Func<int, Task>? action))
//            {
//                action?.Invoke()
//            }
//        }
//    }
//}
