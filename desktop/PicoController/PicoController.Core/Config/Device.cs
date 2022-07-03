using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PicoController.Core.Config
{
    public class Device
    {
        [JsonConstructor]
        public Device()
        {}

        public Device(Interface @interface, Input[] inputs)
        {
            Interface = @interface;
            Inputs = inputs;
        }

        [JsonPropertyName("interface")]
        public Interface Interface { get; set; } = new();

        [JsonPropertyName("inputs")]
        public Input[] Inputs { get; set; } = Array.Empty<Input>();
    }
}
