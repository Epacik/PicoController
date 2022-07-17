using PicoController.Plugin;
using SharpHook;
using SharpHook.Native;
using System.ComponentModel;

namespace PicoController.Core.BuiltInActions.HidSimulation;

[Description("Simulate a key combination.")]
internal class KeyboardCombination : IPluginAction, IValidValues
{
    private EventSimulator _simulator;
    private readonly Dictionary<string, KeyCode> _buttons = new();

    public KeyboardCombination()
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

        foreach (var key in argument.Split("+"))
        {
            if (!_buttons.ContainsKey(key))
                throw new ArgumentException($"'{key}' is not a valid key");
        }

        var buttons = argument.Split("+").Select(x => _buttons[x]).ToArray();

        for (int i = 0; i < buttons.Length; i++)
        {
            _simulator.SimulateKeyPress(buttons[i]);
        }

        for (int i = buttons.Length - 1; i >= 0; i--)
        {
            _simulator.SimulateKeyRelease(buttons[i]);
        }
    }
}
