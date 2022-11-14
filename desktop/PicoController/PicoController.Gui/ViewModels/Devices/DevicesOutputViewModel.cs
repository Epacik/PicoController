using Avalonia.Collections;
using PicoController.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.Devices
{
    internal class DevicesOutputViewModel : ViewModelBase
    {
        public DevicesOutputViewModel()
        {
            Core.RemoveLater.Output.MessageSent += Output_MessageSent;
            
        }

        private void Output_MessageSent(object? sender, Core.RemoveLater.OutputMessageEventArgs e)
        {
            Output.Add($"Device: {e.DeviceId}, Input: {e.InputId}, Action: {e.ActionName}");
        }

        internal void ActionThrownAnException(PluginActionExceptionEventArgs e)
        {
            Output.Add("An action thrown an exception!\n" +
            $"Device:    {e.DeviceNumber}, Input: {e.InputId}\n" +
            $"Action:    {e.ActionName}\n" +
            $"Exception: {e.Exception.Message}\n");
        }

        private AvaloniaList<string> _output = new();
        public AvaloniaList<string> Output
        {
            get => _output;
            set => this.RaiseAndSetIfChanged(ref _output, value);
        }

    }
}
