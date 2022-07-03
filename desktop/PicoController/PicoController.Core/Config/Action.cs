using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PicoController.Core.Config
{
    internal class Action
    {
        [JsonConstructor]
        public Action()
        {}
        public Action(string handler, string data)
        {
            Handler = handler;
            Data = data;
        }

        [JsonPropertyName("handler")]
        public string Handler { get; set; } = "";

        [JsonPropertyName("data")]
        public string Data { get; set; } = "";
    }
}
