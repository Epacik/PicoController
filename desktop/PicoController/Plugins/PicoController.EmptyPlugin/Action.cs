using PicoController.Plugin.Interfaces;

namespace PicoController.EmptyPlugin
{
    public class Action : IPluginAction
    {
        public void Execute(string? argument)
        {
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(string? argument)
        {
            throw new NotImplementedException();
        }
    }
}