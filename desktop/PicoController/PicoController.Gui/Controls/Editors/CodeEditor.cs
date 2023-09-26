using Avalonia.Media;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace PicoController.Gui.Controls.Editors;

public abstract class CodeEditor : AvaloniaEdit.TextEditor
{
    protected readonly RegistryOptions _registryOptions;
    protected readonly TextMate.Installation _installation;

    protected CodeEditor()
    {
        FontFamily = FontFamily.Parse("Cascadia Code,Consolas,Menlo,Monospace");
        ShowLineNumbers = true;
        _registryOptions = new RegistryOptions(ThemeName.LightPlus);
        _installation = this.InstallTextMate(_registryOptions);
    }
}
