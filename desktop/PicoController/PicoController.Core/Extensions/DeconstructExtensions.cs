namespace PicoController.Core;

public static class DeconstructExtensions
{
    public static void Deconstruct<T>(this T[] array, out T? item1, out T? item2)
    {
        item1 = array.Length > 0 ? array[0] : default;
        item2 = array.Length > 1 ? array[1] : default;
    }
}
