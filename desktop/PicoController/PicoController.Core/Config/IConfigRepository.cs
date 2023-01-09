using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core.Config
{
    public interface IConfigRepository
    {
        void Save(Config config);
        Task SaveAsync(Config config, CancellationToken? token = null);
        Config? Read();
        ValueTask<Config?> ReadAsync(CancellationToken? token = null);
        bool Exists();

        event EventHandler<ConfigChangedEventArgs>? Changed;
    }
}
