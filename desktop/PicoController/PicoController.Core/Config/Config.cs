using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicoController.Core.Config
{
    public class Config
    {
        [JsonConstructor]
        public Config() {}

        public Config(List<Device> devices)
        {
            Devices = devices;
        }

        [JsonPropertyName("devices")]
        public List<Device> Devices { get; set; } = new List<Device>();

        public static Config Read()
        {
            var configPath = ConfigPath();
            if (!File.Exists(configPath))
                return new Config();

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
            return JsonSerializer.Deserialize<Config>(json, options) ?? new Config();
        }

        public static void Save(Config config)
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
            File.WriteAllText(configPath, json);
        }

        private static string ConfigPath()
        {
            var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var configPath = Path.Combine(userFolder, @".picoController/config.jsonc");
            return configPath;
        }
    }
}
