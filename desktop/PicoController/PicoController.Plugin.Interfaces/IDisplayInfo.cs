using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PicoController.Plugin.DisplayInfos;
namespace PicoController.Plugin;

public interface IDisplayInfo
{
    void Display(params DisplayInformations[] infos);
    void Display(IEnumerable<DisplayInformations> infos);
}