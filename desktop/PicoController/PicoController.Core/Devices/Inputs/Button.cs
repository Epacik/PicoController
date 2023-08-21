using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiger.Clock;

namespace PicoController.Core.Devices.Inputs;

internal class Button : Input
{
    private readonly int _maxDelayBetweenClicks;
    //private readonly System.Timers.Timer _timer;

    public Button(
        int deviceId,
        byte inputId,
        Dictionary<string, Func<int, Task>?> actions,
        int maxDelayBetweenClicks,
        ILogger logger)
        : base(
            deviceId,
            inputId,
            InputType.Button,
            new string[] { ActionNames.Press, ActionNames.DoublePress, ActionNames.TriplePress },
            actions,
            logger)
    {
        _maxDelayBetweenClicks = maxDelayBetweenClicks;
    }


    private bool _isPressed;
    private int _presses;
    private CancellationTokenSource? _tokenSource;

    protected override async Task ExecuteInternal(InputMessage message)
    {
        const int Pressed = 1, Released = 1 << 1;

        await Task.Yield();

        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = null;

        if (_isPressed && message.Value == Released)
        {
            _isPressed = false;
            Interlocked.Increment(ref _presses);

            try
            {
                _tokenSource = new CancellationTokenSource();
                await Task.Delay(_maxDelayBetweenClicks, _tokenSource.Token);
                await InvokeClicks(Interlocked.Exchange(ref _presses, 0));
            }
            catch (TaskCanceledException)
            {
                // swallow
            }

        }
        else if(!_isPressed && message.Value == Pressed)
        {
            _isPressed = true;
        }
    }

    async Task InvokeClicks(int presses)
    {
        switch (presses)
        {
            case 1:
                await InvokeAction(0, ActionNames.Press); break;
            case 2:
                await InvokeAction(0, ActionNames.DoublePress); break;
            case 3:
                await InvokeAction(0, ActionNames.TriplePress); break;
            case > 3:
                await InvokeAction(0, ActionNames.TriplePress);
                await InvokeClicks(presses - 3);
                break;
        }
    }
}
