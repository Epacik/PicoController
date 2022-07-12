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

        [JsonPropertyName("maxDelayBetweenClicks")]
        public int MaxDelayBetweenClicks { get; set; }

        [JsonPropertyName("devices")]
        public List<Device> Devices { get; set; } = new List<Device>();
        public static Config? Read()
        {
            var configPath = ConfigPath();
            if (!File.Exists(configPath))
                return null;

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
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            File.WriteAllText(configPath, json);
        }

        public static void SaveExampleConfig()
        {
            var configPath = ConfigPath();
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
            File.WriteAllText(configPath, exampleConfig);
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


        private const string exampleConfig = @"{
    ""maxDelayBetweenClicks"": 50,
    ""devices"": [
      {
        ""interface"": {
          ""type"": ""COM"",
          ""data"": {
            ""port"": ""COM6"",
            ""rate"": 115200,
            ""dataBits"": 8,
            ""stopBits"": 1,
            ""parity"": 0
          }
        },
        ""inputs"": [
          {
            ""id"": 0,
            ""type"": ""button"",
            ""actions"": {
              ""press"": {
                ""handler"": null,
                ""data"": null
              }
            }
          },
          {
            ""id"": 1,
            ""type"": ""button"",
            ""actions"": {
              ""press"": {
                ""handler"": null,
                ""data"": null
              }
            }
          },
          {
            ""id"": 2,
            ""type"": ""button"",
            ""actions"": {
              ""press"": {
                ""handler"": null,
                ""data"": null
              }
            }
          },
          {
            ""id"": 3,
            ""type"": ""button"",
            ""actions"": {
              ""press"": {
                ""handler"": null,
                ""data"": null
              }
            }
          },
          {
            ""id"": 4,
            ""type"": ""encoderWithButton"",
            ""actions"": {
              ""rotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""rotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""press"": {
                ""handler"": null,
                ""data"": null
              }
            }
          },
          {
            ""id"": 5,
            ""type"": ""encoderWithButton"",
            ""actions"": {
              ""rotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""rotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""press"": {
                ""handler"": null,
                ""data"": null
              }
            }
          },
          {
            ""id"": 6,
            ""type"": ""encoderWithButton"",
            ""actions"": {
              ""rotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""rotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""press"": {
                ""handler"": null,
                ""data"": null
              }
            }
          },
          {
            ""id"": 7,
            ""type"": ""encoderWithButton"",
            ""actions"": {
              ""rotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""rotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""press"": {
                ""handler"": null,
                ""data"": null
              }
            }
          },
          {
            ""id"": 8,
            ""type"": ""encoderWithButton"",
            ""actions"": {
              ""rotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""rotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationCounterClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""pressedRotationClockwise"": {
                ""handler"": null,
                ""data"": null
              },
              ""press"": {
                ""handler"": null,
                ""data"": null
              }
            }
          }
        ]
      }
    ]
  }";
    }
}
