using PicoController.Plugin;
using SharpHook;
using SharpHook.Native;
using System.ComponentModel;

namespace PicoController.Core.BuiltInActions.HidSimulation;

[Description("Simulate a key sequence.\nKeys separated by '+' are pressed in sequence, keys separated by '&' are pressed together")]
internal class KeyboardSequence : IPluginAction, IValidValues
{
    private EventSimulator _simulator;
    private Dictionary<string, KeyCode> _buttons = new Dictionary<string, KeyCode>();

    public KeyboardSequence()
    {
        _simulator = new EventSimulator();
        foreach (var key in Enum.GetNames<KeyCode>())
        {
            var name = (ReadOnlySpan<char>)key;
            name = name.Slice(2, name.Length - 2);
            _buttons.Add(name.ToString(), Enum.Parse<KeyCode>(key));
        }
        _buttons.Add("LeftWindows", KeyCode.VcLeftMeta);
        _buttons.Add("RightWindows", KeyCode.VcRightMeta);

        _buttons.Add("LeftArrow", KeyCode.VcLeft);
        _buttons.Add("RightArrow", KeyCode.VcRight);
        _buttons.Add("UpArrow", KeyCode.VcUp);
        _buttons.Add("DownArrow", KeyCode.VcDown);
    }

    public IDictionary<string, string> ValidValues => _buttons.Keys.ToDictionary(x => x);

    public void Execute(string? argument)
    {
        if (argument is null)
            return;

        KeyCombination(argument);
    }

    public async Task ExecuteAsync(string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        await Task.Yield();
        KeyCombination(argument);
    }

    private void KeyCombination(string argument)
    {
        var sequencesStr = argument.Split("+", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var sequences = new List<KeyCode[]>();
        foreach (var seq in sequencesStr)
        {
            if (seq.Contains('&'))
            {
                var buttons = seq.Split('&');
                var sequence = new KeyCode[buttons.Length];
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (!_buttons.ContainsKey(buttons[i]))
                        throw new ArgumentException($"'{buttons[i]}' is not a valid key");

                    sequence[i] = _buttons[buttons[i]];
                }

                sequences.Add(sequence);
            }
            else
            {
                if (!_buttons.ContainsKey(seq))
                    throw new ArgumentException($"'{seq}' is not a valid key");

                sequences.Add(new KeyCode[] { _buttons[seq] });
            }
        }

        foreach (var seq in sequences)
        {
            if (seq.Length == 0)
            {
                _simulator.SimulateKeyPress(seq[0]);
                _simulator.SimulateKeyRelease(seq[0]);
            }
            else
            {
                for (int i = 0; i < seq.Length; i++)
                    _simulator.SimulateKeyPress(seq[i]);

                for (int i = seq.Length - 1; i >= 0; i--)
                    _simulator.SimulateKeyRelease(seq[i]);
            }
        }
    }
}
