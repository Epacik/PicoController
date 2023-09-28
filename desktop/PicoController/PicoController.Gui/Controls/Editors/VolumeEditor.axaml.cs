using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PicoController.Plugin;
using System.Diagnostics;
using System.Management;
using System.Web.Services.Description;

namespace PicoController.Gui.Controls.Editors;

public partial class VolumeEditor : UserControl, IEditor, IDisposable
{
    private bool _saved;
    private readonly string _data;

    public VolumeEditor(string data)
    {
        InitializeComponent();

        if (data is null)
            return;

        CancelButton.Click += CancelButton_Click;
        SaveButton.Click += SaveButton_Click;
        ProgramSwitch.Click += ProgramSwitch_Click;

        var programs = new List<string>();
        programs.AddRange(Process.GetProcesses().Select(x => x.ProcessName));
        if (OperatingSystem.IsWindows())
        {
            ManagementObjectSearcher searcher = new("SELECT PROCESSID, NAME, DISPLAYNAME FROM WIN32_SERVICE");
            foreach (ManagementObject mngntObj in searcher.Get().Cast<ManagementObject>())
            {
                programs.Add((string)mngntObj["NAME"]);
            }
        }

        programs = programs
            .Distinct()
            .ToList();

        programs.Sort();

        Programs.ItemsSource = programs;

        _data = data;

        var args = data.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (args.Length == 0)
        {
            Increment.Value = 0;
            ProgramSwitch.IsEnabled = false;
            ProgramSwitch.IsChecked = false;
        }
        else if (args.Length == 1 && int.TryParse(args[0], out int val1))
        {
            Increment.Value = val1;
            ProgramSwitch.IsEnabled = false;
            ProgramSwitch.IsChecked = false;
        }
        else if (args.Length >= 2 && int.TryParse(args[1], out int val2))
        {
            Increment.Value = val2;
            ProgramSwitch.IsEnabled = true;
            ProgramSwitch.IsChecked = true;
            bool exact = args.Contains("!") || args.Contains("EXACT");
            ExactSwitch.IsChecked = exact;

            if (exact)
            {
                Programs.SelectedItem = programs.FirstOrDefault(x => x == args[0]);
            }
            else
            {
                Programs.SelectedItem = programs.FirstOrDefault(
                    x => x.Contains(args[0], StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }

    private void ProgramSwitch_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is ToggleSwitch ts)
        {
            Programs.IsEnabled = ts.IsChecked == true;
            ExactSwitch.IsEnabled = ts.IsChecked == true;
        }
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
        if (!_saved)
            return _data;

        if (ProgramSwitch.IsChecked == true && ExactSwitch.IsChecked == true)
            return $"{Programs.SelectedItem};{(int?)Increment.Value};!";
        else if (ProgramSwitch.IsChecked == true)
            return $"{Programs.SelectedItem};{(int?)Increment.Value}";
        else
            return $"{Increment.Value}";
    }

    public void Dispose()
    {
        CancelButton.Click -= CancelButton_Click;
        SaveButton.Click -= SaveButton_Click;
        ProgramSwitch.Click -= ProgramSwitch_Click;
    }
}