namespace PicoController.Plugin;

public interface IPluginAction
{
    public void Execute(string? argument);
    public Task ExecuteAsync(string? argument);
}