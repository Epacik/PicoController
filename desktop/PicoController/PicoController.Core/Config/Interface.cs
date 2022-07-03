using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PicoController.Core.Config
{
    public class Interface
    {
        [JsonConstructor]
        public Interface()
        {
        }

        public Interface(string type, Dictionary<string, JsonElement> data)
        {
            Type = type;
            Data = data;
        }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("data")]
        public Dictionary<string, JsonElement> Data { get; set; } = new();
    }
}
