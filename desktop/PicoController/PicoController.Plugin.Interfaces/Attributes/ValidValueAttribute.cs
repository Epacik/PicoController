using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ValidValueAttribute : Attribute
    {
        public readonly string Value;
        public readonly string? Description;

        public ValidValueAttribute(string value, string? description = null)
        {
            Value = value;
            Description = description;
        }
    }
}
