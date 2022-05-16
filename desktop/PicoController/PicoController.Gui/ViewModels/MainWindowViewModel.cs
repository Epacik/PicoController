
using Avalonia.Collections;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace SerialControler.Gui.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        

        public string Greeting 
        { 
            get => greeting;
            set => this.RaiseAndSetIfChanged(ref greeting, value);
        }

        private string greeting = "";

        public AvaloniaList<Encoder> Encoders 
        {
            get => encoders;
            set => this.RaiseAndSetIfChanged(ref encoders, value);
        }
        private AvaloniaList<Encoder> encoders = new();
    }
}
