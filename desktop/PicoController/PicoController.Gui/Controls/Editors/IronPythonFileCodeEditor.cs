using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Controls.Editors;

internal class IronPythonFileCodeEditor : FileCodeEditor
{
    public IronPythonFileCodeEditor(string path) : base(path, ".py")
    {
    }
}
