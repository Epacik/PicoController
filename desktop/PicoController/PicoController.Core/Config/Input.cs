
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using PicoController.Core.Devices.Inputs;

namespace PicoController.Core.Config
{
    public class Input
    {
        [JsonConstructor]
        public Input(byte id, InputType type, Dictionary<string, Action> actions)
        {
            Id = id;
            Type = type;
            Actions = actions;
        }

        [JsonPropertyName("id")]
        public byte Id { get; set; }

        [JsonPropertyName("type")]
        public InputType Type { get; set; }

        [JsonPropertyName("actions")]
        public Dictionary<string, Action> Actions { get; set; }
    }
}
