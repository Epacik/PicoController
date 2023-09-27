using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Plugin.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class FileCodeEditorAttribute : Attribute
{
    public FileCodeEditorAttribute(string languageExtension)
    {
        LanguageExtension = languageExtension;
    }

    public string LanguageExtension { get; }
}
