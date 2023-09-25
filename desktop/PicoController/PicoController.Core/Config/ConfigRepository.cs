using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PicoController.Core.Extensions;

namespace PicoController.Core.Config;

public class ConfigRepository : IConfigRepository
{
    private readonly ILocationProvider _location;
    private readonly IFileSystem _filesystem;
    private readonly ILogger? _logger;

    public ConfigRepository(ILocationProvider location, IFileSystem filesystem, Serilog.ILogger? logger)
    {
        _location = location;
        _filesystem = filesystem;
        _logger = logger;
    }
    public event EventHandler<ConfigChangedEventArgs>? Changed;
    void OnChanged(Config config)
    {
        Changed?.Invoke(this, new ConfigChangedEventArgs(config));
    }

    private readonly object lockObject = new object();
    private (Config config, DateTime readTime)? _configCache;
    
    public Config? Read()
    {
        if (_logger.ExistsAndIsEnabled(LogEventLevel.Debug))
            _logger?.Debug("Checking configuration cache");

        lock (lockObject)
        {
            if (_configCache is (Config, DateTime) cc && cc.readTime >= DateTime.Now.AddSeconds(-5))
            {
                if (_logger.ExistsAndIsEnabled(LogEventLevel.Debug))
                    _logger?.Debug("Cache exists and isn't stale, returning");

                return cc.config;
            }
        }

        if (!Exists())
            return null;
        var configPath = _location.ConfigPath;

        var json = _filesystem.FileReadAllText(configPath) ?? "";
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
        var configPath = _location.ConfigPath;

        var json = await _filesystem.FileReadAllTextAsync(configPath) ?? "";
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
        var configPath = _location.ConfigPath;
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
        _filesystem.CreateDirectory(Path.GetDirectoryName(configPath)!);

        await _filesystem.WriteAllTextAsync(configPath, json);

        lock (lockObject)
        {
            _configCache = null;
        }
        OnChanged(config);
    }

    public void Save(Config config)
    {
        var configPath = _location.ConfigPath;
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
        _filesystem.CreateDirectory(Path.GetDirectoryName(configPath)!);
        _filesystem.FileWriteAllText(configPath, json);

        lock (lockObject)
        {
            _configCache = null;
        }
        OnChanged(config);
    }

    public bool Exists() => _filesystem.FileExists(_location.ConfigPath);
}
