using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using PicoController.Plugin;
using SharpHook;
using SharpHook.Native;
using System.Text;

namespace PicoController.Gui.Controls.Editors
{
    public partial class KeyboardSequenceEditor : UserControl, IEditor, IDisposable
    {
        private readonly string _data;
        private readonly TaskPoolGlobalHook _hook;
        private bool _saved;
        private bool _capturing;
        private List<(KeyCode code, bool pressed)> _keys = new();

        public KeyboardSequenceEditor(string data)
        {
            InitializeComponent();
            _data = data;

            if (data is null)
                return;

            var values = Core.BuiltInActions.HidSimulation.KeyboardSequence.GetValidValues();

            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            CaptureButton.Click += CaptureButton_Click;


            _hook = new TaskPoolGlobalHook();
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            EditorHelpers.CloseWindow(this);
        }

        private void SaveButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _saved = true;
            EditorHelpers.CloseWindow(this);
        }

        private void CaptureButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _capturing = CaptureButton.IsChecked == true;
            this.Focus();

            if (_capturing)
            {
                ClearKeyList();
                _keys.Clear();
                _hook.KeyPressed += _hook_KeyPressed;
                _hook.KeyReleased += _hook_KeyReleased;

                if (!_hook.IsRunning)
                    _hook.Run();
            }
            else
            {
                _hook.KeyPressed -= _hook_KeyPressed;
                _hook.KeyReleased -= _hook_KeyReleased;
            }

        }

        private void _hook_KeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            AddKey(e.Data.KeyCode, false);
        }

        private void _hook_KeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            var lastDown = _keys.LastIndexOf((e.Data.KeyCode, true));
            var lastUp = _keys.LastIndexOf((e.Data.KeyCode, false));

            if(lastUp >= lastDown)
                AddKey(e.Data.KeyCode, true);
        }

        private void ClearKeyList()
        {
            foreach(Control item in KeysList.Items)
            {
                item.PointerPressed -= Key_PointerPressed;
            }
        }

        private void AddKey(KeyCode keyCode, bool pressed)
        {
            _keys.Add((keyCode, pressed));
            Dispatcher.UIThread.Invoke(() =>
            {
                var keyName = keyCode.ToString().Replace("Vc", "");
                var key = new Border()
                {
                    BorderThickness = new(1),
                    BorderBrush = Brushes.Black,
                    Background = Brushes.White,
                    Padding = new(5),
                    CornerRadius = new(5),
                    Child = new TextBlock()
                    {
                        Text = $"{keyName} {(pressed ? "🔼" : "🔽")}",
                    },
                };

                key.PointerPressed += Key_PointerPressed;

                KeysList.Items.Add(key);
                KeysList.SelectedItem = key;

                var container = KeysList.ContainerFromItem(key);

                if(container is ListBoxItem lbi)
                {
                    lbi.Margin = new(0);
                    lbi.Padding = new(0);
                    lbi.MinWidth = 0;
                    lbi.MinHeight = 0;
                }
                //container.
            });
        }

        private void Key_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if(sender is Control ctrl)
                ctrl.PointerPressed -= Key_PointerPressed;

            KeysList.Items.Remove(sender);
        }

        public void Dispose()
        {
            SaveButton.Click -= SaveButton_Click;
            CancelButton.Click -= CancelButton_Click;
            CaptureButton.Click -= CaptureButton_Click;
            ClearKeyList();
            if (_hook is not null)
            {
                _hook.KeyPressed -= _hook_KeyPressed;
                _hook.KeyReleased -= _hook_KeyReleased;

                _hook?.Dispose();
            }
        }

        public string GetValue()
        {
            if (_saved)
                return _data;

            var builder = new StringBuilder();
            var pressed = new List<KeyCode>();

            foreach(var key in _keys)
            {

            }

            return _data;
        }
    }
}
