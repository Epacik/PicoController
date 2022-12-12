using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Plugin;

public interface IDisplayInfo
{
    void Display(IEnumerable<OneOf<DisplayInfos.Text, DisplayInfos.ProgressBar>> infos);
}