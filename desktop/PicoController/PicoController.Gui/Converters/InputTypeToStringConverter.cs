using Avalonia.Data.Converters;
using PicoController.Core.Devices.Inputs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Converters;

public class InputTypeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value?.GetType() != typeof(InputType))
        {
            return null;
        }
        InputType inputType = (InputType)value;

        return inputType switch
        {
            InputType.Button =>  "Button",
            InputType.Encoder => "Encoder",
            InputType.EncoderWithButton => "Encoder with button",
            _ => throw new InvalidOperationException(),
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
