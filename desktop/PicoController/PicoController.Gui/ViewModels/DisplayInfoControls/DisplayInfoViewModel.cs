using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.ViewModels.DisplayInfoControls;

public class DisplayInfoViewModel : ViewModelBase
{
    public DisplayInfoViewModel(object item)
    {
        Item = item;
    }

    public object Item { get; }
}
