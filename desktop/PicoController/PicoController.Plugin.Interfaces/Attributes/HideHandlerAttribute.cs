using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Plugin.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class HideHandlerAttribute : Attribute
    {
    }
}
