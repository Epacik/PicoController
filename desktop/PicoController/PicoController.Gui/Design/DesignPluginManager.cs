﻿using PicoController.Core;
using PicoController.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Design;

internal class DesignPluginManager : IPluginManager
{
    private static readonly IDictionary<string, string>? _exampleActions = new Dictionary<string, string>
    {
    };

    public bool AreLoaded => true;

    public IEnumerable<string> AllAvailableActions() => Array.Empty<string>();

    public HandlerInfo? GetHandlerInfo(string handler)
        => new HandlerInfo("Example handler", _exampleActions);

    public void LoadPlugins(string? directory = null)
    {
        
    }

    public Func<int, Task>? LookupActions(InputAction value)
    {
        return null;
    }

    public void UnloadPlugins()
    {
        
    }
}
