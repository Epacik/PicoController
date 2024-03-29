using Avalonia.Controls;
using Avalonia.Controls.Templates;
using PicoController.Gui.ViewModels;
using System;

namespace PicoController.Gui;

public class ViewLocator : IDataTemplate
{
    Control? ITemplate<object?, Control?>.Build(object? data)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type is not null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }
        else
        {
            return new TextBlock { Text = "Not Found: " + name };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    
}
