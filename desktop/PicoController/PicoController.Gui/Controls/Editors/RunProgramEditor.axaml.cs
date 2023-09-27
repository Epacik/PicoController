using Avalonia.Platform.Storage;
using PicoController.Plugin;

namespace PicoController.Gui.Controls.Editors;

public partial class RunProgramEditor : UserControl, IEditor, IDisposable
{
    private readonly string _data;
    private bool _saved = false;

    public RunProgramEditor(string data)
    {
        InitializeComponent();
        _data = data;
        var split = data?.Split(';');

        ProgramName.Text = split?.Length > 0 ? split[0] : "";
        Arguments.Text = split?.Length > 1 ? split[1] : "";

        BrowseButton.Click += BrowseButton_Click;
        CancelButton.Click += CancelButton_Click;
        SaveButton.Click += SaveButton_Click;
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

    private async void BrowseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
                   new("Executable")
                   {
                       Patterns = new[] { "*.exe" },
                   }
            }
        });

        if (result?.Count >= 1)
        {
            var file = result[0];
            ProgramName.Text = file.TryGetLocalPath();
        }
    }

    public string GetValue()
    {
        return _saved ? $"{ProgramName.Text};{Arguments.Text}" : _data;
    }

    public void Dispose()
    {
        BrowseButton.Click -= BrowseButton_Click;
        CancelButton.Click -= CancelButton_Click;
        SaveButton.Click -= SaveButton_Click;
    }
}