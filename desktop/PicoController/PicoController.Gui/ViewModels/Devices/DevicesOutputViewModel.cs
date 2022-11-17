using Avalonia.Collections;
using PicoController.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.Devices;

public class DevicesOutputViewModel : ViewModelBase
{
    public DevicesOutputViewModel()
    {
        Core.RemoveLater.Output.MessageSent += Output_MessageSent;
    }

    private void Output_MessageSent(object? sender, Core.RemoveLater.OutputMessageEventArgs e)
    {
        AddToOutput($"Device: {e.DeviceId}, Input: {e.InputId}, Action: {e.ActionName}");
    }

    internal void ActionThrownAnException(PluginActionExceptionEventArgs e)
    {
        AddToOutput("An action thrown an exception!\n" +
        $"Device:    {e.DeviceNumber}, Input: {e.InputId}\n" +
        $"Action:    {e.ActionName}\n" +
        $"Exception: {e.Exception.Message}\n");
    }

    internal void OtherException(Exception ex)
    {
        AddToOutput($"Exception was thrown: \n{ex}");
    }

    object locker = new();

    private void AddToOutput(string text)
    {
        lock(locker)
        {
            Output.Add(new(text));
            LastItem = Output.LastOrDefault();
            this.RaisePropertyChanged(nameof(Output));
        }
    }

    private AvaloniaList<OutputItem> _output = new();
    public AvaloniaList<OutputItem> Output
    {
        get => _output;
        set => this.RaiseAndSetIfChanged(ref _output, value);
    }

    private OutputItem? _lastItem;
    public OutputItem? LastItem
    {
        get => _lastItem;
        set => this.RaiseAndSetIfChanged(ref _lastItem, value);
    }

    public class OutputItem
    {
        public OutputItem(string text)
        {
            Text = text;
        }

        public string Text { get; }

        public override string ToString() => Text;
    }
}
