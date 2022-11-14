using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicoController.Core.Config;

public class ConfigRepository : IConfigRepository
{
    public event EventHandler<ConfigChangedEventArgs>? Changed;
    void OnChanged(Config config)
    {
        Changed?.Invoke(this, new ConfigChangedEventArgs(config));
    }

    private object lockObject = new object();
    private (Config config, DateTime readTime)? _configCache;

    public Config? Read()
    {
        lock (lockObject)
        {
            if (_configCache is (Config, DateTime) cc && cc.readTime >= DateTime.Now.AddSeconds(-5))
            {
                return cc.config;
            }
        }

        if (!Exists())
            return null;
        var configPath = ConfigPath();

        var json = File.ReadAllText(configPath) ?? "";
        var options = new JsonSerializerOptions()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            }
        };
        var conf = JsonSerializer.Deserialize<Config>(json, options) ?? new Config();
        lock (lockObject)
        {
            _configCache = (conf, DateTime.Now);
        }

        return conf;
    }

    public async ValueTask<Config?> ReadAsync(CancellationToken? token = null)
    {
        lock (lockObject)
        {
            if (_configCache is (Config, DateTime) cc && cc.readTime >= DateTime.Now.AddSeconds(-5))
            {
                return cc.config;
            }
        }

        if (!Exists())
                return null;
        var configPath = ConfigPath();

        var json = await File.ReadAllTextAsync(configPath) ?? "";
        var options = new JsonSerializerOptions()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            }
        };
        var conf = JsonSerializer.Deserialize<Config>(json, options) ?? new Config();

        lock (lockObject)
        {
            _configCache = (conf, DateTime.Now);
        }

        return conf;
    }

    public async Task SaveAsync(Config config, CancellationToken? token = null)
    {
        var configPath = ConfigPath();
        var options = new JsonSerializerOptions()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            }
        };

        var json = JsonSerializer.Serialize(config, options);
        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);

        await File.WriteAllTextAsync(configPath, json);

        lock (lockObject)
        {
            _configCache = null;
        }
        OnChanged(config);
    }

    public void Save(Config config)
    {
        var configPath = ConfigPath();
        var options = new JsonSerializerOptions()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            }
        };
        var json = JsonSerializer.Serialize(config, options);
        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
        File.WriteAllText(configPath, json);

        lock (lockObject)
        {
            _configCache = null;
        }
        OnChanged(config);
    }

    public static string ConfigDirectory()
    {
        var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userFolder, ".picoController");
    }
    public static string ConfigPath()
    {
        var configPath = Path.Combine(ConfigDirectory(), "config.json");
        return configPath;
    }

    public bool Exists() => File.Exists(ConfigPath());
}
