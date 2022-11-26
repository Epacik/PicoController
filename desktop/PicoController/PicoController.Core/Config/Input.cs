
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using PicoController.Core.Devices.Inputs;
using PicoController.Core.Misc;

namespace PicoController.Core.Config;

public class Input : ICloneable<Input>
{
    public Input(byte id, InputType type, Dictionary<string, InputAction> actions)
        : this(id, type, actions, false) { }
    [JsonConstructor]
    public Input(byte id, InputType type, Dictionary<string, InputAction> actions, bool split)
    {
        Id = id;
        Type = type;
        Actions = actions;
        Split = split;
    }

    [JsonPropertyName("id")]
    public byte Id { get; set; }

    [JsonPropertyName("type")]
    public InputType Type { get; set; }

    [JsonPropertyName("split")]
    public bool Split { get; set; }

    [JsonPropertyName("actions")]
    public Dictionary<string, InputAction> Actions { get; set; }

    public Input Clone() =>
        new (Id,
             Type,
             Actions.Select(x => (x.Key, Value: x.Value.Clone()))
                    .ToDictionary(x => x.Key, x => x.Value),
             Split);

    public override string ToString()
    {
        return $"Id: {Id}, Type: {Type}, Split: {Split}\n\n{string.Join("\n", Actions.Select(x => $"{x.Key}: {x.Value.Handler}({x.Value.Data})"))}";
    }
}
