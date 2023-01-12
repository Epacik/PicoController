
using CoreAudio;
using LanguageExt.Common;
using System.IO.Ports;

Result<int>[] results =
{
    Add("a", "b"),
    Add("1", "2"),
};

foreach (var result in results)
{
    Console.WriteLine(
        result.Match(
            i => i.ToString(), // return normal response
            e => e.Message));  // return error 500
}

Console.WriteLine("Hello, World!");
Console.ReadKey();


Result<int> Add(string x, string y)
{
    if (int.TryParse(x, out int xInt) && int.TryParse(y, out int yInt))
    {
        return xInt + yInt;
    }
    else
    {
        return new Result<int>(new InvalidDataException("Arguments are not parsable"));
    }
}
