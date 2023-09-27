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
        private readonly string _text;
        private bool _saved;

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

            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            _text = text;
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

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _saved = true;
            EditorHelpers.CloseWindow(this);
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            EditorHelpers.CloseWindow(this);
        }

        public string GetValue()
        {
            return _saved ? Editor.Document.Text : _text;
        }

        public void Dispose()
        {
            Editor.TextArea.TextEntered -= textEditor_TextArea_TextEntered;
            Editor.TextArea.TextEntering -= textEditor_TextArea_TextEntering;
            Editor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        }
    }
}
