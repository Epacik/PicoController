using Avalonia.Controls;
using Avalonia.Data.Converters;
using System.Globalization;

namespace PicoController.Gui.Converters;

public class BooleanToGridLengthConverterParameter
{
    public GridLength ForTrue { get; init; }
    public GridLength ForFalse { get; init; }
}

public class BooleanToGridLengthConverter : IValueConverter
{
    private static readonly BooleanToGridLengthConverterParameter DefaultParam = new BooleanToGridLengthConverterParameter
    {
        ForTrue = new GridLength(230),
        ForFalse = new GridLength(90),
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool || targetType != typeof(GridLength))
        {
            return null;
        }


        var param = parameter switch
        {
            BooleanToGridLengthConverterParameter x => x,
            _ => DefaultParam,
        };

        var b = (bool)value;

        return b ? param.ForTrue : param.ForFalse;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
