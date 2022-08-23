using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core
{
    public class HandlerInformations
    {
        public string Description { get; }
        public IEnumerable<string> ValidArguments { get; }

        public HandlerInformations(string description, IEnumerable<string> validArguments)
        {
            Description = description;
            ValidArguments = validArguments;
        }
    }
}
