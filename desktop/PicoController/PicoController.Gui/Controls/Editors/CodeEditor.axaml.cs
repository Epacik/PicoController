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
    public partial class CodeEditor : UserControl, IEditor, IDisposable
    {
        private readonly RegistryOptions _registryOptions;
        private readonly Installation _textMateInstallation;

        public CodeEditor(string text, string languageExt)
        {
            InitializeComponent();

            (var python, var scopeName, _registryOptions, _textMateInstallation) =
                EditorHelpers.InitializeEditor(Editor, languageExt, text, this.Background);

            Editor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            Editor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            Editor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

            this.AddHandler(PointerWheelChangedEvent, (o, i) =>
            {
                if (i.KeyModifiers != KeyModifiers.Control) return;
                if (i.Delta.Y > 0) Editor.FontSize++;
                else Editor.FontSize = Editor.FontSize > 1 ? Editor.FontSize - 1 : 1;
            }, RoutingStrategies.Bubble, true);

            //_textEditor.TextArea.IndentationStrategy = new Indentation.CSharp.CSharpIndentationStrategy(_textEditor.Options);
        }

        private void Caret_PositionChanged(object? sender, EventArgs e)
        {
            var caret = Editor.TextArea.Caret;
            LineNumber.Text = caret.Line.ToString();
            ColumnNumber.Text = caret.Column.ToString();
        }

        private void textEditor_TextArea_TextEntering(object? sender, TextInputEventArgs e)
        {
        }

        private void textEditor_TextArea_TextEntered(object? sender, TextInputEventArgs e)
        {
        }

        public string GetValue()
        {
            return Editor.Document.Text;
        }

        public void Dispose()
        {
            Editor.TextArea.TextEntered -= textEditor_TextArea_TextEntered;
            Editor.TextArea.TextEntering -= textEditor_TextArea_TextEntering;
            Editor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        }
    }
}
