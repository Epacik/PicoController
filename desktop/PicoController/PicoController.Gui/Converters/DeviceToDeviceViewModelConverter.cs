using Avalonia.Data.Converters;
using Mapster;
using PicoController.Core.Config;
using PicoController.Gui.Models;
using PicoController.Gui.ViewModels.Devices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Converters;

public class DeviceToDeviceViewModelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return null;
        if(value is Device device)
        {
            return new DeviceViewModel(device.Adapt<DeviceConfigModel>());
        }
        return new DeviceViewModel((DeviceConfigModel)value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return null;
        var devVm = (DeviceViewModel)value;
        return devVm.Device;
    }
}
