using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AvTextAlignment = Avalonia.Media.TextAlignment;
using PcTextAlignment = PicoController.Plugin.DisplayInfos.TextAlignment;

namespace PicoController.Gui.Converters;

public class PicoControllerTextAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not PcTextAlignment ta)
            return null;

        return ta switch
        {
            PcTextAlignment.Left => AvTextAlignment.Left,
            PcTextAlignment.Center => AvTextAlignment.Center,
            PcTextAlignment.Right => AvTextAlignment.Right,
            _ => throw new ArgumentException(nameof(value)),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not AvTextAlignment ta)
            return null;

        return ta switch
        {
            AvTextAlignment.Left => PcTextAlignment.Left,
            AvTextAlignment.Center => PcTextAlignment.Center,
            AvTextAlignment.Right => PcTextAlignment.Right,
            _ => throw new ArgumentException(nameof(value)),
        };
    }
}
