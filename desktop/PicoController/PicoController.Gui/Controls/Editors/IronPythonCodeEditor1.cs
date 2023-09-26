using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Gui.Controls.Editors;

public class IronPythonCodeEditor1 : CodeEditor
{
    public IronPythonCodeEditor1(string text)
    {
        var python = _registryOptions.GetLanguageByExtension(".py");
        _installation.SetGrammar(_registryOptions.GetScopeByLanguageId(python.Id));

        Text = text;
    }
}
