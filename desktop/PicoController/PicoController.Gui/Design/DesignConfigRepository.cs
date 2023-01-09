using PicoController.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Design;

internal class DesignConfigRepository : IConfigRepository
{
    public event EventHandler<ConfigChangedEventArgs>? Changed;
    private static readonly Config _cfg = Config.ExampleConfig(5);

    public bool Exists() => true;

    public Config? Read() => _cfg;

    public ValueTask<Config?> ReadAsync(CancellationToken? token = null)
    {
        return new ValueTask<Config?>(Task.Run<Config?>(() => _cfg));
    }

    public void Save(Config config)
    {
        
    }

    public Task SaveAsync(Config config, CancellationToken? token = null) => Task.Run(() => { });
}
