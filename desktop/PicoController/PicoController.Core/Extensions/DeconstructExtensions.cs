namespace PicoController.Core;

internal static class DeconstructExtensions
{
    public static void Deconstruct(this string[] array, out string? item1, out string? item2)
    {
        item1 = array.Length > 0 ? array[0] : null;
        item2 = array.Length > 1 ? array[1] : null;
    }
}
