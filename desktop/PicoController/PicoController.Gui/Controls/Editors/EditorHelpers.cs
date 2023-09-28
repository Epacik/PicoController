using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextMateSharp.Grammars;

namespace PicoController.Gui.Controls.Editors;

internal static class EditorHelpers
{
    internal static void CloseWindow(Control control)
    {
        var top = TopLevel.GetTopLevel(control);
        if (top is not Window window)
            return;

        window.Close();
    }

    internal static InitializeResult InitializeEditor(
        TextEditor editor,
        string languageExt,
        string text,
        IBrush? background)
    {
        editor.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
        editor.Background = Brushes.Transparent;
        editor.ShowLineNumbers = true;
        editor.ContextMenu = new ContextMenu
        {
            ItemsSource = new List<MenuItem>
                {
                    new MenuItem { Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) },
                    new MenuItem { Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) },
                    new MenuItem { Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) }
                }
        };
        editor.TextArea.Background = background;
        editor.TextArea.RightClickMovesCaret = true;
        
        editor.Options.ShowBoxForControlCharacters = true;
        editor.Options.ColumnRulerPositions = new List<int>() { 80, 100 };
        editor.Options.ShowEndOfLine = true;
        editor.Options.ShowSpaces = true;
        editor.Options.ShowTabs = true;
        editor.Options.RequireControlModifierForHyperlinkClick = true;
        editor.Options.ConvertTabsToSpaces = true;
        editor.Options.EnableHyperlinks = true;
        editor.Options.EnableRectangularSelection = true;
        //editor.Options.EnableVirtualSpace = true;

        var _registryOptions = new RegistryOptions(
                ThemeName.LightPlus);

        var _textMateInstallation = editor.InstallTextMate(_registryOptions);

        Language lang = _registryOptions.GetLanguageByExtension(languageExt);

        string scopeName = _registryOptions.GetScopeByLanguageId(lang.Id);

        editor.Document = new TextDocument(text ?? "");
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(lang.Id));

        return new InitializeResult(lang, scopeName, _registryOptions, _textMateInstallation);
    }
}

internal record struct InitializeResult(Language lang, string scopeName, RegistryOptions _registryOptions, TextMate.Installation _textMateInstallation)
{
    public static implicit operator (Language lang, string scopeName, RegistryOptions _registryOptions, TextMate.Installation _textMateInstallation)(InitializeResult value)
    {
        return (value.lang, value.scopeName, value._registryOptions, value._textMateInstallation);
    }

    public static implicit operator InitializeResult((Language lang, string scopeName, RegistryOptions _registryOptions, TextMate.Installation _textMateInstallation) value)
    {
        return new InitializeResult(value.lang, value.scopeName, value._registryOptions, value._textMateInstallation);
    }
}