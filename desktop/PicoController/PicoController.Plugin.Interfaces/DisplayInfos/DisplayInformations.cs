using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneOf;

namespace PicoController.Plugin.DisplayInfos;

[GenerateOneOf]
public partial class DisplayInformations : OneOfBase<Text, ProgressBar>
{
}
