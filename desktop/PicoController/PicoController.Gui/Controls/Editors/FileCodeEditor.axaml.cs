using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using PicoController.Plugin;
using TextMateSharp.Grammars;
using static AvaloniaEdit.TextMate.TextMate;

namespace PicoController.Gui.Controls.Editors
{
    public partial class FileCodeEditor : UserControl, IEditor, IDisposable
    {
        private readonly RegistryOptions _registryOptions;
        private readonly Installation _textMateInstallation;
        private readonly string _path;
        private readonly string _languageExt;
        private bool _saved = false;

        public FileCodeEditor(string path, string languageExt)
        {
            InitializeComponent();

            FileName.Text = path;

            var text = "";

            if (File.Exists(path))
            {
                text = File.ReadAllText(path);
            }

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

            BrowseButton.Click += BrowseButton_Click;
            ReloadButton.Click += ReloadButton_Click;
            SaveFileButton.Click += SaveFileButton_Click;
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            _path = path;
            _languageExt = languageExt;
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

        private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
        {
            var top = TopLevel.GetTopLevel(this);
            if (top is null)
                return;
            var sp = top.StorageProvider;
            var result = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Text File",
                AllowMultiple = false,
                FileTypeFilter = new FilePickerFileType[]
                {
                   new("Source file")
                   {
                       Patterns = new[] { $"*{_languageExt}" },
                   }
                }
            });

            if(result?.Count >= 1)
            {
                var file = result[0];
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);

                Editor.Document = new TextDocument(reader.ReadToEnd());
                FileName.Text = file.TryGetLocalPath();
            }
        }

        private void ReloadButton_Click(object? sender, RoutedEventArgs e)
        {
            var path = FileName.Text;
            if (!File.Exists(path))
                return;

            Editor.Document = new TextDocument( File.ReadAllText(path));
        }

        private void SaveFileButton_Click(object? sender, RoutedEventArgs e)
        {
            var path = FileName.Text;
            if (!File.Exists(path))
                return;

            File.WriteAllText(path, Editor.Document.Text);
        }
        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            EditorHelpers.CloseWindow(this);
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            _saved = true;
            EditorHelpers.CloseWindow(this);
        }

        public string GetValue()
        {
            return _saved ? FileName.Text ?? "" : _path;
        }
        public void Dispose()
        {
            Editor.TextArea.TextEntered -= textEditor_TextArea_TextEntered;
            Editor.TextArea.TextEntering -= textEditor_TextArea_TextEntering;
            Editor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
            BrowseButton.Click -= BrowseButton_Click;
            SaveFileButton.Click += SaveFileButton_Click;
            ReloadButton.Click -= ReloadButton_Click;
            CancelButton.Click -= CancelButton_Click;
            SaveButton.Click -= SaveButton_Click;
        }
    }
}
