using System.Diagnostics;
var processName = "PicoController.Gui";
Console.WriteLine($"{processName} seems to crash, restarting");

for (int i = 1; i <= 10; i++)
{
    Console.WriteLine($"Try {i}");
    if (Process.GetProcessesByName(processName).Length == 0)
    {
        Console.WriteLine("Starting...");
        Process.Start(processName + ".exe", "--hide");
        return;
    }
    Console.WriteLine("Still running, waiting 5s");
    Thread.Sleep(5000);
}