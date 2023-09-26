﻿using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Converters;

public class BooleanToTextWrappingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? TextWrapping.WrapWithOverflow : TextWrapping.NoWrap;
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TextWrapping tw)
        {
            return tw != TextWrapping.NoWrap;
        }
        return null;
    }
}
