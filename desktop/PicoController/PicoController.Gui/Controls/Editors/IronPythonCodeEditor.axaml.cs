using Avalonia.Controls;
using static AvaloniaEdit.TextMate.TextMate;
using TextMateSharp.Grammars;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Interactivity;
using System.Resources;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Document;
using PicoController.Plugin;

namespace PicoController.Gui.Controls.Editors
{
    public partial class IronPythonCodeEditor : UserControl, IEditor
    {
        private readonly RegistryOptions _registryOptions;
        private readonly Installation _textMateInstallation;

        public IronPythonCodeEditor(string text)
        {
            InitializeComponent();
            //_registryOptions = new RegistryOptions(ThemeName.LightPlus);
            //_installation = Editor.InstallTextMate(_registryOptions);

            //var python = _registryOptions.GetLanguageByExtension(".py");
            //_installation.SetGrammar(_registryOptions.GetScopeByLanguageId(python.Id));

            //Editor.Text = text;
            var _textEditor = Editor;
            _textEditor.HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible;
            _textEditor.Background = Brushes.Transparent;
            _textEditor.ShowLineNumbers = true;
            _textEditor.ContextMenu = new ContextMenu
            {
                ItemsSource = new List<MenuItem>
                {
                    new MenuItem { Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) },
                    new MenuItem { Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) },
                    new MenuItem { Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) }
                }
            };
            _textEditor.TextArea.Background = this.Background;
            _textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            _textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            _textEditor.Options.ShowBoxForControlCharacters = true;
            _textEditor.Options.ColumnRulerPositions = new List<int>() { 80, 100 };
            //_textEditor.TextArea.IndentationStrategy = new Indentation.CSharp.CSharpIndentationStrategy(_textEditor.Options);
            _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            _textEditor.TextArea.RightClickMovesCaret = true;


            _registryOptions = new RegistryOptions(
                ThemeName.LightPlus);

            _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);

            Language python = _registryOptions.GetLanguageByExtension(".py");

            string scopeName = _registryOptions.GetScopeByLanguageId(python.Id);

            _textEditor.Document = new TextDocument(text);
            _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(python.Id));

            this.AddHandler(PointerWheelChangedEvent, (o, i) =>
            {
                if (i.KeyModifiers != KeyModifiers.Control) return;
                if (i.Delta.Y > 0) _textEditor.FontSize++;
                else _textEditor.FontSize = _textEditor.FontSize > 1 ? _textEditor.FontSize - 1 : 1;
            }, RoutingStrategies.Bubble, true);

        }

        private void Caret_PositionChanged(object? sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void textEditor_TextArea_TextEntering(object? sender, TextInputEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void textEditor_TextArea_TextEntered(object? sender, TextInputEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public string GetValue()
        {
            return Editor.Document.Text;
        }
    }
}
