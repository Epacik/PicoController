using PicoController.Core;
using PicoController.Plugin;
using PicoController.Plugin.DisplayInfos;
using Serilog;
using System.Text;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416", Justification = "this lib is Windows only")]

namespace PicoController.VirtualDesktop.Windows;

public class SwitchDesktop : IPluginAction
{
    private readonly ILogger _logger;
    private readonly IDisplayInfo _displayInfo;

    public SwitchDesktop(ILogger logger, IDisplayInfo displayInfo)
    {
        _logger = logger;
        _displayInfo = displayInfo;
    }
    public async Task ExecuteAsync(int inputValue, string? argument) 
        => await Task.Run(() =>
    {
        
        argument = argument ?? throw new ArgumentNullException(nameof(argument));

        if (Environment.OSVersion.Version.Build < 14393)
            throw new InvalidOperationException("Use at least Windows 10 1607");

        if (int.TryParse(argument, out int index))
        {
            VirtualDesktopAccessorInterop.GoToDesktopNumber(index);
        }
        else
        {
            var count = VirtualDesktopAccessorInterop.GetDesktopCount();
            var current = VirtualDesktopAccessorInterop.GetCurrentDesktopNumber();

            if (count < 0)
                throw new InvalidOperationException("There was a problem querying desktop count, got negative value");

            if (current < 0)
                throw new InvalidOperationException("There was a problem querying current desktop index, got negative value");

            var newIndex = argument.ToLowerInvariant() switch
            {
                "switch" when inputValue < 0 && current > 0 => current - 1,
                "left"   when current > 0 => current - 1,

                "switch" when inputValue< 0 && current == 0 => count - 1,
                "left"   when current == 0 => count - 1,

                "switch" when inputValue > 0 && current < (count - 1) => current + 1,
                "right"  when current < (count -1) => current + 1,

                "switch" when inputValue > 0 && current == (count - 1) => 0,
                "right"  when current == (count - 1) => 0,

                _ => throw new InvalidOperationException("invalid argument")
            };

            if (_logger.IsEnabled(Serilog.Events.LogEventLevel.Information))
                _logger.Information("Switching desktop to {NewIndex}", newIndex + 1);

            VirtualDesktopAccessorInterop.GoToDesktopNumber(newIndex);

            string name = "";
            try
            {
                StringBuilder data = new StringBuilder(32);

                var bufLen = VirtualDesktopAccessorInterop.GetDesktopName(newIndex, data, data.Capacity);
                name = data.ToString();
            }
            catch (Exception ex)
            {
            }

            _displayInfo.Display(
                new Text($"Desktop {newIndex + 1}", 25, 600),
                new Text(name));
        }
    });
}