
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using PicoController.Core.Devices.Inputs;
using PicoController.Core.Misc;
using System.Configuration;
using System.Diagnostics;

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

    private static readonly string[] buttonActions = 
        { ActionNames.Press, ActionNames.DoublePress, ActionNames.TriplePress };

    private static readonly string[] encoderActions = 
        { ActionNames.Rotate };
    private static readonly string[] encoderActionsSplit = 
        { ActionNames.RotateSplitC, ActionNames.RotateSplitCC };

    private static readonly string[] encoderButtonActions = 
        { ActionNames.Rotate, ActionNames.RotatePressed, ActionNames.Press, ActionNames.DoublePress, ActionNames.TriplePress };
    private static readonly string[] encoderButtonActionsSplit = 
        { ActionNames.RotateSplitC, ActionNames.RotateSplitCC, ActionNames.RotatePressedSplitC, ActionNames.RotatePressedSplitCC, ActionNames.Press, ActionNames.DoublePress, ActionNames.TriplePress };

    public static IEnumerable<string> GetPossibleActions(InputType type, bool split) =>
        (type, split) switch
        {
            (InputType.Button, _)                => buttonActions,
            (InputType.Encoder, false)           => encoderActions,
            (InputType.Encoder, true)            => encoderActionsSplit,
            (InputType.EncoderWithButton, false) => encoderButtonActions,
            (InputType.EncoderWithButton, true)  => encoderButtonActionsSplit,
            _ => throw new UnreachableException("This combination of type and split shouldn't be possible"),
        };

    public static IEnumerable<string> GetAllPossibleActions(InputType type) =>
        type switch
        {
            InputType.Button            => buttonActions,
            InputType.Encoder           => encoderActions.Union(encoderActionsSplit),
            InputType.EncoderWithButton => encoderButtonActions.Union(encoderButtonActionsSplit),
            _                           => throw new UnreachableException($"That is an invalid type (int: {(int)type})"),
        };
}
