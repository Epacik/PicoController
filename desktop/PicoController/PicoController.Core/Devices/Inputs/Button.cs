using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiger.Clock;

namespace PicoController.Core.Devices.Inputs;

internal class Button : InputBase
{
    private readonly int _maxDelayBetweenClicks;
    private readonly System.Timers.Timer _timer;

    public Button(
        int deviceId,
        byte inputId,
        Dictionary<string, Func<int, Task>?> actions,
        int maxDelayBetweenClicks) 
        : base(
            deviceId,
            inputId,
            InputType.Button,
            new string[] { ActionNames.Press, ActionNames.DoublePress, ActionNames.TriplePress },
            actions)
    {
        _maxDelayBetweenClicks = maxDelayBetweenClicks;
        _timer = new System.Timers.Timer(_maxDelayBetweenClicks);
        _timer.Elapsed += _timer_Elapsed;
        _timer.AutoReset = false;
    }

    private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        void invoke(int presses)
        {
            switch (presses)
            {
                case 1:
                    InvokeAction(0, ActionNames.Press); break;
                case 2:
                    InvokeAction(0, ActionNames.DoublePress); break;
                case 3:
                    InvokeAction(0, ActionNames.TriplePress); break;
                case > 3:
                    InvokeAction(0, ActionNames.TriplePress);
                    invoke(presses - 3);
                    break;
            }
        }
        
        invoke(Interlocked.Exchange(ref _presses, 0));
    }

    private bool IsPressed;
    private int _presses;

    protected override void ExecuteInternal(InputMessage message)
    {
        const int Pressed = 1, Released = 1 << 1;
        if (IsPressed && message.Value == Released)
        {
            IsPressed = false;
            Interlocked.Increment(ref _presses);

            _timer.Stop();
            _timer.Start();
        }
        else if(!IsPressed && message.Value == Pressed)
        {
            IsPressed = true;
        }
    }
}
