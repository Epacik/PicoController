using PicoController.Core;
using SharpHook;
using SharpHook.Native;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.Json;

namespace PicoController.Core.BuiltInActions.HidSimulation;

[Description("Simulate a key sequence.\nKeys separated by '+' are pressed in sequence, keys separated by '&' are pressed together")]
public class KeyboardSequence : IPluginAction, IValidValues
{
    private readonly EventSimulator _simulator;
    private static readonly Dictionary<string, KeyCode> _buttons = new();
    private readonly Queue<KeyPressData> _keysToPress = new();
    private readonly object _locker = new object();

    static KeyboardSequence()
    {
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
    }

    public KeyboardSequence()
    {
        _simulator = new EventSimulator();
        
        _timer = new PeriodicTimer(new TimeSpan(0, 0, 0, 0, 100));
        _sequenceRunner = new Thread(RunSequence);
        _sequenceRunner.Start();
    }


    public static Dictionary<string, KeyCode> GetValidValues() => _buttons;
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
        try
        {
            var keys = JsonSerializer.Deserialize<List<KeyPressData>>(argument);
            if (keys is not null)
            {
                lock (_locker)
                {
                    foreach (var key in keys)
                        _keysToPress.Enqueue(key);
                }
                
            }
        }
        catch
        {
            return;
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
            while(_keysToPress.Count > 0)
            {
                KeyPressData key;
                lock (_locker)
                {
                    key = _keysToPress.Dequeue();
                }

                if (key.Pressed == true)
                {
                    _simulator.SimulateKeyPress(key.Code);
                }
                else if (key.Pressed == false) 
                {
                    _simulator.SimulateKeyRelease(key.Code);
                }
                else
                {
                    Thread.Sleep((ushort)key.Code);
                }
            }
        }
    }
}

public class KeyPressData
{
    public KeyPressData(KeyCode code, bool? pressed)
    {
        Code = code;
        Pressed = pressed;
    }
    public KeyCode Code { get; }
    public bool? Pressed { get; }
    public static implicit operator (KeyCode code, bool? pressed)(KeyPressData value)
    {
        return (value.Code, value.Pressed);
    }

    public static implicit operator KeyPressData((KeyCode code, bool? pressed) value)
    {
        return new KeyPressData(value.code, value.pressed);
    }

    public void Deconstruct(out KeyCode keyCode, out bool? pressed)
    {
        keyCode = Code;
        pressed = Pressed;
    }
}
