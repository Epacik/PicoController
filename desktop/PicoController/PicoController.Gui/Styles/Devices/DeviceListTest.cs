using Avalonia.Collections;
using Mapster;
using PicoController.Core.Config;
using PicoController.Gui.Design;
using PicoController.Gui.Models;
using PicoController.Gui.ViewModels.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Styles.Devices;

public static class DeviceListTest
{
    public static Device TestDevice => Config.ExampleConfig(1).Devices[0];
    public static Input[] TestInputs => TestDevice.Inputs;

    public static AvaloniaList<DeviceViewModel> Devices => new (
        new DeviceViewModel[] {
            new( TestDevice.Adapt<DeviceConfigModel>()!, new DesignRepositoryHelper(), new DesignPluginManager() ),
        });
}
