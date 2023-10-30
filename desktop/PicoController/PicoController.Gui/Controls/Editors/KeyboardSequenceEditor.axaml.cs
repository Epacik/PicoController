using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using PicoController.Core.BuiltInActions.HidSimulation;
using PicoController.Plugin;
using SharpHook;
using SharpHook.Native;
using System.Text;
using System.Text.Json;

namespace PicoController.Gui.Controls.Editors
{
    public partial class KeyboardSequenceEditor : UserControl, IEditor, IDisposable
    {
        private readonly string _data;
        private readonly TaskPoolGlobalHook _hook;
        private bool _saved;
        private bool _capturing;
        private List<KeyPressData> _keys = new();

        public KeyboardSequenceEditor(string data)
        {
            InitializeComponent();
            _data = data ?? "";

            if (!string.IsNullOrWhiteSpace(_data))
            {
                try
                {
                    var keys = JsonSerializer.Deserialize<List<KeyPressData>>(_data);
                    if (keys is not null)
                    {
                        foreach (var (code, pressed) in keys)
                        {
                            AddKey(code, pressed);
                        }
                    }
                }
                catch {}
            }

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

            SaveButton.IsTabStop = !_capturing;
            CancelButton.IsTabStop = !_capturing;
            CaptureButton.IsTabStop = !_capturing;


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
            var lastDown = _keys.LastIndexOf(new (e.Data.KeyCode, true));
            var lastUp = _keys.LastIndexOf(new (e.Data.KeyCode, false));

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

        private void AddKey(KeyCode keyCode, bool? pressed, int? index = null)
        {
            var data = new KeyPressData(keyCode, pressed);
            if (index is int i)
            {
                _keys.Insert(i, data);
            }
            else
            {
                _keys.Add(data);
            }
            
            Dispatcher.UIThread.Invoke(() =>
            {
                var key = new Border()
                {
                    BorderThickness = new(1),
                    BorderBrush = Brushes.Black,
                    Background = Brushes.White,
                    Padding = new(5),
                    CornerRadius = new(5),
                    IsTabStop = false,
                    Width = 396,
                    Tag = data,
                };

                var removeButt = new Button()
                {
                    Content = "×",
                    Width = 20,
                    Height = 20,
                    Padding = new(2),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    IsTabStop = false,
                };

                if (pressed is bool prsd)
                {
                    var keyName = keyCode.ToString().Replace("Vc", "");

                    var text = new TextBlock()
                    {
                        Text = $"{(prsd ? "🔼" : "🔽")} {keyName}",
                        IsTabStop = false,
                    };

                    Grid.SetColumn(text, 0);

                    Grid.SetColumn(removeButt, 2);

                    
                    key.Child = new Grid()
                    {
                        ColumnDefinitions =
                            {
                                new ColumnDefinition(),
                                new ColumnDefinition(20, GridUnitType.Pixel),
                            },
                        Children =
                            {
                                text,
                                removeButt,
                            },
                        IsTabStop = false,
                    };

                    removeButt.Click += RemoveButt_Click;
                    key.PointerPressed += Key_PointerPressed1;

                    if (index is int i)
                    {
                        KeysList.Items.Insert(i, key);
                    }
                    else
                    {
                        KeysList.Items.Add(key);
                    }
                    KeysList.SelectedItem = key;

                    var container = KeysList.ContainerFromItem(key);

                    if (container is ListBoxItem lbi)
                    {
                        lbi.Margin = new(0);
                        lbi.Padding = new(0);
                        lbi.MinWidth = 0;
                        lbi.MinHeight = 0;
                        lbi.IsTabStop = false;
                    }
                }
                else
                {
                    var numUpDown = new NumericUpDown()
                    {
                        Minimum = 10,
                        Maximum = ushort.MaxValue,
                        Value = (ushort)keyCode,
                        Increment = 1,
                        FormatString = "Wait for: {0:0} ms",
                        Tag = data,
                        Width = 300,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        IsTabStop = false,
                    };
                    Grid.SetColumn(numUpDown, 0);
                    numUpDown.ValueChanged += NumUpDown_ValueChanged;

                    
                    Grid.SetColumn(removeButt, 2);
                    removeButt.Click += RemoveButt_ClickWait;

                    key.Padding = new(0, 0, 5, 0);
                    key.Child = new Grid()
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition(),
                            new ColumnDefinition(20, GridUnitType.Pixel),
                        },
                        Children =
                        {
                            numUpDown,
                            removeButt,
                        },
                    };

                    key.PointerPressed += Key_PointerPressed1;

                    if (index is int i)
                    {
                        KeysList.Items.Insert(i, key);
                    }
                    else
                    {
                        KeysList.Items.Add(key);
                    }
                    KeysList.SelectedItem = key;

                    var container = KeysList.ContainerFromItem(key);

                    if (container is ListBoxItem lbi)
                    {
                        lbi.Margin = new(0);
                        lbi.Padding = new(0);
                        lbi.MinWidth = 0;
                        lbi.MinHeight = 0;
                        lbi.IsTabStop = false;
                    }
                }
                
                //container.
            });
        }

        private void Key_PointerPressed1(object? sender, PointerPressedEventArgs e)
        {
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                return;

            var offset = e.KeyModifiers.HasFlag(KeyModifiers.Control) ? 1 : 0;

            var index = KeysList.Items.IndexOf(sender) + offset;

            AddKey((KeyCode)100, null, index);
        }

        private void NumUpDown_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
        {
            if (
                sender is NumericUpDown ctrl &&
                ctrl.Tag is KeyPressData kpd &&
                ctrl.Parent is Grid grid &&
                grid.Parent is Border border)
            {
                var value = (ushort)(ctrl.Value ?? 0);
                var index = _keys.IndexOf(kpd);
                var data = new KeyPressData((KeyCode)value, null);
                _keys[index] = data;
                ctrl.Tag = data;
                border.Tag = data;
            }
        }

        private void RemoveButt_ClickWait(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var button = sender as Button;
            var grid = button?.Parent as Grid;
            var border = grid?.Parent as Border;
            var numeric = grid?.Children?[0] as NumericUpDown;
            var item = border?.Tag as KeyPressData;

            if (button is null || grid is null || border is null || numeric is null || item is null)
                return;

            numeric.ValueChanged -= NumUpDown_ValueChanged;
            button.Click -= RemoveButt_ClickWait;
            border.PointerPressed -= Key_PointerPressed1;

            _keys.Remove(item);
            KeysList.Items.Remove(border);
            KeysList.SelectedItem = null;
        }

        private void RemoveButt_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var button = sender as Button;
            var grid = button?.Parent as Grid;
            var border = grid?.Parent as Border;
            var item = border?.Tag as KeyPressData;

            if (button is null || grid is null || border is null || item is null)
                return;

            button.Click -= RemoveButt_Click;
            border.PointerPressed -= Key_PointerPressed1;

            _keys.Remove(item);
            KeysList.Items.Remove(border);
            KeysList.SelectedItem = null;
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
            if (!_saved)
                return _data;

            var data = JsonSerializer.Serialize(_keys);

            return data;
        }
    }
}
