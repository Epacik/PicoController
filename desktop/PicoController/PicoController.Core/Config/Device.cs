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
    public class Device : IClonable<Device>
    {
        [JsonConstructor]
        public Device()
        {
            Id = Guid.NewGuid().ToString();
        }

        public Device(string id, string name, DeviceInterface @interface, Input[] inputs)
        {
            Id = id;
            Name = name;
            Interface = @interface;
            Inputs = inputs;
        }

        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("interface")]
        public DeviceInterface Interface { get; set; } = new();

        [JsonPropertyName("inputs")]
        public Input[] Inputs { get; set; } = Array.Empty<Input>();

        public Device Clone() => new(Id, Name, Interface.Clone(), Inputs.Select(x => x.Clone()).ToArray());

        public override string ToString()
        {
            return $"Device: {Name}\nInterface: {Interface.Type}\nInputs: {Inputs.Length}";
        }
    }
}
