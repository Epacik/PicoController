
using PicoController.VirtualDesktop.Windows;
using System.Runtime.InteropServices;
using System.Text;

var argument = args[0];
var inputValue = args.Length >= 2 ? int.Parse(args[1]) : 0;


if (int.TryParse(argument, out int index))
{
    VirtualDesktopAccessorInterop.GoToDesktopNumber(index);
}
else
{
    var count = VirtualDesktopAccessorInterop.GetDesktopCount();
    var current = VirtualDesktopAccessorInterop.GetCurrentDesktopNumber();

    if (count < 0)
        return -2; // negative count

    if (current < 0)
        return -3; // negative current

    var newIndex = argument.ToLowerInvariant() switch
    {
        "switch" when inputValue < 0 && current > 0 => current - 1,
        "left" when current > 0 => current - 1,

        "switch" when inputValue < 0 && current == 0 => count - 1,
        "left" when current == 0 => count - 1,

        "switch" when inputValue > 0 && current < (count - 1) => current + 1,
        "right" when current < (count - 1) => current + 1,

        "switch" when inputValue > 0 && current == (count - 1) => 0,
        "right" when current == (count - 1) => 0,

        _ => -4, // invalid argument
    } ;

    if (newIndex < 0)
        return newIndex;

    VirtualDesktopAccessorInterop.GoToDesktopNumber(newIndex);

    var output = new Output(newIndex + 1, GetDesktopName(newIndex));

    Console.WriteLine("|" + System.Text.Json.JsonSerializer.Serialize(output));
}


return 0;

string GetDesktopName(int newIndex)
{
    var ptr = Marshal.AllocHGlobal(1024);
    try
    {
        var bufLen = VirtualDesktopAccessorInterop.GetDesktopName(newIndex, ptr, 1024);

        return PtrToString(ptr, 1024);
    }
    catch (Exception)
    {
        return "";
    }
    finally
    {
        Marshal.FreeHGlobal(ptr);
    }
}

string PtrToString(nint ptr, int bufLen)
{
    List<byte> buffer = new List<byte>(bufLen);
    var offset = 0;
    byte ch = 0;
    do
    {
        ch = Marshal.ReadByte(ptr, offset++);
        buffer.Add(ch);
    }
    while (ch != 0 && offset < bufLen);

    return Encoding.UTF8.GetString(buffer.ToArray());
}


class Output
{
    public Output(int index, string name)
    {
        Index = index;
        Name = name;
    }

    public int Index { get; }
    public string Name { get; }
}