using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core;

public class HandlerInfo
{
    public string? Description { get; }
    public IDictionary<string, string>? ValidArguments { get; }

    public HandlerInfo(string? description, IDictionary<string, string>? validArguments)
    {
        Description = description;
        ValidArguments = validArguments;
    }
}
