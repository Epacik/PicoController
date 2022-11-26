namespace PicoController.Core;

public interface IPluginAction
{
    //public void Execute(string? argument);
    public Task ExecuteAsync(int inputValue, string? argument);
}