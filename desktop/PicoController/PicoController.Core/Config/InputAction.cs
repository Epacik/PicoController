using PicoController.Core.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicoController.Core.Config
{
    public class InputAction : ICloneable<InputAction>
    {
        [JsonConstructor]
        public InputAction()
        {}
        public InputAction(string handler, string data)
        {
            Handler = handler;
            Data = data;
        }

        [JsonPropertyName("handler")]
        public string? Handler { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("inputValueOverride")]
        public int? InputValueOverride { get; set; }

        public InputAction Clone() => (InputAction)MemberwiseClone();
    }
}
