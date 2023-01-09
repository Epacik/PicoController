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
        private static readonly string _mainDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".picoController");
        public string MainDirectoryPath { get; } = _mainDir;
        public string ConfigPath { get; } = Path.Combine(_mainDir, "config.json");
        public string LogsDirectoryPath { get; } = Path.Combine(_mainDir, "Logs", "Text");
        public string JsonLogsDirectoryPath { get; } = Path.Combine(_mainDir, "Logs", "JSON");
        public string PluginsDirectory { get; } = Path.Combine(_mainDir, "Plugins");
    }
}
