using System.Text;

var count = VirtualDesktopAccessorInterop.GetDesktopCount();
var current = VirtualDesktopAccessorInterop.GetCurrentDesktopNumber();

if (count < 0)
    throw new InvalidOperationException("There was a problem querying desktop count, got negative value");

if (current < 0)
    throw new InvalidOperationException("There was a problem querying current desktop index, got negative value");

var newIndex = current + 1;
newIndex = newIndex < count - 1 ? newIndex : 0;

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
    Console.WriteLine(ex.ToString());
}