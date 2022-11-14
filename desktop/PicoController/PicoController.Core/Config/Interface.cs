using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using PicoController.Core.Misc;

namespace PicoController.Core.Config
{
    public class DeviceInterface : ICloneable<DeviceInterface>
    {
        [JsonConstructor]
        public DeviceInterface()
        {
        }

        public DeviceInterface(InterfaceType type, Dictionary<string, JsonElement> data)
        {
            Type = type;
            Data = data;
        }

        [JsonPropertyName("type")]
        public InterfaceType Type { get; set; } = InterfaceType.None;

        [JsonPropertyName("data")]
        public Dictionary<string, JsonElement> Data { get; set; } = new();

        public DeviceInterface Clone() =>
            new(Type,
                Data.Select(x => (x.Key, Value: x.Value.Clone()))
                    .ToDictionary(x => x.Key, x => x.Value));
    }
}
