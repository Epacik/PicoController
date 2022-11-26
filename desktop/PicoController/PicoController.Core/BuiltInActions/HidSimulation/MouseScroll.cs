using PicoController.Core.BuiltInActions.HidSimulation.Native;
using SharpHook;
using System.Runtime.InteropServices;
using System.Security;

namespace PicoController.Core.BuiltInActions.HidSimulation;

internal class MouseScroll : IPluginAction
{
    private INativeInputSimulator _simulator;

    public MouseScroll()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            _simulator = new WindowsInputSimulator();
        else
            _simulator = new EmptyInputSimulator();
    }

    [SuppressUnmanagedCodeSecurity]
    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        if (string.IsNullOrWhiteSpace(argument))
            return;

        await Task.Yield();

        var (axis, amountStr) = argument.Split(';');

        if (short.TryParse(amountStr, out short amount))
            _simulator.MouseScrollWheelScrolled(
                axis switch { "x" => Axis.Horizontal, "y" => Axis.Vertical, _ => Axis.None },
                (short)(amount * inputValue));
    }
}
