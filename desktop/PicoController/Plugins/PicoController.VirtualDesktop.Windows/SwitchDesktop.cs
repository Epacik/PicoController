using PicoController.Core;
using WindowsDesktop;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416", Justification = "this lib is Windows only")]

namespace PicoController.VirtualDesktop.Windows;

public class SwitchDesktop : IPluginAction
{
    public async Task ExecuteAsync(int inputValue, string? argument)
    {
        argument = argument ?? throw new ArgumentNullException(nameof(argument));
        await Task.Yield();

        if (Environment.OSVersion.Version.Build < 14393)
            throw new InvalidOperationException("Use at least Windows 10 1607");

        if (int.TryParse(argument, out int index))
        {
            WindowsDesktop.VirtualDesktop.GetDesktops()[index].Switch();
        }
        else
        {
            var current = WindowsDesktop.VirtualDesktop.Current;
            var desktop = argument.ToLowerInvariant() switch
            {
                "left"  => current.GetLeft()  ?? WindowsDesktop.VirtualDesktop.GetDesktops().Last(),
                "right" => current.GetRight() ?? WindowsDesktop.VirtualDesktop.GetDesktops().First(),
                "switch" => inputValue switch
                {
                     > 0 => current.GetLeft() ?? WindowsDesktop.VirtualDesktop.GetDesktops().Last(),
                     <= 0 => current.GetRight() ?? WindowsDesktop.VirtualDesktop.GetDesktops().First(),
                },
                _       => throw new NotImplementedException(),
            };
            desktop.Switch();
        }
    }
}