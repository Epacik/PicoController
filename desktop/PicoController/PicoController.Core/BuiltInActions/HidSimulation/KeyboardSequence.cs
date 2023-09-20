using PicoController.Core;
using SharpHook;
using SharpHook.Native;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

namespace PicoController.Core.BuiltInActions.HidSimulation;

[Description("Simulate a key sequence.\nKeys separated by '+' are pressed in sequence, keys separated by '&' are pressed together")]
internal class KeyboardSequence : IPluginAction, IValidValues
{
    private readonly EventSimulator _simulator;
    private readonly Dictionary<string, KeyCode> _buttons = new();
    private readonly Queue<KeyCode[]> _sequencesToRun = new();
    private readonly object _locker = new object();

    public KeyboardSequence()
    {
        _simulator = new EventSimulator();
        foreach (var key in Enum.GetNames<KeyCode>())
        {
            var name = (ReadOnlySpan<char>)key;
            name = name[2..];
            _buttons.Add(name.ToString(), Enum.Parse<KeyCode>(key));
        }
        _buttons.Add("LeftWindows", KeyCode.VcLeftMeta);
        _buttons.Add("RightWindows", KeyCode.VcRightMeta);

        _buttons.Add("LeftArrow", KeyCode.VcLeft);
        _buttons.Add("RightArrow", KeyCode.VcRight);
        _buttons.Add("UpArrow", KeyCode.VcUp);
        _buttons.Add("DownArrow", KeyCode.VcDown);

        _timer = new PeriodicTimer(new TimeSpan(0, 0, 0, 0, 100));
        _sequenceRunner = new Thread(RunSequence);
        _sequenceRunner.Start();
    }

    public IDictionary<string, string> ValidValues => _buttons.Keys./*OrderBy(x => x).*/ToDictionary(x => x);

    private readonly PeriodicTimer _timer;
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
    private readonly Thread _sequenceRunner;
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables

    public async Task ExecuteAsync(int inputValue, string? data)
    {
        if (string.IsNullOrWhiteSpace(data))
            return;

        await Task.Yield();
        KeyCombination(data);
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

        lock (_locker)
        {
            foreach (var sequence in sequences)
                _sequencesToRun.Enqueue(sequence);
        }
    }

    [SuppressUnmanagedCodeSecurity]
#pragma warning disable S2190 // Loops and recursions should not be infinite
    private async void RunSequence(object? _)
#pragma warning restore S2190 // Loops and recursions should not be infinite
    {
        while (true)
        {
            await _timer.WaitForNextTickAsync();
            while(_sequencesToRun.Count > 0)
            {
                KeyCode[]? seq = null;
                lock (_locker)
                    seq = _sequencesToRun.Dequeue();

                if (seq is null)
                    continue;

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
}
