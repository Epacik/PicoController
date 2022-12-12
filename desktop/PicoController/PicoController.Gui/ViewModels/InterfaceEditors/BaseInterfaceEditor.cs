using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.InterfaceEditors;

public abstract class InterfaceEditorViewModel : ViewModelBase
{
    public abstract Dictionary<string, JsonElement> GetInterfaceSettings();
}
