using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Core
{
    public interface ILocationProvider
    {
        string ConfigPath { get; }
        string JsonLogsDirectoryPath { get; }
        string LogsDirectoryPath { get; }
        string MainDirectoryPath { get; }
        string PluginsDirectory { get; }

    }

    internal class LocationProvider : ILocationProvider
    {
        public LocationProvider(string? path = null)
        {
            MainDirectoryPath = path ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".picoController");
            ConfigPath            = Path.Combine(MainDirectoryPath, "config.json");
            LogsDirectoryPath     = Path.Combine(MainDirectoryPath, "Logs", "Text");
            JsonLogsDirectoryPath = Path.Combine(MainDirectoryPath, "Logs", "JSON");
            PluginsDirectory      = Path.Combine(MainDirectoryPath, "Plugins");
        }
        public string MainDirectoryPath { get; }
        public string ConfigPath { get; }
        public string LogsDirectoryPath { get; }
        public string JsonLogsDirectoryPath { get; }
        public string PluginsDirectory { get; }
    }
}
