﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace PicoController.Core.Devices.Inputs;

public readonly struct InputMessage
{
    public readonly byte InputId;
    public readonly InputType InputType;
    public readonly UInt32 Value;

    public InputMessage(byte id, InputType type, uint value) : this()
    {
        InputId = id;
        InputType = type;
        Value = value;
    }

    public bool ValueHasBits(UInt32 bits) => (Value & bits) == bits;

    public static bool TryParse(string str, out InputMessage? message)
    {
        // Example message:
        // Inp08000300000002;
        // Inp%02X%04X%08lX;
        // InpXXYYYYZZZZZZZZ; -> XX: id, YYYY: Type, ZZZZZZZZ: value
        if (string.IsNullOrEmpty(str))
            return fail(out message);

        if (str.Length < 18)
            return fail(out message);

        if (!str.StartsWith("Inp") || !str.EndsWith(";"))
            return fail(out message);

        ReadOnlySpan<char> span = str;

        var idSpan = span.Slice(3, 2);
        var typeSpan = span.Slice(5, 4);
        var valueSpan = span.Slice(9, 8);

        if (!byte.TryParse(idSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var id))
            return fail(out message);

        if (!UInt16.TryParse(typeSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var type) ||
            type > (UInt16)InputType.MAX)
            return fail(out message);

        if (!UInt32.TryParse(valueSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
            return fail(out message);

        message = new InputMessage(id, (InputType)type, value);
        return true;

        static bool fail(out InputMessage? message)
        {
            message = null;
            return false;
        }
    }

    public static InputMessage Parse(string str)
    {
        if (TryParse(str, out var message))
            return (InputMessage)message!;

        throw new FormatException();
    }
}
