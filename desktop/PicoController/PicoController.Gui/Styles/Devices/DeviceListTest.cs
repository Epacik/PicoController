using Avalonia.Collections;
using PicoController.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Styles.Devices;

public static class DeviceListTest
{
    public static Device TestDevice { get; set; } = Config.ExampleConfig(1).Devices[0];
    public static Input[] TestInputs => TestDevice.Inputs;

    public static AvaloniaList<Device> Devices = new AvaloniaList<Device>(new Device[] { TestDevice });
}
